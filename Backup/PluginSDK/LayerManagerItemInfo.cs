using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace WorldWind.Menu
{
	/// <summary>
	/// Summary description for LayerManagerItemInfo.
	/// </summary>
	public class LayerManagerItemInfo : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ColumnHeader columnHeaderCategory;
		private System.Windows.Forms.ColumnHeader columnHeaderValue;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Menu.LayerManagerItemInfo"/> class.
		/// </summary>
		/// <param name="itemHash"></param>
		public LayerManagerItemInfo(System.Collections.Hashtable itemHash)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			
			foreach(string key in itemHash.Keys)
			{
				Object o = itemHash[key];
				this.listView1.Items.Add(new System.Windows.Forms.ListViewItem(new string[] {key, o.ToString()}));
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.listView1 = new System.Windows.Forms.ListView();
			this.columnHeaderCategory = new System.Windows.Forms.ColumnHeader();
			this.columnHeaderValue = new System.Windows.Forms.ColumnHeader();
			this.SuspendLayout();
			// 
			// listView1
			// 
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						this.columnHeaderCategory,
																						this.columnHeaderValue});
			this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listView1.Location = new System.Drawing.Point(0, 0);
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(360, 262);
			this.listView1.TabIndex = 0;
			this.listView1.View = System.Windows.Forms.View.Details;
			// 
			// columnHeaderCategory
			// 
			this.columnHeaderCategory.Text = "Category";
			this.columnHeaderCategory.Width = 150;
			// 
			// columnHeaderValue
			// 
			this.columnHeaderValue.Text = "Value";
			this.columnHeaderValue.Width = 200;
			// 
			// LayerManagerItemInfo
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(360, 262);
			this.Controls.Add(this.listView1);
			this.Name = "LayerManagerItemInfo";
			this.Text = "LayerManagerItemInfo";
			this.ResumeLayout(false);

		}
		#endregion
	}
}
