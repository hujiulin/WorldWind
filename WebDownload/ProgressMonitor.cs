using System;
using System.Globalization;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Utility;

namespace WorldWind.Net.Monitor
{
	/// <summary>
	/// Simple window showing all http downloads and progress.
	/// </summary>
	public class ProgressMonitor : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ListView listView;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.ContextMenu contextMenu;
		private System.Windows.Forms.MenuItem menuItemSpacer1;
		private System.Windows.Forms.MenuItem menuItemCopy;
		private System.Windows.Forms.MenuItem menuItemOpenUrl;
		private System.Windows.Forms.MenuItem menuItemViewDir;
		private System.Windows.Forms.MenuItem menuItemClear;
		private System.Windows.Forms.ColumnHeader columnHeader6;
		private System.Windows.Forms.MenuItem menuItemHeaders;
		private System.Windows.Forms.MenuItem menuItemSpacer2;
		private System.Windows.Forms.ColumnHeader columnHeader7;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItemEdit;
		private System.Windows.Forms.MenuItem menuItem6;
		private System.Windows.Forms.MenuItem menuItem7;
		private System.Windows.Forms.MenuItem menuItem9;
		private System.Windows.Forms.MenuItem menuItemRun;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.MenuItem menuItemEditCopy;
		private System.Windows.Forms.MenuItem menuItemEditDelete;
		private System.Windows.Forms.MenuItem menuItemEditClear;
		private bool isRunning = true;
		private System.Windows.Forms.MenuItem menuItemSpacer3;
		private const int maxItems = 50;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.MenuItem menuItemWriteLog;
		private bool logToFile;
		private System.Windows.Forms.ColumnHeader columnHeader8;
		private System.Windows.Forms.MenuItem menuItemTools;
		private System.Windows.Forms.MenuItem menuItemFile;
		private System.Windows.Forms.MenuItem menuItemFileClose;
		private System.Windows.Forms.MenuItem menuItemFileRetry;
		private System.Windows.Forms.MenuItem menuItemSeparator3;
		private System.Windows.Forms.MenuItem menuItemToolsDetails;
		private System.Windows.Forms.MenuItem menuItemToolsViewUrl;
		private System.Windows.Forms.MenuItem menuItemToolsViewDirectory;
		private const string logCategory = "HTTP";
		private System.Windows.Forms.MenuItem menuItemEditSelectAll;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private ArrayList retryDownloads = new ArrayList();
		// Sorting disabled
		//	private DebugItemComparer comparer = new DebugItemComparer();

		/// <summary>
		/// Initializes a new instance of the <see cref="ProgressMonitor"/> class.
		/// </summary>
		public ProgressMonitor()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			InitializeComponentText();

