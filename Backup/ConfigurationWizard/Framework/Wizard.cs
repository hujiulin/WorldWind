using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using WorldWind;

namespace ConfigurationWizard
{
	/// <summary>
	/// The Configuration Wizard
	/// </summary>
	public class Wizard : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button buttonNext;
		private System.Windows.Forms.Button buttonBack;
		private System.Windows.Forms.Button buttonCancel;

		private string title = "Wizard";
		private string abortMsg = "The wizard has not been completed.  Are you sure you want to abort?";
		private ArrayList wizardPages;
		private Image headingBitmap;
		private WizardPage currentPage;
		private int currentPageIndex=-1;
		private bool isComplete;
		internal static WorldWindSettings Settings;

		/// <summary>
		/// The window title on the wizard
		/// </summary>
		public override string Text
		{
			get { return title; }
			set { title = value; }
		}

		/// <summary>
		/// The bitmap to be displayed on the upper part of the wizard form.
		/// </summary>
		public Image HeadingBitmap
		{
			get { return headingBitmap; }
			set { headingBitmap = value; }
		}

		/// <summary>
		/// The list of WizardPage derived pages.
		/// </summary>
		public ArrayList WizardPages
		{
			get { return wizardPages; }
			set { wizardPages = value; }
		}

		/// <summary>
		/// Message displayed to user when he closes the wizard before it is finished.
		/// </summary>
		public string AbortMessage
		{
			get { return abortMsg; }
			set { abortMsg = value; }
		}

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:ConfigurationWizard.WizardPage"/> class.
		/// Initializes settings, message and pages.
		/// </summary>
		/// <param name="settings">The WorldWindSettings to use</param>
		public Wizard( WorldWindSettings settings )
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			Wizard.Settings = settings;
			wizardPages = new ArrayList();

			this.Text = "World Wind welcome screen";
			this.AbortMessage = "Are you sure you want to cancel the wizard?";
			AddPage( new WelcomePage() );
			AddPage( new CachePage() );
			AddPage( new ProxyPage() );
			AddPage( new AtmospherePage() );
			AddPage( new FinalPage() );
		}

		/// <summary>
		/// Adds a page to the wizard
		/// </summary>
		/// <param name="page">The page to add</param>
		public void AddPage( WizardPage page )
		{
			this.WizardPages.Add( page );
		}

		/// <summary>
		/// Moves to a specific page
		/// </summary>
		/// <param name="pageindex">The index of the page to move to</param>
		public void GotoPage( int pageindex )
		{
			if (currentPageIndex == pageindex)
				return;

			if (this.currentPage!=null)
				this.Controls.Remove(currentPage);
			currentPage = (WizardPage)wizardPages[pageindex];
			this.Controls.Add(currentPage);
			currentPageIndex = pageindex;

			UpdateControlStates();
			this.Invalidate();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		/// <summary>
		/// Indicates whether the wizard is displaying the last page
		/// </summary>
		bool isOnLastPage
		{
			get
			{
				return currentPageIndex>=wizardPages.Count-1;
			}
		}

		/// <summary>
		/// Indicates whether the wizard is displaying the first page
		/// </summary>
		bool isOnFirstPage
		{
			get
			{
				return currentPageIndex<=0;
			}
		}

		/// <summary>
		/// Updates the button and window text
		/// </summary>
		private void UpdateControlStates()
		{
			if (isOnLastPage)
			{
				buttonNext.Text = "&Finish";
				isComplete = true;
			}
			else
			{
				buttonNext.Text = "&Next >";
				isComplete = false;
			}

			buttonBack.Enabled = !isOnFirstPage;
			base.Text = string.Format("{0} ({1}/{2})",
				title, currentPageIndex+1, wizardPages.Count );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.buttonNext = new System.Windows.Forms.Button();
			this.buttonBack = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// buttonNext
			// 
			this.buttonNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonNext.Location = new System.Drawing.Point(335, 382);
			this.buttonNext.Name = "buttonNext";
			this.buttonNext.Size = new System.Drawing.Size(82, 27);
			this.buttonNext.TabIndex = 1;
			this.buttonNext.Text = "&Next >";
			this.buttonNext.Click += new System.EventHandler(this.buttonNext_Click);
			// 
			// buttonBack
			// 
			this.buttonBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonBack.Location = new System.Drawing.Point(247, 382);
			this.buttonBack.Name = "buttonBack";
			this.buttonBack.Size = new System.Drawing.Size(82, 27);
			this.buttonBack.TabIndex = 2;
			this.buttonBack.Text = "< &Back";
			this.buttonBack.Click += new System.EventHandler(this.buttonBack_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(449, 382);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(82, 27);
			this.buttonCancel.TabIndex = 3;
			this.buttonCancel.Text = "&Cancel";
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// Wizard
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(540, 414);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonBack);
			this.Controls.Add(this.buttonNext);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "Wizard";
			this.ShowInTaskbar = false;
			this.Text = "Configuration Wizard";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.WizardBase_Closing);
			this.ResumeLayout(false);

		}
		#endregion

		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
		{
			int spacing=3;
			ControlPaint.DrawBorder3D(e.Graphics, spacing, 371,ClientSize.Width-2*spacing,3,Border3DStyle.Sunken);
			base.OnPaint(e);
			if(headingBitmap!=null)
				e.Graphics.DrawImageUnscaled(headingBitmap, 0,0);
		}

		/// <summary>
		/// Called when the user clicks the 'Cancel' button, closes the wizard
		/// </summary>
		private void buttonCancel_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Called when the wizard is closing, asks for confirmation from the user
		/// </summary>
		private void WizardBase_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (isComplete)
				return;

			if( MessageBox.Show(abortMsg,"Abort",MessageBoxButtons.YesNo,
				MessageBoxIcon.Question, MessageBoxDefaultButton.Button2 ) != DialogResult.Yes)
			{
				e.Cancel = true;
			}
		}

		/// <summary>
		/// Called when the user clicks the 'Back' button, goes to the previous page
		/// </summary>
		private void buttonBack_Click(object sender, System.EventArgs e)
		{
			GotoPage(currentPageIndex-1);
		}

		/// <summary>
		/// Called when the user clicks the 'Next' button, goes to the next page
		/// </summary>
		private void buttonNext_Click(object sender, System.EventArgs e)
		{
			if(!currentPage.Validate())
				return;
			if(isOnLastPage)
			{
				// Show the WW Tour to the user
				if (((FinalPage)this.wizardPages[wizardPages.Count-1]).checkBoxIntro.Checked == true)
				{
					string TourPath = Path.Combine(Wizard.Settings.WorldWindDirectory, @"Data\Documentation\WW_Tour.exe");
					if (File.Exists(TourPath))
						System.Diagnostics.Process.Start(TourPath);
					else
                        System.Diagnostics.Process.Start("http://www.earthissquare.com/WorldWind/index.php?title=World_Wind_Tours");
				}

				Close();
				return;
			}

			GotoPage(currentPageIndex+1);
		}

		/// <summary>
		/// Called when the wizard initializes, loads the first page
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad (e);

			GotoPage(0);
			UpdateControlStates();
		}
	}
}
