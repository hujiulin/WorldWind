using System;

namespace ConfigurationWizard
{
	/// <summary>
	/// Summary description for FinalPage .
	/// </summary>
	public class FinalPage : WizardPage
	{
		private System.Windows.Forms.PictureBox pictureBox1;
		internal System.Windows.Forms.CheckBox checkBoxIntro;
		private System.Windows.Forms.Label label1;

		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FinalPage));
			this.label1 = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.checkBoxIntro = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(48, 96);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(448, 136);
			this.label1.TabIndex = 0;
			this.label1.Text = "Congratulations you have succesfully configured NASA World Wind.\r\n\r\nRemember you " +
				"can return to this wizard at any time by clicking\nTools->Configuration Wizard.";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(440, 264);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(96, 96);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pictureBox1.TabIndex = 2;
			this.pictureBox1.TabStop = false;
			// 
			// checkBoxIntro
			// 
			this.checkBoxIntro.Location = new System.Drawing.Point(23, 321);
			this.checkBoxIntro.Name = "checkBoxIntro";
			this.checkBoxIntro.Size = new System.Drawing.Size(240, 24);
			this.checkBoxIntro.TabIndex = 3;
			this.checkBoxIntro.Text = "&Play World Wind introduction movie";
			// 
			// FinalPage
			// 
			this.Controls.Add(this.checkBoxIntro);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.label1);
			this.Name = "FinalPage";
			this.SubTitle = "World Wind configuration is complete";
			this.Title = "All done!";
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.pictureBox1, 0);
			this.Controls.SetChildIndex(this.checkBoxIntro, 0);
			this.ResumeLayout(false);

		}
	
		/// <summary>
		/// Initializes a new instance of the <see cref= "T:ConfigurationWizard.FinalPage"/> class.
		/// </summary>
		public FinalPage()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}
	}
}
