using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WorldWind
{
    public class KMLDialog : Form
    {
        private System.Windows.Forms.Label closeButton;
        private WebBrowser myBrowser;
        public bool isVisible = false;
        public bool HTMLIsSet = false;
        private String HTML;

        public KMLDialog()
        {
            InitializeComponent();           
            isVisible = true;
        }

        void myBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            String URL = e.Url.ToString();

            if (URL.Contains("about:blank") == false)
            {
                MainApplication nt = (MainApplication)this.FindForm().Owner;
                nt.BrowseTo(URL);

                e.Cancel = true;
            }
        }

        public void SetHTML(String Html){

            HTMLIsSet = true;

            if (Html.IndexOf("<font>") == -1)
            {
                Html = "<font face='Arial' size='1'>" + Html + "</font>";
            }

            myBrowser.Navigate("about:blank");
            
            HTML = Html;

            // create an IHTMLDocument2
            HtmlDocument htmlDoc = myBrowser.Document;
             
            // write to the doc
            htmlDoc.Write(Html);
            
         }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.FindForm().Visible = false;
            isVisible = false;
            this.Dispose();
        }

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.closeButton = new System.Windows.Forms.Label();
            this.myBrowser = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // closeButton
            // 
            this.closeButton.AutoSize = true;
            this.closeButton.BackColor = System.Drawing.Color.Transparent;
            this.closeButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.closeButton.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.closeButton.ForeColor = System.Drawing.Color.DodgerBlue;
            this.closeButton.Location = new System.Drawing.Point(252, 11);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(19, 18);
            this.closeButton.TabIndex = 0;
            this.closeButton.Text = "X";
            this.closeButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // myBrowser
            // 
            this.myBrowser.AllowNavigation = false;
            this.myBrowser.Location = new System.Drawing.Point(15, 12);
            this.myBrowser.Name = "myBrowser";
            this.myBrowser.ScriptErrorsSuppressed = true;
            this.myBrowser.Size = new System.Drawing.Size(273, 158);
            this.myBrowser.TabIndex = 1;
            this.myBrowser.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.myBrowser_Navigating);
            // 
            // KMLDialog
            // 
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImage = global::WorldWind.Properties.Resources.outline;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(309, 182);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.myBrowser);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "KMLDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.TransparencyKey = System.Drawing.Color.Lime;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

    }
}