using System;
using System.Globalization;
using System.Xml;
using WorldWind;

namespace Timeline
{
	internal sealed class DisplayMessage : ScriptElement
	{
		OnScreenMessage message = new OnScreenMessage(0, 0, String.Empty);

		public DisplayMessage(XmlNode node)
		{
			this.loadElement(node);
		}

		public OnScreenMessage Message 
		{
			get 
			{
				return this.message;
			}
		}

		protected override void doLoadElementAttribute(XmlNode node)
		{
		}

		protected override void doLoadElementChild(XmlNode child)
		{
			if (child.NodeType != XmlNodeType.Element)
				return;

			String nodeName = child.LocalName;
			if (nodeName.Equals("Message"))
			{
				String text = child.InnerText;
				if (text != null)
					this.message.Message = text;
			}
			else if (nodeName.Equals("X"))
			{
				double value = Double.Parse(child.InnerText, CultureInfo.InvariantCulture);
				this.message.X = value;
			}
			else if (nodeName.Equals("Y"))
			{
				double value = Double.Parse(child.InnerText, CultureInfo.InvariantCulture);
				this.message.Y = value;
			}
		}		
	}	
}