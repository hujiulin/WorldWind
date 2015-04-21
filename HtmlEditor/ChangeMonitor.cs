using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace onlyconnect
{

[System.Runtime.InteropServices.ComVisible(true)] 
public class ChangeMonitor : IHTMLChangeSink
{
    private HtmlEditor mHtmlEditor;

    public ChangeMonitor(HtmlEditor he) : base()
    {
        mHtmlEditor = he;
    }

    public void Notify()
    {
        mHtmlEditor.InvokeContentChanged();
    }
}
}

