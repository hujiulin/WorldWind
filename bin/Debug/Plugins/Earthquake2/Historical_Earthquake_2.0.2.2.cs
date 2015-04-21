//----------------------------------------------------------------------------
// NAME: Historical Quake Query
// DESCRIPTION: Historical Quake Query version 2.0.2.2
// DEVELOPER: Chad Zimmerman
// WEBSITE: http://www.alteviltech.com
//----------------------------------------------------------------------------
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Globalization;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Diagnostics;
using WorldWind;
using WorldWind.Renderable;
using WorldWind.Net;

namespace CZimmerman.Plugin
{
    public class Form1 : System.Windows.Forms.Form
    {
        #region Form Controls
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox SYEAR;
        private System.Windows.Forms.TextBox EYEAR;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox SMONTH;
        private System.Windows.Forms.TextBox EMONTH;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox SDAY;
        private System.Windows.Forms.TextBox EDAY;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox LMAG;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox UMAG;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox NDEP1;
        private System.Windows.Forms.TextBox NDEP2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.TextBox CLAT;
        private System.Windows.Forms.TextBox CLON;
        private System.Windows.Forms.TextBox CRAD;
        private System.Windows.Forms.Button SUBMIT;
        private System.Windows.Forms.ComboBox SEARCHMETHOD1;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItem3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button GetLatLon;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        #endregion
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.Button buttonUpdateRectangle;
        private System.Windows.Forms.TextBox WEST;
        private System.Windows.Forms.TextBox EAST;
        private System.Windows.Forms.TextBox SOUTH;
        private System.Windows.Forms.TextBox NORTH;

        Historical_Earthquake_Query owner;

