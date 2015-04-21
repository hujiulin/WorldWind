using System;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.XPath;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Globalization;
using System.Web;
using WorldWind.Renderable;
using WorldWind.Net;
using System.Text.RegularExpressions;
using System.IO.Compression;


namespace WorldWind
{
	/// <summary>
	/// Summary description for PlaceFinder.
	/// </summary>
	public class PlaceFinder : System.Windows.Forms.Form
	{
		private string lastSearchTerm = null;

		private System.Windows.Forms.GroupBox groupBoxResults;
		private System.Windows.Forms.GroupBox groupBoxYahooSimple;
		private System.Windows.Forms.GroupBox groupBoxYahooDetailed;
		private System.Windows.Forms.GroupBox groupBoxWWSearch;
		private System.Windows.Forms.TextBox textBoxYahooSimple;
		private System.Windows.Forms.TextBox textBoxYahooStreet;
		private System.Windows.Forms.TextBox textBoxYahooCity;
		private System.Windows.Forms.TextBox textBoxYahooState;
		private System.Windows.Forms.TextBox textBoxYahooZip;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button buttonYahooSimpleSearch;
		private System.Windows.Forms.Button buttonYahooDetailedSearch;
		private System.Windows.Forms.Button buttonWWSearch;
		private System.Windows.Forms.TextBox textBoxLatitude;
		private System.Windows.Forms.TextBox textBoxLongitude;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button buttonGo;
		private System.Windows.Forms.ListView listViewResults;
		private System.Windows.Forms.StatusBar statusBar;

