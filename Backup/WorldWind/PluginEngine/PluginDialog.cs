using System;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace WorldWind.PluginEngine
{
	/// <summary>
	/// Plugin manager dialog.
	/// </summary>
	public class PluginDialog : System.Windows.Forms.Form
	{
		private PluginListView listView;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private PluginCompiler compiler;
		private System.Windows.Forms.Button buttonLoad;
		private System.Windows.Forms.Button buttonUnload;
		private System.Windows.Forms.ImageList imageList;
		private System.Windows.Forms.TextBox description;
		private System.Windows.Forms.LinkLabel webSite;
		private System.Windows.Forms.Label labelDescription;
		private System.Windows.Forms.Label labelWebSite;
		private System.Windows.Forms.Label labelDeveloper;
		private System.Windows.Forms.Label developer;
		private System.Windows.Forms.Button buttonInstall;
		private System.Windows.Forms.Button buttonUninstall;
		private System.ComponentModel.IContainer components;

		/// <summary>
		/// On/Off images for items.
		/// </summary>
		public ImageList ImageList
		{
			get
			{
				return imageList;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.PluginEngine.PluginDialog"/> class.
		/// </summary>
		/// <param name="compiler"></param>
		public PluginDialog(PluginCompiler compiler)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.compiler = compiler;
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
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(PluginDialog));
			this.listView = new WorldWind.PluginEngine.PluginListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.buttonLoad = new System.Windows.Forms.Button();
			this.buttonUnload = new System.Windows.Forms.Button();
			this.imageList = new System.Windows.Forms.ImageList(this.components);
			this.description = new System.Windows.Forms.TextBox();
			this.webSite = new System.Windows.Forms.LinkLabel();
			this.labelDescription = new System.Windows.Forms.Label();
			this.labelWebSite = new System.Windows.Forms.Label();
			this.labelDeveloper = new System.Windows.Forms.Label();
			this.developer = new System.Windows.Forms.Label();
			this.buttonInstall = new System.Windows.Forms.Button();
			this.buttonUninstall = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// listView
			// 
			this.listView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																																							 this.columnHeader1,
																																							 this.columnHeader2});
			this.listView.FullRowSelect = true;
			this.listView.GridLines = true;
			this.listView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listView.Location = new System.Drawing.Point(8, 8);
			this.listView.Name = "listView";
			this.listView.Size = new System.Drawing.Size(231, 160);
			this.listView.TabIndex = 0;
			this.listView.View = System.Windows.Forms.View.Details;
			this.listView.DoubleClick += new System.EventHandler(this.listView_DoubleClick);
			this.listView.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Available plugins";
			this.columnHeader1.Width = 163;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Startup";
			this.columnHeader2.Width = 47;
			// 
			// buttonLoad
			// 
			this.buttonLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonLoad.Location = new System.Drawing.Point(248, 8);
			this.buttonLoad.Name = "buttonLoad";
			this.buttonLoad.TabIndex = 1;
			this.buttonLoad.Text = "&Load";
			this.buttonLoad.Click += new System.EventHandler(this.buttonLoad_Click);
			// 
			// buttonUnload
			// 
			this.buttonUnload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonUnload.Location = new System.Drawing.Point(248, 40);
			this.buttonUnload.Name = "buttonUnload";
			this.buttonUnload.TabIndex = 2;
			this.buttonUnload.Text = "&Unload";
			this.buttonUnload.Click += new System.EventHandler(this.buttonUnload_Click);
			// 
			// imageList
			// 
			this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imageList.ImageSize = new System.Drawing.Size(16, 11);
			this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
			this.imageList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// description
			// 
			this.description.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.description.Location = new System.Drawing.Point(24, 195);
			this.description.Multiline = true;
			this.description.Name = "description";
			this.description.ReadOnly = true;
			this.description.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.description.Size = new System.Drawing.Size(288, 48);
			this.description.TabIndex = 6;
			this.description.Text = "";
			// 
			// webSite
			// 
			this.webSite.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.webSite.Location = new System.Drawing.Point(24, 312);
			this.webSite.Name = "webSite";
			this.webSite.Size = new System.Drawing.Size(288, 16);
			this.webSite.TabIndex = 10;
			this.webSite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.webSite_LinkClicked);
			// 
			// labelDescription
			// 
			this.labelDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelDescription.Location = new System.Drawing.Point(10, 179);
			this.labelDescription.Name = "labelDescription";
			this.labelDescription.Size = new System.Drawing.Size(100, 16);
			this.labelDescription.TabIndex = 5;
			this.labelDescription.Text = "Description:";
			// 
			// labelWebSite
			// 
			this.labelWebSite.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelWebSite.Location = new System.Drawing.Point(10, 296);
			this.labelWebSite.Name = "labelWebSite";
			this.labelWebSite.Size = new System.Drawing.Size(56, 16);
			this.labelWebSite.TabIndex = 9;
			this.labelWebSite.Text = "Web Site:";
			// 
			// labelDeveloper
			// 
			this.labelDeveloper.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelDeveloper.Location = new System.Drawing.Point(10, 253);
			this.labelDeveloper.Name = "labelDeveloper";
			this.labelDeveloper.Size = new System.Drawing.Size(62, 16);
			this.labelDeveloper.TabIndex = 7;
			this.labelDeveloper.Text = "Developer:";
			// 
			// developer
			// 
			this.developer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.developer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.developer.Location = new System.Drawing.Point(24, 271);
			this.developer.Name = "developer";
			this.developer.Size = new System.Drawing.Size(296, 16);
			this.developer.TabIndex = 8;
			// 
			// buttonInstall
			// 
			this.buttonInstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonInstall.Location = new System.Drawing.Point(248, 88);
			this.buttonInstall.Name = "buttonInstall";
			this.buttonInstall.TabIndex = 3;
			this.buttonInstall.Text = "&Install";
			this.buttonInstall.Click += new System.EventHandler(this.buttonInstall_Click);
			// 
			// buttonUninstall
			// 
			this.buttonUninstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonUninstall.Location = new System.Drawing.Point(248, 120);
			this.buttonUninstall.Name = "buttonUninstall";
			this.buttonUninstall.TabIndex = 4;
			this.buttonUninstall.Text = "&Uninstall";
			this.buttonUninstall.Click += new System.EventHandler(this.buttonUninstall_Click);
			// 
			// PluginDialog
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(328, 342);
			this.Controls.Add(this.buttonUninstall);
			this.Controls.Add(this.buttonInstall);
			this.Controls.Add(this.developer);
			this.Controls.Add(this.labelDeveloper);
			this.Controls.Add(this.labelWebSite);
			this.Controls.Add(this.labelDescription);
			this.Controls.Add(this.webSite);
			this.Controls.Add(this.description);
			this.Controls.Add(this.buttonUnload);
			this.Controls.Add(this.buttonLoad);
			this.Controls.Add(this.listView);
			this.MinimumSize = new System.Drawing.Size(336, 272);
			this.Name = "PluginDialog";
			this.Text = "Plugin Load/Unload";
			this.Load += new System.EventHandler(this.PluginDialog_Load);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// Fill the list view with currently installed plugins.
		/// </summary>
		void AddPluginList()
		{
			listView.Items.Clear();
			foreach (PluginInfo pi in compiler.Plugins) 
			{
				PluginListItem li = new PluginListItem(pi);
				listView.Items.Add(li);
			}
		}

		private void PluginDialog_Load(object sender, System.EventArgs e)
		{
			AddPluginList();
			
			//Force UI state update
			listView_SelectedIndexChanged(this,EventArgs.Empty);
			UpdateUIStates();
		}

		/// <summary>
		/// Unload selected plugins.
		/// </summary>
		private void buttonUnload_Click(object sender, System.EventArgs e)
		{
			foreach(PluginListItem pi in listView.SelectedItems)
				PluginUnload(pi);
			listView.Invalidate();
			UpdateUIStates();
		}

		/// <summary>
		/// Load selected plugins.
		/// </summary>
		private void buttonLoad_Click(object sender, System.EventArgs e)
		{
			foreach(PluginListItem pi in listView.SelectedItems)
				PluginLoad(pi);
			listView.Invalidate();
			UpdateUIStates();
		}

		/// <summary>
		/// Load plugin and display message on failure.
		/// </summary>
		private void PluginLoad(PluginListItem pi)
		{
			try
			{
				compiler.Load(pi.PluginInfo);
			}
			catch(Exception caught)
			{
				MessageBox.Show("The error was:\n\n" + caught.Message, pi.Name + 
					" plugin failed to load.", 
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
		}

		/// <summary>
		/// Unload plugin and display message on failure.
		/// </summary>
		public void PluginUnload(PluginListItem pi)
		{
			try
			{
				compiler.Unload(pi.PluginInfo);
			}
			catch(Exception caught)
			{
				MessageBox.Show("The error was:\n\n" + caught.Message, pi.Name + 
					" failed to unload.", 
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
		}

		/// <summary>
		/// Updates enabled states of controls to reflect selection.
		/// </summary>
		void UpdateUIStates()
		{
			bool isItemSelected = listView.SelectedItems.Count > 0;
			buttonUninstall.Enabled = isItemSelected;
			if(!isItemSelected)
			{
				buttonLoad.Enabled = false;
				buttonUnload.Enabled = false;
				return;
			}

			PluginListItem item = (PluginListItem)listView.SelectedItems[0];
			buttonLoad.Enabled = !item.PluginInfo.IsCurrentlyLoaded;
			buttonUnload.Enabled = item.PluginInfo.IsCurrentlyLoaded;
		}

		private void listView_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			description.Text = "";
			developer.Text = "";
			webSite.Text = "";

			UpdateUIStates();

			if(listView.SelectedItems.Count != 1)
				return;

			PluginListItem item = (PluginListItem)listView.SelectedItems[0];

			description.Text = item.PluginInfo.Description;
			developer.Text = item.PluginInfo.Developer;
			webSite.Text = item.PluginInfo.WebSite;
		}

		private void webSite_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			if(webSite.Text == null || webSite.Text.Length<=0)
				return;

			Process.Start(webSite.Text);
		}

		private void buttonInstall_Click(object sender, System.EventArgs e)
		{
			Form installDialog = new PluginInstallDialog(compiler);
			installDialog.Icon = this.Icon;
			installDialog.ShowDialog();

			// Rescan for plugins
			compiler.FindPlugins();
			AddPluginList();
		}

		/// <summary>
		/// Uninstall/remove plugins
		/// </summary>
		private void buttonUninstall_Click(object sender, System.EventArgs e)
		{
			foreach(PluginListItem pi in listView.SelectedItems)
			{
				string fullPath = pi.PluginInfo.FullPath;
				if(!File.Exists(fullPath))
				{
					// Ignore internal plugins
					MessageBox.Show("Plugin '" + pi.Name + "' is inside worldwind.exe and cannot be uninstalled.",
						"Uninstall", 
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);
					continue;
				}

				// Show uninstall warning
				string msg = string.Format("Do you really want to uninstall {0}?", pi.Name );
				if( MessageBox.Show(msg, "Delete plugin", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
					MessageBoxDefaultButton.Button2) != DialogResult.Yes)
					continue;

				try
				{
					compiler.Uninstall( pi.PluginInfo );

					// Remove if from the plugin list
					listView.Items.Remove(pi);
				}
				catch(Exception caught)
				{
					MessageBox.Show("Uninstall failed.  The error was:\n\n" + caught.Message, pi.Name + 
						" plugin failed to uninstall.", 
						MessageBoxButtons.OK,
						MessageBoxIcon.Error);
				}
			}

			UpdateUIStates();
		}

		/// <summary>
		/// Invert state of double clicked item (load/unload)
		/// </summary>
		private void listView_DoubleClick(object sender, System.EventArgs e)
		{
			foreach(PluginListItem pi in listView.SelectedItems)
			{
				if(pi.PluginInfo.IsCurrentlyLoaded)
					PluginUnload(pi);
				else
					PluginLoad(pi);
			}
			listView.Invalidate();
			UpdateUIStates();
		}
	}
}
