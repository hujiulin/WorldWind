using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System;
using WorldWind.Net;
using WorldWind;
using WorldWind.Renderable;
using Utility;

namespace WorldWind
{
	/// <summary>
	/// Rapid Fire MODIS form.
	/// </summary>
	public class RapidFireModisManager : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ImageList imageList;
		private System.Windows.Forms.DateTimePicker dateTimePickerEndDate;
		private System.Windows.Forms.DateTimePicker dateTimePickerBeginDate;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.ProgressBar progressBar;
		private System.Windows.Forms.Label labelStatus;
		WorldWindow worldWindow1;

		static string CacheDirectory = "";
		string imageListFilePath = "";
		int currentTourScene;
		ArrayList currentTourScenes;

		DownloadableImageFromIconSet iconSet;
		System.Timers.Timer tourModeTimer;

		bool showFires = true;
		bool showFloods= true;
		bool showDust = true;
		bool showStorms = true;
		bool showVolcanoes = true;
		bool showMisc = true;

		Hashtable modisList = new Hashtable();

		private System.Windows.Forms.GroupBox groupBoxDateRange;
		private System.Windows.Forms.GroupBox groupBoxResolution;
		private System.Windows.Forms.RadioButton radioButtonResolution1km;
		private System.Windows.Forms.RadioButton radioButtonResolution500m;
		private System.Windows.Forms.RadioButton radioButtonResolution250m;
		private System.Windows.Forms.GroupBox groupBoxDisplay;
		private System.Windows.Forms.RadioButton radioButtonDisplayGroup;
		private System.Windows.Forms.RadioButton radioButtonDisplaySelected;
		private System.Windows.Forms.RadioButton radioButtonDisplayNone;
		private System.Windows.Forms.RadioButton radioButtonDisplayTourMode;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ImageList imageList2;
		private Hashtable currentList = new Hashtable();
		private Thread imageListUpdateThread;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.RapidFireModisManager"/> class.
		/// </summary>
		/// <param name="ww"></param>
		public RapidFireModisManager(WorldWindow ww)
		{
			InitializeComponent();
			
			this.worldWindow1 = ww;
		}

		protected override void OnLoad(EventArgs e)
		{
			this.dateTimePickerEndDate.Value = DateTime.Now.AddDays(1);
			this.dateTimePickerBeginDate.Value = DateTime.Now.Subtract(TimeSpan.FromDays(120));

			// TODO v1.3 - change association from "current world"
			CacheDirectory = Path.Combine(
				Path.Combine(worldWindow1.Cache.CacheDirectory, this.worldWindow1.CurrentWorld.Name),
				"Rapid Fire MODIS" );

			this.imageListFilePath = Path.Combine(CacheDirectory,"WWInventory.txt");

			for(int i = 0; i < this.listView1.Items.Count; i++)
				this.listView1.Items[i].Checked = true;

			this.tourModeTimer = new System.Timers.Timer(30000);
			this.tourModeTimer.Elapsed += new System.Timers.ElapsedEventHandler(tourModeTimer_Elapsed);
			imageListUpdateThread = new Thread(new ThreadStart(this.DownloadImageListUpdate));
			imageListUpdateThread.Name = "RapidFireModisManager.DownloadImageListUpdate";
			imageListUpdateThread.IsBackground = true;
			imageListUpdateThread.Start();

			base.OnLoad (e);
		}
		
		private void RemoveIconFromCurrentList(RapidFireModisRecord rfmr)
		{
			lock(this.currentList.SyncRoot)
			{
				if(this.currentList.Contains(rfmr.Title))
				{
					this.iconSet.RemoveDownloadableIcon(rfmr.Title);
					this.currentList.Remove(rfmr.Title);
				}
			}
		}

		private void AddIconToCurrentList(RapidFireModisRecord rfmr, string iconFilePath)
		{
			if(this.iconSet == null)
			{
				return;
			}

			lock(this.currentList.SyncRoot)
			{
				if(!this.currentList.Contains(rfmr.Title))
				{
					int imageIndex = 0;
					if(this.radioButtonResolution1km.Checked)
					{
						for(int i = 0; i < rfmr.ImagePaths.Length; i++)
						{
							if(rfmr.ImagePaths[i].IndexOf("1km") != -1)
							{
								imageIndex = i;
								break;
							}
						}
					}
					else if(this.radioButtonResolution500m.Checked)
					{
						for(int i = 0; i < rfmr.ImagePaths.Length; i++)
						{
							if(rfmr.ImagePaths[i].IndexOf("500m") != -1)
							{
								imageIndex = i;
								break;
							}
						}
					}
					else if(this.radioButtonResolution250m.Checked)
					{
						for(int i = 0; i < rfmr.ImagePaths.Length; i++)
						{
							if(rfmr.ImagePaths[i].IndexOf("250m") != -1)
							{
								imageIndex = i;
								break;
							}
						}
					}

					string[] titleParts = rfmr.Title.Split('.');
					string dateString = titleParts[1].Replace("A", "");
					
					int year = Int32.Parse(dateString.Substring(0,4), CultureInfo.InvariantCulture);
					int dayOfYear = Int32.Parse(dateString.Substring(4,3), CultureInfo.InvariantCulture);

					System.DateTime dt = new DateTime(year,1,1);
					
					dt = dt.AddDays(dayOfYear - 1);
					dt = dt.AddHours(Double.Parse(titleParts[2].Substring(0,2), CultureInfo.InvariantCulture));
					dt = dt.AddMinutes(Double.Parse(titleParts[2].Substring(2,2), CultureInfo.InvariantCulture));

					string desc = String.Format(CultureInfo.CurrentCulture,"{0} ({1} @ {2})\n{3}", titleParts[0],dt.ToShortDateString(), dt.ToShortTimeString(), rfmr.Description); 
					
					string iconDirectory = Path.Combine(
						Path.Combine( MainApplication.Settings.DataPath, "Icons" ),
						"Modis" );
					string iconFullPath = Path.Combine( iconDirectory, iconFilePath );
					
					iconSet.AddDownloadableIcon(rfmr.Title, 0, rfmr.west, rfmr.south, rfmr.east, rfmr.north,
						"http://rapidfire.sci.gsfc.nasa.gov/gallery/" + rfmr.ImagePaths[imageIndex],
						CacheDirectory + "\\" + rfmr.ImagePaths[imageIndex].Replace("/","\\"), iconFullPath, World.Settings.ModisIconSize, desc);
					this.currentList.Add(rfmr.Title, rfmr);
				}
			}
		}

