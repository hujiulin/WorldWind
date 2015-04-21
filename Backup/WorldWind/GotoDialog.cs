using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System;
using WorldWind.Configuration;
using WorldWind.Renderable;
using WorldWind;

namespace WorldWind
{
   /// <summary>
   /// Place Finder (placename search) dialog
   /// </summary>
   public class GotoDialog : System.Windows.Forms.Form
   {
      private System.Windows.Forms.TextBox textBoxSearchKeywords;
      private System.Windows.Forms.Label labelLookfor;
      private System.Windows.Forms.Button buttonSearch;
      private System.Windows.Forms.Button buttonStop;
      private System.Windows.Forms.Button buttonGo;
      private System.Windows.Forms.Label labelLatitude;
      private System.Windows.Forms.NumericUpDown numericUpDownLatitude;
      private System.Windows.Forms.Label labelLongitude;
      private System.Windows.Forms.NumericUpDown numericUpDownLongitude;
      private System.Windows.Forms.Label labelRange;
      private System.Windows.Forms.NumericUpDown numericUpDownAltitude;
      private System.Windows.Forms.GroupBox groupBox1;
      private System.Windows.Forms.StatusBar statusBar;
      private System.Windows.Forms.StatusBarPanel statusBarPanel;
      private System.Windows.Forms.StatusBarPanel progressPanel;
      private System.Windows.Forms.ProgressBar progressBarSearch;

      WorldWindow worldWindow = null;
      //WorldXmlDescriptor.WorldType currentWorld = null;
      private System.Windows.Forms.CheckBox checkBoxFastSearch;
      private System.Windows.Forms.ToolTip toolTip1;
      private System.ComponentModel.IContainer components;
      private System.Windows.Forms.TabPage tabPageResults;
      private WWPlaceListView listViewResults;
      private System.Windows.Forms.TabControl tabControlLists;
      private System.Windows.Forms.TabPage tabPageFavorites;
      private WWPlaceListView listViewFavorites;
      private System.Windows.Forms.TabPage tabPageHistory;
      private WWPlaceListView listViewHistory;

      const int maxPlaceResults = 1000;
      const string LogCategory = "GOTO";

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

      // ------------------------------------------------------------
      // *** NOTE: if you get an error CS0234: in the 
      //           Windows Form Designer code in this line:
      //
      // this.listViewResults = new WorldWind.WWPlaceListView();
      // 
      // remove the "WorldWind." prefix the Designer insists on adding
      // (Same for listViewFavorites and listViewHistory)
      // ------------------------------------------------------------
      
