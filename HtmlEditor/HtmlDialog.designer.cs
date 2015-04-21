namespace onlyconnect
{
    partial class HtmlDialog
    {
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
            this.exitPicture = new System.Windows.Forms.PictureBox();
            this.htmlEditor1 = new onlyconnect.HtmlEditor();
            ((System.ComponentModel.ISupportInitialize)(this.exitPicture)).BeginInit();
            this.SuspendLayout();
            // 
            // exitPicture
            // 
            this.exitPicture.BackColor = System.Drawing.Color.Transparent;
            this.exitPicture.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.exitPicture.Image = global::onlyconnect.Properties.Resources.close;
            this.exitPicture.Location = new System.Drawing.Point(251, 5);
            this.exitPicture.Name = "exitPicture";
            this.exitPicture.Size = new System.Drawing.Size(18, 21);
            this.exitPicture.TabIndex = 2;
            this.exitPicture.TabStop = false;
            this.exitPicture.Click += new System.EventHandler(this.exitPicture_Click);
            this.exitPicture.MouseEnter += new System.EventHandler(this.exitPicture_MouseEnter);
            // 
            // htmlEditor1
            // 
            this.htmlEditor1.BackColor = System.Drawing.Color.Black;
            this.htmlEditor1.DefaultComposeSettings.BackColor = System.Drawing.Color.White;
            this.htmlEditor1.DefaultComposeSettings.DefaultFont = new System.Drawing.Font("Arial", 10F);
            this.htmlEditor1.DefaultComposeSettings.Enabled = false;
            this.htmlEditor1.DefaultComposeSettings.ForeColor = System.Drawing.Color.Black;
            this.htmlEditor1.DefaultPreamble = onlyconnect.EncodingType.UTF8;
            this.htmlEditor1.DocumentEncoding = onlyconnect.EncodingType.WindowsCurrent;
            this.htmlEditor1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.htmlEditor1.IsUrlDetectionEnabled = true;
            this.htmlEditor1.Location = new System.Drawing.Point(12, 12);
            this.htmlEditor1.Name = "htmlEditor1";
            this.htmlEditor1.OpenLinksInNewWindow = true;
            this.htmlEditor1.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Left;
            this.htmlEditor1.SelectionBackColor = System.Drawing.Color.Empty;
            this.htmlEditor1.SelectionBullets = false;
            this.htmlEditor1.SelectionFont = null;
            this.htmlEditor1.SelectionForeColor = System.Drawing.Color.Empty;
            this.htmlEditor1.SelectionNumbering = false;
            this.htmlEditor1.Size = new System.Drawing.Size(255, 134);
            this.htmlEditor1.TabIndex = 1;
            this.htmlEditor1.Text = "htmlEditor1";
            // 
            // HtmlDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImage = global::onlyconnect.Properties.Resources.outline1;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(279, 158);
            this.ControlBox = false;
            this.Controls.Add(this.exitPicture);
            this.Controls.Add(this.htmlEditor1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "HtmlDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.TransparencyKey = System.Drawing.Color.Lime;
            ((System.ComponentModel.ISupportInitialize)(this.exitPicture)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private HtmlEditor htmlEditor1;
        private System.Windows.Forms.PictureBox exitPicture;

    }
}