		private void UpdateIcon(RapidFireModisRecord rfmr, string iconImagePath, bool show, bool exists )
		{
			if(!show)
			{
				if(exists)
					RemoveIconFromCurrentList(rfmr);
				return;
			}

			if(!exists)
				AddIconToCurrentList(rfmr, iconImagePath);
		}

		public void UpdateIcons()
		{
			if(this.iconSet == null)
			{
				this.iconSet = new 
					DownloadableImageFromIconSet(this.Name,
					this.worldWindow1.CurrentWorld, 0.0f, this.worldWindow1.DrawArgs, this.worldWindow1.CurrentWorld.TerrainAccessor);
				this.iconSet.IsOn = true;
				this.worldWindow1.CurrentWorld.RenderableObjects.Add( this.iconSet );
			}

			if(this.radioButtonDisplayNone.Checked)
			{
				this.ClearAllIcons();
				this.worldWindow1.Invalidate();
				return;
			}

			lock(this.modisList.SyncRoot)
			{
				foreach(string key in this.modisList.Keys)
				{
					RapidFireModisRecord rfmr = (RapidFireModisRecord)this.modisList[key];

					DateTime date = DateTime.ParseExact(rfmr.dateString, "M/d/yyyy", CultureInfo.InvariantCulture);
					if(date > this.dateTimePickerEndDate.Value || date < this.dateTimePickerBeginDate.Value)
					{
						//date out of range...
						if(this.currentList.Contains(rfmr.Title))
							this.RemoveIconFromCurrentList(rfmr);
						continue;
					}

					bool exists;
					lock(this.currentList.SyncRoot)
					{
						exists = this.currentList.Contains(rfmr.Title);
					}

					string desc = rfmr.Description.ToLower();
					if(desc.IndexOf("fire") != -1)
						UpdateIcon( rfmr, "modis-fire.png", this.showFires, exists );
					else if(desc.IndexOf("flood") != -1)
						UpdateIcon( rfmr, "modis-flood.png", this.showFloods, exists );
					else if(desc.IndexOf("smoke") != -1 || desc.IndexOf("dust") != -1)
						UpdateIcon( rfmr, "modis-dustsmoke.png", this.showDust, exists );
					else if(desc.IndexOf("storm") != -1 ||
						desc.IndexOf("blizzard") != -1 ||
						desc.IndexOf("typhoon") != -1 ||
						desc.IndexOf("hurricane") != -1 )
					{
						UpdateIcon( rfmr, "modis-storm.png", this.showStorms, exists );
					}
					else if(desc.IndexOf("volcano") != -1)
						UpdateIcon( rfmr, "modis-volcano.png", this.showVolcanoes, exists );
					else
						UpdateIcon( rfmr, "modis-misc.png", this.showMisc, exists );
				}
			}

			this.worldWindow1.Invalidate();
		}

		public void Reset()
		{
			if(this.iconSet != null)
			{
				this.worldWindow1.CurrentWorld.RenderableObjects.Remove(this.iconSet.Name);
				this.iconSet.Dispose();
				this.iconSet = null;
			}
			this.Visible = false;
		}

		private void loadSavedFileList(string filePath)
		{
			if(!File.Exists(filePath))
				return;

			using( StreamReader fileReader = new StreamReader(filePath, System.Text.Encoding.UTF8) )
			{
				try
				{
					string line = fileReader.ReadLine();
					while(line != null)
					{
						RapidFireModisRecord rfmr = new RapidFireModisRecord();
						rfmr.Title = line.Trim();
						rfmr.Description = fileReader.ReadLine().Trim().Replace("<br>", Environment.NewLine);
						rfmr.dateString = fileReader.ReadLine().Trim();
						string imageLine = fileReader.ReadLine().Trim();
						rfmr.ImagePaths = imageLine.Split('*');

						rfmr.west = Single.Parse(fileReader.ReadLine().Trim(), CultureInfo.InvariantCulture);
						rfmr.south = Single.Parse(fileReader.ReadLine().Trim(), CultureInfo.InvariantCulture);
						rfmr.east = Single.Parse(fileReader.ReadLine().Trim(), CultureInfo.InvariantCulture);
						rfmr.north = Single.Parse(fileReader.ReadLine().Trim(), CultureInfo.InvariantCulture);

						lock(this.modisList.SyncRoot)
						{
							this.modisList.Add(rfmr.Title, rfmr);
						}
						line = fileReader.ReadLine();
					}
				}			
				catch
				{
					if(File.Exists(filePath))
						File.Delete(filePath);
				}
			}
		}