			// Sorting disabled
			//	listView.ListViewItemSorter = comparer;
			WebDownload.DebugCallback += new DownloadCompleteHandler(Update);
		}

		
		#region I18N code
		/// <summary>
		/// We use this to override any strings set in InitializeComponent()
		/// that need to be able to be translated, because the Visual Studio
		/// Designer will overwrite things like menu names if the Designer is
		/// used after changes are made in InitializeComponent().
		/// 
		/// Any time this class is changed via the Designer, make sure that
		/// any translatable strings in InitializeComponent() are also
		/// represented in InitializeComponentText().
		/// </summary>
		private void InitializeComponentText()
		{
			string tabChar = "\t";

			this.columnHeader1.Text = "Start time";
			this.columnHeader2.Text = "Position";
			this.columnHeader8.Text = "Length";
			this.columnHeader5.Text = "Status";
			this.columnHeader4.Text = "Url";
			this.columnHeader6.Text = "Destination file";
			this.columnHeader7.Text = "Headers";
			this.columnHeader3.Text = "Download Time";
			this.menuItemHeaders.Text = "View request &details";
			this.menuItemOpenUrl.Text = "Open &Url in web browser";
			this.menuItemViewDir.Text = "&View destination directory";
			this.menuItemCopy.Text = "&Copy information";
			this.menuItemClear.Text = "&Clear list";
			this.menuItemFile.Text = "&File";
			this.menuItemFileRetry.Text = "&Retry download";
			this.menuItemFileClose.Text = "&Close";
			this.menuItemEdit.Text = "&Edit";
			this.menuItemEditCopy.Text = "&Copy";
			this.menuItemEditDelete.Text = "&Delete";
			this.menuItemEditSelectAll.Text = "Select &All";
			this.menuItemEditClear.Text = "&Clear";
			this.menuItem9.Text = "&Monitor";
			this.menuItemRun.Text = "&Run";
			this.menuItemWriteLog.Text = "&Write Log";
			this.menuItemTools.Text = "&Tools";
			this.menuItemToolsDetails.Text = string.Format("View request &details{0}{1}", tabChar, "Enter");
			this.menuItemToolsViewUrl.Text = "Open &Url in web browser";
			this.menuItemToolsViewDirectory.Text = "&View destination directory";
			this.Text = "Download progress monitor";
		}
		#endregion


		/// <summary>
		/// Updates the specified download.
		/// </summary>
		/// <param name="wd">The wd.</param>
		public void Update(WebDownload wd)
		{
			// Make sure we're on the right thread
			if( this.InvokeRequired ) 
			{
                try
                {
                    // Update progress asynchronously
                    DownloadCompleteHandler dlgt = new DownloadCompleteHandler(Update);
                    this.Invoke(dlgt, new object[] { wd });
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
				return;
			}

			foreach( DebugItem item in listView.Items )
			{
				if(item.WebDownload == wd)
				{
					item.Update( wd );

					if(logToFile)
					{
						if(wd.Exception != null)
						{
							Log.Write(Log.Levels.Error, logCategory, "Error : " + wd.Url.Replace("http://","") );
                            Log.Write(Log.Levels.Error, logCategory, "  \\---" + wd.Exception.Message);
						}
						if(wd.ContentLength == wd.BytesProcessed)
						{
							string msg = string.Format(CultureInfo.CurrentCulture, 
								"Done ({0:f2}s): {1}",
								item.ElapsedTime.TotalSeconds,
								wd.Url.Replace("http://","") );
                            Log.Write(Log.Levels.Debug, logCategory, msg);
						}
					}
					return;
				}
			}
			if(!isRunning)
				return;

			if( listView.Items.Count >= maxItems )
				RemoveOldestItem();

			if(logToFile)
			{
				Log.Write(Log.Levels.Debug, logCategory, "Get  : " + wd.Url );
				if(wd.SavedFilePath!=null)
					Log.Write(Log.Levels.Debug+1, logCategory, "  \\--- " + wd.SavedFilePath );
			}

			DebugItem newItem = new DebugItem( wd );
			newItem.Update(wd);
			listView.Items.Insert(0,newItem);
		}

		/// <summary>
		/// Removes the oldest item.
		/// </summary>
		private void RemoveOldestItem()
		{
			DebugItem oldest = null;
			foreach( DebugItem item in listView.Items)
			{
				if (oldest==null)
				{
					oldest = item;
					continue;
				}

				if(oldest.StartTime > item.StartTime)
					oldest = item;
			}
			if(oldest!=null)
				listView.Items.Remove( oldest );
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.KeyUp"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs"/> that contains the event data.</param>
		protected override void OnKeyUp(System.Windows.Forms.KeyEventArgs e) 
		{
			if(e==null)
				return;

			switch(e.KeyCode) 
			{
				case Keys.Space:
					menuItemHeaders_Click(this,e);
					break;
				case Keys.Escape:
					Close();
					e.Handled = true;
					break;
				case Keys.H:
				case Keys.F4:
					if(e.Modifiers==Keys.Control)
					{
						Close();
						e.Handled = true;
					}
					break;
			}

			base.OnKeyUp(e);
		}

		/// <summary>
		/// Disposes of the resources (other than memory) used by
		/// the <see cref="T:System.Windows.Forms.Form"/>
		/// .
		/// </summary>
		/// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			WebDownload.DebugCallback -= new DownloadCompleteHandler(Update);

			foreach( WebDownload dl in retryDownloads )
				if (dl!=null)
					dl.Dispose();
			retryDownloads.Clear();

			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.listView = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader8 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.contextMenu = new System.Windows.Forms.ContextMenu();
			this.menuItemHeaders = new System.Windows.Forms.MenuItem();
			this.menuItemSpacer1 = new System.Windows.Forms.MenuItem();
			this.menuItemOpenUrl = new System.Windows.Forms.MenuItem();
			this.menuItemViewDir = new System.Windows.Forms.MenuItem();
			this.menuItemSpacer2 = new System.Windows.Forms.MenuItem();
			this.menuItemCopy = new System.Windows.Forms.MenuItem();
			this.menuItemClear = new System.Windows.Forms.MenuItem();
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.menuItemFile = new System.Windows.Forms.MenuItem();
			this.menuItemFileRetry = new System.Windows.Forms.MenuItem();
			this.menuItemSeparator3 = new System.Windows.Forms.MenuItem();
			this.menuItemFileClose = new System.Windows.Forms.MenuItem();
			this.menuItemEdit = new System.Windows.Forms.MenuItem();
			this.menuItemEditCopy = new System.Windows.Forms.MenuItem();
			this.menuItemEditDelete = new System.Windows.Forms.MenuItem();
			this.menuItem6 = new System.Windows.Forms.MenuItem();
			this.menuItemEditSelectAll = new System.Windows.Forms.MenuItem();
			this.menuItem7 = new System.Windows.Forms.MenuItem();
			this.menuItemEditClear = new System.Windows.Forms.MenuItem();
			this.menuItem9 = new System.Windows.Forms.MenuItem();
			this.menuItemRun = new System.Windows.Forms.MenuItem();
			this.menuItem4 = new System.Windows.Forms.MenuItem();
			this.menuItemWriteLog = new System.Windows.Forms.MenuItem();
			this.menuItemTools = new System.Windows.Forms.MenuItem();
			this.menuItemToolsDetails = new System.Windows.Forms.MenuItem();
			this.menuItemSpacer3 = new System.Windows.Forms.MenuItem();
			this.menuItemToolsViewUrl = new System.Windows.Forms.MenuItem();
			this.menuItemToolsViewDirectory = new System.Windows.Forms.MenuItem();
			this.SuspendLayout();
			// 
			// listView
			// 
			this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					   this.columnHeader1,
																					   this.columnHeader2,
																					   this.columnHeader8,
																					   this.columnHeader5,
																					   this.columnHeader4,
																					   this.columnHeader6,
																					   this.columnHeader7,
																					   this.columnHeader3});
			this.listView.ContextMenu = this.contextMenu;
			this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView.FullRowSelect = true;
			this.listView.GridLines = true;
			this.listView.Location = new System.Drawing.Point(0, 0);
			this.listView.Name = "listView";
			this.listView.Size = new System.Drawing.Size(768, 179);
			this.listView.TabIndex = 0;
			this.listView.View = System.Windows.Forms.View.Details;
			this.listView.DoubleClick += new System.EventHandler(this.menuItemHeaders_Click);
			this.listView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_ColumnClick);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Start time";
			this.columnHeader1.Width = 79;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Position";
			this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.columnHeader2.Width = 65;
			// 
			// columnHeader8
			// 
			this.columnHeader8.Text = "Length";
			this.columnHeader8.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.columnHeader8.Width = 65;
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "Status";
			this.columnHeader5.Width = 200;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Url";
			this.columnHeader4.Width = 191;
			// 
			// columnHeader6
			// 
			this.columnHeader6.Text = "Destination file";
			this.columnHeader6.Width = 235;
			// 
			// columnHeader7
			// 
			this.columnHeader7.Text = "Headers";
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Download Time";
			this.columnHeader3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.columnHeader3.Width = 70;
			// 
			// contextMenu
			// 
			this.contextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						this.menuItemHeaders,
																						this.menuItemSpacer1,
																						this.menuItemOpenUrl,
																						this.menuItemViewDir,
																						this.menuItemSpacer2,
																						this.menuItemCopy,
																						this.menuItemClear});
			this.contextMenu.Popup += new System.EventHandler(this.contextMenu_Popup);
			// 
			// menuItemHeaders
			// 
			this.menuItemHeaders.DefaultItem = true;
			this.menuItemHeaders.Index = 0;
			this.menuItemHeaders.Text = "View request &details";
			this.menuItemHeaders.Click += new System.EventHandler(this.menuItemHeaders_Click);
			// 
			// menuItemSpacer1
			// 
			this.menuItemSpacer1.Index = 1;
			this.menuItemSpacer1.Text = "-";
			// 
			// menuItemOpenUrl
			// 
			this.menuItemOpenUrl.Index = 2;
			this.menuItemOpenUrl.Text = "Open &Url in web browser";
			this.menuItemOpenUrl.Click += new System.EventHandler(this.menuItemOpenUrl_Click);
			// 
			// menuItemViewDir
			// 
			this.menuItemViewDir.Index = 3;
			this.menuItemViewDir.Text = "&View destination directory";
			this.menuItemViewDir.Click += new System.EventHandler(this.menuItemViewDir_Click);
			// 
			// menuItemSpacer2
			// 
			this.menuItemSpacer2.Index = 4;
			this.menuItemSpacer2.Text = "-";
			// 
			// menuItemCopy
			// 
			this.menuItemCopy.Index = 5;
			this.menuItemCopy.Text = "&Copy information";
			this.menuItemCopy.Click += new System.EventHandler(this.menuItemCopy_Click);
			// 
			// menuItemClear
			// 
			this.menuItemClear.Index = 6;
			this.menuItemClear.Text = "&Clear list";
			this.menuItemClear.Click += new System.EventHandler(this.menuItemClear_Click);
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItemFile,
																					  this.menuItemEdit,
																					  this.menuItem9,
																					  this.menuItemTools});
			// 
			// menuItemFile
			// 
			this.menuItemFile.Index = 0;
			this.menuItemFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuItemFileRetry,
																						 this.menuItemSeparator3,
																						 this.menuItemFileClose});
			this.menuItemFile.Text = "&File";
			this.menuItemFile.Popup += new System.EventHandler(this.menuItemFile_Popup);
			// 
			// menuItemFileRetry
			// 
			this.menuItemFileRetry.Index = 0;
			this.menuItemFileRetry.Shortcut = System.Windows.Forms.Shortcut.CtrlR;
			this.menuItemFileRetry.Text = "&Retry download";
			this.menuItemFileRetry.Click += new System.EventHandler(this.menuItemFileRetry_Click);
			// 
			// menuItemSeparator3
			// 
			this.menuItemSeparator3.Index = 1;
			this.menuItemSeparator3.Text = "-";
			// 
			// menuItemFileClose
			// 
			this.menuItemFileClose.Index = 2;
			this.menuItemFileClose.Shortcut = System.Windows.Forms.Shortcut.CtrlF4;
			this.menuItemFileClose.Text = "&Close";
			this.menuItemFileClose.Click += new System.EventHandler(this.menuItemFileClose_Click);
			// 
			// menuItemEdit
			// 
			this.menuItemEdit.Index = 1;
			this.menuItemEdit.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuItemEditCopy,
																						 this.menuItemEditDelete,
																						 this.menuItem6,
																						 this.menuItemEditSelectAll,
																						 this.menuItem7,
																						 this.menuItemEditClear});
			this.menuItemEdit.Text = "&Edit";
			this.menuItemEdit.Popup += new System.EventHandler(this.menuItemEdit_Popup);
			// 
			// menuItemEditCopy
			// 
			this.menuItemEditCopy.Index = 0;
			this.menuItemEditCopy.Shortcut = System.Windows.Forms.Shortcut.CtrlC;
			this.menuItemEditCopy.Text = "&Copy";
			this.menuItemEditCopy.Click += new System.EventHandler(this.menuItemCopy_Click);
			// 
			// menuItemEditDelete
			// 
			this.menuItemEditDelete.Index = 1;
			this.menuItemEditDelete.Shortcut = System.Windows.Forms.Shortcut.Del;
			this.menuItemEditDelete.Text = "&Delete";
			this.menuItemEditDelete.Click += new System.EventHandler(this.menuItemEditDelete_Click);
			// 
			// menuItem6
			// 
			this.menuItem6.Index = 2;
			this.menuItem6.Text = "-";
			// 
			// menuItemEditSelectAll
			// 
			this.menuItemEditSelectAll.Index = 3;
			this.menuItemEditSelectAll.Shortcut = System.Windows.Forms.Shortcut.CtrlA;
			this.menuItemEditSelectAll.Text = "Select &All";
			this.menuItemEditSelectAll.Click += new System.EventHandler(this.menuItemSelectAll_Click);
			// 
			// menuItem7
			// 
			this.menuItem7.Index = 4;
			this.menuItem7.Text = "-";
			// 
			// menuItemEditClear
			// 
			this.menuItemEditClear.Index = 5;
			this.menuItemEditClear.Shortcut = System.Windows.Forms.Shortcut.AltBksp;
			this.menuItemEditClear.Text = "&Clear";
			this.menuItemEditClear.Click += new System.EventHandler(this.menuItemClear_Click);
			// 
			// menuItem9
			// 
			this.menuItem9.Index = 2;
			this.menuItem9.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItemRun,
																					  this.menuItem4,
																					  this.menuItemWriteLog});
			this.menuItem9.Text = "&Monitor";
			// 
			// menuItemRun
			// 
			this.menuItemRun.Checked = true;
			this.menuItemRun.Index = 0;
			this.menuItemRun.Shortcut = System.Windows.Forms.Shortcut.CtrlR;
			this.menuItemRun.Text = "&Run";
			this.menuItemRun.Click += new System.EventHandler(this.menuItemRun_Click);
			// 
			// menuItem4
			// 
			this.menuItem4.Index = 1;
			this.menuItem4.Text = "-";
			// 
			// menuItemWriteLog
			// 
			this.menuItemWriteLog.Index = 2;
			this.menuItemWriteLog.Text = "&Write Log";
			this.menuItemWriteLog.Click += new System.EventHandler(this.menuItemWriteLog_Click);
			// 
			// menuItemTools
			// 
			this.menuItemTools.Index = 3;
			this.menuItemTools.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						  this.menuItemToolsDetails,
																						  this.menuItemSpacer3,
																						  this.menuItemToolsViewUrl,
																						  this.menuItemToolsViewDirectory});
			this.menuItemTools.Text = "&Tools";
			this.menuItemTools.Popup += new System.EventHandler(this.menuItemTools_Popup);
			// 
			// menuItemToolsDetails
			// 
			this.menuItemToolsDetails.Index = 0;
			this.menuItemToolsDetails.Text = "View request &details\tEnter";
			// 
			// menuItemSpacer3
			// 
			this.menuItemSpacer3.Index = 1;
			this.menuItemSpacer3.Text = "-";
			// 
			// menuItemToolsViewUrl
			// 
			this.menuItemToolsViewUrl.Index = 2;
			this.menuItemToolsViewUrl.Text = "Open &Url in web browser";
			this.menuItemToolsViewUrl.Click += new System.EventHandler(this.menuItemOpenUrl_Click);
			// 
			// menuItemToolsViewDirectory
			// 
			this.menuItemToolsViewDirectory.Index = 3;
			this.menuItemToolsViewDirectory.Text = "&View destination directory";
			this.menuItemToolsViewDirectory.Click += new System.EventHandler(this.menuItemViewDir_Click);
			// 
			// ProgressMonitor
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(768, 179);
			this.Controls.Add(this.listView);
			this.KeyPreview = true;
			this.Menu = this.mainMenu1;
			this.Name = "ProgressMonitor";
			this.Text = "Download progress monitor";
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// Sorts ascending/descending on clicked column
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.ColumnClickEventArgs"/> instance containing the event data.</param>
		private void listView_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
		{
			// Sorting disabled
			//			comparer.SortColumn( e.Column );
		}

		/// <summary>
		/// Clears the download list
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemClear_Click(object sender, System.EventArgs e)
		{
			listView.Items.Clear();
		}

		/// <summary>
		/// Opens a directory window containing selected item(s).
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemViewDir_Click(object sender, System.EventArgs e)
		{
			foreach( DebugItem item in listView.SelectedItems)
			{
				if (item.SavedFilePath==null)
					continue;
				Process.Start( Path.GetDirectoryName( item.SavedFilePath ) );
			}
		}

		/// <summary>
		/// Opens the selected items in web browser
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemOpenUrl_Click(object sender, System.EventArgs e)
		{
			foreach( DebugItem item in listView.SelectedItems)
				Process.Start(item.Url);
		}

		/// <summary>
		/// Copy download info to clipboard
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		internal void menuItemCopy_Click(object sender, System.EventArgs e)
		{
			StringBuilder res = new StringBuilder();
			foreach( DebugItem item in listView.SelectedItems)
			{
				res.Append(item.ToString());
				res.Append(Environment.NewLine);
				Clipboard.SetDataObject(res.ToString());
			}
		}

		/// <summary>
		/// Displays HTTP headers in a new window
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemHeaders_Click(object sender, System.EventArgs e)
		{
			if(listView.SelectedItems.Count<=0)
				return;
			DebugItem item = (DebugItem) listView.SelectedItems[0];
			ProgressDetailForm form = new ProgressDetailForm( item );
			form.Icon = this.Icon;
			form.Show();
		}

		/// <summary>
		/// Selects all items
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemSelectAll_Click(object sender, System.EventArgs e)
		{
			foreach( DebugItem item in listView.Items )
				item.Selected = true;
		}

		/// <summary>
		/// Enables debug monitor
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemRun_Click(object sender, System.EventArgs e)
		{
			isRunning = !isRunning;
			menuItemRun.Checked = isRunning;
		}

		/// <summary>
		/// Delete selected items from list
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemEditDelete_Click(object sender, System.EventArgs e)
		{
			while(listView.SelectedItems.Count>0)
				listView.Items.Remove(listView.SelectedItems[0]);
		}

		/// <summary>
		/// Enables writing to log file.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemWriteLog_Click(object sender, System.EventArgs e)
		{
			logToFile = !logToFile;
			menuItemWriteLog.Checked = logToFile;
		}

		/// <summary>
		/// Handles the Popup event of the contextMenu control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void contextMenu_Popup(object sender, System.EventArgs e)
		{
			bool hasSelection = listView.SelectedItems.Count>0;
			bool hasItems = listView.Items.Count > 0;

			this.menuItemHeaders.Enabled = listView.SelectedItems.Count==1;
			this.menuItemOpenUrl.Enabled = hasSelection;
			this.menuItemViewDir.Enabled = hasSelection;
			this.menuItemCopy.Enabled = hasSelection;
			this.menuItemClear.Enabled = hasItems;
		}

		/// <summary>
		/// Handles the Popup event of the menuItemFile control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemFile_Popup(object sender, System.EventArgs e)
		{
			if(listView.SelectedItems.Count<=0)
			{
				this.menuItemFileRetry.Enabled = false;
				return;
			}

			DebugItem li = (DebugItem) listView.SelectedItems[0];
			this.menuItemFileRetry.Enabled = li.HasFailed;
		}

		/// <summary>
		/// Handles the Popup event of the menuItemEdit control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemEdit_Popup(object sender, System.EventArgs e)
		{
			bool hasSelection = listView.SelectedItems.Count>0;
			bool hasItems = listView.Items.Count > 0;
			this.menuItemEditCopy.Enabled = hasSelection;
			this.menuItemEditDelete.Enabled = hasSelection;
			this.menuItemEditClear.Enabled = hasItems;
			this.menuItemEditSelectAll.Enabled = hasItems;
		}

		/// <summary>
		/// Handles the Popup event of the menuItemTools control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemTools_Popup(object sender, System.EventArgs e)
		{
			bool hasSelection = listView.SelectedItems.Count>0;

			this.menuItemToolsDetails.Enabled = hasSelection;
			this.menuItemToolsViewDirectory.Enabled = hasSelection;
			this.menuItemToolsViewUrl.Enabled = hasSelection;
		}

		/// <summary>
		/// Handles the Click event of the menuItemFileRetry control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemFileRetry_Click(object sender, System.EventArgs e)
		{
			foreach(DebugItem li in listView.SelectedItems)
			{
				WebDownload dl = new WebDownload(li.Url);
				dl.SavedFilePath = li.SavedFilePath;
				dl.CompleteCallback += new DownloadCompleteHandler(OnDownloadComplete);
				dl.BackgroundDownloadFile();
				retryDownloads.Add(dl);
			}
		}

		/// <summary>
		/// Called when download is complete.
		/// </summary>
		/// <param name="dl">The download.</param>
		private void OnDownloadComplete( WebDownload dl )
		{
			if (retryDownloads.Contains(dl))
				retryDownloads.Remove(dl);
			dl.Dispose();
		}

		/// <summary>
		/// Handles the Click event of the menuItemFileClose control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemFileClose_Click(object sender, System.EventArgs e)
		{
			Close();
		}
	}

	/// <summary>
	/// List view item
	/// </summary>
	class DebugItem : ListViewItem
	{
		DateTime m_startTime;
		TimeSpan m_elapsed;
		long m_bytesProcessed;
		long m_contentLength;
		string m_status;
		string m_url;
		string m_savedFilePath;
		string m_headers;
		WebDownload m_webDownload;

		static Color AliveColor = Color.Blue;
		static Color FailedColor = Color.Red;
		static Color CompleteColor = Color.Black;

		/// <summary>
		/// Initializes a new instance of the <see cref="DebugItem"/> class.
		/// </summary>
		/// <param name="dl">The download.</param>
		public DebugItem( WebDownload dl )
		{
			m_webDownload = dl;
			m_startTime = dl.DownloadStartTime;
			Text = m_startTime.ToLongTimeString();
			m_url = dl.Url;
			m_savedFilePath = dl.SavedFilePath == null ? "<Memory>" : dl.SavedFilePath;
			SubItems.AddRange( new string[]{"","",m_status, "", m_url, m_savedFilePath, "", "" } );
			ForeColor = Color.Blue;
		}

		/// <summary>
		/// Gets the actual web download.
		/// </summary>
		/// <value>The web download.</value>
		public WebDownload WebDownload
		{
			get
			{
				return m_webDownload;
			}
		}

		/// <summary>
		/// Gets the download start time.
		/// </summary>
		/// <value>The start time.</value>
		public DateTime StartTime
		{
			get { return m_startTime; }
		}

		/// <summary>
		/// Gets or sets the number of bytes processed.
		/// </summary>
		/// <value>The bytes processed.</value>
		public long BytesProcessed
		{
			get { return m_bytesProcessed; }
			set 
			{ 
				m_bytesProcessed = value; 
				this.SubItems[1].Text = value.ToString(CultureInfo.CurrentCulture);
			}
		}

		/// <summary>
		/// Gets or sets the byte length of the downloaded content.
		/// </summary>
		/// <value>The length of the content.</value>
		public long ContentLength
		{
			get { return m_contentLength; }
			set 
			{
				m_contentLength = value;
				SubItems[2].Text = value > 0 ? value.ToString(CultureInfo.CurrentCulture) : "?";
			}
		}

		/// <summary>
		/// Gets or sets the download status.
		/// </summary>
		/// <value>The status.</value>
		public string Status
		{
			get { return m_status; }
			set 
			{ 
				m_status = value;
				SubItems[3].Text = value; 
			}
		}

		/// <summary>
		/// Gets the downloaded URL.
		/// </summary>
		/// <value>The URL.</value>
		public string Url
		{
			get { return m_url; }
		}

		/// <summary>
		/// Gets the file path where the file will be saved.
		/// </summary>
		/// <value>The saved file path.</value>
		public string SavedFilePath
		{
			get { return m_savedFilePath; }
		}

		/// <summary>
		/// Gets or sets the HTTP headers.
		/// </summary>
		/// <value>The headers.</value>
		public string Headers
		{
			get { return m_headers; }
			set 
			{
				m_headers = value; 
				SubItems[6].Text = value; 
			}
		}

		/// <summary>
		/// Gets or sets the elapsed time.
		/// </summary>
		/// <value>The elapsed time.</value>
		public TimeSpan ElapsedTime
		{
			get { return m_elapsed; }
			set 
			{
				m_elapsed = value;
				SubItems[7].Text = value.TotalSeconds.ToString("f2", CultureInfo.CurrentCulture) +"s"; 
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance has failed.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance has failed; otherwise, <c>false</c>.
		/// </value>
		public bool HasFailed
		{
			get { return ForeColor == FailedColor; }
		}

		/// <summary>
		/// Updates the display for the specified download.
		/// </summary>
		/// <param name="wd">The download to update.</param>
		public void Update( WebDownload wd )
		{
			try
			{
				BytesProcessed = wd.BytesProcessed;
				ContentLength = wd.ContentLength;
				ElapsedTime = DateTime.Now.Subtract(m_startTime);

				if(wd.Exception!=null)
				{
					// Failed
					Status = wd.Exception.Message;
					ForeColor = FailedColor;
				}
				else if(wd.BytesProcessed==0)
				{
					// Connecting
					Status = "Connecting...";
					ForeColor = AliveColor;
				}
				else if(wd.BytesProcessed==wd.ContentLength)
				{
					// Complete
					Status = "Complete";
					ForeColor = CompleteColor;
				}
				else 
					// In progress
					Status = "Receiving.";

                if(wd.response != null)
                {
                    Status = wd.response.StatusCode.ToString() + " " + Status;
                }

				if(m_headers==null && wd.response != null && wd.response.Headers != null)
				{
					string hdr="";
					foreach (string key in wd.response.Headers.Keys )
						hdr += key + ": " + wd.response.Headers[key] + Environment.NewLine;
					Headers = hdr;
				}
			} 
			catch (ObjectDisposedException)
			{
			}
		}

		public override string ToString()
		{
			string proxy = System.Net.WebRequest.DefaultWebProxy.GetProxy(new Uri(Url)).ToString();
			return string.Format(CultureInfo.CurrentCulture,
				"Start time: {7}{0}Total download time: {10}{0}Average transfer rate: {11:f1} kbit/s{0}Url: {1}{0}Target file: {2}{0}Progress: {3}/{4} bytes{0}Status: {5}{0}Proxy: {8}{0}{0}Response headers:{0}{9}{0}{6}{0}",
				Environment.NewLine,
				this.m_url,
				this.m_savedFilePath,
				this.m_bytesProcessed,
				this.ContentLength,
				this.m_status, 
				this.Headers,
				this.m_startTime,
				proxy,
				"".PadRight(40,'='),
				this.m_elapsed,
				m_contentLength>0 ? m_contentLength*8/m_elapsed.TotalSeconds/1000 : 0);
		}
	}

	/// <summary>
	/// Sorts the debug window list.
	/// </summary>
	class DebugItemComparer : IComparer
	{
		private int column;
		private SortOrder	sortOrder = SortOrder.Ascending;

		/// <summary>
		/// Initializes a new instance of the <see cref="DebugItemComparer"/> class.
		/// </summary>
		public DebugItemComparer()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DebugItemComparer"/> class.
		/// </summary>
		/// <param name="column">The column to sort on.</param>
		/// <param name="sortOrder">The sort order.</param>
		public DebugItemComparer(int column, SortOrder sortOrder)
		{
			this.column = column;
			this.sortOrder = sortOrder;
		}

		/// <summary>
		/// Sorts by a specific column.
		/// </summary>
		/// <param name="column">The column to sort by.</param>
		public void SortColumn( int column )
		{
			if ( this.column != column )
			{
				this.column = column;
				this.sortOrder = SortOrder.Ascending;
				return;
			}

			switch( sortOrder )
			{
				case SortOrder.Ascending:
					sortOrder = SortOrder.Descending;
					break;
				case SortOrder.Descending:
				case SortOrder.None:
					sortOrder = SortOrder.Ascending;
					break;
			}
		}

		/// <summary>
		/// Compares two objects and returns a value indicating whether one
		/// is less than, equal to or greater than the other.
		/// </summary>
		/// <param name="x">First object to compare.</param>
		/// <param name="y">Second object to compare.</param>
		/// <returns>
		/// 	<list type="table">
		/// 		<listheader>
		/// 			<term>Value</term>
		/// 			<description>Condition</description>
		/// 		</listheader>
		/// 		<item>
		/// 			<term> Less than zero</term>
		/// 			<description>
		/// 				<paramref name="x"/> is less than <paramref name="y"/>.</description>
		/// 		</item>
		/// 		<item>
		/// 			<term> Zero</term>
		/// 			<description>
		/// 				<paramref name="x"/> equals <paramref name="y"/>.</description>
		/// 		</item>
		/// 		<item>
		/// 			<term> Greater than zero</term>
		/// 			<description>
		/// 				<paramref name="x"/> is greater than <paramref name="y"/>.</description>
		/// 		</item>
		/// 	</list>
		/// </returns>
		/// <exception cref="T:System.ArgumentException">
		/// 	<para>Neither <paramref name="x"/> nor <paramref name="y"/> implements the <see cref="T:System.IComparable"/> interface.</para>
		/// 	<para>-or-</para>
		/// 	<para>
		/// 		<paramref name="x"/> and <paramref name="y"/> are of different types and neither one can handle comparisons with the other.</para>
		/// </exception>
		public int Compare(object x, object y)
		{
			if (sortOrder == SortOrder.None)
				return 0;

			DebugItem a = (DebugItem) x;
			DebugItem b = (DebugItem) y;

			int result = 0;
			switch( column )
			{
				case 0:
					result = a.StartTime.CompareTo( b.StartTime);
					break;
				case 1:
					result = a.BytesProcessed.CompareTo( b.BytesProcessed );
					break;
				case 2:
					result = a.ContentLength.CompareTo( b.ContentLength );
					break;
				case 3:
					result = a.Status.CompareTo( b.Status );
					break;
				case 4:
					result = a.Url.CompareTo( b.Url );
					break;
				case 5:
					result = a.SavedFilePath.CompareTo( b.SavedFilePath );
					break;
				case 6:
					result = a.Headers.CompareTo( b.Headers );
					break;
				default:
					Debug.Assert(false, "Sorting on Invalid column." );
					break;
			}

			if (sortOrder == SortOrder.Descending)
				result = -result;

			return result;
		}
	}
}

