using Microsoft.DirectX.Direct3D;
using WorldWind.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Utility;

namespace WorldWind
{
	/// <summary>
	/// Various image manipulation functions.
	/// </summary>
	public sealed class ImageHelper
	{
		/// <summary>
		/// Static class
		/// </summary>
		private ImageHelper()
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
			return GdiSupportedExtensions.IndexOf(extension) >= 0;
		}

		/// <summary>
		/// Loads an image file from disk into a texture.
		/// </summary>
		/// <param name="textureFileName">Path/filename to the image file</param>
		public static Texture LoadTexture(string textureFileName)
		{
			Texture texture = LoadTexture(textureFileName, 0);
			return texture;
		}

		/// <summary>
		/// Loads an image file from disk into a texture.
		/// </summary>
		/// <param name="textureFileName">Path/filename to the image file</param>
		/// <param name="colorKey">Transparent color. Any pixels in the image with this color will be made transparent.</param>
		public static Texture LoadTexture(string textureFileName, int colorKey)
		{
			try
			{
				using (Stream imageStream = File.OpenRead(textureFileName))
					return LoadTexture(imageStream, colorKey);
			}
			catch
			{
				throw new Microsoft.DirectX.Direct3D.InvalidDataException(string.Format("Error reading image file '{0}'.", textureFileName));
			}
		}

		public static void CreateAlphaPngFromBrightness(string srcFilePath, string destinationPngFilePath)
		{
			Bitmap image = (Bitmap)Image.FromFile(srcFilePath);

			BitmapData srcInfo = image.LockBits(new Rectangle(0, 0, 
				image.Width, image.Height), 
				ImageLockMode.ReadOnly, 
				PixelFormat.Format32bppArgb);

			// We must always copy it because the source might not be 32bpp ARGB
			Bitmap transparentImage = new Bitmap(image.Width, image.Height, 
				PixelFormat.Format32bppArgb);

			BitmapData dstInfo = transparentImage.LockBits(new Rectangle(0, 0, 
				transparentImage.Width, transparentImage.Height), 
				ImageLockMode.WriteOnly, 
				PixelFormat.Format32bppArgb);

			unsafe 
			{
				int* srcPointer = (int*)srcInfo.Scan0;
				int* dstPointer = (int*)dstInfo.Scan0;
				for(int i = 0; i < dstInfo.Height; i++) 
				{
					for(int j = 0; j < dstInfo.Width; j++) 
					{
						int color = *srcPointer++;
						int sum = (color & 0xff) + 
							((color >> 8) & 0xff) + 
							((color >> 16) & 0xff);

						color &= 0xffffff; // strip alpha
						color |= (sum / 3) << 24;
					
						*dstPointer++ = color;
					}

					srcPointer += (srcInfo.Stride>>2) - srcInfo.Width;
					dstPointer += (srcInfo.Stride>>2) - srcInfo.Width;
				}
			}
			transparentImage.UnlockBits(dstInfo);
			image.UnlockBits(srcInfo);

			transparentImage.Save(destinationPngFilePath, System.Drawing.Imaging.ImageFormat.Png);
			image.Dispose();
			transparentImage.Dispose();
		}

		/// <summary>
		/// Loads an image file from disk into a texture and makes a color range transparent.
		/// </summary>
		/// <param name="textureFileName">Path/filename to the image file</param>
		/// <param name="transparentRangeDarkColor">Color for start of transparent range.</param>
		/// <param name="transparentRangeBrightColor">Color for end of transparent range.</param>
		/// <returns></returns>
		public static Texture LoadTexture(string textureFileName, int transparentRangeDarkColor, int transparentRangeBrightColor)
		{
			Bitmap image = (Bitmap)Image.FromFile(textureFileName);

			BitmapData srcInfo = image.LockBits(new Rectangle(0, 0, 
				image.Width, image.Height), 
				ImageLockMode.ReadOnly, 
				PixelFormat.Format32bppArgb);

			// We must always copy it because the source might not be 32bpp ARGB
			Bitmap transparentImage = new Bitmap(image.Width, image.Height, 
				PixelFormat.Format32bppArgb);

			BitmapData dstInfo = transparentImage.LockBits(new Rectangle(0, 0, 
				transparentImage.Width, transparentImage.Height), 
				ImageLockMode.WriteOnly, 
				PixelFormat.Format32bppArgb);

			// TODO: Optimize this code
			int max = 3*(transparentRangeBrightColor & 0xff);
			int min = 3*(transparentRangeDarkColor & 0xff);
			unsafe 
			{
				int* srcPointer = (int*)srcInfo.Scan0;
				int* dstPointer = (int*)dstInfo.Scan0;
				for(int i = 0; i < dstInfo.Height; i++) 
				{
					for(int j = 0; j < dstInfo.Width; j++) 
					{
						int color = *srcPointer++;
						int sum = (color & 0xff) + 
							((color >> 8) & 0xff) + 
							((color >> 16) & 0xff);

						if(sum <= max && sum >= min)
						{
							color &= 0xffffff; // strip alpha
							// Add linear alpha: min = transparent, max = opaque
							color |= (255 * (sum - min) / (max-min)) << 24;
						}
						
						*dstPointer++ = color;
					}

					srcPointer += (srcInfo.Stride>>2) - srcInfo.Width;
					dstPointer += (srcInfo.Stride>>2) - srcInfo.Width;
				}
			}
			transparentImage.UnlockBits(dstInfo);
			image.UnlockBits(srcInfo);

			return new Texture(DrawArgs.Device, transparentImage, Usage.None, Pool.Managed);
		}