		private static int getNumberUseableImagesFromList(string filename)
		{
			try
			{
				if(!File.Exists(filename))
					return 0;
				
				int numRecordsUsable = 0;
				using(StreamReader reader = new StreamReader(filename, System.Text.Encoding.UTF8))
				{
					int lineNumber = 0;
					
					string curRecordLine = "";
					string curImagesLine = "";
					string curWorldFilesLine = "";
					string curCaptionLine = "";
					string curProjectionLine = "";
					
					string line = reader.ReadLine();
					while(line != null)
					{
						if(line.IndexOf("record:") != -1)
						{
							if(lineNumber != 0)
							{
								// process the record somewhere
								if(curProjectionLine.IndexOf("Plate Carree") > 0 && curWorldFilesLine.Length > 3)
								{
									string[] parsedImagesLine = curImagesLine.Split(':');
									if(parsedImagesLine.Length == 2)
									{
										string[] parsedImages = parsedImagesLine[1].Trim().Split(' ');

										if(parsedImages.Length > 0)
										{
											numRecordsUsable++;
										}
									}	
								}
							}
							
							curRecordLine = line;
							curImagesLine = "";
							curWorldFilesLine = "";
							curCaptionLine = "";
							curProjectionLine = "";
						}
						else if(line.IndexOf("world files:") != -1)
						{
							curWorldFilesLine = line;
						}
						else if(line.IndexOf("images:") != -1)
						{
							curImagesLine = line;
						}
						else if(line.IndexOf("projection:") != -1)
						{
							curProjectionLine = line;
						}
						else if(line.IndexOf("caption:") != -1)
						{
							curCaptionLine = line;
						}

						line = reader.ReadLine();
						lineNumber++;	
					}
				}
				return numRecordsUsable;
			}
			catch
			{
			}
			return 0;
		}

		private void parseImageListFile(string filename)
		{
			updateProgressBar(0, RapidFireModisManager.getNumberUseableImagesFromList(this.imageListFilePath));
			using(StreamReader reader = new StreamReader(filename, System.Text.Encoding.UTF8))
			{
				if(!Directory.Exists(Path.GetDirectoryName(filename) + "\\Info Files"))
					Directory.CreateDirectory(Path.GetDirectoryName(filename) + "\\Info Files");

				NameValueCollection fields = new NameValueCollection();
				string line = reader.ReadLine(); // Skip first line
				while (true)
				{
					line = reader.ReadLine();
					if(line == null)
						break;
					if(line.Trim().Length<=0)
					{
						ProcessRecord(fields);
						fields = new NameValueCollection();
						updateProgressBar( progressBar.Value+1, progressBar.Maximum);
						continue;
					}

					string[] tokens = line.Split(new char[]{':'},2);
					if(tokens.Length<2)
						continue;

					string key = tokens[0].Trim().ToLower();
					string value = tokens[1].Trim();
					fields.Add( key, value );
				}
			}
			this.labelStatus.Text = "Done";
		}

		void ProcessRecord( NameValueCollection fields )
		{
			// process the record somewhere
			string projection = fields["projection"];
			if (projection == null)
				return;

			// We currently only support "geographic projection
			if (projection.IndexOf("Plate Carree") < 0)
				return;
			
			string worldFiles = fields["world files"];
			if (worldFiles == null)
				return;
			if(worldFiles.Length <= 3)
				return;
							
			string images = fields["images"];
			if (images==null)
				return;
			string[] parsedImages = images.Trim().Split(' ');

			if (parsedImages.Length <= 0)
				return;
									
			string title = fields["record"];
			if (title == null)
				return;

			RapidFireModisRecord rfmr = new RapidFireModisRecord();
			rfmr.Title = title.Trim();
			rfmr.ImagePaths = parsedImages;

			string rawDateString = rfmr.Title.Split('.')[1].Substring(1);
			DateTime dt = new DateTime(
				Int32.Parse(rawDateString.Substring(0, 4)), 1, 1) + 
				System.TimeSpan.FromDays(Double.Parse(rawDateString.Substring(4, 3)) - 1);

// TODO: Incremental download (date range)
//			if (dateTimePickerBeginDate.Value > dt)
//				return;
			rfmr.dateString = String.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2}", dt.Month, dt.Day, dt.Year);

			// We need the info file for each record....
			string moreInfoUrl = "http://rapidfire.sci.gsfc.nasa.gov/gallery/" + parsedImages[0].Split('/')[0].Trim() + "/" + rfmr.Title + ".txt";

			lock (this.modisList.SyncRoot)
			{
				if (this.modisList.Contains(rfmr.Title))
					return;
			}

