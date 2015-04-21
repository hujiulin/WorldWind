using System;
using System.Xml;

namespace Timeline
{
	internal class ToggleBase : TimelineElement
	{
		protected bool toggleValue;

		public ToggleBase(XmlNode node)
		{
			this.loadElement(node);
		}

		public bool ToggleValue
		{
			get {return this.toggleValue;}
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

			this.toggleValue = Boolean.Parse(valueString);
		}
	}
}