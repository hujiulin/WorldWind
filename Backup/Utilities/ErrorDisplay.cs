using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Utility
{
	/// <summary>
	/// Summary description for ErrorDisplay.
	/// </summary>
	public class ErrorDisplay : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TextBox errorText;
		private System.Windows.Forms.Button copyButton;
		private System.Windows.Forms.Button exitButton;
		private System.Windows.Forms.Label errorLabel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:Utility.ErrorDisplay"/> class 
		/// with default data.
		/// </summary>
		public ErrorDisplay()
		{
			InitializeComponent();
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

		
		public void errorMessages(string errorMessages)
		{
			this.errorText.Text = errorMessages;
		}

		private void exitButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void copyButton_Click(object sender, System.EventArgs e)
		{
			// HACK Use this.errorLabel.Tag to avoid the "&" in the label's Text
			string temp = this.Text + "\r\n" + this.errorLabel.Tag + "\r\n" + this.errorText.Text;
			Clipboard.SetDataObject(temp, true);
			this.exitButton.Focus();
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ErrorDisplay));
			this.exitButton = new System.Windows.Forms.Button();
			this.errorLabel = new System.Windows.Forms.Label();
			this.errorText = new System.Windows.Forms.TextBox();
			this.copyButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// exitButton
			// 
			this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.exitButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.exitButton.Location = new System.Drawing.Point(8, 336);
			this.exitButton.Name = "exitButton";
			this.exitButton.Size = new System.Drawing.Size(120, 23);
			this.exitButton.TabIndex = 0;
			this.exitButton.Text = "E&xit";
			this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
			// 
			// errorLabel
			// 
			this.errorLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.errorLabel.Location = new System.Drawing.Point(8, 8);
			this.errorLabel.Name = "errorLabel";
			this.errorLabel.Size = new System.Drawing.Size(152, 23);
			this.errorLabel.TabIndex = 2;
			this.errorLabel.Tag = "A Fatal Error has occurred:";
			this.errorLabel.Text = "&A Fatal Error has occurred:";
			// 
			// errorText
			// 
			this.errorText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.errorText.HideSelection = false;
			this.errorText.Location = new System.Drawing.Point(8, 24);
			this.errorText.Multiline = true;
			this.errorText.Name = "errorText";
			this.errorText.ReadOnly = true;
			this.errorText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.errorText.Size = new System.Drawing.Size(606, 304);
			this.errorText.TabIndex = 3;
			this.errorText.Text = "";
			// 
			// copyButton
			// 
			this.copyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.copyButton.Location = new System.Drawing.Point(168, 336);
			this.copyButton.Name = "copyButton";
			this.copyButton.Size = new System.Drawing.Size(200, 23);
			this.copyButton.TabIndex = 1;
			this.copyButton.Text = "&Copy Error Information to Clipboard";
			this.copyButton.Click += new System.EventHandler(this.copyButton_Click);
			// 
			// ErrorDisplay
			// 
			this.AcceptButton = this.exitButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.exitButton;
			this.ClientSize = new System.Drawing.Size(622, 366);
			this.Controls.Add(this.copyButton);
			this.Controls.Add(this.errorText);
			this.Controls.Add(this.errorLabel);
			this.Controls.Add(this.exitButton);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(400, 200);
			this.Name = "ErrorDisplay";
			this.Text = "World Wind Error";
			this.ResumeLayout(false);

		}
		#endregion

	}
}
