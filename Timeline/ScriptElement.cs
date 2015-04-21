using System;
using System.Xml;

namespace Timeline
{
	internal abstract class ScriptElement
	{
		protected abstract void doLoadElementChild(XmlNode node);
		protected abstract void doLoadElementAttribute(XmlNode node);

		static protected String getElementId(XmlNode node)
		{
			String id = getAttribute(node, "id");
			if (id != null && !id.Equals(String.Empty))
				return id;
			else
				return null;
		}

		static protected String getElementIdref(XmlNode node)
		{
			String id = getAttribute(node, "idref");
			if (id != null && !id.Equals(String.Empty))
				return id;
			else
				return null;
		}

		static protected bool hasElementId(XmlNode node)
		{
			return getElementId(node) != null;
		}

		static protected bool isReference(XmlNode node)
		{
			return getElementIdref(node) != null;
		}

		static protected String getAttribute(XmlNode node, String attrName)
		{
			String attrValue = null;
			if (attrName != null && node.Attributes.Count > 0)
			{
				XmlNamedNodeMap attrs = node.Attributes;
				XmlNode attrNode = attrs.GetNamedItem(attrName);
				if (attrNode != null)
				{
					attrValue = attrNode.Value;
				}
			}
			return attrValue;
		}

		static protected String getElementValue(XmlNode node)
		{
			return node.InnerText;
		}

		protected void loadElement(XmlNode node)
		{
			foreach (XmlNode attrNode in node.Attributes)
			{
				this.doLoadElementAttribute(attrNode);
			}

			foreach (XmlNode elementNode in node.ChildNodes)
			{
				this.doLoadElementChild(elementNode);
			}
		}

		// Constant arguments for object instancing below.
		private static String ns = typeof(ScriptElement).Namespace + ".";
		private static Type[] argTypes = new Type[] {typeof(XmlNode)};

		protected Object instanceEntryForElement(XmlNode node)
		{
			try
			{
				Type t = Type.GetType(ns + node.LocalName);
				if (t == null)
					return null;

				System.Reflection.ConstructorInfo ctor = t.GetConstructor(argTypes);
				if (ctor == null)
					return null;

				Object[] argValues = new Object[] {node};
				return ctor.Invoke(argValues);
			}
			catch (Exception e)
			{
				throw new TimelineException("Error constructing script element.", e);
			}
		}
	}
}