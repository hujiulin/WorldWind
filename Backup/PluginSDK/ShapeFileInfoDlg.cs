using System;
using System.IO;
using System.Data;
using System.Data.Odbc;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;

namespace WorldWind
{
	/// <summary>
	/// Summary description for ShapeFileInfoDlg.
	/// </summary>
	public class ShapeFileInfoDlg : System.Windows.Forms.Form
	{
		public System.Windows.Forms.DataGrid dataGrid1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ShapeFileInfoDlg(string dbfPath, bool isInZip)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			if(dbfPath != "")
			{
				if(!isInZip)
					setDbfInfo(dbfPath);
				else
					setDbfInfoFromZip(dbfPath);
			}
		}

		/// <summary>
		/// gets the information from a dbf as a DataTable (null if there was an error)
		/// </summary>		
		private DataTable getInfoFromDBF(string dbfPath)
		{
			try
			{	
				string connectionString = "Driver={Microsoft dBASE Driver (*.dbf)};DBQ="+
					Path.GetDirectoryName(Path.GetFullPath(dbfPath));
				OdbcConnection conn = new OdbcConnection(connectionString);
				OdbcCommand command = new OdbcCommand("SELECT * FROM "
					+Path.GetFileNameWithoutExtension(dbfPath),conn);
				DataSet ds = new DataSet();
				OdbcDataAdapter da = new OdbcDataAdapter(command);
				da.Fill(ds);
				return ds.Tables[0];
			}
			catch
			{
				return null;
			}			
		}

		private void setDbfInfo(string dbfPath)
		{
			DataTable dt=getInfoFromDBF(dbfPath);			
			dataGrid1.DataSource=dt;
			dataGrid1.CaptionText=dbfPath;			
		}

		private void setDbfInfoFromZip(string dbfPath)
		{	
			try
			{	
				//Navigate the Zip to find the files and update their index
				ZipFile zFile = new ZipFile(dbfPath);				
				foreach (ZipEntry ze in zFile) 
				{
					if(ze.Name.ToLower().EndsWith(".dbf"))
					{
						//Extracts the file in temp
						FastZip fz = new FastZip();
						fz.ExtractZip(dbfPath, Path.GetTempPath(),
							ICSharpCode.SharpZipLib.Zip.FastZip.Overwrite.Always,null,"","");
						setDbfInfo(Path.Combine(Path.GetTempPath(),ze.Name));						
					}
				}						
			}
			catch { return; }			


			

            
			
			//TODO: read the sbf data into the datagrid

			//DataTable dt=getInfoFromDBF(dbfPath);			
			//dataGrid1.DataSource=dt;
			//dataGrid1.CaptionText=dbfPath;			
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
			this.dataGrid1 = new System.Windows.Forms.DataGrid();
			((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// dataGrid1
			// 
			this.dataGrid1.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.dataGrid1.DataMember = "";
			this.dataGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dataGrid1.Location = new System.Drawing.Point(0, 0);
			this.dataGrid1.Name = "dataGrid1";
			this.dataGrid1.ReadOnly = true;
			this.dataGrid1.Size = new System.Drawing.Size(680, 334);
			this.dataGrid1.TabIndex = 0;
			this.dataGrid1.TabStop = false;
			// 
			// ShapeFileInfoDlg
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(680, 334);
			this.Controls.Add(this.dataGrid1);
			this.Name = "ShapeFileInfoDlg";
			this.Text = "Shapefile\'s .dbf information Window";
			((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion
	}
}
