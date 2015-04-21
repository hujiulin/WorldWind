using System;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using WorldWind;
using WorldWind.Renderable;

// TODO: coding style adaptation (from MFC to C# naming convention) needed in this file
namespace WorldWind 
{
   /// <summary>
   /// ListView-derived class that supports dynamically adding columns and sorting.
   /// </summary>
   public class WWListView : ListView 
   {
      int m_nSortCol = 0;
      // this hashtable will match attribute names to column header indices
      Hashtable m_htColumnIndices = new Hashtable();
      ColumnDescriptor [] m_colDesc;

      /// <summary>
      /// Describes a WWListView column
      /// </summary>
      public class ColumnDescriptor 
      {
         /// <summary>
         /// Name of the column as the user will see it
         /// </summary>
         public string m_columnName;

         /// <summary>
         /// Attribute name, used as key
         /// </summary>
         public string m_attribName;

         /// <summary>
         /// Column width
         /// </summary>
         public int m_width;

         /// <summary>
         /// Indicates a numeric column that has to be sorted accordingly
         /// </summary>
         public bool m_isNumeric;

         /// <summary>
         /// Construct a column descriptor
         /// </summary>
         /// <param name="colName">Name of the column</param>
         /// <param name="attribName">Attribute name, used as key</param>
         /// <param name="width">Column width</param>
         /// <param name="isNumeric">If true, column will be sorted numerically</param>
         public ColumnDescriptor(string colName, string attribName, int width, bool isNumeric) 
         {
            m_columnName = colName;
            m_attribName = attribName;
            m_width = width;
            m_isNumeric = isNumeric;
         }
      }

      // used to compare items when sorting
      class WWListViewItemComparer: IComparer 
      {
         WWListView m_wwList; // the list we're currently sorting

         // Construct a comparer
         public WWListViewItemComparer(WWListView wwlv) 
         {
            m_wwList = wwlv; // keep track of the list we're sorting
         }

         #region IComparer Members

         // compare two elements in a given column
         public int Compare(object x, object y) 
         {
            int nCol = m_wwList.m_nSortCol; // get the column to sort
            // get elements to sort
            ListViewItem lvix = (ListViewItem)x;
            ListViewItem lviy = (ListViewItem)y;

            int nCompareResult; // result of numeric or string comparison
            if(m_wwList.m_colDesc[nCol].m_isNumeric) // comparing number?
            {
               // yep - convert item strings to doubles, substituting zero for empty strings
               double xval = lvix.SubItems[nCol].Text == string.Empty ? 0 : Double.Parse(lvix.SubItems[nCol].Text);
               double yval = lviy.SubItems[nCol].Text == string.Empty ? 0 : Double.Parse(lviy.SubItems[nCol].Text);
               
               nCompareResult = Math.Sign(xval - yval); // the result is the sign of the subtraction
            }
            else // we have a string column to sort
            {
               nCompareResult = String.Compare(lvix.SubItems[nCol].Text, lviy.SubItems[nCol].Text);
            }
               
            // return result - if sorting in descending order, negate comparison result
            return this.m_wwList.Sorting == SortOrder.Descending ? -nCompareResult : nCompareResult;
         }
         #endregion
      }

      /// <summary>
      /// Handles column click for sorting
      /// </summary>
      /// <param name="e">Information about the column clicked</param>
      protected override void OnColumnClick(ColumnClickEventArgs e) 
      {
         base.OnColumnClick(e); // call base class handler

         if(this.ListViewItemSorter == null) 
         {
            // defers sorting until first column is actually clicked
            this.ListViewItemSorter = new WWListViewItemComparer(this);
         }
         if(this.m_nSortCol == e.Column) // same column clicked again?
         {  // yes - toggle sorting order ...
            this.Sorting = (this.Sorting == SortOrder.Descending) ? SortOrder.Ascending : SortOrder.Descending;
         }  
         else 
         {  // no, change sorted column ...
            this.m_nSortCol = e.Column;
            this.Sorting = SortOrder.Ascending;
         }
         this.Sort(); // ... and sort
      }

      /// <summary>
      /// Initializes a new instance of the <see cref= "T:WorldWind.WWListView"/> class.
      /// </summary>
      public WWListView() : base() 
      {
         this.View = System.Windows.Forms.View.Details;
      }

