using System;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using WorldWind.Menu;

namespace WorldWind
{
	/// <summary>
	/// Summary description for LineGraph.
	/// </summary>
	public class LineGraph
	{
		float m_Min = 0f;
		float m_Max = 50.0f;

		float[] m_Values = new float[0];

		System.Drawing.Point m_Location = new System.Drawing.Point(100,100);
		System.Drawing.Size m_Size = new System.Drawing.Size(300,100);
		System.Drawing.Color m_BackgroundColor = System.Drawing.Color.FromArgb(100, 0, 0, 0);
		System.Drawing.Color m_LineColor = System.Drawing.Color.Red;

		bool m_Visible = false;
		bool m_ResetVerts = true;

		public bool Visible
		{
			get { return m_Visible; }
			set { m_Visible = value; }
		}

		public float[] Values
		{
			get
			{
				return m_Values;
			}
			set
			{
				m_Values = value;
				m_ResetVerts = true;
			}
		}

		public System.Drawing.Color BackgroundColor
		{
			get
			{
				return m_BackgroundColor;
			}
			set
			{
				m_BackgroundColor = value;
			}
		}

		public System.Drawing.Color LineColor
		{
			get
			{
				return m_LineColor;
			}
			set
			{
				m_LineColor = value;
				m_ResetVerts = true;
			}
		}

		public System.Drawing.Point Location
		{
			get{ return m_Location; }
			set
			{
				if(m_Location != value)
				{
					m_Location = value;
					m_ResetVerts = true;
				}
			 }
		}

		public System.Drawing.Size Size
		{
			get{ return m_Size; }
			set
			{
				if(m_Size != value)
				{
					m_Size = value;
					m_ResetVerts = true;
				}
			}
		}

		public LineGraph()
		{
			
		}

		CustomVertex.TransformedColored[] m_Verts = new Microsoft.DirectX.Direct3D.CustomVertex.TransformedColored[0];

		public void Render(DrawArgs drawArgs)
		{
			if(!m_Visible)
				return;

			MenuUtils.DrawBox(
				m_Location.X,
				m_Location.Y,
				m_Size.Width,
				m_Size.Height,
				0.0f,
				m_BackgroundColor.ToArgb(),
				drawArgs.device);

			if(m_Values == null || m_Values.Length == 0)
				return;

			float xIncr = (float)m_Size.Width / (float)m_Values.Length;

			m_Verts = new CustomVertex.TransformedColored[m_Values.Length];

			if(m_ResetVerts)
			{
				for(int i = 0; i < m_Values.Length; i++)
				{
					if(m_Values[i] < m_Min)
					{
						m_Verts[i].Y = m_Location.Y + m_Size.Height;
					}
					else if(m_Values[i] > m_Max)
					{
						m_Verts[i].Y = m_Location.Y;
					}
					else
					{
						float p = (m_Values[i] - m_Min) / (m_Max - m_Min);
						m_Verts[i].Y = m_Location.Y + m_Size.Height - (float)m_Size.Height * p;
					}
					
					m_Verts[i].X = m_Location.X + i * xIncr;
					m_Verts[i].Z = 0.0f;
					m_Verts[i].Color = m_LineColor.ToArgb();
				}
			}

			drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;
			drawArgs.device.VertexFormat = CustomVertex.TransformedColored.Format;

			drawArgs.device.VertexFormat = CustomVertex.TransformedColored.Format;
			drawArgs.device.DrawUserPrimitives(
				PrimitiveType.LineStrip,
				m_Verts.Length - 1,
				m_Verts);

		}
	}
}
