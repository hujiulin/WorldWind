using System;
using System.Globalization;
using System.Xml;
using WorldWind;

namespace Timeline
{
	internal sealed class Layer : ScriptElement
	{
		private LayerDescriptor descriptor =
			new LayerDescriptor(null, null, 100.0);

		public Layer(XmlNode node)
		{
			this.loadElement(node);
		}

		public LayerDescriptor Descriptor {get {return this.descriptor;}}

		protected override void doLoadElementAttribute(XmlNode node)
		{
		}

		protected override void doLoadElementChild(XmlNode child)
		{
			if (child.NodeType != XmlNodeType.Element)
				return;

			String nodeName = child.LocalName;
			if (nodeName.Equals("Category"))
			{
				this.descriptor.Category = child.InnerText;
			}
			else if (nodeName.Equals("Name"))
			{
				this.descriptor.Name = child.InnerText;
			}
			else if (nodeName.Equals("Opacity"))
			{
				double value = Double.Parse(child.InnerText, CultureInfo.InvariantCulture);
				this.descriptor.Opacity = value;
			}
		}		
	}	
}