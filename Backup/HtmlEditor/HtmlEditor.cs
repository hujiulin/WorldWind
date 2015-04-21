using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;

namespace onlyconnect
{

    /// <summary>
    /// Modified for Google Summer of Code by Ashish Datta
    /// The DLL now impliments a HTML browser for NASA World Wind
    /// 
    /// Implements MSHTML as active document in a control
    /// With thanks to Lutz Roeder
    /// 
    /// also thanks to Steven Wood for the HTML Event handling idea
    /// and code.
    /// 
    /// and thanks to Christopher Slee for the region marking
    /// 
    /// and thanks to James Hancock for ideas/code re. making this a better-behaved winform 
    /// control.
    /// 
    /// This version does not require the Microsoft.mshtml Primary Interop Assembly.
    /// 
    /// Comments, improvements, suggestions - 
    /// email tim@itwriting.com
    /// or visit the messageboard at http://www.itwriting.com/htmleditor.php
    /// </summary>
    /// 
    /// Basic Reference:
    /// Activating the MSHTML Editor  Internet Development Index 
    /// --------------------------------------------------------------------------------
    /// This topic describes how to activate the MSHTML Editor from 
    /// Microsoft Visual C++®, Microsoft Visual Basic®, Microsoft JScript®, and Visual Basic Scripting Edition (VBScript). 
    /// The Editor provides a rich set of capabilities and serves as a fine 
    /// "what you see is what you get" (WYSIWYG) HTML editing environment. 
    /// In addition, the Editor includes the ability to customize its behavior. 
    /// For more information, see Related Topics at the end of this topic.

    [System.ComponentModel.ToolboxItemAttribute(true)]
    public class HtmlEditor : Control
    {
        #region class variables and constructor

        private bool bLoadDocumentWhenReady = false;
        private bool bLoadUrlWhenReady = false;
        private bool bSetComposeSettingsWhenReady = false;
        private int iLoadAttempts = 0;
        private EncodingType mDocumentEncoding = EncodingType.WindowsCurrent;
        private EncodingType mDefaultPreamble = EncodingType.UTF8;
        private bool mIsWin98 = false;
        private bool mAlwaysLoadAnsi = false;
        internal bool mLinksNewWindow = false;
        internal bool bNeedsActivation = false;
        internal bool mAcceptsTab = false;
        internal bool mAcceptsReturn = true;
        internal DocHTML mDocHTML = null;

        internal HtmlSite site;
        String url = String.Empty;
        String sDocument = String.Empty;
        internal bool mDesignMode = false;
        private bool mIsContextMenuEnabled = true;
        private string mOptionKeyPath = null;
        internal bool mAllowActivation = true;
        internal HTMLDocument mHtmlDoc = null;
        internal IHTMLElement mcurrentElement = null;
        private IntPtr mDocumentHandle = IntPtr.Zero;
        private bool mCreating = true;

        private NativeWindow mNativeWindow = null;
        private HandleWndProc mNativeDocWindow = null;
        private System.Windows.Forms.Timer activateTimer;
        private System.Windows.Forms.Timer createTimer;
        private System.ComponentModel.IContainer components;
        private ComposeSettings mComposeSettings = null;

        internal bool mEnableActiveContent = true;
        internal bool mEnableAutoComplete = false;
        internal bool mDivOnEnter = false;
        internal bool mShowScrollBars = true;
        internal bool mEnableUrlDetection = false;
        internal uint changeCookie;

        private ChangeMonitor mChangeMonitor;

        public HtmlEditor()
            : base()
        {
            //initialize components
            InitializeComponent();

            //Detect Windows version
            mIsWin98 = (System.Environment.OSVersion.Platform == PlatformID.Win32Windows);

            //force creation of handle, needed to host mshtml
            this.CreateControl();

            //see OnHandleCreated for purpose of mCreating
            mCreating = false;

        }


        #endregion class variables and constructor

        #region Keyboard and mouse handling

        internal void InvokeOnMouseDown(MouseEventArgs e)
        {
            this.OnMouseDown(e);
        }

        internal void InvokeOnDoubleClick()
        {
            this.OnDoubleClick(EventArgs.Empty);
        }

        /// <summary>
        /// Executes The short cut keys that should be available and handles all of the cases of design mode versus not.
        /// </summary>
        /// <param name="Key">The key to process.</param>
        /// 
        private bool doShortCut(Keys Key)
        {
            //fire the BeforeShortcut event and cancel if necessary
            BeforeShortcutEventArgs e = new BeforeShortcutEventArgs(Key);
            this.OnBeforeShortcut(e);

            //if cancelled, return True for Handled
            return (e.Cancel);

        }

        #endregion

        #region Override WndProc ===============

        internal void InvokeWndProc(ref Message msg)
        {
            Message message = Message.Create(this.Handle, msg.Msg, msg.WParam, msg.LParam);
            this.WndProc(ref message);
        }

        internal void setupWndProc()
        {

            if (this.mNativeDocWindow != null)
            {
                this.mNativeDocWindow.ReleaseHandle();
            }

            this.mNativeDocWindow = new HandleWndProc();
            this.mNativeDocWindow.thecontrol = this;
            this.mNativeDocWindow.AssignHandle(this.site.DocumentHandle);
        }

        internal void releaseWndProc()
        {
            if (this.mNativeDocWindow != null)
            {
                this.mNativeDocWindow.ReleaseHandle();
                this.mNativeDocWindow = null;
            }
        }

