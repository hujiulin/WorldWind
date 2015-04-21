using System;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;

namespace Timeline
{
	internal sealed class Script : ScriptElement
	{
		private int repeatCount = 0;
		private String name = "Unnamed Script";
		private String shortDescription = null;
		private System.Collections.ArrayList ats = new System.Collections.ArrayList();
		private XmlDocument doc;

		public Script(System.IO.FileInfo script)
		{
			this.createDocument(script);
			this.loadElements();
		}

		public String Name
		{
			get {return this.name;}
		}

		public int RepeatCount
		{
			get {return this.repeatCount;}
		}

		public String ShortDescription
		{
			get {return this.shortDescription;}
		}

		public System.Collections.IList AtElements
		{
			get {return this.ats;}
		}

		private void createDocument(System.IO.FileInfo script)
		{
            XmlReaderSettings readerSettings = new XmlReaderSettings();

			XmlSchemaSet schemas = new XmlSchemaSet();
			String schemaPath = System.Windows.Forms.Application.StartupPath;
			schemas.Add(null, schemaPath + "/WorldWindAutomation_v1.xsd");
			readerSettings.Schemas = schemas;
            readerSettings.ValidationType = ValidationType.Schema;
            readerSettings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings;
            try
			{
                XmlReader docReader = XmlReader.Create(script.FullName, readerSettings);
                this.doc = new XmlDocument();
				doc.Load(docReader);
			}
			catch (XmlSchemaException e)
			{
				String message = "Script file is invalid: \n" + e.Message;
				throw new TimelineException(message, e);
			}
		}

		private void loadElements()
		{
			XmlNode docElement = this.doc.DocumentElement;
			String rootName = docElement.LocalName;
			if (!rootName.Equals("Script"))
			{
				throw new TimelineException("Root element of script is not a Script node.");
			}
			this.loadElement(docElement);
		}

		protected override void doLoadElementAttribute(XmlNode attr)
		{
			String attrName = attr.LocalName;
			String attrValue = attr.Value;

			if (attrValue != null && attrName.Equals("repeat"))
			{
				this.repeatCount = Int32.Parse(attrValue, CultureInfo.InvariantCulture);
			}
			else if (attrValue != null && attrName.Equals("name"))
			{
				this.name = attrValue;
			}
		}

		protected override void doLoadElementChild(XmlNode child)
		{
			if (child.NodeType == XmlNodeType.Element)
			{
				String nodeName = child.LocalName;
				if (nodeName.Equals("At"))
				{
					At at = new At(child);
					this.ats.Add(at);
				}
				else if (nodeName.Equals("ShortDescription"))
				{
					this.shortDescription = child.InnerText;
				}
			}
		}
	}
}