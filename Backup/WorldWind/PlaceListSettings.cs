using System;
using System.Xml.Serialization;
using WorldWind.Configuration;

namespace WorldWind
{
	/// <summary>
	/// Implements persistent XML storage of place data
	/// (Used as a base class to save favorites and history)
	/// </summary>
	public class PlaceListSettings : SettingsBase
	{
      /// <summary>
      /// This class represents one metadatum name/value pair
      /// </summary>
      public class MetaDataEntry 
      {
         /// <summary>
         /// Meta-datum name
         /// </summary>
         [XmlAttribute]
         public string name;
         /// <summary>
         /// Meta-datum value
         /// </summary>
         [XmlAttribute]
         public string value;
      }

      /// <summary>
      /// This class stores fixed place attributes
      /// </summary>
      public class PlaceData 
      {
         /// <summary>
         /// The name of the place
         /// </summary>
         [XmlAttribute]
         public string Name;

         /// <summary>
         /// Latitude in decimal degrees
         /// </summary>
         [XmlAttribute]
         public float Lat;

         /// <summary>
         /// Longitude in decimal degrees
         /// </summary>
         [XmlAttribute]
         public float Lon;

         /// <summary>
         /// A place can have an arbitrary number of associated metadata
         /// </summary>
         public MetaDataEntry [] metadata;
      }

      /// <summary>
      /// All the places in one array
      /// </summary>
      public PlaceData [] places;

      public PlaceListSettings()
		{
		}
	}

   /// <summary>
   /// This class is used for persistent XML storage of the users favorites
   /// </summary>
   public class FavoritesSettings : PlaceListSettings 
   {
      /// <summary>
      /// Override ToString to use "Favorites.xml" as default filename
      /// </summary>
      /// <returns></returns>
      public override string ToString() { return "Favorites"; }
   }

   /// <summary>
   /// This class is used for persistent XML storage of the users place visit history
   /// </summary>
   public class HistorySettings : PlaceListSettings 
   {
      /// <summary>
      /// Override ToString to use "History.xml" as default filename
      /// </summary>
      /// <returns></returns>
      public override string ToString() { return "History"; }
   }
}
