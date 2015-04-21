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
// Copyright (c) 2005 The Johns Hopkins University. 
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
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System;
using System.IO;
using System.Windows.Forms;

using WorldWind;
using Utility;

namespace jhuapl.util
{
	/// <summary>
	/// Various image manipulation functions.  Evidently not in 1.2 because plugin fails to load there.
	/// </summary>
	public sealed class JHU_ImageHelper
	{
		/// <summary>
		/// Static class
		/// </summary>
		private JHU_ImageHelper() 
		{
		}

		/// <summary>
		/// Tests based on file extension whether the image format is supported by GDI+ image loader.
		/// </summary>
		/// <param name="imageFileName">Full path or just filename incl. extension.</param>
		public static bool IsGdiSupportedImageFormat(string imageFileName)
		{
			string extension = Path.GetExtension(imageFileName).ToLower();
			const string GdiSupportedExtensions = ".bmp.gif.jpg.jpeg.png.gif.tif";
			return GdiSupportedExtensions.IndexOf(extension)>=0;
		}

		/// <summary>
		/// Loads an image file from disk into a texture.
		/// </summary>
		/// <param name="device">Initialized Direct3D device</param>
		/// <param name="textureFileName">Path/filename to the image file</param>
		public static Texture LoadTexture( Device device, string textureFileName )
		{
			return LoadTexture(device, textureFileName, 0);
		}

		/// <summary>
		/// Loads an image file from disk into a texture.
		/// </summary>
		/// <param name="device">Initialized Direct3D device</param>
		/// <param name="textureFileName">Path/filename to the image file</param>
		public static Texture LoadTexture( Device device, string textureFileName, int transparentColor )
		{
			try
			{
				using(Stream imageStream = File.OpenRead(textureFileName))
					return LoadTexture(device, imageStream, transparentColor);
			}
			catch
			{
				throw new Microsoft.DirectX.Direct3D.InvalidDataException("Error reading image file '" + textureFileName +"'.");
			}
		}

		/// <summary>
		/// Creates a texture from a data stream.
		/// </summary>
		/// <param name="device">Initialized Direct3D device</param>
		/// <param name="textureFileName">Stream containing the image file</param>
		public  static Texture LoadTexture( Device device, Stream textureStream )
		{
			return LoadTexture(device, textureStream, 0);
		}

		/// <summary>
		/// Creates a texture from a data stream.
		/// </summary>
		/// <param name="device">Initialized Direct3D device</param>
		/// <param name="textureFileName">Stream containing the image file</param>
		public  static Texture LoadTexture( Device device, Stream textureStream, int transparentColor )
		{
			try
			{
				Texture texture = TextureLoader.FromStream(device, textureStream, 0, 0, 
					1, Usage.None, World.Settings.TextureFormat, Pool.Managed, Filter.Box, Filter.Box, transparentColor);
				
				return texture;
			}
            catch (Microsoft.DirectX.Direct3D.InvalidDataException)
			{
			}
			
			try
			{
				// DirectX failed to load the file, try GDI+
				// Additional formats supported by GDI+: GIF, TIFF
				using(Bitmap image = (Bitmap)Image.FromStream(textureStream))
				{
					Texture texture = new Texture(device, image, Usage.None, Pool.Managed);
					return texture;
				}
			}
			catch
			{
                throw new Microsoft.DirectX.Direct3D.InvalidDataException("Error reading image stream.");
			}
		}

		/// <summary>
		/// Loads image from file. Returns dummy image on load fail.
		/// </summary>
		public static Image LoadImage( string bitmapFileName )
		{
			try
			{
				return Image.FromFile(bitmapFileName);
			}
			catch
			{
				Log.Write(Log.Levels.Error, "IMAG", "Error loading image '" + bitmapFileName + "'.");
				return CreateDefaultImage();
			}
		}

		/// <summary>
		/// Loads a custom mouse cursor from file
		/// </summary>
		/// <param name="relativePath">Path and filename of the .cur file relative to Data\Icons\Interface</param>
		public static Cursor LoadCursor( string relativePath )
		{
			string fullPath = Path.Combine("Data\\Icons\\Interface", relativePath );
			try
			{
				Cursor res = new Cursor(fullPath);
				return res;
			}
			catch(Exception caught)
			{
				Log.Write(Log.Levels.Error, "IMAG", "Unable to load cursor '"+relativePath+"': "  + caught.Message);
				return Cursors.Default;
			}
		}

		/// <summary>
		/// Loads an icon texture from a file
		/// </summary>
		/// <param name="relativePath">Path and filename relative to Data\Icons, absolute paths also valid.</param>
		public static Texture LoadIconTexture( Device device, string relativePath )
		{
			try
			{
				if(!Path.IsPathRooted(relativePath))
					relativePath = Path.Combine("Data\\Icons", relativePath );
				if(File.Exists(relativePath))
					return TextureLoader.FromFile(device, relativePath, 0, 0, 1,Usage.None, 
						Format.Dxt5, Pool.Managed, Filter.Box, Filter.Box, 0 );
			}
			catch
			{
				Log.Write(Log.Levels.Error, "IMAG", "Error loading texture '" + relativePath + "'.");
			}

			// Make a replacement warning texture with a red cross over.
			using(Bitmap bitmap = CreateDefaultImage())
				return new Texture(device, bitmap, 0, Pool.Managed);
		}

