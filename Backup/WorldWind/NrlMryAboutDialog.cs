using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace NASA.Plugins
{
	/// <summary>
	/// Summary description for NrlMryAboutDialog.
	/// </summary>
	public class NrlMryAboutDialog : System.Windows.Forms.Form
	{
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.LinkLabel linkLabel1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public NrlMryAboutDialog()
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(NrlMryAboutDialog));
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(8, 8);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(128, 128);
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(136, 56);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(328, 32);
			this.label1.TabIndex = 1;
			this.label1.Text = "This plugin was developed independantly from Naval Research Labs, Monterey.   Add" +
				"itional functionality added by Dan Deneau.";
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label2.Location = new System.Drawing.Point(136, 8);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(328, 23);
			this.label2.TabIndex = 2;
			this.label2.Text = "Naval Reseach Labs, Monterey \"Real-Time\" Weather Plugin";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(136, 32);
			this.label3.Name = "label3";
			this.label3.TabIndex = 3;
			this.label3.Text = "Version 1.1";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(136, 88);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(224, 23);
			this.label4.TabIndex = 4;
			this.label4.Text = "For questions or comments, please contact:";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(136, 112);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(80, 23);
			this.label5.TabIndex = 5;
			this.label5.Text = "Chris Maxwell";
			// 
			// linkLabel1
			// 
			this.linkLabel1.Location = new System.Drawing.Point(216, 112);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(128, 23);
			this.linkLabel1.TabIndex = 6;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "cmaxwell@arc.nasa.gov";
			// 
			// NrlMryAboutDialog
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(472, 142);
			this.Controls.Add(this.linkLabel1);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.pictureBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "NrlMryAboutDialog";
			this.Text = "NRL Monterey Weather Plugin";
			this.ResumeLayout(false);

		}
		#endregion
	}
}
