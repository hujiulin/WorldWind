//========================= (UNCLASSIFIED) ==============================
// Copyright © 2005-2006 The Johns Hopkins University /
// Applied Physics Laboratory.  All rights reserved.
//
// WorldWind Source Code - Copyright 2005 NASA World Wind 
// Modified under the NOSA License
//
//========================= (UNCLASSIFIED) ==============================
//
// LICENSE AND DISCLAIMER 
//
// Copyright (c) 2006 The Johns Hopkins University. 
//
// This software was developed at The Johns Hopkins University/Applied 
// Physics Laboratory (“JHU/APL”) that is the author thereof under the 
// “work made for hire” provisions of the copyright law.  Permission is 
// hereby granted, free of charge, to any person obtaining a copy of this 
// software and associated documentation (the “Software”), to use the 
// Software without restriction, including without limitation the rights 
// to copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit others to do so, subject to the 
// following conditions: 
//
// 1.  This LICENSE AND DISCLAIMER, including the copyright notice, shall 
//     be included in all copies of the Software, including copies of 
//     substantial portions of the Software; 
//
// 2.  JHU/APL assumes no obligation to provide support of any kind with 
//     regard to the Software.  This includes no obligation to provide 
//     assistance in using the Software nor to provide updated versions of 
//     the Software; and 
//
// 3.  THE SOFTWARE AND ITS DOCUMENTATION ARE PROVIDED AS IS AND WITHOUT 
//     ANY EXPRESS OR IMPLIED WARRANTIES WHATSOEVER.  ALL WARRANTIES 
//     INCLUDING, BUT NOT LIMITED TO, PERFORMANCE, MERCHANTABILITY, FITNESS
//     FOR A PARTICULAR PURPOSE, AND NONINFRINGEMENT ARE HEREBY DISCLAIMED.  
//     USERS ASSUME THE ENTIRE RISK AND LIABILITY OF USING THE SOFTWARE.  
//     USERS ARE ADVISED TO TEST THE SOFTWARE THOROUGHLY BEFORE RELYING ON 
//     IT.  IN NO EVENT SHALL THE JOHNS HOPKINS UNIVERSITY BE LIABLE FOR 
//     ANY DAMAGES WHATSOEVER, INCLUDING, WITHOUT LIMITATION, ANY LOST 
//     PROFITS, LOST SAVINGS OR OTHER INCIDENTAL OR CONSEQUENTIAL DAMAGES, 
//     ARISING OUT OF THE USE OR INABILITY TO USE THE SOFTWARE. 
//
using System;
using System.Drawing;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using WorldWind;
using WorldWind.Renderable;

namespace jhuapl.util
{
	/// <summary>
	/// This is the classification banner for CollabSpace.
	/// </summary>
	public class JHU_Banner : RenderableObject
	{
		public enum ClassificationLevel
		{
			UNCLASS,
			CONFIDENTIAL,
			SECRET,
			TOPSECRET
		}

		public string[] ClassificationString =
		{
			"UNCLASSIFIED",
			"CONFIDENTIAL",
			"SECRET",
			"TOP SECRET"
		};

		public int[] ClassificationColor = 
		{
			Color.PaleGreen.ToArgb(),
			Color.Turquoise.ToArgb(),
			Color.Red.ToArgb(),
			Color.Orange.ToArgb()
		};

		#region private members
		
		private const int distanceFromRight = 65;
		private const int distanceFromBottom = 5;

		#endregion

		#region public members

		public ClassificationLevel Classification;

		#endregion

		/// <summary>
		/// Default Constructor
		/// </summary>
		public JHU_Banner() : base("Banner", Vector3.Empty, Quaternion.Identity)
		{
			// draw with icons (on top)
			this.RenderPriority = RenderPriority.Icons;

			// enable this layer
			this.IsOn = true;

			// start as unclass
			Classification = ClassificationLevel.UNCLASS;
		}

		#region RenderableObject methods

		/// <summary>
		/// RenderableObject abstract member (needed)
		/// OBS: Worker thread (don't update UI directly from this thread)
		/// </summary>
		public override void Dispose()
		{
		}

		/// <summary>
		/// RenderableObject abstract member (needed) 
		/// OBS: Worker thread (don't update UI directly from this thread)
		/// </summary>
		public override void Initialize(DrawArgs drawArgs)
		{
		}

		/// <summary>
		/// RenderableObject abstract member (needed)
		/// Called from UI thread = UI code safe in this function
		/// </summary>
		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			return false;
		}

		/// <summary>
		/// This is where we do our rendering 
		/// Called from UI thread = UI code safe in this function
		/// </summary>
		public override void Render(DrawArgs drawArgs)
		{
			// Draw the current time using default font in lower right corner
			string text = ClassificationString[(int) Classification] + " - " + DateTime.Now.ToString();
			Rectangle bounds = drawArgs.defaultDrawingFont.MeasureString(null, text, DrawTextFormat.None, 0);
			drawArgs.defaultDrawingFont.DrawText(null, text, 
				(drawArgs.screenWidth-bounds.Width)/2, drawArgs.screenHeight-bounds.Height-distanceFromBottom,
				ClassificationColor[(int) Classification] );
		}
		
		/// <summary>
		/// RenderableObject abstract member (needed)
		/// OBS: Worker thread (don't update UI directly from this thread)
		/// </summary>
		public override void Update(DrawArgs drawArgs)
		{
		}

		#endregion
	}
}