        override protected void WndProc(ref Message m)
        {
            bool isHandled = false;

            switch (m.Msg)
            {
                case win32.WM_KEYDOWN:

                    //if ctrl pressed, pass it to doShortCut
                    if (win32.GetKeyState((int)Keys.ControlKey) < 0)
                    {
                        //check NOT alt   
                        if (!((win32.GetKeyState((int)Keys.Alt) < 0)
                           ||
                           (win32.GetKeyState((int)Keys.LMenu) < 0)
                           ||
                           (win32.GetKeyState((int)Keys.RMenu) < 0)
                           ))
                        {
                            Keys k = (Keys)m.WParam.ToInt32();
                            isHandled = doShortCut(k);
                        }
                    }

                    //if not handled, pass it to TranslateAccelerator
                    if (!isHandled)
                    {
                        MSG msg = new MSG();
                        msg.hwnd = m.HWnd;
                        msg.wParam = m.WParam;
                        msg.lParam = m.LParam;
                        msg.message = m.Msg;

                        Point pos = new Point(win32.GetMessagePos());
                        msg.pt_x = pos.X;
                        msg.pt_y = pos.Y;
                        msg.time = win32.GetMessageTime();

                        isHandled = site.CallTranslateAccelerator(msg);
                    }

                    break;

                case win32.WM_MOUSEACTIVATE:
                    {
                        this.Select();
                        if (site != null)
                        {

                            if (!this.DesignMode)
                            {
                                IntPtr fromHandle = win32.GetFocus();

                                this.mDocumentHandle = site.DocumentHandle;

                                if ((!this.Focused) & (fromHandle != this.Handle) & (fromHandle != this.mDocumentHandle))
                                {
                                    win32.SendMessage(this.Handle, win32.WM_SETFOCUS, (int)fromHandle, 0);
                                }
                            }

                        }
                        break;
                    }

                case win32.WM_SETFOCUS:
                    this.setFocusToMshtml();
                    isHandled = true;
                    break;

                case win32.WM_KEYUP:

                    break;
            }

            if (!isHandled)
            {
                base.WndProc(ref m);
            }

        }
        #endregion Override WndProc ===============

        #region Override Handle events ===============