		/// <summary>
		/// Creates a texture from a data stream.
		/// </summary>
		/// <param name="textureFileName">Stream containing the image file</param>
		public static Texture LoadTexture(Stream textureStream)
		{
			Texture texture = LoadTexture(textureStream, 0);
			return texture;
		}

		/// <summary>
		/// Creates a texture from a data stream.
		/// </summary>
		/// <param name="textureFileName">Stream containing the image file</param>
		/// <param name="textureStream"></param>
		/// <param name="colorKey">Transparent color. Any pixels in the image with this color will be made transparent.</param>
		public static Texture LoadTexture(Stream textureStream, int colorKey)
		{
			try
			{
				Texture texture = TextureLoader.FromStream(DrawArgs.Device, textureStream, 0, 0,
					1, Usage.None, World.Settings.TextureFormat, Pool.Managed, Filter.Box, Filter.Box, colorKey);

				return texture;
			}
			catch (Microsoft.DirectX.Direct3D.InvalidDataException)
			{
			}

			try
			{
				// DirectX failed to load the file, try GDI+
				// Additional formats supported by GDI+: GIF, TIFF
				// TODO: Support color keying.  See: System.Drawing.Imaging.ImageAttributes
				using (Bitmap image = (Bitmap)Image.FromStream(textureStream))
				{
					Texture texture = new Texture(DrawArgs.Device, image, Usage.None, Pool.Managed);
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
		public static Image LoadImage(string bitmapFileName)
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
		public static Cursor LoadCursor(string relativePath)
		{
			string fullPath = Path.Combine("Data\\Icons\\Interface", relativePath);
			try
			{
				Cursor res = new Cursor(fullPath);
				return res;
			}
			catch (Exception caught)
			{
				Log.Write(Log.Levels.Error, "IMAG", "Unable to load cursor '" + relativePath + "': " + caught.Message);
				return Cursors.Default;
			}
		}

		/// <summary>
		/// Loads an icon texture from a file
		/// </summary>
		/// <param name="relativePath">Path and filename relative to Data\Icons</param>
		public static Texture LoadIconTexture(string relativePath)
		{
			try
			{
				string fullPath = FindResource(relativePath);
				if (File.Exists(fullPath))
					return TextureLoader.FromFile(DrawArgs.Device, fullPath, 0, 0, 1, Usage.None,
						Format.Dxt5, Pool.Managed, Filter.Box, Filter.Box, 0);
			}
			catch
			{
                Log.Write(Log.Levels.Error, "IMAG", "Error loading texture '" + relativePath + "'.");
			}

			// Make a replacement warning texture with a red cross over.
			using (Bitmap bitmap = CreateDefaultImage())
				return new Texture(DrawArgs.Device, bitmap, 0, Pool.Managed);
		}

		/// <summary>
		/// Tries it's best to locate an image file specified using relative path.
		/// </summary>
		/// <param name="relativePath"></param>
		public static string FindResource(string relativePath)
		{
			if(File.Exists(relativePath))
				return relativePath;

			FileInfo executableFile = new FileInfo(System.Windows.Forms.Application.ExecutablePath);

			string fullPath = Path.Combine(Path.Combine(executableFile.Directory.FullName, "Data"), relativePath);
			if(File.Exists(fullPath))
				return fullPath;
			fullPath = Path.Combine(executableFile.Directory.FullName, relativePath);
			if(File.Exists(fullPath))
				return fullPath;

			fullPath = Path.Combine(Path.Combine(executableFile.Directory.FullName, "Data\\Icons"), relativePath);
			return fullPath;
		}

		/// <summary>
		/// Converts an image in any format readable by GDI+ to a DXT1 DDS file.
		/// </summary>
		/// <param name="originalImagePath">Input file (any supported format).</param>
		/// <param name="outputDdsPath">Output file to be created.</param>
		public static void ConvertToDxt1(string originalImagePath, string outputDdsPath, bool eraseOriginal)
		{
			ConvertToDds(originalImagePath, outputDdsPath, Format.Dxt1, eraseOriginal);
		}

		/// <summary>
		/// Converts an image in any format readable by GDI+ to a DXT1 DDS file.
		/// </summary>
		/// <param name="originalImageStream">Stream containing a bitmap.</param>
		/// <param name="originalImagePath">Input file (any supported format).</param>
		public static void ConvertToDxt1(Stream originalImageStream, string outputDdsPath)
		{
			ConvertToDds(originalImageStream, outputDdsPath, Format.Dxt1);
		}

		/// <summary>
		/// Converts an image in any format readable by GDI+ to a DXT3 DDS file.
		/// </summary>
		/// <param name="originalImagePath">Input file (any supported format).</param>
		/// <param name="outputDdsPath">Output file to be created.</param>
		public static void ConvertToDxt3(string originalImagePath, string outputDdsPath, bool eraseOriginal)
		{
			ConvertToDds(originalImagePath, outputDdsPath, Format.Dxt3, eraseOriginal);
		}

		/// <summary>
		/// Converts an image in any format readable by GDI+ to a DXT3 DDS file.
		/// </summary>
		/// <param name="originalImageStream">Stream containing a bitmap.</param>
		/// <param name="outputDdsPath">Output file to be created.</param>
		public static void ConvertToDxt3(Stream originalImageStream, string outputDdsPath)
		{
			ConvertToDds(originalImageStream, outputDdsPath, Format.Dxt3);
		}

		/// <summary>
		/// Converts an image in any format readable by GDI+ to a DDS file.
		/// </summary>
		/// <param name="originalImagePath">Input file (any supported format).</param>
		/// <param name="outputDdsPath">Output file to be created.</param>
		/// <param name="format">DirectX format of file.</param>
		public static void ConvertToDds(string originalImagePath, string outputDdsPath, Format format, bool eraseOriginal)
		{
			try
			{
				using (Texture t = TextureLoader.FromFile(
								 DrawArgs.Device,
								 originalImagePath,
								 0, 0,
								 1, 0, format, Pool.Scratch,
								 Filter.Box | Filter.DitherDiffusion, Filter.None, 0))
					TextureLoader.Save(outputDdsPath, ImageFileFormat.Dds, t);

				if (eraseOriginal)
					File.Delete(originalImagePath);
			}
			catch (Microsoft.DirectX.Direct3D.InvalidDataException)
			{
				throw new ApplicationException(string.Format("Failed to load image data from {0}.", originalImagePath));
			}
		}

		/// <summary>
		/// Converts an image in any format readable by GDI+ to a DDS file.
		/// </summary>
		/// <param name="originalImagePath">Input file (any supported format).</param>
		/// <param name="outputDdsPath">Output file to be created.</param>
		/// <param name="format">DirectX format of file.</param>
		public static void ConvertToDds(Stream originalImageStream, string outputDdsPath, Format format)
		{
			try
			{
				originalImageStream.Seek(0, SeekOrigin.Begin);
				using (Texture t = TextureLoader.FromStream(
								 DrawArgs.Device,
								 originalImageStream,
								 0, 0,
								 1, 0, format, Pool.Scratch,
								 Filter.Box | Filter.DitherDiffusion, Filter.None, 0))
					TextureLoader.Save(outputDdsPath, ImageFileFormat.Dds, t);
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
			Bitmap b = new Bitmap(32, 32);
			using (Graphics g = Graphics.FromImage(b))
			{
				g.Clear(Color.FromArgb(88, 255, 255, 255));
				g.DrawLine(Pens.Red, 0, 0, b.Width, b.Height);
				g.DrawLine(Pens.Red, 0, b.Height, b.Width, 0);
			}
			return b;
		}
	}
}