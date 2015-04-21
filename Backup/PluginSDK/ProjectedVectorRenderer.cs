using System;
using Utility;

namespace WorldWind
{
	/// <summary>
	/// Summary description for ProjectedVectorRenderer.
	/// </summary>
	public class ProjectedVectorRenderer
	{
		ProjectedVectorTile[] m_rootTiles = null;
		double m_lzts = 36.0;
		public bool EnableCaching = false;
		public World World = null;
		public System.Drawing.Size TileSize = new System.Drawing.Size(256, 256);
		System.Collections.ArrayList m_polygons = new System.Collections.ArrayList();
		System.Collections.ArrayList m_lineStrings = new System.Collections.ArrayList();
		public System.DateTime LastUpdate = System.DateTime.Now;

		public Polygon[] Polygons
		{
			get{ return (Polygon[])m_polygons.ToArray(typeof(Polygon)); }
		}

		public LineString[] LineStrings
		{
			get{ return (LineString[])m_lineStrings.ToArray(typeof(LineString)); }
		}

		public ProjectedVectorRenderer(World parentWorld)
		{
			World = parentWorld;
			UpdateRootTiles();
		}

		private void UpdateRootTiles()
		{
			int numberRows = (int)(180.0f/m_lzts);
			
			System.Collections.ArrayList tileList = new System.Collections.ArrayList();

			int istart = 0;
			int iend = numberRows;
			int jstart = 0;
			int jend = numberRows * 2;

			for(int i = istart; i < iend; i++)
			{
				for(int j = jstart; j < jend; j++)
				{
					double north = (i + 1) * m_lzts - 90.0f;
					double south = i  * m_lzts - 90.0f;
					double west = j * m_lzts - 180.0f;
					double east = (j + 1) * m_lzts - 180.0f;
					
					ProjectedVectorTile newTile = new ProjectedVectorTile(
						new GeographicBoundingBox(
						north,
						south,
						west,
						east),
						this);

					newTile.Level = 0;
					newTile.Row = i;
					newTile.Col = j;
					tileList.Add(newTile);
				}
			}

			m_rootTiles = (ProjectedVectorTile[])tileList.ToArray(typeof(ProjectedVectorTile));
		}


		public void Add(Polygon polygon)
		{
            if(polygon.ParentRenderable == null)
                Log.Write(Log.Levels.Error, "null null null");
			polygon.Visible = IsRenderableVisible(polygon.ParentRenderable);
			m_polygons.Add(polygon);
			LastUpdate = System.DateTime.Now;
		}

		public void Add(LineString lineString)
		{
			lineString.Visible = IsRenderableVisible(lineString.ParentRenderable);
			m_lineStrings.Add(lineString);
			LastUpdate = System.DateTime.Now;
		}

		public void Update(DrawArgs drawArgs)
		{
			for(int i = 0; i < m_lineStrings.Count; i++)
			{
				LineString lineString = (LineString)m_lineStrings[i];
				if(lineString.Remove)
				{
					m_lineStrings.RemoveAt(i);
					LastUpdate = System.DateTime.Now;
					i--;
				}
				else if(lineString.ParentRenderable != null)
				{
					bool visibility =  IsRenderableVisible(lineString.ParentRenderable);
					if(visibility != lineString.Visible)
					{
						lineString.Visible = visibility;
						LastUpdate = System.DateTime.Now;
					}
				}
			}

			for(int i = 0; i < m_polygons.Count; i++)
			{
				Polygon polygon = (Polygon)m_polygons[i];
				if(polygon.Remove)
				{
					m_polygons.RemoveAt(i);
					LastUpdate = System.DateTime.Now;
					i--;
				}
				
				else if(polygon.ParentRenderable != null)
				{
					bool visibility =  IsRenderableVisible(polygon.ParentRenderable);
					if(visibility != polygon.Visible)
					{
						polygon.Visible = visibility;
						LastUpdate = System.DateTime.Now;
					}
				}
			}

			foreach(ProjectedVectorTile tile in m_rootTiles)
				tile.Update(drawArgs);
		}

		private static bool IsRenderableVisible(WorldWind.Renderable.RenderableObject renderable)
		{
			if(!renderable.IsOn)
			{
				return false;
			}
			else if(renderable.ParentList != null)
			{
				return IsRenderableVisible(renderable.ParentList);
			}
			else
			{
				return true;
			}
		}

		public void Render(DrawArgs drawArgs)
		{
			drawArgs.device.Clear(Microsoft.DirectX.Direct3D.ClearFlags.ZBuffer, 0, 1.0f, 0);

			foreach(ProjectedVectorTile tile in m_rootTiles)
				tile.Render(drawArgs);
		}
	}
}
