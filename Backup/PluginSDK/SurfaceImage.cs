using System;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace WorldWind
{
	public class SurfaceImage : IComparable
	{
		#region Private Members
		string m_ImageFilePath;
		double m_North;
		double m_South;
		double m_West;
		double m_East;
		Texture m_Texture = null;
		bool m_Enabled = true;
		WorldWind.Renderable.RenderableObject m_ParentRenderable;

		public System.DateTime LastUpdate = System.DateTime.Now;
		#endregion

		public byte Opacity = 255;

		#region Properties
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="SurfaceImage"/> is enabled. Useful if you don't want the surface renderer 
		/// to render an image without removing the image from the renderer
		/// </summary>
		/// <value>
		/// 	<c>true</c> if enabled; otherwise, <c>false</c>.
		/// </value>
		public bool Enabled
		{
			get
			{
				return m_Enabled;
			}

			set
			{
				m_Enabled = value;
			}
		}
		public double North
		{
			get
			{
				return m_North;
			}
		}
		public double South
		{
			get
			{
				return m_South;
			}
		}
		public double West
		{
			get
			{
				return m_West;
			}
		}
		public double East
		{
			get
			{
				return m_East;
			}
		}

		public string ImageFilePath
		{
			get
			{
				return m_ImageFilePath;
			}
		}
		public WorldWind.Renderable.RenderableObject ParentRenderable
		{
			get
			{
				return m_ParentRenderable;
			}
		}
		public Texture ImageTexture
		{
			get
			{
				return m_Texture;
			}
			set
			{
				m_Texture = value;
			}
		}
		#endregion
	
		public SurfaceImage(
			string imageFilePath,
			double north,
			double south,
			double west,
			double east,
			Texture texture,
			WorldWind.Renderable.RenderableObject parentRenderable)
		{
			m_ParentRenderable = parentRenderable;
			m_ImageFilePath = imageFilePath;
			m_North = north;
			m_South = south;
			m_West = west;
			m_East = east;
			m_Texture = texture;
		}

		public void Dispose()
		{
		/*	if(m_Texture != null && !m_Texture.Disposed)
			{
				m_Texture.Dispose();
				m_Texture = null;
			}*/
		}

		public Vector2 GetTextureCoordinate(double latitude, double longitude)
		{
			double deltaLat = m_North - latitude;
			double deltaLon = longitude - m_West;

			double latRange = m_North - m_South;
			double lonRange = m_East - m_West;

			Vector2 v = new Vector2(
				(float)(deltaLat / latRange),
				(float)(deltaLon / lonRange));

		/*	float borderDifference = 1.0f / ( 0.5f * WorldSurfaceRenderer.RenderSurfaceSize);
			if(v.X >= 0 && v.X < borderDifference)
			{
				v.X = borderDifference;
			}
			else if(v.X < 0 && v.X > -borderDifference)
			{
				v.X = -borderDifference;
			}
			
			if(v.X <= 1 && v.X > 1 - borderDifference)
			{
				v.X = 1 - borderDifference;
			}
			else if(v.X > 1 && v.X < 1 + borderDifference)
			{
				v.X = 1 + borderDifference;
			}

			if(v.Y >= 0 && v.Y < borderDifference)
			{
				v.Y = borderDifference;
			}
			else if(v.Y < 0 && v.Y > -borderDifference)
			{
				v.Y = -borderDifference;
			}
			
			if(v.Y <= 1 && v.Y > 1 - borderDifference)
			{
				v.Y = 1 - borderDifference;
			}
			else if(v.Y > 1 && v.Y < 1 + borderDifference)
			{
				v.Y = 1 + borderDifference;
			}

*/
			return v;
		}
		private string getZOrderStringFromIRenderable(WorldWind.Renderable.RenderableObject renderable)
		{
			if(renderable.ParentList != null)
			{
				return getZOrderStringFromIRenderable(renderable.ParentList) + "." + getIRenderableIndexFromParent(renderable).ToString().PadLeft(5, '0');
			}
			else
			{
				return "";
			}
		}

		private int getIRenderableIndexFromParent(WorldWind.Renderable.RenderableObject renderable)
		{
			if(renderable.ParentList == null)
			{
				return -1;
			}
			else
			{
				for(int index = 0; index < renderable.ParentList.ChildObjects.Count; index++)
				{
					if(renderable == renderable.ParentList.ChildObjects[index])
					{
						return index;
					}
				}
			}

			return -1;
		}

		public int CompareTo(object obj)
		{
			if(!(obj is SurfaceImage))
				return 1;
			
			SurfaceImage robj = obj as SurfaceImage;

			string parentRenderableZOrderString = getZOrderStringFromIRenderable(m_ParentRenderable);
			string objectRenderableZOrderString = getZOrderStringFromIRenderable(robj.ParentRenderable);
		
			if(parentRenderableZOrderString == objectRenderableZOrderString)
			{
				double parentSourceRange = m_North - m_South;
				parentSourceRange = 1.0 / parentSourceRange;
				double objectSourceRange = robj.North - robj.South;
				objectSourceRange = 1.0 / objectSourceRange;

				return parentSourceRange.CompareTo(objectSourceRange);
			}
			else
			{
				return parentRenderableZOrderString.CompareTo(objectRenderableZOrderString);
			}
		}
	}
}