        protected override void OnHandleCreated(EventArgs e)
        {
            //earlier versions of HtmlEditor always init mshtml
            //here. However, according to BoundsChecker that 
            //causes a memory overrun. It also means that the 
            //document is passed a rect with zero bounds, as 
            //the control has not yet been sized.

            //Therefore, mshtml is now initialized for the first
            //time in OnParentChanged. In most cases, that will
            //be when the designer-generated code adds the control
            //to the form's controls collection.

            if (!mCreating)
            {
              initMshtml();
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            CleanupControl();
            base.OnHandleDestroyed(e);
        }

        #endregion

        #region Overrides of container control methods ===============

        protected override void OnParentChanged(System.EventArgs e)
        {
            if (this.Parent == null)
            {
                return;
            }

           if (site == null)
            {
              this.createTimer.Enabled = true;
              //initMshtml();
            }

            base.OnParentChanged(e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (this.Visible)
            {
                if (this.bNeedsActivation)
                {
                    this.activateTimer.Enabled = true;
                }
            }
            base.OnVisibleChanged(e);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            if (this.DesignMode)
            {
                pevent.Graphics.FillRectangle(System.Drawing.Brushes.White, this.ClientRectangle);
            }

            base.OnPaint(pevent);
        }

        public override Boolean PreProcessMessage(ref Message m)
        {
            bool callBase = true;


            if (m.Msg == win32.WM_KEYDOWN)
            {
                Keys k = (Keys)m.WParam.ToInt32();

                if ((k == Keys.Return) && this.AcceptsReturn)
                {
                    callBase = false;
                }

                if ((k == Keys.Tab) && this.mAcceptsTab)
                {
                    callBase = false;
                }

                if ((k == Keys.Right) || (k == Keys.Left) || (k == Keys.Up) || (k == Keys.Down))
                {
                    callBase = false;
                }

            }


            if (callBase)
            {
                return base.PreProcessMessage(ref m);
            }
            else
            {
                return false; // don't call base method, don't return true
            }
        }

        #endregion

        #region initialization and cleanup

        #region initialization

        void initMshtml()
        {

            //don't create in design mode
            if (this.DesignMode)
            {
                return;
            }

            if (this.isCreated)
            {
                return; //can't create if already created
            }

            // force creating Host handle since we need it to parent MSHTML
            if (!this.IsHandleCreated)
            {
                IntPtr hostHandle = Handle;
            }

            site = new HtmlSite(this);

            site.CreateDocument();

            if (this.Visible)
            {
                site.ActivateDocument();
            }
            else
            {
                this.bNeedsActivation = true;
                this.activateTimer.Enabled = true;
            }

            this.mHtmlDoc = (HTMLDocument)site.Document;
            this.mDocHTML = new DocHTML(mHtmlDoc);

            if (this.mDesignMode)
            {
                IHTMLDocument2 htmldoc = (IHTMLDocument2)this.mHtmlDoc;
                htmldoc.SetDesignMode("On");

            }

            site.SetPropertyNotifyEvents();

            if (url != String.Empty)
            {
                LoadUrl(url);
            }
            else
            {
                //always initialize with at least the blank url
                LoadUrl("About:Blank");
            }

            if (sDocument != String.Empty)
            {
                LoadDocument(sDocument);
            }
            else
            {
                if (this.mDesignMode)
                {
                    LoadDocument("<html></html>");
                }
            }


        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.activateTimer = new System.Windows.Forms.Timer(this.components);
            this.createTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // activateTimer
            // 
            this.activateTimer.Interval = 1;
            this.activateTimer.Tick += new System.EventHandler(this.activateTimer_Tick);
            this.activateTimer.Enabled = false;

            this.createTimer.Interval = 1;
            this.createTimer.Enabled = false;
            this.createTimer.Tick += new System.EventHandler(this.createTimer_Tick);

            this.ResumeLayout(false);

        }

        private void activateTimer_Tick(object sender, System.EventArgs e)
        {
            this.activateTimer.Enabled = false;
            if (site != null)
            {
                site.ActivateDocument();
            }
        }

        private void createTimer_Tick(object sender, System.EventArgs e)
        {
            this.createTimer.Enabled = false;
            if (!this.isCreated)
            {
                this.initMshtml();
            }
        }

        #endregion

        #region cleanup

        protected override void Dispose(bool disposing)
        {

            if (disposing)
            {

                if (components != null)
                {
                    components.Dispose();
                }

                if (!this.DesignMode)
                {

                    IntPtr ptr = Marshal.GetIDispatchForObject(this);
                    int i = Marshal.Release(ptr);
                    while (i > 0)
                    {
                        i = Marshal.Release(ptr);
                    }
                }

                if (this.site != null)
                {
                    this.site.Dispose();
                }

                if (this.mNativeWindow != null)
                {
                    this.mNativeWindow.ReleaseHandle();
                }

            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        public void CleanupControl()
        {
            if (isCreated)
            {
                site.CloseDocument();
                site.Dispose();
                site = null;
                this.mHtmlDoc = null;
                this.mDocHTML = null;
            }
        }

        public void ReloadMshtml()
        {
            CleanupControl();
            initMshtml();
        }

        #endregion

        #endregion

        #region Declare Events ====================================================

        //declare the events
        public event HtmlNavigateEventHandler Navigate;
        public event ReadyStateChangedHandler ReadyStateChanged;
        public event UpdateUIHandler UpdateUI;
        public event HtmlKeyPressHandler HtmlKeyPress;
        public event HtmlEventHandler HtmlEvent;
        public event BeforeNavigateEventHandler BeforeNavigate;
        public event BeforeShortcutEventHandler BeforeShortcut;
        public event BeforePasteHandler BeforePaste;

        [Description("Fires when content is edited in design mode.")]
        public event EventHandler ContentChanged;

        #endregion Declare Events ===========

        #region Invoke Events =========================================================
        //invoke the UpdateUI event
        public void InvokeUpdateUI(IHTMLElement ae)
        {
            if (UpdateUI != null)
            {
                HtmlUpdateUIEventArgs ea = new HtmlUpdateUIEventArgs();
                ea.currentElement = ae;
                UpdateUI(this, ea);
            }
        }

        //invoke the ReadyStateChanged event
        public void InvokeReadyStateChanged(String newReadyState)
        {
            if (ReadyStateChanged != null)
            {
                ReadyStateChangedEventArgs ea = new ReadyStateChangedEventArgs(newReadyState);
                ReadyStateChanged(this, ea);
            }
        }

        public void InvokeHtmlKeyPress(ref IHTMLEventObj eobj)
        {
            if (HtmlKeyPress != null)
            {
                HtmlKeyPressEventArgs ea = new HtmlKeyPressEventArgs(ref eobj);
                HtmlKeyPress(this, ea);
            }
        }

        [DispId(0)]
        public void InvokeHtmlEvent()
        {
            if (HtmlEvent != null)
            {
                // Get the event.
                IHTMLEventObj pobjEvent = ((IHTMLDocument2)
                    mHtmlDoc).GetParentWindow().eventobj;
                HtmlEventArgs ea = new HtmlEventArgs(pobjEvent);
                HtmlEvent(this, ea);
            }
            return;
        }

        public void InvokeNavigate(String target)
        {

            if (Navigate != null) Navigate(this, new HtmlNavigateEventArgs(target));
        }

        internal void InvokeContentChanged()
        {
            if (ContentChanged != null)
            {
                ContentChanged(this, new System.EventArgs());
            }
        }

        /// <summary>
        /// Fires the BeforeShortcut event. Handle this event to pre-process or cancel the
        /// HtmlEditor's shortcut events.
        /// </summary>
        /// <param name="e">Cancellable event args</param>
        protected virtual void OnBeforeShortcut(onlyconnect.BeforeShortcutEventArgs e)
        {
            if (BeforeShortcut != null) BeforeShortcut(this, e);
        }

        /// <summary>
        /// Fires the BeforeNavigateEvent
        /// </summary>
        /// <param name="e">Cancellable event args</param>
        protected internal virtual void OnBeforeNavigate(onlyconnect.BeforeNavigateEventArgs e)
        {
            if (BeforeNavigate != null) BeforeNavigate(this, e);
        }

        /// <summary>
        /// Fires the BeforePaste event
        /// </summary>
        /// <param name="e"></param>
        protected internal virtual void OnBeforePaste(BeforePasteArgs e)
        {
            if (BeforePaste != null) BeforePaste(this, e);
        }

        #endregion Invoke Events ===============

        #region Setup Event Management ======================================================


        internal void ReadyStateChangeActions(IHTMLEventObj o)
        {

            //defensive - I've known this to be called
            //after doc was deactivated
            if (this.mHtmlDoc == null) return;

            string theReadyState = this.HtmlDocument2.GetReadyState();

            if (theReadyState == "complete")
            {
                //if changed to "COMPLETE", set edit designer

                if (this.bLoadDocumentWhenReady)
                {
                    Debug.WriteLine("Now loading doc");
                    this.LoadDocument(string.Empty);
                    return;
                }
                else if (this.bLoadUrlWhenReady)
                {
                    Debug.WriteLine("Now loading url");
                    this.LoadUrl(this.url);
                    return;
                }

                Debug.WriteLine("Setting events");

                {
                    if (this.IsDesignMode)
                    {
                        this.SetEditDesigner();
                        this.execCommand(commandids.IDM_AUTOURLDETECT_MODE, this.mEnableUrlDetection, false, false);
                        this.setChangeNotify();
                    }
                }

                if (this.bSetComposeSettingsWhenReady)
                {
                    this.setDefaultFont();
                }

                //set HTMLEvents
                this.SetHTMLEvents();


                //refresh ReadyState since the above actions could have changed it
                theReadyState = this.HtmlDocument2.GetReadyState();

            }

            //invoke ready state changed event
            this.InvokeReadyStateChanged(theReadyState);

        }

        private void setChangeNotify()
        {
            //only set this if there is an event handler
            if (ContentChanged == null)
            {
                return;
            }

            int hr;

            this.mChangeMonitor = new ChangeMonitor(this);
            hr = ((IMarkupContainer2)this.HtmlDocument2).RegisterForDirtyRange(mChangeMonitor, out changeCookie);
        }


        public void SetEditDesigner()
        {
            IHTMLDocument2 htmldoc = (IHTMLDocument2)this.mHtmlDoc;
            htmldoc.SetDesignMode("On");
            onlyconnect.IServiceProvider isp = (onlyconnect.IServiceProvider)mHtmlDoc;
            onlyconnect.IHTMLEditServices es;
            System.Guid IHtmlEditServicesGuid = new System.Guid("3050f663-98b5-11cf-bb82-00aa00bdce0b");
            System.Guid SHtmlEditServicesGuid = new System.Guid(0x3050f7f9, 0x98b5, 0x11cf, 0xbb, 0x82, 0x00, 0xaa, 0x00, 0xbd, 0xce, 0x0b);
            IntPtr ppv;
            onlyconnect.IHTMLEditDesigner ds = (onlyconnect.IHTMLEditDesigner)site;
            if (isp != null)
            {
                isp.QueryService(ref SHtmlEditServicesGuid, ref IHtmlEditServicesGuid, out ppv);
                es = (onlyconnect.IHTMLEditServices)Marshal.GetObjectForIUnknown(ppv);
                int retval = es.AddDesigner(ds);
                Marshal.Release(ppv);
            }
        }


        void SetHTMLEvents()
        {
            if ((mHtmlDoc != null) && (HtmlEvent != null))
            {
                //m_htmldoc.onkeydown = this; 
                //keydown does not work, keypress does...
                IHTMLDocument2 htmldoc = (IHTMLDocument2)this.mHtmlDoc;
                IHTMLDocument4 htmldoc4 = (IHTMLDocument4)this.mHtmlDoc;
                IHTMLDocument5 htmldoc5 = (IHTMLDocument5)this.mHtmlDoc;

                htmldoc.SetOnkeypress(this);
                htmldoc.SetOnclick(this);

                htmldoc4.onselectionchange = this;
                htmldoc5.onfocusin = this;
                htmldoc5.onfocusout = this;
            }
        }
        #endregion Setup Event Management ================

        #region The main public API

        [Browsable(false)]
        public IHTMLElement CurrentElement
        {
            get
            {
                return mcurrentElement;
            }
        }

        [System.ComponentModel.TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter)),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        Description("Sets the initial font used by the editor. Disabled by default as it inserts a FONT tag.")]
        public ComposeSettings DefaultComposeSettings
        {
            get
            {
                if (this.mComposeSettings == null)
                {
                    mComposeSettings = new ComposeSettings(this);
                }

                return mComposeSettings;
            }
        }

        [Description("Encoding to use when getting the string value of the HTML document"), DefaultValue(false)]
        public EncodingType DocumentEncoding
        {
            get { return mDocumentEncoding; }
            set { mDocumentEncoding = value; }
        }

        [Description("The preamble to be added by the editor if no byte order mark is detected"), DefaultValue(false)]
        public EncodingType DefaultPreamble
        {
            get { return mDefaultPreamble; }
            set { mDefaultPreamble = value; }
        }

        [Description("Indicates if return characters are accepted as input."),
        DefaultValue(true)]
        public bool AcceptsReturn
        {
            get
            {
                return this.mAcceptsReturn;
            }
            set
            {
                this.mAcceptsReturn = value;
            }
        }

        [Description("Indicates if tab characters are accepted as input. Set false to enable tabbing between controls."),
        DefaultValue(false)]
        public bool AcceptsTab
        {
            get
            {
                return this.mAcceptsTab;
            }
            set
            {
                this.mAcceptsTab = value;
            }
        }

        [Description("Turn off auto-detection of string type in LoadDocument and always treat it as an Ansi string."), DefaultValue(false)]
        public Boolean IsAnsiStreamAlwaysUsed
        {
            get
            {
                return this.mAlwaysLoadAnsi;
            }
            set
            {
                this.mAlwaysLoadAnsi = value;
            }
        }

        [Description("Is the control in edit mode so that users can edit the contents."), DefaultValue(false)]
        public Boolean IsDesignMode
        {
            get { return mDesignMode; }
            set
            {
                if (value)
                {
                    mDesignMode = true;
                    if (this.mHtmlDoc != null)
                    {

                        IHTMLDocument2 htmldoc = (IHTMLDocument2)this.mHtmlDoc;
                        htmldoc.SetDesignMode("On");
                        if (this.mComposeSettings.Enabled)
                        {
                            this.setDefaultFont();
                        }
                        //refresh url detection
                        this.IsUrlDetectionEnabled = this.mEnableUrlDetection;
                        this.LoadDocument("<html></html>");

                    }
                }
                else
                {
                    mDesignMode = false;
                    if (this.mHtmlDoc != null)
                    {
                        IHTMLDocument2 htmldoc = (IHTMLDocument2)this.mHtmlDoc;
                        htmldoc.SetDesignMode("Off");
                    }

                }
            }
        }

        [Description("Gets/Sets if the context menu on right clicks will be displayed."), DefaultValue(true)]
        public bool IsContextMenuEnabled
        {
            get
            {
                return mIsContextMenuEnabled;
            }
            set
            {
                mIsContextMenuEnabled = value;
            }
        }

        [Description("Gets/Sets if the control can receive the focus."), DefaultValue(true)]
        public bool IsActivationEnabled
        {
            get
            {
                return mAllowActivation;
            }
            set
            {
                mAllowActivation = value;
            }
        }


        [Description("Gets/Sets whether the Tab key, in design mode, stays within the HtmlEditor or sets focus on the next control in the tab order."),
          DefaultValue(true), Browsable(false)]
        public bool IsTabToNextControl
        {
            get
            {
                return !AcceptsTab;
            }
            set
            {
                AcceptsTab = !value;
            }
        }

        [Description("Gets/Sets where the control stores preferences in the registry."),
        DefaultValue(null)]
        public string OptionKeyPath
        {
            get
            {
                return this.mOptionKeyPath;
            }
            set
            {
                mOptionKeyPath = value;
            }
        }

        [Description("Gets/Sets if scripts, Java etc is enabled"), DefaultValue(true)]
        public bool IsActiveContentEnabled
        {
            //This must be set BEFORE the document is created

            get
            {
                return mEnableActiveContent;
            }

            set
            {
                mEnableActiveContent = value;
            }
        }

        [Description("Gets/Sets if autocomplete is enabled"), DefaultValue(false)]
        public bool IsAutoCompleteEnabled
        {
            //This must be set BEFORE the document is created

            get
            {
                return mEnableAutoComplete;
            }

            set
            {
                mEnableAutoComplete = value;
            }
        }

        [Description("Gets/Sets if urls are automatically changed to hyperlinks"), DefaultValue(false)]
        public bool IsUrlDetectionEnabled
        {
            get
            {
                return this.mEnableUrlDetection;
            }

            set
            {
                this.mEnableUrlDetection = value;
                // we does this both here and in ReadyStateCompleteActions
                // it doesn't take effect if doc is not ready
                if (this.IsDesignMode)
                {
                    this.SetEditDesigner();
                    this.execCommand(commandids.IDM_AUTOURLDETECT_MODE, this.mEnableUrlDetection, false, true);
                }
            }
        }

        [Description("Gets/Sets if pressing Enter creates DIV rather than P"), DefaultValue(false)]
        public bool IsDivOnEnter
        {
            //This must be set BEFORE the document is created

            get
            {
                return mDivOnEnter;
            }

            set
            {
                mDivOnEnter = value;
            }
        }

        [Description("Gets/Sets if a vertical scroll bar is shown"), DefaultValue(true)]
        public bool IsScrollBarShown
        {
            //This must be set BEFORE the document is created

            get
            {
                return this.mShowScrollBars;
            }

            set
            {
                mShowScrollBars = value;
            }
        }

        [Browsable(false)]
        public HTMLDocument Document
        {
            get
            {
                if (site == null)
                {
                    return null;
                }

                if (site.Document != null)
                {
                    return (HTMLDocument)site.Document;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// The thinking behind this is to wrap selected key properties and methods
        /// in a developer-friendly class - called DocHTML / DocumentHTML to distinguish
        /// from the other official items
        /// </summary>
        [Browsable(false)]
        public DocHTML DocumentHTML
        {
            get
            {
                return this.mDocHTML;
            }
        }

        /// <summary>
        /// Quick access to the key IHTMLDocument2 interface
        /// </summary>
        [Browsable(false)]
        public IHTMLDocument2 HtmlDocument2
        {
            get
            {
                if (site == null)
                {
                    return null;
                }

                if (site.Document != null)
                {
                    return (IHTMLDocument2)site.Document;
                }
                else
                {
                    return null;
                }
            }
        }

        [Browsable(false)]
        public string ReadyState
        {
            get
            {
                if (this.mHtmlDoc == null)
                {
                    return String.Empty;
                }

                return HtmlDocument2.GetReadyState().ToLower();
            }
        }

        /// <summary>
        /// Gets/Sets if links that are clicked on in the editor will be opened in the editor or launch your default browser.
        /// </summary>
        [Description("Gets/Sets if links that are clicked on in the editor will be opened in the editor or launch your default browser."), DefaultValue(false)]
        public bool OpenLinksInNewWindow
        {
            get { return mLinksNewWindow; }
            set
            {
                mLinksNewWindow = value;
            }
        }

        public void SetStyleSheet(string sFileName)
        {
            if (this.mHtmlDoc == null)
            {
                return;
            }

            IHTMLDocument2 htmldoc = (IHTMLDocument2)this.mHtmlDoc;

            if (htmldoc.GetReadyState().ToLower() == "complete")
            {
                htmldoc.CreateStyleSheet(sFileName, 0);
            }
        }

        public bool Copy()
        {
            return this.execCommand(commandids.IDM_COPY, null, false, true);
        }

        public bool Paste()
        {
            BeforePasteArgs e = new BeforePasteArgs();
            this.OnBeforePaste(e);
            if (e.Cancel)
            {
                return false;
            }
            else
                return this.execCommand(commandids.IDM_PASTE, null, false, true);
        }

        public bool SaveAs(string DefaultPath)
        {
            return this.execCommand(commandids.IDM_SAVEAS, DefaultPath, true, true);
        }

        public bool Undo()
        {
            return this.execCommand(commandids.IDM_UNDO, null, false, true);
        }

        public bool Redo()
        {
            return this.execCommand(commandids.IDM_REDO, null, false, true);
        }

        public bool Cut()
        {
            return this.execCommand(commandids.IDM_CUT, null, false, true);
        }

        public bool SetSelectionBold()
        {
            return this.execCommand(commandids.IDM_BOLD, null, false, true);
        }

        public bool SetSelectionItalic()
        {
            return this.execCommand(commandids.IDM_ITALIC, null, false, true);
        }

        public bool ClearSelectionFormatting()
        {
            return this.execCommand(commandids.IDM_REMOVEFORMAT, null, false, true);
        }

        [Browsable(false)]
        public Color SelectionBackColor
        {
            get
            {
                if (Document == null) return Color.Empty;
                else
                {
                    object Result = HtmlDocument2.QueryCommandValue("ForeColor");
                    try
                    {
                        return System.Drawing.ColorTranslator.FromHtml(Result.ToString());
                    }
                    catch
                    {
                        return Color.Empty;
                    }
                }
            }
            set
            {
                //need to send a COLORREF value
                this.execCommand(commandids.IDM_BACKCOLOR, utils.GetCsColor(value), false, true);
            }
        }

        /// <summary>
        /// Gets/Sets the color for the selected text.
        /// </summary>
        [Description("Gets/Sets the color of the selected text."), Browsable(false)]
        public Color SelectionForeColor
        {
            get
            {
                if (Document == null) return Color.Empty;
                else
                {
                    object Result = HtmlDocument2.QueryCommandValue("ForeColor");
                    try
                    {
                        return System.Drawing.ColorTranslator.FromHtml(Result.ToString());
                    }
                    catch
                    {
                        return Color.Empty;
                    }
                }
            }
            set
            {
                //need to send a COLORREF value
                this.execCommand(commandids.IDM_FORECOLOR, utils.GetCsColor(value), false, true);
            }
        }

        [Description("Gets/sets the font for the selected text"), Browsable(false)]
        public Font SelectionFont
        {
            get
            {
                try
                {
                    //This is here because of hosting issues on a UserControl 
                    if (this.Document == null) return null;

                    System.Drawing.FontStyle fs = new FontStyle();
                    int FontSize = 8;

                    if (Convert.ToBoolean(HtmlDocument2.QueryCommandValue("bold"))) fs |= FontStyle.Bold;
                    if (Convert.ToBoolean(HtmlDocument2.QueryCommandValue("italic"))) fs |= FontStyle.Italic;
                    if (Convert.ToBoolean(HtmlDocument2.QueryCommandValue("underline"))) fs |= FontStyle.Underline;

                    switch (Convert.ToInt32(HtmlDocument2.QueryCommandValue("FontSize")))
                    {
                        case 1:
                            FontSize = 8;
                            break;
                        case 2:
                            FontSize = 10;
                            break;
                        case 3:
                            FontSize = 12;
                            break;
                        case 4:
                            FontSize = 18;
                            break;
                        case 5:
                            FontSize = 24;
                            break;
                        case 6:
                            FontSize = 36;
                            break;
                        case 7:
                            FontSize = 48;
                            break;
                    }

                    string FontName = HtmlDocument2.QueryCommandValue("FontName").ToString();

                    return new Font(FontName, FontSize, fs);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                try
                { //This is here because of hosting issues on a UserControl
                    if ((Document == null) | (value == null)) return;

                    HtmlDocument2.ExecCommand("FontName", false, value.Name);

                    if (value.SizeInPoints <= 8)
                    {
                        HtmlDocument2.ExecCommand("FontSize", false, 1);
                    }
                    else if (value.SizeInPoints <= 10)
                    {
                        HtmlDocument2.ExecCommand("FontSize", false, 2);
                    }
                    else if (value.SizeInPoints <= 12)
                    {
                        HtmlDocument2.ExecCommand("FontSize", false, 3);
                    }
                    else if (value.SizeInPoints <= 18)
                    {
                        HtmlDocument2.ExecCommand("FontSize", false, 4);
                    }
                    else if (value.SizeInPoints <= 24)
                    {
                        HtmlDocument2.ExecCommand("FontSize", false, 5);
                    }
                    else if (value.SizeInPoints <= 36)
                    {
                        HtmlDocument2.ExecCommand("FontSize", false, 6);
                    }
                    else
                    {
                        HtmlDocument2.ExecCommand("FontSize", false, 7);
                    }

                    if (Convert.ToBoolean(HtmlDocument2.QueryCommandValue("bold")) != Font.Bold) HtmlDocument2.ExecCommand("bold", false, null);
                    if (Convert.ToBoolean(HtmlDocument2.QueryCommandValue("italic")) != Font.Bold) HtmlDocument2.ExecCommand("italic", false, null);
                    if (Convert.ToBoolean(HtmlDocument2.QueryCommandValue("underline")) != Font.Bold) HtmlDocument2.ExecCommand("underline", false, null);
                }
                catch { }
            }
        }

        /// <summary>
        /// Gets/Sets the alignment of the selected text.
        /// </summary>
        [Description("Gets/Sets the alignment of the selected text."), Browsable(false)]
        public System.Windows.Forms.HorizontalAlignment SelectionAlignment
        {
            get
            {
                if (Document == null) return HorizontalAlignment.Left;
                else
                {
                    if (Convert.ToBoolean(HtmlDocument2.QueryCommandValue("JustifyRight")))
                    {
                        return HorizontalAlignment.Right;
                    }
                    else if (Convert.ToBoolean(HtmlDocument2.QueryCommandValue("JustifyCenter")))
                    {
                        return HorizontalAlignment.Center;
                    }
                    else
                    {
                        return HorizontalAlignment.Left;
                    }
                }
            }
            set
            {
                if (Document == null) return;

                switch (value)
                {
                    case HorizontalAlignment.Left:
                        HtmlDocument2.ExecCommand("JustifyLeft", false, null);
                        break;
                    case HorizontalAlignment.Center:
                        HtmlDocument2.ExecCommand("JustifyCenter", false, null);
                        break;
                    case HorizontalAlignment.Right:
                        HtmlDocument2.ExecCommand("JustifyRight", false, null);
                        break;
                }
            }
        }

        /// <summary>
        /// Get/Sets if numbering is on for the selected text.
        /// </summary>
        [Description("Get/Sets if numbering is on for the selected text."), Browsable(false)]
        public bool SelectionNumbering
        {
            get
            {
                if (Document == null) return false;
                else return Convert.ToBoolean(HtmlDocument2.QueryCommandValue("InsertOrderedList"));
            }
            set
            {
                if (Document != null && Convert.ToBoolean(HtmlDocument2.QueryCommandValue("InsertOrderedList")) != value) HtmlDocument2.ExecCommand("InsertOrderedList", false, null);
            }
        }

        /// <summary>
        /// Gets/Sets if bullets are on or off for the selected text.
        /// </summary>
        [Description("Gets/Sets if bullets are on or off for the selected text."), Browsable(false)]
        public bool SelectionBullets
        {
            get
            {
                if (Document == null) return false;
                else return Convert.ToBoolean(HtmlDocument2.QueryCommandValue("InsertUnorderedList"));
            }
            set
            {
                if (Document != null && Convert.ToBoolean(HtmlDocument2.QueryCommandValue("InsertUnorderedList")) != value) HtmlDocument2.ExecCommand("InsertUnorderedList", false, null);
            }
        }

        public bool SelectAll()
        {
            return this.execCommand(commandids.IDM_SELECTALL, null, false, true);
        }

        public void ClearSelection()
        {
            this.HtmlDocument2.GetSelection().Empty();
        }

        public void DeleteSelection()
        {
            this.execCommand(commandids.IDM_DELETE, null, false, true);
        }

        #region Load and save documents

        public void LoadDocument(String documentVal)
        {
            if ((documentVal != string.Empty) | (!this.bLoadDocumentWhenReady))
            {
                //if doc is waiting to load, it is already in string variable
                this.bLoadDocumentWhenReady = false; 
                sDocument = documentVal;
            }
            else
            {
                this.bLoadDocumentWhenReady = false;  
                this.iLoadAttempts += 1;
            }

            if (!isCreated)
            {
                if (iLoadAttempts < 2)
                {
                    this.bLoadDocumentWhenReady = true;
                    return;
                }
                else
                {
                    throw new HtmlEditorException("Document not created");
                }
           
            }

            if ((this.HtmlDocument2.GetReadyState().ToLower() != "complete") & (this.HtmlDocument2.GetReadyState().ToLower() != "interactive"))
            //try to load on interactive as well as complete
            {
                if (iLoadAttempts < 2)
                {
                    this.bLoadDocumentWhenReady = true;
                    return;
                }
                else
                {
                    throw new HtmlEditorException("Document not ready");
                }
            }

            //this is a fix for exception raised in UpdateUI
            site.mFullyActive = false;

            Encoding theDefaultPreamble = getEncodingFromEncodingType(this.mDefaultPreamble);

            utils.LoadDocument(ref this.mHtmlDoc, sDocument, this.mAlwaysLoadAnsi, false, theDefaultPreamble);

            this.iLoadAttempts = 0;

        }


        public void LoadUrl(String url)
        {
            this.bLoadUrlWhenReady = false;
            this.url = url;

            if (!isCreated)
            {
                Debug.WriteLine("Doc not created" + iLoadAttempts.ToString());
                if (iLoadAttempts < 2)
                {
                    this.bLoadUrlWhenReady = true;
                    return;
                }
                else
                {
                    throw new HtmlEditorException("Document not created");
                }
            }

            if (!isCreated) return;

            //this is a workaround for a problem calling Caret.SetLocation before it
            //is ready, in UpdateUI
            site.mFullyActive = false;

            //don't ask for downloadOnly since clientsite is already set
            utils.LoadUrl(ref mHtmlDoc, url, false);
            iLoadAttempts = 0;
        }

        public String GetDocumentSource()
        {
            if (!isCreated) return null;

            HTMLDocument thedoc = this.Document;

            if (thedoc == null) return null;

            return utils.GetDocumentSource(ref thedoc, this.mDocumentEncoding);

        }



        #endregion


        public void ShowFindDialog()
        {
            this.execCommand(commandids.IDM_FIND, null, true, true);
        }


        public void Print(bool bPreview)
        {
            this.Print(bPreview, string.Empty, true);

        }

        public void Print(bool bPreview, String sTemplatePath)
        {
            this.Print(bPreview, sTemplatePath, true);

        }

        public void Print(bool bPreview, String sTemplatePath, bool bPromptUser)
        {

            Object pvaIn;

            if (sTemplatePath == string.Empty)
            {
                pvaIn = null;
            }
            else if (!File.Exists(sTemplatePath))
            {
                pvaIn = null;
            }
            else
            {
                pvaIn = sTemplatePath;
            }


            if (bPreview)
            {
                this.execCommand(commandids.IDM_PRINTPREVIEW, pvaIn, bPromptUser, true);

            }
            else
            {
                this.execCommand(commandids.IDM_PRINT, pvaIn, bPromptUser, true);
            }

        }



        public bool Stop()
        {
            return this.execCommand(commandids.IDM_STOP, null, false, false);
        }


        #endregion

        #region private properties and methods

        Boolean isCreated
        {
            get
            {
                return (site != null) && (site.Document != null);
            }
        }


        internal bool execCommand(uint iCommand, object argument, bool bPromptUser, bool checkReadystate)
        {
            if (this.mHtmlDoc == null)
            {
                return false;
            }

            //Check readystate

            if (checkReadystate)
            {
                if (this.ReadyState != "complete")
                {
                    //throw new HtmlEditorException("Document is not ready");
                    return false;
                }
            }

            //get the command target
            IOleCommandTarget ct = (IOleCommandTarget)this.mHtmlDoc;

            if (ct == null)
            {
                throw new HtmlEditorException("Cannot get COM command target");
            }

            //exec the command
            System.Guid pguidCmdGroup = new Guid("DE4BA900-59CA-11CF-9592-444553540000");

            Object pvaOut = null;
            int iRetval;

            uint iPromptUser;

            if (bPromptUser)
            {
                iPromptUser = (uint)OLECMDEXECOPT.OLECMDEXECOPT_PROMPTUSER;
            }
            else
            {
                iPromptUser = (uint)OLECMDEXECOPT.OLECMDEXECOPT_DONTPROMPTUSER;
            }

            iRetval = ct.Exec(ref pguidCmdGroup, iCommand, iPromptUser, ref argument, ref pvaOut);

            return (iRetval == 0);

        }

        /// <summary>
        /// Queries the status of the specified command
        /// </summary>
        private CommandStatus GetCommandInfo(int command)
        {
            //get the command target
            IOleCommandTarget ct = (IOleCommandTarget)this.mHtmlDoc;

            if (ct == null)
            {
                throw new HtmlEditorException("Cannot get COM command target");
            }

            System.Guid pguidCmdGroup = new Guid("DE4BA900-59CA-11CF-9592-444553540000");

            //Query the command target for the command status

            OLECMD oleCommand = new OLECMD();

            oleCommand.cmdID = command;

            onlyconnect.OLECMDTEXT ot = new onlyconnect.OLECMDTEXT();

            int hr = ct.QueryStatus(ref pguidCmdGroup, 1, oleCommand, ref ot);

            if (hr != HRESULT.S_OK)
            {
                return CommandStatus.Unknown;
            }


            if ((oleCommand.cmdf | (int)OLECMDF.OLECMDF_LATCHED) == (int)OLECMDF.OLECMDF_LATCHED)
            {
                return CommandStatus.EnabledAndToggledOn;
            }
            else if ((oleCommand.cmdf | (int)OLECMDF.OLECMDF_ENABLED) == (int)OLECMDF.OLECMDF_ENABLED)
            {
                return CommandStatus.Enabled;
            }
            else if ((oleCommand.cmdf | (int)OLECMDF.OLECMDF_SUPPORTED) == (int)OLECMDF.OLECMDF_SUPPORTED)
            {
                return CommandStatus.Disabled;
            }
            else
            {
                return CommandStatus.Unsupported;
            }

        }

        private bool isCommandEnabled(int command)
        {
            CommandStatus cs = this.GetCommandInfo(command);

            if ((cs == CommandStatus.Enabled) | (cs == CommandStatus.EnabledAndToggledOn))
            {
                return true;
            }
            else
            {
                return false;
            }


        }

        /// <summary>
        /// set focus to the hosted document, not just to the winform control
        /// </summary>
        private void setFocusToMshtml()
        {
            if (this.mNativeDocWindow != null)
            {
                win32.SetFocus(this.mNativeDocWindow.Handle);
            }
        }

        internal bool setDefaultFont()
        {

            if (!this.mDesignMode) return false;

            if (this.HtmlDocument2.GetReadyState() != "complete")
            {
                this.bSetComposeSettingsWhenReady = true;
                return false;
            }

            this.bSetComposeSettingsWhenReady = false;

            object o = mComposeSettings.CommandString;

            if (this.execCommand(commandids.IDM_HTMLEDITMODE, true, false, false))
            {
                return this.execCommand(commandids.IDM_COMPOSESETTINGS, o, false, false);
            }
            else
            {
                return false;
            }
        }

        Encoding getEncodingFromEncodingType(EncodingType enc)
        {
            switch (enc)
            {
                case EncodingType.ASCII: return Encoding.ASCII;
                case EncodingType.Auto: return Encoding.Default;
                case EncodingType.Unicode: return Encoding.Unicode;
                case EncodingType.UTF7: return Encoding.UTF7;
                case EncodingType.UTF8: return Encoding.UTF8;
                case EncodingType.WindowsCurrent: return Encoding.Default;
                default: return Encoding.Default;
            }
        }

        #endregion

    }



    class HandleWndProc : NativeWindow
    {
        internal HtmlEditor thecontrol;

        protected override void WndProc(ref Message message)
        {
            bool bPassToControl = false;

            //The idea is only pass the key presses and mouse clicks (and not the right clicks) to the base form to process the events correctly.
            if ((message.Msg >= 0x100 && message.Msg <= 0x0108) ||
                (message.Msg >= 0x200 && message.Msg <= 0x020A &&
                message.Msg != win32.WM_LBUTTONDOWN
                && message.Msg != win32.WM_RBUTTONDOWN
                && message.Msg != win32.WM_LBUTTONDBLCLK
                && message.Msg != win32.WM_MBUTTONDBLCLK
                && message.Msg != win32.WM_RBUTTONDBLCLK))
            {
                bPassToControl = true;
            }
            else
            {
                //These are separate because if you pass it to the base control then the right mouse clicks etc. don't fire.
                switch (message.Msg)
                {
                    case win32.WM_LBUTTONDOWN:
                        thecontrol.InvokeOnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, message.LParam.ToInt32() & 0xffff, Convert.ToInt32((message.LParam.ToInt32() & 0xffff0000) >> 16), 0));
                        break;
                    case win32.WM_RBUTTONDOWN:
                        thecontrol.InvokeOnMouseDown(new MouseEventArgs(MouseButtons.Right, 1, message.LParam.ToInt32() & 0xffff, Convert.ToInt32((message.LParam.ToInt32() & 0xffff0000) >> 16), 0));
                        break;
                    case win32.WM_LBUTTONDBLCLK:
                    case win32.WM_MBUTTONDBLCLK:
                    case win32.WM_RBUTTONDBLCLK:
                        thecontrol.InvokeOnDoubleClick();
                        break;
                }
            }


            if (message.Msg == win32.WM_KEYDOWN)
            {
                Keys k = (Keys)message.WParam.ToInt32();
                if (k == Keys.Back)
                {
                    bPassToControl = false;
                }

                if ((k == Keys.Right) || (k == Keys.Left) || (k == Keys.Up) || (k == Keys.Down))
                {
                    bPassToControl = false;
                }
            }

            if (bPassToControl)
            {
                thecontrol.InvokeWndProc(ref message);
            }

            base.WndProc(ref message);
        }

    }


    public enum EncodingType
    {
        UTF7,
        UTF8,
        Unicode,
        ASCII,
        WindowsCurrent,
        Auto
    }

    public enum CommandStatus : byte
    {
        Unsupported,
        Disabled,
        Enabled,
        EnabledAndToggledOn,
        Unknown
    }
}
