using System;
using System.IO;
using System.Collections;
using System.Globalization;
using WorldWind;
using WorldWind.Net;
using WorldWind.Renderable;

// TODO: coding style adaptation (from MFC to C# naming convention) needed in this file
namespace WorldWind 
{
   /// <summary>
   /// Utility structure representing an index entry
   /// </summary>
   public struct WplIndexEntry 
   {
      public System.Int16 fileNumber;
      public System.Int32 seekOffset;
   }

   /// <summary>
   /// utility class - combines TiledPlacenameSet with corresponding index
   /// </summary>
   public class IndexedTiledPlaceNameSet 
   {
      public LayerSet.Type_TiledPlacenameSet2 placenameSet;
      public WplIndex wplIndex;

		 /// <summary>
		 /// Initializes a new instance of the <see cref= "T:WorldWind.IndexedTiledPlaceNameSet"/> class. 
		 /// Initializes placenameset and index.
		 /// </summary>
		 /// <param name="pns"></param>
		 /// <param name="idx"></param>
      public IndexedTiledPlaceNameSet(LayerSet.Type_TiledPlacenameSet2 pns, WplIndex idx) 
      {
         placenameSet = pns;
         wplIndex = idx;
      }
   }

   /// <summary>
   /// Utility class PlaceItem associates a WorldWindPlacename with its TiledPlacenameset aka "Layer"
   /// </summary>
   public class PlaceItem 
   {
      /// <summary>
      /// Placename info from WorldWind 
      /// </summary>
      public WorldWindPlacename pn;

      /// <summary>
      /// Associated TiledPlaceNameSe
      /// </summary>
      public LayerSet.Type_TiledPlacenameSet2 placeDescriptor;

      /// <summary>
      /// Altitude accessor - either gets altitude from placeDescriptor
      /// or a default one (22500 currently)
      /// </summary>
      public double Altitude 
      {
         get 
         { 
            double altitude = 22500; // HACK: what to do if we haven't got a placeDescriptor
            if(placeDescriptor != null) 
            {
               altitude = placeDescriptor.MaximumDisplayAltitude.DoubleValue() * 0.9;
            }
            return altitude;
         }
      }

      /// <summary>
      /// Goes to this place item at its lat/lon and altitude
      /// </summary>
      /// <param name="ww"></param>
      public void Goto(WorldWindow ww) 
      {
         ww.GotoLatLonAltitude(pn.Lat, pn.Lon, this.Altitude);
      }

      /// <summary>
      /// Computes a WorldWind URL (as string)
      /// </summary>
      /// <param name="ww"></param>
      /// <returns></returns>
      public String GotoURL(WorldWindow ww) 
      {
         WorldWindUri uri = new WorldWindUri();
         uri.Latitude = Angle.FromDegrees(this.pn.Lat);
         uri.Longitude = Angle.FromDegrees(this.pn.Lon);

         return uri.ToString();
      }
   }

   /// <summary>
   /// This class is for index generation only - associates an index entry with a placename
   /// for later sorting
   /// </summary>
   public class IndexedPlace : PlaceItem
   {
      /// <summary>
      /// Index entry for this PlaceItem
      /// </summary>
      public WplIndexEntry indexEntry;
   }

   /// <summary>
   /// Utility class to compare place items for sorting
   /// </summary>
   public class PlaceItemComparer : IComparer 
   {
      #region IComparer Members

      /// <summary>
      /// Implements IComparer.Compare. Will compare the names of
      /// two PlaceItems
      /// </summary>
      /// <param name="x">First PlaceItem to compare</param>
      /// <param name="y">Second PlaceItem to compare</param>
      /// <returns>Result of PlaceItem name string comparison</returns>
      public int Compare(object x, object y) 
      {
         return String.Compare(((PlaceItem)x).pn.Name, ((PlaceItem)y).pn.Name, true);
      }

      #endregion
   }

   /// <summary>
   /// Represents an index for a .WPL file.
   /// </summary>
   public class WplIndex 
   {
      /// <summary>
      /// Utility class to compare index entries for binary search
      /// </summary>
      public class IndexEntryToStringComparer: IComparer 
      {
         WplIndex myWplIndex = null;
         bool partialAllowed = false;

