using System;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;
using Utility;

namespace WorldWind.Configuration
{
	public class SettingsBase
	{
		private string m_fileName; // location where settings will be stored / were loaded from
		// Filename property, do not serialize
		[XmlIgnore]
		[Browsable(false)]
		public string FileName 
		{
			get { return m_fileName; }
			set { m_fileName = value; }
		}

		private string m_formatVersion; // Version of application that created file
		[Browsable(false)]
		public string FormatVersion 
		{
			get { return m_formatVersion; }
			set { m_formatVersion = value; }
		}

		// types of location supported
		public enum LocationType 
		{
			User = 0,       // regular, roaming user - settings will move
			UserLocal,      // local user - settings will be stored on local machine
			UserCommon,     // location common to all users
			Application,    // application - settings will be saved in appdir
		}


		// get the default location, given type of location
		public static string DefaultLocation(LocationType locationType)
		{
			string directory;

			switch(locationType) 
			{
				case LocationType.UserLocal:
					// Example: @"C:\Documents and Settings\<user>\Local Settings\Application Data\NASA\NASA World Wind\1.3.3.11250"
					return Application.LocalUserAppDataPath;
				
				case LocationType.UserCommon:
					// Example: @"C:\Documents and Settings\All Users\Application Data\NASA\NASA World Wind\1.3.3.11250"
					return Application.CommonAppDataPath;
				
				case LocationType.Application:
					// Example: @"C:\Program Files\NASA\World Wind\"
					return Application.StartupPath;

				default:
					// fall through to regular (roaming) user
				case LocationType.User:   
					// Example: @"C:\Documents and Settings\<user>\Application Data\NASA\World Wind\1.3.3"
					directory = Log.DefaultSettingsDirectory();
					Directory.CreateDirectory(directory);
					return directory;
			}
		}

		// Return the default filename (without path) to be used when saving
		// this class's data(e.g. via serialization).
		// Always add the ".xml" file extension.
		// If ToString is not overridden, the default filename will be the
		// class name.
		public string DefaultName()
		{
			return String.Format("{0}.xml", this.ToString());
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Configuration.SettingsBase"/> class.
		/// A default constructor is required for serialization.
		/// </summary>
		public SettingsBase()
		{
			// experimental: store app version
			m_formatVersion = Application.ProductVersion;
		}


		// Save settings to XML file given specifically by name
		// Note: the FileName property will stay unchanged
		public virtual void Save(string fileName) 
		{
			XmlSerializer ser = null;

			try 
			{
				ser = new XmlSerializer(this.GetType());
				using(TextWriter tw = new StreamWriter(fileName)) 
				{
					ser.Serialize(tw, this);
				}
			}
			catch(Exception ex) 
			{
				throw new System.Exception(String.Format("Saving settings class '{0}' to {1} failed", this.GetType().ToString(), fileName), ex);
			}
		}

		// Save to default name
		public virtual void Save()
		{
			try
			{
				Save(m_fileName);
			}
			catch(Exception caught)
			{
				Log.Write(caught);
			}
		}

		// load settings from a given file (full path and name)
		public static SettingsBase Load(SettingsBase defaultSettings, string fileName) 
		{
			// remember where we loaded from for a later save
			defaultSettings.m_fileName = fileName;

			// return the default instance if the file does not exist
			if(!File.Exists(fileName)) 
			{
				return defaultSettings;
			}

			// start out with the default instance
			SettingsBase settings = defaultSettings;
			try 
			{
				XmlSerializer ser = new XmlSerializer(defaultSettings.GetType());

				using(TextReader tr = new StreamReader(fileName)) 
				{
					settings = (SettingsBase)ser.Deserialize(tr);
					settings.m_fileName = fileName; // remember where we loaded from for a later save
				}
			}
			catch(Exception ex) 
			{
				throw new System.Exception(String.Format("Loading settings from file '{1}' to {0} failed", 
					defaultSettings.GetType().ToString(), fileName), ex);
			}
         
			return settings;
		}

		// Load settings from specified location using specified path and default filename
		public static SettingsBase LoadFromPath(SettingsBase defaultSettings, string path)
		{
			string fileName = Path.Combine(path, defaultSettings.DefaultName());
			return Load(defaultSettings, fileName);
		}


		// Load settings from specified location using specified name
		public static SettingsBase Load(SettingsBase defaultSettings, LocationType locationType, string name)
		{
			string fileName = Path.Combine(DefaultLocation(locationType), name);
			return Load(defaultSettings, fileName);
		}

		// load settings from specified location using default name
		public static SettingsBase Load(SettingsBase defaultSettings, LocationType locationType)
		{
			return Load(defaultSettings, locationType, defaultSettings.DefaultName());
		}

		// load settings from default file
		public static SettingsBase Load(SettingsBase defaultSettings) 
		{
			return Load(defaultSettings, LocationType.User);
		}

		public string SettingsFilePath
		{
			get
			{
				return m_fileName;
			}
		}	
	}
}

