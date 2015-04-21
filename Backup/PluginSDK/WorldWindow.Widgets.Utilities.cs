using System;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using WorldWind;

namespace WorldWind.Widgets
{
	/// <summary>
	/// Interface must be implemented in order to recieve user input.  Can be used by IRenderables and IWidgets.
	/// </summary>
	public interface IInteractive
	{
		#region Methods
		bool OnKeyDown(KeyEventArgs e);
		
		bool OnKeyUp(KeyEventArgs e);

		bool OnKeyPress(KeyPressEventArgs e);
		
		bool OnMouseDown(MouseEventArgs e);
		
		bool OnMouseEnter(EventArgs e);
		
		bool OnMouseLeave(EventArgs e);
		
		bool OnMouseMove(MouseEventArgs e);
		
		bool OnMouseUp(MouseEventArgs e);
		
		bool OnMouseWheel(MouseEventArgs e);
		#endregion
	}

	/// <summary>
	/// Base Interface for DirectX GUI Widgets
	/// </summary>
	public interface IWidget
	{
		#region Methods
		void Render(DrawArgs drawArgs);
		#endregion

		#region Properties
		IWidgetCollection ChildWidgets{get;set;}
		IWidget ParentWidget{get;set;}
		System.Drawing.Point AbsoluteLocation{get;}
		System.Drawing.Point ClientLocation{get;set;}
		System.Drawing.Size ClientSize{get;set;}
		bool Enabled{get;set;}
		bool Visible{get;set;}
		object Tag{get;set;}
		string Name{get;set;}
		#endregion
	}

	/// <summary>
	/// Collection of IWidgets
	/// </summary>
	public interface IWidgetCollection
	{
		#region Methods
		void BringToFront(int index);
		void BringToFront(IWidget widget);
		void Add(IWidget widget);
		void Clear();
		void Insert(IWidget widget, int index);
		IWidget RemoveAt(int index);
		#endregion

		#region Properties
		int Count{get;}
		#endregion

		#region Indexers
		IWidget this[int index] {get;set;}
		#endregion

	}

	public sealed class Utilities
		{
			private Utilities(){}

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

			public static void DrawSector(double startAngle, double endAngle, int centerX, int centerY, int radius, float z, int color, Device device)
			{
				int prec = 7;

				CustomVertex.TransformedColored[] verts = new CustomVertex.TransformedColored[prec + 2];
				verts[0].X = centerX;
				verts[0].Y = centerY;
				verts[0].Z = z;
				verts[0].Color = color;
				double angleInc = (double)(endAngle - startAngle) / prec;

				for(int i = 0; i <= prec; i++)
				{
					verts[i + 1].X = (float)Math.Cos((double)(startAngle + angleInc * i))*radius + centerX;
					verts[i + 1].Y = (float)Math.Sin((double)(startAngle + angleInc * i))*radius*(-1.0f) + centerY;
					verts[i + 1].Z = z;
					verts[i + 1].Color = color;
				}

				device.VertexFormat = CustomVertex.TransformedColored.Format;
				device.TextureState[0].ColorOperation = TextureOperation.Disable;
				device.DrawUserPrimitives(PrimitiveType.TriangleFan, verts.Length - 2, verts);
			}
		}

	
}