         /// <summary>
         /// Creates a new IndexEntryToStringComparer
         /// </summary>
         /// <param name="theIndex">The index the comparer will work withz</param>
         /// <param name="bPartialAllowed">Set to true if partial matches are allowed.</param>
         public IndexEntryToStringComparer(WplIndex theIndex, bool bPartialAllowed) 
         {
            // remember the index we are working with
            myWplIndex = theIndex;

            // are partial matches allowed?
            partialAllowed = bPartialAllowed;
         }

         #region IComparer Members
         /// <summary>
         /// Implements IComparer.Compare. Will compare either a WplIndexEntry or its numeric index (int)
         /// to a string (the name being searched for.
         /// </summary>
         /// <param name="x">The WplIndexEntry itself or its offset as an int</param>
         /// <param name="y">The name bein searched for as string</param>
         /// <returns></returns>
         public int Compare(object x, object y) 
         {
            WplIndexEntry ie;

            // support comparing of index index or index entries directly
            if(x.GetType() == typeof(int)) 
            {
               // a numeric index was provided, retrieve corresponding entry
               ie = myWplIndex.m_indexEntries[(int)x];
            }
            else 
            {
               // we got an entry, just cast
               ie = (WplIndexEntry)x;
            }
            // Read place item, omitting metadata as we're interested in the name only
            PlaceItem pi = myWplIndex.GetPlaceItemFromIndexEntry(ie, MetaDataAction.Omit);
            if(!partialAllowed) 
            {
               return String.Compare(pi.pn.Name, (string)y, true);
            }
            string strSearch = (string)y;
            return String.Compare(pi.pn.Name, 0, strSearch, 0, strSearch.Length, true);
         }
         #endregion
      }

      LayerSet.Type_TiledPlacenameSet2 m_placeNameSet; // to remember placename set 
      string m_strWplPath;     // full path to the place list file for this index
      string m_strBasedir;     // just the base dir of the wpl and idx files and the wwp(s)
      string [] m_strWwpNames; // the names of the various place files (wwp), as per the wpl
      int m_nTotalPlaceNames = -1; // how many places in total, as per sum of the wwp. -1 means not initialized yet
      
      // define array of place informations that we'll load and sort to build the index
      IndexedPlace [] m_indexedPlaces = null; // this is only required when creating the index

      // an array of index entries - will be loaded from file and used to search
      WplIndexEntry [] m_indexEntries = null;
      WeakReference weakReferenceIndexEntries = null;

      // TODO: move these utility functions somewhere less specific
      // utility functions
      static double ProgressPercent(int nTaskNbr, int nTaskTotal, double dblFrom, double dblTo, double dblCur) 
      {
         double dblPercentStart = ((nTaskNbr-1)/(double)nTaskTotal)*100.0;
         double dblPercentEnd = ((nTaskNbr)/(double)nTaskTotal)*100.0;
         double dblCompletionRatio = (dblCur-dblFrom)/dblTo;
         return dblPercentStart+(dblPercentEnd-dblPercentStart)*dblCompletionRatio;
      }

