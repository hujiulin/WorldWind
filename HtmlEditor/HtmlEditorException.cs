using System;

namespace onlyconnect
{
	//Exception class for HtmlEditor
	public class HtmlEditorException : ApplicationException
	{
		// Default constructor
		public HtmlEditorException ()
		{
		}
   
		// Constructor accepting a single string message
		public HtmlEditorException (string message) : base(message)
		{
		}
   
		// Constructor accepting a string message and an 
		// inner exception which will be wrapped by this 
		// custom exception class
		public HtmlEditorException(string message, 
			Exception inner) : base(message, inner)
		{
		}
	}
}

