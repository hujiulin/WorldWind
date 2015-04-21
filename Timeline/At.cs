using System;
using System.Globalization;
using System.Xml;

namespace Timeline
{
	internal sealed class At : ScriptElement
	{
		private double time;
		private System.Collections.Hashtable paramEntries =
			new System.Collections.Hashtable(3);

		public At(XmlNode node)
		{
			this.loadElement(node);
		}

		public double Time
		{
			get {return this.time;}
		}

		public System.Collections.Hashtable Parameters
		{
			get {return this.paramEntries;}
		}

		protected override void doLoadElementChild(XmlNode child)
		{
			if (child.NodeType != XmlNodeType.Element)
				return;

			String idref = getElementIdref(child);
			while (idref != null)
			{
				XmlNode referencedChild = child.OwnerDocument.GetElementById(idref);
				if (referencedChild != null)
					child = referencedChild;
				idref = getElementId(child);
			}

			Object o = this.instanceEntryForElement(child);
			if (o != null)
			{
				if (o is TimelineElement)
				{
					TimelineElement te = o as TimelineElement;
					te.Time = this.Time;
				}
				this.paramEntries.Add(child.LocalName, o);
			}
		}

		protected override void doLoadElementAttribute(XmlNode attr)
		{
			String attrName = attr.LocalName;
			String value = attr.Value;

			if (value != null)
			{
				this.time = Double.Parse(value, CultureInfo.InvariantCulture);
			}
		}
	}
}