			if (!File.Exists(rfmr.DetailFilePath))
			{
				try
				{
					this.labelStatus.Text = string.Format(CultureInfo.CurrentCulture,
						"Downloading {0}/{1}",
						progressBar.Value,progressBar.Maximum);
					using (WebDownload client = new WebDownload(moreInfoUrl))
						client.DownloadFile(rfmr.DetailFilePath);
				}
				catch (Exception)
				{
					//Delete the file in case it was only partially saved...
					if (File.Exists(rfmr.DetailFilePath))
						File.Delete(rfmr.DetailFilePath);
					return;
				}
			}

			if (new FileInfo(rfmr.DetailFilePath).Length<=0)
				return;

			ReadDetails( ref rfmr );

			lock (this.modisList.SyncRoot)
			{
				this.modisList.Add(rfmr.Title, rfmr);
			}

			using (StreamWriter sw = new StreamWriter(CacheDirectory + "\\CachedModisListNew.txt", true, System.Text.Encoding.UTF8))
			{
				sw.WriteLine(rfmr.Title);
				sw.WriteLine(rfmr.Description);
				sw.WriteLine(rfmr.dateString);
				sw.Write(rfmr.ImagePaths[0]);
				for (int image = 1; image < rfmr.ImagePaths.Length; image++)
					sw.Write("*" + rfmr.ImagePaths[image]);
				sw.Write(Environment.NewLine);
				sw.WriteLine(rfmr.west.ToString(CultureInfo.InvariantCulture));
				sw.WriteLine(rfmr.south.ToString(CultureInfo.InvariantCulture));
				sw.WriteLine(rfmr.east.ToString(CultureInfo.InvariantCulture));
				sw.WriteLine(rfmr.north.ToString(CultureInfo.InvariantCulture));
			}
		}

		private static void ReadDetails( ref RapidFireModisRecord rfmr )
		{
			using (StreamReader sr = new StreamReader(rfmr.DetailFilePath, System.Text.Encoding.UTF8))
			{
				rfmr.Description = sr.ReadLine();
				string regionString = sr.ReadLine();

				if (regionString.IndexOf("region:") != -1)
				{
					rfmr.regionCode = regionString.Split(':')[1].Trim();
				}

				string temp = sr.ReadLine();
				while (temp.IndexOf("projection:") == -1 && temp.IndexOf("sat:") == -1)
				{
					rfmr.Description += " [" + temp + "]";
					temp = sr.ReadLine();
				}

				while (temp.IndexOf("sat:") != -1)
				{
					if (rfmr.satellite != null && rfmr.satellite.Length > 2)
						rfmr.satellite += " & " + temp.Split(':')[1].Trim();
					else
						rfmr.satellite = temp.Split(':')[1].Trim();

					temp = sr.ReadLine();
				}

				if (temp.IndexOf("projection:") != -1)
				{
					rfmr.Projection = temp.Split(':')[1].Trim();
					temp = sr.ReadLine();
				}
				else
				{
					Log.Write(Log.Levels.Error, "RFMR", "Problem parsing record '" + rfmr.Title + "'.");
				}

				while (temp != null)
				{
					string n = (temp.Split(':')[1].Replace("+", "").Trim());
					if (temp.IndexOf("UL lon:") != -1)
					{
						rfmr.west = Single.Parse(n, CultureInfo.InvariantCulture );
					}
					else if (temp.IndexOf("UL lat:") != -1)
					{
						rfmr.north = Single.Parse(n, CultureInfo.InvariantCulture );
					}
					else if (temp.IndexOf("LR lon:") != -1)
					{
						rfmr.east = Single.Parse(n, CultureInfo.InvariantCulture );
					}
					else if (temp.IndexOf("LR lat:") != -1)
					{
						rfmr.south = Single.Parse(n, CultureInfo.InvariantCulture );
					}

					temp = sr.ReadLine();
				}
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			if(this.Visible)
			{
				if (WindowState != System.Windows.Forms.FormWindowState.Normal)
					WindowState = System.Windows.Forms.FormWindowState.Normal;

				// Clear status area
				labelStatus.Text = "";
				updateProgressBar(0,0);

				// TODO: Restart list update on "reopening" window?
			}
			else
			{
				this.worldWindow1.Focus();
			}
			base.OnVisibleChanged (e);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			// Shut down list update operation
			if (imageListUpdateThread!=null && imageListUpdateThread.IsAlive)
				imageListUpdateThread.Abort();

			e.Cancel = true;
			this.Hide();
			this.worldWindow1.Focus();

			base.OnClosing(e);
		}

		protected override void OnKeyUp(System.Windows.Forms.KeyEventArgs e) 
		{
			switch(e.KeyCode) 
			{
				case Keys.F2:
				case Keys.Escape:
					Close();
					e.Handled = true;
					break;
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

		public void DownloadImageListUpdate()
		{
			string imageListURL = "http://rapidfire.sci.gsfc.nasa.gov/gallery/WWinventory.txt";
			try
			{
				this.loadSavedFileList(CacheDirectory + "\\CachedModisListNew.txt");
				this.UpdateIcons();
				if(File.Exists(this.imageListFilePath))
				{
					this.labelStatus.Text = "Parsing cached image list...";
					this.parseImageListFile(this.imageListFilePath);
					this.UpdateIcons();
				}

				this.labelStatus.Text = "Downloading new image list...";
				using( WebDownload webDownload = new WebDownload(imageListURL) )
				{
					webDownload.ProgressCallback += new DownloadProgressHandler(updateProgressBar);
					webDownload.DownloadFile(this.imageListFilePath);
				}

				this.labelStatus.Text = "Parsing cached image list...";

				this.parseImageListFile(this.imageListFilePath);
				this.UpdateIcons();

				this.labelStatus.Text = "";
				this.progressBar.Value = 0;
			}
			catch(ThreadAbortException)
			{
				// Shut down by user
			}
			catch(Exception caught)
			{
				Log.Write(caught);
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

		private class RapidFireModisRecord
		{
			string title;
			string description;
			string[] imagePaths;
			public string dateString;
			string moreInfoTextFilePath;
			string projection;
			string captionTextFilePath;
			public string satellite;
			public string regionCode;

			public float west;
			public float east;
			public float north;
			public float south;

			public string Title
			{
				get 
				{
					return this.title;
				}
				set
				{
					this.title = value;
				}
			}

			public string Description
			{
				get
				{
					return this.description;
				}
				set
				{
					this.description = value;
				}
			}
			
			public string[] ImagePaths
			{
				get
				{
					return this.imagePaths;
				}
				set
				{
					this.imagePaths = value;
				}
			}

			public string MoreInfoTextFilePath
			{
				get
				{
					return this.moreInfoTextFilePath;
				}
				set
				{
					this.moreInfoTextFilePath = value;
				}
			}

			public string Projection
			{
				get
				{
					return this.projection;
				}
				set
				{
					this.projection = value;
				}
			}

			public string CaptionTextFilePath
			{
				get
				{
					return this.captionTextFilePath;
				}
				set
				{
					this.captionTextFilePath = value;
				}
			}

			public string DetailFilePath
			{
				get
				{
					string detailFilePath = String.Format(@"{0}\{1}\{2}.txt",
						RapidFireModisManager.CacheDirectory,
						"Info Files",
						this.Title);
					return detailFilePath;
				}
			}

			public override string ToString()
			{
				return this.title;
			}
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(RapidFireModisManager));
			System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("Fires", 0);
			System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("Floods", 1);
			System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("Dust & Smoke", 2);
			System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem("Storms", 3);
			System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem("Volcanoes", 4);
			System.Windows.Forms.ListViewItem listViewItem6 = new System.Windows.Forms.ListViewItem("Other", 5);
			this.imageList = new System.Windows.Forms.ImageList(this.components);
			this.dateTimePickerEndDate = new System.Windows.Forms.DateTimePicker();
			this.dateTimePickerBeginDate = new System.Windows.Forms.DateTimePicker();
			this.panel1 = new System.Windows.Forms.Panel();
			this.labelStatus = new System.Windows.Forms.Label();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.groupBoxDateRange = new System.Windows.Forms.GroupBox();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBoxResolution = new System.Windows.Forms.GroupBox();
			this.radioButtonResolution250m = new System.Windows.Forms.RadioButton();
			this.radioButtonResolution500m = new System.Windows.Forms.RadioButton();
			this.radioButtonResolution1km = new System.Windows.Forms.RadioButton();
			this.groupBoxDisplay = new System.Windows.Forms.GroupBox();
			this.radioButtonDisplayTourMode = new System.Windows.Forms.RadioButton();
			this.radioButtonDisplayNone = new System.Windows.Forms.RadioButton();
			this.radioButtonDisplaySelected = new System.Windows.Forms.RadioButton();
			this.radioButtonDisplayGroup = new System.Windows.Forms.RadioButton();
			this.listView1 = new System.Windows.Forms.ListView();
			this.imageList2 = new System.Windows.Forms.ImageList(this.components);
			this.panel1.SuspendLayout();
			this.groupBoxDateRange.SuspendLayout();
			this.groupBoxResolution.SuspendLayout();
			this.groupBoxDisplay.SuspendLayout();
			this.SuspendLayout();
			// 
			// imageList
			// 
			this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imageList.ImageSize = new System.Drawing.Size(32, 32);
			this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
			this.imageList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// dateTimePickerEndDate
			// 
			this.dateTimePickerEndDate.CustomFormat = "yyyyMMdd";
			this.dateTimePickerEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimePickerEndDate.Location = new System.Drawing.Point(24, 63);
			this.dateTimePickerEndDate.Name = "dateTimePickerEndDate";
			this.dateTimePickerEndDate.Size = new System.Drawing.Size(110, 20);
			this.dateTimePickerEndDate.TabIndex = 2;
			this.dateTimePickerEndDate.ValueChanged += new System.EventHandler(this.dateTimePickerEndDate_ValueChanged);
			// 
			// dateTimePickerBeginDate
			// 
			this.dateTimePickerBeginDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimePickerBeginDate.Location = new System.Drawing.Point(24, 19);
			this.dateTimePickerBeginDate.Name = "dateTimePickerBeginDate";
			this.dateTimePickerBeginDate.Size = new System.Drawing.Size(110, 20);
			this.dateTimePickerBeginDate.TabIndex = 0;
			this.dateTimePickerBeginDate.ValueChanged += new System.EventHandler(this.dateTimePickerBeginDate_ValueChanged);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.labelStatus);
			this.panel1.Controls.Add(this.progressBar);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 246);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(418, 24);
			this.panel1.TabIndex = 15;
			// 
			// labelStatus
			// 
			this.labelStatus.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelStatus.Location = new System.Drawing.Point(0, 0);
			this.labelStatus.Name = "labelStatus";
			this.labelStatus.Size = new System.Drawing.Size(290, 24);
			this.labelStatus.TabIndex = 0;
			this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// progressBar
			// 
			this.progressBar.Dock = System.Windows.Forms.DockStyle.Right;
			this.progressBar.Location = new System.Drawing.Point(290, 0);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(128, 24);
			this.progressBar.TabIndex = 1;
			// 
			// groupBoxDateRange
			// 
			this.groupBoxDateRange.Controls.Add(this.label1);
			this.groupBoxDateRange.Controls.Add(this.dateTimePickerBeginDate);
			this.groupBoxDateRange.Controls.Add(this.dateTimePickerEndDate);
			this.groupBoxDateRange.Location = new System.Drawing.Point(8, 144);
			this.groupBoxDateRange.Name = "groupBoxDateRange";
			this.groupBoxDateRange.Size = new System.Drawing.Size(160, 96);
			this.groupBoxDateRange.TabIndex = 0;
			this.groupBoxDateRange.TabStop = false;
			this.groupBoxDateRange.Text = "Date Range";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(24, 40);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(112, 23);
			this.label1.TabIndex = 1;
			this.label1.Text = "To:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// groupBoxResolution
			// 
			this.groupBoxResolution.Controls.Add(this.radioButtonResolution250m);
			this.groupBoxResolution.Controls.Add(this.radioButtonResolution500m);
			this.groupBoxResolution.Controls.Add(this.radioButtonResolution1km);
			this.groupBoxResolution.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.groupBoxResolution.Location = new System.Drawing.Point(296, 8);
			this.groupBoxResolution.Name = "groupBoxResolution";
			this.groupBoxResolution.Size = new System.Drawing.Size(112, 96);
			this.groupBoxResolution.TabIndex = 1;
			this.groupBoxResolution.TabStop = false;
			this.groupBoxResolution.Text = "Resolution";
			// 
			// radioButtonResolution250m
			// 
			this.radioButtonResolution250m.Location = new System.Drawing.Point(8, 64);
			this.radioButtonResolution250m.Name = "radioButtonResolution250m";
			this.radioButtonResolution250m.Size = new System.Drawing.Size(96, 24);
			this.radioButtonResolution250m.TabIndex = 2;
			this.radioButtonResolution250m.Text = "250m (Slower)";
			this.radioButtonResolution250m.CheckedChanged += new System.EventHandler(this.radioButtonResolution2km_CheckedChanged);
			// 
			// radioButtonResolution500m
			// 
			this.radioButtonResolution500m.Location = new System.Drawing.Point(8, 40);
			this.radioButtonResolution500m.Name = "radioButtonResolution500m";
			this.radioButtonResolution500m.Size = new System.Drawing.Size(96, 24);
			this.radioButtonResolution500m.TabIndex = 1;
			this.radioButtonResolution500m.Text = "500m";
			this.radioButtonResolution500m.CheckedChanged += new System.EventHandler(this.radioButtonResolution2km_CheckedChanged);
			// 
			// radioButtonResolution1km
			// 
			this.radioButtonResolution1km.Checked = true;
			this.radioButtonResolution1km.Location = new System.Drawing.Point(8, 16);
			this.radioButtonResolution1km.Name = "radioButtonResolution1km";
			this.radioButtonResolution1km.Size = new System.Drawing.Size(96, 24);
			this.radioButtonResolution1km.TabIndex = 0;
			this.radioButtonResolution1km.TabStop = true;
			this.radioButtonResolution1km.Text = "1km (Faster)";
			this.radioButtonResolution1km.CheckedChanged += new System.EventHandler(this.radioButtonResolution2km_CheckedChanged);
			// 
			// groupBoxDisplay
			// 
			this.groupBoxDisplay.Controls.Add(this.radioButtonDisplayTourMode);
			this.groupBoxDisplay.Controls.Add(this.radioButtonDisplayNone);
			this.groupBoxDisplay.Controls.Add(this.radioButtonDisplaySelected);
			this.groupBoxDisplay.Controls.Add(this.radioButtonDisplayGroup);
			this.groupBoxDisplay.Location = new System.Drawing.Point(296, 112);
			this.groupBoxDisplay.Name = "groupBoxDisplay";
			this.groupBoxDisplay.Size = new System.Drawing.Size(112, 128);
			this.groupBoxDisplay.TabIndex = 2;
			this.groupBoxDisplay.TabStop = false;
			this.groupBoxDisplay.Text = "Display Mode";
			// 
			// radioButtonDisplayTourMode
			// 
			this.radioButtonDisplayTourMode.Location = new System.Drawing.Point(9, 66);
			this.radioButtonDisplayTourMode.Name = "radioButtonDisplayTourMode";
			this.radioButtonDisplayTourMode.Size = new System.Drawing.Size(96, 24);
			this.radioButtonDisplayTourMode.TabIndex = 3;
			this.radioButtonDisplayTourMode.Text = "Tour Mode";
			this.radioButtonDisplayTourMode.CheckedChanged += new System.EventHandler(this.radioButtonDisplayTourMode_CheckedChanged);
			// 
			// radioButtonDisplayNone
			// 
			this.radioButtonDisplayNone.Location = new System.Drawing.Point(9, 90);
			this.radioButtonDisplayNone.Name = "radioButtonDisplayNone";
			this.radioButtonDisplayNone.Size = new System.Drawing.Size(96, 24);
			this.radioButtonDisplayNone.TabIndex = 2;
			this.radioButtonDisplayNone.Text = "None";
			this.radioButtonDisplayNone.CheckedChanged += new System.EventHandler(this.radioButtonDisplayNone_CheckedChanged);
			// 
			// radioButtonDisplaySelected
			// 
			this.radioButtonDisplaySelected.Location = new System.Drawing.Point(9, 42);
			this.radioButtonDisplaySelected.Name = "radioButtonDisplaySelected";
			this.radioButtonDisplaySelected.Size = new System.Drawing.Size(96, 24);
			this.radioButtonDisplaySelected.TabIndex = 1;
			this.radioButtonDisplaySelected.Text = "Only Selected";
			this.radioButtonDisplaySelected.CheckedChanged += new System.EventHandler(this.radioButtonDisplaySelected_CheckedChanged);
			// 
			// radioButtonDisplayGroup
			// 
			this.radioButtonDisplayGroup.Checked = true;
			this.radioButtonDisplayGroup.Location = new System.Drawing.Point(9, 18);
			this.radioButtonDisplayGroup.Name = "radioButtonDisplayGroup";
			this.radioButtonDisplayGroup.Size = new System.Drawing.Size(96, 24);
			this.radioButtonDisplayGroup.TabIndex = 0;
			this.radioButtonDisplayGroup.TabStop = true;
			this.radioButtonDisplayGroup.Text = "Entire Group";
			// 
			// listView1
			// 
			this.listView1.CheckBoxes = true;
			this.listView1.FullRowSelect = true;
			listViewItem1.StateImageIndex = 0;
			listViewItem2.StateImageIndex = 0;
			listViewItem3.StateImageIndex = 0;
			listViewItem4.StateImageIndex = 0;
			listViewItem5.StateImageIndex = 0;
			listViewItem6.StateImageIndex = 0;
			this.listView1.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
																					  listViewItem1,
																					  listViewItem2,
																					  listViewItem3,
																					  listViewItem4,
																					  listViewItem5,
																					  listViewItem6});
			this.listView1.LargeImageList = this.imageList2;
			this.listView1.Location = new System.Drawing.Point(8, 8);
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(264, 128);
			this.listView1.SmallImageList = this.imageList;
			this.listView1.TabIndex = 3;
			this.listView1.View = System.Windows.Forms.View.List;
			this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
			this.listView1.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.listView1_ItemCheck);
			// 
			// imageList2
			// 
			this.imageList2.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imageList2.ImageSize = new System.Drawing.Size(64, 64);
			this.imageList2.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList2.ImageStream")));
			this.imageList2.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// RapidFireModisManager
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(418, 270);
			this.Controls.Add(this.listView1);
			this.Controls.Add(this.groupBoxDisplay);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.groupBoxDateRange);
			this.Controls.Add(this.groupBoxResolution);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimumSize = new System.Drawing.Size(302, 297);
			this.Name = "RapidFireModisManager";
			this.Text = "Rapid Fire Modis";
			this.VisibleChanged += new System.EventHandler(this.RapidFireModisManager_VisibleChanged);
			this.panel1.ResumeLayout(false);
			this.groupBoxDateRange.ResumeLayout(false);
			this.groupBoxResolution.ResumeLayout(false);
			this.groupBoxDisplay.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// Updates progress bar (thread safe)
		/// </summary>
		private void updateProgressBar(int value, int maximum) 
		{
			// Make sure we're on the right thread
			if( this.InvokeRequired ) 
			{
				// Update progress asynchronously
				DownloadProgressHandler dlgt =
					new DownloadProgressHandler(updateProgressBar);
				this.BeginInvoke(dlgt, new object[] { value, maximum });
				return;
			}

			if(value < 0)
				value = 0;
			if(maximum < 0)
				maximum = 0;
			this.progressBar.Maximum = maximum;
			this.progressBar.Value = value<=maximum ? value : maximum;
		}

		private void checkedListBox1_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			bool show = e.NewValue == CheckState.Checked;
			switch(e.Index)
			{
				case 0:
					this.showFires = show;
					break;
				case 1:
					this.showFloods = show;
					break;
				case 2:
					this.showDust = show;
					break;
				case 3:
					this.showStorms = show;
					break;
				case 4:
					this.showVolcanoes = show;
					break;
				case 5:
					this.showMisc = show;
					break;
			}
			this.UpdateIcons();
			this.worldWindow1.Invalidate();
		}

		private void dateTimePickerBeginDate_ValueChanged(object sender, System.EventArgs e)
		{
			this.UpdateIcons();
		}

		private void dateTimePickerEndDate_ValueChanged(object sender, System.EventArgs e)
		{
			this.UpdateIcons();
		}

		private void radioButtonResolution2km_CheckedChanged(object sender, System.EventArgs e)
		{
			this.ClearAllIcons();
			this.UpdateIcons();
			this.worldWindow1.Invalidate();
		}

		public void ClearAllIcons()
		{
			lock(this.currentList.SyncRoot)
			{
				foreach(string key in this.currentList.Keys)
				{
					this.iconSet.RemoveDownloadableIcon(key);
				}

				this.currentList.Clear();
			}
		}

		private void radioButtonDisplaySelected_CheckedChanged(object sender, System.EventArgs e)
		{
			this.iconSet.ShowOnlyCurrentlySelected = this.radioButtonDisplaySelected.Checked;
		}

		private void radioButtonDisplayNone_CheckedChanged(object sender, System.EventArgs e)
		{
			if(this.radioButtonDisplayNone.Checked)
			{
				this.ClearAllIcons();
			}
			else
			{
				this.ClearAllIcons();
				this.UpdateIcons();
			}
		}

		private void radioButtonDisplayTourMode_CheckedChanged(object sender, System.EventArgs e)
		{
			if(this.radioButtonDisplayTourMode.Checked)
			{
				this.groupBoxDateRange.Enabled = false;
				this.groupBoxResolution.Enabled = false;
				this.listView1.Enabled = false;
				
				this.currentTourScenes = new ArrayList();
				foreach(string key in this.currentList.Keys)
					this.currentTourScenes.Add(key);

				if (this.currentTourScenes.Count == 0)			// No MODIS available
				{
					this.groupBoxDateRange.Enabled = true;
					this.groupBoxResolution.Enabled = true;
					this.listView1.Enabled = true;
					this.radioButtonDisplayTourMode.Checked = false;
					this.radioButtonDisplayGroup.Checked = true;

					return;
				}

				int interval;
				if(this.radioButtonResolution1km.Checked)
				{
					interval = 20000;
				}
				else if(this.radioButtonResolution500m.Checked)
				{
					interval = 40000;
				}
				else
					interval = 60000;

				this.tourModeTimer.Interval = interval;

				string currentKey = (string)currentTourScenes[currentTourScene];
				RapidFireModisRecord rfmr = (RapidFireModisRecord)this.currentList[currentKey];
				if(rfmr != null)
				{
					this.worldWindow1.GotoLatLonHeadingViewRange(0.5f*(rfmr.north + rfmr.south), 0.5f*(rfmr.west + rfmr.east), 0, 180.0f);
					this.iconSet.LoadDownloadableIcon(currentKey);
					this.worldWindow1.Invalidate();
					Thread.Sleep(1500);
					this.worldWindow1.GotoLatLonHeadingViewRange(0.5f*(rfmr.north + rfmr.south), 0.5f*(rfmr.west + rfmr.east), 0, 2.5f*(rfmr.north - rfmr.south));
					this.worldWindow1.Invalidate();
				}
				if(++currentTourScene >= currentTourScenes.Count)
					currentTourScene = 0;

				this.tourModeTimer.Start();

				this.iconSet.ShowOnlyCurrentlySelected = true;
			}
			else
			{
				this.groupBoxDateRange.Enabled = true;
				this.groupBoxResolution.Enabled = true;
				this.listView1.Enabled = true;
				this.tourModeTimer.Stop();
				this.currentTourScenes.Clear();
				this.iconSet.ShowOnlyCurrentlySelected = false;
			}
		}

		private void listView1_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			bool enable = e.NewValue == CheckState.Checked;
			switch(e.Index)
			{
				case 0:
					this.showFires = enable;
					break;
				case 1:
					this.showFloods = enable;
					break;
				case 2:
					this.showDust = enable;
					break;
				case 3:
					this.showStorms = enable;
					break;
				case 4:
					this.showVolcanoes = enable;
					break;
				case 5:
					this.showMisc = enable;
					break;
			}
		
			this.UpdateIcons();
			this.worldWindow1.Invalidate();
		}

		private void RapidFireModisManager_VisibleChanged(object sender, System.EventArgs e)
		{
			if(this.Visible)
			{
				this.UpdateIcons();
			}
			else
			{
				this.currentList.Clear();
				if(this.iconSet != null)
				{
					this.worldWindow1.CurrentWorld.RenderableObjects.Remove(this.iconSet.Name);
				
					this.iconSet.Dispose();
					this.iconSet = null;
				}
			}
		}

		private void tourModeTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			string currentKey = (string)currentTourScenes[currentTourScene];
			RapidFireModisRecord rfmr = (RapidFireModisRecord)this.currentList[currentKey];
			if(rfmr != null)
			{
				this.worldWindow1.GotoLatLonHeadingViewRange(0.5f*(rfmr.north + rfmr.south), 0.5f*(rfmr.west + rfmr.east), 0, 180.0f);
				this.iconSet.LoadDownloadableIcon(currentKey);
				this.worldWindow1.Invalidate();
				Thread.Sleep(1500);
				this.worldWindow1.GotoLatLonHeadingViewRange(0.5f*(rfmr.north + rfmr.south), 0.5f*(rfmr.west + rfmr.east), 0, 2.5f*(rfmr.north - rfmr.south));
				this.worldWindow1.Invalidate();
			}
			if(++currentTourScene == currentTourScenes.Count)
				currentTourScene = 0;
		}

		private void listView1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		
		}
	}
}