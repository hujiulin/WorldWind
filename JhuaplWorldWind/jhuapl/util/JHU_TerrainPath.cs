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
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using WorldWind;
using WorldWind.Terrain;
using WorldWind.Renderable;
using Utility;

namespace jhuapl.util
{
	/// <summary>
	/// Class used to create and render a terrain following path
	/// TODO: Re-Implement terrain mapping based on new TerrainAccessor functionality
	/// </summary>
	public class JHU_TerrainPath : RenderableObject
	{
		float north;
		float south;
		float east;
		float west;
		BoundingBox boundingBox;
		World _parentWorld;
		BinaryReader _dataArchiveReader;
		long _fileOffset;
		long _fileSize;
		TerrainAccessor _terrainAccessor;
		float heightAboveSurface;
		string terrainFileName;
		bool isLoaded;
		Vector3 lastUpdatedPosition;
		float verticalExaggeration = 1.0f;
		
		
		double _minDisplayAltitude, _maxDisplayAltitude; 

		int lineColor;
		public CustomVertex.PositionColored[] linePoints;

		#region JHU Changes
		//		Vector3[] sphericalCoordinates = new Vector3[0]; // x = lat, y = lon, z = height
		ArrayList sphericalCoordinates; // x = lat, y = lon, z = height

		bool m_needsUpdate = false;

