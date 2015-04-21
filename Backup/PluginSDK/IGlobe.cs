using System;
using WorldWind.Net.Wms;

namespace WorldWind
{
	public interface IGlobe
	{
		void SetDisplayMessages(System.Collections.IList messages);
		void SetLatLonGridShow(bool show);
		void SetLayers(System.Collections.IList layers);
		void SetVerticalExaggeration(double exageration);
		void SetViewDirection(String type, double horiz, double vert, double elev);
		void SetViewPosition(double degreesLatitude, double degreesLongitude,
			double metersElevation);
		void SetWmsImage(WmsDescriptor imageA, WmsDescriptor imageB, double alpha);
	}

	public sealed class OnScreenMessage
	{
		private String message;
		private double x;
		private double y;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.OnScreenMessage"/> class.
		/// </summary>
		public OnScreenMessage() {}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.OnScreenMessage"/> class.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="message"></param>
		public OnScreenMessage(double x, double y, String message)
		{
			this.x = x;
			this.y = y;
			this.message = message;
		}

		public String Message
		{
			get {return this.message;}
			set {this.message = value;}
		}
		
		public double X
		{
			get {return this.x;}
			set {this.x = value;}
		}
		
		public double Y
		{
			get {return this.y;}
			set {this.y = value;}
		}

	}

	public sealed class LayerDescriptor
	{
		private String category;
		private String name;
		private double opacity;

		public LayerDescriptor() {}

		public LayerDescriptor(String category, String name, double opacity)
		{
			this.category = category;
			this.name = name;
			this.opacity = opacity;
		}

		public String Category
		{
			get {return this.category;}
			set {this.category = value;}
		}

		public String Name
		{
			get {return this.name;}
			set {this.name = value;}
		}

		public double Opacity
		{
			get {return this.opacity;}
			set {this.opacity = value;}
		}
	}
}

namespace WorldWind.Net.Wms
{
	public sealed class WmsDescriptor
	{
		private System.Uri url;
		private double opacity;

		public WmsDescriptor() {}

		public WmsDescriptor(System.Uri url, double opacity)
		{
			this.url = url;
			this.opacity = opacity;
		}

		public System.Uri Url
		{
			get {return this.url;}
			set {this.url = value;}
		}

		public double Opacity
		{
			get {return this.opacity;}
			set {this.opacity = value;}
		}
	}
}