        public Form1(Historical_Earthquake_Query historicalQuery)
        {
            owner = historicalQuery;
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //Set initial values for date range
            this.SDAY.Text = DateTime.Now.Day.ToString();
            this.EDAY.Text = DateTime.Now.Day.ToString();

            this.SMONTH.Text = DateTime.Now.Month.ToString();
            this.EMONTH.Text = DateTime.Now.Month.ToString();

            this.SYEAR.Text = DateTime.Now.AddYears(-1).ToString("yyyy");
            this.EYEAR.Text = DateTime.Now.ToString("yyyy");

            this.SEARCHMETHOD1.SelectedIndex = 0;
            this.UMAG.SelectedIndex = 0;
            this.LMAG.SelectedIndex = 0;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.NDEP2 = new System.Windows.Forms.TextBox();
            this.NDEP1 = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.UMAG = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.LMAG = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.EDAY = new System.Windows.Forms.TextBox();
            this.SDAY = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.EMONTH = new System.Windows.Forms.TextBox();
            this.SMONTH = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.EYEAR = new System.Windows.Forms.TextBox();
            this.SYEAR = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SEARCHMETHOD1 = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.buttonUpdateRectangle = new System.Windows.Forms.Button();
            this.WEST = new System.Windows.Forms.TextBox();
            this.EAST = new System.Windows.Forms.TextBox();
            this.SOUTH = new System.Windows.Forms.TextBox();
            this.NORTH = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.GetLatLon = new System.Windows.Forms.Button();
            this.CRAD = new System.Windows.Forms.TextBox();
            this.CLON = new System.Windows.Forms.TextBox();
            this.CLAT = new System.Windows.Forms.TextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.SUBMIT = new System.Windows.Forms.Button();
            this.label23 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.QuakesFound = new System.Windows.Forms.Label();
            this.QuakesFoundNumber = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.TQlink5 = new System.Windows.Forms.LinkLabel();
            this.TQlink4 = new System.Windows.Forms.LinkLabel();
            this.TQlink3 = new System.Windows.Forms.LinkLabel();
            this.TQlink2 = new System.Windows.Forms.LinkLabel();
            this.TQlink1 = new System.Windows.Forms.LinkLabel();
            this.TQ5 = new System.Windows.Forms.Label();
            this.TQ3 = new System.Windows.Forms.Label();
            this.TQ4 = new System.Windows.Forms.Label();
            this.TQ2 = new System.Windows.Forms.Label();
            this.TQ1 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.MajorQuakeNumber = new System.Windows.Forms.Label();
            this.MajorQuakes = new System.Windows.Forms.Label();
            this.LargeQuakeNumber = new System.Windows.Forms.Label();
            this.LargeQuakes = new System.Windows.Forms.Label();
            this.MediumQuakeNumber = new System.Windows.Forms.Label();
            this.MediumQuakes = new System.Windows.Forms.Label();
            this.SmallQuakeNumber = new System.Windows.Forms.Label();
            this.SmallQuakes = new System.Windows.Forms.Label();
            this.buttonClear = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Linen;
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.NDEP2);
            this.groupBox1.Controls.Add(this.NDEP1);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.UMAG);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.LMAG);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.EDAY);
            this.groupBox1.Controls.Add(this.SDAY);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.EMONTH);
            this.groupBox1.Controls.Add(this.SMONTH);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.EYEAR);
            this.groupBox1.Controls.Add(this.SYEAR);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Enabled = false;
            this.groupBox1.Location = new System.Drawing.Point(8, 64);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(512, 152);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Search Limiters";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(352, 92);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 16);
            this.label1.TabIndex = 21;
            this.label1.Text = "Depth in km";
            // 
            // NDEP2
            // 
            this.NDEP2.Location = new System.Drawing.Point(264, 88);
            this.NDEP2.Name = "NDEP2";
            this.NDEP2.Size = new System.Drawing.Size(48, 20);
            this.NDEP2.TabIndex = 20;
            this.NDEP2.Text = "20";
            // 
            // NDEP1
            // 
            this.NDEP1.Location = new System.Drawing.Point(96, 88);
            this.NDEP1.Name = "NDEP1";
            this.NDEP1.Size = new System.Drawing.Size(48, 20);
            this.NDEP1.TabIndex = 19;
            this.NDEP1.Text = "0";
            // 
            // label12
            // 
            this.label12.Location = new System.Drawing.Point(176, 92);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(64, 16);
            this.label12.TabIndex = 18;
            this.label12.Text = "Max Depth:";
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(16, 92);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(64, 16);
            this.label11.TabIndex = 17;
            this.label11.Text = "Min Depth:";
            // 
            // UMAG
            // 
            this.UMAG.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.UMAG.Items.AddRange(new object[] {
													  "9.9",
													  "9.0",
													  "8.5",
													  "8.0",
													  "7.5",
													  "7.0",
													  "6.5",
													  "6.0",
													  "5.5",
													  "5.0",
													  "4.5",
													  "4.0",
													  "3.5",
													  "3.0",
													  "2.5",
													  "2.0",
													  "1.5",
													  "1.0"});
            this.UMAG.Location = new System.Drawing.Point(264, 120);
            this.UMAG.Name = "UMAG";
            this.UMAG.Size = new System.Drawing.Size(56, 21);
            this.UMAG.TabIndex = 16;
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(176, 128);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(88, 16);
            this.label10.TabIndex = 15;
            this.label10.Text = "Max Magnitude:";
            // 
            // LMAG
            // 
            this.LMAG.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LMAG.Items.AddRange(new object[] {
													  "1.0",
													  "1.5",
													  "2.0",
													  "2.5",
													  "3.0",
													  "3.5",
													  "4.0",
													  "4.5",
													  "5.0",
													  "5.5",
													  "6.0",
													  "6.5",
													  "7.0",
													  "7.5",
													  "8.0",
													  "8.5",
													  "9.0",
													  "9.5",
													  "9.9"});
            this.LMAG.Location = new System.Drawing.Point(96, 120);
            this.LMAG.Name = "LMAG";
            this.LMAG.Size = new System.Drawing.Size(56, 21);
            this.LMAG.TabIndex = 14;
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(16, 128);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(88, 16);
            this.label9.TabIndex = 12;
            this.label9.Text = "Min Magnitude:";
            // 
            // EDAY
            // 
            this.EDAY.Location = new System.Drawing.Point(424, 52);
            this.EDAY.Name = "EDAY";
            this.EDAY.Size = new System.Drawing.Size(32, 20);
            this.EDAY.TabIndex = 11;
            this.EDAY.Text = "2";
            // 
            // SDAY
            // 
            this.SDAY.Location = new System.Drawing.Point(424, 20);
            this.SDAY.Name = "SDAY";
            this.SDAY.Size = new System.Drawing.Size(32, 20);
            this.SDAY.TabIndex = 10;
            this.SDAY.Text = "1";
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(352, 56);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(64, 16);
            this.label8.TabIndex = 9;
            this.label8.Text = "Ending Day";
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(352, 24);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(72, 16);
            this.label7.TabIndex = 8;
            this.label7.Text = "Starting Day:";
            // 
            // EMONTH
            // 
            this.EMONTH.Location = new System.Drawing.Point(264, 52);
            this.EMONTH.Name = "EMONTH";
            this.EMONTH.Size = new System.Drawing.Size(48, 20);
            this.EMONTH.TabIndex = 7;
            this.EMONTH.Text = "4";
            // 
            // SMONTH
            // 
            this.SMONTH.Location = new System.Drawing.Point(264, 20);
            this.SMONTH.Name = "SMONTH";
            this.SMONTH.Size = new System.Drawing.Size(48, 20);
            this.SMONTH.TabIndex = 6;
            this.SMONTH.Text = "12";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(176, 56);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(80, 16);
            this.label6.TabIndex = 5;
            this.label6.Text = "Ending Month:";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(176, 24);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(88, 16);
            this.label5.TabIndex = 4;
            this.label5.Text = "Starting Month:";
            // 
            // EYEAR
            // 
            this.EYEAR.Location = new System.Drawing.Point(96, 52);
            this.EYEAR.Name = "EYEAR";
            this.EYEAR.Size = new System.Drawing.Size(48, 20);
            this.EYEAR.TabIndex = 3;
            this.EYEAR.Text = "2006";
            // 
            // SYEAR
            // 
            this.SYEAR.Location = new System.Drawing.Point(96, 20);
            this.SYEAR.Name = "SYEAR";
            this.SYEAR.Size = new System.Drawing.Size(48, 20);
            this.SYEAR.TabIndex = 2;
            this.SYEAR.Text = "2005";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(16, 56);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 16);
            this.label4.TabIndex = 1;
            this.label4.Text = "Ending Year:";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(16, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 16);
            this.label3.TabIndex = 0;
            this.label3.Text = "Starting Year:";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(16, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 16);
            this.label2.TabIndex = 3;
            this.label2.Text = "Search Method:";
            // 
            // SEARCHMETHOD1
            // 
            this.SEARCHMETHOD1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SEARCHMETHOD1.Items.AddRange(new object[] {
															   "1",
															   "2",
															   "3"});
            this.SEARCHMETHOD1.Location = new System.Drawing.Point(104, 16);
            this.SEARCHMETHOD1.Name = "SEARCHMETHOD1";
            this.SEARCHMETHOD1.Size = new System.Drawing.Size(64, 21);
            this.SEARCHMETHOD1.TabIndex = 15;
            this.SEARCHMETHOD1.SelectedIndexChanged += new System.EventHandler(this.SEARCHMETHOD1_SelectedIndexChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.Wheat;
            this.groupBox2.Controls.Add(this.buttonUpdateRectangle);
            this.groupBox2.Controls.Add(this.WEST);
            this.groupBox2.Controls.Add(this.EAST);
            this.groupBox2.Controls.Add(this.SOUTH);
            this.groupBox2.Controls.Add(this.NORTH);
            this.groupBox2.Controls.Add(this.label17);
            this.groupBox2.Controls.Add(this.label16);
            this.groupBox2.Controls.Add(this.label15);
            this.groupBox2.Controls.Add(this.label14);
            this.groupBox2.Controls.Add(this.label13);
            this.groupBox2.Enabled = false;
            this.groupBox2.Location = new System.Drawing.Point(8, 224);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(512, 100);
            this.groupBox2.TabIndex = 16;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Rectangular Search";
            // 
            // buttonUpdateRectangle
            // 
            this.buttonUpdateRectangle.BackColor = System.Drawing.SystemColors.Control;
            this.buttonUpdateRectangle.Location = new System.Drawing.Point(184, 48);
            this.buttonUpdateRectangle.Name = "buttonUpdateRectangle";
            this.buttonUpdateRectangle.Size = new System.Drawing.Size(80, 23);
            this.buttonUpdateRectangle.TabIndex = 9;
            this.buttonUpdateRectangle.Text = "Get from WW";
            this.buttonUpdateRectangle.Click += new System.EventHandler(this.updateViewRect);
            // 
            // WEST
            // 
            this.WEST.Location = new System.Drawing.Point(328, 68);
            this.WEST.Name = "WEST";
            this.WEST.Size = new System.Drawing.Size(80, 20);
            this.WEST.TabIndex = 8;
            this.WEST.Text = "92.20";
            // 
            // EAST
            // 
            this.EAST.Location = new System.Drawing.Point(328, 32);
            this.EAST.Name = "EAST";
            this.EAST.Size = new System.Drawing.Size(80, 20);
            this.EAST.TabIndex = 7;
            this.EAST.Text = "128.45";
            // 
            // SOUTH
            // 
            this.SOUTH.Location = new System.Drawing.Point(72, 68);
            this.SOUTH.Name = "SOUTH";
            this.SOUTH.Size = new System.Drawing.Size(80, 20);
            this.SOUTH.TabIndex = 6;
            this.SOUTH.Text = "9.00";
            // 
            // NORTH
            // 
            this.NORTH.Location = new System.Drawing.Point(72, 32);
            this.NORTH.Name = "NORTH";
            this.NORTH.Size = new System.Drawing.Size(80, 20);
            this.NORTH.TabIndex = 5;
            this.NORTH.Text = "42.90";
            // 
            // label17
            // 
            this.label17.Location = new System.Drawing.Point(272, 72);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(48, 16);
            this.label17.TabIndex = 4;
            this.label17.Text = "West";
            this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label16
            // 
            this.label16.Location = new System.Drawing.Point(264, 36);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(56, 16);
            this.label16.TabIndex = 3;
            this.label16.Text = "East";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label15
            // 
            this.label15.Location = new System.Drawing.Point(256, 8);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(248, 16);
            this.label15.TabIndex = 2;
            this.label15.Text = "Decimal Coords and negative for south and west ";
            // 
            // label14
            // 
            this.label14.Location = new System.Drawing.Point(16, 72);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(40, 16);
            this.label14.TabIndex = 1;
            this.label14.Text = "South";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label13
            // 
            this.label13.Location = new System.Drawing.Point(8, 36);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(48, 16);
            this.label13.TabIndex = 0;
            this.label13.Text = "North";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBox3
            // 
            this.groupBox3.BackColor = System.Drawing.Color.Tan;
            this.groupBox3.Controls.Add(this.GetLatLon);
            this.groupBox3.Controls.Add(this.CRAD);
            this.groupBox3.Controls.Add(this.CLON);
            this.groupBox3.Controls.Add(this.CLAT);
            this.groupBox3.Controls.Add(this.label21);
            this.groupBox3.Controls.Add(this.label20);
            this.groupBox3.Controls.Add(this.label19);
            this.groupBox3.Controls.Add(this.label18);
            this.groupBox3.Enabled = false;
            this.groupBox3.Location = new System.Drawing.Point(8, 336);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(512, 88);
            this.groupBox3.TabIndex = 17;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Radial Search";
            // 
            // GetLatLon
            // 
            this.GetLatLon.BackColor = System.Drawing.SystemColors.Control;
            this.GetLatLon.Location = new System.Drawing.Point(136, 56);
            this.GetLatLon.Name = "GetLatLon";
            this.GetLatLon.Size = new System.Drawing.Size(168, 23);
            this.GetLatLon.TabIndex = 16;
            this.GetLatLon.Text = "Get Lat/Lon From World Wind";
            this.GetLatLon.Click += new System.EventHandler(this.GetLatLon_Click);
            // 
            // CRAD
            // 
            this.CRAD.Location = new System.Drawing.Point(448, 32);
            this.CRAD.Name = "CRAD";
            this.CRAD.Size = new System.Drawing.Size(44, 20);
            this.CRAD.TabIndex = 15;
            this.CRAD.Text = "500";
            // 
            // CLON
            // 
            this.CLON.Location = new System.Drawing.Point(272, 32);
            this.CLON.Name = "CLON";
            this.CLON.Size = new System.Drawing.Size(72, 20);
            this.CLON.TabIndex = 14;
            this.CLON.Text = "-120.00";
            // 
            // CLAT
            // 
            this.CLAT.Location = new System.Drawing.Point(96, 32);
            this.CLAT.Name = "CLAT";
            this.CLAT.Size = new System.Drawing.Size(72, 20);
            this.CLAT.TabIndex = 13;
            this.CLAT.Text = "45.00";
            // 
            // label21
            // 
            this.label21.Location = new System.Drawing.Point(360, 36);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(88, 16);
            this.label21.TabIndex = 12;
            this.label21.Text = "Search Radius:";
            // 
            // label20
            // 
            this.label20.Location = new System.Drawing.Point(176, 36);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(96, 16);
            this.label20.TabIndex = 11;
            this.label20.Text = "Center Longitude:";
            // 
            // label19
            // 
            this.label19.Location = new System.Drawing.Point(8, 36);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(88, 16);
            this.label19.TabIndex = 10;
            this.label19.Text = "Center Latitude:";
            // 
            // label18
            // 
            this.label18.Location = new System.Drawing.Point(184, 8);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(320, 16);
            this.label18.TabIndex = 9;
            this.label18.Text = "Decimal Coords and negative for south and west , radius in Km";
            // 
            // SUBMIT
            // 
            this.SUBMIT.BackColor = System.Drawing.SystemColors.Control;
            this.SUBMIT.Location = new System.Drawing.Point(8, 432);
            this.SUBMIT.Name = "SUBMIT";
            this.SUBMIT.Size = new System.Drawing.Size(104, 23);
            this.SUBMIT.TabIndex = 18;
            this.SUBMIT.Text = "Query Database";
            this.SUBMIT.Click += new System.EventHandler(this.SUBMIT_Click);
            // 
            // label23
            // 
            this.label23.Location = new System.Drawing.Point(248, 8);
            this.label23.Name = "label23";
            this.label23.TabIndex = 22;
            this.label23.Text = "1 = Global Search";
            // 
            // label24
            // 
            this.label24.Location = new System.Drawing.Point(248, 24);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(128, 23);
            this.label24.TabIndex = 23;
            this.label24.Text = "2 = Rectangular Search";
            // 
            // label25
            // 
            this.label25.Location = new System.Drawing.Point(248, 40);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(104, 23);
            this.label25.TabIndex = 24;
            this.label25.Text = "3 = Radial Search";
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem1});
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem3});
            this.menuItem1.Text = "Help";
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 0;
            this.menuItem3.Text = "About";
            this.menuItem3.Click += new System.EventHandler(this.menuItem3_Click);
            // 
            // QuakesFound
            // 
            this.QuakesFound.Font = new System.Drawing.Font("Verdana", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.World);
            this.QuakesFound.Location = new System.Drawing.Point(7, 23);
            this.QuakesFound.Name = "QuakesFound";
            this.QuakesFound.Size = new System.Drawing.Size(179, 22);
            this.QuakesFound.TabIndex = 25;
            this.QuakesFound.Text = "Total Earthquakes Found:";
            // 
            // QuakesFoundNumber
            // 
            this.QuakesFoundNumber.Location = new System.Drawing.Point(200, 23);
            this.QuakesFoundNumber.Name = "QuakesFoundNumber";
            this.QuakesFoundNumber.Size = new System.Drawing.Size(56, 13);
            this.QuakesFoundNumber.TabIndex = 26;
            this.QuakesFoundNumber.Text = "0";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.TQlink5);
            this.groupBox4.Controls.Add(this.TQlink4);
            this.groupBox4.Controls.Add(this.TQlink3);
            this.groupBox4.Controls.Add(this.TQlink2);
            this.groupBox4.Controls.Add(this.TQlink1);
            this.groupBox4.Controls.Add(this.TQ5);
            this.groupBox4.Controls.Add(this.TQ3);
            this.groupBox4.Controls.Add(this.TQ4);
            this.groupBox4.Controls.Add(this.TQ2);
            this.groupBox4.Controls.Add(this.TQ1);
            this.groupBox4.Controls.Add(this.label22);
            this.groupBox4.Controls.Add(this.MajorQuakeNumber);
            this.groupBox4.Controls.Add(this.MajorQuakes);
            this.groupBox4.Controls.Add(this.LargeQuakeNumber);
            this.groupBox4.Controls.Add(this.LargeQuakes);
            this.groupBox4.Controls.Add(this.MediumQuakeNumber);
            this.groupBox4.Controls.Add(this.MediumQuakes);
            this.groupBox4.Controls.Add(this.SmallQuakeNumber);
            this.groupBox4.Controls.Add(this.SmallQuakes);
            this.groupBox4.Controls.Add(this.QuakesFoundNumber);
            this.groupBox4.Controls.Add(this.QuakesFound);
            this.groupBox4.Location = new System.Drawing.Point(10, 459);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(512, 153);
            this.groupBox4.TabIndex = 27;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Query Results";
            // 
            // TQlink5
            // 
            this.TQlink5.Location = new System.Drawing.Point(314, 123);
            this.TQlink5.Name = "TQlink5";
            this.TQlink5.Size = new System.Drawing.Size(174, 16);
            this.TQlink5.TabIndex = 44;
            this.TQlink5.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.TQlink5_LinkClicked);
            // 
            // TQlink4
            // 
            this.TQlink4.Location = new System.Drawing.Point(314, 102);
            this.TQlink4.Name = "TQlink4";
            this.TQlink4.Size = new System.Drawing.Size(174, 17);
            this.TQlink4.TabIndex = 43;
            this.TQlink4.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.TQlink4_LinkClicked);
            // 
            // TQlink3
            // 
            this.TQlink3.Location = new System.Drawing.Point(314, 82);
            this.TQlink3.Name = "TQlink3";
            this.TQlink3.Size = new System.Drawing.Size(174, 15);
            this.TQlink3.TabIndex = 42;
            this.TQlink3.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.TQlink3_LinkClicked);
            // 
            // TQlink2
            // 
            this.TQlink2.Location = new System.Drawing.Point(314, 61);
            this.TQlink2.Name = "TQlink2";
            this.TQlink2.Size = new System.Drawing.Size(174, 15);
            this.TQlink2.TabIndex = 41;
            this.TQlink2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.TQlink2_LinkClicked);
            // 
            // TQlink1
            // 
            this.TQlink1.Location = new System.Drawing.Point(314, 40);
            this.TQlink1.Name = "TQlink1";
            this.TQlink1.Size = new System.Drawing.Size(174, 15);
            this.TQlink1.TabIndex = 40;
            this.TQlink1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.TQlink1_LinkClicked);
            // 
            // TQ5
            // 
            this.TQ5.Location = new System.Drawing.Point(292, 123);
            this.TQ5.Name = "TQ5";
            this.TQ5.Size = new System.Drawing.Size(22, 16);
            this.TQ5.TabIndex = 39;
            this.TQ5.Text = "5)";
            // 
            // TQ3
            // 
            this.TQ3.Location = new System.Drawing.Point(292, 82);
            this.TQ3.Name = "TQ3";
            this.TQ3.Size = new System.Drawing.Size(22, 15);
            this.TQ3.TabIndex = 38;
            this.TQ3.Text = "3)";
            // 
            // TQ4
            // 
            this.TQ4.Location = new System.Drawing.Point(292, 102);
            this.TQ4.Name = "TQ4";
            this.TQ4.Size = new System.Drawing.Size(22, 17);
            this.TQ4.TabIndex = 37;
            this.TQ4.Text = "4)";
            // 
            // TQ2
            // 
            this.TQ2.Location = new System.Drawing.Point(292, 61);
            this.TQ2.Name = "TQ2";
            this.TQ2.Size = new System.Drawing.Size(22, 19);
            this.TQ2.TabIndex = 36;
            this.TQ2.Text = "2)";
            // 
            // TQ1
            // 
            this.TQ1.Location = new System.Drawing.Point(292, 40);
            this.TQ1.Name = "TQ1";
            this.TQ1.Size = new System.Drawing.Size(22, 18);
            this.TQ1.TabIndex = 35;
            this.TQ1.Text = "1)";
            // 
            // label22
            // 
            this.label22.Font = new System.Drawing.Font("Verdana", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.World);
            this.label22.Location = new System.Drawing.Point(292, 17);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(157, 20);
            this.label22.TabIndex = 34;
            this.label22.Text = "Top Five Earthquakes";
            // 
            // MajorQuakeNumber
            // 
            this.MajorQuakeNumber.Location = new System.Drawing.Point(200, 98);
            this.MajorQuakeNumber.Name = "MajorQuakeNumber";
            this.MajorQuakeNumber.Size = new System.Drawing.Size(52, 16);
            this.MajorQuakeNumber.TabIndex = 33;
            this.MajorQuakeNumber.Text = "0";
            // 
            // MajorQuakes
            // 
            this.MajorQuakes.Location = new System.Drawing.Point(7, 97);
            this.MajorQuakes.Name = "MajorQuakes";
            this.MajorQuakes.Size = new System.Drawing.Size(169, 18);
            this.MajorQuakes.TabIndex = 32;
            this.MajorQuakes.Text = "Major Earthquakes (7.1+):";
            // 
            // LargeQuakeNumber
            // 
            this.LargeQuakeNumber.Location = new System.Drawing.Point(200, 80);
            this.LargeQuakeNumber.Name = "LargeQuakeNumber";
            this.LargeQuakeNumber.Size = new System.Drawing.Size(58, 15);
            this.LargeQuakeNumber.TabIndex = 31;
            this.LargeQuakeNumber.Text = "0";
            // 
            // LargeQuakes
            // 
            this.LargeQuakes.Location = new System.Drawing.Point(7, 79);
            this.LargeQuakes.Name = "LargeQuakes";
            this.LargeQuakes.Size = new System.Drawing.Size(179, 23);
            this.LargeQuakes.TabIndex = 30;
            this.LargeQuakes.Text = "Large Earthquakes (5.6-7.0):";
            // 
            // MediumQuakeNumber
            // 
            this.MediumQuakeNumber.Location = new System.Drawing.Point(200, 61);
            this.MediumQuakeNumber.Name = "MediumQuakeNumber";
            this.MediumQuakeNumber.Size = new System.Drawing.Size(56, 15);
            this.MediumQuakeNumber.TabIndex = 29;
            this.MediumQuakeNumber.Text = "0";
            // 
            // MediumQuakes
            // 
            this.MediumQuakes.Location = new System.Drawing.Point(7, 61);
            this.MediumQuakes.Name = "MediumQuakes";
            this.MediumQuakes.Size = new System.Drawing.Size(187, 23);
            this.MediumQuakes.TabIndex = 28;
            this.MediumQuakes.Text = "Medium Earthquakes (3.6-5.5):";
            // 
            // SmallQuakeNumber
            // 
            this.SmallQuakeNumber.Location = new System.Drawing.Point(200, 43);
            this.SmallQuakeNumber.Name = "SmallQuakeNumber";
            this.SmallQuakeNumber.Size = new System.Drawing.Size(45, 12);
            this.SmallQuakeNumber.TabIndex = 27;
            this.SmallQuakeNumber.Text = "0";
            // 
            // SmallQuakes
            // 
            this.SmallQuakes.Location = new System.Drawing.Point(7, 43);
            this.SmallQuakes.Name = "SmallQuakes";
            this.SmallQuakes.Size = new System.Drawing.Size(179, 22);
            this.SmallQuakes.TabIndex = 0;
            this.SmallQuakes.Text = "Small Earthquakes (1.0-3.5):";
            // 
            // buttonClear
            // 
            this.buttonClear.BackColor = System.Drawing.SystemColors.Control;
            this.buttonClear.Location = new System.Drawing.Point(128, 432);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(104, 23);
            this.buttonClear.TabIndex = 25;
            this.buttonClear.Text = "Clear";
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.Color.Gainsboro;
            this.ClientSize = new System.Drawing.Size(528, 620);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.buttonClear);
            this.Controls.Add(this.label25);
            this.Controls.Add(this.label24);
            this.Controls.Add(this.label23);
            this.Controls.Add(this.SUBMIT);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.SEARCHMETHOD1);
            this.Menu = this.mainMenu1;
            this.Name = "Form1";
            this.Text = "USGS Historical Earthquake Query";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private System.Windows.Forms.LinkLabel TQlink1;
        private System.Windows.Forms.LinkLabel TQlink2;
        private System.Windows.Forms.LinkLabel TQlink3;
        private System.Windows.Forms.LinkLabel TQlink4;
        private System.Windows.Forms.LinkLabel TQlink5;
        private System.Windows.Forms.Label TQ1;
        private System.Windows.Forms.Label TQ2;
        private System.Windows.Forms.Label TQ4;
        private System.Windows.Forms.Label TQ3;
        private System.Windows.Forms.Label TQ5;
        private System.Windows.Forms.Label SmallQuakes;
        private System.Windows.Forms.Label SmallQuakeNumber;
        private System.Windows.Forms.Label MediumQuakes;
        private System.Windows.Forms.Label MediumQuakeNumber;
        private System.Windows.Forms.Label LargeQuakes;
        private System.Windows.Forms.Label LargeQuakeNumber;
        private System.Windows.Forms.Label MajorQuakes;
        private System.Windows.Forms.Label MajorQuakeNumber;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label QuakesFoundNumber;
        private System.Windows.Forms.Label QuakesFound;
        #endregion

        private void SUBMIT_Click(object sender, System.EventArgs e)
        {
            System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(RefreshEarthquakes));
            t.IsBackground = true;
            t.Start();
        }

        private void RefreshEarthquakes()
        {
            try
            {
                SUBMIT.BeginInvoke(
                    new SetControlEnabledDelegate(SetControlEnabled), new object[] { SUBMIT, false });

                DownloadData(SEARCHMETHOD1.Text, SYEAR.Text, SMONTH.Text, SDAY.Text, EYEAR.Text, EMONTH.Text, EDAY.Text, LMAG.Text, UMAG.Text, NDEP1.Text, NDEP2.Text, NORTH.Text, SOUTH.Text, EAST.Text, WEST.Text, CLAT.Text, CLON.Text, CRAD.Text);

            }
            catch (Exception ex)
            {
                Utility.Log.Write(ex);
            }
            finally
            {
                SUBMIT.BeginInvoke(
                    new SetControlEnabledDelegate(SetControlEnabled), new object[] { SUBMIT, true });
            }
        }

        delegate void SetControlEnabledDelegate(Control control, bool enabled);
        void SetControlEnabled(Control control, bool enabled)
        {
            control.Enabled = enabled;
        }

        AboutPlugin myForm2 = null;
        private void menuItem3_Click(object sender, System.EventArgs e)
        {
            if (myForm2 == null)
            {
                myForm2 = new AboutPlugin();
            }

            myForm2.Show();
        }

        private void GetLatLon_Click(object sender, System.EventArgs e)
        {
            this.CLON.Text = owner.Application.WorldWindow.DrawArgs.WorldCamera.Longitude.Degrees.ToString();
            this.CLAT.Text = owner.Application.WorldWindow.DrawArgs.WorldCamera.Latitude.Degrees.ToString();
        }



        private void SEARCHMETHOD1_SelectedIndexChanged(object sender, System.EventArgs e)
        {

            if (SEARCHMETHOD1.SelectedIndex == 0)
            {
                this.groupBox1.Enabled = true;
                this.groupBox2.Enabled = false;
                this.groupBox3.Enabled = false;
            }
            else if (SEARCHMETHOD1.SelectedIndex == 1)
            {
                this.groupBox1.Enabled = true;
                this.groupBox2.Enabled = true;
                this.groupBox3.Enabled = false;
            }
            else
            {
                this.groupBox1.Enabled = true;
                this.groupBox2.Enabled = false;
                this.groupBox3.Enabled = true;
            }

        }

        private void buttonClear_Click(object sender, System.EventArgs e)
        {
            ClearIcons();
        }

        public void DownloadData(string SEARCHMETHOD1_Text, string SYEAR_Text, string SMONTH_Text, string SDAY_Text, string EYEAR_Text, string EMONTH_Text, string EDAY_Text, string LMAG_Text, string UMAG_Text, string NDEP1_Text, string NDEP2_Text, string SLAT2_Text, string SLAT1_Text, string SLON2_Text, string SLON1_Text, string CLAT_Text, string CLON_Text, string CRAD_Text)
        {
            // Download the queried data
            string USGSUrl = "http://eqint.cr.usgs.gov/neic/cgi-bin/epic/epic.cgi?SEARCHMETHOD=" + SEARCHMETHOD1_Text + "&FILEFORMAT=6&SEARCHRANGE=HH&SYEAR=" + SYEAR_Text + "&SMONTH=" + SMONTH_Text + "&SDAY=" + SDAY_Text + "&EYEAR=" + EYEAR_Text + "&EMONTH=" + EMONTH_Text + "&EDAY=" + EDAY_Text + "&LMAG=" + LMAG_Text + "&UMAG=" + UMAG_Text + "&NDEP1=" + NDEP1_Text + "&NDEP2=" + NDEP2_Text + "&IO1=&IO2=&SLAT2=" + SLAT2_Text + "&SLAT1=" + SLAT1_Text + "&SLON2=" + SLON2_Text + "&SLON1=" + SLON1_Text + "&CLAT=" + CLAT_Text + "&CLON=" + CLON_Text + "&CRAD=" + CRAD_Text + "&SUBMIT=Submit+Search";
            //MessageBox.Show("Submited URL: "+USGSUrl);

            ClearIcons();

            owner.EQIcons.IsOn = true;

            WebDownload dl = new WebDownload(USGSUrl);
            dl.DownloadMemory(WorldWind.Net.DownloadType.Unspecified);

            CultureInfo icy = CultureInfo.InvariantCulture;

            // Create a reader to read the response from the server
            StreamReader reader = new StreamReader(dl.ContentStream);
            string line;

            // Find the 1/3 break point for the date range
            DateTime CStartDate = new DateTime(int.Parse(SYEAR_Text, icy), int.Parse(SMONTH_Text, icy), int.Parse(SDAY_Text, icy));
            DateTime CEndDate = new DateTime(int.Parse(EYEAR_Text, icy), int.Parse(EMONTH_Text, icy), int.Parse(EDAY_Text, icy));
            TimeSpan diff = TimeSpan.FromDays((CEndDate - CStartDate).TotalDays / 3);

            // Counting Earthquakes
            int QuakeCount = 0;
            int CountSmall = 0;
            int CountMedium = 0;
            int CountLarge = 0;
            int CountMajor = 0;

            ArrayList quakeList = new ArrayList();

            while ((line = reader.ReadLine()) != null)
            {

                string[] fields = line.Trim().Split(',');
                if ((fields.Length < 8) || (fields.Length > 0 && fields[0] == "Year"))
                    continue;


                // The rest is for displaying the earthquake markers
                string Q_Date = fields[1] + "/" + fields[2] + "/" + fields[0];// Pulls out the date of the Earthquake
                string Q_Time = fields[3];// Get the Earthquake even time
                float Latitude = float.Parse(fields[4], icy);// Get the Latitude
                float Longitude = float.Parse(fields[5], icy);// Get the Longitude
                float Magnitude = 0;
                if (fields[6].Trim().Length > 0)
                    Magnitude = float.Parse(fields[6], icy);// Get the Magnitude
                float Depth = float.Parse(fields[7], icy);// Get the Depth of the Earthquake


                DateTime DateCompare = new DateTime(int.Parse(fields[0], icy), int.Parse(fields[1], icy), int.Parse(fields[2], icy));

                QuakeItem curQuakeItem = new QuakeItem();
                curQuakeItem.Depth = Depth;
                curQuakeItem.Latitude = Latitude;
                curQuakeItem.Longitude = Longitude;
                curQuakeItem.Magnitude = Magnitude;
                curQuakeItem.Q_Date = Q_Date;
                curQuakeItem.Q_Time = Q_Time;
                curQuakeItem.DateCompare = DateCompare;

                quakeList.Add(curQuakeItem);

                int IconWidth = 20;
                int IconHeight = 20;
                string bitmapShape = "circle";
                string bitmapColor = "red";

                if (DateCompare < (CStartDate + diff))
                    bitmapColor = "green";
                else if (DateCompare > (CEndDate - diff))
                    bitmapColor = "red";
                else
                    bitmapColor = "yellow";


                if (Depth <= 40) // Shallow Depth
                {
                    bitmapShape = "circle";
                }
                else if ((Depth == 41) || (Depth <= 70)) // Medium Depth
                {
                    bitmapShape = "triangle";
                }
                else if ((Depth == 71) || (Depth <= 200)) // Deep Depth
                {
                    bitmapShape = "square";
                }
                else // Ultra Deep
                {
                    bitmapShape = "star";
                }

                if (Magnitude <= 3.5) // Small Icons
                {
                    CountSmall = CountSmall + 1;
                    IconWidth = 20;
                    IconHeight = 20;
                }
                else if ((Magnitude == 3.6) || (Magnitude <= 5.5)) // Medium Icons
                {
                    CountMedium = CountMedium + 1;
                    IconWidth = 33;
                    IconHeight = 33;
                }
                else if ((Magnitude == 5.6) || (Magnitude <= 7.0)) // Large Icons
                {
                    CountLarge = CountLarge + 1;
                    IconWidth = 48;
                    IconHeight = 48;
                }
                else // Major Icons
                {
                    CountMajor = CountMajor + 1;
                    IconWidth = 60;
                    IconHeight = 60;
                }

                string bitmapFileName = String.Format("{0}_{1}.png", bitmapShape, bitmapColor);
                string bitmapFullPath = Path.Combine(owner.PluginDirectory, bitmapFileName);

                // Increase the count
                QuakeCount = QuakeCount + 1;

                // Disply the icon and related data
                WorldWind.Renderable.Icon ic = new WorldWind.Renderable.Icon(
                    Q_Date + " - " + Magnitude, // name
                    Latitude,
                    Longitude,
                    0); // DistanceAboveSurface
                ic.Description = "Earthquake Details: \n  Date: " + Q_Date + "\n  Time: " + Q_Time + " UTC\n  Magnitude: " + Magnitude + "\n  Depth: " + Depth + " Km"; // description
                ic.TextureFileName = bitmapFullPath; // textureFullPath
                ic.Width = IconWidth;  // IconWidthPixels
                ic.Height = IconHeight;  //IconHeightPixels    
                ic.ClickableActionURL = ""; //ClickableUrl

                // Add the icon to the layer
                owner.EQIcons.AddIcon(ic);
            }

            // show count 
            //MessageBox.Show("Number of Earthquakes Found: "+ QuakeCount);
            this.QuakesFoundNumber.Text = QuakeCount.ToString();
            this.MajorQuakeNumber.Text = CountMajor.ToString();
            this.LargeQuakeNumber.Text = CountLarge.ToString();
            this.MediumQuakeNumber.Text = CountMedium.ToString();
            this.SmallQuakeNumber.Text = CountSmall.ToString();

            //check the top five quakes
            quakesMagSortList.Clear();

            foreach (QuakeItem curQuakeItem in quakeList)
            {
                bool quakeAdded = false;
                for (int i = 0; i < quakesMagSortList.Count; i++)
                {
                    QuakeItem quakeItem = (QuakeItem)quakesMagSortList[i];

                    if (curQuakeItem.Magnitude >= quakeItem.Magnitude)
                    {
                        quakesMagSortList.Insert(i, curQuakeItem);
                        quakeAdded = true;
                        break;
                    }
                }

                if (!quakeAdded)
                {
                    quakesMagSortList.Add(curQuakeItem);
                }
            }

            int numberTopQuakes = 5;
            if (quakesMagSortList.Count < numberTopQuakes)
                numberTopQuakes = quakesMagSortList.Count;

            for (int i = 0; i < numberTopQuakes; i++)
            {
                QuakeItem quakeItem = (QuakeItem)quakesMagSortList[i];
                switch (i)
                {
                    case 0:
                        TQlink1.BeginInvoke(new SetControlTextDelegate(SetControlText), new object[] { TQlink1, string.Format("{0} - {1}", quakeItem.Q_Date, quakeItem.Magnitude) });
                        break;
                    case 1:
                        TQlink2.BeginInvoke(new SetControlTextDelegate(SetControlText), new object[] { TQlink2, string.Format("{0} - {1}", quakeItem.Q_Date, quakeItem.Magnitude) });
                        break;
                    case 2:
                        TQlink3.BeginInvoke(new SetControlTextDelegate(SetControlText), new object[] { TQlink3, string.Format("{0} - {1}", quakeItem.Q_Date, quakeItem.Magnitude) });
                        break;
                    case 3:
                        TQlink4.BeginInvoke(new SetControlTextDelegate(SetControlText), new object[] { TQlink4, string.Format("{0} - {1}", quakeItem.Q_Date, quakeItem.Magnitude) });
                        break;
                    case 4:
                        TQlink5.BeginInvoke(new SetControlTextDelegate(SetControlText), new object[] { TQlink5, string.Format("{0} - {1}", quakeItem.Q_Date, quakeItem.Magnitude) });
                        break;
                }
            }

            if (numberTopQuakes < 5)
            {
                if (numberTopQuakes == 4)
                    TQlink4.Text = "";

                if (numberTopQuakes < 4)
                    TQlink3.Text = "";

                if (numberTopQuakes < 3)
                    TQlink2.Text = "";

                if (numberTopQuakes < 1)
                    TQlink1.Text = "";
            }

        }

        ArrayList quakesMagSortList = new ArrayList();

        private delegate void SetControlTextDelegate(Control control, string msg);
        private void SetControlText(Control control, string msg)
        {
            control.Text = msg;
        }

        struct QuakeItem
        {
            public System.DateTime DateCompare;
            public string Q_Date;
            public string Q_Time;
            public float Latitude;
            public float Longitude;
            public float Magnitude;
            public float Depth;
        }

        private void updateViewRect(object sender, System.EventArgs e)
        {
            double centerLat = owner.Application.WorldWindow.DrawArgs.WorldCamera.Latitude.Degrees;
            double centerLon = owner.Application.WorldWindow.DrawArgs.WorldCamera.Longitude.Degrees;

            double viewRange = owner.Application.WorldWindow.DrawArgs.WorldCamera.ViewRange.Degrees;

            double north = centerLat + viewRange * 0.05;
            double south = centerLat - viewRange * 0.05;
            double west = centerLon - viewRange * 0.05;
            double east = centerLon + viewRange * 0.05;

            if (north > 90)
                north = 90;
            if (south < -90)
                south = -90;
            if (west < -180)
                west = -180;
            if (east > 180)
                east = 180;

            NORTH.Text = north.ToString();
            EAST.Text = east.ToString();
            SOUTH.Text = south.ToString();
            WEST.Text = west.ToString();
        }

        private void ClearIcons()
        {
            try
            {
                while (owner.EQIcons.ChildObjects.Count > 0)
                {
                    RenderableObject ro = (RenderableObject)owner.EQIcons.ChildObjects[0];
                    owner.EQIcons.ChildObjects.RemoveAt(0);
                    ro.Dispose();
                }
            }
            catch { }
        }

        private void TQlink1_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                if (this.quakesMagSortList.Count > 0)
                {
                    QuakeItem item = (QuakeItem)this.quakesMagSortList[0];
                    owner.WorldWindow.GotoLatLon(item.Latitude, item.Longitude);
                }
            }
            catch { }
        }

        private void TQlink2_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                if (this.quakesMagSortList.Count > 1)
                {
                    QuakeItem item = (QuakeItem)this.quakesMagSortList[1];
                    owner.WorldWindow.GotoLatLon(item.Latitude, item.Longitude);
                }
            }
            catch { }
        }

        private void TQlink3_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                if (this.quakesMagSortList.Count > 0)
                {
                    QuakeItem item = (QuakeItem)this.quakesMagSortList[2];
                    owner.WorldWindow.GotoLatLon(item.Latitude, item.Longitude);
                }
            }
            catch { }
        }

        private void TQlink4_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                if (this.quakesMagSortList.Count > 0)
                {
                    QuakeItem item = (QuakeItem)this.quakesMagSortList[3];
                    owner.WorldWindow.GotoLatLon(item.Latitude, item.Longitude);
                }
            }
            catch { }
        }

        private void TQlink5_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                if (this.quakesMagSortList.Count > 0)
                {
                    QuakeItem item = (QuakeItem)this.quakesMagSortList[4];
                    owner.WorldWindow.GotoLatLon(item.Latitude, item.Longitude);
                }
            }
            catch { }
        }
    }

    // Load in About Form
    public class AboutPlugin : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public AboutPlugin()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label1.Location = new System.Drawing.Point(8, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(336, 23);
            this.label1.TabIndex = 1;
            this.label1.Text = "About Historical Query Earthquake Plug-In";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label2.Location = new System.Drawing.Point(16, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(168, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "Author: Chad Zimmerman";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label3.Location = new System.Drawing.Point(16, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(192, 16);
            this.label3.TabIndex = 3;
            this.label3.Text = "Website: http://www.alteviltech.com/";
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label4.Location = new System.Drawing.Point(16, 88);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(296, 16);
            this.label4.TabIndex = 4;
            this.label4.Text = "Current Version: 2.0.2.1 - Historical Data Query";
            // 
            // AboutPlugin
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(352, 138);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "AboutPlugin";
            this.Text = "About HistEQ";
            this.ResumeLayout(false);

        }
        #endregion


    }

    /// <summary>
    /// Tutorial1
    /// </summary>
    public class Historical_Earthquake_Query : WorldWind.PluginEngine.Plugin
    {
        System.Windows.Forms.MenuItem menuItem;
        //System.Windows.Forms.MenuItem menuItem2;

        public WorldWindow WorldWindow = null;
        public Icons EQIcons = new Icons("Historical EarthQuake Icons");

        public override void Load()
        {
            if (ParentApplication.WorldWindow.CurrentWorld != null && ParentApplication.WorldWindow.CurrentWorld.Name.IndexOf("Earth") >= 0)
            {
                menuItem = new System.Windows.Forms.MenuItem();
                menuItem.Text = "Historical Earthquake Query";
                menuItem.Click += new System.EventHandler(menuItem_Click);
                ParentApplication.PluginsMenu.MenuItems.Add(menuItem);

                WorldWindow = m_Application.WorldWindow;
                EQIcons.IsOn = false;
                m_Application.WorldWindow.CurrentWorld.RenderableObjects.Add(EQIcons);
                base.Load();
            }
        }

        public override void Unload()
        {
            // Clean up, remove menu item
            ParentApplication.PluginsMenu.MenuItems.Remove(menuItem);

            m_Application.WorldWindow.CurrentWorld.RenderableObjects.Remove(EQIcons);

            base.Unload();
        }

        void menuItem_Click(object sender, EventArgs e)
        {
            // Fired when user clicks our main menu item.
            Form1 myForm = new Form1(this);
            myForm.Show();
        }
    }
}

