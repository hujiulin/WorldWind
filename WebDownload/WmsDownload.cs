using System;

namespace WorldWind.Net
{
	public class WMSDownload
	{
		private string _title = "";
		private string _date = "";
		private string _name = "";
		private decimal _north = 90;
		private decimal _south = -90;
		private decimal _west = -180;
		private decimal _east = 180;
		private string _version = "";

		private string _url;
		private string _savedFilePath ="";

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Net.WMSDownload"/> class.
		/// </summary>
		/// <param name="url"></param>
		public WMSDownload( string url )
		{
			this._url = url;
		}

		#region Public properties
		public string Title
		{
			get
			{
				return _title;
			}
			set
			{
				_title = value;
			}
		}

		public string Date
		{
			get
			{
				return _date;
			}
			set
			{
				_date = value;
			}
		}

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
			}
		}

		public decimal North
		{
			get
			{
				return _north;
			}
			set
			{
				_north = value;
			}
		}

		public decimal South
		{
			get
			{
				return _south;
			}
			set
			{
				_south = value;
			}
		}

		public decimal West
		{
			get
			{
				return _west;
			}
			set
			{
				_west = value;
			}
		}

		public decimal East
		{
			get
			{
				return _east;
			}
			set
			{
				_east = value;
			}
		}

		public string Version
		{
			get
			{
				return _version;
			}
			set
			{
				_version = value;
			}
		}
		
		public string SavedFilePath
		{
			get
			{
				return _savedFilePath;
			}
			set
			{
				_savedFilePath = value;
			}
		}

		public string Url
		{
			get
			{
				return _url;
			}
			set
			{
				_url = value;
			}
		}

		#endregion

		public override string ToString()
		{
			return this._title + " - " + this._date;
		}
	}
}
