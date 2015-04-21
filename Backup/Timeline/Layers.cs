using System;
using System.Xml;

namespace Timeline
{
	internal sealed class Layers : TimelineElement
	{
		private System.Collections.ArrayList layers =
			new System.Collections.ArrayList();

		public Layers(XmlNode node)
		{
			this.loadElement(node);
		}

		public System.Collections.IList LayerList {get {return this.layers;}}

		protected override void doLoadElementAttribute(XmlNode node)
		{
		}

		protected override void doLoadElementChild(XmlNode child)
		{
			if (child.NodeType != XmlNodeType.Element)
				return;

			String nodeName = child.LocalName;
			if (nodeName.Equals("Layer"))
			{
				Layer layer = new Layer(child);
				if (layer != null)
				{
					this.layers.Add(layer.Descriptor);
				}
			}
		}		
	}
}