		public int NumPoints
		{
			get
			{
				if (sphericalCoordinates != null)
					return sphericalCoordinates.Count;
				else
					return 0;
			}
		}

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.TerrainPath"/> class.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parentWorld"></param>
		/// <param name="minDisplayAltitude"></param>
		/// <param name="maxDisplayAltitude"></param>
		/// <param name="terrainFileName"></param>
		/// <param name="heightAboveSurface"></param>
		/// <param name="lineColor"></param>
		/// <param name="terrainAccessor"></param>
		public JHU_TerrainPath(
			string name, 
			World parentWorld, 
			double minDisplayAltitude, 
			double maxDisplayAltitude, 
			string terrainFileName, 
			float heightAboveSurface, 
			System.Drawing.Color lineColor,
			TerrainAccessor terrainAccessor) 
			: base(name, parentWorld.Position, Quaternion.RotationYawPitchRoll(0,0,0)) 
		{
			this._parentWorld = parentWorld;
			this._minDisplayAltitude = minDisplayAltitude;
			this._maxDisplayAltitude = maxDisplayAltitude;
			this.terrainFileName = terrainFileName;
			this.heightAboveSurface = heightAboveSurface;
			this.lineColor = lineColor.ToArgb();
			this._terrainAccessor = terrainAccessor;
			this.RenderPriority = RenderPriority.LinePaths;
			this.sphericalCoordinates = new ArrayList();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.TerrainPath"/> class.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parentWorld"></param>
		/// <param name="minDisplayAltitude"></param>
		/// <param name="maxDisplayAltitude"></param>
		/// <param name="dataArchiveReader"></param>
		/// <param name="fileOffset"></param>
		/// <param name="fileSize"></param>
		/// <param name="north"></param>
		/// <param name="south"></param>
		/// <param name="east"></param>
		/// <param name="west"></param>
		/// <param name="heightAboveSurface"></param>
		/// <param name="lineColor"></param>
		/// <param name="terrainAccessor"></param>
		public JHU_TerrainPath(
			string name, 
			World parentWorld, 
			double minDisplayAltitude, 
			double maxDisplayAltitude, 
			BinaryReader dataArchiveReader,
			long fileOffset,
			long fileSize,
			double north,
			double south,
			double east, 
			double west,
			float heightAboveSurface, 
			System.Drawing.Color lineColor,
			TerrainAccessor terrainAccessor) 
			: base(name, parentWorld.Position, Quaternion.RotationYawPitchRoll(0,0,0)) 
		{
			this._parentWorld = parentWorld;
			this._minDisplayAltitude = minDisplayAltitude;
			this._maxDisplayAltitude = maxDisplayAltitude;
			this._dataArchiveReader = dataArchiveReader;
			this._fileOffset = fileOffset;
			this._fileSize = fileSize;
			this.heightAboveSurface = heightAboveSurface;
			this.lineColor = lineColor.ToArgb();
			this._terrainAccessor = terrainAccessor;
			this.sphericalCoordinates = new ArrayList();
			
			this.north = (float)north;
			this.south = (float)south;
			this.west = (float)west;
			this.east = (float)east;

			this.RenderPriority = RenderPriority.LinePaths;

			this.boundingBox = new BoundingBox( this.south, this.north, this.west, this.east, 
				(float)this._parentWorld.EquatorialRadius, 
				(float)(this._parentWorld.EquatorialRadius + this.verticalExaggeration * heightAboveSurface));
		}

		public override void Initialize(DrawArgs drawArgs)
		{
			this.verticalExaggeration = World.Settings.VerticalExaggeration;
			this.isInitialized = true;
		}

		public void Load(DrawArgs drawArgs)
		{
			try
			{
				#region JHU CHANGES

				if (this.sphericalCoordinates == null)
					this.sphericalCoordinates = new ArrayList();

				#endregion

				if(this._dataArchiveReader == null)
				{
					if(this.terrainFileName == null)
					{
						this.isInitialized = true;
						return;
					}
			
					FileInfo inFile = new FileInfo(this.terrainFileName);
					if(!inFile.Exists)
					{
						this.isInitialized = true;
						return;
					}

					using( BufferedStream fs = new BufferedStream(inFile.OpenRead()) )
					using( BinaryReader br = new BinaryReader(fs) )
					{
						int numCoords = br.ReadInt32();

						#region JHU CHANGES

						//						this.sphericalCoordinates = new Vector3[numCoords];
						//						this.sphericalCoordinates[0].X = br.ReadSingle();
						//						this.sphericalCoordinates[0].Y = br.ReadSingle();
						//
						//						this.north = this.sphericalCoordinates[0].X;
						//						this.south = this.sphericalCoordinates[0].X;
						//						this.west = this.sphericalCoordinates[0].Y;
						//						this.east = this.sphericalCoordinates[0].Y;

						Vector3 newCoord = new Vector3();

						newCoord.X = br.ReadSingle();
						newCoord.Y = br.ReadSingle();
						sphericalCoordinates.Add(newCoord);

						for (int i = 1; i < numCoords; i++)
						{
							//							this.sphericalCoordinates[i].X = br.ReadSingle();
							//							this.sphericalCoordinates[i].Y = br.ReadSingle();
							//
							//							if (this.north < this.sphericalCoordinates[i].X)
							//								this.north = this.sphericalCoordinates[i].X;
							//							if (this.east < this.sphericalCoordinates[i].Y)
							//								this.east = this.sphericalCoordinates[i].Y;
							//							if (this.south > this.sphericalCoordinates[i].X)
							//								this.south = this.sphericalCoordinates[i].X;
							//							if (this.west > this.sphericalCoordinates[i].Y)
							//								this.west = this.sphericalCoordinates[i].Y;

							newCoord.X = br.ReadSingle();
							newCoord.Y = br.ReadSingle();
							sphericalCoordinates.Add(newCoord);

							if (this.north < newCoord.X)
								this.north = newCoord.X;
							if (this.east < newCoord.Y)
								this.east = newCoord.Y;
							if (this.south > newCoord.X)
								this.south = newCoord.X;
							if (this.west > newCoord.Y)
								this.west = newCoord.Y;
						}

						#endregion

					}

					this.boundingBox = new BoundingBox( this.south, this.north, this.west, this.east, 
						(float)this._parentWorld.EquatorialRadius,
						(float)(this._parentWorld.EquatorialRadius + this.verticalExaggeration * heightAboveSurface));
				}
				else
				{
					this._dataArchiveReader.BaseStream.Seek(this._fileOffset, SeekOrigin.Begin);
				
					int numCoords = this._dataArchiveReader.ReadInt32();
					
					byte numElements = this._dataArchiveReader.ReadByte();

					#region JHU CHANGES

					//					this.sphericalCoordinates = new Vector3[numCoords];

					Vector3 newCoord = new Vector3();
				
					for(int i = 0; i < numCoords; i++)
					{
						//						this.sphericalCoordinates[i].X = (float)this._dataArchiveReader.ReadDouble();
						//						this.sphericalCoordinates[i].Y = (float)this._dataArchiveReader.ReadDouble();
						//						if(numElements == 3)			
						//							this.sphericalCoordinates[i].Z = this._dataArchiveReader.ReadInt16();

						newCoord.X = (float)this._dataArchiveReader.ReadDouble();
						newCoord.Y = (float)this._dataArchiveReader.ReadDouble();
						if(numElements == 3)
							newCoord.Z = this._dataArchiveReader.ReadInt16();

						sphericalCoordinates.Add(newCoord);
					}

					#endregion
				}
			}
			catch(Exception caught)
			{
				Log.Write( caught );
			}
			
			this.isLoaded = true;
		}

		public override void Dispose()
		{
			this.isLoaded = false;
			this.linePoints = null;
			this.sphericalCoordinates = null;
			this.isInitialized = false;
		}

		public void SaveToFile(string fileName)
		{
			using (BinaryWriter output = new BinaryWriter(new FileStream(fileName, FileMode.Create)))
			{
				output.Write(this.sphericalCoordinates.Count);
				for (int i = 0; i < this.sphericalCoordinates.Count; i++)
				{
					#region JHU Changes
					Vector3 outCoord = (Vector3) this.sphericalCoordinates[i];
					output.Write(outCoord.X);
					output.Write(outCoord.Y);
					#endregion
				}
			}
		}

		public override void Update(DrawArgs drawArgs)
		{
			try
			{

				if(!drawArgs.WorldCamera.ViewFrustum.Intersects(boundingBox))
					return;

				if(!isLoaded)
					Load(drawArgs);
			
				if(linePoints != null)
					if((lastUpdatedPosition - drawArgs.WorldCamera.Position).LengthSq() < 10*10) // Update if camera moved more than 10 meters
						if(Math.Abs(this.verticalExaggeration - World.Settings.VerticalExaggeration) < 0.01)
							// Already loaded and up-to-date
							return;

				verticalExaggeration = World.Settings.VerticalExaggeration;

				ArrayList renderablePoints = new ArrayList();
				Vector3 lastPointProjected = Vector3.Empty;
				Vector3 currentPointProjected;
				Vector3 currentPointXyz = Vector3.Empty;
				for(int i = 0; i < sphericalCoordinates.Count; i++)
				{
					double altitude = 0;

					#region JHU Changes
					Vector3 curCoord = (Vector3) sphericalCoordinates[i];

					if(_parentWorld.TerrainAccessor != null && drawArgs.WorldCamera.Altitude < 300000)
						altitude = _terrainAccessor.GetElevationAt(
							curCoord.X, 
							curCoord.Y,
							(100.0 / drawArgs.WorldCamera.ViewRange.Degrees));

					currentPointXyz = MathEngine.SphericalToCartesian(
						curCoord.X, 
						curCoord.Y, 
						this._parentWorld.EquatorialRadius + this.heightAboveSurface + 
						this.verticalExaggeration * altitude );

					#endregion

					currentPointProjected = drawArgs.WorldCamera.Project(currentPointXyz);

					float dx = lastPointProjected.X - currentPointProjected.X;
					float dy = lastPointProjected.Y - currentPointProjected.Y;
					float distanceSquared = dx*dx + dy*dy;
					const float minimumPointSpacingSquaredPixels = 2*2;
					if(distanceSquared > minimumPointSpacingSquaredPixels)
					{
						renderablePoints.Add(currentPointXyz);
						lastPointProjected = currentPointProjected;
					}
				}

				// Add the last point if it's not already in there
				int pointCount = renderablePoints.Count;
				if(pointCount>0 && (Vector3)renderablePoints[pointCount-1] != currentPointXyz)
				{
					renderablePoints.Add(currentPointXyz);
					pointCount++;
				}

				CustomVertex.PositionColored[] newLinePoints = new CustomVertex.PositionColored[pointCount];
				for(int i = 0; i < pointCount; i++)
				{
					currentPointXyz = (Vector3)renderablePoints[i];
					newLinePoints[i].X = currentPointXyz.X;
					newLinePoints[i].Y = currentPointXyz.Y;
					newLinePoints[i].Z = currentPointXyz.Z;

					newLinePoints[i].Color = this.lineColor;
				}

				this.linePoints = newLinePoints;

				lastUpdatedPosition = drawArgs.WorldCamera.Position;
				System.Threading.Thread.Sleep(1);
			}
			catch(Exception caught)
			{
				Log.Write( caught );
				Debug.WriteLine(this.name + ": " + caught);
			}
		}

		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			return false;
		}

