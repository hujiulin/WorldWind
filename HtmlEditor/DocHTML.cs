using System;

namespace onlyconnect
{
    
    public class DocHTML
    {
        HTMLDocument mDoc;
        IHTMLDocument2 mDoc2;
        IHTMLDocument3 mDoc3;
        IHTMLDocument4 mDoc4;
        IHTMLDocument5 mDoc5;

        public DocHTML(HTMLDocument doc)
        {
            mDoc = doc;
            mDoc2 = (IHTMLDocument2)mDoc;
            mDoc3 = (IHTMLDocument3)mDoc;
            mDoc4 = (IHTMLDocument4)mDoc;
            mDoc5 = (IHTMLDocument5)mDoc;
        }

        //IHTMLDocument2

        public bool ExecCommand(String cmdID)
        {
            return mDoc2.ExecCommand(cmdID, true, null);
        }

        public IHTMLWindow2 ParentWindow()
        {
            return mDoc2.GetParentWindow();
        }

        public String ReadyState
        {
            get
            {
                return mDoc2.GetReadyState();
            }
        }

        //IHTMLDocument3
        public IHTMLElement GetElementByID(string idval)
        {
            return mDoc3.getElementById(idval);
        }

        public IHTMLElementCollection GetElementsByTagName(String tagname)
        {
            return mDoc3.getElementsByTagName(tagname);
        }


     }
}
