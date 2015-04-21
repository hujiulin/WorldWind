using System;

namespace ConfigurationWizard
{
	/// <summary>
	/// Summary description for WelcomePage.
	/// </summary>
	public class WelcomePage : WizardPage
	{
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label label1;

		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(WelcomePage));
			this.label1 = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(64, 96);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(408, 163);
			this.label1.TabIndex = 0;
			this.label1.Text = "This wizard will guide you through setting up NASA World Wind. \r\n\r\n You can return to this wizard at any time by clicking\nTools->Configuration Wizard";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(440, 264);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(96, 96);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pictureBox1.TabIndex = 1;
			this.pictureBox1.TabStop = false;
			// 
			// WelcomePage
			// 
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.label1);
			this.Name = "WelcomePage";
			this.SubTitle = "Welcome to NASA World Wind";
			this.Title = "Welcome";
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.pictureBox1, 0);
			this.ResumeLayout(false);

		}
	
		/// <summary>
		/// Initializes a new instance of the <see cref= "T:ConfigurationWizard.WelcomePage"/> class.
		/// </summary>
		public WelcomePage()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}
	}
}