		const string YahooUri = "http://api.local.yahoo.com/MapsService/V1/geocode";
		//const string YahooUri = "http://www.alteviltech.com/temp/gazettequery.php";
		const string YahooAppId = "nasaworldwind";
		WorldWind.WorldWindow m_WorldWindow = null;
		private EnterTextBox textBoxWWFeature;
		//private System.Windows.Forms.TextBox textBoxWWFeature;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.NumericUpDown numericUpDownAltitude;
		private System.Windows.Forms.TabPage tabPageYahoo;
		private System.Windows.Forms.TabPage tabPageWWFeature;
		private System.Windows.Forms.PictureBox pictureBoxYahoo;
		private Label label7;
		private Label label6;
		private Label label5;
        private PictureBox pictureBoxWW;
        private TabPage tabPage1;
        private PictureBox pictureBox2;
        private Label label8;
        private Label label9;
        private Label label10;
        private GroupBox groupBox2;
        private Button button1;
        private Label label11;
        private EnterTextBox enterTextBox1;
        private TabPage tabPage2;
        private PictureBox pictureBox3;
        private GroupBox groupBox3;
        private Button button2;
        private TextBox textBox1;
        private GroupBox groupBox4;
        private Button button3;
        private TextBox textBox2;
        private TextBox textBox3;
        private TextBox textBox4;
        private TextBox textBox5;
        private TabPage tabPage3;
        private PictureBox pictureBox4;
        private GroupBox groupBox5;
        private TextBox textBox6;
        private Button button4;
        private TextBox textBox7;
        private TextBox textBox8;
        private TextBox textBox9;
        private TextBox textBox10;
        private TabPage tabPage4;
        private PictureBox pictureBox5;
        private Label label12;
        private Label label13;
        private Label label14;
        private GroupBox groupBox6;
        private Button button5;
        private Label label15;
        private EnterTextBox enterTextBox2;
        private TabPage tabPage5;
        private PictureBox pictureBox6;
        private GroupBox groupBox7;
        private Button button6;
        private TextBox textBox11;
        private GroupBox groupBox8;
        private Button button7;
        private TextBox textBox12;
        private TextBox textBox13;
        private TextBox textBox14;
        private TextBox textBox15;
        private TabPage tabPage6;
        private PictureBox pictureBox7;
        private GroupBox groupBox9;
        private TextBox textBox16;
        private Button button8;
        private TextBox textBox17;
        private TextBox textBox18;
        private TextBox textBox19;
        private TextBox textBox20;
        private TabPage tabPageVE;
        private Button vesearch;
        private EnterTextBox VEenterTextBox;
        private PictureBox pictureBoxVE;
		private Label label16;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public PlaceFinder(WorldWind.WorldWindow ww)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			try
			{
				Image yahooImage = Image.FromFile(
					Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Plugins\\PlaceFinder\\yahoowebservices.gif"));
				
				pictureBoxYahoo.Size = new Size(yahooImage.Width, yahooImage.Height);

				pictureBoxYahoo.Image = yahooImage;

				Image wwImage = Image.FromFile(
					Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Plugins\\PlaceFinder\\fef2.png"));

				pictureBoxWW.Size = new Size(wwImage.Width, wwImage.Height);

				pictureBoxWW.Image = wwImage;

                Image veImage = Image.FromFile(
                    Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Plugins\\PlaceFinder\\vejewel.png"));

                pictureBoxVE.Size = new Size(veImage.Width, veImage.Height);

                pictureBoxVE.Image = veImage;
			}
			catch{}

			m_WorldWindow = ww;
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
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PlaceFinder));
			this.listViewResults = new System.Windows.Forms.ListView();
			this.groupBoxResults = new System.Windows.Forms.GroupBox();
			this.numericUpDownAltitude = new System.Windows.Forms.NumericUpDown();
			this.label4 = new System.Windows.Forms.Label();
			this.buttonGo = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.textBoxLongitude = new System.Windows.Forms.TextBox();
			this.textBoxLatitude = new System.Windows.Forms.TextBox();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.groupBoxYahooSimple = new System.Windows.Forms.GroupBox();
			this.buttonYahooSimpleSearch = new System.Windows.Forms.Button();
			this.textBoxYahooSimple = new System.Windows.Forms.TextBox();
			this.groupBoxYahooDetailed = new System.Windows.Forms.GroupBox();
			this.buttonYahooDetailedSearch = new System.Windows.Forms.Button();
			this.textBoxYahooZip = new System.Windows.Forms.TextBox();
			this.textBoxYahooState = new System.Windows.Forms.TextBox();
			this.textBoxYahooCity = new System.Windows.Forms.TextBox();
			this.textBoxYahooStreet = new System.Windows.Forms.TextBox();
			this.groupBoxWWSearch = new System.Windows.Forms.GroupBox();
			this.buttonWWSearch = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.textBoxWWFeature = new WorldWind.EnterTextBox();
			this.statusBar = new System.Windows.Forms.StatusBar();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPageWWFeature = new System.Windows.Forms.TabPage();
			this.pictureBoxWW = new System.Windows.Forms.PictureBox();
			this.label7 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.tabPageYahoo = new System.Windows.Forms.TabPage();
			this.pictureBoxYahoo = new System.Windows.Forms.PictureBox();
			this.tabPageVE = new System.Windows.Forms.TabPage();
			this.pictureBoxVE = new System.Windows.Forms.PictureBox();
			this.vesearch = new System.Windows.Forms.Button();
			this.VEenterTextBox = new WorldWind.EnterTextBox();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.pictureBox2 = new System.Windows.Forms.PictureBox();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.button1 = new System.Windows.Forms.Button();
			this.label11 = new System.Windows.Forms.Label();
			this.enterTextBox1 = new WorldWind.EnterTextBox();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.pictureBox3 = new System.Windows.Forms.PictureBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.button2 = new System.Windows.Forms.Button();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.button3 = new System.Windows.Forms.Button();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.textBox3 = new System.Windows.Forms.TextBox();
			this.textBox4 = new System.Windows.Forms.TextBox();
			this.textBox5 = new System.Windows.Forms.TextBox();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.pictureBox4 = new System.Windows.Forms.PictureBox();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.textBox6 = new System.Windows.Forms.TextBox();
			this.button4 = new System.Windows.Forms.Button();
			this.textBox7 = new System.Windows.Forms.TextBox();
			this.textBox8 = new System.Windows.Forms.TextBox();
			this.textBox9 = new System.Windows.Forms.TextBox();
			this.textBox10 = new System.Windows.Forms.TextBox();
			this.tabPage4 = new System.Windows.Forms.TabPage();
			this.pictureBox5 = new System.Windows.Forms.PictureBox();
			this.label12 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.groupBox6 = new System.Windows.Forms.GroupBox();
			this.button5 = new System.Windows.Forms.Button();
			this.label15 = new System.Windows.Forms.Label();
			this.enterTextBox2 = new WorldWind.EnterTextBox();
			this.tabPage5 = new System.Windows.Forms.TabPage();
			this.pictureBox6 = new System.Windows.Forms.PictureBox();
			this.groupBox7 = new System.Windows.Forms.GroupBox();
			this.button6 = new System.Windows.Forms.Button();
			this.textBox11 = new System.Windows.Forms.TextBox();
			this.groupBox8 = new System.Windows.Forms.GroupBox();
			this.button7 = new System.Windows.Forms.Button();
			this.textBox12 = new System.Windows.Forms.TextBox();
			this.textBox13 = new System.Windows.Forms.TextBox();
			this.textBox14 = new System.Windows.Forms.TextBox();
			this.textBox15 = new System.Windows.Forms.TextBox();
			this.tabPage6 = new System.Windows.Forms.TabPage();
			this.pictureBox7 = new System.Windows.Forms.PictureBox();
			this.groupBox9 = new System.Windows.Forms.GroupBox();
			this.textBox16 = new System.Windows.Forms.TextBox();
			this.button8 = new System.Windows.Forms.Button();
			this.textBox17 = new System.Windows.Forms.TextBox();
			this.textBox18 = new System.Windows.Forms.TextBox();
			this.textBox19 = new System.Windows.Forms.TextBox();
			this.textBox20 = new System.Windows.Forms.TextBox();
			this.label16 = new System.Windows.Forms.Label();
			this.groupBoxResults.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownAltitude)).BeginInit();
			this.groupBoxYahooSimple.SuspendLayout();
			this.groupBoxYahooDetailed.SuspendLayout();
			this.groupBoxWWSearch.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabPageWWFeature.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxWW)).BeginInit();
			this.tabPageYahoo.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxYahoo)).BeginInit();
			this.tabPageVE.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxVE)).BeginInit();
			this.tabPage1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
			this.groupBox2.SuspendLayout();
			this.tabPage2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
			this.groupBox3.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.tabPage3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
			this.groupBox5.SuspendLayout();
			this.tabPage4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
			this.groupBox6.SuspendLayout();
			this.tabPage5.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).BeginInit();
			this.groupBox7.SuspendLayout();
			this.groupBox8.SuspendLayout();
			this.tabPage6.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox7)).BeginInit();
			this.groupBox9.SuspendLayout();
			this.SuspendLayout();
			// 
			// listViewResults
			// 
			this.listViewResults.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.listViewResults.FullRowSelect = true;
			this.listViewResults.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listViewResults.HideSelection = false;
			this.listViewResults.Location = new System.Drawing.Point(8, 16);
			this.listViewResults.MultiSelect = false;
			this.listViewResults.Name = "listViewResults";
			this.listViewResults.Size = new System.Drawing.Size(536, 120);
			this.listViewResults.TabIndex = 1;
			this.listViewResults.UseCompatibleStateImageBehavior = false;
			this.listViewResults.View = System.Windows.Forms.View.Details;
			this.listViewResults.SelectedIndexChanged += new System.EventHandler(this.listViewResults_SelectedIndexChanged);
			// 
			// groupBoxResults
			// 
			this.groupBoxResults.Controls.Add(this.numericUpDownAltitude);
			this.groupBoxResults.Controls.Add(this.label4);
			this.groupBoxResults.Controls.Add(this.buttonGo);
			this.groupBoxResults.Controls.Add(this.label3);
			this.groupBoxResults.Controls.Add(this.label2);
			this.groupBoxResults.Controls.Add(this.textBoxLongitude);
			this.groupBoxResults.Controls.Add(this.textBoxLatitude);
			this.groupBoxResults.Controls.Add(this.listViewResults);
			this.groupBoxResults.Controls.Add(this.comboBox1);
			this.groupBoxResults.Location = new System.Drawing.Point(16, 216);
			this.groupBoxResults.Name = "groupBoxResults";
			this.groupBoxResults.Size = new System.Drawing.Size(552, 240);
			this.groupBoxResults.TabIndex = 2;
			this.groupBoxResults.TabStop = false;
			this.groupBoxResults.Text = "Results";
			// 
			// numericUpDownAltitude
			// 
			this.numericUpDownAltitude.Location = new System.Drawing.Point(208, 208);
			this.numericUpDownAltitude.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
			this.numericUpDownAltitude.Name = "numericUpDownAltitude";
			this.numericUpDownAltitude.Size = new System.Drawing.Size(192, 20);
			this.numericUpDownAltitude.TabIndex = 10;
			this.numericUpDownAltitude.Value = new decimal(new int[] {
            50000,
            0,
            0,
            0});
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(136, 208);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(64, 23);
			this.label4.TabIndex = 9;
			this.label4.Text = "Altitude (m)";
			// 
			// buttonGo
			// 
			this.buttonGo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonGo.Location = new System.Drawing.Point(456, 152);
			this.buttonGo.Name = "buttonGo";
			this.buttonGo.Size = new System.Drawing.Size(88, 72);
			this.buttonGo.TabIndex = 6;
			this.buttonGo.Text = "Go";
			this.buttonGo.Click += new System.EventHandler(this.buttonGo_Click);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(136, 176);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(64, 23);
			this.label3.TabIndex = 5;
			this.label3.Text = "Longitude";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(144, 144);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(56, 23);
			this.label2.TabIndex = 4;
			this.label2.Text = "Latitude";
			// 
			// textBoxLongitude
			// 
			this.textBoxLongitude.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxLongitude.Location = new System.Drawing.Point(208, 176);
			this.textBoxLongitude.Name = "textBoxLongitude";
			this.textBoxLongitude.Size = new System.Drawing.Size(240, 20);
			this.textBoxLongitude.TabIndex = 3;
			// 
			// textBoxLatitude
			// 
			this.textBoxLatitude.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxLatitude.Location = new System.Drawing.Point(208, 144);
			this.textBoxLatitude.Name = "textBoxLatitude";
			this.textBoxLatitude.Size = new System.Drawing.Size(240, 20);
			this.textBoxLatitude.TabIndex = 2;
			// 
			// comboBox1
			// 
			this.comboBox1.Items.AddRange(new object[] {
            "Decimal Degrees",
            "Degrees / Seconds"});
			this.comboBox1.Location = new System.Drawing.Point(8, 144);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(121, 21);
			this.comboBox1.TabIndex = 7;
			this.comboBox1.Text = "Decimal Degrees";
			// 
			// groupBoxYahooSimple
			// 
			this.groupBoxYahooSimple.Controls.Add(this.buttonYahooSimpleSearch);
			this.groupBoxYahooSimple.Controls.Add(this.textBoxYahooSimple);
			this.groupBoxYahooSimple.Location = new System.Drawing.Point(8, 16);
			this.groupBoxYahooSimple.Name = "groupBoxYahooSimple";
			this.groupBoxYahooSimple.Size = new System.Drawing.Size(224, 88);
			this.groupBoxYahooSimple.TabIndex = 3;
			this.groupBoxYahooSimple.TabStop = false;
			this.groupBoxYahooSimple.Text = "Yahoo! Simple Search";
			// 
			// buttonYahooSimpleSearch
			// 
			this.buttonYahooSimpleSearch.Location = new System.Drawing.Point(136, 56);
			this.buttonYahooSimpleSearch.Name = "buttonYahooSimpleSearch";
			this.buttonYahooSimpleSearch.Size = new System.Drawing.Size(75, 23);
			this.buttonYahooSimpleSearch.TabIndex = 1;
			this.buttonYahooSimpleSearch.Text = "Search";
			this.buttonYahooSimpleSearch.Click += new System.EventHandler(this.buttonYahooSimpleSearch_Click);
			// 
			// textBoxYahooSimple
			// 
			this.textBoxYahooSimple.Location = new System.Drawing.Point(16, 24);
			this.textBoxYahooSimple.Name = "textBoxYahooSimple";
			this.textBoxYahooSimple.Size = new System.Drawing.Size(200, 20);
			this.textBoxYahooSimple.TabIndex = 0;
			this.textBoxYahooSimple.Text = "Address or Keyword(s)";
			this.textBoxYahooSimple.MouseUp += new System.Windows.Forms.MouseEventHandler(this.textBoxYahooSimple_MouseUp);
			// 
			// groupBoxYahooDetailed
			// 
			this.groupBoxYahooDetailed.Controls.Add(this.buttonYahooDetailedSearch);
			this.groupBoxYahooDetailed.Controls.Add(this.textBoxYahooZip);
			this.groupBoxYahooDetailed.Controls.Add(this.textBoxYahooState);
			this.groupBoxYahooDetailed.Controls.Add(this.textBoxYahooCity);
			this.groupBoxYahooDetailed.Controls.Add(this.textBoxYahooStreet);
			this.groupBoxYahooDetailed.Location = new System.Drawing.Point(240, 16);
			this.groupBoxYahooDetailed.Name = "groupBoxYahooDetailed";
			this.groupBoxYahooDetailed.Size = new System.Drawing.Size(296, 120);
			this.groupBoxYahooDetailed.TabIndex = 4;
			this.groupBoxYahooDetailed.TabStop = false;
			this.groupBoxYahooDetailed.Text = "Yahoo! Detailed Search (USA)";
			// 
			// buttonYahooDetailedSearch
			// 
			this.buttonYahooDetailedSearch.Location = new System.Drawing.Point(208, 88);
			this.buttonYahooDetailedSearch.Name = "buttonYahooDetailedSearch";
			this.buttonYahooDetailedSearch.Size = new System.Drawing.Size(75, 23);
			this.buttonYahooDetailedSearch.TabIndex = 5;
			this.buttonYahooDetailedSearch.Text = "Search";
			this.buttonYahooDetailedSearch.Click += new System.EventHandler(this.buttonYahooDetailedSearch_Click);
			// 
			// textBoxYahooZip
			// 
			this.textBoxYahooZip.Location = new System.Drawing.Point(216, 56);
			this.textBoxYahooZip.Name = "textBoxYahooZip";
			this.textBoxYahooZip.Size = new System.Drawing.Size(64, 20);
			this.textBoxYahooZip.TabIndex = 4;
			this.textBoxYahooZip.Text = "Zip Code";
			// 
			// textBoxYahooState
			// 
			this.textBoxYahooState.Location = new System.Drawing.Point(168, 56);
			this.textBoxYahooState.Name = "textBoxYahooState";
			this.textBoxYahooState.Size = new System.Drawing.Size(40, 20);
			this.textBoxYahooState.TabIndex = 3;
			this.textBoxYahooState.Text = "State";
			// 
			// textBoxYahooCity
			// 
			this.textBoxYahooCity.Location = new System.Drawing.Point(16, 56);
			this.textBoxYahooCity.Name = "textBoxYahooCity";
			this.textBoxYahooCity.Size = new System.Drawing.Size(144, 20);
			this.textBoxYahooCity.TabIndex = 2;
			this.textBoxYahooCity.Text = "City";
			// 
			// textBoxYahooStreet
			// 
			this.textBoxYahooStreet.Location = new System.Drawing.Point(16, 24);
			this.textBoxYahooStreet.Name = "textBoxYahooStreet";
			this.textBoxYahooStreet.Size = new System.Drawing.Size(264, 20);
			this.textBoxYahooStreet.TabIndex = 1;
			this.textBoxYahooStreet.Text = "Street";
			// 
			// groupBoxWWSearch
			// 
			this.groupBoxWWSearch.Controls.Add(this.buttonWWSearch);
			this.groupBoxWWSearch.Controls.Add(this.label1);
			this.groupBoxWWSearch.Controls.Add(this.textBoxWWFeature);
			this.groupBoxWWSearch.Location = new System.Drawing.Point(157, 14);
			this.groupBoxWWSearch.Name = "groupBoxWWSearch";
			this.groupBoxWWSearch.Size = new System.Drawing.Size(296, 80);
			this.groupBoxWWSearch.TabIndex = 4;
			this.groupBoxWWSearch.TabStop = false;
			this.groupBoxWWSearch.Text = "World Wind Feature Search (Global)";
			// 
			// buttonWWSearch
			// 
			this.buttonWWSearch.Location = new System.Drawing.Point(208, 48);
			this.buttonWWSearch.Name = "buttonWWSearch";
			this.buttonWWSearch.Size = new System.Drawing.Size(75, 23);
			this.buttonWWSearch.TabIndex = 3;
			this.buttonWWSearch.Text = "Search";
			this.buttonWWSearch.Click += new System.EventHandler(this.buttonWWSearch_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 48);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(192, 23);
			this.label1.TabIndex = 2;
			this.label1.Text = "(Cities, geographical features, etc.)";
			// 
			// textBoxWWFeature
			// 
			this.textBoxWWFeature.Location = new System.Drawing.Point(16, 24);
			this.textBoxWWFeature.Name = "textBoxWWFeature";
			this.textBoxWWFeature.Size = new System.Drawing.Size(264, 20);
			this.textBoxWWFeature.TabIndex = 1;
			this.textBoxWWFeature.Text = "Keyword(s)";
			this.textBoxWWFeature.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxWWFeature_KeyUp);
			// 
			// statusBar
			// 
			this.statusBar.Location = new System.Drawing.Point(0, 472);
			this.statusBar.Name = "statusBar";
			this.statusBar.Size = new System.Drawing.Size(584, 22);
			this.statusBar.TabIndex = 5;
			// 
			// tabControl1
			// 
			this.tabControl1.Alignment = System.Windows.Forms.TabAlignment.Bottom;
			this.tabControl1.Controls.Add(this.tabPageWWFeature);
			this.tabControl1.Controls.Add(this.tabPageYahoo);
			this.tabControl1.Controls.Add(this.tabPageVE);
			this.tabControl1.Location = new System.Drawing.Point(16, 16);
			this.tabControl1.Multiline = true;
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(552, 192);
			this.tabControl1.TabIndex = 6;
			// 
			// tabPageWWFeature
			// 
			this.tabPageWWFeature.Controls.Add(this.label16);
			this.tabPageWWFeature.Controls.Add(this.pictureBoxWW);
			this.tabPageWWFeature.Controls.Add(this.label7);
			this.tabPageWWFeature.Controls.Add(this.label6);
			this.tabPageWWFeature.Controls.Add(this.label5);
			this.tabPageWWFeature.Controls.Add(this.groupBoxWWSearch);
			this.tabPageWWFeature.Location = new System.Drawing.Point(4, 4);
			this.tabPageWWFeature.Name = "tabPageWWFeature";
			this.tabPageWWFeature.Size = new System.Drawing.Size(544, 166);
			this.tabPageWWFeature.TabIndex = 2;
			this.tabPageWWFeature.Text = "Global Placename Search";
			this.tabPageWWFeature.UseVisualStyleBackColor = true;
			// 
			// pictureBoxWW
			// 
			this.pictureBoxWW.Location = new System.Drawing.Point(22, 43);
			this.pictureBoxWW.Name = "pictureBoxWW";
			this.pictureBoxWW.Size = new System.Drawing.Size(96, 80);
			this.pictureBoxWW.TabIndex = 8;
			this.pictureBoxWW.TabStop = false;
			this.pictureBoxWW.Click += new System.EventHandler(this.pictureBoxWW_Click);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label7.Location = new System.Drawing.Point(136, 102);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(69, 13);
			this.label7.TabIndex = 7;
			this.label7.Text = "Search Help:";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(136, 132);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(362, 13);
			this.label6.TabIndex = 6;
			this.label6.Text = "� Enclosing multiple words in quotes will search for the exact phrase entered";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(136, 119);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(368, 13);
			this.label5.TabIndex = 5;
			this.label5.Text = "� Searching for multiple words will find result thats contain every word entered";
			// 
			// tabPageYahoo
			// 
			this.tabPageYahoo.Controls.Add(this.pictureBoxYahoo);
			this.tabPageYahoo.Controls.Add(this.groupBoxYahooSimple);
			this.tabPageYahoo.Controls.Add(this.groupBoxYahooDetailed);
			this.tabPageYahoo.Location = new System.Drawing.Point(4, 4);
			this.tabPageYahoo.Name = "tabPageYahoo";
			this.tabPageYahoo.Size = new System.Drawing.Size(544, 166);
			this.tabPageYahoo.TabIndex = 0;
			this.tabPageYahoo.Text = "Yahoo! Search";
			this.tabPageYahoo.UseVisualStyleBackColor = true;
			// 
			// pictureBoxYahoo
			// 
			this.pictureBoxYahoo.Location = new System.Drawing.Point(72, 112);
			this.pictureBoxYahoo.Name = "pictureBoxYahoo";
			this.pictureBoxYahoo.Size = new System.Drawing.Size(88, 31);
			this.pictureBoxYahoo.TabIndex = 5;
			this.pictureBoxYahoo.TabStop = false;
			this.pictureBoxYahoo.Click += new System.EventHandler(this.pictureBoxYahoo_Click);
			// 
			// tabPageVE
			// 
			this.tabPageVE.Controls.Add(this.pictureBoxVE);
			this.tabPageVE.Controls.Add(this.vesearch);
			this.tabPageVE.Controls.Add(this.VEenterTextBox);
			this.tabPageVE.Location = new System.Drawing.Point(4, 4);
			this.tabPageVE.Name = "tabPageVE";
			this.tabPageVE.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageVE.Size = new System.Drawing.Size(544, 166);
			this.tabPageVE.TabIndex = 4;
			this.tabPageVE.Text = "Virtual Earth Search (Global)";
			this.tabPageVE.UseVisualStyleBackColor = true;
			// 
			// pictureBoxVE
			// 
			this.pictureBoxVE.Location = new System.Drawing.Point(390, 48);
			this.pictureBoxVE.Name = "pictureBoxVE";
			this.pictureBoxVE.Size = new System.Drawing.Size(54, 52);
			this.pictureBoxVE.TabIndex = 6;
			this.pictureBoxVE.TabStop = false;
			this.pictureBoxVE.Click += new System.EventHandler(this.pictureBoxVE_Click);
			// 
			// vesearch
			// 
			this.vesearch.Location = new System.Drawing.Point(227, 58);
			this.vesearch.Name = "vesearch";
			this.vesearch.Size = new System.Drawing.Size(75, 23);
			this.vesearch.TabIndex = 5;
			this.vesearch.Text = "Search";
			this.vesearch.Click += new System.EventHandler(this.VEbutton_Click);
			// 
			// VEenterTextBox
			// 
			this.VEenterTextBox.Location = new System.Drawing.Point(38, 32);
			this.VEenterTextBox.Name = "VEenterTextBox";
			this.VEenterTextBox.Size = new System.Drawing.Size(264, 20);
			this.VEenterTextBox.TabIndex = 4;
			this.VEenterTextBox.Text = "Keyword(s) or address";
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.pictureBox2);
			this.tabPage1.Controls.Add(this.label8);
			this.tabPage1.Controls.Add(this.label9);
			this.tabPage1.Controls.Add(this.label10);
			this.tabPage1.Controls.Add(this.groupBox2);
			this.tabPage1.Location = new System.Drawing.Point(4, 4);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(544, 166);
			this.tabPage1.TabIndex = 2;
			this.tabPage1.Text = "World Wind Feature Search (Global)";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// pictureBox2
			// 
			this.pictureBox2.Location = new System.Drawing.Point(22, 43);
			this.pictureBox2.Name = "pictureBox2";
			this.pictureBox2.Size = new System.Drawing.Size(96, 80);
			this.pictureBox2.TabIndex = 8;
			this.pictureBox2.TabStop = false;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label8.Location = new System.Drawing.Point(154, 106);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(69, 13);
			this.label8.TabIndex = 7;
			this.label8.Text = "Search Help:";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(154, 136);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(362, 13);
			this.label9.TabIndex = 6;
			this.label9.Text = "� Enclosing multiple words in quotes will search for the exact phrase entered";
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(154, 123);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(368, 13);
			this.label10.TabIndex = 5;
			this.label10.Text = "� Searching for multiple words will find result thats contain every word entered";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.button1);
			this.groupBox2.Controls.Add(this.label11);
			this.groupBox2.Controls.Add(this.enterTextBox1);
			this.groupBox2.Location = new System.Drawing.Point(157, 14);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(296, 80);
			this.groupBox2.TabIndex = 4;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "World Wind Feature Search (Global)";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(208, 48);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 3;
			this.button1.Text = "Search";
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(16, 48);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(192, 23);
			this.label11.TabIndex = 2;
			this.label11.Text = "(Cities, geographical features, etc.)";
			// 
			// enterTextBox1
			// 
			this.enterTextBox1.Location = new System.Drawing.Point(16, 24);
			this.enterTextBox1.Name = "enterTextBox1";
			this.enterTextBox1.Size = new System.Drawing.Size(264, 20);
			this.enterTextBox1.TabIndex = 1;
			this.enterTextBox1.Text = "Keyword(s)";
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.pictureBox3);
			this.tabPage2.Controls.Add(this.groupBox3);
			this.tabPage2.Controls.Add(this.groupBox4);
			this.tabPage2.Location = new System.Drawing.Point(4, 4);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(544, 166);
			this.tabPage2.TabIndex = 0;
			this.tabPage2.Text = "Yahoo! Search";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// pictureBox3
			// 
			this.pictureBox3.Location = new System.Drawing.Point(72, 112);
			this.pictureBox3.Name = "pictureBox3";
			this.pictureBox3.Size = new System.Drawing.Size(88, 31);
			this.pictureBox3.TabIndex = 5;
			this.pictureBox3.TabStop = false;
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.button2);
			this.groupBox3.Controls.Add(this.textBox1);
			this.groupBox3.Location = new System.Drawing.Point(8, 16);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(224, 88);
			this.groupBox3.TabIndex = 3;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Yahoo! Simple Search";
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(136, 56);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 23);
			this.button2.TabIndex = 1;
			this.button2.Text = "Search";
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(16, 24);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(200, 20);
			this.textBox1.TabIndex = 0;
			this.textBox1.Text = "Address or Keyword(s)";
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.button3);
			this.groupBox4.Controls.Add(this.textBox2);
			this.groupBox4.Controls.Add(this.textBox3);
			this.groupBox4.Controls.Add(this.textBox4);
			this.groupBox4.Controls.Add(this.textBox5);
			this.groupBox4.Location = new System.Drawing.Point(240, 16);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(296, 120);
			this.groupBox4.TabIndex = 4;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Yahoo! Detailed Search (USA)";
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(208, 88);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(75, 23);
			this.button3.TabIndex = 5;
			this.button3.Text = "Search";
			// 
			// textBox2
			// 
			this.textBox2.Location = new System.Drawing.Point(216, 56);
			this.textBox2.Name = "textBox2";
			this.textBox2.Size = new System.Drawing.Size(64, 20);
			this.textBox2.TabIndex = 4;
			this.textBox2.Text = "Zip Code";
			// 
			// textBox3
			// 
			this.textBox3.Location = new System.Drawing.Point(168, 56);
			this.textBox3.Name = "textBox3";
			this.textBox3.Size = new System.Drawing.Size(40, 20);
			this.textBox3.TabIndex = 3;
			this.textBox3.Text = "State";
			// 
			// textBox4
			// 
			this.textBox4.Location = new System.Drawing.Point(16, 56);
			this.textBox4.Name = "textBox4";
			this.textBox4.Size = new System.Drawing.Size(144, 20);
			this.textBox4.TabIndex = 2;
			this.textBox4.Text = "City";
			// 
			// textBox5
			// 
			this.textBox5.Location = new System.Drawing.Point(16, 24);
			this.textBox5.Name = "textBox5";
			this.textBox5.Size = new System.Drawing.Size(264, 20);
			this.textBox5.TabIndex = 1;
			this.textBox5.Text = "Street";
			// 
			// tabPage3
			// 
			this.tabPage3.Controls.Add(this.pictureBox4);
			this.tabPage3.Controls.Add(this.groupBox5);
			this.tabPage3.Location = new System.Drawing.Point(4, 4);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(544, 166);
			this.tabPage3.TabIndex = 3;
			this.tabPage3.Text = "Terra Page Detailed Search (Australia)";
			this.tabPage3.UseVisualStyleBackColor = true;
			// 
			// pictureBox4
			// 
			this.pictureBox4.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox4.Image")));
			this.pictureBox4.Location = new System.Drawing.Point(432, 112);
			this.pictureBox4.Name = "pictureBox4";
			this.pictureBox4.Size = new System.Drawing.Size(104, 40);
			this.pictureBox4.TabIndex = 5;
			this.pictureBox4.TabStop = false;
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.textBox6);
			this.groupBox5.Controls.Add(this.button4);
			this.groupBox5.Controls.Add(this.textBox7);
			this.groupBox5.Controls.Add(this.textBox8);
			this.groupBox5.Controls.Add(this.textBox9);
			this.groupBox5.Controls.Add(this.textBox10);
			this.groupBox5.Location = new System.Drawing.Point(120, 8);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(296, 120);
			this.groupBox5.TabIndex = 4;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "TerraPage Detailed Search (Australia)";
			// 
			// textBox6
			// 
			this.textBox6.Location = new System.Drawing.Point(16, 24);
			this.textBox6.Name = "textBox6";
			this.textBox6.Size = new System.Drawing.Size(44, 20);
			this.textBox6.TabIndex = 6;
			this.textBox6.Text = "No";
			// 
			// button4
			// 
			this.button4.Location = new System.Drawing.Point(208, 88);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(75, 23);
			this.button4.TabIndex = 5;
			this.button4.Text = "Search";
			// 
			// textBox7
			// 
			this.textBox7.Location = new System.Drawing.Point(216, 56);
			this.textBox7.Name = "textBox7";
			this.textBox7.Size = new System.Drawing.Size(64, 20);
			this.textBox7.TabIndex = 4;
			this.textBox7.Text = "Post Code";
			// 
			// textBox8
			// 
			this.textBox8.Location = new System.Drawing.Point(168, 56);
			this.textBox8.Name = "textBox8";
			this.textBox8.Size = new System.Drawing.Size(40, 20);
			this.textBox8.TabIndex = 3;
			this.textBox8.Text = "State";
			// 
			// textBox9
			// 
			this.textBox9.Location = new System.Drawing.Point(16, 56);
			this.textBox9.Name = "textBox9";
			this.textBox9.Size = new System.Drawing.Size(144, 20);
			this.textBox9.TabIndex = 2;
			this.textBox9.Text = "Suburb";
			// 
			// textBox10
			// 
			this.textBox10.Location = new System.Drawing.Point(72, 24);
			this.textBox10.Name = "textBox10";
			this.textBox10.Size = new System.Drawing.Size(208, 20);
			this.textBox10.TabIndex = 1;
			this.textBox10.Text = "Street";
			// 
			// tabPage4
			// 
			this.tabPage4.Controls.Add(this.pictureBox5);
			this.tabPage4.Controls.Add(this.label12);
			this.tabPage4.Controls.Add(this.label13);
			this.tabPage4.Controls.Add(this.label14);
			this.tabPage4.Controls.Add(this.groupBox6);
			this.tabPage4.Location = new System.Drawing.Point(4, 4);
			this.tabPage4.Name = "tabPage4";
			this.tabPage4.Size = new System.Drawing.Size(544, 166);
			this.tabPage4.TabIndex = 2;
			this.tabPage4.Text = "World Wind Feature Search (Global)";
			this.tabPage4.UseVisualStyleBackColor = true;
			// 
			// pictureBox5
			// 
			this.pictureBox5.Location = new System.Drawing.Point(22, 43);
			this.pictureBox5.Name = "pictureBox5";
			this.pictureBox5.Size = new System.Drawing.Size(96, 80);
			this.pictureBox5.TabIndex = 8;
			this.pictureBox5.TabStop = false;
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label12.Location = new System.Drawing.Point(154, 106);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(69, 13);
			this.label12.TabIndex = 7;
			this.label12.Text = "Search Help:";
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(154, 136);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(362, 13);
			this.label13.TabIndex = 6;
			this.label13.Text = "� Enclosing multiple words in quotes will search for the exact phrase entered";
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(154, 123);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(368, 13);
			this.label14.TabIndex = 5;
			this.label14.Text = "� Searching for multiple words will find result thats contain every word entered";
			// 
			// groupBox6
			// 
			this.groupBox6.Controls.Add(this.button5);
			this.groupBox6.Controls.Add(this.label15);
			this.groupBox6.Controls.Add(this.enterTextBox2);
			this.groupBox6.Location = new System.Drawing.Point(157, 14);
			this.groupBox6.Name = "groupBox6";
			this.groupBox6.Size = new System.Drawing.Size(296, 80);
			this.groupBox6.TabIndex = 4;
			this.groupBox6.TabStop = false;
			this.groupBox6.Text = "World Wind Feature Search (Global)";
			// 
			// button5
			// 
			this.button5.Location = new System.Drawing.Point(208, 48);
			this.button5.Name = "button5";
			this.button5.Size = new System.Drawing.Size(75, 23);
			this.button5.TabIndex = 3;
			this.button5.Text = "Search";
			// 
			// label15
			// 
			this.label15.Location = new System.Drawing.Point(16, 48);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(192, 23);
			this.label15.TabIndex = 2;
			this.label15.Text = "(Cities, geographical features, etc.)";
			// 
			// enterTextBox2
			// 
			this.enterTextBox2.Location = new System.Drawing.Point(16, 24);
			this.enterTextBox2.Name = "enterTextBox2";
			this.enterTextBox2.Size = new System.Drawing.Size(264, 20);
			this.enterTextBox2.TabIndex = 1;
			this.enterTextBox2.Text = "Keyword(s)";
			// 
			// tabPage5
			// 
			this.tabPage5.Controls.Add(this.pictureBox6);
			this.tabPage5.Controls.Add(this.groupBox7);
			this.tabPage5.Controls.Add(this.groupBox8);
			this.tabPage5.Location = new System.Drawing.Point(4, 4);
			this.tabPage5.Name = "tabPage5";
			this.tabPage5.Size = new System.Drawing.Size(544, 166);
			this.tabPage5.TabIndex = 0;
			this.tabPage5.Text = "Yahoo! Search";
			this.tabPage5.UseVisualStyleBackColor = true;
			// 
			// pictureBox6
			// 
			this.pictureBox6.Location = new System.Drawing.Point(72, 112);
			this.pictureBox6.Name = "pictureBox6";
			this.pictureBox6.Size = new System.Drawing.Size(88, 31);
			this.pictureBox6.TabIndex = 5;
			this.pictureBox6.TabStop = false;
			// 
			// groupBox7
			// 
			this.groupBox7.Controls.Add(this.button6);
			this.groupBox7.Controls.Add(this.textBox11);
			this.groupBox7.Location = new System.Drawing.Point(8, 16);
			this.groupBox7.Name = "groupBox7";
			this.groupBox7.Size = new System.Drawing.Size(224, 88);
			this.groupBox7.TabIndex = 3;
			this.groupBox7.TabStop = false;
			this.groupBox7.Text = "Yahoo! Simple Search";
			// 
			// button6
			// 
			this.button6.Location = new System.Drawing.Point(136, 56);
			this.button6.Name = "button6";
			this.button6.Size = new System.Drawing.Size(75, 23);
			this.button6.TabIndex = 1;
			this.button6.Text = "Search";
			// 
			// textBox11
			// 
			this.textBox11.Location = new System.Drawing.Point(16, 24);
			this.textBox11.Name = "textBox11";
			this.textBox11.Size = new System.Drawing.Size(200, 20);
			this.textBox11.TabIndex = 0;
			this.textBox11.Text = "Address or Keyword(s)";
			// 
			// groupBox8
			// 
			this.groupBox8.Controls.Add(this.button7);
			this.groupBox8.Controls.Add(this.textBox12);
			this.groupBox8.Controls.Add(this.textBox13);
			this.groupBox8.Controls.Add(this.textBox14);
			this.groupBox8.Controls.Add(this.textBox15);
			this.groupBox8.Location = new System.Drawing.Point(240, 16);
			this.groupBox8.Name = "groupBox8";
			this.groupBox8.Size = new System.Drawing.Size(296, 120);
			this.groupBox8.TabIndex = 4;
			this.groupBox8.TabStop = false;
			this.groupBox8.Text = "Yahoo! Detailed Search (USA)";
			// 
			// button7
			// 
			this.button7.Location = new System.Drawing.Point(208, 88);
			this.button7.Name = "button7";
			this.button7.Size = new System.Drawing.Size(75, 23);
			this.button7.TabIndex = 5;
			this.button7.Text = "Search";
			// 
			// textBox12
			// 
			this.textBox12.Location = new System.Drawing.Point(216, 56);
			this.textBox12.Name = "textBox12";
			this.textBox12.Size = new System.Drawing.Size(64, 20);
			this.textBox12.TabIndex = 4;
			this.textBox12.Text = "Zip Code";
			// 
			// textBox13
			// 
			this.textBox13.Location = new System.Drawing.Point(168, 56);
			this.textBox13.Name = "textBox13";
			this.textBox13.Size = new System.Drawing.Size(40, 20);
			this.textBox13.TabIndex = 3;
			this.textBox13.Text = "State";
			// 
			// textBox14
			// 
			this.textBox14.Location = new System.Drawing.Point(16, 56);
			this.textBox14.Name = "textBox14";
			this.textBox14.Size = new System.Drawing.Size(144, 20);
			this.textBox14.TabIndex = 2;
			this.textBox14.Text = "City";
			// 
			// textBox15
			// 
			this.textBox15.Location = new System.Drawing.Point(16, 24);
			this.textBox15.Name = "textBox15";
			this.textBox15.Size = new System.Drawing.Size(264, 20);
			this.textBox15.TabIndex = 1;
			this.textBox15.Text = "Street";
			// 
			// tabPage6
			// 
			this.tabPage6.Controls.Add(this.pictureBox7);
			this.tabPage6.Controls.Add(this.groupBox9);
			this.tabPage6.Location = new System.Drawing.Point(4, 4);
			this.tabPage6.Name = "tabPage6";
			this.tabPage6.Size = new System.Drawing.Size(544, 166);
			this.tabPage6.TabIndex = 3;
			this.tabPage6.Text = "Terra Page Detailed Search (Australia)";
			this.tabPage6.UseVisualStyleBackColor = true;
			// 
			// pictureBox7
			// 
			this.pictureBox7.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox7.Image")));
			this.pictureBox7.Location = new System.Drawing.Point(432, 112);
			this.pictureBox7.Name = "pictureBox7";
			this.pictureBox7.Size = new System.Drawing.Size(104, 40);
			this.pictureBox7.TabIndex = 5;
			this.pictureBox7.TabStop = false;
			// 
			// groupBox9
			// 
			this.groupBox9.Controls.Add(this.textBox16);
			this.groupBox9.Controls.Add(this.button8);
			this.groupBox9.Controls.Add(this.textBox17);
			this.groupBox9.Controls.Add(this.textBox18);
			this.groupBox9.Controls.Add(this.textBox19);
			this.groupBox9.Controls.Add(this.textBox20);
			this.groupBox9.Location = new System.Drawing.Point(120, 8);
			this.groupBox9.Name = "groupBox9";
			this.groupBox9.Size = new System.Drawing.Size(296, 120);
			this.groupBox9.TabIndex = 4;
			this.groupBox9.TabStop = false;
			this.groupBox9.Text = "TerraPage Detailed Search (Australia)";
			// 
			// textBox16
			// 
			this.textBox16.Location = new System.Drawing.Point(16, 24);
			this.textBox16.Name = "textBox16";
			this.textBox16.Size = new System.Drawing.Size(44, 20);
			this.textBox16.TabIndex = 6;
			this.textBox16.Text = "No";
			// 
			// button8
			// 
			this.button8.Location = new System.Drawing.Point(208, 88);
			this.button8.Name = "button8";
			this.button8.Size = new System.Drawing.Size(75, 23);
			this.button8.TabIndex = 5;
			this.button8.Text = "Search";
			// 
			// textBox17
			// 
			this.textBox17.Location = new System.Drawing.Point(216, 56);
			this.textBox17.Name = "textBox17";
			this.textBox17.Size = new System.Drawing.Size(64, 20);
			this.textBox17.TabIndex = 4;
			this.textBox17.Text = "Post Code";
			// 
			// textBox18
			// 
			this.textBox18.Location = new System.Drawing.Point(168, 56);
			this.textBox18.Name = "textBox18";
			this.textBox18.Size = new System.Drawing.Size(40, 20);
			this.textBox18.TabIndex = 3;
			this.textBox18.Text = "State";
			// 
			// textBox19
			// 
			this.textBox19.Location = new System.Drawing.Point(16, 56);
			this.textBox19.Name = "textBox19";
			this.textBox19.Size = new System.Drawing.Size(144, 20);
			this.textBox19.TabIndex = 2;
			this.textBox19.Text = "Suburb";
			// 
			// textBox20
			// 
			this.textBox20.Location = new System.Drawing.Point(72, 24);
			this.textBox20.Name = "textBox20";
			this.textBox20.Size = new System.Drawing.Size(208, 20);
			this.textBox20.TabIndex = 1;
			this.textBox20.Text = "Street";
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Location = new System.Drawing.Point(136, 145);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(402, 13);
			this.label16.TabIndex = 9;
			this.label16.Text = "� This tab can\'t search addresses; use the Yahoo! or Virtual Earth tabs for addre" +
				"sses";
			// 
			// PlaceFinder
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(584, 494);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.statusBar);
			this.Controls.Add(this.groupBoxResults);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "PlaceFinder";
			this.Text = "Place Finder";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.OnClosing);
			this.groupBoxResults.ResumeLayout(false);
			this.groupBoxResults.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownAltitude)).EndInit();
			this.groupBoxYahooSimple.ResumeLayout(false);
			this.groupBoxYahooSimple.PerformLayout();
			this.groupBoxYahooDetailed.ResumeLayout(false);
			this.groupBoxYahooDetailed.PerformLayout();
			this.groupBoxWWSearch.ResumeLayout(false);
			this.groupBoxWWSearch.PerformLayout();
			this.tabControl1.ResumeLayout(false);
			this.tabPageWWFeature.ResumeLayout(false);
			this.tabPageWWFeature.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxWW)).EndInit();
			this.tabPageYahoo.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxYahoo)).EndInit();
			this.tabPageVE.ResumeLayout(false);
			this.tabPageVE.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxVE)).EndInit();
			this.tabPage1.ResumeLayout(false);
			this.tabPage1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.tabPage2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.groupBox4.ResumeLayout(false);
			this.groupBox4.PerformLayout();
			this.tabPage3.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
			this.groupBox5.ResumeLayout(false);
			this.groupBox5.PerformLayout();
			this.tabPage4.ResumeLayout(false);
			this.tabPage4.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
			this.groupBox6.ResumeLayout(false);
			this.groupBox6.PerformLayout();
			this.tabPage5.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).EndInit();
			this.groupBox7.ResumeLayout(false);
			this.groupBox7.PerformLayout();
			this.groupBox8.ResumeLayout(false);
			this.groupBox8.PerformLayout();
			this.tabPage6.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox7)).EndInit();
			this.groupBox9.ResumeLayout(false);
			this.groupBox9.PerformLayout();
			this.ResumeLayout(false);

		}
		#endregion

		string m_CurrentSearchUri = null;
		private void buttonYahooSimpleSearch_Click(object sender, System.EventArgs e)
		{
			lastSearchTerm = "";
			m_CurrentSearchUri = string.Format(
				"{0}?appid={1}&location={2}", YahooUri, YahooAppId, textBoxYahooSimple.Text);

			this.listViewResults.Items.Clear();
			this.listViewResults.Columns.Clear();
	
			this.listViewResults.Columns.Add("Address", 120, HorizontalAlignment.Center);
			this.listViewResults.Columns.Add("City", 80, HorizontalAlignment.Center);
			this.listViewResults.Columns.Add("State", 30, HorizontalAlignment.Center);
			this.listViewResults.Columns.Add("Zip", 50, HorizontalAlignment.Center);
			this.listViewResults.Columns.Add("Country", 80, HorizontalAlignment.Center);
			this.listViewResults.Columns.Add("Latitude", 80, HorizontalAlignment.Center);
			this.listViewResults.Columns.Add("Longitude", 80, HorizontalAlignment.Center);

			try
			{
				this.statusBar.BeginInvoke(new SetStatusMessageDelegate(SetStatusMessage), new object[] { "Searching Yahoo Simple..." });
				System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(SearchFunction));
				t.IsBackground = true;
				t.Start();
			}
			catch//(Exception ex)
			{
				//this.richTextBox1.Text = ex.ToString();
			}
		}

		private void SearchFunction()
		{
			try
			{
				if(m_CurrentSearchUri != null)
				{
					//using(WebClient client = new WebClient())
					using(WebDownload client = new WebDownload(m_CurrentSearchUri))
					{
						client.DownloadMemory();
						XmlTextReader reader = new XmlTextReader(client.ContentStream);
						ArrayList resultList = new ArrayList();
						Hashtable curResult = new Hashtable();

						while(reader.Read())
						{
							if(reader.NodeType == XmlNodeType.Element)
							{
								if(reader.Name != "Result" && reader.Name != "ResultSet")
								{
									//results come from Yahoo with . a the decimal separator,
									//but are parsed so they appear in the list using the local
									//settings
									if (reader.Name == "Latitude" || reader.Name == "Longitude")
									{
										string sval = reader.ReadString();
										double dval = double.Parse(sval, CultureInfo.InvariantCulture);
										curResult.Add(reader.Name, dval.ToString());
									}
									else
										curResult.Add(reader.Name, reader.ReadString());

								}
							}
							else if(reader.NodeType == XmlNodeType.EndElement)
							{
								if(reader.Name.Equals("Result"))
								{
									resultList.Add(curResult);
									curResult = new Hashtable();
								}
							}
						}

						foreach(Hashtable r in resultList)
						{
							string[] parts = new string[this.listViewResults.Columns.Count];

							for(int i = 0; i < this.listViewResults.Columns.Count; i++)
							{
								parts[i] = (string)r[this.listViewResults.Columns[i].Text];
							}

							this.listViewResults.Items.Add(new ListViewItem(parts));
						}
					
					}
				
				}

				this.statusBar.BeginInvoke(new SetStatusMessageDelegate(SetStatusMessage), new object[] {this.listViewResults.Items.Count.ToString() + " Yahoo results found"});
			}
			catch
			{
				this.statusBar.BeginInvoke(new SetStatusMessageDelegate(SetStatusMessage), new object[] { "Error searching Yahoo" });
			}
		}

		static string getInnerTextFromFirstChild(XPathNodeIterator iter)
		{
			if(iter.Count == 0)
			{
				return null;
			}
			else
			{
				iter.MoveNext();
				return iter.Current.Value;
			}
		}

		private void buttonYahooDetailedSearch_Click(object sender, System.EventArgs e)
		{
			lastSearchTerm = "";

			m_CurrentSearchUri = string.Format(
				"{0}?appid={1}&street={2}&city={3}&state={4}&zip={5}", YahooUri, YahooAppId,
				this.textBoxYahooStreet.Text,
				this.textBoxYahooCity.Text,
				this.textBoxYahooState.Text,
				this.textBoxYahooZip.Text);

			this.listViewResults.Items.Clear();
			this.listViewResults.Columns.Clear();
	
			this.listViewResults.Columns.Add("Address", 120, HorizontalAlignment.Center);
			this.listViewResults.Columns.Add("City", 80, HorizontalAlignment.Center);
			this.listViewResults.Columns.Add("State", 30, HorizontalAlignment.Center);
			this.listViewResults.Columns.Add("Zip", 50, HorizontalAlignment.Center);
			this.listViewResults.Columns.Add("Country", 80, HorizontalAlignment.Center);
			this.listViewResults.Columns.Add("Latitude", 80, HorizontalAlignment.Center);
			this.listViewResults.Columns.Add("Longitude", 80, HorizontalAlignment.Center);

			try
			{
				this.statusBar.BeginInvoke(new SetStatusMessageDelegate(SetStatusMessage), new object[] { "Searching Yahoo Detailed..." });
				System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(SearchFunction));
				t.IsBackground = true;
				t.Start();
			}
			catch//(Exception ex)
			{
				//this.richTextBox1.Text = ex.ToString();
			}
		}

		/// <summary>
		/// Search WWC's placename db
		/// </summary>
		private void SearchOnline()
		{
			// Stop unnecessary queryies
			if (this.textBoxWWFeature.Text == lastSearchTerm || this.textBoxWWFeature.Text == "Keyword(s)")
			{
				return;
			}


			this.listViewResults.Items.Clear();
			this.listViewResults.Columns.Clear();
			this.listViewResults.Columns.Add("Name", 150, HorizontalAlignment.Left);
			this.listViewResults.Columns.Add("Country", 120, HorizontalAlignment.Left);
			this.listViewResults.Columns.Add("Region", 80, HorizontalAlignment.Left);
			this.listViewResults.Columns.Add("Type", 90, HorizontalAlignment.Left);
			this.listViewResults.Columns.Add("Latitude", 60, HorizontalAlignment.Left);
			this.listViewResults.Columns.Add("Longitude", 60, HorizontalAlignment.Left);

			this.statusBar.BeginInvoke(new SetStatusMessageDelegate(SetStatusMessage), new object[] { "Searching global placenames..." });


			XmlTextReader reader = null;

			try
			{
				
				string query = this.textBoxWWFeature.Text;

				// use '+' for an AND query
				if (query.Contains(" "))
				{
					query = "+" + query;
					query = Regex.Replace(query, @"\s+", " +");
				}


				//urlencode query string
				string urlQuery = HttpUtility.UrlEncode(query);

				string searchUriString = "http://s0.tileservice.worldwindcentral.org/queryGeoNames?q=" + urlQuery + "&columns=className,countryName,priAdmName,latitude,longitude,name,origName";


				try
				{

					WorldWind.Net.WebDownload dl = new WorldWind.Net.WebDownload(searchUriString);
					dl.Compressed = true;
					dl.DownloadMemory();

					if (String.Compare("gzip", dl.ContentEncoding, true) == 0)
					{
						GZipStream uncompressed = new GZipStream(dl.ContentStream, CompressionMode.Decompress);
						reader = new XmlTextReader(uncompressed);
					}
					else
					{
						reader = new XmlTextReader(dl.ContentStream);
					}

					// Stop duplicate searches
					lastSearchTerm = this.textBoxWWFeature.Text;
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);
				}

				int counter = 0;

				while (reader.Read())
				{
					if (reader.Name == "Row")
					{
						string name = null;

						if (reader.GetAttribute("origName") == "")
						{
							name = reader.GetAttribute("name");
						}
						else
						{
							name = reader.GetAttribute("origName");
						}

						//string name = reader.GetAttribute("name");
						string sLon = reader.GetAttribute("longitude");
						string sLat = reader.GetAttribute("latitude");
						string country = reader.GetAttribute("countryName");
						string region = reader.GetAttribute("priAdmName");
						string type = reader.GetAttribute("className");
						double lon = double.Parse(sLon, CultureInfo.InvariantCulture);
						double lat = double.Parse(sLat, CultureInfo.InvariantCulture);
						ListViewItem item = new ListViewItem(new string[] { name, country, region, type, lat.ToString(CultureInfo.CurrentCulture), lon.ToString(CultureInfo.CurrentCulture) });
						listViewResults.Items.Add(item);
						counter++;
					}


				}

				if (counter != 0)
				{
					string status = "Found " + counter + " results from global placenames.";
					this.statusBar.BeginInvoke(new SetStatusMessageDelegate(SetStatusMessage), new object[] { status });
				}
				else
				{
					this.statusBar.BeginInvoke(new SetStatusMessageDelegate(SetStatusMessage), new object[] { "Found no results." });
				}

			}
			catch (XmlException xmle)
			{
				MessageBox.Show(xmle.Message);
			}

		}


		private void buttonWWSearch_Click(object sender, System.EventArgs e)
		{
			this.startWWSearch();
		}

		private void startWWSearch()
		{
			if (!World.Settings.UseOfflineSearch && m_WorldWindow.CurrentWorld.IsEarth)
			{
				SearchOnline();
			}
			else
			{
				this.listViewResults.Items.Clear();
				this.listViewResults.Columns.Clear();
				this.listViewResults.Columns.Add("Name", 120, HorizontalAlignment.Center);
				this.listViewResults.Columns.Add("Country", 80, HorizontalAlignment.Center);
				this.listViewResults.Columns.Add("Latitude", 80, HorizontalAlignment.Center);
				this.listViewResults.Columns.Add("Longitude", 80, HorizontalAlignment.Center);

				System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(SearchWWData));
				t.IsBackground = true;
				t.Start();
			}
		}

		private void SearchWWData()
		{
			this.statusBar.BeginInvoke(new SetStatusMessageDelegate(SetStatusMessage), new object[] {"Searching..."});

			string[] searchtokens = textBoxWWFeature.Text.Split(' ');
			//first find the relavent placename data files
			string[] wplFiles = getWplFiles(m_WorldWindow.CurrentWorld.RenderableObjects);
			if(wplFiles != null)
			{
				foreach(string file in wplFiles)
				{
					PlaceNameSetFullSearch(searchtokens, file);
				}
			}

			this.statusBar.BeginInvoke(new SetStatusMessageDelegate(SetStatusMessage), new object[] {this.listViewResults.Items.Count.ToString() + " items found."});
		}

		private delegate void SetStatusMessageDelegate(string msg);
		private void SetStatusMessage(string msg)
		{
			this.statusBar.Text = msg;
		}

		// perform a full search in a placename set, with attributes
		bool PlaceNameSetFullSearch(string [] searchTokens, string placenameDataFile) 
		{
			DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(
				Path.Combine(
				Path.GetDirectoryName(Application.ExecutablePath), 
				placenameDataFile)));

			// ignore this set if the corresponding directory does not exist for some reason
			if(!dir.Exists) return true;

			// loop over all WWP files in directory
			foreach(FileInfo placenameFile in dir.GetFiles("*.wwp"))
			{
				using(BinaryReader reader = new BinaryReader(new BufferedStream(placenameFile.OpenRead(), 10000000)) ) 
				{
					int placenameCount = reader.ReadInt32();

					// loop over all places
					for(int i = 0; i < placenameCount; i++) 
					{
						// return false if stop requested
						if(CheckStopRequested()) return false;

						// instantiate and read current placename
						WorldWindPlacename pn = new WorldWindPlacename();
						WplIndex.ReadPlaceName(reader, ref pn, WplIndex.MetaDataAction.Store);

						// if we have a match ...
						if(isPlaceMatched(searchTokens, pn)) 
						{
							if(CheckMaxResults()) return false;

							

							// add item via delegate to avoid MT issues
							listViewResults.Invoke(new addPlaceDelegate(addPlace), new object[] { pn });
						}
					}
				}
			}
			return true; // go on 
		}

		private delegate void addPlaceDelegate(WorldWindPlacename pn);
		private void addPlace(WorldWindPlacename pn)
		{
			ListViewItem item = new ListViewItem(
				new string[] { pn.Name, (string)pn.metaData["Country"], pn.Lat.ToString(), pn.Lon.ToString() }
				);
			item.Tag = pn;
			listViewResults.Items.Add(item);
		}

		bool cancelSearch = false;

		// utility: sets UI to "stopped" state
		void SetStopped( string statusMessage ) 
		{
			this.cancelSearch = false;
		//	this.buttonStop.Enabled = false;
		//	this.statusBarPanel.AutoSize = StatusBarPanelAutoSize.Contents;
		//	this.statusBarPanel.Text = statusMessage;
		//	this.progressBarSearch.Visible = false;
		}

		void SetStopped() 
		{
			SetStopped("Stopped.");
		}

		// utility: checks for stop request on search thread, sets UI
		bool CheckStopRequested() 
		{
			if(this.cancelSearch) 
			{
				SetStopped();
				return true;
			}

			return false;
		}

		// utility: checks for max results on search thread
		bool CheckMaxResults()
		{
		//	if(this.listViewResults.Items.Count > maxPlaceResults) 
		//	{  // too many matches
		//		SetStopped(String.Format("More Than {0} matches...please revise search", maxPlaceResults));
		//		return true; // max results reached
		//	}

			return false;
		}

		// Utility function for exhaustive search: check if place meets criteria
		static bool isPlaceMatched(string[] searchTokens, WorldWindPlacename pn)
		{
			char[] delimiters = new char[] {' ','(',')',','};

			string targetString;

			if(pn.metaData != null)
			{  // concatenate all metadata, separate with spaces
				StringBuilder sb = new StringBuilder(pn.Name);
				foreach(string str in pn.metaData.Values)
				{
					sb.Append(' ');
					sb.Append(str);
				}
				targetString = sb.ToString();
			}
			else 
			{
				targetString  = pn.Name;
			}

			// now compute new target tokens
			string[] targetTokens = targetString.Split(delimiters);
         
			// Note that all searchtokens have to match before we consider a place found
			foreach(string curSearchToken in searchTokens)
			{
				bool found = false;
				foreach(string curTargetToken in targetTokens)
				{
					if(String.Compare(curSearchToken, curTargetToken, true) == 0)
					{
						found = true;
						break; // found this search token, move to next one
					}
				}

				// continue only if at least one target token was found
				if(!found)
					return false;
			}

			return true;
		}

		private string[] getWplFiles(WorldWind.Renderable.RenderableObject ro)
		{
			if(ro is WorldWind.Renderable.TiledPlacenameSet)
			{
				if(ro.MetaData.Contains("PlacenameDataFile"))
				{
					return (string[])new string[] {(string)ro.MetaData["PlacenameDataFile"]};
				}
			}
			else if(ro is WorldWind.Renderable.RenderableObjectList)
			{
				WorldWind.Renderable.RenderableObjectList rol = (WorldWind.Renderable.RenderableObjectList)ro;
				ArrayList wplFiles = new ArrayList();

				foreach(WorldWind.Renderable.RenderableObject childRo in rol.ChildObjects)
				{
					string[] childStrings = getWplFiles(childRo);
					if(childStrings != null)
						foreach(string childString in childStrings)
							wplFiles.Add(childString);

				}

				if(wplFiles.Count > 0)
					return (string[])wplFiles.ToArray(typeof(string));
			}

			return null;

				
		}

		private void buttonGo_Click(object sender, System.EventArgs e)
		{
			try
			{
				double lat = double.Parse(this.textBoxLatitude.Text);
				double lon = double.Parse(this.textBoxLongitude.Text);
				double alt = (double)this.numericUpDownAltitude.Value;

				m_WorldWindow.GotoLatLonAltitude(lat, lon, alt);
			}
			catch{}
		}

		private void textBoxYahooSimple_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(this.textBoxYahooSimple.Text == "Address or Keyword(s)")
			{
				this.textBoxYahooSimple.Text = "";
			}
		}

		private void listViewResults_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(this.listViewResults.SelectedItems.Count > 0)
			{
				string latString = null;
				string lonString = null;
				string addressString = null;
				string cityString = null;
				string countryString = null;

				for(int i = 0; i < this.listViewResults.Columns.Count; i++)
				{
					if(this.listViewResults.Columns[i].Text == "Latitude")
					{
						latString = this.listViewResults.SelectedItems[0].SubItems[i].Text;
					}
					else if(this.listViewResults.Columns[i].Text == "Longitude")
					{
						lonString = this.listViewResults.SelectedItems[0].SubItems[i].Text;
					}
					else if(this.listViewResults.Columns[i].Text == "Address")
					{
						addressString = this.listViewResults.SelectedItems[0].SubItems[i].Text;
					}
					else if(this.listViewResults.Columns[i].Text == "City")
					{
						cityString = this.listViewResults.SelectedItems[0].SubItems[i].Text;
					}
					else if(this.listViewResults.Columns[i].Text == "Country")
					{
						countryString = this.listViewResults.SelectedItems[0].SubItems[i].Text;
					}
				}

				if(addressString != null && addressString.Length > 0)
				{
					this.numericUpDownAltitude.Value = 2500;
				}
				else if(cityString != null && cityString.Length > 0)
				{
					this.numericUpDownAltitude.Value = 50000;
				}
				else if(countryString != null && countryString.Length > 0)
				{
					this.numericUpDownAltitude.Value = 500000;
				}

				if(latString != null && lonString != null)
				{
					this.textBoxLatitude.Text = latString;
					this.textBoxLongitude.Text = lonString;

					double lat = double.Parse(latString, CultureInfo.CurrentCulture);
					double lon = double.Parse(lonString, CultureInfo.CurrentCulture);

					m_WorldWindow.GotoLatLon(lat, lon);
				}
			}
		}

		private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;	
			this.Visible = false;
		}

		private void pictureBoxYahoo_Click(object sender, System.EventArgs e)
		{
			ProcessStartInfo psi = new ProcessStartInfo();
			psi.FileName = "http://developer.yahoo.net/about";
			psi.Verb = "open";
			psi.UseShellExecute = true;
			psi.CreateNoWindow = true;
			Process.Start(psi);
		}

		private void pictureBoxWW_Click(object sender, System.EventArgs e)
		{
			ProcessStartInfo psi = new ProcessStartInfo();
			psi.FileName = "http://www.freeearthfoundation.com/";
			psi.Verb = "open";
			psi.UseShellExecute = true;
			psi.CreateNoWindow = true;
			Process.Start(psi);
		}

		void textBoxWWFeature_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				this.startWWSearch();
				e.Handled = true;
			}
		}

        private void VEbutton_Click(object sender, EventArgs e)
        {
			lastSearchTerm = "";

            string address = VEenterTextBox.Text.Trim();

            this.listViewResults.Items.Clear();
            this.listViewResults.Columns.Clear();

            this.listViewResults.Columns.Add("Address", 120, HorizontalAlignment.Center);
            this.listViewResults.Columns.Add("Latitude", 80, HorizontalAlignment.Center);
            this.listViewResults.Columns.Add("Longitude", 80, HorizontalAlignment.Center);

            double lat1, lon1, lat2, lon2;
			this.statusBar.BeginInvoke(new SetStatusMessageDelegate(SetStatusMessage), new object[] { "Searching Virtual Earth..." });
            Search.SearchForAddress(address, out lat1, out lon1, out lat2, out lon2);

            double lat = (lat1 + lat2) / 2.0;
            double lon = (lon1 + lon2) / 2.0;

            string[] parts = { address, Convert.ToString(lat), Convert.ToString(lon) };
            ListViewItem result = new ListViewItem(parts);

			this.statusBar.BeginInvoke(new SetStatusMessageDelegate(SetStatusMessage), new object[] { "Displaying Virtual Earth search result" });
            listViewResults.Items.Add(result);
        }

        private void pictureBoxVE_Click(object sender, EventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "http://intl.local.live.com/";
            psi.Verb = "open";
            psi.UseShellExecute = true;
            psi.CreateNoWindow = true;
            Process.Start(psi);
        }

	}

    #region SEARCH
    //from Jason Fuller's VE Mobile, via Reflector
    //only slightly modified to use my basic PushPin structure
    public class Search
    {
        private Search()
        {
        }

        private static string DoSearchRequest(string searchParams)
        {
            string text1 = string.Empty;
            HttpWebRequest request1 = (HttpWebRequest)WebRequest.Create("http://local.live.com/search.ashx");
            request1.Method = "POST";
            request1.ContentType = "application/x-www-form-urlencoded";
            UTF8Encoding encoding1 = new UTF8Encoding();
            byte[] buffer1 = encoding1.GetBytes(searchParams);
            request1.ContentLength = buffer1.Length;
            try
            {
                Stream stream1 = request1.GetRequestStream();
                stream1.Write(buffer1, 0, buffer1.Length);
                stream1.Close();
                stream1 = null;
                text1 = Search.GetSearchResults(request1);
            }
            catch (WebException)
            {
            }
            return text1;
        }

        internal static string GetSearchResults(HttpWebRequest searchRequest)
        {
            string text1 = string.Empty;
            HttpWebResponse response1 = (HttpWebResponse)searchRequest.GetResponse();
            Cursor.Current = Cursors.WaitCursor;
            Stream stream1 = response1.GetResponseStream();
            Cursor.Current = Cursors.Default;
            Encoding encoding1 = Encoding.GetEncoding("utf-8");
            StreamReader reader1 = new StreamReader(stream1, encoding1);
            char[] chArray1 = new char[0x100];
            for (int num1 = reader1.Read(chArray1, 0, 0x100); num1 > 0; num1 = reader1.Read(chArray1, 0, 0x100))
            {
                string text2 = new string(chArray1, 0, num1);
                text1 = text1 + text2;
            }
            reader1.Close();
            reader1 = null;
            response1.Close();
            response1 = null;
            return text1;
        }

        public static bool SearchForAddress(string address, out double lat1, out double long1, out double lat2, out double long2)
        {
            double num1;
            long2 = num1 = 0;
            long1 = num1 = num1;
            lat2 = num1 = num1;
            lat1 = num1;
            string text1 = "a=&b=" + address + "&c=0.0&d=0.0&e=0.0&f=0.0&g=&i=&r=0";
            string text2 = Search.DoSearchRequest(text1);
            if ((text2 == null) || (text2 == string.Empty))
            {
                return false;
            }
            Regex regex1 = new Regex(@"SetViewport\((?<lat1>\S+),(?<long1>\S+),(?<lat2>\S+),(?<long2>\S+)\)");
            Match match1 = regex1.Match(text2);
            if (!match1.Success)
            {
                return false;
            }
			lat1 = double.Parse(match1.Groups["lat1"].Value, CultureInfo.InvariantCulture);
			long1 = double.Parse(match1.Groups["long1"].Value, CultureInfo.InvariantCulture);
			lat2 = double.Parse(match1.Groups["lat2"].Value, CultureInfo.InvariantCulture);
			long2 = double.Parse(match1.Groups["long2"].Value, CultureInfo.InvariantCulture);
            return true;
        }

    }
    #endregion

	public class PlaceFinderLoader : WorldWind.PluginEngine.Plugin
	{
		MenuItem m_MenuItem;
		PlaceFinder m_Form = null;
		WorldWind.WindowsControlMenuButton m_ToolbarItem = null;

		/// <summary>
		/// Plugin entry point 
		/// </summary>
		public override void Load() 
		{
			if(ParentApplication.WorldWindow.CurrentWorld != null && ParentApplication.WorldWindow.CurrentWorld.Name.IndexOf("Earth") >= 0)
			{
				m_MenuItem = new MenuItem("Place Finder");
				m_MenuItem.Click += new EventHandler(menuItemClicked);
				foreach (MenuItem menuItem in m_Application.MainMenu.MenuItems)
				{
					if (menuItem.Text.Replace("&", "") == "Edit")
					{
						menuItem.MenuItems.Add( m_MenuItem );
						break;
					}
				}
			
				m_Form = new PlaceFinder(ParentApplication.WorldWindow);
				m_Form.Closing += new CancelEventHandler(m_Form_Closing);
				m_Form.Owner = ParentApplication;
			
				m_ToolbarItem = new WorldWind.WindowsControlMenuButton(
					"PlaceFinder",
					Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Data\\Icons\\Interface\\search.png",
					m_Form);
			
				ParentApplication.WorldWindow.MenuBar.AddToolsMenuButton(m_ToolbarItem);
			}
		}

		/// <summary>
		/// Unload our plugin
		/// </summary>
		public override void Unload() 
		{
			if(m_MenuItem!=null)
			{
				foreach (MenuItem menuItem in m_Application.MainMenu.MenuItems)
				{
					if (menuItem.Text.Replace("&", "") == "Edit")
					{
						menuItem.MenuItems.Remove( m_MenuItem );
						break;
					}
				}
				m_MenuItem.Dispose();
				m_MenuItem = null;
			}

			if(m_ToolbarItem != null)
			{
				ParentApplication.WorldWindow.MenuBar.RemoveToolsMenuButton(m_ToolbarItem);
				m_ToolbarItem.Dispose();
				m_ToolbarItem = null;
			}

			if(m_Form != null)
			{
				m_Form.Dispose();
				m_Form = null;
			}
		}
	
		void menuItemClicked(object sender, EventArgs e)
		{
			if(m_Form.Visible)
			{
				m_Form.Visible = false;
				m_MenuItem.Checked = false;
			}
			else
			{
				
				m_Form.Visible = true;
				m_MenuItem.Checked = true;
			}
		}

		private void m_Form_Closing(object sender, CancelEventArgs e)
		{
			m_MenuItem.Checked = false;
		}



	}

}
