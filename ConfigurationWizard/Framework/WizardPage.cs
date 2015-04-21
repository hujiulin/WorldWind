using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ConfigurationWizard
{
	/// <summary>
	/// Summary description for TabPage.
	/// </summary>
//	[System.ComponentModel.Designer(typeof(WizardPageDesigner))]
	public class WizardPage : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.Panel panel1;
		private string _title;
		private string _subTitle;
		private Font _titleFont = new Font("Arial", 10, FontStyle.Bold);
		protected Wizard wizard;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:ConfigurationWizard.WizardPage"/> class.
		/// </summary>
		public WizardPage()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
		}

		[Browsable(true)]
		public string Title
		{
			get { return _title; }
			set { _title = value; }
		}

		[Browsable(true)]
		public string SubTitle
		{
			get {return _subTitle;}
			set {_subTitle = value;}
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
			if(_titleFont!=null)
			{
				_titleFont.Dispose();
				_titleFont=null;
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.panel1 = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.White;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(541, 60);
			this.panel1.TabIndex = 0;
			this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
			// 
			// WizardPage
			// 
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.panel1);
			this.Name = "WizardPage";
			this.Size = new System.Drawing.Size(541, 363);
			this.SizeChanged += new System.EventHandler(this.WizardPage_SizeChanged);
			this.ResumeLayout(false);

		}
		#endregion

		private void WizardPage_SizeChanged(object sender, System.EventArgs e)
		{
			this.Size = new System.Drawing.Size(541, 363);
		}

		private void panel1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			ControlPaint.DrawBorder3D(e.Graphics, 0, panel1.Height-2, panel1.Width, 4,Border3DStyle.Sunken);
			e.Graphics.DrawString(_title, _titleFont, Brushes.Black, 17,9);
			e.Graphics.DrawString(_subTitle, Font, Brushes.Black, 38,25);
		}
	}
}