      #region Windows Form Designer generated code
      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         this.components = new System.ComponentModel.Container();
         this.textBoxSearchKeywords = new System.Windows.Forms.TextBox();
         this.labelLookfor = new System.Windows.Forms.Label();
         this.buttonSearch = new System.Windows.Forms.Button();
         this.buttonGo = new System.Windows.Forms.Button();
         this.groupBox1 = new System.Windows.Forms.GroupBox();
         this.numericUpDownLatitude = new System.Windows.Forms.NumericUpDown();
         this.numericUpDownLongitude = new System.Windows.Forms.NumericUpDown();
         this.numericUpDownAltitude = new System.Windows.Forms.NumericUpDown();
         this.labelRange = new System.Windows.Forms.Label();
         this.labelLatitude = new System.Windows.Forms.Label();
         this.labelLongitude = new System.Windows.Forms.Label();
         this.buttonStop = new System.Windows.Forms.Button();
         this.statusBar = new System.Windows.Forms.StatusBar();
         this.statusBarPanel = new System.Windows.Forms.StatusBarPanel();
         this.progressPanel = new System.Windows.Forms.StatusBarPanel();
         this.progressBarSearch = new System.Windows.Forms.ProgressBar();
         this.checkBoxFastSearch = new System.Windows.Forms.CheckBox();
         this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
         this.tabControlLists = new System.Windows.Forms.TabControl();
         this.tabPageResults = new System.Windows.Forms.TabPage();
         this.listViewResults = new WWPlaceListView();
         this.tabPageFavorites = new System.Windows.Forms.TabPage();
         this.listViewFavorites = new WWPlaceListView();
         this.tabPageHistory = new System.Windows.Forms.TabPage();
         this.listViewHistory = new WWPlaceListView();
         this.groupBox1.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLatitude)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLongitude)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownAltitude)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.statusBarPanel)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.progressPanel)).BeginInit();
         this.tabControlLists.SuspendLayout();
         this.tabPageResults.SuspendLayout();
         this.tabPageFavorites.SuspendLayout();
         this.tabPageHistory.SuspendLayout();
         this.SuspendLayout();
         // 
         // textBoxSearchKeywords
         // 
         this.textBoxSearchKeywords.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.textBoxSearchKeywords.Location = new System.Drawing.Point(8, 323);
         this.textBoxSearchKeywords.Name = "textBoxSearchKeywords";
         this.textBoxSearchKeywords.Size = new System.Drawing.Size(152, 20);
         this.textBoxSearchKeywords.TabIndex = 1;
         this.textBoxSearchKeywords.Text = "";
         this.textBoxSearchKeywords.Enter += new System.EventHandler(this.textBoxSearchKeywords_Enter);
         // 
         // labelLookfor
         // 
         this.labelLookfor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.labelLookfor.Location = new System.Drawing.Point(8, 307);
         this.labelLookfor.Name = "labelLookfor";
         this.labelLookfor.Size = new System.Drawing.Size(144, 16);
         this.labelLookfor.TabIndex = 0;
         this.labelLookfor.Text = "&Look for place named:";
         this.labelLookfor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
         // 
         // buttonSearch
         // 
         this.buttonSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.buttonSearch.Location = new System.Drawing.Point(8, 371);
         this.buttonSearch.Name = "buttonSearch";
         this.buttonSearch.Size = new System.Drawing.Size(72, 24);
         this.buttonSearch.TabIndex = 3;
         this.buttonSearch.Text = "&Search";
         this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
         // 
         // buttonGo
         // 
         this.buttonGo.Location = new System.Drawing.Point(160, 37);
         this.buttonGo.Name = "buttonGo";
         this.buttonGo.Size = new System.Drawing.Size(64, 35);
         this.buttonGo.TabIndex = 6;
         this.buttonGo.Text = "&Go";
         this.buttonGo.Click += new System.EventHandler(this.buttonGo_Click);
         // 
         // groupBox1
         // 
         this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.groupBox1.Controls.Add(this.numericUpDownLatitude);
         this.groupBox1.Controls.Add(this.numericUpDownLongitude);
         this.groupBox1.Controls.Add(this.numericUpDownAltitude);
         this.groupBox1.Controls.Add(this.labelRange);
         this.groupBox1.Controls.Add(this.buttonGo);
         this.groupBox1.Controls.Add(this.labelLatitude);
         this.groupBox1.Controls.Add(this.labelLongitude);
         this.groupBox1.Location = new System.Drawing.Point(184, 299);
         this.groupBox1.Name = "groupBox1";
         this.groupBox1.Size = new System.Drawing.Size(232, 100);
         this.groupBox1.TabIndex = 5;
         this.groupBox1.TabStop = false;
         this.groupBox1.Text = "Position";
         // 
         // numericUpDownLatitude
         // 
         this.numericUpDownLatitude.DecimalPlaces = 3;
         this.numericUpDownLatitude.Location = new System.Drawing.Point(88, 21);
         this.numericUpDownLatitude.Maximum = new System.Decimal(new int[] {
                                                                              90000,
                                                                              0,
                                                                              0,
                                                                              196608});
         this.numericUpDownLatitude.Minimum = new System.Decimal(new int[] {
                                                                              90000,
                                                                              0,
                                                                              0,
                                                                              -2147287040});
         this.numericUpDownLatitude.Name = "numericUpDownLatitude";
         this.numericUpDownLatitude.Size = new System.Drawing.Size(64, 20);
         this.numericUpDownLatitude.TabIndex = 1;
         this.numericUpDownLatitude.KeyUp += new System.Windows.Forms.KeyEventHandler(this.PositionUpDowns_KeyUp);
         // 
         // numericUpDownLongitude
         // 
         this.numericUpDownLongitude.DecimalPlaces = 3;
         this.numericUpDownLongitude.Location = new System.Drawing.Point(88, 45);
         this.numericUpDownLongitude.Maximum = new System.Decimal(new int[] {
                                                                               180000,
                                                                               0,
                                                                               0,
                                                                               196608});
         this.numericUpDownLongitude.Minimum = new System.Decimal(new int[] {
                                                                               180000,
                                                                               0,
                                                                               0,
                                                                               -2147287040});
         this.numericUpDownLongitude.Name = "numericUpDownLongitude";
         this.numericUpDownLongitude.Size = new System.Drawing.Size(64, 20);
         this.numericUpDownLongitude.TabIndex = 3;
         this.numericUpDownLongitude.KeyUp += new System.Windows.Forms.KeyEventHandler(this.PositionUpDowns_KeyUp);
         // 
         // numericUpDownAltitude
         // 
         this.numericUpDownAltitude.Location = new System.Drawing.Point(88, 69);
         this.numericUpDownAltitude.Maximum = new System.Decimal(new int[] {
                                                                              100000000,
                                                                              0,
                                                                              0,
                                                                              0});
         this.numericUpDownAltitude.Minimum = new System.Decimal(new int[] {
                                                                              1,
                                                                              0,
                                                                              0,
                                                                              0});
         this.numericUpDownAltitude.Name = "numericUpDownAltitude";
         this.numericUpDownAltitude.Size = new System.Drawing.Size(64, 20);
         this.numericUpDownAltitude.TabIndex = 5;
         this.numericUpDownAltitude.Value = new System.Decimal(new int[] {
                                                                            10000,
                                                                            0,
                                                                            0,
                                                                            0});
         this.numericUpDownAltitude.KeyUp += new System.Windows.Forms.KeyEventHandler(this.PositionUpDowns_KeyUp);
         // 
         // labelRange
         // 
         this.labelRange.Location = new System.Drawing.Point(8, 70);
         this.labelRange.Name = "labelRange";
         this.labelRange.Size = new System.Drawing.Size(72, 23);
         this.labelRange.TabIndex = 4;
         this.labelRange.Text = "Al&titude (km):";
         this.labelRange.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // labelLatitude
         // 
         this.labelLatitude.Location = new System.Drawing.Point(32, 22);
         this.labelLatitude.Name = "labelLatitude";
         this.labelLatitude.Size = new System.Drawing.Size(48, 23);
         this.labelLatitude.TabIndex = 0;
         this.labelLatitude.Text = "L&atitude:";
         this.labelLatitude.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // labelLongitude
         // 
         this.labelLongitude.Location = new System.Drawing.Point(16, 46);
         this.labelLongitude.Name = "labelLongitude";
         this.labelLongitude.Size = new System.Drawing.Size(64, 23);
         this.labelLongitude.TabIndex = 2;
         this.labelLongitude.Text = "L&ongitude:";
         this.labelLongitude.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // buttonStop
         // 
         this.buttonStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.buttonStop.Enabled = false;
         this.buttonStop.Location = new System.Drawing.Point(88, 371);
         this.buttonStop.Name = "buttonStop";
         this.buttonStop.Size = new System.Drawing.Size(72, 23);
         this.buttonStop.TabIndex = 4;
         this.buttonStop.Text = "Sto&p";
         this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
         // 
         // statusBar
         // 
         this.statusBar.Location = new System.Drawing.Point(0, 400);
         this.statusBar.Name = "statusBar";
         this.statusBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
                                                                                     this.statusBarPanel,
                                                                                     this.progressPanel});
         this.statusBar.ShowPanels = true;
         this.statusBar.Size = new System.Drawing.Size(424, 22);
         this.statusBar.TabIndex = 6;
         this.statusBar.DrawItem += new System.Windows.Forms.StatusBarDrawItemEventHandler(this.statusBar_DrawItem);
         // 
         // progressPanel
         // 
         this.progressPanel.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
         this.progressPanel.Style = System.Windows.Forms.StatusBarPanelStyle.OwnerDraw;
         this.progressPanel.Width = 308;
         // 
         // progressBarSearch
         // 
         this.progressBarSearch.Location = new System.Drawing.Point(128, 400);
         this.progressBarSearch.Name = "progressBarSearch";
         this.progressBarSearch.Size = new System.Drawing.Size(240, 23);
         this.progressBarSearch.TabIndex = 7;
         // 
         // checkBoxFastSearch
         // 
         this.checkBoxFastSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.checkBoxFastSearch.Enabled = false;
         this.checkBoxFastSearch.Location = new System.Drawing.Point(8, 347);
         this.checkBoxFastSearch.Name = "checkBoxFastSearch";
         this.checkBoxFastSearch.Size = new System.Drawing.Size(152, 20);
         this.checkBoxFastSearch.TabIndex = 2;
         this.checkBoxFastSearch.Text = "Use fast search";
         this.toolTip1.SetToolTip(this.checkBoxFastSearch, "Fast search needs index files and will search the name column only but will do so" +
            " much faster.");
         this.checkBoxFastSearch.CheckedChanged += new System.EventHandler(this.checkBoxFastSearch_CheckedChanged);
         // 
         // tabControlLists
         // 
         this.tabControlLists.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.tabControlLists.Controls.Add(this.tabPageResults);
         this.tabControlLists.Controls.Add(this.tabPageFavorites);
         this.tabControlLists.Controls.Add(this.tabPageHistory);
         this.tabControlLists.Location = new System.Drawing.Point(2, 2);
         this.tabControlLists.Name = "tabControlLists";
         this.tabControlLists.SelectedIndex = 0;
         this.tabControlLists.Size = new System.Drawing.Size(420, 295);
         this.tabControlLists.TabIndex = 8;
         // 
         // tabPageResults
         // 
         this.tabPageResults.Controls.Add(this.listViewResults);
         this.tabPageResults.Location = new System.Drawing.Point(4, 22);
         this.tabPageResults.Name = "tabPageResults";
         this.tabPageResults.Size = new System.Drawing.Size(412, 269);
         this.tabPageResults.TabIndex = 0;
         this.tabPageResults.Text = "Results";
         // 
         // listViewResults
         // 
         this.listViewResults.Dock = System.Windows.Forms.DockStyle.Fill;
         this.listViewResults.Favorites = null;
         this.listViewResults.FullRowSelect = true;
         this.listViewResults.Location = new System.Drawing.Point(0, 0);
         this.listViewResults.Name = "listViewResults";
         this.listViewResults.RecentFinds = null;
         this.listViewResults.Size = new System.Drawing.Size(412, 269);
         this.listViewResults.TabIndex = 0;
         this.listViewResults.View = System.Windows.Forms.View.Details;
         this.listViewResults.WorldWindow = null;
         this.listViewResults.SelectedIndexChanged += new EventHandler(placelistView_SelectedIndexChanged);
         // 
         // tabPageFavorites
         // 
         this.tabPageFavorites.Controls.Add(this.listViewFavorites);
         this.tabPageFavorites.Location = new System.Drawing.Point(4, 22);
         this.tabPageFavorites.Name = "tabPageFavorites";
         this.tabPageFavorites.Size = new System.Drawing.Size(412, 269);
         this.tabPageFavorites.TabIndex = 1;
         this.tabPageFavorites.Text = "Favorites";
         // 
         // listViewFavorites
         // 
         this.listViewFavorites.Dock = System.Windows.Forms.DockStyle.Fill;
         this.listViewFavorites.Favorites = null;
         this.listViewFavorites.FullRowSelect = true;
         this.listViewFavorites.Location = new System.Drawing.Point(0, 0);
         this.listViewFavorites.Name = "listViewFavorites";
         this.listViewFavorites.RecentFinds = null;
         this.listViewFavorites.Size = new System.Drawing.Size(412, 269);
         this.listViewFavorites.TabIndex = 0;
         this.listViewFavorites.View = System.Windows.Forms.View.Details;
         this.listViewFavorites.WorldWindow = null;
         this.listViewFavorites.SelectedIndexChanged += new EventHandler(placelistView_SelectedIndexChanged);
         // 
         // tabPageHistory
         // 
         this.tabPageHistory.Controls.Add(this.listViewHistory);
         this.tabPageHistory.Location = new System.Drawing.Point(4, 22);
         this.tabPageHistory.Name = "tabPageHistory";
         this.tabPageHistory.Size = new System.Drawing.Size(412, 269);
         this.tabPageHistory.TabIndex = 2;
         this.tabPageHistory.Text = "History";
         // 
         // listViewHistory
         // 
         this.listViewHistory.Dock = System.Windows.Forms.DockStyle.Fill;
         this.listViewHistory.Favorites = null;
         this.listViewHistory.FullRowSelect = true;
         this.listViewHistory.Location = new System.Drawing.Point(0, 0);
         this.listViewHistory.Name = "listViewHistory";
         this.listViewHistory.RecentFinds = null;
         this.listViewHistory.Size = new System.Drawing.Size(412, 269);
         this.listViewHistory.TabIndex = 0;
         this.listViewHistory.View = System.Windows.Forms.View.Details;
         this.listViewHistory.WorldWindow = null;
         this.listViewHistory.SelectedIndexChanged += new EventHandler(placelistView_SelectedIndexChanged);
         // 
         // GotoDialog
         // 
         this.AcceptButton = this.buttonSearch;
         this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
         this.ClientSize = new System.Drawing.Size(424, 422);
         this.Controls.Add(this.tabControlLists);
         this.Controls.Add(this.checkBoxFastSearch);
         this.Controls.Add(this.textBoxSearchKeywords);
         this.Controls.Add(this.progressBarSearch);
         this.Controls.Add(this.statusBar);
         this.Controls.Add(this.groupBox1);
         this.Controls.Add(this.buttonStop);
         this.Controls.Add(this.buttonSearch);
         this.Controls.Add(this.labelLookfor);
         this.KeyPreview = true;
         this.MinimumSize = new System.Drawing.Size(432, 250);
         this.Name = "GotoDialog";
         this.Text = "Place Finder";
         this.Closing += new System.ComponentModel.CancelEventHandler(this.GotoDialog_Closing);
         this.Load += new System.EventHandler(this.GotoDialog_Load);
         this.groupBox1.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLatitude)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLongitude)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDownAltitude)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.statusBarPanel)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.progressPanel)).EndInit();
         this.tabControlLists.ResumeLayout(false);
         this.tabPageResults.ResumeLayout(false);
         this.tabPageFavorites.ResumeLayout(false);
         this.tabPageHistory.ResumeLayout(false);
         this.ResumeLayout(false);

      }
      #endregion

      // indexedTiledPlacenameSets has all the placename descriptors (layers) of the current world,
      // and in addition, a reference to an index descriptor for fast searching
      System.Collections.ArrayList indexedTiledPlacenameSets = new ArrayList();

      // utility function - given a layerset, add all its placenamesets to the above collection, recursing into child layers
      private void collectTiledPlacenamesSets(LayerSet.Type_LayerSet curLayerSet)
      {
         if(curLayerSet.HasTiledPlacenameSet()) // any placenames at all?
         {
            // yes, iterate over them
            for(int i = 0; i < curLayerSet.TiledPlacenameSetCount; i++)
            {
               // get tilesplacenameset
               LayerSet.Type_TiledPlacenameSet2 tpns = curLayerSet.GetTiledPlacenameSetAt(i);

               // compute full path to wpl file - WplIndex constructor needs this
               string wplFullPath = Path.Combine(MainApplication.DirectoryPath, tpns.PlacenameListFilePath.Value);

               // build an index descriptor (does not create or load the index yet)
               WplIndex idx = new WplIndex(tpns, wplFullPath);

               // an indexedTilePlacenameSet associates the index descriptor with the placenameset
               IndexedTiledPlaceNameSet ipns = new IndexedTiledPlaceNameSet(tpns, idx);

               // add them to our collection
               this.indexedTiledPlacenameSets.Add(ipns);
            }
         }
			
         // now recurse into child layers of this set and do the same
         if(curLayerSet.HasChildLayerSet())
         {
            for(int i = 0; i < curLayerSet.ChildLayerSetCount; i++)
            {
               this.collectTiledPlacenamesSets(curLayerSet.GetChildLayerSetAt(i));
            }
         }
      }

      // check if all indices are available - that means QuickFind can be done
      bool AllIndicesAvailable() 
      {
         foreach(IndexedTiledPlaceNameSet tpns in indexedTiledPlacenameSets) 
         {
            if(!tpns.wplIndex.IsAvailable) return false;            
         }

         return true;
      }

      /// <summary>
			/// Initializes a new instance of the <see cref= "T:WorldWind.GotoDialog"/> class.
      /// </summary>
      /// <param name="ww"></param>
      /// <param name="currentWorld"></param>
      /// <param name="worldsXMLFilePath"></param>
      public GotoDialog(WorldWindow ww, WorldXmlDescriptor.WorldType currentWorld, string worldsXMLFilePath)
      {
         // Required for Windows Form Designer support
         InitializeComponent();

		  /*
         // manually add the search progress bar to the statusBar controls
         this.statusBar.Controls.Add(this.progressBarSearch);
         
         // keep track of our output window and the current World.
         this.worldWindow = ww;
         this.currentWorld = currentWorld;

         this.listViewResults.WorldWindow = ww;
         this.listViewFavorites.WorldWindow = ww;
         this.listViewHistory.WorldWindow = ww;

         this.listViewResults.Favorites = this.listViewFavorites;
         this.listViewResults.RecentFinds = this.listViewHistory;

         this.listViewHistory.Favorites = this.listViewFavorites;
         this.listViewFavorites.RecentFinds = this.listViewHistory;



         // create list of tiled placenamesets for this world
         if(this.currentWorld.HasLayerDirectory())
         {
            string dirPath = this.currentWorld.LayerDirectory.Value;

            // if LayerDirectory is not an absolute path, prepend worldsXMLFilePath
            if(!Path.IsPathRooted(this.currentWorld.LayerDirectory.Value)) 
            {
               dirPath = Path.Combine( worldsXMLFilePath, dirPath );
            }

            // handle all XML files in that directory
            foreach(string layerSetFileName in Directory.GetFiles(dirPath, "*.xml"))
            {
							try
							{
								LayerSet.LayerSetDoc curLayerSetDoc = new LayerSet.LayerSetDoc();
								LayerSet.Type_LayerSet curLayerSet = new LayerSet.Type_LayerSet(curLayerSetDoc.Load(layerSetFileName));
								this.collectTiledPlacenamesSets(curLayerSet);
							}
							catch (IOException caught)
							{
								Log.Write(LogCategory, "Problem reading place names: " + caught.Message);
							}
							catch (System.Xml.XmlException)
							{
								// Malformed XML (problem already reported to user on app load - ignore)
							}
            }
         }

         // fast search can be switched on only if all indices required for QuickFind are available
         this.checkBoxFastSearch.Enabled = true;

         // make fast search the default - if possible.
         this.checkBoxFastSearch.Checked = this.AllIndicesAvailable();
		 */
      }


      // Utility function for exhaustive search: check if place meets criteria
      static bool isPlaceMatched(string[] searchTokens, WorldWindPlacename pn)
      {
         char[] delimiters = new char[] {' ','(',')',','};

         string targetString;

         if(pn.metaData != null)
         {  // concatenate all metadata, separate with spaces
            StringBuilder sb = new StringBuilder(pn.Name);
            foreach(string str in pn.metaData.Values)
            {
               sb.Append(' ');
               sb.Append(str);
            }
            targetString = sb.ToString();
         }
         else 
         {
            targetString  = pn.Name;
         }

         // now compute new target tokens
         string[] targetTokens = targetString.Split(delimiters);
         
         // Note that all searchtokens have to match before we consider a place found
         foreach(string curSearchToken in searchTokens)
         {
            bool found = false;
            foreach(string curTargetToken in targetTokens)
            {
               if(String.Compare(curSearchToken, curTargetToken, true) == 0)
               {
                  found = true;
                  break; // found this search token, move to next one
               }
            }

            // continue only if at least one target token was found
            if(!found)
               return false;
         }

         return true;
      }


      volatile bool _cancelSearch = false; // volatile because of multiple thread access
      private bool cancelSearch 
      {
         get 
         {
            return _cancelSearch;
         }
         set 
         {
            _cancelSearch = value;
         }
      }

      System.Threading.Thread searchThread = null;

      // utility: sets UI to "stopped" state
      void SetStopped( string statusMessage ) 
      {
         this.cancelSearch = false;
         this.buttonStop.Enabled = false;
         this.statusBarPanel.AutoSize = StatusBarPanelAutoSize.Contents;
         this.statusBarPanel.Text = statusMessage;
         this.progressBarSearch.Visible = false;
      }

      void SetStopped() 
      {
         SetStopped("Stopped.");
      }

      // utility: checks for stop request on search thread, sets UI
      bool CheckStopRequested() 
      {
         if(this.cancelSearch) 
         {
            SetStopped();
            return true;
         }

         return false;
      }

      // utility: checks for max results on search thread
      bool CheckMaxResults()
      {
         if(this.listViewResults.Items.Count > maxPlaceResults) 
         {  // too many matches
            SetStopped(String.Format("More Than {0} matches...please revise search", maxPlaceResults));
            return true; // max results reached
         }

         return false;
      }


      // signal the search thread that it should stop, and wait for it to do so
      void StopSearchThread() 
      {
         this.cancelSearch = true; // request stop
         while(this.searchThread != null && this.searchThread.IsAlive) 
         {
            Application.DoEvents();
         }
      }

      // perform a full search in a placename set, with attributes
      bool PlaceNameSetFullSearch(string [] searchTokens, IndexedTiledPlaceNameSet curIndexedTiledSet) 
      {
         DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(
            Path.Combine(
			 Path.GetDirectoryName(Application.ExecutablePath), 
			 curIndexedTiledSet.placenameSet.PlacenameListFilePath.Value)));

         // ignore this set if the corresponding directory does not exist for some reason
         if(!dir.Exists) return true;

         // loop over all WWP files in directory
         foreach(FileInfo placenameFile in dir.GetFiles("*.wwp"))
         {
            using(BinaryReader reader = new BinaryReader(placenameFile.OpenRead()) ) 
            {
               int placenameCount = reader.ReadInt32();

               // loop over all places
               for(int i = 0; i < placenameCount; i++) 
               {
                  // return false if stop requested
                  if(CheckStopRequested()) return false;

                  // instantiate and read current placename
                  WorldWindPlacename pn = new WorldWindPlacename();
                  WplIndex.ReadPlaceName(reader, ref pn, WplIndex.MetaDataAction.Store);

                  // if we have a match ...
                  if(isPlaceMatched(searchTokens, pn)) 
                  {
                     if(CheckMaxResults()) return false;

                     PlaceItem pi = new PlaceItem();
                     pi.pn = pn;
                     pi.placeDescriptor = curIndexedTiledSet.placenameSet;

                     // add item via delegate to avoid MT issues
                     listViewResults.Invoke(listViewResults.addPlaceDelegate, new object[] { pi });
                  }
               }
            }
         }
         return true; // go on 
      }

      // adds a place (given by index) found via binary search to the list, and adds places above and below it
      // in the sort order as long as they match
      bool AddIndexAndVicinity(int nPos, IndexedTiledPlaceNameSet itps, string strSearch, bool bPartialAllowed) 
      { 
         PlaceItem pi = itps.wplIndex.GetPlaceItem(nPos); // get the place from the index

         // add item via delegate to avoid MT issues
         listViewResults.Invoke(listViewResults.addPlaceDelegate, new object[] { pi });

         // instantiate our special comparer - will compare search string to a place given by index - this is the same one
         // we're using in the binary search
         WplIndex.IndexEntryToStringComparer cmp = new WplIndex.IndexEntryToStringComparer(itps.wplIndex, bPartialAllowed);
         int nBelow = nPos-1; // left neighbour in search order (if any)
         int nAbove = nPos+1; // right neighbour in search order (if any)

         bool bFoundOne; // keeps track if "expansion mechanism" still found something
         do 
         {
            // return false if stop requested or too many results
            if(CheckStopRequested() || CheckMaxResults()) 
            {
               return false;
            }

            bFoundOne = false;
            if(itps.wplIndex.IsValidIndex(nBelow) && cmp.Compare(nBelow, strSearch) == 0) 
            {  // left neighbour index valid and matched 
               pi = itps.wplIndex.GetPlaceItem(nBelow); // get the data

               // add item via delegate to avoid MT issues
               listViewResults.Invoke(listViewResults.addPlaceDelegate, new object[] { pi });

               --nBelow; // expand to the left
               bFoundOne = true;
            }

            if(itps.wplIndex.IsValidIndex(nAbove) && cmp.Compare(nAbove, strSearch) == 0) 
            { // right neighbour index valid and matched 
               pi = itps.wplIndex.GetPlaceItem(nAbove); // get the data

               // add item via delegate to avoid MT issues
               listViewResults.Invoke(listViewResults.addPlaceDelegate, new object[] { pi });

               ++nAbove; // expand to the left
               bFoundOne = true;
            }
         } while(bFoundOne); // keep expanding until nothing new on both sides

         return true; // keep searching
      }

      // this is the "fast find" or quick variant of the search. Does not search attributes and needs exact
      // names, but much faster due to indexed binary search
      bool PlaceNameSetQuickSearch(string searchString, IndexedTiledPlaceNameSet itps, bool bPartial) 
      {
         try 
         {
            // return false if stop requested
            if(CheckStopRequested()) 
            {
               return false;
            }

            itps.wplIndex.Lock(null); // load and lock index

            // find using binary search
            int nRet = itps.wplIndex.FindPlaceByName(searchString, bPartial);
            if(nRet < 0) 
            {
               return true; // didn't find anything, return indicating "go on"
            }

            // found one - add it and its matching neighbours.
            bool ret;
            ret = AddIndexAndVicinity(nRet, itps, searchString, bPartial);

            return ret;
         }
         finally 
         {
            // don't forget to release the index
            itps.wplIndex.Release();
         }
      }
         
      // the search thread proper
      void searchFunc()
      {
         System.DateTime startTime = System.DateTime.Now; // how long did we take

         bool bFastSearch = this.checkBoxFastSearch.Checked; // fast search wanted?
         bool bPartial = false; // partial means the start of the name is enough to match - relevant for FastSearch only

         string searchString;
         string [] searchTokens = null;

         if(bFastSearch) 
         {  // normalize search string
            searchString = textBoxSearchKeywords.Text.Trim().ToLower();
            if(searchString.EndsWith("*")) // partial search is currently signaled by appending a "*"
            {
               searchString = searchString.Substring(0, searchString.Length-1); // remove *
               bPartial = true; // set partial flag

               // TODO: removed maxlen check on pattern as we now check for too many matches, but remember to notify user about this
            }
         }
         else 
         {  // split string into tokens
            searchString = this.textBoxSearchKeywords.Text.Replace(","," ").Replace("  "," ");
            searchTokens = searchString.Split(' ');
         }
				
         // loop over place name sets
         foreach(IndexedTiledPlaceNameSet curIndexedTiledSet in this.indexedTiledPlacenameSets)
         {
            if(bFastSearch) 
            {
               // quicksearch will add entries. If it returns false, abort the loop (cancel)
               if(!PlaceNameSetQuickSearch(searchString, curIndexedTiledSet, bPartial)) 
               {
                  break;
               }
            }
            else 
            {
               // fullsearch will add entries. If it returns false, abort the loop (cancel)
               if(!PlaceNameSetFullSearch(searchTokens, curIndexedTiledSet)) break;
            }
            this.progressBarSearch.Increment(1);
         }

         TimeSpan searchTime = DateTime.Now - startTime;
         
         int nFound = this.listViewResults.Items.Count;

         string searchInfo;
         if(nFound > maxPlaceResults) 
         {
            searchInfo = String.Format("More than {0} matches, please revise search", maxPlaceResults);
         }
         else 
         {
            searchInfo = String.Format("{0} place{1} found ({2} secs)", nFound > 0 ? nFound.ToString() : "No", nFound > 1 ? "s" : "", 
               searchTime.TotalSeconds.ToString("##0.##"));         
         }
         SetStopped(searchInfo);
      }
      
      // start search
      void buttonSearch_Click(object sender, System.EventArgs e)
      {
         // don't search if search string is empty or whitespace only
         if(this.textBoxSearchKeywords.Text.Trim().Length == 0)
         {
            return;
         }

         // stop thread if we're currently searching
         if(this.searchThread != null && this.searchThread.IsAlive)
         {
            StopSearchThread();
         }

         this.tabControlLists.SelectedTab = this.tabPageResults;
         this.listViewResults.Items.Clear();

         // turn off sorting - simply append search results
         // to end of list unless user requests otherwise
         this.listViewResults.Sorting = SortOrder.None;
         this.listViewResults.ListViewItemSorter = null;

         this.buttonStop.Enabled = true;
         this.progressBarSearch.Visible = true;

         this.statusBarPanel.Text = "Searching...";
         this.progressBarSearch.Minimum = 0;
         this.progressBarSearch.Maximum = this.indexedTiledPlacenameSets.Count;
         this.progressBarSearch.Value = 0;

         // init search thread ...
         this.searchThread = new System.Threading.Thread(new System.Threading.ThreadStart(this.searchFunc));
         this.searchThread.IsBackground = true;
         this.searchThread.Start(); // and GO!
      }

      void buttonStop_Click(object sender, System.EventArgs e)
      {
         StopSearchThread();
      }

      // Search windows don't die, they simply hide from view :-)
      protected override void OnClosing(CancelEventArgs e)
      {
         e.Cancel = true;
         this.Visible = false;
         worldWindow.Focus();
         base.OnClosing(e);
      }

      // Someone clicked on a search result line
      void placelistView_SelectedIndexChanged(object sender, System.EventArgs e)
      {
         WWPlaceListView plw = (WWPlaceListView)sender;
         if(plw.SelectedItems.Count  == 0 ||           // ignore if no entries in list ..
            plw.SelectedItems[0].Tag == null) return;  // ... or if selected entry has no attached placename info

         PlaceItem pi = (PlaceItem)plw.SelectedItems[0].Tag; // retrieve info

         // set lat, lon and altitude
         this.numericUpDownLatitude.Value = (decimal)pi.pn.Lat;
         this.numericUpDownLongitude.Value = (decimal)pi.pn.Lon;
         this.numericUpDownAltitude.Value = (decimal)(pi.Altitude/1000.0);

         // and go there.
         this.worldWindow.GotoLatLonViewRange(pi.pn.Lat, pi.pn.Lon, 90.0f);
      }

      // handle a few keystrokes on our own
      protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs e) 
      {
         switch(e.KeyCode) 
         {
            case Keys.F6:      
            case Keys.Escape:  
               Close();
               break;
            case Keys.F4:
               if(e.Modifiers==Keys.Control)
                  Close();
               break;
         }

         base.OnKeyDown(e);
      }

      protected override void OnVisibleChanged(EventArgs e)
      {
         textBoxSearchKeywords.Focus();     // focus on the textbox and
         textBoxSearchKeywords.SelectAll(); // select all to replace text when user types

         base.OnVisibleChanged (e);
      }

      // Select all text when tabbed into
      void textBoxSearchKeywords_Enter(object sender, System.EventArgs e)
      {
         ((TextBox)sender).SelectAll();
      }

      void buttonGo_Click(object sender, System.EventArgs e)
      {
         StopSearchThread();
         this.worldWindow.GotoLatLonAltitude((float)this.numericUpDownLatitude.Value, (float)this.numericUpDownLongitude.Value, (float)this.numericUpDownAltitude.Value*1000.0f);
      }

      /// <summary>
      /// Enter key in Longitude/latitude/altitude = "Go"
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void PositionUpDowns_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e) 
      {
         if(e.KeyCode == System.Windows.Forms.Keys.Enter)
         {
            this.buttonGo_Click(this.buttonGo, null);
            e.Handled = true;
         }
      }

      // the panel hosting the search progress bar is owner drawn - this is handled here
      void statusBar_DrawItem(object sender, System.Windows.Forms.StatusBarDrawItemEventArgs sbdevent) 
      {
         if(sbdevent.Panel == this.progressPanel) 
         { // is it us?
            this.progressBarSearch.Bounds = sbdevent.Bounds; // yep - simply adapt size and location
         }
      }

      private void IndexCreationProgressDelegate(double percentComplete, string currentAction)
      {
         this.statusBarPanel.Text = currentAction;
         this.progressBarSearch.Value = (int)percentComplete;
      }


      private void checkBoxFastSearch_CheckedChanged(object sender, System.EventArgs e)
      {
         // if transition from checked to unchecked or all indices are available, that's OK
         if(!checkBoxFastSearch.Checked || this.AllIndicesAvailable()) return;

         if(MessageBox.Show("Fast search requires precomputed index files that were not found.\n\n" + 
            "PlaceFinder can compute these now, but this requires large amounts of \n" + 
            "memory and World Wind may temporarily appear 'frozen'.\n\n" +
            "Before you proceed, save your work and close other applications\n" + 
            "to free more memory. Once the index files are created, you won't see\n" + 
            "this message again.", 
            "Compute missing indices?", 
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2) == DialogResult.Yes) 
         {
            this.progressBarSearch.Minimum = 0;
            this.progressBarSearch.Maximum = 100;
            string savedTitle = this.Text;
            
            for(int i=0; i < indexedTiledPlacenameSets.Count; i++) 
            {
               this.Text = String.Format(savedTitle + " : Computing index {0} of {1}", i, indexedTiledPlacenameSets.Count);
               IndexedTiledPlaceNameSet tpns = (IndexedTiledPlaceNameSet)indexedTiledPlacenameSets[i];
               if(!tpns.wplIndex.IsAvailable) 
               {
                  tpns.wplIndex.CreateIndex(new WplIndex.ProgressReportDelegate(IndexCreationProgressDelegate));
                  GC.Collect();
               }
            }
            SetStopped("Done.");
            this.Text = savedTitle;
         }
         else 
         {
            checkBoxFastSearch.Checked = false;
         }
      }

      FavoritesSettings favsettings = null;
      HistorySettings histsettings = null;

      private void GotoDialog_Load(object sender, System.EventArgs e)
      {
         // Load settings into Favorites:
         this.favsettings = new FavoritesSettings();
         this.favsettings = (FavoritesSettings)FavoritesSettings.Load(this.favsettings);

         this.listViewFavorites.FillListFromSettings(this.favsettings);
         this.favsettings.places = null; // free space used by places hack

         // Load settings into History:
         this.histsettings = new HistorySettings();
         this.histsettings = (HistorySettings)HistorySettings.Load(this.histsettings);

         this.listViewHistory.FillListFromSettings(this.histsettings);
         this.histsettings.places = null; // free space used by places hack
      }

      private void GotoDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
      {
         // Save settings from Favorites:
         this.listViewFavorites.FillSettingsFromList(this.favsettings);

         this.favsettings.Save();
         this.favsettings.places = null; // free space used by places hack

         // Save settings from History:
         this.listViewHistory.FillSettingsFromList(this.histsettings);

         this.histsettings.Save();
         this.histsettings.places = null; // free space used by places hack
      }
   }
}