      /// <summary>
      /// Initializes a new instance of the WWListView class, with predefined columns
      /// </summary>
      /// <param name="columnDescriptors">Array of column descriptors</param>
      public WWListView(ColumnDescriptor [] columnDescriptors) : this() 
      {
         this.m_colDesc = columnDescriptors;

         for(int i=0; i < columnDescriptors.Length; i++) 
         {
            ColumnHeader ch = new ColumnHeader();
            ch.Text = columnDescriptors[i].m_columnName;
            ch.Width = columnDescriptors[i].m_width;
            this.Columns.Add(ch);
            this.m_htColumnIndices.Add(columnDescriptors[i].m_attribName, i);
         }
      }

      /// <summary>
      /// Add a a List view item via a hashtable with attribute names and values
      /// </summary>
      /// <param name="knv">A Hashtable with column attribute names and values</param>
      /// <returns></returns>
      public ListViewItem AddKeysAndValues(Hashtable knv) 
      {
         string [] strSubItems = new string [this.m_htColumnIndices.Count];

         foreach(DictionaryEntry AttribAndValue in knv) 
         {
            // find Column title in collection
            object oCol = this.m_htColumnIndices[AttribAndValue.Key];
            if(oCol != null) 
            {  
               // if column exists, add value
               strSubItems[(int)oCol] = (string)AttribAndValue.Value;
            }
         }

         ListViewItem lvi = new ListViewItem(strSubItems);
         this.Items.Add(lvi);
         return lvi;
      }
   }


   /// <summary>
   /// WWListView derived class that implements a listview for places.
   /// </summary>
   public class WWPlaceListView : WWListView 
   {
      /// <summary>
      /// Delegate type to add a place, to prevent multithreading problems
      /// </summary>
      public delegate void AddPlaceDelegate(PlaceItem pi);

      /// <summary>
      /// Delegate to add a place, to prevent multithreading problems
      /// </summary>
      public AddPlaceDelegate addPlaceDelegate;

      /// <summary>
      /// WWPlaceMenuItem extends MenuItem class with information about required context
      /// </summary>
      public class WWPlaceMenuItem : MenuItem 
      {  
         /// <summary>
         /// Describes what this menu item requires:
         /// always valid even on empty list, items must be present, 
         /// single selection must be present, selection must be present
         /// </summary>
         public enum RequiredElement
         {
            None,             // menu item is always valid
            Items,            // menu item requires a non-empty listbox
            SingleSelection,  // menu item does not work with multi-selection
            Selection,        // menu item can handle any selection
         }

         private RequiredElement m_requiredElement = RequiredElement.None;
         /// <summary>
         /// Gets/sets the menu item requirements
         /// </summary>
         public RequiredElement Requires
         {
            get 
            {
               return m_requiredElement;
            }
            set 
            {
               m_requiredElement = value;
            }
         }

         /// <summary>
         /// Creates a new WWPlaceMenuItem given a name, handle, and requirements
         /// </summary>
         /// <param name="name">The name of the menu item</param>
         /// <param name="handler">The event handler that gets called when the item is clicked</param>
         /// <param name="requires">Elements that need to be present in the list for item to be enabled</param>
         public WWPlaceMenuItem(string name, EventHandler handler, RequiredElement requires)
            : base(name, handler)
         {
            Requires = requires;
         }

      }


      /// <summary>
      /// Returns an array of the place items that are selected in the list
      /// </summary>
      public PlaceItem [] SelectedPlaces 
      {
         get 
         {
            ArrayList al = new ArrayList();
            foreach(ListViewItem lvi in this.SelectedItems) 
            {
               al.Add(lvi.Tag);
            }
            return (PlaceItem [])al.ToArray(typeof(PlaceItem));
         }
      }

      /// <summary>
      /// A doubleclick goes to the corresponding place and
      /// also adds it to the history list
      /// </summary>
      /// <param name="e"></param>
      protected override void OnDoubleClick(EventArgs e) 
      {
         base.OnDoubleClick (e);
         ListViewItem lvi = this.FocusedItem;
         if(lvi == null) return;

         PlaceItem pi = (PlaceItem)lvi.Tag;
         if(this.WorldWindow != null) pi.Goto(this.m_worldWindow);
         if(this.RecentFinds != null) this.RecentFinds.AddPlace(pi);
      }

