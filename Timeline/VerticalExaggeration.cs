using System;
using System.Globalization;
using System.Xml;

namespace Timeline
{
	internal sealed class VerticalExaggeration : TimelineElement
	{
		private double exaggeration;

		public VerticalExaggeration(XmlNode node)
		{
			this.loadElement(node);
		}

		public double Exaggeration
		{
			get {return this.exaggeration;}
		}

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

			this.exaggeration = Double.Parse(valueString, CultureInfo.InvariantCulture);
		}	
	}
}