using System;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using WorldWind.Terrain;

namespace WorldWind.Renderable
{
	/// <summary>
	/// Summary description for PathList.
	/// </summary>
	public class PathList : RenderableObjectList
	{
		BinaryReader dataArchiveReader = null;

		World parentWorld;
		double minDisplayAltitude;
		double maxDisplayAltitude;
		string pathsDirectoryPath;
		double altitude;
		System.Drawing.Color color;
		TerrainAccessor terrainAccessor;
		
		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.PathList"/> class.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parentWorld"></param>
		/// <param name="minDisplayAltitude"></param>
		/// <param name="maxDisplayAltitude"></param>
		/// <param name="pathsDirectoryPath"></param>
		/// <param name="altitude"></param>
		/// <param name="color"></param>
		/// <param name="terrainAccessor"></param>
		public PathList(
			string name,
			World parentWorld,
			double minDisplayAltitude,
			double maxDisplayAltitude,
			string pathsDirectoryPath,
			double altitude,
			System.Drawing.Color color,
			TerrainAccessor terrainAccessor) : base( name )
		{

			this.parentWorld = parentWorld;
			this.minDisplayAltitude = minDisplayAltitude;
			this.maxDisplayAltitude = maxDisplayAltitude;
			this.pathsDirectoryPath = pathsDirectoryPath;
			this.altitude = altitude;
			this.color = color;
			this.terrainAccessor = terrainAccessor;
		}

		public override void Initialize(DrawArgs drawArgs)
		{
			DirectoryInfo pathsDir = new DirectoryInfo(pathsDirectoryPath);

			if(!pathsDir.Exists)
			{
				return;
			}

			FileInfo indexFileInfo = new FileInfo( Path.Combine( pathsDir.FullName, "pathlist.idx" ));
			FileInfo dataFileInfo = new FileInfo( Path.Combine( pathsDir.FullName, "pathlist.pkg" ));

			if(indexFileInfo.Exists && dataFileInfo.Exists) 
			{
				using( BufferedStream indexFileStream = new BufferedStream(indexFileInfo.OpenRead()) )
				using( BinaryReader indexFileReader = new BinaryReader(indexFileStream, System.Text.Encoding.ASCII) ) 
				{
					dataArchiveReader = new BinaryReader(new BufferedStream(dataFileInfo.OpenRead()), System.Text.Encoding.ASCII);

					int count = indexFileReader.ReadInt32();

					for(int i = 0; i < count; i++) 
					{
						string fileName = indexFileReader.ReadString();
						double west = indexFileReader.ReadDouble();
						double south = indexFileReader.ReadDouble();
						double east = indexFileReader.ReadDouble();
						double north = indexFileReader.ReadDouble();
						long offset = indexFileReader.ReadInt64();

						TerrainPath tp = new TerrainPath(
							fileName,
							parentWorld,
							minDisplayAltitude,
							maxDisplayAltitude,
							dataArchiveReader,
							offset,
							0,
							north,
							south,
							east,
							west,
							(float)altitude, color, terrainAccessor);

						tp.ParentList = this;
						m_children.Add(tp);
					}
				}
			}
			else
			{
				foreach(FileInfo file in pathsDir.GetFiles("*.wwb"))
				{
					m_children.Add(new TerrainPath(file.Name, parentWorld, 
						minDisplayAltitude, maxDisplayAltitude,
						file.FullName, (float)altitude, color, terrainAccessor));
				}
			}

			isInitialized = true;
		}

		public override void Dispose()
		{
			try
			{
				foreach(RenderableObject ro in m_children)
					ro.Dispose();
				m_children.Clear();

				if (dataArchiveReader!=null)
					dataArchiveReader.Close();
			}
			finally
			{
				base.Dispose();
			}
		}


		public static void CreateArchiveFromDirectory(string directoryPath)
		{
			DirectoryInfo inDir = new DirectoryInfo(directoryPath);

			if(!inDir.Exists)
				return;

			using( FileStream dataStream = File.Open( Path.Combine( inDir.FullName, "data.pkg" ), FileMode.Create, FileAccess.Write, FileShare.None) )
			using( FileStream indexStream = File.Open( Path.Combine( inDir.FullName, "master.idx" ), FileMode.Create, FileAccess.Write, FileShare.None) )
			using( BinaryWriter indexWriter = new BinaryWriter(indexStream, System.Text.Encoding.ASCII) ) 
			{
				long curOffset = 0;

				FileInfo[] files = inDir.GetFiles("*.wwb");

				indexWriter.Write(files.Length);

				foreach(FileInfo file in files) 
				{
					using( FileStream curBoundaryStream = file.OpenRead() )
					using( BinaryReader curBoundaryReader = new BinaryReader(curBoundaryStream, System.Text.Encoding.ASCII) ) 
					{
						int count = curBoundaryReader.ReadInt32();

						double north = double.MinValue;
						double south = double.MaxValue;
						double east = double.MinValue;
						double west = double.MaxValue;

						for(int i = 0; i < count; i++) 
						{
							float curLat = curBoundaryReader.ReadSingle();
							float curLon = curBoundaryReader.ReadSingle();

							if(curLat < south)
								south = curLat;
							if(curLat > north)
								north = curLat;
							if(curLon < west)
								west = curLon;
							if(curLon > east)
								east = curLon;
						}

						indexWriter.Write(file.Name);
						indexWriter.Write(west);
						indexWriter.Write(south);
						indexWriter.Write(east);
						indexWriter.Write(north);
						indexWriter.Write(curOffset);
						indexWriter.Write(file.Length);

						byte[] buffer = new byte[file.Length];
						curBoundaryStream.Seek(0, SeekOrigin.Begin);
						curBoundaryStream.Read(buffer, 0, (int)file.Length);
						dataStream.Write(buffer, 0, buffer.Length);
					}

					curOffset += file.Length;
				}
			}
		}
	}
}
