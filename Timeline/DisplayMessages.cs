using System;
using System.Xml;

namespace Timeline
{
	internal sealed class DisplayMessages : TimelineElement
	{
		private System.Collections.ArrayList messages =
			new System.Collections.ArrayList();

		public DisplayMessages(XmlNode node)
		{
			this.loadElement(node);
		}

		public System.Collections.IList Messages {get {return this.messages;}}

		protected override void doLoadElementAttribute(XmlNode node)
		{
		}

		protected override void doLoadElementChild(XmlNode child)
		{
			if (child.NodeType != XmlNodeType.Element)
				return;

			String nodeName = child.LocalName;
			if (nodeName.Equals("DisplayMessage"))
			{
				DisplayMessage dm = new DisplayMessage(child);
				if (dm != null)
				{
					this.messages.Add(dm.Message);
				}
			}
		}		
	}
}