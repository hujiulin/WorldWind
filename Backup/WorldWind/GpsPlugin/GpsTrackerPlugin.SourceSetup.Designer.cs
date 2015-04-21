namespace GpsTrackerPlugin
{
    partial class GpsSetup
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
            this.checkBoxTrackLine = new System.Windows.Forms.CheckBox();
            this.checkBoxInformationText = new System.Windows.Forms.CheckBox();
            this.checkBoxTrackHeading = new System.Windows.Forms.CheckBox();
            this.checkBoxDistanceFromPOI = new System.Windows.Forms.CheckBox();
            this.checkBoxDistanceToPOI = new System.Windows.Forms.CheckBox();
            this.labelTitle = new System.Windows.Forms.Label();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.SuspendLayout();
            // 
            // checkBoxTrackLine
            // 
            this.checkBoxTrackLine.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBoxTrackLine.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxTrackLine.Location = new System.Drawing.Point(12, 74);
            this.checkBoxTrackLine.Name = "checkBoxTrackLine";
            this.checkBoxTrackLine.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.checkBoxTrackLine.Size = new System.Drawing.Size(128, 16);
            this.checkBoxTrackLine.TabIndex = 12;
            this.checkBoxTrackLine.Text = "Track line";
            this.checkBoxTrackLine.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // checkBoxInformationText
            // 
            this.checkBoxInformationText.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBoxInformationText.Checked = true;
            this.checkBoxInformationText.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxInformationText.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxInformationText.Location = new System.Drawing.Point(12, 29);
            this.checkBoxInformationText.Name = "checkBoxInformationText";
            this.checkBoxInformationText.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.checkBoxInformationText.Size = new System.Drawing.Size(128, 16);
            this.checkBoxInformationText.TabIndex = 11;
            this.checkBoxInformationText.Text = "Information Text";
            this.checkBoxInformationText.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // checkBoxTrackHeading
            // 
            this.checkBoxTrackHeading.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBoxTrackHeading.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxTrackHeading.Location = new System.Drawing.Point(12, 51);
            this.checkBoxTrackHeading.Name = "checkBoxTrackHeading";
            this.checkBoxTrackHeading.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.checkBoxTrackHeading.Size = new System.Drawing.Size(104, 17);
            this.checkBoxTrackHeading.TabIndex = 10;
            this.checkBoxTrackHeading.Text = "Track Heading";
            this.checkBoxTrackHeading.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // checkBoxDistanceFromPOI
            // 
            this.checkBoxDistanceFromPOI.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBoxDistanceFromPOI.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxDistanceFromPOI.Location = new System.Drawing.Point(12, 119);
            this.checkBoxDistanceFromPOI.Name = "checkBoxDistanceFromPOI";
            this.checkBoxDistanceFromPOI.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.checkBoxDistanceFromPOI.Size = new System.Drawing.Size(328, 16);
            this.checkBoxDistanceFromPOI.TabIndex = 15;
            this.checkBoxDistanceFromPOI.Text = "Distance & bearing to ... from all Gps";
            this.checkBoxDistanceFromPOI.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // checkBoxDistanceToPOI
            // 
            this.checkBoxDistanceToPOI.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBoxDistanceToPOI.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxDistanceToPOI.Location = new System.Drawing.Point(12, 96);
            this.checkBoxDistanceToPOI.Name = "checkBoxDistanceToPOI";
            this.checkBoxDistanceToPOI.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.checkBoxDistanceToPOI.Size = new System.Drawing.Size(351, 17);
            this.checkBoxDistanceToPOI.TabIndex = 13;
            this.checkBoxDistanceToPOI.Text = "Distance from ... to all POIs";
            this.checkBoxDistanceToPOI.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTitle.Location = new System.Drawing.Point(9, 9);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(106, 13);
            this.labelTitle.TabIndex = 16;
            this.labelTitle.Text = "Set options for ...";
            // 
            // buttonAccept
            // 
            this.buttonAccept.Location = new System.Drawing.Point(220, 155);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(72, 23);
            this.buttonAccept.TabIndex = 17;
            this.buttonAccept.Text = "Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.buttonAccept_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Location = new System.Drawing.Point(-26, 141);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(475, 8);
            this.groupBox1.TabIndex = 18;
            this.groupBox1.TabStop = false;
            // 
            // GpsSetup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(304, 184);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonAccept);
            this.Controls.Add(this.labelTitle);
            this.Controls.Add(this.checkBoxDistanceFromPOI);
            this.Controls.Add(this.checkBoxDistanceToPOI);
            this.Controls.Add(this.checkBoxTrackLine);
            this.Controls.Add(this.checkBoxInformationText);
            this.Controls.Add(this.checkBoxTrackHeading);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GpsSetup";
            this.ShowInTaskbar = false;
            this.Text = "Gps Source Options...";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.GpsSetup_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxTrackLine;
        private System.Windows.Forms.CheckBox checkBoxInformationText;
        private System.Windows.Forms.CheckBox checkBoxTrackHeading;
        private System.Windows.Forms.CheckBox checkBoxDistanceFromPOI;
        private System.Windows.Forms.CheckBox checkBoxDistanceToPOI;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}