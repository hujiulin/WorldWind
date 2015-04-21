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
using System;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace jhuapl.util
{
	public sealed class JHU_Utilities
	{
		private JHU_Utilities(){}

		public static void DrawLine(Vector2[] linePoints, int color, Device device)
		{
			CustomVertex.TransformedColored[] lineVerts = new CustomVertex.TransformedColored[linePoints.Length];

			for(int i = 0; i < linePoints.Length; i++)
			{
				lineVerts[i].X = linePoints[i].X;
				lineVerts[i].Y = linePoints[i].Y;
				lineVerts[i].Z = 0.0f;

				lineVerts[i].Color = color;
			}

			device.TextureState[0].ColorOperation = TextureOperation.Disable;
			device.VertexFormat = CustomVertex.TransformedColored.Format;

			device.DrawUserPrimitives(PrimitiveType.LineStrip, lineVerts.Length - 1, lineVerts);
		}

		public static void DrawBox(int ulx, int uly, int width, int height, float z, int color, Device device)
		{
			CustomVertex.TransformedColored[] verts = new CustomVertex.TransformedColored[4];
			verts[0].X = (float)ulx;
			verts[0].Y = (float)uly;
			verts[0].Z = z;
			verts[0].Color = color;

			verts[1].X = (float)ulx;
			verts[1].Y = (float)uly + height;
			verts[1].Z = z;
			verts[1].Color = color;

			verts[2].X = (float)ulx + width;
			verts[2].Y = (float)uly;
			verts[2].Z = z;
			verts[2].Color = color;

			verts[3].X = (float)ulx + width;
			verts[3].Y = (float)uly + height;
			verts[3].Z = z;
			verts[3].Color = color;

			device.VertexFormat = CustomVertex.TransformedColored.Format;
			device.TextureState[0].ColorOperation = TextureOperation.Disable;
			device.DrawUserPrimitives(PrimitiveType.TriangleStrip, verts.Length - 2, verts);
		}

		public static void DrawSector(double startdeg, double enddeg, int centerX, int centerY, int radius, float z, int color, Device device)
		{
			int prec = 7;

			CustomVertex.TransformedColored[] verts = new CustomVertex.TransformedColored[prec + 2];
			verts[0].X = centerX;
			verts[0].Y = centerY;
			verts[0].Z = z;
			verts[0].Color = System.Drawing.Color.Red.ToArgb(); // color;
			double degInc = (double)(enddeg - startdeg) / prec;

			for(int i = 0; i <= prec; i++)
			{
				verts[i + 1].X = (float)Math.Cos((double)(startdeg + degInc * i))*radius + centerX;
				verts[i + 1].Y = (float)Math.Sin((double)(startdeg + degInc * i))*radius*(-1.0f) + centerY;
				verts[i + 1].Z = z;
				verts[i + 1].Color = color;
			}

			device.VertexFormat = CustomVertex.TransformedColored.Format;
			device.TextureState[0].ColorOperation = TextureOperation.Disable;
			device.DrawUserPrimitives(PrimitiveType.TriangleFan, verts.Length - 2, verts);
		}

		public static string Degrees2DMS(double decimalDegrees, char pos, char neg)
		{
			char dir = pos;
 
			if (decimalDegrees < 0)
			{
				dir = neg;
				decimalDegrees *= -1.0;
			}
        
			long deg = (long) decimalDegrees;
 
			decimalDegrees = (decimalDegrees - (double) deg) * 60.0;
 
			long min = (long) decimalDegrees;
 
			decimalDegrees = (decimalDegrees - (double) min) * 60.0;
 
			double sec = ( (double) Math.Round(decimalDegrees * Math.Pow(10,3)))/Math.Pow(10,3);

			if ( (long) sec == 60.0 )
			{
				sec-=60.0;
				min++;
			}
        
			if(min >= 60L)
			{
				deg++;
				min=0L;
			}

			deg = deg % 360;
 
			while ( deg >= 360L ) deg -= 360L;

			if (pos.ToString().ToUpper() == "E")
				return String.Format("{0:000}° {1:00}' {2:00.000}\" {3}", deg, min, sec, dir);
			else
				return String.Format("{0:00}° {1:00}' {2:00.000}\" {3}", deg, min, sec, dir);
		}
	}
}
