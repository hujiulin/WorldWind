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
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using WorldWind;
using WorldWind.Renderable;
using WorldWind.Menu;
using System.Windows.Forms;

namespace jhuapl.util
{
	/// <summary>
	/// An icon texture to be stored in the global texture table
	/// </summary>
	public class JHU_IconTexture : IDisposable
	{
		public Texture Texture;
		public int Width;
		public int Height;
		public int ReferenceCount;


		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.IconTexture"/> class 
		/// from a texture file on disk.
		/// </summary>
		public JHU_IconTexture(Device device, string textureFileName)
		{
			if(JHU_ImageHelper.IsGdiSupportedImageFormat(textureFileName))
			{
				// Load without rescaling source bitmap
				using(Image image = JHU_ImageHelper.LoadImage(textureFileName))
					LoadImage(device, image);
			}
			else
			{
				// Only DirectX can read this file, might get upscaled depending on input dimensions.
				Texture = JHU_ImageHelper.LoadIconTexture( device, textureFileName );
				// Read texture level 0 size
				using(Surface s = Texture.GetSurfaceLevel(0))
				{
					SurfaceDescription desc = s.Description;
					Width = desc.Width;
					Height = desc.Height;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.IconTexture"/> class 
		/// from a bitmap.
		/// </summary>
		public JHU_IconTexture(Device device, Bitmap image)
		{
			LoadImage(device, image);
		}

		protected void LoadImage(Device device, Image image)
		{
			Width = (int)Math.Round(Math.Pow(2, (int)(Math.Ceiling(Math.Log(image.Width)/Math.Log(2)))));
			if(Width>device.DeviceCaps.MaxTextureWidth)
				Width = device.DeviceCaps.MaxTextureWidth;

			Height = (int)Math.Round(Math.Pow(2, (int)(Math.Ceiling(Math.Log(image.Height)/Math.Log(2)))));
			if(Height>device.DeviceCaps.MaxTextureHeight)
				Height = device.DeviceCaps.MaxTextureHeight;

			using(Bitmap textureSource = new Bitmap(Width, Height))
			using(Graphics g = Graphics.FromImage(textureSource))
			{
				g.DrawImage(image, 0,0,Width,Height);
				if(Texture!=null)
					Texture.Dispose();
				Texture = new Texture(device, textureSource, Usage.None, Pool.Managed);
			}
		}


		#region IDisposable Members

		public void Dispose()
		{
			if(Texture!=null)
			{
				Texture.Dispose();
				Texture = null;
			}
			
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
