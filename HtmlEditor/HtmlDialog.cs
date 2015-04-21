using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using onlyconnect;

namespace onlyconnect
{

    public partial class HtmlDialog : Form
    {
        public Boolean isVisible = false;

        public HtmlDialog()
        {
            InitializeComponent();
            isVisible = true;
        }

        public void SetHTML(String Html){
            if (Html.Contains("<font>") == false)
            {
                Html = "<font face='Arial' size='1'>" + Html + "</font>";
            }

            htmlEditor1.LoadDocument(Html);
        }

        private void exitPicture_Click(object sender, EventArgs e)
        {
            this.FindForm().Visible = false;
            isVisible = false;
        }

        private void exitPicture_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

    }
}