using System;
using System.Windows.Forms;

namespace ConfigurationWizard
{
	/// <summary>
	/// Summary description for CachePage.
	/// </summary>
	public class CachePage : WizardPage
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.NumericUpDown cacheSizeMegaBytes;
		private System.Windows.Forms.Label label4;

		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.cacheSizeMegaBytes = new System.Windows.Forms.NumericUpDown();
			this.label4 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.cacheSizeMegaBytes)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(24, 96);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(120, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Maximum cache size:";
			// 
			// cacheSizeMegaBytes
			// 
			this.cacheSizeMegaBytes.Location = new System.Drawing.Point(136, 96);
			this.cacheSizeMegaBytes.Maximum = new System.Decimal(new int[] {
																			   1048576,
																			   0,
																			   0,
																			   0});
			this.cacheSizeMegaBytes.Minimum = new System.Decimal(new int[] {
																			   100,
																			   0,
																			   0,
																			   0});
			this.cacheSizeMegaBytes.Name = "cacheSizeMegaBytes";
			this.cacheSizeMegaBytes.Size = new System.Drawing.Size(80, 20);
			this.cacheSizeMegaBytes.TabIndex = 3;
			this.cacheSizeMegaBytes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.cacheSizeMegaBytes.Value = new System.Decimal(new int[] {
																			 500,
																			 0,
																			 0,
																			 0});
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(216, 96);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(205, 16);
			this.label4.TabIndex = 6;
			this.label4.Text = "Megabytes (1024 MB = 1 Gigabyte)";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(24, 72);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(496, 16);
			this.label6.TabIndex = 8;
			this.label6.Text = "Once this folder reaches its maximum size the cached files that are the oldest w" +
				"ill be removed.";
			this.label6.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// CachePage
			// 
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.cacheSizeMegaBytes);
			this.Controls.Add(this.label1);
			this.Name = "CachePage";
			this.SubTitle = "Adjust World Wind\'s Cache settings";
			this.Title = "Cache";
			this.Load += new System.EventHandler(this.CachePage_Load);
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.cacheSizeMegaBytes, 0);
			this.Controls.SetChildIndex(this.label4, 0);
			this.Controls.SetChildIndex(this.label6, 0);
			((System.ComponentModel.ISupportInitialize)(this.cacheSizeMegaBytes)).EndInit();
			this.ResumeLayout(false);

		}
	
		/// <summary>
		/// Initializes a new instance of the <see cref= "T:ConfigurationWizard.CachePage"/> class.
		/// </summary>
		public CachePage()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
		}

		protected override void OnValidating(System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				Wizard.Settings.CacheSizeMegaBytes = (int)this.cacheSizeMegaBytes.Value;
			}
			catch(Exception caught)
			{
				MessageBox.Show(caught.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				e.Cancel = true;
			}
		}

		private void CachePage_Load(object sender, System.EventArgs e)
		{
			this.cacheSizeMegaBytes.Value = Wizard.Settings.CacheSizeMegaBytes;
		}
	}
}