      // Go to a specific item in the list. The first element of the
      // selection (if any) is used.
      private void placeListMenu_Goto(object sender, System.EventArgs e) 
      {
         PlaceItem [] selplaces = this.SelectedPlaces;
         if(selplaces.Length == 0) return;
         
         if(this.m_worldWindow != null) selplaces[0].Goto(this.m_worldWindow);
      }

      // Copy the first selected item to the clipboard as a WorldWind URL
      private void placeListMenu_CopyURL(object sender, System.EventArgs e) 
      {
         PlaceItem [] selplaces = this.SelectedPlaces;
         if(selplaces.Length == 0) return;
         Clipboard.SetDataObject(selplaces[0].GotoURL(this.m_worldWindow), true);
      }

      // delete all selected places from list
      private void placeListMenu_Del(object sender, System.EventArgs e) 
      {
         foreach(ListViewItem li in this.SelectedItems) 
         {
            this.Items.Remove(li);
         }
      }

      // clear the entire list
      private void placeListMenu_Clear(object sender, System.EventArgs e) 
      {
         this.Items.Clear();
      }

      // add selected places to favorites
      private void placeListMenu_AddToFavs(object sender, System.EventArgs e) 
      {
         if(this.Favorites == null) return;

         PlaceItem [] selplaces = this.SelectedPlaces;
         foreach(PlaceItem pi in selplaces) 
         {
            this.Favorites.AddPlace(pi);
         }
      }

      // Show file selection dialog and load places from specified XML file in GPX format
      private void placeListMenu_GpxLoad(object sender, System.EventArgs e) 
      { 
         OpenFileDialog ofd = new OpenFileDialog();

         ofd.Filter = "gpx files (*.gpx)|*.gpx|All files (*.*)|*.*" ;
         ofd.FilterIndex = 1;
         ofd.RestoreDirectory = true;
         ofd.Title = "Import from GPX file";
         ofd.CheckFileExists = true;

         if(ofd.ShowDialog() == DialogResult.OK) 
         {
            this.LoadFromGpx(ofd.FileName);
         }
      }

         
      // Show file save dialog and save places to specified XML file in GPX format
      private void placeListMenu_GpxSave(object sender, System.EventArgs e) 
      { 
         SaveFileDialog sfd = new SaveFileDialog();

         sfd.Filter = "gpx files (*.gpx)|*.gpx|All files (*.*)|*.*" ;
         sfd.FilterIndex = 1;
         sfd.RestoreDirectory = true;
         sfd.Title = "Export to GPX file";
         sfd.OverwritePrompt = true;

         if(sfd.ShowDialog() == DialogResult.OK) 
         {
            this.SaveToGpx(sfd.FileName);
         }
      }

      /// <summary>
      /// Creates a new WWPlaceListView object, initializing place-specific columns and menus
      /// </summary>
      public WWPlaceListView() : base (
         new WWListView.ColumnDescriptor [] {
                                               new WWListView.ColumnDescriptor("Name", "Name", 70, false),
                                               new WWListView.ColumnDescriptor("State", "State", 100, false),
                                               new WWListView.ColumnDescriptor("County", "County", 100, false),
                                               new WWListView.ColumnDescriptor("Country", "Country", 130, false),
                                               new WWListView.ColumnDescriptor("Layer", "Layer", 170, false),
                                               new WWListView.ColumnDescriptor("Type", "Feature Type", 50, false),
                                               new WWListView.ColumnDescriptor("Elevation", "Elevation", 40, true),
                                               new WWListView.ColumnDescriptor("Population", "Population", 50, true),
                                               new WWListView.ColumnDescriptor("Latitude", "Latitude", 60, true),
                                               new WWListView.ColumnDescriptor("Longitude", "Longitude", 60, true),
         // Attributes present in WWP: feature type, country, state, county, state code, elevation, population
      }
         ) 
      {

         ContextMenu placeListMenu = new ContextMenu(
            new MenuItem [] {
                               new WWPlaceMenuItem("&Goto", new EventHandler(placeListMenu_Goto), WWPlaceMenuItem.RequiredElement.SingleSelection),
                               new WWPlaceMenuItem("&Copy URL", new EventHandler(placeListMenu_CopyURL), WWPlaceMenuItem.RequiredElement.SingleSelection),
                               new WWPlaceMenuItem("&Remove selected", new EventHandler(placeListMenu_Del), WWPlaceMenuItem.RequiredElement.Selection),
                               new WWPlaceMenuItem("Remove &all", new EventHandler(placeListMenu_Clear), WWPlaceMenuItem.RequiredElement.Items),
                               new MenuItem("-"),
                               new WWPlaceMenuItem("&Import from GPX", new EventHandler(placeListMenu_GpxLoad), WWPlaceMenuItem.RequiredElement.None),
                               new WWPlaceMenuItem("&Export to GPX", new EventHandler(placeListMenu_GpxSave), WWPlaceMenuItem.RequiredElement.Items),
                               new MenuItem("-"),
         });

         placeListMenu.Popup += new EventHandler(ContextMenu_Popup);
         this.ContextMenu = placeListMenu;

         this.addPlaceDelegate = new AddPlaceDelegate(AddPlace);
      }

