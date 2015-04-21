using System;
using System.Globalization;
using System.Xml;

namespace Timeline
{
	internal sealed class ViewPosition : TimelineElement
	{
		private double latitude = 0;
		private double longitude = 0;
		private double elevation = 0;

		public ViewPosition(XmlNode node)
		{
			this.loadElement(node);
		}

		public double Latitude { get {return this.latitude;}}
		public double Longitude { get {return this.longitude;}}
		public double Elevation { get {return this.elevation;}}

		protected override void doLoadElementAttribute(XmlNode node)
		{
		}

		protected override void doLoadElementChild(XmlNode child)
		{
			if (child.NodeType != XmlNodeType.Element)
				return;

			String valueString = child.InnerText;
			if (valueString == null)
				return;

			double value = Double.Parse(valueString, CultureInfo.InvariantCulture);

			String nodeName = child.LocalName;
			if (nodeName.Equals("Latitude"))
			{
				this.latitude = value;
			}
			else if (nodeName.Equals("Longitude"))
			{
				this.longitude = value;
			}
			else if (nodeName.Equals("Elevation"))
			{
				this.elevation = value;
			}
		}
	}
}