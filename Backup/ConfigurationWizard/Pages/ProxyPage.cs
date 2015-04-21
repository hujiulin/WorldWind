using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WorldWind;

namespace ConfigurationWizard
{
	/// <summary>
	/// Summary description for ProxyPage.
	/// </summary>
	public class ProxyPage : WizardPage
	{
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.ToolTip toolTipProxyPage;

		private System.Windows.Forms.RadioButton radioButtonUseWindowsDefaultProxy;
		private System.Windows.Forms.RadioButton radioButtonNoProxy;
		private System.Windows.Forms.RadioButton radioButtonUserDefinedProxy;
      
		private System.Windows.Forms.GroupBox groupBoxUserDefinedSettings;
		private System.Windows.Forms.TextBox textBoxProxyUrl;
		private System.Windows.Forms.CheckBox checkBoxUseProxyScript;
      
		private System.Windows.Forms.GroupBox groupBoxCredentials;
		private System.Windows.Forms.TextBox textBoxUsername;
		private System.Windows.Forms.TextBox textBoxPassword;

		private System.Windows.Forms.Label labelProxyPageInfo;
		private System.Windows.Forms.Label labelProxyUrl;
		private System.Windows.Forms.Label labelUsername;
		private System.Windows.Forms.Label labelPassword;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:ConfigurationWizard.ProxyPage"/> class.
		/// </summary>
		public ProxyPage()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
		}