      // Popup handler - enable / disable WWPlaceMenuItems according to RequiredElements
      private void ContextMenu_Popup(object sender, EventArgs e)
      {
         ContextMenu mnu = (ContextMenu)sender;
         foreach(MenuItem mi in mnu.MenuItems) 
         {
            WWPlaceMenuItem wwpmi = mi as WWPlaceMenuItem;
            if(wwpmi == null) continue;

            switch(wwpmi.Requires) 
            {
               case WWPlaceMenuItem.RequiredElement.None: 
                  wwpmi.Enabled = true; 
                  break;

               case WWPlaceMenuItem.RequiredElement.Items:
                  wwpmi.Enabled = this.Items.Count > 0;
                  break;

               case WWPlaceMenuItem.RequiredElement.Selection:
                  wwpmi.Enabled = this.SelectedItems.Count > 0;
                  break;

               case WWPlaceMenuItem.RequiredElement.SingleSelection:
                  wwpmi.Enabled = this.SelectedItems.Count == 1;
                  break;

            }
         }
      }

      private WWPlaceListView m_lvwFavorites = null;
      /// <summary>
      /// Gets/sets the WWPlaceListView displaying the favorites
      /// </summary>
      public WWPlaceListView Favorites 
      {
         get 
         {
            return m_lvwFavorites;
         }
         set 
         {
            if(m_lvwFavorites == null && value != null) 
            {
               this.ContextMenu.MenuItems.Add(new WWPlaceMenuItem("&Add to Favorites", new EventHandler(placeListMenu_AddToFavs), WWPlaceMenuItem.RequiredElement.Selection));
            }
            m_lvwFavorites = value;
         }
      }

      private WWPlaceListView m_lvwRecentFinds = null;
      /// <summary>
      /// Gets/sets the WWPlaceListView displaying the history aka Recent Finds
      /// </summary>
      public WWPlaceListView RecentFinds 
      {
         get 
         {
            return m_lvwRecentFinds;
         }
         set 
         {
            m_lvwRecentFinds = value;
         }
      }

      private WorldWindow m_worldWindow = null;
      /// <summary>
      /// Gets/sets the WorldWindow object this PlaceList is associated with (e.g. for Goto functionality)
      /// </summary>
      public WorldWindow WorldWindow 
      {
         get 
         {
            return m_worldWindow;
         }
         set 
         {
            m_worldWindow = value;
         }
      }

      /// <summary>
      /// Adds a place given a PlaceItem
      /// </summary>
      /// <param name="pi">PlaceItem class describing the place</param>
      public void AddPlace(PlaceItem pi) 
      {
         Hashtable knv = new Hashtable(); // holds name/value pairs

         // add the "standard" attributes to the hashtable
         knv.Add("Name", pi.pn.Name); // pi.strPlaceName);
         knv.Add("Layer", pi.placeDescriptor == null ? "" : pi.placeDescriptor.Name.Value);
         knv.Add("Latitude", pi.pn.Lat.ToString()); // pi.lat.ToString());
         knv.Add("Longitude", pi.pn.Lon.ToString()); // pi.lon.ToString());
         
         // now add metadata attributes
         if(pi.pn.metaData != null) 
         {
            foreach(DictionaryEntry de in pi.pn.metaData) 
            {
               knv.Add(de.Key, de.Value);
            }
         }

         // add to the list via the hashtable
         ListViewItem lvi = this.AddKeysAndValues(knv);
         lvi.Tag = pi; // keep track of the placeitem
      }

