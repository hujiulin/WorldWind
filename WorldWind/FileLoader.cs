using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using WorldWind;
using WorldWind.Net;

namespace WorldWind
{
	public class FileLoader : Form
	{
		private MainApplication mainapp;
		private System.Windows.Forms.Label lblFileTextBox;
		private System.Windows.Forms.TextBox tbFileName;
		private System.Windows.Forms.Button btnChooseFile;
		private System.Windows.Forms.Button btnLoad;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private Label label1;


		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
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
			this.lblFileTextBox = new System.Windows.Forms.Label();
			this.tbFileName = new System.Windows.Forms.TextBox();
			this.btnChooseFile = new System.Windows.Forms.Button();
			this.btnLoad = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lblFileTextBox
			// 
			this.lblFileTextBox.AutoSize = true;
			this.lblFileTextBox.Location = new System.Drawing.Point(9, 9);
			this.lblFileTextBox.Name = "lblFileTextBox";
			this.lblFileTextBox.Size = new System.Drawing.Size(126, 13);
			this.lblFileTextBox.TabIndex = 0;
			this.lblFileTextBox.Text = "Enter URL or choose file:";
			// 
			// tbFileName
			// 
			this.tbFileName.Location = new System.Drawing.Point(12, 25);
			this.tbFileName.Name = "tbFileName";
			this.tbFileName.Size = new System.Drawing.Size(333, 20);
			this.tbFileName.TabIndex = 1;
			// 
			// btnChooseFile
			// 
			this.btnChooseFile.Location = new System.Drawing.Point(351, 24);
			this.btnChooseFile.Name = "btnChooseFile";
			this.btnChooseFile.Size = new System.Drawing.Size(71, 22);
			this.btnChooseFile.TabIndex = 2;
			this.btnChooseFile.Text = "Choose...";
			this.btnChooseFile.UseVisualStyleBackColor = true;
			this.btnChooseFile.Click += new System.EventHandler(this.btnChooseFile_Click);
			// 
			// btnLoad
			// 
			this.btnLoad.Location = new System.Drawing.Point(266, 76);
			this.btnLoad.Name = "btnLoad";
			this.btnLoad.Size = new System.Drawing.Size(75, 23);
			this.btnLoad.TabIndex = 4;
			this.btnLoad.Text = "Load";
			this.btnLoad.UseVisualStyleBackColor = true;
			this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(347, 76);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 5;
			this.btnCancel.Text = "Close";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// openFileDialog
			// 
			this.openFileDialog.Filter = "WW add-ons (*.xml)|*.xml|WW plugins (*.cs)|*.cs|QuickInstall packages (*.zip)|*.z" +
				"ip";
			this.openFileDialog.Title = "Choose File";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(9, 48);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(286, 13);
			this.label1.TabIndex = 6;
			this.label1.Text = "File will be copied to the World Wind directory upon loading";
			// 
			// FileLoader
			// 
			this.AcceptButton = this.btnLoad;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(434, 108);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnLoad);
			this.Controls.Add(this.btnChooseFile);
			this.Controls.Add(this.tbFileName);
			this.Controls.Add(this.lblFileTextBox);
			this.MaximizeBox = false;
			this.Name = "FileLoader";
			this.Text = "Load File";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		
		
		public FileLoader(MainApplication app)
		{
			this.mainapp = app;

			InitializeComponent();
		}

		private void btnChooseFile_Click(object sender, EventArgs e)
		{
			if (openFileDialog.ShowDialog() != DialogResult.Cancel)
				tbFileName.Text = openFileDialog.FileName;
		}

		private void btnLoad_Click(object sender, EventArgs e)
		{
			if (tbFileName.Text == "")
				return;


			if (tbFileName.Text.EndsWith(".zip"))
			{
				//MessageBox.Show("Found zip");
				mainapp.InstallFromZip(tbFileName.Text);
				this.Close();
			}

			if (tbFileName.Text.Trim().StartsWith(@"http://"))
			{
				//MessageBox.Show("Found url");
				WebDownload dl = new WebDownload(tbFileName.Text);
				string[] urlList = tbFileName.Text.Trim().Split('/');
				string fileName = urlList[urlList.Length - 1];

				if (fileName.EndsWith(".xml"))
				{
					//MessageBox.Show("Found web xml");
					string dlPath = Path.Combine(MainApplication.Settings.ConfigPath, mainapp.WorldWindow.CurrentWorld.ToString());
					dlPath = Path.Combine(dlPath, fileName);
					
					try
					{
						dl.DownloadFile(dlPath);
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.Message, "Download error");
						return;
					}

					mainapp.LoadAddon(dlPath);
					this.Close();
				}
				else if (fileName.EndsWith(".cs"))
				{
					//MessageBox.Show("Found web cs");
					string dlPath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "//Plugins//");
					dlPath = Path.Combine(dlPath, fileName);

					try
					{
						dl.DownloadFile(dlPath);
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.Message, "Download error");
						return;
					}

					MessageBox.Show("TODO: Load plugin here...");
					this.Close();
				}
			}
			else
			{
				if (!File.Exists(tbFileName.Text))
				{
					MessageBox.Show(tbFileName.Text + " does not exist", "Load error");
					return;
				}

				FileInfo fi = new FileInfo(tbFileName.Text);
				string name = fi.Name;

				if (name.EndsWith(".xml"))
				{
					//MessageBox.Show("Found local xml");
					string dlPath = Path.Combine(MainApplication.Settings.ConfigPath, mainapp.WorldWindow.CurrentWorld.ToString());
					dlPath = Path.Combine(dlPath, name);

					if(!File.Exists(dlPath))
					{
						try
						{
							File.Copy(tbFileName.Text, dlPath);
						}
						catch (Exception ex)
						{
							MessageBox.Show(ex.Message, "Copy error");
							return;
						}
					}

					mainapp.LoadAddon(dlPath);
					this.Close();
				}
				else if (tbFileName.Text.EndsWith(".cs"))
				{
					//MessageBox.Show("Found local cs");
					string dlPath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "//Plugins//");
					dlPath = Path.Combine(dlPath, name);

					if (!File.Exists(dlPath))
					{
						try
						{
							File.Copy(tbFileName.Text, dlPath);
						}
						catch (Exception ex)
						{
							MessageBox.Show(ex.Message, "Copy error");
							return;
						}
					}

					MessageBox.Show("TODO: Load plugin here...");
					this.Close();
				}
			}
				

		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.Close();
		}

	}
}