using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace onlyconnect
{
	/// <summary>
	/// Event Handler classes for the HtmlEditor
	/// </summary>

	public delegate void HtmlNavigateEventHandler(Object s,
	HtmlNavigateEventArgs e);
	
	public class HtmlNavigateEventArgs
	{
		String target;

		public HtmlNavigateEventArgs(String target)
		{
			this.target = target;
		}

		public String Target
		{
			get { return target; }
		}
	}


	public class HtmlUpdateUIEventArgs : EventArgs
	{
		private IHTMLElement mcurrentElement;
		public HtmlUpdateUIEventArgs(): base()
		{

		}

		public IHTMLElement currentElement
		{
			get 
			{
				return mcurrentElement;
			}
			set 
			{
				mcurrentElement = value;
			}
		}
	}

	//declare the event handler
	public delegate void ReadyStateChangedHandler(object sender, ReadyStateChangedEventArgs e);

	//declare the event args

	public class ReadyStateChangedEventArgs : EventArgs 
	{
		private string mReadyState;
		public ReadyStateChangedEventArgs(string readystateVal) : base()
		{
			mReadyState = readystateVal;
		}

		public string ReadyState 
		{
			get 
			{
				return mReadyState;
			}
			//			set 
			//			{
			//				mReadyState = value;
			//			}
		}
		
	}

	//declare the update event handler
	public delegate void UpdateUIHandler(object sender, HtmlUpdateUIEventArgs e);


	//declare the keypress event handler
	public delegate void HtmlKeyPressHandler(object sender, HtmlKeyPressEventArgs e);

	//declare the event args

	public class HtmlKeyPressEventArgs : EventArgs 
	{
		private IHTMLEventObj m_ev;
		public HtmlKeyPressEventArgs(ref  IHTMLEventObj ev) : base()
		{
			m_ev = ev;
		}

		public IHTMLEventObj HtmlEventObject
		{
			get 
			{
				return m_ev;
			}
		}
		
	}

	public delegate void HtmlEventHandler(Object s, HtmlEventArgs e);

	public class HtmlEventArgs
	{
		public IHTMLEventObj Event;        
 
		public HtmlEventArgs(IHTMLEventObj Event)
		{
			this.Event = Event;
		}
	}

    public delegate void BeforeNavigateEventHandler(object s, BeforeNavigateEventArgs e);

    /// <summary>
    /// Used for the BeforeNavigate Event
    /// </summary>
    public class BeforeNavigateEventArgs : System.ComponentModel.CancelEventArgs
    {
        string pTarget = string.Empty;
        string pNewTarget = string.Empty;

        public delegate void BeforeNavigateEventHandler(object s, BeforeNavigateEventArgs e);

        public BeforeNavigateEventArgs(string Target)
        {
            this.pTarget = Target;
            this.pNewTarget = Target;
        }

        /// <summary>
        /// Gets the URL that will be navigated to.
        /// </summary>
        [Description("Gets the URL that will be navigated to.")]
        public string Target
        {
            get { return pTarget; }
        }

        /// <summary>
        /// Gets/Sets the revised URL that will be used to navigate.
        /// </summary>
        [Description("Gets/Sets the revised URL that will be used to navigate.")]
        public string NewTarget
        {
            get { return pTarget; }
            set
            {
                pTarget = value;
            }
        }
    }


    public delegate void BeforeShortcutEventHandler(HtmlEditor h, BeforeShortcutEventArgs e);

    public class BeforeShortcutEventArgs
    {
        bool mCancel = false;
        Keys mKey;

        public BeforeShortcutEventArgs(Keys key)
        {
            mKey = key;
        }

        public Keys Key
        {
            get
            { return mKey; }
        }

        public bool Cancel
        {
            get
            { return mCancel; }
            set
            { mCancel = value; }
        }

    }

    public delegate void BeforePasteHandler(object s, BeforePasteArgs e);

    public class BeforePasteArgs
    {
        bool mCancel = false;

        public bool Cancel
        {
            get
            { return mCancel; }
            set
            { mCancel = value; }
        }



    }
}