      /// <summary>
      /// Loads places from a file in GPX format, given pathname
      /// </summary>
      /// <param name="strGpxPath">The file (with full path) in GPX format to load from</param>
      public void LoadFromGpx(string strGpxPath) 
      {
         if(!File.Exists(strGpxPath)) return;

         XmlSerializer ser = new XmlSerializer(typeof(gpxType));
         TextReader tr = new StreamReader(strGpxPath);
   
         gpxType gpx = null;
         try 
         {
            gpx = (gpxType)ser.Deserialize(tr);
         }
         catch(Exception caught) 
         {
            // TODO: log error
            System.Diagnostics.Debug.WriteLine(caught.InnerException.ToString());
         }

         tr.Close();

         if(gpx == null || gpx.wpt == null) return;

         foreach(wptType wpt in gpx.wpt) 
         {
            PlaceItem pi = new PlaceItem();
            pi.pn = new WorldWindPlacename();
            pi.pn.Name = wpt.name;
            pi.pn.Lat = (float)wpt.lat;
            pi.pn.Lon = (float)wpt.lon;

            AddPlace(pi);
         }
      }

      /// <summary>
      ///  Saves places to a file in GPX format, given pathname
      /// </summary>
      /// <param name="strGpxPath">The file (with full path) to save to</param>
      public void SaveToGpx(string strGpxPath) 
      {
         gpxType gpx = new gpxType();
         gpx.creator = "NASA World Wind";
         gpx.version = "1.1";
         
         gpx.wpt = new wptType [this.Items.Count];
         int i = 0;
         foreach(ListViewItem lvi in this.Items) 
         {
            PlaceItem pi = (PlaceItem)lvi.Tag;
            wptType wp = new wptType();
            wp.name = pi.pn.Name;
            wp.lat = (decimal)pi.pn.Lat;
            wp.lon = (decimal)pi.pn.Lon;
            wp.sym = "Waypoint";
            gpx.wpt[i++] = wp;
         }

         XmlSerializer ser = new XmlSerializer(typeof(gpxType));
         TextWriter tw = new StreamWriter(strGpxPath);
         ser.Serialize(tw, gpx);
         tw.Close();
      }

      /// <summary>
      /// Fills a PlaceListSettings class (used to serialize places) with all the places in the list
      /// </summary>
      /// <param name="pls">The PlaceListSettings class that will receive the places</param>
      public void FillSettingsFromList(PlaceListSettings pls) 
      {
         pls.places = new PlaceListSettings.PlaceData [this.Items.Count];
         for(int i = 0; i < this.Items.Count; i++)
         {
            PlaceItem pi = (PlaceItem)this.Items[i].Tag;
            PlaceListSettings.PlaceData pd = new PlaceListSettings.PlaceData();
            pd.Name = pi.pn.Name;
            pd.Lat = pi.pn.Lat;
            pd.Lon = pi.pn.Lon;

            int mdCount = pi.pn.metaData == null ? 0 : pi.pn.metaData.Count;

            pd.metadata = new PlaceListSettings.MetaDataEntry [mdCount];
            int j = 0;
            if(pi.pn.metaData != null) 
            {
               foreach(DictionaryEntry de in pi.pn.metaData)
               {
                  pd.metadata[j] = new PlaceListSettings.MetaDataEntry();
                  pd.metadata[j].name = de.Key.ToString();
                  pd.metadata[j].value = de.Value.ToString();
                  j++;
               }
            }
            pls.places[i] = pd;
         }
      }

      /// <summary>
      /// Fills the list with all the places in a PlaceListSettings class (used to serialize places)
      /// </summary>
      /// <param name="pls">The PlaceListSettings class that contains all the places</param>
      public void FillListFromSettings(PlaceListSettings pls) 
      {
         // clear the list
         this.Items.Clear();
         if(pls.places == null) return; // nothing to do

         foreach(PlaceListSettings.PlaceData pd in pls.places) 
         {
            PlaceItem pi = new PlaceItem();
            pi.pn = new WorldWindPlacename();
            pi.pn.Name = pd.Name;
            pi.pn.Lat = (float)pd.Lat;
            pi.pn.Lon = (float)pd.Lon;

            pi.pn.metaData = new Hashtable();
            for(int i=0; i < pd.metadata.Length; i++) 
            {
               pi.pn.metaData.Add(pd.metadata[i].name, pd.metadata[i].value);
            }
            AddPlace(pi);
         }
      }
   }
}
