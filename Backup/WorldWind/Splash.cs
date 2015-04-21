using System;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Resources;
using System.Windows.Forms;

namespace WorldWind
{
	/// <summary>
	/// The splash screen displayed while World Wind is loading.
	/// </summary>
	public class Splash : System.Windows.Forms.Form
	{
		private bool wasClicked;
		private bool hasError;
		private DateTime startTime = DateTime.Now;
		private TimeSpan timeOut = TimeSpan.FromSeconds(1);
        private int defaultHeight;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Splash"/> class.
		/// </summary>
		public Splash()
		{
			InitializeComponent();

			//pictureBox.Image = GetStartupImage();
			defaultHeight = this.Height;

			FileInfo splashImageFile = new FileInfo(
				Path.GetDirectoryName(Application.ExecutablePath) + "\\Data\\Icons\\Interface\\splash.png");

			if(splashImageFile.Exists)
			{
			//	pictureBox.Image = Image.FromFile(splashImageFile.FullName);
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		/// <summary>
		/// Set if an error is displayed.
		/// </summary>
		public bool HasError
		{
			get
			{
				return hasError;
			}

			set
			{
				hasError = value;
				if(hasError)
				{
					// Make space for error messages
					Height = defaultHeight + 80;
				//	label.TextAlign = ContentAlignment.MiddleCenter;
				//	label.BorderStyle = BorderStyle.Fixed3D;
				}
				else
				{
					Height = defaultHeight;
				//	label.TextAlign = ContentAlignment.MiddleLeft;
				//	label.BorderStyle = BorderStyle.None;
				}
			}
		}
		/// <summary>
		/// Display normal message on splash screen.
		/// </summary>
		/// <param name="message">Message to display on the splash screen</param>
		public void SetText(string message)
		{
			if(hasError)
				Wait();
			HasError = false;
		//	this.label.Text = message;
		//	this.label.ForeColor = Color.Black;
			this.Invalidate();
			Application.DoEvents();
		}

		/// <summary>
		/// Display an error message on splash.  Splash will stay visible longer to alert the user.
		/// </summary>
		/// <param name="message">Message to display on the splash screen</param>
		public void SetError(string message)
		{
			if(hasError)
				Wait();
			HasError = true;
			wasClicked = false;
			this.timeOut = TimeSpan.FromSeconds(30);
		//	this.label.Text = message + "\n\nPress any key or click to continue.";
		//	this.label.ForeColor = Color.Red;
			this.Invalidate();
			Application.DoEvents();
		}

		/// <summary>
		/// True when splash is done displaying (timed out or user intervention)
		/// </summary>
		public bool IsDone 
		{
			get
			{
				Application.DoEvents();
				// Remove splash if user got tired, else wait preset time
				if (wasClicked)
					return true;
				TimeSpan timeElapsed = System.DateTime.Now - this.startTime;
				return (timeElapsed >= this.timeOut);
			}
		}

		protected void Wait()
		{
			while(!IsDone)
				Thread.Sleep(100);
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			wasClicked = true;
			base.OnKeyUp(e);
		}

		private void Splash_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			wasClicked = true;
		}

		/// <summary>
		/// Creates the splash/about box picture with version number.
		/// </summary>
		/// <returns></returns>
		public static Image GetStartupImage()
		{
			return Image.FromFile(Path.GetDirectoryName(Application.ExecutablePath) + "\\Data\\Icons\\Interface\\splash.png");
		}

		private static Font LoadFont( string familyName, float emSize, FontStyle newStyle )
		{
			try
			{
				return new Font(familyName, emSize, newStyle );
			}
			catch(ArgumentException)
			{
				// Font load failed.
			}
			// Fall back to default font
			return new Font("", emSize, newStyle);
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Splash));
            this.SuspendLayout();
            // 
            // Splash
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.BackColor = System.Drawing.Color.Lime;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(572, 282);
            this.ControlBox = false;
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "Splash";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Splash";
            this.TransparencyKey = System.Drawing.Color.Lime;
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Splash_MouseDown);
            this.ResumeLayout(false);

		}
		#endregion

		/*protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			ControlPaint.DrawBorder3D(e.Graphics, 0,0,(int)e.Graphics.VisibleClipBounds.Width, (int)e.Graphics.VisibleClipBounds.Height, Border3DStyle.Raised);
		}*/
	}
}
