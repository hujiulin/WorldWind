using System;
using System.Globalization;
using System.Xml;

namespace Timeline
{
	internal sealed class ViewDirection : TimelineElement
	{
		private const String ANGLES = "angles";
		private const String POSITION = "position";

		private String specForm;
		private double latitude;
		private double longitude;
		private double elevation;
		private double horizontalAngle;
		private double verticalAngle;

		public ViewDirection(XmlNode node)
		{
			this.loadElement(node);
		}

		public String SpecForm {get {return this.specForm;}}
		public double Latitude {get {return this.latitude;}}
		public double Longitude {get {return this.longitude;}}
		public double Elevation {get {return this.elevation;}}
		public double HorizontalAngle {get {return this.horizontalAngle;}}
		public double VerticalAngle {get {return this.verticalAngle;}}

		protected override void doLoadElementAttribute(XmlNode node)
		{
		}

		protected override void doLoadElementChild(XmlNode child)
		{
			if (child.NodeType != XmlNodeType.Element)
				return;

			String valueString = child.InnerText;

			String nodeName = child.LocalName;
			if (nodeName.Equals("Angles"))
			{
				this.loadFromAngles(child);
			}
			else if (nodeName.Equals("Position"))
			{
				this.loadFromAngles(child);
			}
			else if (nodeName.Equals("HorizontalAngle") && valueString != null)
			{
				this.horizontalAngle = Double.Parse(valueString, CultureInfo.InvariantCulture);
			}
			else if (nodeName.Equals("VerticalAngle") && valueString != null)
			{
				this.verticalAngle = Double.Parse(valueString, CultureInfo.InvariantCulture);
			}
			else if (nodeName.Equals("Latitude") && valueString != null)
			{
				this.latitude = Double.Parse(valueString, CultureInfo.InvariantCulture);
			}
			else if (nodeName.Equals("Longitude") && valueString != null)
			{
				this.longitude = Double.Parse(valueString, CultureInfo.InvariantCulture);
			}
			else if (nodeName.Equals("Elevation") && valueString != null)
			{
				this.elevation = Double.Parse(valueString, CultureInfo.InvariantCulture);
			}
		}
	
		private void loadFromAngles(XmlNode child)
		{
			this.specForm = ANGLES;

			foreach (XmlNode elementNode in child.ChildNodes)
			{
				this.doLoadElementChild(elementNode);
			}
		}

		private void loadFromPosition(XmlNode child)
		{
			this.specForm = POSITION;

			foreach (XmlNode elementNode in child.ChildNodes)
			{
				this.doLoadElementChild(elementNode);
			}
		}
	}
}