		protected override void OnValidating(System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				// TODO: additional plausibility checks here
				Wizard.Settings.ProxyUrl = this.textBoxProxyUrl.Text;
				Wizard.Settings.ProxyUsername = this.textBoxUsername.Text;
				Wizard.Settings.ProxyPassword = this.textBoxPassword.Text;

				Wizard.Settings.UseWindowsDefaultProxy = radioButtonUseWindowsDefaultProxy.Checked;

				if (radioButtonUserDefinedProxy.Checked) 
				{
					Wizard.Settings.UseDynamicProxy = this.checkBoxUseProxyScript.Checked;
				}
				else 
				{
					// must set proxyurl to empty string to indicate "no proxy"
					Wizard.Settings.ProxyUrl ="";

					// no user-defined proxy means no dynamic one as well
					Wizard.Settings.UseDynamicProxy = false;
				}
			}
			catch(Exception caught)
			{
				MessageBox.Show(caught.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				e.Cancel = true;
			}
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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.radioButtonUserDefinedProxy = new System.Windows.Forms.RadioButton();
			this.radioButtonNoProxy = new System.Windows.Forms.RadioButton();
			this.radioButtonUseWindowsDefaultProxy = new System.Windows.Forms.RadioButton();
			this.groupBoxUserDefinedSettings = new System.Windows.Forms.GroupBox();
			this.checkBoxUseProxyScript = new System.Windows.Forms.CheckBox();
			this.textBoxProxyUrl = new System.Windows.Forms.TextBox();
			this.labelProxyUrl = new System.Windows.Forms.Label();
			this.labelProxyPageInfo = new System.Windows.Forms.Label();
			this.groupBoxCredentials = new System.Windows.Forms.GroupBox();
			this.textBoxPassword = new System.Windows.Forms.TextBox();
			this.labelPassword = new System.Windows.Forms.Label();
			this.textBoxUsername = new System.Windows.Forms.TextBox();
			this.labelUsername = new System.Windows.Forms.Label();
			this.toolTipProxyPage = new System.Windows.Forms.ToolTip(this.components);
			this.groupBoxUserDefinedSettings.SuspendLayout();
			this.groupBoxCredentials.SuspendLayout();
			this.SuspendLayout();
			// 
			// radioButtonUserDefinedProxy
			// 
			this.radioButtonUserDefinedProxy.Location = new System.Drawing.Point(16, 176);
			this.radioButtonUserDefinedProxy.Name = "radioButtonUserDefinedProxy";
			this.radioButtonUserDefinedProxy.Size = new System.Drawing.Size(128, 24);
			this.radioButtonUserDefinedProxy.TabIndex = 2;
			this.radioButtonUserDefinedProxy.Text = "User defined (HTTP)";
			this.toolTipProxyPage.SetToolTip(this.radioButtonUserDefinedProxy, "This will use the proxy settings defined below");
			this.radioButtonUserDefinedProxy.CheckedChanged += new System.EventHandler(this.RelevantControl_Changed);
			// 
			// radioButtonNoProxy
			// 
			this.radioButtonNoProxy.Location = new System.Drawing.Point(16, 152);
			this.radioButtonNoProxy.Name = "radioButtonNoProxy";
			this.radioButtonNoProxy.Size = new System.Drawing.Size(160, 16);
			this.radioButtonNoProxy.TabIndex = 1;
			this.radioButtonNoProxy.Text = "Don\'t use a proxy";
			this.toolTipProxyPage.SetToolTip(this.radioButtonNoProxy, "This will not use a proxy, disregarding both user-defined settings, credentials, " +
				"and browser settings.");
			this.radioButtonNoProxy.CheckedChanged += new System.EventHandler(this.RelevantControl_Changed);
			// 
			// radioButtonUseWindowsDefaultProxy
			// 
			this.radioButtonUseWindowsDefaultProxy.Checked = true;
			this.radioButtonUseWindowsDefaultProxy.Location = new System.Drawing.Point(16, 120);
			this.radioButtonUseWindowsDefaultProxy.Name = "radioButtonUseWindowsDefaultProxy";
			this.radioButtonUseWindowsDefaultProxy.Size = new System.Drawing.Size(176, 24);
			this.radioButtonUseWindowsDefaultProxy.TabIndex = 0;
			this.radioButtonUseWindowsDefaultProxy.TabStop = true;
			this.radioButtonUseWindowsDefaultProxy.Text = "Use Internet Settings (default)";
			this.toolTipProxyPage.SetToolTip(this.radioButtonUseWindowsDefaultProxy, "This will use the proxy settings configured in the browser. Credentials can still" +
				" be provided in the box below");
			this.radioButtonUseWindowsDefaultProxy.CheckedChanged += new System.EventHandler(this.RelevantControl_Changed);
			// 
			// groupBoxUserDefinedSettings
			// 
			this.groupBoxUserDefinedSettings.Controls.Add(this.checkBoxUseProxyScript);
			this.groupBoxUserDefinedSettings.Controls.Add(this.textBoxProxyUrl);
			this.groupBoxUserDefinedSettings.Controls.Add(this.labelProxyUrl);
			this.groupBoxUserDefinedSettings.Location = new System.Drawing.Point(8, 208);
			this.groupBoxUserDefinedSettings.Name = "groupBoxUserDefinedSettings";
			this.groupBoxUserDefinedSettings.Size = new System.Drawing.Size(288, 144);
			this.groupBoxUserDefinedSettings.TabIndex = 3;
			this.groupBoxUserDefinedSettings.TabStop = false;
			this.groupBoxUserDefinedSettings.Text = "User-defined settings";
			// 
			// checkBoxUseProxyScript
			// 
			this.checkBoxUseProxyScript.Location = new System.Drawing.Point(32, 88);
			this.checkBoxUseProxyScript.Name = "checkBoxUseProxyScript";
			this.checkBoxUseProxyScript.Size = new System.Drawing.Size(200, 16);
			this.checkBoxUseProxyScript.TabIndex = 2;
			this.checkBoxUseProxyScript.Text = "Use a script to determine proxy";
			this.toolTipProxyPage.SetToolTip(this.checkBoxUseProxyScript, "Check this box if your proxy is determined by the use of a script - if unsure, as" +
				"k your network administrator.");
			this.checkBoxUseProxyScript.CheckedChanged += new System.EventHandler(this.RelevantControl_Changed);
			// 
			// textBoxProxyUrl
			// 
			this.textBoxProxyUrl.Location = new System.Drawing.Point(13, 56);
			this.textBoxProxyUrl.Name = "textBoxProxyUrl";
			this.textBoxProxyUrl.Size = new System.Drawing.Size(267, 20);
			this.textBoxProxyUrl.TabIndex = 1;
			this.textBoxProxyUrl.Text = "";
			this.toolTipProxyPage.SetToolTip(this.textBoxProxyUrl, "Enter the address of the proxy to be used here");
			// 
			// labelProxyUrl
			// 
			this.labelProxyUrl.Location = new System.Drawing.Point(12, 40);
			this.labelProxyUrl.Name = "labelProxyUrl";
			this.labelProxyUrl.Size = new System.Drawing.Size(100, 16);
			this.labelProxyUrl.TabIndex = 0;
			this.labelProxyUrl.Text = "Proxy URL:";
			// 
			// labelProxyPageInfo
			// 
			this.labelProxyPageInfo.Location = new System.Drawing.Point(8, 72);
			this.labelProxyPageInfo.Name = "labelProxyPageInfo";
			this.labelProxyPageInfo.Size = new System.Drawing.Size(520, 24);
			this.labelProxyPageInfo.TabIndex = 4;
			this.labelProxyPageInfo.Text = "World Wind can use a proxy to download imagery if you cannot directly access the " +
				"internet.";
			this.labelProxyPageInfo.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// groupBoxCredentials
			// 
			this.groupBoxCredentials.Controls.Add(this.textBoxPassword);
			this.groupBoxCredentials.Controls.Add(this.labelPassword);
			this.groupBoxCredentials.Controls.Add(this.textBoxUsername);
			this.groupBoxCredentials.Controls.Add(this.labelUsername);
			this.groupBoxCredentials.Location = new System.Drawing.Point(304, 208);
			this.groupBoxCredentials.Name = "groupBoxCredentials";
			this.groupBoxCredentials.Size = new System.Drawing.Size(232, 104);
			this.groupBoxCredentials.TabIndex = 5;
			this.groupBoxCredentials.TabStop = false;
			this.groupBoxCredentials.Text = "Credentials";
			this.toolTipProxyPage.SetToolTip(this.groupBoxCredentials, "If your proxy requires authentication with a user name and password, please provi" +
				"de them in the fields in this box");
			// 
			// textBoxPassword
			// 
			this.textBoxPassword.Location = new System.Drawing.Point(8, 72);
			this.textBoxPassword.Name = "textBoxPassword";
			this.textBoxPassword.PasswordChar = '*';
			this.textBoxPassword.Size = new System.Drawing.Size(216, 20);
			this.textBoxPassword.TabIndex = 5;
			this.textBoxPassword.Text = "";
			// 
			// labelPassword
			// 
			this.labelPassword.Location = new System.Drawing.Point(8, 56);
			this.labelPassword.Name = "labelPassword";
			this.labelPassword.Size = new System.Drawing.Size(72, 16);
			this.labelPassword.TabIndex = 4;
			this.labelPassword.Text = "Password:";
			// 
			// textBoxUsername
			// 
			this.textBoxUsername.Location = new System.Drawing.Point(8, 32);
			this.textBoxUsername.Name = "textBoxUsername";
			this.textBoxUsername.Size = new System.Drawing.Size(216, 20);
			this.textBoxUsername.TabIndex = 3;
			this.textBoxUsername.Text = "";
			this.textBoxUsername.TextChanged += new System.EventHandler(this.RelevantControl_Changed);
			// 
			// labelUsername
			// 
			this.labelUsername.Location = new System.Drawing.Point(8, 16);
			this.labelUsername.Name = "labelUsername";
			this.labelUsername.Size = new System.Drawing.Size(72, 16);
			this.labelUsername.TabIndex = 2;
			this.labelUsername.Text = "User name:";
			// 
			// ProxyPage
			// 
			this.Controls.Add(this.groupBoxCredentials);
			this.Controls.Add(this.labelProxyPageInfo);
			this.Controls.Add(this.radioButtonUseWindowsDefaultProxy);
			this.Controls.Add(this.radioButtonNoProxy);
			this.Controls.Add(this.groupBoxUserDefinedSettings);
			this.Controls.Add(this.radioButtonUserDefinedProxy);
			this.Name = "ProxyPage";
			this.SubTitle = "Adjust your proxy settings";
			this.Title = "Proxy settings";
			this.Load += new System.EventHandler(this.ProxyPage_Load);
			this.Controls.SetChildIndex(this.radioButtonUserDefinedProxy, 0);
			this.Controls.SetChildIndex(this.groupBoxUserDefinedSettings, 0);
			this.Controls.SetChildIndex(this.radioButtonNoProxy, 0);
			this.Controls.SetChildIndex(this.radioButtonUseWindowsDefaultProxy, 0);
			this.Controls.SetChildIndex(this.labelProxyPageInfo, 0);
			this.Controls.SetChildIndex(this.groupBoxCredentials, 0);
			this.groupBoxUserDefinedSettings.ResumeLayout(false);
			this.groupBoxCredentials.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void UpdateEnabledControls()
		{
			this.groupBoxUserDefinedSettings.Enabled = this.radioButtonUserDefinedProxy.Checked;
			this.groupBoxCredentials.Enabled = !this.radioButtonNoProxy.Checked;
			this.textBoxPassword.Enabled = this.textBoxUsername.Text.Length > 0;
		}

		private void ProxyPage_Load(object sender, System.EventArgs e)
		{
			// Initialize from Settings
			this.textBoxProxyUrl.Text  = Wizard.Settings.ProxyUrl;
			this.textBoxUsername.Text = Wizard.Settings.ProxyUsername;
			this.textBoxPassword.Text = Wizard.Settings.ProxyPassword;
			this.checkBoxUseProxyScript.Checked = Wizard.Settings.UseDynamicProxy;
         
			if (Wizard.Settings.UseWindowsDefaultProxy) 
			{
				this.radioButtonUseWindowsDefaultProxy.Checked = true;
			}
			else 
			{
				if(Wizard.Settings.ProxyUrl.Length > 0 || Wizard.Settings.UseDynamicProxy) 
				{
					this.radioButtonUserDefinedProxy.Checked = true;
				}
				else 
				{
					this.radioButtonNoProxy.Checked = true;
				}
			}
			UpdateEnabledControls();
		}

		private void RelevantControl_Changed(object sender, System.EventArgs e)
		{
			UpdateEnabledControls();
		}
	}
}