      static BinaryReader OpenBinReader(string strFilePath) 
      {
         // open stream
         FileStream fsWwp = File.Open(strFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
         // return reader 
         return new BinaryReader(fsWwp, System.Text.Encoding.Default);
      }

      BinaryReader OpenWwpReader(int n) 
      {
         return OpenBinReader(Path.Combine(m_strBasedir, this.m_strWwpNames[n]));
      }


      // load place file names from a wpl - will fill m_strWwpNames
      void ReadWwpNamesFromWpl(string strWplFile) 
      {
			using( BinaryReader brWpl = OpenBinReader(strWplFile) )
			{
				int count = brWpl.ReadInt32(); // read number of files

				// now allocate array for wwp file names
				m_strWwpNames = new string [count];

				// read the corresponding amount
				for(int i = 0; i < count; i++) 
				{
					m_strWwpNames[i] = brWpl.ReadString();
					brWpl.ReadSingle(); // skip bounding box lat/lon (4 shorts)
					brWpl.ReadSingle();
					brWpl.ReadSingle();
					brWpl.ReadSingle();
				}	
			}
      }


      // determine how many total places are in this index -
      // that's the sum of all places in the individual wwp files
      // this is especially useful to allocate the placeitem array
      int ComputePlaceNameCount() 
      {
         int nTotalPlaceNames = 0; // init count

         // iterate over wwp names
         for(int i=0; i < m_strWwpNames.Length; i++) 
         {
            using( BinaryReader brWwp = OpenWwpReader(i) )
	            nTotalPlaceNames += brWwp.ReadInt32();
         }
         return nTotalPlaceNames; // result
      }


      // add the places from a single wwp file to our places array
      // note that nNextEntry is passed by reference - the total count is accumulated here
      void AddSingleWwpPlaces(int nFileNbr, ref int nNextEntry) 
      {
         // open reader
			using( BinaryReader brWwp = OpenWwpReader(nFileNbr) )
			{
				int nEntryCount = brWwp.ReadInt32(); // read number of entries

				for(int i=0; i < nEntryCount; i++) 
				{
					IndexedPlace ip = new IndexedPlace(); // allocate indexed place class to store entries
					ip.pn = new WorldWindPlacename();
					ip.placeDescriptor = this.m_placeNameSet; // "inherit" placeNameSet 
					ip.indexEntry.fileNumber = (System.Int16)nFileNbr; // remember file number
					ip.indexEntry.seekOffset = (int)brWwp.BaseStream.Position; // and location in file
					PlaceItem pi = ip;
					ReadPlaceName(brWwp, ref pi.pn, MetaDataAction.Skip); // skipping the metadata is faster
					this.m_indexedPlaces[nNextEntry++] = ip; // remember this indexed place
				}
			}
      }


      // load place infos for all wwp files belonging to this index / layer
      void LoadPlaceInfos(ProgressReportDelegate pr, int nTask, int nTotal) 
      {
         // count number of places in all wwp
         int nSize = this.PlaceCount;

         // allocate an array of matching size
         m_indexedPlaces = new IndexedPlace [nSize];

         int nEntryCount = 0; // keep track of current position
         int nWwpCount = this.m_strWwpNames.Length;
         for(int i=0; i < nWwpCount;  i++) 
         { // loop over Wwp
            // handle a single Wwp - will increment nEntryCount
            AddSingleWwpPlaces(i, ref nEntryCount);
            if(pr != null) pr(ProgressPercent(nTask, nTotal, 0, nWwpCount, i), "Loading place infos");
         }
      }


      // Sort Place Info structures
      void SortPlaceInfos(ProgressReportDelegate pr, int nTask, int nTotal) 
      {
         if(pr != null) pr(ProgressPercent(nTask, nTotal, 0, 1, 0), "Sorting");
         Array.Sort(m_indexedPlaces, new PlaceItemComparer());
         if(pr != null) pr(ProgressPercent(nTask, nTotal, 0, 1, 1), "Sorting");
      }


      // utility function: derive the full pathname of the index file from the stored Wpl path
      string IndexFileName() 
      {
         return Path.ChangeExtension(this.m_strWplPath, "idx");
      }

      // Save the index to file, assuming it is loaded and sorted
      void CreateIndexFromPlaceInfos(ProgressReportDelegate pr, int nTask, int nTotal) 
      {
         // initialize Writer
         using( FileStream fsIdx = File.Open(IndexFileName(), FileMode.Create, FileAccess.Write, FileShare.None) )
			using( BinaryWriter bwIdx = new BinaryWriter(fsIdx, System.Text.Encoding.Default) )
			{
				// write index entry count (total overall place infos)
				bwIdx.Write(this.m_indexedPlaces.Length);

				// iterate over PlaceInfos
				int nPlaceCount = m_indexedPlaces.Length;

				for(int i = 0; i < nPlaceCount; i++) 
				{
					IndexedPlace ip = m_indexedPlaces[i];
					bwIdx.Write(ip.indexEntry.fileNumber);
					bwIdx.Write(ip.indexEntry.seekOffset);
					if(i % 100 == 0) 
					{ // don't update progress too often as that slows down
						if(pr != null) pr(ProgressPercent(nTask, nTotal, 0, nPlaceCount, i), "Writing index");
					}
				}
			}
      }

      // free memory used by (hopefully now) sorted place infos
      void DiscardPlaceInfos()
      {
         this.m_indexedPlaces = null;
      }


      // given an index descriptor, seek to and return place information
      PlaceItem GetPlaceItemFromIndexEntry(WplIndexEntry ie, MetaDataAction metaDataAction) 
      {
			using( BinaryReader brWwp = OpenWwpReader(ie.fileNumber) )
			{
				// seek to the relevant info
				brWwp.BaseStream.Seek(ie.seekOffset, SeekOrigin.Begin);

				// create a new PlaceItem
				PlaceItem pi = new PlaceItem();
         
				pi.pn = new WorldWindPlacename();

				// set placeNameSet info
				pi.placeDescriptor = this.m_placeNameSet;

				// now read the rest from the WWP file
				ReadPlaceName(brWwp, ref pi.pn, metaDataAction);
				return pi;
			}
      }


      #region WplIndex Public Interface

      /// <summary>
      /// Delegate type to report progress. 
      /// percentComplete is the total progress in % (from 0 to 100),
      /// currentAction is what is currently being done, e.g. in a multi-phase operation.
      /// </summary>
      public delegate void ProgressReportDelegate(double percentComplete, string currentAction);

      /// <summary>
      /// Initializes a new instance of the <see cref= "T:WorldWind.WplIndex"/> class. 
      /// Gets the full path to the list file as parameter
      /// Note that the constructor will neiter load the index data, nor create it if
      /// not present - all it does is "get ready" by loading the list of WWP files
      /// </summary>
      /// <param name="tps">Placename set (layer) the index is associated with</param>
      /// <param name="strWplFilePath">The full list file path</param>
			public WplIndex(LayerSet.Type_TiledPlacenameSet2 tps, string strWplFilePath) 
      {
         this.m_placeNameSet = tps; // remember placename set (layer) we're associated with
         this.m_strWplPath = strWplFilePath; // remember the full list file path
         m_strBasedir = Path.GetDirectoryName(strWplFilePath); // compute/store basedir

         ReadWwpNamesFromWpl(strWplFilePath); // load the place file names from the wpl.

         // computing total placenames is deferred until actually needed
         // PlaceCount accessor will take care of this.
      }

      /// <summary>
      /// enum for ReadPlaceName: how to handle metadata
      /// </summary>
      public enum MetaDataAction { Store, Skip, Omit };

      /// <summary>
      /// utility routine: read a place info record from a BinaryReader
      /// </summary>
      /// <param name="br">Binary reader to read data from</param>
      /// <param name="pn">Where to write data</param>
      /// <param name="metaDataAction">What to do with metadata, read, skip, omit. The difference between skip and omit is that
      /// the latter is faster, while the former correctly positions to the next record if needed</param>
      static public void ReadPlaceName(BinaryReader br, ref WorldWindPlacename pn, MetaDataAction metaDataAction) 
      {
         pn.Name = br.ReadString(); // get place name
         pn.Lat = br.ReadSingle(); // and latitude
         pn.Lon = br.ReadSingle(); // and longitude

         int metaCount = br.ReadInt32(); // number of metadata (key/value pairs)

         if(metaDataAction == MetaDataAction.Store) 
         {
            pn.metaData = new Hashtable();
         }
         else 
         {
            pn.metaData = null;
         }
         if(metaDataAction == MetaDataAction.Omit) 
         {
            return;
         }

         for(int j = 0; j < metaCount; j++) 
         {
            string strKey = br.ReadString();
            string strValue = br.ReadString();
            // add the metadata pair if so requested
            if(metaDataAction == MetaDataAction.Store) pn.metaData.Add(strKey, strValue);
         }
      }


      /// <summary>
      /// Gets the number of places currently in this index
      /// </summary>
      public int PlaceCount 
      {
         get 
         {
            if(this.m_nTotalPlaceNames == -1) 
            {
               this.m_nTotalPlaceNames = ComputePlaceNameCount();
            }
            return this.m_nTotalPlaceNames;
         }
      }

      /// <summary>
      /// Checks if an int is a valid index
      /// </summary>
      /// <param name="n">The value to check for validity</param></param>
      /// <returns>True if this index is within the bounds</returns>
      public bool IsValidIndex(int n) {
         return n >= 0 && n < this.PlaceCount;
      }

      /// <summary>
      /// True if this index is available (the index file exists)
      /// </summary>
      public bool IsAvailable 
      {
         get 
         {
            return File.Exists(IndexFileName());
         }
      }
      
      /// <summary>
      /// True if this index is loaded if the index entries are there 
      /// </summary>
      public bool IsLoaded 
      {
         get 
         {
            return this.m_indexEntries != null;
         }
      }

      /// <summary>
      /// Creates an index from placeitems
      /// </summary>
      /// <param name="pr">Callback to provide progress report</param>
      public void CreateIndex(ProgressReportDelegate pr) 
      {
         LoadPlaceInfos(pr, 1, 3);
         SortPlaceInfos(pr, 2, 3);
         CreateIndexFromPlaceInfos(pr, 3, 3);
         DiscardPlaceInfos();
      }

      /// <summary>
      /// Loads an index from file (implicitly known via the name of the wpl file)
      /// </summary>
      /// <param name="pr">Callback to provide progress report</param>
      public void Load(ProgressReportDelegate pr) 
      {
         // initialize Reader
         using(BinaryReader brIdx = OpenBinReader(IndexFileName())) 
         {
            // read index entry count
            int nCount = brIdx.ReadInt32();

            // allocate an array of that size
            this.m_indexEntries = new WplIndexEntry [nCount];

            // read index entries
            for(int i = 0; i < nCount; i++) 
            {
               WplIndexEntry ie = new WplIndexEntry();

               ie.fileNumber = brIdx.ReadInt16(); // read file number
               ie.seekOffset = brIdx.ReadInt32(); // read seek offset
               m_indexEntries[i] = ie;
            }
         }
      }

      /// <summary>
      /// Locks the index: makes sure it is loaded and referenced
      /// </summary>
      /// <param name="pr">Callback to provide progress report</param>
      public void Lock(ProgressReportDelegate pr) 
      {
         if(this.IsLoaded) return; // seems fine and "locked" already

         // not loaded, try the weak reference
         if(this.weakReferenceIndexEntries == null) 
         {
            // that doesn't look good, weakref not there. Allocate it
            this.weakReferenceIndexEntries = new WeakReference(null);
         }

         // weakref still valid ?
         if(this.weakReferenceIndexEntries.IsAlive) 
         {  // yes, restore the array from the weakref
            m_indexEntries = (WplIndexEntry [])this.weakReferenceIndexEntries.Target;
         }
         else 
         {  // no - load again, and save as weakref as well
            Load(pr);
            this.weakReferenceIndexEntries = new WeakReference(m_indexEntries);
         }
      }

      /// <summary>
      /// Releases index entries to give GC a chance to free up memory
      /// </summary>
      public void Release() 
      {
         if(!this.IsLoaded) return; // nothing to do
         this.m_indexEntries = null;
      }

      /// <summary>
      /// Given an index position, seeks to and returns place information
      /// </summary>
      /// <param name="nIndex">Index of desired place</param>
      /// <returns></returns>
      public PlaceItem GetPlaceItem(int nIndex) 
      {
         return GetPlaceItemFromIndexEntry(this.m_indexEntries[nIndex], MetaDataAction.Store);
      }

      /// <summary>
      ///  Finds a place given its name.
      /// </summary>
      /// <param name="strPlaceName">The name to search for</param>
      /// <param name="bPartial">If true, will stop at first partial match</param>
      /// <returns></returns>
      public int FindPlaceByName(string strPlaceName, bool bPartial) 
      {
         return Array.BinarySearch(this.m_indexEntries, strPlaceName, 
            new WplIndex.IndexEntryToStringComparer(this, bPartial));
      }

      #endregion
   }
}