		#region JHU Changes

		public void AddPointToPath(float lat, float lon, float alt)
		{
			Vector3 newCoord = new Vector3(lat, lon, alt);

			this.sphericalCoordinates.Add(newCoord);

			if (this.north < newCoord.X)
				this.north = newCoord.X;
			if (this.east < newCoord.Y)
				this.east = newCoord.Y;
			if (this.south > newCoord.X)
				this.south = newCoord.X;
			if (this.west > newCoord.Y)
				this.west = newCoord.Y;

			this.boundingBox = new BoundingBox( this.south, this.north, this.west, this.east, 
				(float)this._parentWorld.EquatorialRadius,
				(float)(this._parentWorld.EquatorialRadius + this.verticalExaggeration * alt));

			lastUpdatedPosition.X = lat;
			this.isLoaded = true;
			this.isInitialized = false;
			this.m_needsUpdate = true;
		}

		public Vector3 GetPoint(int index)
		{
			if ((sphericalCoordinates != null) &&
				(index >= 0) &&
				(index < sphericalCoordinates.Count) )
				return (Vector3) this.sphericalCoordinates[index];
			else
				return new Vector3(0,0,0);

		}

		/// <summary>
		/// Fills the context menu with menu items specific to the layer.
		/// </summary>
		/// <param name="menu">Pre-initialized context menu.</param>
		public override void BuildContextMenu( ContextMenu menu )
		{
			// initialize context menu
			MenuItem topMenuItem = new MenuItem(Name);

			menu.MenuItems.Add("Properties", new System.EventHandler(OnPropertiesClick));
			menu.MenuItems.Add("Delete", new System.EventHandler(MyDeleteClick));
		}