		/// <summary>
		/// Converts an image in any format readable by GDI+ to a DXT1 DDS file.
		/// </summary>
		/// <param name="originalImagePath">Input file (any supported format).</param>
		/// <param name="outputDdsPath">Output file to be created.</param>
		/// <param name="device">Initialized Direct3D device.</param>
		public static void ConvertToDxt1(string originalImagePath, string outputDdsPath, Device device, bool eraseOriginal)
		{
			ConvertToDds(originalImagePath, outputDdsPath, Format.Dxt1, device, eraseOriginal);
		}

		/// <summary>
		/// Converts an image in any format readable by GDI+ to a DXT1 DDS file.
		/// </summary>
		/// <param name="originalImageStream">Stream containing a bitmap.</param>
		/// <param name="originalImagePath">Input file (any supported format).</param>
		/// <param name="device">Initialized Direct3D device.</param>
		public static void ConvertToDxt1(Stream originalImageStream, string outputDdsPath, Device device)
		{
			ConvertToDds(originalImageStream,outputDdsPath,Format.Dxt1,device);
		}

		/// <summary>
		/// Converts an image in any format readable by GDI+ to a DXT3 DDS file.
		/// </summary>
		/// <param name="originalImagePath">Input file (any supported format).</param>
		/// <param name="outputDdsPath">Output file to be created.</param>
		/// <param name="device">Initialized Direct3D device.</param>
		public static void ConvertToDxt3(string originalImagePath, string outputDdsPath, Device device, bool eraseOriginal)
		{
			ConvertToDds(originalImagePath, outputDdsPath, Format.Dxt3, device, eraseOriginal);
		}

		/// <summary>
		/// Converts an image in any format readable by GDI+ to a DXT3 DDS file.
		/// </summary>
		/// <param name="originalImageStream">Stream containing a bitmap.</param>
		/// <param name="outputDdsPath">Output file to be created.</param>
		/// <param name="device">Initialized Direct3D device.</param>
		public static void ConvertToDxt3(Stream originalImageStream, string outputDdsPath, Device device)
		{
			ConvertToDds(originalImageStream,outputDdsPath,Format.Dxt3,device);
		}

		/// <summary>
		/// Converts an image in any format readable by GDI+ to a DDS file.
		/// </summary>
		/// <param name="originalImagePath">Input file (any supported format).</param>
		/// <param name="outputDdsPath">Output file to be created.</param>
		/// <param name="format">DirectX format of file.</param>
		/// <param name="device">Initialized Direct3D device.</param>
		public static void ConvertToDds(string originalImagePath, string outputDdsPath, Format format, Device device, bool eraseOriginal)
		{
			try
			{
				using( Texture t = TextureLoader.FromFile(
								device,
								originalImagePath,
								0, 0,
								1,0, format, Pool.Scratch, 
								Filter.Box | Filter.DitherDiffusion, Filter.None, 0))
					TextureLoader.Save(outputDdsPath,ImageFileFormat.Dds,t);

				if(eraseOriginal)
					File.Delete(originalImagePath);
			}
            catch (Microsoft.DirectX.Direct3D.InvalidDataException)
			{
				throw new ApplicationException("Failed to load image data from " + originalImagePath +".");
			}
		}

		/// <summary>
		/// Converts an image in any format readable by GDI+ to a DDS file.
		/// </summary>
		/// <param name="originalImagePath">Input file (any supported format).</param>
		/// <param name="outputDdsPath">Output file to be created.</param>
		/// <param name="format">DirectX format of file.</param>
		/// <param name="device">Initialized Direct3D device.</param>
		public static void ConvertToDds(Stream originalImageStream, string outputDdsPath, Format format, Device device)
		{
			try
			{
				originalImageStream.Seek(0, SeekOrigin.Begin);
				using( Texture t = TextureLoader.FromStream(
								 device,
								 originalImageStream,
								 0, 0,
								 1, 0, format, Pool.Scratch, 
								 Filter.Box | Filter.DitherDiffusion, Filter.None, 0))
					TextureLoader.Save(outputDdsPath,ImageFileFormat.Dds,t);
			}
            catch (Microsoft.DirectX.Direct3D.InvalidDataException)
			{
				throw new ApplicationException("Failed to load image data from stream.");
			}
		}

		/// <summary>
		/// Makes a default image to use when the requested bitmap wasn't available.
		/// </summary>
		private static Bitmap CreateDefaultImage()
		{
			Bitmap b = new Bitmap(32,32);
			using( Graphics g = Graphics.FromImage(b))
			{
				g.Clear(Color.FromArgb(88,255,255,255));
				g.DrawLine(Pens.Red, 0,0,b.Width, b.Height);
				g.DrawLine(Pens.Red, 0,b.Height,b.Width,0);
			}
			return b;
		}
	}
}