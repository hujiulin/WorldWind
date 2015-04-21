using System.Collections.Specialized;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System;
using WorldWind.Net;
using WorldWind;
using WorldWind.Renderable;
using WorldWind.VisualControl;
using Utility;

namespace WorldWind
{
	/// <summary>
	/// WMS Browser dialog.
	/// </summary>
	public class WMSBrowser : System.Windows.Forms.Form
	{
		private int currentFrame;
		private AnimationState animationState = AnimationState.Stop;

		string wms_config_filepath = Path.Combine( 
			Path.Combine(MainApplication.Settings.ConfigPath, "Earth"),
			Path.Combine("Tools", "wms_server_list.xml"));
		string saved_dirpath = Path.Combine( MainApplication.DirectoryPath, 
			Path.Combine( MainApplication.Settings.CachePath, "WMS Browser"));
		
		private WorldWindow worldWindow;
		private ImageLayer imageLayer;

		private MyWMSLayer curLoadedLayer;
		private WMSLayerStyle curLoadedStyle;

		private WMSList wmsList;
		private bool isRefreshed; // true when server list has been refreshed
		private bool isRefreshing; // true while server list is being refreshed

		private Colorbar colorbar;
		private System.Drawing.Point mouseLocationProgressBarAnimation = new Point(0,0);

		private Thread downloadThread;
		private System.Timers.Timer animationTimer = new System.Timers.Timer();
		private System.Collections.ArrayList animationFrames = new ArrayList();
		private System.Collections.Queue downloadQueue = new Queue();

		private System.Windows.Forms.TreeView treeViewTableOfContents;
		private System.Windows.Forms.RichTextBox richTextBoxStyleDescription;
		private System.Windows.Forms.ComboBox comboBoxTime;
		private System.Windows.Forms.Panel panelContents;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.GroupBox groupBoxLayerOptions;
		private System.Windows.Forms.Button buttonLegend;
		private System.Windows.Forms.ContextMenu contextMenuLegendUrl;
		private System.Windows.Forms.Button buttonStillImage;
		private System.Windows.Forms.Button buttonPlay;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.TrackBar trackBarSpeed;
		private System.Windows.Forms.ProgressBar progressBarStatus;
		private System.Windows.Forms.Label statusBarLabel;
		private System.Windows.Forms.Panel panelStatus;
		private System.Windows.Forms.Button buttonResetExtents;
		private System.Windows.Forms.NumericUpDown numericUpDownNorth;
		private System.Windows.Forms.NumericUpDown numericUpDownEast;
		private System.Windows.Forms.NumericUpDown numericUpDownWest;
		private System.Windows.Forms.NumericUpDown numericUpDownSouth;
		private System.Windows.Forms.GroupBox groupBoxExtents;
		private System.Windows.Forms.Button buttonCurrentExtents;
		private System.Windows.Forms.Label labelNorth;
		private System.Windows.Forms.Label labelSouth;
		private System.Windows.Forms.Label labelWest;
		private System.Windows.Forms.Label labelEast;
		private System.Windows.Forms.ProgressBar progressBarAnimation;
		private System.Windows.Forms.ComboBox comboBoxAnimationStartTime;
		private System.Windows.Forms.ComboBox comboBoxAnimationEndTime;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPageSingleImage;
		private System.Windows.Forms.TabPage tabPageAnimation;
		private System.Windows.Forms.Label labelTransparency;
		private System.Windows.Forms.Label labelHeight;
		private System.Windows.Forms.NumericUpDown numericUpDownHeight;
		private System.Windows.Forms.NumericUpDown numericUpDownTransparency;
		private System.Windows.Forms.CheckBox checkBoxLoadCache;
		private System.Windows.Forms.CheckBox checkBoxAnimationCache;
		private System.Windows.Forms.Button buttonStop;
		private System.Windows.Forms.Button buttonStepBack;
		private System.Windows.Forms.Button buttonStepForward;
		private System.Windows.Forms.ImageList imageList2;
		private System.Windows.Forms.Button buttonClear;
		private System.Windows.Forms.Panel panelLower;
		private System.Windows.Forms.GroupBox groupBoxAnimationTimeFrame;
		private System.Windows.Forms.Label labelLegend;
		private System.Windows.Forms.Label labelKM;
		private System.Windows.Forms.Label labelPercent;
		private System.Windows.Forms.Label labelFps;
		private System.Windows.Forms.Label labelSpeed;
		private System.Windows.Forms.Label labelTime;
		private System.ComponentModel.IContainer components;

		public WMSBrowser(WorldWindow ww)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.worldWindow = ww;
		}

		protected override void OnLoad(EventArgs e)
		{
			StartDownloadThread();

			this.UpdateAnimationSpeed();
			this.animationTimer.Elapsed += new System.Timers.ElapsedEventHandler(animationTimer_Elapsed);
			//this.animationTimer.Start();
			
			base.OnLoad (e);
		}

		void StartDownloadThread()
		{
			if (downloadThread!=null && downloadThread.IsAlive)
				return;
			downloadThread = new Thread( new ThreadStart(Downloader));
			downloadThread.Name = "WMSBrowser.Downloader";
			downloadThread.IsBackground = true;
			downloadThread.Start();
		}

		void StopDownloadThread()
		{
			if (downloadThread==null || !downloadThread.IsAlive)
				return;

			downloadThread.Abort();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}

			if(colorbar != null)
			{
				colorbar.Dispose();
				colorbar = null;
			}

