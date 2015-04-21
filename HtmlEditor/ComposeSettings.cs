using System;
using System.Drawing;
using System.Text;
using System.ComponentModel;

namespace onlyconnect
{
	/// <summary>
	/// Summary description for ComposeSettings.
	/// </summary>
	public class ComposeSettings
	{
		
		private Font mFont = new Font("Arial", 10);
		private Color mForeColor = Color.Black;
		private Color mBackColor = Color.White;
		private bool mEnabled = false;
		private HtmlEditor mHtmlEditor = null;

		public ComposeSettings()
		{
			throw new Exception("You must not use this constructor");
		}
		
		public ComposeSettings(HtmlEditor editor)
		{
			//
			// TODO: Add constructor logic here
			//
			this.mHtmlEditor = editor;
		}

		[Description("Enables the use of the default composition font.")]
		public bool Enabled
		{
			get
			{
				return mEnabled;
			}
		
			set
			{
				mEnabled = value;
				mHtmlEditor.setDefaultFont();
			}
		}

        [Browsable(false)]
			public String CommandString
		{
			get
			{
				if (!mEnabled)
				{
					//clear the compose settings
					return ",,,,,,";
				}

				StringBuilder sb = new StringBuilder();

				if (mFont.Bold) sb.Append("1,");
				else sb.Append("0,");
				
				if (mFont.Italic) sb.Append("1,");
				else sb.Append("0,");

				if (mFont.Underline) sb.Append("1,");
				else sb.Append("0,");
			
				if (mFont.SizeInPoints <= 8) sb.Append ("1,");
				else if (mFont.SizeInPoints <= 10) sb.Append ("2,");
				else if (mFont.SizeInPoints <= 12) sb.Append ("3,");
				else if (mFont.SizeInPoints <= 18) sb.Append ("4,");
				else if (mFont.SizeInPoints <= 24) sb.Append ("5,");
				else if (mFont.SizeInPoints <= 36) sb.Append ("6,");
				else sb.Append ("7,");

				sb.Append(mForeColor.R);
				sb.Append(".");
				sb.Append(mForeColor.G);
				sb.Append(".");
				sb.Append(mForeColor.B);
				sb.Append(",");

				sb.Append(mBackColor.R);
				sb.Append(".");
				sb.Append(mBackColor.G);
				sb.Append(".");
				sb.Append(mBackColor.B);
				sb.Append(",");

				sb.Append(mFont.Name);
				return sb.ToString();
			}
		}

		[Description("Get/Sets the default BackColor that will be used for the editor.")]
		public Color BackColor 
		{
			get {return mBackColor;}
			set 
			{
				if (mBackColor != value) 
				{
					mBackColor = value;
					mHtmlEditor.setDefaultFont();
				}
			}
		}

		[Description("Get/Sets the default ForeColor that will be used for the editor.")]
		public Color ForeColor 
		{
			get {return mForeColor;}
			set 
			{
				if (mForeColor != value) 
				{
					mForeColor = value;
					mHtmlEditor.setDefaultFont();
				}
			}
		}

		/// <summary>
		/// Gets/Sets the default font that the editor will use.
		/// </summary>
		[Description("Gets/Sets the default font that the editor will use.")]
		public Font DefaultFont 
		{
			get {return mFont;}
			set 
			{
				if (mFont != value) 
				{
					mFont = value;
					mHtmlEditor.setDefaultFont();
				}
			}
		}
	}
}