		protected virtual void MyDeleteClick(object sender, System.EventArgs e)
		{
			if(ParentList == null)
			{
				MessageBox.Show("Unable to delete root layer list.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			string message = "Permanently Delete Object '" + name + "'?";
			if(DialogResult.Yes != MessageBox.Show( message, "Delete object", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
				MessageBoxDefaultButton.Button2 ))
				return;

			try
			{
				Delete();
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		public override void Delete()
		{
			// Remove from each parent of just this one?
			ParentList.ChildObjects.Remove(this);
			Dispose();
		}

		#endregion

		public override void Render(DrawArgs drawArgs)
		{
			try
			{
				if(!this.isLoaded)
					Load(drawArgs);

				if(drawArgs.WorldCamera.Altitude > _maxDisplayAltitude)
					return;
				if(drawArgs.WorldCamera.Altitude < _minDisplayAltitude)
					return;

				if (m_needsUpdate)
					this.Update(drawArgs);

				if(this.linePoints == null)
					return;

				if(!drawArgs.WorldCamera.ViewFrustum.Intersects(this.boundingBox))
					return;

				drawArgs.numBoundaryPointsRendered += this.linePoints.Length;
				drawArgs.numBoundaryPointsTotal += this.sphericalCoordinates.Count;
				drawArgs.numBoundariesDrawn++;

				drawArgs.device.VertexFormat = CustomVertex.PositionColored.Format;
				drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;
				drawArgs.device.DrawUserPrimitives( PrimitiveType.LineStrip, this.linePoints.Length - 1, this.linePoints );

			}
			catch(Exception caught)
			{
				Log.Write( caught );
				Debug.WriteLine(this.name + ": " + caught);
			}
		}
	}
}
