using System;
using System.Globalization;
using System.Xml;
using WorldWind.Net.Wms;

namespace Timeline
{
	internal sealed class WmsImage : TimelineElement
	{
		private WmsDescriptor descriptor = new WmsDescriptor(null, 100.0);

		public WmsImage(XmlNode node)
		{
			this.loadElement(node);
		}

		public WmsDescriptor Descriptor {get {return this.descriptor;}}

		protected override void doLoadElementAttribute(XmlNode node)
		{
		}

		protected override void doLoadElementChild(XmlNode child)
		{
			if (child.NodeType != XmlNodeType.Element)
				return;

			String nodeName = child.LocalName;
			if (nodeName.Equals("Url"))
			{
				this.descriptor.Url = new System.Uri(child.InnerText);
			}
			else if (nodeName.Equals("Opacity"))
			{
				double value = Double.Parse(child.InnerText, CultureInfo.InvariantCulture);
				this.descriptor.Opacity = value;
			}
		}			
	}
}