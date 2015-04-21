using System;

namespace onlyconnect
{
	//class to implement IHtmlEditHost
	/// <summary>
	/// enables you to customize the way elements are resized and moved
	/// </summary>
	internal class CSnap: IHTMLEditHost
	{

		public int SnapRect(IHTMLElement pIElement,
			RECT rect,
			ELEMENT_CORNER ehandle
			)
		{
			Console.WriteLine ("SnapRect called");
			int hr = HRESULT.S_OK;
			return hr;
		}

	}
}