			StopDownloadThread();

			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(WMSBrowser));
			this.richTextBoxStyleDescription = new System.Windows.Forms.RichTextBox();
			this.treeViewTableOfContents = new System.Windows.Forms.TreeView();
			this.progressBarStatus = new System.Windows.Forms.ProgressBar();
			this.buttonLegend = new System.Windows.Forms.Button();
			this.contextMenuLegendUrl = new System.Windows.Forms.ContextMenu();
			this.comboBoxTime = new System.Windows.Forms.ComboBox();
			this.panelContents = new System.Windows.Forms.Panel();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.groupBoxLayerOptions = new System.Windows.Forms.GroupBox();
			this.labelPercent = new System.Windows.Forms.Label();
			this.labelKM = new System.Windows.Forms.Label();
			this.labelLegend = new System.Windows.Forms.Label();
			this.numericUpDownHeight = new System.Windows.Forms.NumericUpDown();
			this.numericUpDownTransparency = new System.Windows.Forms.NumericUpDown();
			this.labelHeight = new System.Windows.Forms.Label();
			this.labelTransparency = new System.Windows.Forms.Label();
			this.buttonStillImage = new System.Windows.Forms.Button();
			this.buttonResetExtents = new System.Windows.Forms.Button();
			this.progressBarAnimation = new System.Windows.Forms.ProgressBar();
			this.trackBarSpeed = new System.Windows.Forms.TrackBar();
			this.buttonPlay = new System.Windows.Forms.Button();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.statusBarLabel = new System.Windows.Forms.Label();
			this.panelStatus = new System.Windows.Forms.Panel();
			this.numericUpDownNorth = new System.Windows.Forms.NumericUpDown();
			this.numericUpDownEast = new System.Windows.Forms.NumericUpDown();
			this.numericUpDownWest = new System.Windows.Forms.NumericUpDown();
			this.numericUpDownSouth = new System.Windows.Forms.NumericUpDown();
			this.groupBoxExtents = new System.Windows.Forms.GroupBox();
			this.labelEast = new System.Windows.Forms.Label();
			this.labelWest = new System.Windows.Forms.Label();
			this.labelSouth = new System.Windows.Forms.Label();
			this.labelNorth = new System.Windows.Forms.Label();
			this.buttonCurrentExtents = new System.Windows.Forms.Button();
			this.comboBoxAnimationStartTime = new System.Windows.Forms.ComboBox();
			this.comboBoxAnimationEndTime = new System.Windows.Forms.ComboBox();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPageSingleImage = new System.Windows.Forms.TabPage();
			this.labelTime = new System.Windows.Forms.Label();
			this.buttonClear = new System.Windows.Forms.Button();
			this.checkBoxLoadCache = new System.Windows.Forms.CheckBox();
			this.tabPageAnimation = new System.Windows.Forms.TabPage();
			this.labelSpeed = new System.Windows.Forms.Label();
			this.groupBoxAnimationTimeFrame = new System.Windows.Forms.GroupBox();
			this.checkBoxAnimationCache = new System.Windows.Forms.CheckBox();
			this.buttonStop = new System.Windows.Forms.Button();
			this.buttonStepBack = new System.Windows.Forms.Button();
			this.imageList2 = new System.Windows.Forms.ImageList(this.components);
			this.buttonStepForward = new System.Windows.Forms.Button();
			this.labelFps = new System.Windows.Forms.Label();
			this.panelLower = new System.Windows.Forms.Panel();
			this.panelContents.SuspendLayout();
			this.groupBoxLayerOptions.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownTransparency)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarSpeed)).BeginInit();
			this.panelStatus.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownNorth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownEast)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownWest)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownSouth)).BeginInit();
			this.groupBoxExtents.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabPageSingleImage.SuspendLayout();
			this.tabPageAnimation.SuspendLayout();
			this.groupBoxAnimationTimeFrame.SuspendLayout();
			this.panelLower.SuspendLayout();
			this.SuspendLayout();
			// 
			// richTextBoxStyleDescription
			// 
			this.richTextBoxStyleDescription.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.richTextBoxStyleDescription.Location = new System.Drawing.Point(0, 144);
			this.richTextBoxStyleDescription.Name = "richTextBoxStyleDescription";
			this.richTextBoxStyleDescription.Size = new System.Drawing.Size(496, 56);
			this.richTextBoxStyleDescription.TabIndex = 1;
			this.richTextBoxStyleDescription.Text = "";
			// 
			// treeViewTableOfContents
			// 
			this.treeViewTableOfContents.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeViewTableOfContents.FullRowSelect = true;
			this.treeViewTableOfContents.HideSelection = false;
			this.treeViewTableOfContents.ImageIndex = -1;
			this.treeViewTableOfContents.Location = new System.Drawing.Point(0, 0);
			this.treeViewTableOfContents.Name = "treeViewTableOfContents";
			this.treeViewTableOfContents.SelectedImageIndex = -1;
			this.treeViewTableOfContents.Size = new System.Drawing.Size(496, 144);
			this.treeViewTableOfContents.TabIndex = 0;
			this.treeViewTableOfContents.DoubleClick += new System.EventHandler(this.treeViewTableOfContents_DoubleClick);
			this.treeViewTableOfContents.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewTableOfContents_AfterSelect);
			// 
			// progressBarStatus
			// 
			this.progressBarStatus.Dock = System.Windows.Forms.DockStyle.Right;
			this.progressBarStatus.Location = new System.Drawing.Point(360, 0);
			this.progressBarStatus.Name = "progressBarStatus";
			this.progressBarStatus.Size = new System.Drawing.Size(136, 30);
			this.progressBarStatus.Step = 1;
			this.progressBarStatus.TabIndex = 1;
			// 
			// buttonLegend
			// 
			this.buttonLegend.Enabled = false;
			this.buttonLegend.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonLegend.Image = ((System.Drawing.Image)(resources.GetObject("buttonLegend.Image")));
			this.buttonLegend.Location = new System.Drawing.Point(78, 65);
			this.buttonLegend.Name = "buttonLegend";
			this.buttonLegend.Size = new System.Drawing.Size(48, 23);
			this.buttonLegend.TabIndex = 7;
			this.buttonLegend.Click += new System.EventHandler(this.buttonLegend_Click);
			// 
			// comboBoxTime
			// 
			this.comboBoxTime.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxTime.Location = new System.Drawing.Point(27, 35);
			this.comboBoxTime.Name = "comboBoxTime";
			this.comboBoxTime.Size = new System.Drawing.Size(149, 21);
			this.comboBoxTime.TabIndex = 1;
			this.comboBoxTime.SelectedIndexChanged += new System.EventHandler(this.comboBoxTime_SelectedIndexChanged);
			// 
			// panelContents
			// 
			this.panelContents.Controls.Add(this.splitter1);
			this.panelContents.Controls.Add(this.treeViewTableOfContents);
			this.panelContents.Controls.Add(this.richTextBoxStyleDescription);
			this.panelContents.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelContents.Location = new System.Drawing.Point(0, 0);
			this.panelContents.Name = "panelContents";
			this.panelContents.Size = new System.Drawing.Size(496, 200);
			this.panelContents.TabIndex = 4;
			// 
			// splitter1
			// 
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter1.Location = new System.Drawing.Point(0, 141);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(496, 3);
			this.splitter1.TabIndex = 2;
			this.splitter1.TabStop = false;
			// 
			// groupBoxLayerOptions
			// 
			this.groupBoxLayerOptions.Controls.Add(this.labelPercent);
			this.groupBoxLayerOptions.Controls.Add(this.labelKM);
			this.groupBoxLayerOptions.Controls.Add(this.labelLegend);
			this.groupBoxLayerOptions.Controls.Add(this.numericUpDownHeight);
			this.groupBoxLayerOptions.Controls.Add(this.numericUpDownTransparency);
			this.groupBoxLayerOptions.Controls.Add(this.labelHeight);
			this.groupBoxLayerOptions.Controls.Add(this.labelTransparency);
			this.groupBoxLayerOptions.Controls.Add(this.buttonLegend);
			this.groupBoxLayerOptions.Location = new System.Drawing.Point(272, 120);
			this.groupBoxLayerOptions.Name = "groupBoxLayerOptions";
			this.groupBoxLayerOptions.Size = new System.Drawing.Size(216, 96);
			this.groupBoxLayerOptions.TabIndex = 2;
			this.groupBoxLayerOptions.TabStop = false;
			this.groupBoxLayerOptions.Text = "Options";
			// 
			// labelPercent
			// 
			this.labelPercent.Location = new System.Drawing.Point(126, 21);
			this.labelPercent.Name = "labelPercent";
			this.labelPercent.Size = new System.Drawing.Size(24, 16);
			this.labelPercent.TabIndex = 2;
			this.labelPercent.Text = "%";
			// 
			// labelKM
			// 
			this.labelKM.Location = new System.Drawing.Point(150, 43);
			this.labelKM.Name = "labelKM";
			this.labelKM.Size = new System.Drawing.Size(24, 16);
			this.labelKM.TabIndex = 5;
			this.labelKM.Text = "m";
			// 
			// labelLegend
			// 
			this.labelLegend.Location = new System.Drawing.Point(30, 70);
			this.labelLegend.Name = "labelLegend";
			this.labelLegend.Size = new System.Drawing.Size(48, 16);
			this.labelLegend.TabIndex = 6;
			this.labelLegend.Text = "&Legend:";
			// 
			// numericUpDownHeight
			// 
			this.numericUpDownHeight.DecimalPlaces = 3;
			this.numericUpDownHeight.Increment = new System.Decimal(new int[] {
																										10,
																										0,
																										0,
																										0});
			this.numericUpDownHeight.Location = new System.Drawing.Point(78, 41);
			this.numericUpDownHeight.Maximum = new System.Decimal(new int[] {
																									 1000,
																									 0,
																									 0,
																									 0});
			this.numericUpDownHeight.Name = "numericUpDownHeight";
			this.numericUpDownHeight.Size = new System.Drawing.Size(72, 20);
			this.numericUpDownHeight.TabIndex = 4;
			this.numericUpDownHeight.Value = new System.Decimal(new int[] {
																								  10,
																								  0,
																								  0,
																								  0});
			this.numericUpDownHeight.ValueChanged += new System.EventHandler(this.numericUpDownHeight_ValueChanged);
			this.numericUpDownHeight.Leave += new System.EventHandler(this.numericUpDownHeight_Leave);
			// 
			// numericUpDownTransparency
			// 
			this.numericUpDownTransparency.Increment = new System.Decimal(new int[] {
																												10,
																												0,
																												0,
																												0});
			this.numericUpDownTransparency.Location = new System.Drawing.Point(78, 17);
			this.numericUpDownTransparency.Name = "numericUpDownTransparency";
			this.numericUpDownTransparency.Size = new System.Drawing.Size(48, 20);
			this.numericUpDownTransparency.TabIndex = 1;
			this.numericUpDownTransparency.Value = new System.Decimal(new int[] {
																										  100,
																										  0,
																										  0,
																										  0});
			this.numericUpDownTransparency.ValueChanged += new System.EventHandler(this.numericUpDownTransparency_ValueChanged);
			// 
			// labelHeight
			// 
			this.labelHeight.Location = new System.Drawing.Point(30, 43);
			this.labelHeight.Name = "labelHeight";
			this.labelHeight.Size = new System.Drawing.Size(44, 16);
			this.labelHeight.TabIndex = 3;
			this.labelHeight.Text = "&Height:";
			// 
			// labelTransparency
			// 
			this.labelTransparency.Location = new System.Drawing.Point(30, 18);
			this.labelTransparency.Name = "labelTransparency";
			this.labelTransparency.Size = new System.Drawing.Size(48, 16);
			this.labelTransparency.TabIndex = 0;
			this.labelTransparency.Text = "&Opacity:";
			// 
			// buttonStillImage
			// 
			this.buttonStillImage.Location = new System.Drawing.Point(30, 105);
			this.buttonStillImage.Name = "buttonStillImage";
			this.buttonStillImage.Size = new System.Drawing.Size(88, 40);
			this.buttonStillImage.TabIndex = 3;
			this.buttonStillImage.Text = "&Still Image";
			this.buttonStillImage.Click += new System.EventHandler(this.buttonLoad_Click);
			// 
			// buttonResetExtents
			// 
			this.buttonResetExtents.Location = new System.Drawing.Point(117, 72);
			this.buttonResetExtents.Name = "buttonResetExtents";
			this.buttonResetExtents.Size = new System.Drawing.Size(60, 23);
			this.buttonResetExtents.TabIndex = 9;
			this.buttonResetExtents.Text = "&Reset";
			this.buttonResetExtents.Click += new System.EventHandler(this.buttonResetExtents_Click);
			// 
			// progressBarAnimation
			// 
			this.progressBarAnimation.Cursor = System.Windows.Forms.Cursors.Hand;
			this.progressBarAnimation.Enabled = false;
			this.progressBarAnimation.Location = new System.Drawing.Point(8, 160);
			this.progressBarAnimation.Name = "progressBarAnimation";
			this.progressBarAnimation.Size = new System.Drawing.Size(232, 16);
			this.progressBarAnimation.TabIndex = 6;
			this.progressBarAnimation.Click += new System.EventHandler(this.progressBarAnimation_Click);
			this.progressBarAnimation.MouseMove += new System.Windows.Forms.MouseEventHandler(this.progressBarAnimation_MouseMove);
			// 
			// trackBarSpeed
			// 
			this.trackBarSpeed.Cursor = System.Windows.Forms.Cursors.Hand;
			this.trackBarSpeed.Enabled = false;
			this.trackBarSpeed.LargeChange = 10;
			this.trackBarSpeed.Location = new System.Drawing.Point(200, 24);
			this.trackBarSpeed.Maximum = 100;
			this.trackBarSpeed.Name = "trackBarSpeed";
			this.trackBarSpeed.Orientation = System.Windows.Forms.Orientation.Vertical;
			this.trackBarSpeed.Size = new System.Drawing.Size(40, 96);
			this.trackBarSpeed.TabIndex = 1;
			this.trackBarSpeed.TickFrequency = 10;
			this.trackBarSpeed.TickStyle = System.Windows.Forms.TickStyle.Both;
			this.trackBarSpeed.Value = 60;
			this.trackBarSpeed.Scroll += new System.EventHandler(this.trackBarSpeed_Scroll);
			// 
			// buttonPlay
			// 
			this.buttonPlay.Enabled = false;
			this.buttonPlay.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonPlay.ImageIndex = 4;
			this.buttonPlay.ImageList = this.imageList1;
			this.buttonPlay.Location = new System.Drawing.Point(48, 120);
			this.buttonPlay.Name = "buttonPlay";
			this.buttonPlay.Size = new System.Drawing.Size(35, 35);
			this.buttonPlay.TabIndex = 3;
			this.buttonPlay.Click += new System.EventHandler(this.buttonPlay_Click);
			// 
			// imageList1
			// 
			this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imageList1.ImageSize = new System.Drawing.Size(32, 32);
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// statusBarLabel
			// 
			this.statusBarLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.statusBarLabel.Location = new System.Drawing.Point(0, 0);
			this.statusBarLabel.Name = "labelStatus";
			this.statusBarLabel.Size = new System.Drawing.Size(496, 30);
			this.statusBarLabel.TabIndex = 0;
			this.statusBarLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// panelStatus
			// 
			this.panelStatus.Controls.Add(this.progressBarStatus);
			this.panelStatus.Controls.Add(this.statusBarLabel);
			this.panelStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelStatus.Location = new System.Drawing.Point(0, 424);
			this.panelStatus.Name = "panelStatus";
			this.panelStatus.Size = new System.Drawing.Size(496, 30);
			this.panelStatus.TabIndex = 8;
			// 
			// numericUpDownNorth
			// 
			this.numericUpDownNorth.DecimalPlaces = 2;
			this.numericUpDownNorth.Location = new System.Drawing.Point(48, 20);
			this.numericUpDownNorth.Maximum = new System.Decimal(new int[] {
																									90,
																									0,
																									0,
																									0});
			this.numericUpDownNorth.Minimum = new System.Decimal(new int[] {
																									90,
																									0,
																									0,
																									-2147483648});
			this.numericUpDownNorth.Name = "numericUpDownNorth";
			this.numericUpDownNorth.Size = new System.Drawing.Size(56, 20);
			this.numericUpDownNorth.TabIndex = 1;
			this.numericUpDownNorth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericUpDownNorth.Value = new System.Decimal(new int[] {
																								 90,
																								 0,
																								 0,
																								 0});
			// 
			// numericUpDownEast
			// 
			this.numericUpDownEast.DecimalPlaces = 2;
			this.numericUpDownEast.Location = new System.Drawing.Point(146, 44);
			this.numericUpDownEast.Maximum = new System.Decimal(new int[] {
																								  180,
																								  0,
																								  0,
																								  0});
			this.numericUpDownEast.Minimum = new System.Decimal(new int[] {
																								  180,
																								  0,
																								  0,
																								  -2147483648});
			this.numericUpDownEast.Name = "numericUpDownEast";
			this.numericUpDownEast.Size = new System.Drawing.Size(64, 20);
			this.numericUpDownEast.TabIndex = 7;
			this.numericUpDownEast.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericUpDownEast.Value = new System.Decimal(new int[] {
																								180,
																								0,
																								0,
																								0});
			// 
			// numericUpDownWest
			// 
			this.numericUpDownWest.DecimalPlaces = 2;
			this.numericUpDownWest.Location = new System.Drawing.Point(146, 20);
			this.numericUpDownWest.Maximum = new System.Decimal(new int[] {
																								  180,
																								  0,
																								  0,
																								  0});
			this.numericUpDownWest.Minimum = new System.Decimal(new int[] {
																								  180,
																								  0,
																								  0,
																								  -2147483648});
			this.numericUpDownWest.Name = "numericUpDownWest";
			this.numericUpDownWest.Size = new System.Drawing.Size(64, 20);
			this.numericUpDownWest.TabIndex = 3;
			this.numericUpDownWest.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericUpDownWest.Value = new System.Decimal(new int[] {
																								180,
																								0,
																								0,
																								-2147483648});
			// 
			// numericUpDownSouth
			// 
			this.numericUpDownSouth.DecimalPlaces = 2;
			this.numericUpDownSouth.Location = new System.Drawing.Point(48, 44);
			this.numericUpDownSouth.Maximum = new System.Decimal(new int[] {
																									90,
																									0,
																									0,
																									0});
			this.numericUpDownSouth.Minimum = new System.Decimal(new int[] {
																									90,
																									0,
																									0,
																									-2147483648});
			this.numericUpDownSouth.Name = "numericUpDownSouth";
			this.numericUpDownSouth.Size = new System.Drawing.Size(56, 20);
			this.numericUpDownSouth.TabIndex = 5;
			this.numericUpDownSouth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericUpDownSouth.Value = new System.Decimal(new int[] {
																								 90,
																								 0,
																								 0,
																								 -2147483648});
			// 
			// groupBoxExtents
			// 
			this.groupBoxExtents.Controls.Add(this.labelEast);
			this.groupBoxExtents.Controls.Add(this.labelWest);
			this.groupBoxExtents.Controls.Add(this.labelSouth);
			this.groupBoxExtents.Controls.Add(this.labelNorth);
			this.groupBoxExtents.Controls.Add(this.numericUpDownNorth);
			this.groupBoxExtents.Controls.Add(this.numericUpDownSouth);
			this.groupBoxExtents.Controls.Add(this.numericUpDownEast);
			this.groupBoxExtents.Controls.Add(this.numericUpDownWest);
			this.groupBoxExtents.Controls.Add(this.buttonCurrentExtents);
			this.groupBoxExtents.Controls.Add(this.buttonResetExtents);
			this.groupBoxExtents.Enabled = false;
			this.groupBoxExtents.Location = new System.Drawing.Point(272, 8);
			this.groupBoxExtents.Name = "groupBoxExtents";
			this.groupBoxExtents.Size = new System.Drawing.Size(216, 104);
			this.groupBoxExtents.TabIndex = 1;
			this.groupBoxExtents.TabStop = false;
			this.groupBoxExtents.Text = "Lat/Lon Bounds";
			// 
			// labelEast
			// 
			this.labelEast.Location = new System.Drawing.Point(106, 41);
			this.labelEast.Name = "labelEast";
			this.labelEast.Size = new System.Drawing.Size(40, 23);
			this.labelEast.TabIndex = 6;
			this.labelEast.Text = "&East:";
			this.labelEast.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelWest
			// 
			this.labelWest.Location = new System.Drawing.Point(106, 17);
			this.labelWest.Name = "labelWest";
			this.labelWest.Size = new System.Drawing.Size(40, 23);
			this.labelWest.TabIndex = 2;
			this.labelWest.Text = "&West:";
			this.labelWest.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelSouth
			// 
			this.labelSouth.Location = new System.Drawing.Point(8, 41);
			this.labelSouth.Name = "labelSouth";
			this.labelSouth.Size = new System.Drawing.Size(40, 23);
			this.labelSouth.TabIndex = 4;
			this.labelSouth.Text = "So&uth:";
			this.labelSouth.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelNorth
			// 
			this.labelNorth.Location = new System.Drawing.Point(8, 17);
			this.labelNorth.Name = "labelNorth";
			this.labelNorth.Size = new System.Drawing.Size(40, 23);
			this.labelNorth.TabIndex = 0;
			this.labelNorth.Text = "&North:";
			this.labelNorth.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// buttonCurrentExtents
			// 
			this.buttonCurrentExtents.Location = new System.Drawing.Point(48, 72);
			this.buttonCurrentExtents.Name = "buttonCurrentExtents";
			this.buttonCurrentExtents.Size = new System.Drawing.Size(60, 23);
			this.buttonCurrentExtents.TabIndex = 8;
			this.buttonCurrentExtents.Text = "&Auto";
			this.buttonCurrentExtents.Click += new System.EventHandler(this.buttonCurrentExtents_Click);
			// 
			// comboBoxAnimationStartTime
			// 
			this.comboBoxAnimationStartTime.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAnimationStartTime.Location = new System.Drawing.Point(16, 16);
			this.comboBoxAnimationStartTime.Name = "comboBoxAnimationStartTime";
			this.comboBoxAnimationStartTime.Size = new System.Drawing.Size(160, 21);
			this.comboBoxAnimationStartTime.TabIndex = 0;
			// 
			// comboBoxAnimationEndTime
			// 
			this.comboBoxAnimationEndTime.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAnimationEndTime.Location = new System.Drawing.Point(16, 40);
			this.comboBoxAnimationEndTime.Name = "comboBoxAnimationEndTime";
			this.comboBoxAnimationEndTime.Size = new System.Drawing.Size(160, 21);
			this.comboBoxAnimationEndTime.TabIndex = 1;
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPageSingleImage);
			this.tabControl1.Controls.Add(this.tabPageAnimation);
			this.tabControl1.Location = new System.Drawing.Point(8, 8);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(256, 208);
			this.tabControl1.TabIndex = 0;
			this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
			// 
			// tabPageSingleImage
			// 
			this.tabPageSingleImage.Controls.Add(this.labelTime);
			this.tabPageSingleImage.Controls.Add(this.buttonClear);
			this.tabPageSingleImage.Controls.Add(this.checkBoxLoadCache);
			this.tabPageSingleImage.Controls.Add(this.comboBoxTime);
			this.tabPageSingleImage.Controls.Add(this.buttonStillImage);
			this.tabPageSingleImage.Location = new System.Drawing.Point(4, 22);
			this.tabPageSingleImage.Name = "tabPageSingleImage";
			this.tabPageSingleImage.Size = new System.Drawing.Size(248, 182);
			this.tabPageSingleImage.TabIndex = 0;
			this.tabPageSingleImage.Text = "Single Image";
			// 
			// labelTime
			// 
			this.labelTime.Location = new System.Drawing.Point(25, 18);
			this.labelTime.Name = "labelTime";
			this.labelTime.Size = new System.Drawing.Size(48, 16);
			this.labelTime.TabIndex = 0;
			this.labelTime.Text = "&Time:";
			// 
			// buttonClear
			// 
			this.buttonClear.Location = new System.Drawing.Point(134, 105);
			this.buttonClear.Name = "buttonClear";
			this.buttonClear.Size = new System.Drawing.Size(88, 40);
			this.buttonClear.TabIndex = 4;
			this.buttonClear.Text = "&Clear";
			this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
			// 
			// checkBoxLoadCache
			// 
			this.checkBoxLoadCache.Enabled = false;
			this.checkBoxLoadCache.Location = new System.Drawing.Point(34, 59);
			this.checkBoxLoadCache.Name = "checkBoxLoadCache";
			this.checkBoxLoadCache.Size = new System.Drawing.Size(128, 24);
			this.checkBoxLoadCache.TabIndex = 2;
			this.checkBoxLoadCache.Text = "Load Cached Image";
			this.checkBoxLoadCache.CheckedChanged += new System.EventHandler(this.checkBoxLoadCache_CheckedChanged);
			// 
			// tabPageAnimation
			// 
			this.tabPageAnimation.Controls.Add(this.labelSpeed);
			this.tabPageAnimation.Controls.Add(this.groupBoxAnimationTimeFrame);
			this.tabPageAnimation.Controls.Add(this.buttonStop);
			this.tabPageAnimation.Controls.Add(this.buttonStepBack);
			this.tabPageAnimation.Controls.Add(this.buttonStepForward);
			this.tabPageAnimation.Controls.Add(this.labelFps);
			this.tabPageAnimation.Controls.Add(this.buttonPlay);
			this.tabPageAnimation.Controls.Add(this.progressBarAnimation);
			this.tabPageAnimation.Controls.Add(this.trackBarSpeed);
			this.tabPageAnimation.Location = new System.Drawing.Point(4, 22);
			this.tabPageAnimation.Name = "tabPageAnimation";
			this.tabPageAnimation.Size = new System.Drawing.Size(248, 182);
			this.tabPageAnimation.TabIndex = 1;
			this.tabPageAnimation.Text = "Animation";
			// 
			// labelSpeed
			// 
			this.labelSpeed.Location = new System.Drawing.Point(200, 8);
			this.labelSpeed.Name = "labelSpeed";
			this.labelSpeed.Size = new System.Drawing.Size(40, 16);
			this.labelSpeed.TabIndex = 7;
			this.labelSpeed.Text = "Speed";
			// 
			// groupBoxAnimationTimeFrame
			// 
			this.groupBoxAnimationTimeFrame.Controls.Add(this.comboBoxAnimationStartTime);
			this.groupBoxAnimationTimeFrame.Controls.Add(this.comboBoxAnimationEndTime);
			this.groupBoxAnimationTimeFrame.Controls.Add(this.checkBoxAnimationCache);
			this.groupBoxAnimationTimeFrame.Location = new System.Drawing.Point(8, 8);
			this.groupBoxAnimationTimeFrame.Name = "groupBoxAnimationTimeFrame";
			this.groupBoxAnimationTimeFrame.Size = new System.Drawing.Size(184, 96);
			this.groupBoxAnimationTimeFrame.TabIndex = 0;
			this.groupBoxAnimationTimeFrame.TabStop = false;
			this.groupBoxAnimationTimeFrame.Text = "Time Frame";
			// 
			// checkBoxAnimationCache
			// 
			this.checkBoxAnimationCache.Checked = true;
			this.checkBoxAnimationCache.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxAnimationCache.Enabled = false;
			this.checkBoxAnimationCache.Location = new System.Drawing.Point(16, 64);
			this.checkBoxAnimationCache.Name = "checkBoxAnimationCache";
			this.checkBoxAnimationCache.Size = new System.Drawing.Size(96, 24);
			this.checkBoxAnimationCache.TabIndex = 2;
			this.checkBoxAnimationCache.Text = "Use Cache";
			// 
			// buttonStop
			// 
			this.buttonStop.Enabled = false;
			this.buttonStop.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonStop.ImageIndex = 7;
			this.buttonStop.ImageList = this.imageList1;
			this.buttonStop.Location = new System.Drawing.Point(8, 120);
			this.buttonStop.Name = "buttonStop";
			this.buttonStop.Size = new System.Drawing.Size(35, 35);
			this.buttonStop.TabIndex = 2;
			this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
			// 
			// buttonStepBack
			// 
			this.buttonStepBack.Enabled = false;
			this.buttonStepBack.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonStepBack.ImageIndex = 0;
			this.buttonStepBack.ImageList = this.imageList2;
			this.buttonStepBack.Location = new System.Drawing.Point(88, 129);
			this.buttonStepBack.Name = "buttonStepBack";
			this.buttonStepBack.Size = new System.Drawing.Size(26, 26);
			this.buttonStepBack.TabIndex = 4;
			this.buttonStepBack.Click += new System.EventHandler(this.buttonStepBack_Click);
			// 
			// imageList2
			// 
			this.imageList2.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imageList2.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList2.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList2.ImageStream")));
			this.imageList2.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// buttonStepForward
			// 
			this.buttonStepForward.Enabled = false;
			this.buttonStepForward.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonStepForward.ImageIndex = 1;
			this.buttonStepForward.ImageList = this.imageList2;
			this.buttonStepForward.Location = new System.Drawing.Point(116, 129);
			this.buttonStepForward.Name = "buttonStepForward";
			this.buttonStepForward.Size = new System.Drawing.Size(26, 26);
			this.buttonStepForward.TabIndex = 5;
			this.buttonStepForward.Click += new System.EventHandler(this.buttonStepForward_Click);
			// 
			// labelFps
			// 
			this.labelFps.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
			this.labelFps.Location = new System.Drawing.Point(192, 120);
			this.labelFps.Name = "labelFps";
			this.labelFps.Size = new System.Drawing.Size(54, 32);
			this.labelFps.TabIndex = 1;
			this.labelFps.Text = "-";
			this.labelFps.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// panelLower
			// 
			this.panelLower.Controls.Add(this.tabControl1);
			this.panelLower.Controls.Add(this.groupBoxLayerOptions);
			this.panelLower.Controls.Add(this.groupBoxExtents);
			this.panelLower.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelLower.Location = new System.Drawing.Point(0, 200);
			this.panelLower.Name = "panelLower";
			this.panelLower.Size = new System.Drawing.Size(496, 224);
			this.panelLower.TabIndex = 0;
			// 
			// WMSBrowser
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(496, 454);
			this.Controls.Add(this.panelContents);
			this.Controls.Add(this.panelLower);
			this.Controls.Add(this.panelStatus);
			this.KeyPreview = true;
			this.MinimumSize = new System.Drawing.Size(504, 483);
			this.Name = "WMSBrowser";
			this.Text = "Web Mapping Server Browser";
			this.VisibleChanged += new System.EventHandler(this.WMSBrowser_VisibleChanged);
			this.panelContents.ResumeLayout(false);
			this.groupBoxLayerOptions.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownHeight)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownTransparency)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarSpeed)).EndInit();
			this.panelStatus.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownNorth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownEast)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownWest)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownSouth)).EndInit();
			this.groupBoxExtents.ResumeLayout(false);
			this.tabControl1.ResumeLayout(false);
			this.tabPageSingleImage.ResumeLayout(false);
			this.tabPageAnimation.ResumeLayout(false);
			this.groupBoxAnimationTimeFrame.ResumeLayout(false);
			this.panelLower.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private string CacheDirectory
		{
			get
			{
				string dir = Path.Combine( worldWindow.Cache.CacheDirectory, "WMS Browser" );
				Directory.CreateDirectory(dir);
				return dir;
			}
		}

		private wms_server_list.WMS_SERVER_LISTType GetServerList()
		{
			try
			{
				wms_server_list.wms_server_listDoc doc = new wms_server_list.wms_server_listDoc();
				wms_server_list.WMS_SERVER_LISTType root = new wms_server_list.WMS_SERVER_LISTType(
					doc.Load(this.wms_config_filepath));
				if(!root.HasServer())
					throw new ApplicationException("No WMS servers could be found in " + wms_config_filepath + "." );
				return root;
			}
			catch( Exception caught )
			{
				throw new ApplicationException("Error loading WMS server list '" + wms_config_filepath + "'.", caught );
			}
		}

		private void RefreshTableOfContents()
		{
			if(isRefreshing)
				return;

			isRefreshing = true;
			try
			{
				wms_server_list.WMS_SERVER_LISTType root = GetServerList();
				updateCurrentProgressBar(0, root.ServerCount );
				UpdateStatusBar( "Loading WMS servers... Please wait." );

				for(int i = root.ServerMinCount; i < root.ServerCount; i++)
				{
					wms_server_list.WMS_server curServer = root.GetServerAt(i);

					try
					{
						string savePath = Path.Combine( this.CacheDirectory, curServer.Name.Value);
						string xmlPath = savePath + ".xml";
						Directory.CreateDirectory( savePath );
						string url = curServer.ServerUrl.Value + (curServer.ServerUrl.Value.IndexOf("?") > 0 ? "&request=GetCapabilities&service=WMS&version=" + curServer.Version.Value : "?request=GetCapabilities&service=WMS&version=" + curServer.Version.Value);
						using( WebDownload download = new WebDownload(url) )
							download.DownloadFile(xmlPath );

						this.wmsList = new WMSList(curServer.Name.Value, curServer.ServerUrl.Value, xmlPath, curServer.Version.Value);

						if(this.wmsList.Layers != null)
						{
							foreach(MyWMSLayer l in this.wmsList.Layers)
							{
								TreeNode tn = this.getTreeNodeFromWMSLayer(l);

								this.treeViewTableOfContents.BeginInvoke(new UpdateTableTreeDelegate(this.UpdateTableTree), new object[] {tn});
							}
						}
					}
					catch(WebException)
					{
						TreeNode tn = new TreeNode(curServer.Name.Value + " (Connection Error)");
						tn.ForeColor = Color.Red;
						this.treeViewTableOfContents.BeginInvoke(new UpdateTableTreeDelegate(this.UpdateTableTree), new object[] {tn});
					}
					catch(Exception ex)
					{		
						TreeNode tn = new TreeNode(curServer.Name.Value + " " + ex.ToString());
						tn.ForeColor = Color.Red;
						this.treeViewTableOfContents.BeginInvoke(new UpdateTableTreeDelegate(this.UpdateTableTree), new object[] {tn});
					}

					updateCurrentProgressBar(i+1, root.ServerCount);
				}

				// TODO: Invoke
				this.treeViewTableOfContents.AfterSelect += new TreeViewEventHandler(treeViewTableOfContents_AfterSelect);

				this.panelLower.Enabled = true;
				UpdateStatusBar( "" );
				updateCurrentProgressBar(0,1);
				this.isRefreshed = true;
			}
			catch( Exception caught )
			{
				this.worldWindow.Caption = caught.Message;
			}
			finally
			{
				this.isRefreshing = false;
			}
		}

		delegate 
			void UpdateTableTreeDelegate(TreeNode tn);

		private void UpdateTableTree(TreeNode tn)
		{
			this.treeViewTableOfContents.Nodes.Add(tn);
		}

		private void displayFrame(int frameNumber)
		{
			ImageLayerInfo imageLayerInfo = null;
			lock(this.animationFrames.SyncRoot)
			{
				if(this.animationFrames.Count<=0)
					return;
				if(frameNumber<0)
					frameNumber = this.animationFrames.Count+frameNumber;
				if(frameNumber>=this.animationFrames.Count)
					frameNumber = frameNumber % this.animationFrames.Count;
				imageLayerInfo = this.animationFrames[frameNumber] as ImageLayerInfo;

				if(imageLayerInfo == null)
					return;

				if(this.imageLayer == null)
				{
					this.imageLayer = new ImageLayer(this.Text, this.worldWindow.CurrentWorld,
						1000.0f * (float)this.numericUpDownHeight.Value, imageLayerInfo.ImageFilePath, 
						imageLayerInfo.South, imageLayerInfo.North,
						imageLayerInfo.West, imageLayerInfo.East, (float)this.numericUpDownTransparency.Value * 0.01f, null);
					this.imageLayer.RenderPriority = RenderPriority.AtmosphericImages;
					this.imageLayer.IsOn = true;

					this.worldWindow.CurrentWorld.RenderableObjects.Add(this.imageLayer);
				}
				else
				{
					this.imageLayer.UpdateTexture(imageLayerInfo.ImageFilePath);
				}
			}

			this.worldWindow.Caption = imageLayerInfo.Description;
		}

		private TreeNode getTreeNodeFromWMSLayer(MyWMSLayer layer)
		{
			TreeNode tn = new TreeNode(layer.Title);
			tn.Tag = layer;
			if(layer.ChildLayers != null)
			{
				foreach(MyWMSLayer childLayer in layer.ChildLayers)
				{
					tn.Nodes.Add(this.getTreeNodeFromWMSLayer(childLayer));
				}
			}
			else if(layer.Styles != null)
			{
				foreach(WMSLayerStyle curStyle in layer.Styles)
				{
					TreeNode styleNode = new TreeNode(curStyle.title);
					styleNode.Tag = curStyle;
					tn.Nodes.Add(styleNode);
				}
			}

			return tn;
		}

		private MyWMSLayer getCurrentlySelectedWMSLayer()
		{
			if(this.treeViewTableOfContents.SelectedNode == null)
				return null;

			object tag = this.treeViewTableOfContents.SelectedNode.Tag;
			if (tag==null)
				return null;

			if(tag.GetType() == typeof(MyWMSLayer))
				return (MyWMSLayer)this.treeViewTableOfContents.SelectedNode.Tag;
			if(tag.GetType() == typeof(WMSLayerStyle))
				return (MyWMSLayer)this.treeViewTableOfContents.SelectedNode.Parent.Tag;

			return null;
		}

		private WMSLayerStyle getCurrentlySelectedWMSLayerStyle()
		{
			if(this.treeViewTableOfContents.SelectedNode == null)
				return null;

			WMSLayerStyle someStyle = new WMSLayerStyle();
			if(this.treeViewTableOfContents.SelectedNode.Tag.GetType() == someStyle.GetType())
			{
				return (WMSLayerStyle)this.treeViewTableOfContents.SelectedNode.Tag;
			}

			return null;
		}

		private bool isCachedFileBoundsAvailable(WMSDownload cachedDownloadInfo)
		{
			if(cachedDownloadInfo.North == this.numericUpDownNorth.Value && 
				cachedDownloadInfo.South == this.numericUpDownSouth.Value &&
				cachedDownloadInfo.West == this.numericUpDownWest.Value &&
				cachedDownloadInfo.East == this.numericUpDownEast.Value)
				return true;
			else
				return false;
		}

		private void getColorbar(string url)
		{
			if(colorbar == null)
			{
				this.colorbar = new Colorbar(this);
				this.colorbar.Text = "Legend";
			}

			this.colorbar.LoadImage(url);
		}

		public void Reset()
		{
			lock(this.downloadQueue.SyncRoot)
				this.downloadQueue.Clear();

			StopDownloadThread();

			this.animationTimer.Stop();

			lock(this.animationFrames.SyncRoot)
			{
				this.animationFrames.Clear();
			}

			if(this.imageLayer != null)
			{
				this.worldWindow.CurrentWorld.RenderableObjects.Remove(this.imageLayer.Name);
				this.worldWindow.Invalidate();
				this.imageLayer.Dispose();
				this.imageLayer = null;
			}

			UpdateStatusBar( "" );

			if(this.colorbar != null) 
			{
				this.colorbar.Visible = false;
				this.colorbar.Dispose();
				this.colorbar = null;
			}

			bool isAnim = false;
			MyWMSLayer curLayer = this.getCurrentlySelectedWMSLayer();
			if(curLayer != null)
			{
				if(curLayer.Width != 0 || curLayer.Height != 0)
					this.groupBoxExtents.Enabled = false;
				else
					this.groupBoxExtents.Enabled = true;

				isAnim = curLayer.Dates != null;
			}

			foreach(Control cntrl in this.tabPageAnimation.Controls)
				cntrl.Enabled = isAnim;

			this.animationState = AnimationState.Stop;
			this.buttonPlay.ImageIndex = 4;
			this.worldWindow.Caption = "";
		}


		/// <summary>
		/// Sets the animation speed to match the trackbar's setting.
		/// </summary>
		public void UpdateAnimationSpeed()
		{
			float percent = (float)this.trackBarSpeed.Value / this.trackBarSpeed.Maximum;
			this.animationTimer.Interval = 10*Math.Pow(10,3*(1.0f - percent));
			this.labelFps.Text = string.Format(CultureInfo.CurrentCulture, "{0:f1}\nFPS", 1000.0/animationTimer.Interval );
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			if(this.Visible)
				StartDownloadThread();

			base.OnVisibleChanged (e);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			e.Cancel = true;
			StopDownloadThread();
			this.Reset();
			this.Hide();
			this.worldWindow.Focus();
			base.OnClosing(e);
		}

		protected override void OnKeyUp(System.Windows.Forms.KeyEventArgs e) 
		{
			switch(e.KeyCode) 
			{
				case Keys.L:
					if(e.Modifiers==Keys.Alt)
					{
						buttonLegend_Click(this,e);
						e.Handled = true;
					}
					break;
				case Keys.Escape:
					Close();
					e.Handled = true;
					break;
				case Keys.F4:
					if(e.Modifiers==Keys.Control)
					{
						Close();
						e.Handled = true;
					}
					break;
			}

			base.OnKeyUp(e);
		}


		#region Callbacks

		/// <summary>
		/// Updates progress bar (thread safe)
		/// </summary>
		private void updateCurrentProgressBar(int bytesSoFar, int bytesTotal) 
		{
			// Make sure we're on the right thread
			if( this.InvokeRequired ) 
			{
				// Update progress asynchronously
				DownloadProgressHandler dlgt = new DownloadProgressHandler(updateCurrentProgressBar);
				this.BeginInvoke(dlgt, new object[] { bytesSoFar, bytesTotal });
				return;
			}

			if(bytesSoFar < 0)
				bytesSoFar = 0;
			if(bytesTotal < 0)
				bytesTotal = 0;
			this.progressBarStatus.Maximum = bytesTotal;
			this.progressBarStatus.Value = bytesSoFar<=bytesTotal ? bytesSoFar : bytesTotal;
		}

		#endregion

		private void treeViewTableOfContents_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			if(e.Node.Tag != null)
			{
				this.comboBoxTime.Text = "";
				this.richTextBoxStyleDescription.Text = "";
				MyWMSLayer curLayer = this.getCurrentlySelectedWMSLayer();
				WMSLayerStyle curStyle = this.getCurrentlySelectedWMSLayerStyle();

				if(this.animationState != AnimationState.Stop)
				{
					if(this.curLoadedLayer != curLayer ||
						this.curLoadedStyle != curStyle)
					{
						this.buttonPlay.ImageIndex = 4;
						this.groupBoxAnimationTimeFrame.Enabled = true;
					}
					else if(this.curLoadedLayer == curLayer && this.curLoadedStyle == curStyle)
					{
						this.groupBoxAnimationTimeFrame.Enabled = false;

						if(this.animationState == AnimationState.Pause)
							this.buttonPlay.ImageIndex = 2;
						else
							this.buttonPlay.ImageIndex = 4;
					}
				}
				
				if(curStyle != null)
				{
					this.richTextBoxStyleDescription.Text = curStyle.description;
				}
				else
				{
					this.richTextBoxStyleDescription.Text = curLayer.Description;
				}

				this.comboBoxTime.Items.Clear();
				this.comboBoxAnimationStartTime.Items.Clear();
				this.comboBoxAnimationEndTime.Items.Clear();
					
				if(curLayer.Dates != null && curLayer.Dates.Length>0)
				{
					foreach(string s in curLayer.Dates)
					{
						this.comboBoxTime.Items.Add(s);
						this.comboBoxAnimationEndTime.Items.Add(s);
						this.comboBoxAnimationStartTime.Items.Add(s);
					}

					if(this.comboBoxAnimationStartTime.Items.Count > 50)
					{
						this.comboBoxAnimationStartTime.SelectedIndex = this.comboBoxAnimationStartTime.Items.Count - 49;
					}
					else
						this.comboBoxAnimationStartTime.SelectedIndex = 0;

					this.comboBoxAnimationEndTime.SelectedIndex = this.comboBoxAnimationEndTime.Items.Count - 1;

					if(curLayer.DefaultDate != null)
					{
						this.comboBoxTime.SelectedIndex = this.comboBoxTime.Items.Count-1;
					}
				}

				foreach(Control cntrl in this.tabPageAnimation.Controls)
					cntrl.Enabled = curLayer.Dates!=null;

				if(curLayer.Width != 0 || curLayer.Height != 0)
					this.groupBoxExtents.Enabled = false;
				else
					this.groupBoxExtents.Enabled = true;

				if(curLayer.West > curLayer.East)
				{
					this.numericUpDownWest.Maximum = curLayer.West;
					this.numericUpDownWest.Minimum = curLayer.East;
					this.numericUpDownWest.Value = curLayer.West;

					this.numericUpDownEast.Maximum = curLayer.West;
					this.numericUpDownEast.Minimum = curLayer.East;
					this.numericUpDownEast.Value = curLayer.East;
				}
				else
				{
					this.numericUpDownWest.Maximum = curLayer.East;
					this.numericUpDownWest.Minimum = curLayer.West;
					this.numericUpDownWest.Value = curLayer.West;

					this.numericUpDownEast.Maximum = curLayer.East;
					this.numericUpDownEast.Minimum = curLayer.West;
					this.numericUpDownEast.Value = curLayer.East;
				}
					
				this.numericUpDownNorth.Maximum = curLayer.North;
				this.numericUpDownNorth.Minimum = curLayer.South;
				this.numericUpDownNorth.Value = curLayer.North;

				this.numericUpDownSouth.Maximum = curLayer.North;
				this.numericUpDownSouth.Minimum = curLayer.South;
				this.numericUpDownSouth.Value = curLayer.South;
				
				float targetViewRange = (float)(4 * (this.numericUpDownNorth.Value - this.numericUpDownSouth.Value > 
					this.numericUpDownEast.Value - this.numericUpDownWest.Value ? this.numericUpDownNorth.Value - this.numericUpDownSouth.Value : this.numericUpDownEast.Value - this.numericUpDownWest.Value));
				if(targetViewRange>1e-8)
				{
					double lat = 0.5 * (double)(this.numericUpDownNorth.Value + this.numericUpDownSouth.Value);
					double lon = 0.5 * (double)(this.numericUpDownWest.Value + this.numericUpDownEast.Value);

					if(numericUpDownWest.Value > numericUpDownEast.Value)
						lon += 180;

					if(targetViewRange>120)
					{
						if(numericUpDownNorth.Value>89 && numericUpDownSouth.Value>=0)
							lat = 90;
						else if(numericUpDownSouth.Value<-89 && numericUpDownNorth.Value<=0)
							lat = -90;
					}

					this.worldWindow.GotoLatLon( lat, lon, 
						0,
						double.NaN, 
						(targetViewRange > 180.0f ? 180.0f : targetViewRange),
						0);
				}

				string cacheFilepath = GetCurrentCacheFilePath(curLayer, curStyle );
				this.checkBoxLoadCache.Enabled = File.Exists(cacheFilepath);
			}
			else
			{
				this.richTextBoxStyleDescription.Text = "";
			}
		}

		private void WMSBrowser_VisibleChanged(object sender, System.EventArgs e)
		{
			if(this.Visible)
			{
				if (!isRefreshed)
				{
					Thread refreshThread = new Thread(new ThreadStart(this.RefreshTableOfContents));
					refreshThread.Name = "WMSBrowser.RefreshTableOfContents";
					refreshThread.IsBackground = true;
					refreshThread.Start();
				}

				if(this.Visible)
					StartDownloadThread();
			}
		}

		private void Downloader()
		{
			WMSDownload wmsDownload = null;
			while(true)
			{
				try
				{
					lock(this.downloadQueue)
					{
						if(this.downloadQueue.Count <= 0)
							// Queue empty
							return;
						wmsDownload = (WMSDownload)this.downloadQueue.Dequeue();
					}

					if(this.checkBoxAnimationCache.Checked && 
						File.Exists(wmsDownload.SavedFilePath + ".wwi") &&
						File.Exists(wmsDownload.SavedFilePath + ".dds"))
					{
						try
						{
							ImageLayerInfo imageLayerInfo = ImageLayerInfo.FromFile(wmsDownload.SavedFilePath + ".wwi");
							const float epsilon = Single.Epsilon*100;
							if(Math.Abs(imageLayerInfo.North - (float)wmsDownload.North)<epsilon && 
								Math.Abs(imageLayerInfo.South - (float)wmsDownload.South)<epsilon &&
								Math.Abs(imageLayerInfo.West - (float)wmsDownload.West)<epsilon &&
								Math.Abs(imageLayerInfo.East - (float)wmsDownload.East)<epsilon)
							{
								lock(this.animationFrames.SyncRoot)
								{
									animationFrames.Add(imageLayerInfo);
									// Process next queue item
									continue;
								}
							}
						}
							// Cached file not readable for some reason - reload
						catch(IOException) {}
						catch(FormatException) {}
					}

					UpdateStatusBar( "Downloading: " + Path.GetFileName(wmsDownload.SavedFilePath) );
					using( WebDownload dl = new WebDownload(wmsDownload.Url) )
					{
						dl.ProgressCallback += new DownloadProgressHandler(this.updateCurrentProgressBar);
						dl.DownloadMemory(DownloadType.Wms);
						if(this.animationState == AnimationState.Cancel || this.animationState == AnimationState.Stop)
							return;

						dl.Verify();
						ProcessDownloadedImage(wmsDownload, dl.ContentStream);
					}
				}
				catch(WebException caught)
				{
					this.worldWindow.Caption = caught.Message;
				}
				finally
				{
					updateCurrentProgressBar(0,1);
				}
			}
		}


		delegate
			void UpdateStatusBarDelegate( string statusMsg );

		/// <summary>
		/// Displays a message in the status bar (thread safe)
		/// </summary>
		private void UpdateStatusBar( string statusMsg ) 
		{
			// Make sure we're on the right thread
			if( InvokeRequired ) 
			{
				// run asynchronously
				UpdateStatusBarDelegate updateDelegate =
					new UpdateStatusBarDelegate(UpdateStatusBar);
				BeginInvoke(updateDelegate, new object[] { statusMsg });
				return;
			}

			statusBarLabel.Text = statusMsg;
		}
		/// <summary>
		/// Convert if requested and prepare downloaded data for display
		/// </summary>
		private void ProcessDownloadedImage(WMSDownload wdl, Stream dataStream)
		{
			try
			{
				string ddsFile = wdl.SavedFilePath + ".dds";
				if( wdl.Date.Length>0)
					UpdateStatusBar( "Converting " + wdl.Date );
				else
					UpdateStatusBar( "Converting... " );
				Directory.CreateDirectory(Path.GetDirectoryName(wdl.SavedFilePath));
				if (dataStream.Length==0)
					throw new WebException("Server returned no data.");
				ImageHelper.ConvertToDxt3(dataStream, ddsFile );
				if(this.animationState == AnimationState.Cancel || animationState == AnimationState.Stop)
					return;
		
				ImageLayerInfo imageLayerInfo = new ImageLayerInfo(wdl);
				imageLayerInfo.Save(wdl.SavedFilePath + ".wwi");
				lock(this.animationFrames.SyncRoot)
					animationFrames.Add(imageLayerInfo);
				
				if(this.animationState == AnimationState.Cancel || this.animationState == AnimationState.Stop)
					return;

				UpdateStatusBar( "" );
			}
			catch(Exception caught)
			{
				this.worldWindow.Caption = caught.Message;
				Log.Write( caught );
			}
		}

		private void buttonLegend_Click(object sender, System.EventArgs e)
		{
			if (colorbar!=null && colorbar.Visible)
			{
				colorbar.Hide();
				return;
			}

			if(this.curLoadedLayer == null)
				return;

			if(this.curLoadedLayer.Styles == null || this.curLoadedLayer.Styles.Length == 0)
				return;

			ArrayList possibleLegendUrls = new ArrayList();
			if(this.curLoadedStyle == null)
			{
				foreach(WMSLayerStyle curStyle in this.curLoadedLayer.Styles)
				{
					if(curStyle.legendURL != null && curStyle.legendURL.Length > 0)
					{
						foreach(WMSLayerStyleLegendURL curStyleUrl in curStyle.legendURL)
						{
							possibleLegendUrls.Add(curStyleUrl);
						}
					}
				}
			}
			else
			{
				foreach(WMSLayerStyleLegendURL curStyleUrl in this.curLoadedStyle.legendURL)
				{
					possibleLegendUrls.Add(curStyleUrl);
				}
			}

			string[] extensions = new string[] { ".PNG", ".JPG", ".JPEG", ".GIF", ".TIF", ".BMP" };
			foreach (string ext in extensions)
			{
				foreach (WMSLayerStyleLegendURL curUrl in possibleLegendUrls)
				{
					if (curUrl.href == null)
						continue;
					if (curUrl.href.ToUpper(CultureInfo.InvariantCulture).EndsWith(ext))
					{
						this.getColorbar(curUrl.href);
						return;
					}
				}
			}
		}

		private void buttonLoad_Click(object sender, System.EventArgs e)
		{
			MyWMSLayer curLayer = this.getCurrentlySelectedWMSLayer();
			if (curLayer==null || curLayer.Name == null)
				return;

			this.Reset();

			this.buttonLegend.Enabled = curLayer.HasLegend;
			string dateString = "";
			if(this.comboBoxTime.SelectedItem != null)
				dateString = (string)this.comboBoxTime.SelectedItem;

			WMSLayerStyle curStyle = this.getCurrentlySelectedWMSLayerStyle();

			WMSDownload wmsDownload = curLayer.GetWmsRequest( 
				dateString,
				curStyle,
				this.numericUpDownNorth.Value,
				this.numericUpDownSouth.Value,
				this.numericUpDownWest.Value,
				this.numericUpDownEast.Value,
				this.CacheDirectory);

			if(this.checkBoxLoadCache.Enabled && 
				this.checkBoxLoadCache.Checked &&
				File.Exists(wmsDownload.SavedFilePath + ".wwi") && 
				File.Exists(wmsDownload.SavedFilePath + ".dds"))
			{
				ImageLayerInfo imageLayerInfo = ImageLayerInfo.FromFile(wmsDownload.SavedFilePath + ".wwi");
				lock(this.animationFrames.SyncRoot)
				{
					this.animationFrames.Add(imageLayerInfo);
				}
			}
			else
			{
				EnqueueDownload(wmsDownload);
			}

			this.curLoadedLayer = curLayer;
			this.curLoadedStyle = curStyle;

			this.animationState = AnimationState.Single;
			this.animationTimer.Start();
		}

		private void buttonCurrentExtents_Click(object sender, System.EventArgs e)
		{
			double curNorth = (this.worldWindow.DrawArgs.WorldCamera.Latitude + 0.25f * this.worldWindow.DrawArgs.WorldCamera.ViewRange).Degrees;
			double curSouth = (this.worldWindow.DrawArgs.WorldCamera.Latitude - 0.25f * this.worldWindow.DrawArgs.WorldCamera.ViewRange).Degrees;
			double curWest = (this.worldWindow.DrawArgs.WorldCamera.Longitude - 0.25f * this.worldWindow.DrawArgs.WorldCamera.ViewRange).Degrees;
			double curEast = (this.worldWindow.DrawArgs.WorldCamera.Longitude + 0.25f * this.worldWindow.DrawArgs.WorldCamera.ViewRange).Degrees;

			if(curNorth > (double)this.numericUpDownNorth.Maximum)
				this.numericUpDownNorth.Value = this.numericUpDownNorth.Maximum;
			else if(curNorth < (double)this.numericUpDownNorth.Minimum)
				this.numericUpDownNorth.Value = this.numericUpDownNorth.Minimum;
			else
				this.numericUpDownNorth.Value = (decimal)curNorth;

			if(curSouth < (double)this.numericUpDownSouth.Minimum)
				this.numericUpDownSouth.Value = this.numericUpDownSouth.Minimum;
			else if(curSouth > (double)this.numericUpDownSouth.Maximum)
				this.numericUpDownSouth.Value = this.numericUpDownSouth.Maximum;
			else
				this.numericUpDownSouth.Value = (decimal)curSouth;

			if(curWest < (double)this.numericUpDownWest.Minimum)
				this.numericUpDownWest.Value = this.numericUpDownWest.Minimum;
			else if(curWest > (double)this.numericUpDownWest.Maximum)
				this.numericUpDownWest.Value = this.numericUpDownWest.Maximum;
			else
				this.numericUpDownWest.Value = (decimal)curWest;

			if(curEast < (double)this.numericUpDownEast.Minimum)
				this.numericUpDownEast.Value = this.numericUpDownEast.Minimum;
			else if(curEast > (double)this.numericUpDownEast.Maximum)
				this.numericUpDownEast.Value = this.numericUpDownEast.Maximum;
			else
				this.numericUpDownEast.Value = (decimal)curEast;
		}

		private void buttonResetExtents_Click(object sender, System.EventArgs e)
		{
			this.numericUpDownEast.Value = this.numericUpDownEast.Maximum;
			this.numericUpDownNorth.Value = this.numericUpDownNorth.Maximum;
			this.numericUpDownSouth.Value = this.numericUpDownSouth.Minimum;
			this.numericUpDownWest.Value = this.numericUpDownWest.Minimum;
		}

		private void progressBarAnimation_Click(object sender, System.EventArgs e)
		{
			if (this.animationFrames.Count<=0)
				return;

			float percent = (float)this.mouseLocationProgressBarAnimation.X / this.progressBarAnimation.Size.Width;

			this.animationState = AnimationState.Pause;

			int requestedFrame = (int)(percent * this.progressBarAnimation.Maximum);
			lock(this.animationFrames.SyncRoot)
			{
				if(requestedFrame > this.animationFrames.Count-1)
					requestedFrame = this.animationFrames.Count - 1;
			}

			this.currentFrame = requestedFrame;

			this.progressBarAnimation.Value = this.currentFrame;
		
			this.displayFrame(this.currentFrame);
			this.worldWindow.Invalidate();
		}

		private void progressBarAnimation_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			this.mouseLocationProgressBarAnimation.X = e.X;
		}

		private void checkBoxLoadCache_CheckedChanged(object sender, System.EventArgs e)
		{
			this.groupBoxExtents.Enabled = true;
			if(!this.checkBoxLoadCache.Checked)
				return;

			MyWMSLayer curSelectedLayer = this.getCurrentlySelectedWMSLayer();
			if(curSelectedLayer == null)
			{
				this.checkBoxLoadCache.Enabled = false;
				this.groupBoxExtents.Enabled = true;
				return;
			}
			WMSLayerStyle  curSelectedStyle = this.getCurrentlySelectedWMSLayerStyle();
			string cacheFilepath = GetCurrentCacheFilePath( curSelectedLayer, curSelectedStyle );
			ImageLayerInfo imageLayerInfo = ImageLayerInfo.FromFile(cacheFilepath);
			this.numericUpDownEast.Value = (decimal)imageLayerInfo.East;
			this.numericUpDownNorth.Value = (decimal)imageLayerInfo.North;
			this.numericUpDownSouth.Value = (decimal)imageLayerInfo.South;
			this.numericUpDownWest.Value = (decimal)imageLayerInfo.West;
			this.groupBoxExtents.Enabled = false;
		}

		private void comboBoxTime_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			MyWMSLayer curSelectedLayer = this.getCurrentlySelectedWMSLayer();
			if (curSelectedLayer==null)
				return;
			this.groupBoxExtents.Enabled = true;
			WMSLayerStyle  curSelectedStyle = this.getCurrentlySelectedWMSLayerStyle();
			if(curSelectedLayer.Width != 0 || curSelectedLayer.Height != 0)
				this.groupBoxExtents.Enabled = false;

			string cacheFilepath = GetCurrentCacheFilePath(curSelectedLayer, curSelectedStyle);
			this.checkBoxLoadCache.Enabled = File.Exists(cacheFilepath);
		}

		private string GetCurrentCacheFilePath(MyWMSLayer curLayer, WMSLayerStyle curStyle )
		{
			string parentWmsPath = Path.Combine( this.CacheDirectory, curLayer.ParentWMSList.Name );
			string layerPath = parentWmsPath;
			if(curLayer.Name!=null)
				layerPath = Path.Combine( parentWmsPath, curLayer.Name );
			if(curStyle != null)
				layerPath = Path.Combine( layerPath, curStyle.name );

			string dateString = "Default";
			if (this.comboBoxTime.SelectedIndex > -1)
				dateString = (string) this.comboBoxTime.Items[this.comboBoxTime.SelectedIndex]; 

			string fileName = dateString.Replace(":","");

			string cacheFilePath = Path.Combine( layerPath, fileName+ ".wwi" );
			return cacheFilePath;
		}

		private void animationTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			try
			{
				animationTimer.Stop();
				if(this.animationState != AnimationState.Play && this.animationState != AnimationState.Single)
					return;

				if(this.animationFrames.Count <= 0)
					return;

				if(this.currentFrame >= this.animationFrames.Count)
					this.currentFrame = 0;

				if(this.animationFrames.Count >= 1 || this.imageLayer == null)
				{
					this.displayFrame(this.currentFrame++);
					this.worldWindow.Invalidate();
				}

				int totalFrames = 0;
				lock(this.animationFrames.SyncRoot)
				{
					totalFrames += this.animationFrames.Count;
				}

				lock(this.downloadQueue.SyncRoot)
				{
					totalFrames += this.downloadQueue.Count;
				}
				totalFrames--;
				if (totalFrames<0)
					totalFrames=0;

				this.progressBarAnimation.Value = 0;
				this.progressBarAnimation.Maximum = totalFrames;
				this.progressBarAnimation.Value = this.currentFrame > totalFrames ? totalFrames : this.currentFrame;
			}
			catch (Exception caught)
			{
				Log.Write( caught );
			}
			finally
			{
				if(this.animationState == AnimationState.Play || this.animationState == AnimationState.Single)
					animationTimer.Start();
			}
		}

		private void numericUpDownHeight_ValueChanged(object sender, System.EventArgs e)
		{
			if(this.imageLayer == null)
				return;

			this.imageLayer.UpdateLayerRadius((float)this.worldWindow.CurrentWorld.EquatorialRadius + 1000.0f * (float)this.numericUpDownHeight.Value);
			this.worldWindow.Invalidate();
		}

		private void numericUpDownTransparency_ValueChanged(object sender, System.EventArgs e)
		{
			if(this.imageLayer != null)
			{
				this.imageLayer.UpdateOpacity( 0.01f * (float)this.numericUpDownTransparency.Value);
				this.worldWindow.Invalidate();
			}
		}

		private void buttonPlay_Click(object sender, System.EventArgs e)
		{
			if(this.animationState == AnimationState.Play)
			{
				animationState = AnimationState.Pause;
				buttonPlay.ImageIndex = 4;
				return;
			}

			if(this.animationState == AnimationState.Pause)
			{
				animationState = AnimationState.Play;
				buttonPlay.ImageIndex = 2;
				animationTimer.Start();
				return;
			}

			if(this.animationState == AnimationState.Stop)
			{
				MyWMSLayer curLayer = this.getCurrentlySelectedWMSLayer();
				if(curLayer == null)
					return;

				WMSLayerStyle curStyle = this.getCurrentlySelectedWMSLayerStyle();
				if(curLayer.Dates == null)
					return;

				this.curLoadedLayer = curLayer;
				this.curLoadedStyle = curStyle;
				
				buttonLegend.Enabled = curLayer.HasLegend;

				if(this.comboBoxAnimationStartTime.SelectedIndex == -1 ||
					this.comboBoxAnimationEndTime.SelectedIndex == -1)
					return;

				if(this.comboBoxAnimationStartTime.SelectedIndex > this.comboBoxAnimationEndTime.SelectedIndex)
				{
					// Swap start and end
					int start = comboBoxAnimationEndTime.SelectedIndex;
					comboBoxAnimationEndTime.SelectedIndex = comboBoxAnimationStartTime.SelectedIndex;
					comboBoxAnimationStartTime.SelectedIndex = start;
				}

				if(this.treeViewTableOfContents.SelectedNode.Tag == null) 
					//wms layer not properly selected
					return;

				this.Reset();

				this.groupBoxAnimationTimeFrame.Enabled = false;
				this.groupBoxExtents.Enabled = false;
		
				for(int i = this.comboBoxAnimationStartTime.SelectedIndex; i <= this.comboBoxAnimationEndTime.SelectedIndex; i++)
				{
					WMSDownload wmsDownload = curLayer.GetWmsRequest(
						curLayer.Dates[i],
						curStyle,
						this.numericUpDownNorth.Value,
						this.numericUpDownSouth.Value,
						this.numericUpDownWest.Value,
						this.numericUpDownEast.Value,
						CacheDirectory);

					EnqueueDownload(wmsDownload);
				}

				buttonPlay.ImageIndex = 2;
				animationState = AnimationState.Play;
				animationTimer.Start();
			}
		}

		private void EnqueueDownload(WMSDownload dl)
		{
			this.downloadQueue.Enqueue(dl);
			StartDownloadThread();
		}

		private void buttonStop_Click(object sender, System.EventArgs e)
		{
			this.buttonPlay.ImageIndex = 4;
			this.Reset();
		}

		private void trackBarSpeed_Scroll(object sender, System.EventArgs e)
		{
			UpdateAnimationSpeed();
			if (animationState!=AnimationState.Play)
				buttonPlay_Click(sender,e);
		}

		private void buttonStepBack_Click(object sender, System.EventArgs e)
		{
			if(this.animationState != AnimationState.Pause)
			{
				buttonPlay_Click(sender,e);
				if(this.animationState == AnimationState.Play)
					buttonPlay_Click(sender,e);
			}

			if(this.animationState != AnimationState.Pause)
				return;

			this.currentFrame--;
			if(this.currentFrame < 0)
				this.currentFrame = this.animationFrames.Count - 1;
			this.displayFrame(this.currentFrame);
			this.worldWindow.Invalidate();
		}

		private void buttonStepForward_Click(object sender, System.EventArgs e)
		{
			if(this.animationState != AnimationState.Pause)
			{
				buttonPlay_Click(sender,e);
				if(this.animationState == AnimationState.Play)
					buttonPlay_Click(sender,e);
			}

			if(this.animationState != AnimationState.Pause)
				return;

			this.currentFrame++;
			if(this.currentFrame >= this.animationFrames.Count)
				this.currentFrame = 0;

			this.displayFrame(this.currentFrame);
			this.worldWindow.Invalidate();
		}

		private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			MyWMSLayer curLayer = this.getCurrentlySelectedWMSLayer();
			if(curLayer==null)
				return;

			this.Reset();
			this.comboBoxTime_SelectedIndexChanged(null, null);
		}

		private void buttonClear_Click(object sender, System.EventArgs e)
		{
			this.Reset();
			this.comboBoxTime_SelectedIndexChanged(null, null);
		}

		private void numericUpDownHeight_Leave(object sender, System.EventArgs e)
		{
			if(this.imageLayer != null)
			{
				this.imageLayer.UpdateLayerRadius((float)this.worldWindow.CurrentWorld.EquatorialRadius + 1000.0f * (float)this.numericUpDownHeight.Value);
				this.worldWindow.Invalidate();
			}
		}

		private void treeViewTableOfContents_DoubleClick(object sender, System.EventArgs e)
		{
			buttonStop_Click(this,e);

			// Double-click = show still or play animation depending on active tab
			if (this.tabControl1.SelectedTab==tabPageSingleImage)
				// Still pane visible
				buttonLoad_Click(sender,e);		
			else if(comboBoxTime.Items.Count<=0)
				// Only still available
				buttonLoad_Click(sender,e);		
			else
				// Animation pane active, animation available - play it
				buttonPlay_Click(sender,e);
		}

		enum AnimationState
		{
			Play,
			Stop,
			Pause,
			Single,
			Cancel
		}
	}

	class WMSList
	{
		#region Private Members
		MyWMSLayer[] _layers;
		string _serverGetCapabilitiesUrl;
		string _serverGetMapUrl;
		string _version;
		string _name;
		#endregion

		#region Public Methods
		public WMSList(string name, string serverGetCapabilitiesUrl, string capabilitiesFilePath, string version)
		{
			this._name = name;
			this._serverGetCapabilitiesUrl = serverGetCapabilitiesUrl;
			this._version = version;
			if(version == "1.1.1")
			{
				this.load_version_1_1_1(capabilitiesFilePath);
			}
			else if(version == "1.3.0")
			{
				this.load_version_1_3_0(capabilitiesFilePath);
			}
		}

		public static string[] GetDatesFromDateTimeString(string dateTimeString)
		{
			System.Collections.ArrayList dates = new ArrayList();
			string[] parsedTimeValues = dateTimeString.Split(',');
			foreach(string s in parsedTimeValues)
			{
				if(s.IndexOf("/") < 0)
				{
					dates.Add(s);
					continue;
				}

				try
				{
					string[] dateList = WMSList.GetTimeValuesFromTimePeriodString(s);
					foreach(string curDate in dateList)
						dates.Add(curDate + "Z");
				}
				catch(Exception caught)
				{
					Log.Write( caught );
				}
			}

			string[] returnDates = new string[dates.Count];
			for(int i = 0; i < dates.Count; i++)
			{
				returnDates[i] = (string)dates[i];
			}
			return returnDates;
		}

		public static string[] GetTimeValuesFromTimePeriodString(string timePeriodString)
		{
			string temp = "";
			int numYears = 0;
			int numMonths = 0;
			int numDays = 0;
			double numHours = 0;
			double numMins = 0;
			double numSecs = 0;
			bool isTime = false;

			string[] timePeriodParts = timePeriodString.Split('/');
			if(timePeriodParts.Length != 3)	
				throw new ArgumentException("Unexpected time period string: " + timePeriodString );

			string startDateString = timePeriodParts[0];
			string endDateString = timePeriodParts[1];
			DateTime startDate = WMSList.GetDateTimeFromWMSDate(startDateString);
			DateTime endDate = WMSList.GetDateTimeFromWMSDate(endDateString);
			string interval = timePeriodParts[2];
			for(int i = 0; i < interval.Length; i++)
			{
				if(interval[i] == 'Y')
				{
					numYears = Int32.Parse(temp,CultureInfo.InvariantCulture);
					temp = "";
				}
				else if(interval[i] == 'M' && !isTime)
				{
					numMonths = Int32.Parse(temp,CultureInfo.InvariantCulture);
					temp = "";
				}
				else if(interval[i] == 'D')
				{
					numDays = Int32.Parse(temp,CultureInfo.InvariantCulture);
					temp = "";
				}
				else if(interval[i] == 'H')
				{
					numHours = Double.Parse(temp,CultureInfo.InvariantCulture);
					temp = "";
				}
				else if(interval[i] == 'M' && isTime)
				{
					numMins = Double.Parse(temp,CultureInfo.InvariantCulture);
					temp = "";
				}
				else if(interval[i] == 'S')
				{
					numSecs = Double.Parse(temp,CultureInfo.InvariantCulture);
					temp = "";
				}
				else if(interval[i] == 'T')
				{
					isTime = true;
				}
				else if(interval[i] != 'P')
				{
					temp += interval[i];
				}
			}

			StringCollection dateList = new StringCollection();
			while(startDate <= endDate)
			{
				// Year-Month
				string curDate = string.Format("{0:0000}-", startDate.Year );
				if(startDateString.Length > 5)
					curDate += string.Format( startDate.Month.ToString("00") );
				if(startDateString.Length > 7)
					// Add day
					curDate += "-" + startDate.Day.ToString("00");
				if(startDateString.Length > 10)
				{
					// Add hours+minutes
					curDate += string.Format("T{0:00}:{1:00}",
						startDate.Hour,
						startDate.Minute );
					if(startDateString.Length > 17)
						// Add seconds
						curDate += ":" + startDate.Second.ToString("00");
				}
				dateList.Add(curDate);
				startDate = startDate.AddYears(numYears);
				startDate = startDate.AddMonths(numMonths);
				startDate = startDate.AddDays(numDays);
				startDate = startDate.AddHours(numHours);
				startDate = startDate.AddMinutes(numMins);
				startDate = startDate.AddSeconds(numSecs);
			}

			string[] res = new string[dateList.Count];
			dateList.CopyTo(res,0);
			return res;
		}

		/// <summary>
		/// Parses WMS string dates.
		/// </summary>
		/// <param name="wmsDate">Input WMS date string.</param>
		/// <returns>Time converted to DateTime or DateTime.MinValue if date string is incorrect format.</returns>
		public static DateTime GetDateTimeFromWMSDate(string wmsDate) 
		{
			// result = UTC (not local)
			DateTime result = DateTime.ParseExact( wmsDate,
				new string[] {
											 "yyyy-MM-ddTHH:mm:ssZ",
											 "yyyy-MM-ddTHH:mmZ",
											 "yyyy-MM-ddTHHZ",
											 "yyyy-MM-dd",
											 "yyyy-MM",
											 "yyyy-"},
				null, DateTimeStyles.AdjustToUniversal);
			return result;
		}

		#endregion

		#region Private Methods
		private void load_version_1_1_1(string capabilitiesFilePath)
		{
			capabilities_1_1_1.capabilities_1_1_1Doc doc = new capabilities_1_1_1.capabilities_1_1_1Doc();
			capabilities_1_1_1.WMT_MS_CapabilitiesType root = new capabilities_1_1_1.WMT_MS_CapabilitiesType(doc.Load(capabilitiesFilePath));
			if(!root.HasCapability())
				return;

			if(!root.Capability.HasLayer())
				return;

			string[] imageFormats = null;
			if(root.Capability.HasRequest() &&
				root.Capability.Request.HasGetMap())
			{
				if(root.Capability.Request.GetMap.DCPType.HTTP.Get.HasOnlineResource())
				{
					System.Xml.XmlNode hrefnode = root.Capability.Request.GetMap.DCPType.HTTP.Get.OnlineResource.getDOMNode();
					System.Xml.XmlAttribute attr = hrefnode.Attributes["xlink:href"];
					if(attr != null)
						this._serverGetMapUrl = attr.InnerText;
					else
						this._serverGetMapUrl = this._serverGetCapabilitiesUrl;
				}
				else
					this._serverGetMapUrl = this._serverGetCapabilitiesUrl;

				if(root.Capability.Request.GetMap.HasFormat())
				{
					imageFormats = new string[root.Capability.Request.GetMap.FormatCount];
					for(int i = 0; i < root.Capability.Request.GetMap.FormatCount; i++)
					{
						imageFormats[i] = root.Capability.Request.GetMap.GetFormatAt(i).Value;
					}
				}
			}
			else
				this._serverGetMapUrl = this._serverGetCapabilitiesUrl;

			this.Layers = new MyWMSLayer[root.Capability.LayerCount];

			for(int i = 0; i < root.Capability.LayerCount; i++)
			{
				capabilities_1_1_1.LayerType curLayer = (capabilities_1_1_1.LayerType)root.Capability.GetLayerAt(i);
				this.Layers[i] = this.getWMSLayer(curLayer, null, imageFormats);
			}
		}

		private MyWMSLayer getWMSLayer(capabilities_1_1_1.LayerType layer, capabilities_1_1_1.LatLonBoundingBoxType parentLatLonBoundingBox, string[] imageFormats)
		{
			MyWMSLayer wmsLayer = new MyWMSLayer();
			wmsLayer.ParentWMSList = this;
			wmsLayer.ImageFormats = imageFormats;

			if(layer.HasName())
				wmsLayer.Name = layer.Name.Value;

			if(layer.HasTitle())
				wmsLayer.Title = layer.Title.Value;

			if(layer.HasAbstract2())
				wmsLayer.Description = layer.Abstract2.Value;

			if(layer.HasExtent())
			{
				for(int i = layer.ExtentMinCount; i < layer.ExtentCount; i++)
				{
					capabilities_1_1_1.ExtentType curExtent = layer.GetExtentAt(i);
					if(curExtent.Hasname())
					{
						if(String.Compare(curExtent.name.Value, "time", true) == 0)
						{
							wmsLayer.Dates = WMSList.GetDatesFromDateTimeString(curExtent.getDOMNode().InnerText);

							if(curExtent.Hasdefault2())
							{
								wmsLayer.DefaultDate = curExtent.default2.Value;
							}
						}
					}
				}
			}

			if(layer.HasLatLonBoundingBox())
			{
				wmsLayer.North = Decimal.Parse(layer.LatLonBoundingBox.maxy.Value, CultureInfo.InvariantCulture);
				wmsLayer.South = Decimal.Parse(layer.LatLonBoundingBox.miny.Value, CultureInfo.InvariantCulture);
				wmsLayer.West = Decimal.Parse(layer.LatLonBoundingBox.minx.Value, CultureInfo.InvariantCulture);
				wmsLayer.East = Decimal.Parse(layer.LatLonBoundingBox.maxx.Value, CultureInfo.InvariantCulture);
				parentLatLonBoundingBox = layer.LatLonBoundingBox;
			}
			else if(parentLatLonBoundingBox != null)
			{
				wmsLayer.North = Decimal.Parse(parentLatLonBoundingBox.maxy.Value, CultureInfo.InvariantCulture);
				wmsLayer.South = Decimal.Parse(parentLatLonBoundingBox.miny.Value, CultureInfo.InvariantCulture);
				wmsLayer.West = Decimal.Parse(parentLatLonBoundingBox.minx.Value, CultureInfo.InvariantCulture);
				wmsLayer.East = Decimal.Parse(parentLatLonBoundingBox.maxx.Value, CultureInfo.InvariantCulture);
			}

			if(layer.HasStyle())
			{
				wmsLayer.Styles = new WMSLayerStyle[layer.StyleCount];
				for(int i = 0; i < layer.StyleCount; i++)
				{
					capabilities_1_1_1.StyleType curStyle = layer.GetStyleAt(i);

					wmsLayer.Styles[i] = new WMSLayerStyle();
					
					if(curStyle.HasAbstract2())
						wmsLayer.Styles[i].description = curStyle.Abstract2.Value;
					
					if(curStyle.HasName())
						wmsLayer.Styles[i].name = curStyle.Name.Value;

					if(curStyle.HasTitle())
						wmsLayer.Styles[i].title = curStyle.Title.Value;
					
					if(curStyle.HasLegendURL())
					{
						wmsLayer.Styles[i].legendURL = new WMSLayerStyleLegendURL[curStyle.LegendURLCount];
						
						for(int j = 0; j < curStyle.LegendURLCount; j++)
						{
							capabilities_1_1_1.LegendURLType curLegend = curStyle.GetLegendURLAt(j);

							wmsLayer.Styles[i].legendURL[j] = new WMSLayerStyleLegendURL();
							if(curLegend.HasFormat())
								wmsLayer.Styles[i].legendURL[j].format = curLegend.Format.Value;

							if(curLegend.Haswidth())
								wmsLayer.Styles[i].legendURL[j].width = (int)curLegend.width.IntValue();

							if(curLegend.Hasheight())
								wmsLayer.Styles[i].legendURL[j].height = (int)curLegend.height.IntValue();

							if(curLegend.HasOnlineResource())
							{
								System.Xml.XmlNode n = curLegend.OnlineResource.getDOMNode();
								
								foreach(System.Xml.XmlAttribute attr in n.Attributes)
								{
									if(attr.Name.IndexOf("href") >= 0)
									{
										wmsLayer.Styles[i].legendURL[j].href = attr.InnerText;
									}
								}
							}	
						}
					}
				}
			}

			if(layer.HasLayer())
			{
				wmsLayer.ChildLayers = new MyWMSLayer[layer.LayerCount];
				for(int i = 0; i < layer.LayerCount; i++)
				{
					wmsLayer.ChildLayers[i] = this.getWMSLayer((capabilities_1_1_1.LayerType)layer.GetLayerAt(i), parentLatLonBoundingBox, imageFormats);
				}
			}

			return wmsLayer;
		}

		private void load_version_1_3_0(string capabilitiesFilePath)
		{
			capabilities_1_3_0.capabilities_1_3_0Doc doc = new capabilities_1_3_0.capabilities_1_3_0Doc();
			capabilities_1_3_0.wms.WMS_CapabilitiesType root = new capabilities_1_3_0.wms.WMS_CapabilitiesType(doc.Load(capabilitiesFilePath));

			if(!root.HasCapability())
				return;

			if(!root.Capability.HasLayer())
				return;

			string[] imageFormats = null;
			if(root.Capability.HasRequest() && 
				root.Capability.Request.HasGetMap())
			{
				if(root.Capability.Request.GetMap.HasDCPType())
				{
					this._serverGetMapUrl = root.Capability.Request.GetMap.DCPType.HTTP.Get.OnlineResource.href.Value;
				}
				else
					this._serverGetMapUrl = this._serverGetCapabilitiesUrl;

				if(root.Capability.Request.GetMap.HasFormat())
				{
					imageFormats = new string[root.Capability.Request.GetMap.FormatCount];
					for(int i = 0; i < root.Capability.Request.GetMap.FormatCount; i++)
					{
						imageFormats[i] = root.Capability.Request.GetMap.GetFormatAt(i).Value;
					}
				}
			}

			this.Layers = new MyWMSLayer[root.Capability.LayerCount];

			for(int i = 0; i < root.Capability.LayerCount; i++)
			{
				capabilities_1_3_0.wms.LayerType curLayer = (capabilities_1_3_0.wms.LayerType)root.Capability.GetLayerAt(i);
				this.Layers[i] = this.getWMSLayer(curLayer, null, imageFormats);
			}
		}

		private MyWMSLayer getWMSLayer(capabilities_1_3_0.wms.LayerType layer, capabilities_1_3_0.wms.EX_GeographicBoundingBoxType parentLatLonBoundingBox, string[] imageFormats)
		{
			MyWMSLayer wmsLayer = new MyWMSLayer();

			wmsLayer.ParentWMSList = this;
			wmsLayer.ImageFormats = imageFormats;

			if(layer.HasName())
				wmsLayer.Name = layer.Name.Value;

			if(layer.HasTitle())
				wmsLayer.Title = layer.Title.Value;

			if(layer.HasAbstract2())
				wmsLayer.Description = layer.Abstract2.Value;

			if(layer.HasCRS())
				wmsLayer.CRS = layer.CRS.Value;

			if(layer.HasfixedHeight())
				wmsLayer.Height = (uint)layer.fixedHeight.Value;

			if(layer.HasfixedWidth())
				wmsLayer.Width = (uint)layer.fixedWidth.Value;

			if(layer.HasDimension())
			{
				for(int i = layer.DimensionMinCount; i < layer.DimensionCount; i++)
				{
					capabilities_1_3_0.wms.DimensionType curDimension = layer.GetDimensionAt(i);
					if(curDimension.Hasname())
					{
						if(String.Compare(layer.Dimension.name.Value, "time", true,CultureInfo.InvariantCulture) == 0)	
						{
							wmsLayer.Dates = WMSList.GetDatesFromDateTimeString(curDimension.Value.Value);
							if(curDimension.Hasdefault2())
								wmsLayer.DefaultDate = curDimension.default2.Value;
						}
					}
				}
			}

			if(layer.HasEX_GeographicBoundingBox())
			{
				wmsLayer.North = (decimal)layer.EX_GeographicBoundingBox.northBoundLatitude.Value;
				wmsLayer.South = (decimal)layer.EX_GeographicBoundingBox.southBoundLatitude.Value;
				wmsLayer.East = (decimal)layer.EX_GeographicBoundingBox.eastBoundLongitude.Value;
				wmsLayer.West = (decimal)layer.EX_GeographicBoundingBox.westBoundLongitude.Value;
			}
			else if(parentLatLonBoundingBox != null)
			{
				wmsLayer.North = (decimal)parentLatLonBoundingBox.northBoundLatitude.Value;
				wmsLayer.South = (decimal)parentLatLonBoundingBox.southBoundLatitude.Value;
				wmsLayer.West = (decimal)parentLatLonBoundingBox.westBoundLongitude.Value;
				wmsLayer.East = (decimal)parentLatLonBoundingBox.eastBoundLongitude.Value;
			}

			if(layer.HasStyle())
			{
				wmsLayer.Styles = new WMSLayerStyle[layer.StyleCount];
				for(int i = layer.StyleMinCount; i < layer.StyleCount; i++)
				{
					capabilities_1_3_0.wms.StyleType curStyle = layer.GetStyleAt(i);
					
					wmsLayer.Styles[i] = new WMSLayerStyle();
					if(curStyle.HasAbstract2())
						wmsLayer.Styles[i].description = curStyle.Abstract2.Value;
					
					if(curStyle.HasName())
						wmsLayer.Styles[i].name = curStyle.Name.Value;

					if(curStyle.HasTitle())
						wmsLayer.Styles[i].title = curStyle.Title.Value;
					
					if(curStyle.HasLegendURL())
					{
						wmsLayer.Styles[i].legendURL = new WMSLayerStyleLegendURL[curStyle.LegendURLCount];
						
						for(int j = 0; j < curStyle.LegendURLCount; j++)
						{
							capabilities_1_3_0.wms.LegendURLType curLegend = curStyle.GetLegendURLAt(j);
							wmsLayer.Styles[i].legendURL[j] = new WMSLayerStyleLegendURL();
							if(curLegend.HasFormat())
								wmsLayer.Styles[i].legendURL[j].format = curLegend.Format.Value;

							if(curLegend.Haswidth())
								wmsLayer.Styles[i].legendURL[j].width = (int)curLegend.width.Value;

							if(curLegend.Hasheight())
								wmsLayer.Styles[i].legendURL[j].height = (int)curLegend.height.Value;

							if(curLegend.HasOnlineResource())
							{
								if(curLegend.OnlineResource.Hashref())
									wmsLayer.Styles[i].legendURL[j].href = curLegend.OnlineResource.href.Value;
							}	
						}
					}
				}
			}

			if(layer.HasLayer())
			{
				wmsLayer.ChildLayers = new MyWMSLayer[layer.LayerCount];
				for(int i = 0; i < layer.LayerCount; i++)
				{
					wmsLayer.ChildLayers[i] = this.getWMSLayer((capabilities_1_3_0.wms.LayerType)layer.GetLayerAt(i), parentLatLonBoundingBox, imageFormats);
				}
			}

			return wmsLayer;
		}

		#endregion
				
		#region Properties
		public MyWMSLayer[] Layers
		{
			get
			{
				return this._layers;
			}
			set
			{
				this._layers = value;
			}
		}
		public string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				this._name = value;
			}
		}
		public string ServerGetCapabilitiesUrl
		{
			get
			{
				return this._serverGetCapabilitiesUrl;
			}
			set
			{
				this._serverGetCapabilitiesUrl = value;
			}
		}
		public string ServerGetMapUrl
		{
			get
			{
				return this._serverGetMapUrl;
			}
			set
			{
				this._serverGetMapUrl = value;
			}
		}
		public string Version
		{
			get
			{
				return this._version;
			}
			set
			{
				this._version = value;
			}
		}
		#endregion
	}

	class MyWMSLayer
	{
		#region Private Members
		WMSList _parentWMSList;
		private string[] _imageFormats;
		private decimal _north;
		private decimal _south;
		private decimal _east;
		private decimal _west;
		private string _title;
		private string _description;
		private string _name;
		private uint _width;
		private uint _height;
		private string _crs;
		private string _defaultDate;
		private string[] _dates;
		private MyWMSLayer[] _childLayers;
		private WMSLayerStyle[] _styles;
		#endregion
		
		#region Public Methods
		public MyWMSLayer()
		{
		}

		public bool HasLegend
		{
			get
			{
				if(this._styles == null)
					return false;
				foreach(WMSLayerStyle style in this._styles)
					if(style.legendURL != null && style.legendURL.Length > 0)
						return true;
				return false;
			}
		}

		public WMSDownload GetWmsRequest( string dateString, 
			WMSLayerStyle curStyle,
			decimal north,
			decimal south,
			decimal west,
			decimal east,
			string cacheDirectory)
		{
			string url =	GetWMSRequestUrl(dateString,
				(curStyle != null ? curStyle.name : null),
				north,
				south,
				west,
				east);

			WMSDownload wmsDownload = new WMSDownload( url );
		
			wmsDownload.North = north;
			wmsDownload.South = south;
			wmsDownload.West = west;
			wmsDownload.East =  east;

			//fix widening errors from conversion of float to decimal for upDown controls
			// TODO: Is this widening thing needed anymore?
			if(this._west > this._east)
			{
				if(wmsDownload.West > this._west)
					wmsDownload.West = this._west;
				if(wmsDownload.East < this._east)
					wmsDownload.East = this._east;
			}
			else
			{
				if(wmsDownload.West < this._west)
					wmsDownload.West = this._west;

				if(wmsDownload.East > this._east)
					wmsDownload.East = this._east;
			}

			if(wmsDownload.North > this._north)
				wmsDownload.North = this._north;
			if(wmsDownload.South < this._south)
				wmsDownload.South = this._south;

			wmsDownload.Title = this._title;
			if(curStyle != null)
				wmsDownload.Title += " (" + curStyle.title + ")";
			
			string path = Path.Combine( cacheDirectory , Path.Combine( 
				this._parentWMSList.Name,
				this._name + (curStyle != null ? curStyle.name : "")));	
			if(dateString!=null && dateString.Length>0)
			{
				wmsDownload.SavedFilePath = Path.Combine( path, dateString.Replace(":",""));
				wmsDownload.Title += "\n" + dateString;
			}
			else
				wmsDownload.SavedFilePath = Path.Combine( path, "Default" );	
			return wmsDownload;
		}

		public string GetWMSRequestUrl(string date, string style, decimal north, decimal south, decimal west, decimal east)
		{
			if(this._name == null)
			{
				Log.Write(Log.Levels.Error, "WMSB", "No Name");
				return null;
			}
			string projectionRequest = "";
			if(this.ParentWMSList.Version == "1.1.1")
				projectionRequest = "&srs=EPSG:4326";
			else if(this.ParentWMSList.Version == "1.3.0")
				projectionRequest = "&crs=CRS:84";

			string imageFormat = null;

			if(this._imageFormats == null)
			{
				Log.Write(Log.Levels.Error, "WMSB", "No formats");
				return null;
			}

			foreach(string curFormat in this._imageFormats)
			{
				if(string.Compare(curFormat, "image/png", true, CultureInfo.InvariantCulture) == 0)
				{
					imageFormat = curFormat;
					break;
				}
				if(string.Compare(curFormat, "image/jpeg", true, CultureInfo.InvariantCulture) == 0 || 
					String.Compare(curFormat, "image/jpg", true, CultureInfo.InvariantCulture) == 0)
				{
					imageFormat = curFormat;
				}
			}

			if(imageFormat == null)
				return null;

			uint rWidth = 512;
			if((float)(west - east) > 1.75f * (float)(north - south))
				rWidth = 1024;

			string wmsQuery = string.Format(
				CultureInfo.InvariantCulture, 
				"{0}?service=WMS&version={1}&request=GetMap&layers={2}&format={3}&width={4}&height={5}&time={6}{7}&bbox={8},{9},{10},{11}&styles={12}&transparent=TRUE",
				this.ParentWMSList.ServerGetMapUrl,
				this.ParentWMSList.Version,
				this._name,
				imageFormat,
				(this._width != 0 ? this._width : rWidth),
				(this._height != 0 ? this._height : 512),
				(date != null ? date : ""),
				projectionRequest,
				west, south, east, north,
				(style != null ? style : ""));
	
			return wmsQuery;
		}
		#endregion
		
		#region Properties

		public WMSList ParentWMSList
		{
			get
			{
				return this._parentWMSList;
			}
			set
			{
				this._parentWMSList = value;
			}
		}

		public string[] ImageFormats
		{
			get
			{
				return this._imageFormats;
			}
			set
			{
				this._imageFormats = value;
			}
		}

		public decimal North
		{
			get
			{
				return this._north;
			}
			set
			{
				this._north = value;
			}
		}

		public decimal South
		{
			get
			{
				return this._south;
			}
			set
			{
				this._south = value;
			}
		}

		public decimal West
		{
			get
			{
				return this._west;
			}
			set
			{
				this._west = value;
			}
		}

		public decimal East
		{
			get
			{
				return this._east;
			}
			set
			{
				this._east = value;
			}
		}

		public string CRS
		{
			get
			{
				return this._crs;
			}
			set
			{
				this._crs = value;
			}
		}

		public string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				this._name = value;
			}
		}

		public string Title
		{
			get
			{
				return this._title;
			}
			set
			{
				this._title = value;
			}
		}

		public string Description
		{
			get
			{
				return this._description;
			}
			set
			{
				this._description = value;
			}
		}

		public string DefaultDate
		{
			get
			{
				return this._defaultDate;
			}
			set
			{
				this._defaultDate = value;
			}
		}
		
		public uint Width
		{
			get
			{
				return this._width;
			}
			set
			{
				this._width = value;
			}
		}

		public uint Height
		{
			get
			{
				return this._height;
			}
			set
			{
				this._height = value;
			}
		}

		public string[] Dates
		{
			get
			{
				return this._dates;
			}
			set
			{
				this._dates = value;
			}
		}

		public MyWMSLayer[] ChildLayers
		{
			get
			{
				return this._childLayers;
			}
			set
			{
				this._childLayers = value;
			}
		}

		public WMSLayerStyle[] Styles
		{
			get
			{
				return this._styles;
			}
			set
			{
				this._styles = value;
			}
		}

		#endregion
	}

	class WMSLayerStyle
	{
		public string description;
		public string title;
		public string name;
		public WMSLayerStyleLegendURL[] legendURL;

		public override string ToString()
		{
			return this.title;
		}
	}

	class WMSLayerStyleLegendURL
	{
		public string format;
		public string href;
		public int width;
		public int height;

		public override string ToString()
		{
			return this.href;
		}
	}

	class ImageLayerInfo
	{
		#region Private Members
		private float _north;
		private float _south;
		private float _west;
		private float _east;
		private string _imageFilePath;
		private string _id;
		private string _description;
		#endregion

		#region Public Methods
		public ImageLayerInfo()
		{
		}

		public ImageLayerInfo( WMSDownload dl )
		{
			this.Id = dl.Name + "-" + Path.GetFileName(dl.SavedFilePath);
			this.Description = dl.Title;
			this.ImageFilePath = dl.SavedFilePath + ".dds";
			this.North = (float)dl.North;
			this.South = (float)dl.South;
			this.West = (float)dl.West;
			this.East = (float)dl.East;
		}

		public static ImageLayerInfo FromFile(string filePath)
		{
			using( FileStream stream = File.OpenRead(filePath) )
			using( BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Unicode ) )
			{
				ImageLayerInfo imageLayerInfo = new ImageLayerInfo();
				imageLayerInfo.Id = reader.ReadString();
				string imageFileName = Path.GetFileName( reader.ReadString() );
				imageLayerInfo.ImageFilePath = Path.Combine( Path.GetDirectoryName( filePath ), imageFileName );
				if(!File.Exists(imageLayerInfo.ImageFilePath))
					throw new IOException("Cached image '" + imageLayerInfo.ImageFilePath + "' not found.");
				imageLayerInfo.Description = reader.ReadString();
				imageLayerInfo.South = reader.ReadSingle();
				imageLayerInfo.West = reader.ReadSingle();
				imageLayerInfo.North = reader.ReadSingle();
				imageLayerInfo.East = reader.ReadSingle();
				return imageLayerInfo;
			}
		}

		public void Save(string filepath)
		{
			using( FileStream stream = File.Open(filepath, FileMode.Create) )
			using( BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.Unicode ))
			{
				writer.Write(this.Id);
				writer.Write(Path.GetFileName(this.ImageFilePath));
				writer.Write(this.Description);
				writer.Write(this.South);
				writer.Write(this.West);
				writer.Write(this.North);
				writer.Write(this.East);
			}
		}
		#endregion

		#region Properties
		public float North
		{
			get
			{
				return this._north;
			}
			set
			{
				this._north = value;
			}
		}

		public float South
		{
			get
			{
				return this._south;
			}
			set
			{
				this._south = value;
			}
		}

		public float East
		{
			get
			{
				return this._east;
			}
			set
			{
				this._east = value;
			}
		}

		public float West
		{
			get
			{
				return this._west;
			}
			set
			{
				this._west = value;
			}
		}

		public string Id
		{
			get
			{
				return this._id;
			}
			set
			{
				this._id = value;
			}
		}

		public string Description
		{
			get
			{
				return this._description;
			}
			set
			{
				this._description = value;
			}
		}

		public string ImageFilePath
		{
			get
			{
				return this._imageFilePath;
			}
			set
			{
				this._imageFilePath = value;
			}
		}
		#endregion
	}
}
