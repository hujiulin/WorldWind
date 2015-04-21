//----------------------------------------------------------------------------
// NAME: Planimetric Measure Tool
// VERSION: 1.0
// DESCRIPTION: Planimetric Measure Tool
// DEVELOPER: Bjorn Reppen aka "Mashi" - Refinements by Tisham Dhar aka "What_nick"
// WEBSITE: 
// REFERENCES: 
//----------------------------------------------------------------------------
//
// 
//
using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Xml.Serialization;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Windows.Forms;
using WorldWind;
using WorldWind.Renderable;
using WorldWind.Net;
using System.Xml;

//Shape support
using MapTools;

namespace MeasureTool.Plugins
{
	/// <summary>
	/// Planimetric Measure Tool plug-in
	/// </summary>
	public class MeasureTool : WorldWind.PluginEngine.Plugin
	{
		protected MenuItem menuItem;
		MeasureToolLayer layer;

        #region Accessor Methods
        public MenuItem MenuEntry
        {
            get
            {
                return menuItem;
            }
        }
        #endregion

        /// <summary>
		/// Plugin entry point 
		/// </summary>
		public override void Load() 
		{
			layer = new MeasureToolLayer(
				this,
				ParentApplication.WorldWindow.DrawArgs );

			layer.TexturePath = Path.Combine(PluginDirectory,"Plugins\\Measure");
			ParentApplication.WorldWindow.CurrentWorld.RenderableObjects.Add(layer);

			menuItem = new MenuItem("Measure\tM");
			menuItem.Click += new EventHandler(menuItemClicked);
			ParentApplication.ToolsMenu.MenuItems.Add( menuItem );

			// Subscribe events
			ParentApplication.WorldWindow.MouseMove += new MouseEventHandler(layer.MouseMove);
			ParentApplication.WorldWindow.MouseDown += new MouseEventHandler(layer.MouseDown);
			ParentApplication.WorldWindow.MouseUp += new MouseEventHandler(layer.MouseUp);
			ParentApplication.WorldWindow.KeyUp +=new KeyEventHandler(layer.KeyUp);
		}

		/// <summary>
		/// Unload our plugin
		/// </summary>
		public override void Unload() 
		{
			if(menuItem!=null)
			{
				ParentApplication.ToolsMenu.MenuItems.Remove( menuItem );
				menuItem.Dispose();
				menuItem = null;
			}

			ParentApplication.WorldWindow.MouseMove -= new MouseEventHandler(layer.MouseMove);
			ParentApplication.WorldWindow.MouseDown -= new MouseEventHandler(layer.MouseDown);
			ParentApplication.WorldWindow.MouseUp -= new MouseEventHandler(layer.MouseUp);
			ParentApplication.WorldWindow.KeyUp -= new KeyEventHandler(layer.KeyUp);
			
			ParentApplication.WorldWindow.CurrentWorld.RenderableObjects.Remove(layer);
		}
	
		void menuItemClicked(object sender, EventArgs e)
		{
			layer.IsOn = !layer.IsOn;
			menuItem.Checked = layer.IsOn;
		}
	}

	public class MeasureToolLayer : WorldWind.Renderable.RenderableObject
	{
		public enum MeasureState
		{
			Idle,
			Measuring,
			Complete
		}

		#region Public data

		/// <summary>
		/// Tool texture path
		/// </summary>
		public string TexturePath;

		/// <summary>
		/// Latitude of measure start position
		/// </summary>
		public Angle StartLatitude;

		/// <summary>
		/// Longitude of measure start position
		/// </summary>
		public Angle StartLongitude;

		/// <summary>
		/// Latitude of measure end position
		/// </summary>
		public Angle EndLatitude;

		/// <summary>
		/// Longitude of measure end position
		/// </summary>
		public Angle EndLongitude;

		/// <summary>
		/// Heading to go from start to end position. (0 = north)
		/// </summary>
		public Angle Azimuth;

		/// <summary>
		/// Distance (meters) from start to end position
		/// </summary>
		public double Distance;

		/// <summary>
		/// Current state of measuring operation
		/// </summary>
		public MeasureState State;

		/// <summary>
		/// Multi line object; contains series of points.
		/// </summary>
		
		#endregion

		#region Private data
		DrawArgs m_drawArgs;
		SaveMultiLine save;
		MeasureMultiLine multiline = new MeasureMultiLine();

		string labelText;
		Rectangle labelTextRect;

		CustomVertex.PositionColored[] measureLine = new CustomVertex.PositionColored[17];
		CustomVertex.PositionColored[] startPoint = new CustomVertex.PositionColored[2];
		CustomVertex.PositionColored[] endPoint = new CustomVertex.PositionColored[4];

		CustomVertex.TransformedColoredTextured[] rect = new CustomVertex.TransformedColoredTextured[5];
		CustomVertex.TransformedColored[] rectFrame = new CustomVertex.TransformedColored[5];
		CustomVertex.TransformedColored[] rectLineConnection = new CustomVertex.TransformedColored[3];
		bool isPointGotoEnabled;
		Point mouseDownPoint;
		private Texture m_texture;

		/*properties dialog*/
		MeasurePropertiesDialog propertiesDialog;
        private MeasureTool m_srcplugin;
	    //StreamWriter saveFile = new StreamWriter("multipath.csv");

		#endregion


		public MeasureToolLayer(MeasureTool srcPlugin, DrawArgs drawArgs) : base("Measure Tool")
		{
            
		    RenderPriority = RenderPriority.Placenames;
			isOn = false;

            m_srcplugin = srcPlugin;
			m_drawArgs = drawArgs;
            m_world = srcPlugin.ParentApplication.WorldWindow.CurrentWorld;
		    
			// Initialize colors
			for(int i=0;i<measureLine.Length;i++)
				measureLine[i].Color = World.Settings.MeasureLineLinearColorXml;
			for(int i=0;i<rectLineConnection.Length;i++)
				rectLineConnection[i].Color = unchecked((int)0xff808080L);
			for(int i=0;i<rect.Length;i++)
				rect[i].Color = Color.Gray.ToArgb();
				//rect[i].Color = World.Settings.MeasureLineLinearColorXml;
			for(int i=0;i<rectFrame.Length;i++)
				rectFrame[i].Color = unchecked((int)0xff808080L);
			for(int i=0;i<startPoint.Length;i++)
				startPoint[i].Color = World.Settings.MeasureLineLinearColorXml;
			for(int i=0;i<endPoint.Length;i++)
				endPoint[i].Color = World.Settings.MeasureLineLinearColorXml;		

			rect[1].Tv = 1;
			rect[2].Tu = 1;
			rect[3].Tu = 1;
			rect[3].Tv = 1;
		}

		public void MouseDown( object sender, MouseEventArgs e )
		{
			if(!isOn)
				return;
			mouseDownPoint = DrawArgs.LastMousePosition;
		}

		public void MouseUp(object sender, MouseEventArgs e )
		{
			if(!isOn)
				return;
			
			// Test if mouse was clicked and dragged
			if (mouseDragged())
				return;

			// Check if dialog box has been instantiated then get the measure mod
			// if not, automatically assign the mode to be single

			if (World.Settings.MeasureMode == MeasureMode.Multi && multiline == null)
				multiline = new MeasureMultiLine();

			// Cancel selection if right mouse button clicked
			if (e.Button == MouseButtons.Right)
			{
				if(State != MeasureState.Idle && World.Settings.MeasureMode == MeasureMode.Multi)
					MouseRightClick(sender,e);
				else
				{
					multiline.Clear();
					IsOn = false;
				}
				return;
			}

			// Do nothing for all other mouse buttons clicked
			if (e.Button != MouseButtons.Left)
				return;
			
			// Don't know if this is best way to do things...
			if (World.Settings.MeasureMode == MeasureMode.Single)
			{
				switch(State)
				{
					case MeasureState.Idle:
						State = MeasureState.Measuring;
						break;
					case MeasureState.Measuring:
						State = MeasureState.Complete;
						return;
					case MeasureState.Complete:
					{
						multiline.Clear();
						State = MeasureState.Idle;
						return;
					}
				}
			} 
			else if (World.Settings.MeasureMode == MeasureMode.Multi) 
			{
				switch(State)
				{
					case MeasureState.Idle:
						State = MeasureState.Measuring;
						break;
					case MeasureState.Measuring:
					{
						State = MeasureState.Measuring;
						if(multiline.Count>0)
							this.multiline.deleteLine();
						MeasureLine line = new MeasureLine(multiline.Count);
						line.StartLatitude = this.StartLatitude;
						line.EndLatitude = this.EndLatitude;
						line.StartLongitude = this.StartLongitude;
						line.EndLongitude = this.EndLongitude;
						line.Calculate(this.m_world,false);
						this.multiline.addLine(line);
						break;
					}
					case MeasureState.Complete:
					{
						State = MeasureState.Idle;
						return;
					}
				}
			}

			m_drawArgs.WorldCamera.PickingRayIntersection(
				e.X,
				e.Y,
				out StartLatitude,
				out StartLongitude);

			EndLatitude = StartLatitude;
			EndLongitude = StartLongitude;

			measureLine[0].X = (MathEngine.SphericalToCartesian(StartLatitude, 
				StartLongitude, m_drawArgs.WorldCamera.WorldRadius)).X;
			measureLine[0].Y = (MathEngine.SphericalToCartesian(StartLatitude, 
				StartLongitude, m_drawArgs.WorldCamera.WorldRadius)).Y;
			measureLine[0].Z = (MathEngine.SphericalToCartesian(StartLatitude, 
				StartLongitude, m_drawArgs.WorldCamera.WorldRadius)).Z;

			MeasureLine newline = new MeasureLine(multiline.Count);
			newline.StartLatitude = this.StartLatitude;
			newline.EndLatitude = this.EndLatitude;
			newline.StartLongitude = this.StartLongitude;
			newline.EndLongitude = this.EndLongitude;
			newline.Calculate(this.m_world,false);
			this.multiline.addLine(newline);
		}

		// Double click event
		public void MouseRightClick(object sender, MouseEventArgs e)
		{
			switch(State) 
			{
				case MeasureState.Measuring:
					State = MeasureState.Complete;
					return;
				case MeasureState.Complete:
					multiline.Clear();
					State = MeasureState.Idle;
					return;
			}
		}

		public void MouseMove(object sender, MouseEventArgs e)
		{
			if(!isOn)
				return;

			if(State!=MeasureState.Measuring)
				return;

			Angle lat;
			Angle lon;
			m_drawArgs.WorldCamera.PickingRayIntersection(
				e.X,
				e.Y,
				out lat,
				out lon);

			if(Angle.IsNaN(lat))
				return;

			EndLongitude = lon;
			EndLatitude = lat;

			// Calculate distance (meters) and heading between start and current mouse position
			Angle angularDistance = World.ApproxAngularDistance(StartLatitude, StartLongitude, EndLatitude, EndLongitude);
			Distance = angularDistance.Radians * m_world.EquatorialRadius;
			Azimuth = MathEngine.Azimuth(StartLatitude, StartLongitude, EndLatitude, EndLongitude);

			BuildMeasureLine(angularDistance);
			
			if(multiline.Count>0)
			this.multiline.deleteLine();
			MeasureLine line = new MeasureLine(multiline.Count);
			line.StartLatitude = this.StartLatitude;
			line.EndLatitude = this.EndLatitude;
			line.StartLongitude = this.StartLongitude;
			line.EndLongitude = this.EndLongitude;
			line.Calculate(this.m_world,false);
			this.multiline.addLine(line);
		}

		public void KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.M)
			{
				IsOn = !IsOn;
				e.Handled = true;
			    //TODO: Turn menu item off
                if (this.m_srcplugin.MenuEntry!=null)
                    this.m_srcplugin.MenuEntry.Checked = !this.m_srcplugin.MenuEntry.Checked;
			}
			if (e.KeyData == Keys.X)
			{
				if(IsOn)
					saveLine(null,null);
			}
		}

		private bool mouseDragged() 
		{
			int dx = DrawArgs.LastMousePosition.X - mouseDownPoint.X;
			int dy = DrawArgs.LastMousePosition.Y - mouseDownPoint.Y;
			if(dx*dx+dy*dy > 3*3)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Fills the context menu with menu items specific to the layer.
		/// </summary>
		public override void BuildContextMenu(ContextMenu menu)
		{
			menu.MenuItems.Add("Properties", new System.EventHandler(OnPropertiesClick));
			menu.MenuItems.Add("Save Multi-Point Line", new System.EventHandler(saveLine));
		}

		/// <summary>
		/// Properties context menu clicked.
		/// </summary>
		protected override void OnPropertiesClick(object sender, EventArgs e)
		{
			if(propertiesDialog != null && ! propertiesDialog.IsDisposed)
				// Already open
				return;

			// Display the dialog
			propertiesDialog = new MeasurePropertiesDialog();
			propertiesDialog.Show();
		}
		
		public override bool IsOn
		{
			get
			{
				return base.IsOn;
			}
			set
			{
				if(value==isOn)
					return;

				base.IsOn = value;
				if(isOn)
				{
					// Can't use point goto while measuring
					isPointGotoEnabled = World.Settings.CameraIsPointGoto;
					World.Settings.CameraIsPointGoto = false;
					State = MeasureState.Idle;
				}
				else
				{
					World.Settings.CameraIsPointGoto = isPointGotoEnabled;
				}
			}
		}

		public override void Render(DrawArgs drawArgs)
		{
			if(!isOn)
				return;

			// Turn off light
			if (World.Settings.EnableSunShading) drawArgs.device.RenderState.Lighting = false;

			// Check that textures are initialised
			if(!isInitialized)
				Initialize(drawArgs);

			if(DrawArgs.MouseCursor == CursorType.Arrow)
				// Use our cursor when the mouse isn't over other elements requiring different cursor
				DrawArgs.MouseCursor = CursorType.Measure;

			if(State == MeasureState.Idle)
				return;

			if (!CalculateRectPlacement(drawArgs))
				return;

			if(Distance < 0.01)
				return;

			Device device = drawArgs.device;
			device.RenderState.ZBufferEnable = false;
			device.TextureState[0].ColorOperation = TextureOperation.Disable;
			device.VertexFormat = CustomVertex.PositionColored.Format;

			
			// Draw the measure line + ends
			/*
			device.DrawUserPrimitives(PrimitiveType.LineStrip, measureLine.Length-1, measureLine);
			device.DrawUserPrimitives(PrimitiveType.LineStrip, startPoint.Length-1, startPoint);
			device.DrawUserPrimitives(PrimitiveType.LineList, endPoint.Length>>1, endPoint);
			*/

			multiline.Render(drawArgs);


			// Draw the info rect
			device.TextureState[0].ColorOperation = TextureOperation.SelectArg1;
			device.SetTexture(0,m_texture);
			device.VertexFormat = CustomVertex.TransformedColoredTextured.Format;
			device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, rect);

			device.TextureState[0].ColorOperation = TextureOperation.Disable;

			device.DrawUserPrimitives(PrimitiveType.LineStrip, 2, rectLineConnection);
			device.DrawUserPrimitives(PrimitiveType.LineStrip, rectFrame.Length-1, rectFrame);
			drawArgs.defaultDrawingFont.DrawText(null, labelText, labelTextRect, DrawTextFormat.None, 0xff << 24);

			device.RenderState.ZBufferEnable = true;
			if (World.Settings.EnableSunShading) drawArgs.device.RenderState.Lighting = true;

		}

		bool CalculateRectPlacement(DrawArgs drawArgs)
		{
			int labelLinePoint = FindAnchorPoint();
			if(labelLinePoint < 0)
			{
				// Measure line is not visible
				return false;
			}

			Vector3 referenceCenter = new Vector3(
				(float)drawArgs.WorldCamera.ReferenceCenter.X,
				(float)drawArgs.WorldCamera.ReferenceCenter.Y,
				(float)drawArgs.WorldCamera.ReferenceCenter.Z
				);


			Angle displayAngle = CalcAngle(labelLinePoint, referenceCenter);
			if( Angle.IsNaN(displayAngle) )
				return false;

			const int leg1Len = 30;
			const int leg2Len = 5;

			
			Vector3 screenAnchor = m_drawArgs.WorldCamera.Project(
				new Vector3( 
				measureLine[labelLinePoint].X,
				measureLine[labelLinePoint].Y,
				measureLine[labelLinePoint].Z ) - referenceCenter);

			float x1 = (float)(screenAnchor.X + Math.Cos(displayAngle.Radians)*leg1Len);
			float y1 = (float)(screenAnchor.Y + Math.Sin(displayAngle.Radians)*leg1Len);
			float x2 = x1;
			float y2 = y1;


			// Find direction of 2nd leg.
			int quadrant = (int)((displayAngle.Radians)/(Math.PI/2));
			switch (quadrant % 4)
			{
				case 0:
				case 3:
					x2 += leg2Len;
					break;
				case 1:
				case 2:
					x2 -= leg2Len;
					break;
			}

			// Calculate label box position / size
			if (World.Settings.MeasureMode == MeasureMode.Multi)
			{
				Distance = multiline.getLength();
				//labelText = Distance>=10000 ?
				//	string.Format( "Total Distance: {0:f1}km", Distance/1000 ) :
				//	string.Format( "Total Distance: {0:f1}m", Distance );
				labelText = "Total Distance: " + ConvertUnits.GetDisplayString(Distance);
			}
			else
			{
				//labelText = Distance>=10000 ?
				//	string.Format( "Distance: {0:f1}km", Distance/1000 ) :
				//	string.Format( "Distance: {0:f1}m", Distance );
				labelText = "Distance: " + ConvertUnits.GetDisplayString(Distance);

			}
			labelText += string.Format("\nBearing: {0:f1}°", Azimuth.Degrees );

			labelTextRect = m_drawArgs.defaultDrawingFont.MeasureString(null, labelText, DrawTextFormat.None, 0);
			
			Rectangle tsize = labelTextRect;
			const int xPad = 4;
			const int yPad = 1;
			tsize.Inflate( xPad, yPad );
			labelTextRect.Offset(-tsize.Left, -tsize.Top);
			tsize.Offset(-tsize.Left, -tsize.Top);
			
			rectLineConnection[0].X = screenAnchor.X;
			rectLineConnection[0].Y = screenAnchor.Y;
			rectLineConnection[1].X = x1;
			rectLineConnection[1].Y = y1;
			rectLineConnection[2].X = x2;
			rectLineConnection[2].Y = y2;
			if(x2>x1)
			{
				labelTextRect.Offset((int)x2, 0);
				tsize.Offset((int)x2, 0);
			}
			else
			{
				int xof = (int)(x2-tsize.Width);
				labelTextRect.Offset(xof, 0);
				tsize.Offset(xof, 0);
			}
			tsize.Offset(0, (int)(y2 - tsize.Height/2));
			labelTextRect.Offset(0, (int)(y2 - tsize.Height/2));

			rect[0].X = tsize.Left;
			rect[0].Y = tsize.Top;
			rect[1].X = rect[0].X;
			rect[1].Y = tsize.Bottom;
			rect[2].X = tsize.Right;
			rect[2].Y = rect[0].Y;
			rect[3].X = rect[2].X;
			rect[3].Y = rect[1].Y;
			rect[4].X = rect[0].X;
			rect[4].Y = rect[1].Y;

			rectFrame[0].X = tsize.Left;
			rectFrame[0].Y = tsize.Top;
			rectFrame[1].X = rectFrame[0].X;
			rectFrame[1].Y = tsize.Bottom;
			rectFrame[2].X = tsize.Right;
			rectFrame[2].Y = rectFrame[1].Y;
			rectFrame[3].X = rectFrame[2].X;
			rectFrame[3].Y = rectFrame[0].Y;
			rectFrame[4].X = rectFrame[0].X;
			rectFrame[4].Y = rectFrame[0].Y;


			// Cap at start of measure
			Vector3 a = new Vector3(measureLine[0].X, measureLine[0].Y, measureLine[0].Z );
			Vector3 b = new Vector3(measureLine[1].X, measureLine[1].Y, measureLine[1].Z );
			Vector3 vCap  = Vector3.Cross(a,b);
			vCap.Normalize();
			const int lineCapSize = 6;
			vCap.Scale( (float)m_drawArgs.WorldCamera.Distance/750f*lineCapSize );

			Vector3 worldXyzStart = new Vector3( measureLine[0].X, measureLine[0].Y, measureLine[0].Z );
			Vector3 va = Vector3.Add( worldXyzStart, vCap );
			Vector3 vb = Vector3.Add( worldXyzStart, -vCap );

			startPoint[0].X = va.X;
			startPoint[0].Y = va.Y;
			startPoint[0].Z = va.Z;
			startPoint[1].X = vb.X;
			startPoint[1].Y = vb.Y;
			startPoint[1].Z = vb.Z;

			// Cap at end of measure
			int last = measureLine.Length-1;
			Vector3 worldXyzEnd = new Vector3( 
				measureLine[last].X,
				measureLine[last].Y,
				measureLine[last].Z );

			int beforeLast = last-1;
			vCap = new Vector3( 
				measureLine[beforeLast].X,
				measureLine[beforeLast].Y,
				measureLine[beforeLast].Z );
			vCap.Subtract(worldXyzEnd);
			vCap.Normalize();
			vCap.Scale( (float)(m_drawArgs.WorldCamera.Distance/750f*lineCapSize) );

			vb = va = Vector3.Add( worldXyzEnd , vCap );
			const float arrowHeadAngle = 0.25f*(float)Math.PI;
			va.TransformCoordinate( Matrix.RotationAxis( worldXyzEnd, (float)Math.PI+arrowHeadAngle ));
			vb.TransformCoordinate( Matrix.RotationAxis( worldXyzEnd, arrowHeadAngle));

			endPoint[0].X = va.X;
			endPoint[0].Y = va.Y;
			endPoint[0].Z = va.Z;
			endPoint[1].X = vb.X;
			endPoint[1].Y = vb.Y;
			endPoint[1].Z = vb.Z;

			Matrix rotate90 = Matrix.RotationAxis( worldXyzEnd, (float)Math.PI*0.5f );
			va.TransformCoordinate( rotate90 );
			vb.TransformCoordinate( rotate90 );

			endPoint[2].X = va.X;
			endPoint[2].Y = va.Y;
			endPoint[2].Z = va.Z;
			endPoint[3].X = vb.X;
			endPoint[3].Y = vb.Y;
			endPoint[3].Z = vb.Z;

			return true;
		}

		/// <summary>
		/// RenderableObject abstract member (needed) 
		/// OBS: Worker thread (don't update UI directly from this thread)
		/// </summary>
		public override void Initialize(DrawArgs drawArgs)
		{
			isInitialized = true;
			Console.WriteLine(TexturePath);
			if(m_texture==null)
				updateTextures(null,null);
		}

		/// <summary>
		/// RenderableObject abstract member (needed)
		/// OBS: Worker thread (don't update UI directly from this thread)
		/// </summary>
		public override void Update(DrawArgs drawArgs)
		{
			if(!isInitialized)
				Initialize(drawArgs);
		}

		/// <summary>
		/// RenderableObject abstract member (needed)
		/// OBS: Worker thread (don't update UI directly from this thread)
		/// </summary>
		public override void Dispose()
		{
			isInitialized = false;
			if(m_texture!=null)
			{
				m_texture.Dispose();
				m_texture = null;
			}
			if(m_texture!=null)
			{
				propertiesDialog.Dispose();
				propertiesDialog = null;
			}
			if(save!=null)
			{
				save.Dispose();
				save = null;
			}
		}

		/// <summary>
		/// RenderableObject abstract member (needed)
		/// Called from UI thread = UI code safe in this function
		/// </summary>
		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			return false;
		}
		

		/// <summary>
		/// Calculates the segments of the measure curve
		/// </summary>
		void BuildMeasureLine(Angle angularDistance)
		{
			for(int i=0; i<measureLine.Length; i++)
			{
				float t = (float)i / (measureLine.Length-1);
				Vector3 cart = m_world.IntermediateGCPoint(t, StartLatitude, StartLongitude, EndLatitude, EndLongitude,
					angularDistance );

				measureLine[i].X = cart.X;
				measureLine[i].Y = cart.Y;
				measureLine[i].Z = cart.Z;
			}
		}
		
		
		/// <summary>
		/// Checks if a point on the measure line is visible
		/// </summary>
		bool IsMeasureLinePointVisible(int linePoint)
		{
			Vector3 v = new Vector3( measureLine[linePoint].X, measureLine[linePoint].Y, measureLine[linePoint].Z );
			return m_drawArgs.WorldCamera.ViewFrustum.ContainsPoint(v);
		}

		/// <summary>
		/// Find the best visible point to "attach" the labelText to.
		/// </summary>
		/// <returns></returns>
		int FindAnchorPoint()
		{
			int mid = measureLine.Length >> 1;
			if(IsMeasureLinePointVisible(mid))
				return mid;
			if(IsMeasureLinePointVisible(measureLine.Length-2))
				return measureLine.Length-2;
			if(IsMeasureLinePointVisible(1))
				return 1;
			for (int i=mid; i>1; i--)
			{
				if(IsMeasureLinePointVisible(mid+i))
					return mid+i;
				if(IsMeasureLinePointVisible(mid-i))
					return mid-i;
			}

			return -1;
		}

		/// <summary>
		/// Calculates the average angle in screen coordinates of the line segment before and after
		/// linePointNumber.
		/// </summary>
		/// <param name="linePointNumber">Index into the measureLine array.</param>
		/// <returns></returns>
		Angle CalcAngle( int linePointNumber , Vector3 referenceCenter)
		{
			int endSeg = linePointNumber+1;
			int begSeg = linePointNumber-1;
			if(endSeg>=measureLine.Length)
				endSeg--;
			if(begSeg<0)
				begSeg = 0;

			Vector3 a = m_drawArgs.WorldCamera.Project( 
				new Vector3( measureLine[begSeg].X, measureLine[begSeg].Y, measureLine[begSeg].Z ) - referenceCenter);
			Vector3 b = m_drawArgs.WorldCamera.Project( 
				new Vector3( measureLine[endSeg].X, measureLine[endSeg].Y, measureLine[endSeg].Z ) - referenceCenter );

			Vector3 c = b - a;
			Angle displayAngle = Angle.FromRadians( Math.Atan(-c.X/c.Y) );
			if(c.Y>0)
				displayAngle.Radians += Math.PI;
			if(displayAngle.Radians<0)
				displayAngle.Radians += 2*Math.PI;
			return displayAngle;
		}

		/// <summary>
		/// Loads or downloads the bitmaps
		/// </summary>
		public void LoadTextures(string path)
		{
			Console.WriteLine(path);
			if(File.Exists(path))
			{
				try
				{
					m_texture = ImageHelper.LoadTexture(path);
				}
				catch { /* Ignore */ }
				return;

			}
		}
		
		protected void updateTextures(object sender, EventArgs e) 
		{
				LoadTextures(Path.Combine(TexturePath,"rect.jpg"));
		}


		/// <summary>
		/// Method to save a multi-point line. Will open up a dialog box.
		/// </summary>
		/// <param name="multiLine">The multi-point line to be saved.</param>
		public void saveLine(object sender, EventArgs e) 
		{
			// Boo.
			//Console.WriteLine("We got here... yes, yay to you too.");

			if (World.Settings.MeasureMode == MeasureMode.Multi) 
			{
				//Not open
				if(save == null)
					save = new SaveMultiLine(this);
				save.Show();
			}
			else
			{
				MessageBox.Show("Only Polylines can be saved",
				"No Line Save",
				MessageBoxButtons.OK,
				MessageBoxIcon.Exclamation);
			}
		}
		
		#region measurement objects
		internal class MeasureMultiLine:ArrayList
		{
			
			public void addLine(MeasureLine line)
			{
				Add(line);
			}

			public void deleteLine()
			{
				RemoveAt(Count-1);
			}

			public double getLength() 
			{
				double sum = 0.0;
				foreach(MeasureLine line in this)
					sum += line.Linear;
				return sum;
			}
			public void Render(DrawArgs drawArgs)
			{
				foreach(MeasureLine line in this)
				{
					try
					{
						line.Render(drawArgs);
					}
					catch
					{}
				}
			}
		}

		//TODO: Use Pathline rendering code and Only include measurement functions here
		/// <summary>
		/// MeasureLine class: line object.
		/// </summary>
		internal class MeasureLine : ListViewItem
		{
			public Vector3 WorldXyzStart;
			public Vector3 WorldXyzMid;

			private double linearDistance;
			private double groundTrack;
			public bool IsGroundTrackValid;
			public CustomVertex.PositionColored[] GroundTrackLine;
			public CustomVertex.PositionColored[] LinearTrackLine;
			static CustomVertex.TransformedColored[] circle = new CustomVertex.TransformedColored[8];
			private Angle startLatitude;
			private Angle startLongitude;
			private Angle endLatitude;
			private Angle endLongitude;
			private static string Units = "km";

			/// <summary>
			/// Constructor
			/// </summary>
			public MeasureLine(int segmentNumber)
			{
				SubItems.AddRange( new string[]{"","","", Units} );
				Text = "S"+segmentNumber.ToString();
			}

			/// <summary>
			/// Segment starting latitude
			/// </summary>
			public Angle StartLatitude
			{
				get { return startLatitude; }
				set 
				{
					startLatitude = value;
					UpdateCoordinates();
				}
			}

			/// <summary>
			/// Segment starting longitude
			/// </summary>
			public Angle StartLongitude
			{
				get { return startLongitude; }
				set 
				{
					startLongitude = value;
					UpdateCoordinates();
				}
			}

			/// <summary>
			/// Segment ending latitude
			/// </summary>
			public Angle EndLatitude
			{
				get { return endLatitude; }
				set 
				{
					endLatitude = value;
					UpdateCoordinates();
				}
			}

			/// <summary>
			/// Segment ending longitude
			/// </summary>
			public Angle EndLongitude
			{
				get { return endLongitude; }
				set 
				{
					endLongitude = value;
					UpdateCoordinates();
				}
			}

			/// <summary>
			/// Measured linearDistance distance (as the crow flies)
			/// </summary>
			public double Linear
			{
				get
				{
					return linearDistance;
				}
				set
				{
					linearDistance = value;
					// TODO: Units
					SubItems[2].Text = FormatDistance(value);
				}
			}

			/// <summary>
			/// Measured ground track length
			/// </summary>
			public double GroundTrack
			{
				get
				{
					return groundTrack;
				}
				set
				{
					groundTrack = value;
					// TODO: Units
					SubItems[3].Text = FormatDistance(value);
				}
			}

			/// <summary>
			/// Render the line segment
			/// </summary>
			public void Render(DrawArgs drawArgs)
			{
				// Draw the measure line + ends
				Vector3 referenceCenter = new Vector3(
					(float)drawArgs.WorldCamera.ReferenceCenter.X,
					(float)drawArgs.WorldCamera.ReferenceCenter.Y,
					(float)drawArgs.WorldCamera.ReferenceCenter.Z);

				drawArgs.device.Transform.World = Matrix.Translation(
					-referenceCenter
					);

				if(World.Settings.MeasureShowGroundTrack && IsGroundTrackValid)
					drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, GroundTrackLine.Length-1, GroundTrackLine);
				drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, LinearTrackLine.Length-1, LinearTrackLine);

				drawArgs.device.Transform.World = drawArgs.WorldCamera.WorldMatrix;

				if(!drawArgs.WorldCamera.ViewFrustum.ContainsPoint(WorldXyzMid))
					// Label is invisible
					return;
				Vector3 labelXy = drawArgs.WorldCamera.Project(WorldXyzMid - referenceCenter);
				string label ="";//= Text;
				if( groundTrack>0)
					label +=  FormatDistance(groundTrack) + Units;
				else
					label +=  FormatDistance(linearDistance) + Units;
				drawArgs.defaultDrawingFont.DrawText(null, label, (int)labelXy.X, (int)labelXy.Y, World.Settings.MeasureLineLinearColor );
			}

			public void RenderWaypointIcon(DrawArgs drawArgs, Vector3 position)
			{
				if(!drawArgs.WorldCamera.ViewFrustum.ContainsPoint(position))
					return;
				// Draw the circle - TODO: if the circle doesn't have to always face the user it can be pre-calculated
				Vector3 referenceCenter = new Vector3(
					(float)-drawArgs.WorldCamera.ReferenceCenter.X,
					(float)-drawArgs.WorldCamera.ReferenceCenter.Y,
					(float)-drawArgs.WorldCamera.ReferenceCenter.Z);
				Vector3 startXy = drawArgs.WorldCamera.Project(position - referenceCenter);
				float circleRadius = 4;
				for(int i=0;i<circle.Length;i++)
				{
					float angle = (float)(i*2*Math.PI/(circle.Length-1));
					circle[i].X = (float)(startXy.X + Math.Sin(angle)*circleRadius);
					circle[i].Y = (float)(startXy.Y + Math.Cos(angle)*circleRadius);
					circle[i].Color = World.Settings.MeasureLineLinearColorXml;;
				}
				drawArgs.device.VertexFormat = CustomVertex.TransformedColored.Format;
				drawArgs.device.Transform.World = Matrix.Translation(
					referenceCenter
					);

				drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, circle.Length-1, circle);
				drawArgs.device.Transform.World = drawArgs.WorldCamera.WorldMatrix;
				drawArgs.device.VertexFormat = CustomVertex.PositionColored.Format;
			}

			/// <summary>
			/// Calculates the segments of the measure curve
			/// </summary>
			public void Calculate(World world, bool useTerrain)
			{
				Angle angularDistance = World.ApproxAngularDistance( startLatitude, startLongitude, endLatitude, endLongitude );
				Linear = angularDistance.Radians * world.EquatorialRadius;
			
				int samples = (int)(angularDistance.Radians*30);  // 1 point for every 2 degrees.
				if(samples<2)
					samples = 2;

				LinearTrackLine = new CustomVertex.PositionColored[samples];
				for(int i=0;i<LinearTrackLine.Length;i++)
					LinearTrackLine[i].Color = World.Settings.MeasureLineLinearColorXml;;
			
				Angle lat,lon=Angle.Zero;
				for(int i=0; i<samples; i++)
				{
					float t = (float)i / (samples-1);
					World.IntermediateGCPoint(t, startLatitude, startLongitude, endLatitude, endLongitude,
						angularDistance, out lat, out lon );
				
					double elevation = 0;
					if(useTerrain)
						elevation = world.TerrainAccessor.GetElevationAt(lat.Degrees,lon.Degrees,1024);

					Vector3 subSegmentXyz = MathEngine.SphericalToCartesian(lat, lon, 
						world.EquatorialRadius + elevation * World.Settings.VerticalExaggeration );
					LinearTrackLine[i].X = subSegmentXyz.X;
					LinearTrackLine[i].Y = subSegmentXyz.Y;
					LinearTrackLine[i].Z = subSegmentXyz.Z;
				}

				WorldXyzMid = world.IntermediateGCPoint(0.5f, startLatitude, startLongitude, endLatitude, endLongitude,
					angularDistance );
			}

			/// <summary>
			/// Calculate this segment's ground track distance (meters)
			/// </summary>
			public void CalculateElevatedPath(World world)
			{
				// Calculate linear path over terrain
				Calculate(world,true);

				Angle angularDistance = World.ApproxAngularDistance( startLatitude, startLongitude, endLatitude, endLongitude );
				int samples = 1000; // TODO: calculate how many samples based on some criteria
				if(samples<2)
					samples = 2;

				float stepSize = (float)1 / (samples-1);

				// Build flat Line
				GroundTrackLine = new CustomVertex.PositionColored[samples];
				for(int i=0; i<samples; i++)
				{
					Angle lat,lon=Angle.Zero;
					World.IntermediateGCPoint(i*stepSize, startLatitude, startLongitude, endLatitude, endLongitude,
						angularDistance, out lat, out lon );
					Vector3 subSegmentXyz = MathEngine.SphericalToCartesian(lat, lon, world.EquatorialRadius );
					GroundTrackLine[i].X = subSegmentXyz.X;
					GroundTrackLine[i].Y = subSegmentXyz.Y;
					GroundTrackLine[i].Z = subSegmentXyz.Z;
					GroundTrackLine[i].Color = World.Settings.MeasureLineLinearColorXml;;
				}

				Vector3 last = Vector3.Empty;
				double trackLength = 0;

				// Build 3D Line and calculate distance
				for(int i=0; i<samples; i++)
				{
					Angle lat,lon=Angle.Zero;
					World.IntermediateGCPoint(i*stepSize, startLatitude, startLongitude, endLatitude, endLongitude,
						angularDistance, out lat, out lon );
					double elevation = world.TerrainAccessor.GetElevationAt(lat.Degrees,lon.Degrees,1024);

					Vector3 subSegmentXyz = MathEngine.SphericalToCartesian(lat, lon, 
						world.EquatorialRadius + elevation * World.Settings.VerticalExaggeration );
					GroundTrackLine[i].X = subSegmentXyz.X;
					GroundTrackLine[i].Y = subSegmentXyz.Y;
					GroundTrackLine[i].Z = subSegmentXyz.Z;
					GroundTrackLine[i].Color = World.Settings.MeasureLineLinearColorXml;;

					// Calculate length
					Vector3 current = MathEngine.SphericalToCartesian(lat, lon, world.EquatorialRadius + elevation);
					if(i==0)
						WorldXyzStart = subSegmentXyz;
					else
					{
						double dx = current.X - last.X;
						double dy = current.Y - last.Y;
						double dz = current.Z - last.Z;
						double d = Math.Sqrt(dx*dx+dy*dy+dz*dz);
						trackLength += d;				
					}
					last = current;
				}
				groundTrack = trackLength;

				// calculate Mid point (for label)
				Angle midLat, midLon=Angle.Zero;
				World.IntermediateGCPoint(0.5f, startLatitude, startLongitude, endLatitude, endLongitude,
					angularDistance, out midLat, out midLon );
				double midElevation = world.TerrainAccessor.GetElevationAt(midLat.Degrees,midLon.Degrees );

				WorldXyzMid = MathEngine.SphericalToCartesian(midLat, midLon, 
					world.EquatorialRadius + midElevation * World.Settings.VerticalExaggeration );

				IsGroundTrackValid = true;
			}

			/// <summary>
			/// Update the lat/lon column
			/// </summary>
			public void UpdateCoordinates()
			{
				IsGroundTrackValid = false; // Ground track needs recalculation
				string posString = string.Format("P1: [{0:f5}, {1:f5}] P2: [{2:f5}, {3:f5}]",
					startLatitude, startLongitude, 
					endLatitude, endLongitude );
				SubItems[1].Text = posString;
			}

			/// <summary>
			/// Converts numeric distance (meters) to string of selected unit.
			/// </summary>
			public static string FormatDistance( double meters )
			{
				string res = (meters/1000).ToString("f2");
				return res;
			}
		}


		#endregion
	
		#region measuretool gui
		internal class MeasurePropertiesDialog : System.Windows.Forms.Form
		{
			private System.Windows.Forms.RadioButton lineModeButton;
			private System.Windows.Forms.RadioButton multiLineModeButton;
			private System.Windows.Forms.Button okButton;

			private void InitializeComponent()
			{
				this.lineModeButton = new System.Windows.Forms.RadioButton();
				this.multiLineModeButton = new System.Windows.Forms.RadioButton();
				this.okButton = new System.Windows.Forms.Button();
				this.SuspendLayout();
				// 
				// lineModeButton
				// 
				this.lineModeButton.Location = new System.Drawing.Point(16, 8);
				this.lineModeButton.Name = "lineModeButton";
				this.lineModeButton.TabIndex = 0;
				this.lineModeButton.TabStop = true;
				this.lineModeButton.Text = "Line Mode";
				// 
				// multiLineModeButton
				// 
				this.multiLineModeButton.Location = new System.Drawing.Point(16, 32);
				this.multiLineModeButton.Name = "multiLineModeButton";
				this.multiLineModeButton.TabIndex = 1;
				this.multiLineModeButton.Text = "Multiline Mode";
				// 
				// okButton
				// 
				this.okButton.Location = new System.Drawing.Point(16, 64);
				this.okButton.Name = "okButton";
				this.okButton.TabIndex = 2;
				this.okButton.Text = "Ok";
				this.okButton.Click += new System.EventHandler(this.okButton_Click);


				this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
				this.ClientSize = new System.Drawing.Size(114, 98);
				this.Controls.Add(this.lineModeButton);
				this.Controls.Add(this.multiLineModeButton);
				this.Controls.Add(this.okButton);
				this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
				this.MaximizeBox = false;
				this.MinimizeBox = false;
				this.Name = "MeasurePropertiesDialog";
				this.ShowInTaskbar = false;
				this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
				this.Text = "Measure Mode";
				this.TopMost = true;
				this.ResumeLayout(false);
			}

			public MeasurePropertiesDialog()
			{
				InitializeComponent();
			}

			public MeasureMode getMeasureMode() 
			{
				if (lineModeButton.Checked == true) 
					return MeasureMode.Single;
				else 
					return MeasureMode.Multi;
			}

			private void okButton_Click(object sender, EventArgs e)
			{
				if (lineModeButton.Checked == true) 
					World.Settings.MeasureMode = MeasureMode.Single;
				else 
					World.Settings.MeasureMode = MeasureMode.Multi;
				this.Close();
			}

			new public void Show()
			{
				// 
				// MeasurePropertiesDialog
				// 
				if (World.Settings.MeasureMode == MeasureMode.Multi)
					this.multiLineModeButton.Checked = true;
				else
					this.lineModeButton.Checked = true;
				base.Show();	
			}
		}

		/// <summary>
		/// Class to create save dialog for a multi-point line
		/// </summary>
		internal class SaveMultiLine : System.Windows.Forms.Form
		{
			private System.Windows.Forms.Button cancelButton;
			//private System.Windows.Forms.ListView pointListView;
			private System.Windows.Forms.Button saveButton;
//			private System.Windows.Forms.Button openButton;
//			private System.Windows.Forms.ColumnHeader columnHeaderLat;
//			private System.Windows.Forms.ColumnHeader columnHeaderLon;
//			private System.Windows.Forms.ColumnHeader columnHeaderGridRef;

			/// Multiline Object to be serialized
			private MeasureMultiLine m_multiline;

			//Associated Layer
			private MeasureToolLayer m_layer;

		
			private void InitializeComponent()
			{
				this.saveButton = new System.Windows.Forms.Button();
				//this.openButton = new System.Windows.Forms.Button();
				this.cancelButton = new System.Windows.Forms.Button();
				//this.pointListView = new System.Windows.Forms.ListView();
				//this.columnHeaderLat = new System.Windows.Forms.ColumnHeader();
				//this.columnHeaderLon = new System.Windows.Forms.ColumnHeader();
				//this.columnHeaderGridRef = new System.Windows.Forms.ColumnHeader();
				this.SuspendLayout();
				//
				// Column headers
				//
				//this.columnHeaderGridRef.Text = "Grid Reference";
				//this.columnHeaderGridRef.Width = 100;
				//this.columnHeaderLat.Text = "Latitude";
				//this.columnHeaderLat.Width = 190;
				//this.columnHeaderLon.Text = "Longitude";
				//this.columnHeaderLon.Width = 190;
				/*
				// 
				// openButton
				// 
				this.openButton.AccessibleName = "openButton";
				this.openButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
				this.openButton.Location = new System.Drawing.Point(10,10 );
				this.openButton.Name = "openButton";
				this.openButton.TabIndex = 1;
				this.openButton.Text = "Open";
				this.openButton.Click += new System.EventHandler(this.openButton_Click);
				*/
				// 
				// saveButton
				// 
				this.saveButton.AccessibleName = "saveButton";
				this.saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
				this.saveButton.Location = new System.Drawing.Point(10,10);
				this.saveButton.Name = "saveButton";
				this.saveButton.TabIndex = 0;
				this.saveButton.Text = "Save";
				this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
				// 
				// cancelButton
				// 
				this.cancelButton.AccessibleName = "cancelButton";
				this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
				this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
				this.cancelButton.Location = new System.Drawing.Point(110, 10);
				this.cancelButton.Name = "cancelButton";
				this.cancelButton.TabIndex = 1;
				this.cancelButton.Text = "Close";
				this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
				// 
				// pointListView
				// 
//				this.pointListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
//				this.pointListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
//																								this.columnHeaderLat,
//																								this.columnHeaderLon,
//																								this.columnHeaderGridRef});
//				this.pointListView.GridLines = true;
//				this.pointListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
//				this.pointListView.Location = new System.Drawing.Point(24, 24);
//				this.pointListView.Name = "pointListView";
//				this.pointListView.Size = new System.Drawing.Size(480, 168);
//				this.pointListView.TabIndex = 0;
//				this.pointListView.View = System.Windows.Forms.View.Details;
				// 
				// SaveMultiLine
				// 
				this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
				this.ClientSize = new System.Drawing.Size(200, 50);
				//this.Controls.Add(this.pointListView);
				this.Controls.Add(this.cancelButton);
				this.Controls.Add(this.saveButton);
				//this.Controls.Add(this.openButton);
				this.MaximizeBox = false;
				this.Name = "SaveMultiLine";
				this.Text = "Save Multi-Point Line";
				this.ResumeLayout(false);
	
			}
			public SaveMultiLine(MeasureToolLayer layer)
			{
				InitializeComponent();
				m_layer = layer;
				m_multiline = layer.multiline;
			}
			/*
			private void openButton_Click(object sender, System.EventArgs e)
			{
				OpenFileDialog chooser = new OpenFileDialog();
				chooser.DefaultExt = "*.csv";
				chooser.Filter = "csv files (*.csv)|*.csv";
				chooser.Title = "Open Multiline CSV";
				chooser.ShowDialog(MainApplication.ActiveForm);
				String filename = chooser.FileName;
				Console.WriteLine(filename);
				try
				{
					StreamReader reader = new StreamReader(filename);
					String fileline = null;
					int seg = m_multiline.Count;
					while((fileline = reader.ReadLine())!=null)
					{
						String[] posStrings = fileline.Split(',');
						MeasureLine line = new MeasureLine(seg);
						line.StartLatitude = Angle.FromDegrees(Double.Parse(posStrings[0]));
						line.StartLongitude = Angle.FromDegrees(Double.Parse(posStrings[1]));
						line.EndLatitude = Angle.FromDegrees(Double.Parse(posStrings[2]));
						line.EndLongitude = Angle.FromDegrees(Double.Parse(posStrings[3]));
						line.Calculate(m_layer.LayerWorld,false);
						this.m_multiline.addLine(line);
						seg++;
					}
					reader.Close();
				}
				catch(Exception ex)
				{
					throw ex;
				}
			}
			*/
			private void saveButton_Click(object sender, System.EventArgs e)
			{
				// Heh.
				SaveFileDialog chooser = new SaveFileDialog();
				chooser.DefaultExt = "*.csv";
				chooser.Filter = "kml files (*.kml)|*.kml|Shape files (*.shp)|*.shp";
				chooser.Title = "Save Multiline";
				chooser.ShowDialog(MainApplication.ActiveForm);
				String filename = chooser.FileName;
				Console.WriteLine(filename);
				try
				{
					if(filename.EndsWith(".kml"))
					{
						StreamWriter writer = new StreamWriter(filename);
						string kml = writeKML();
						writer.WriteLine(kml);
						writer.Close();
					}
					//need to be able to save to a network a shapefile accessible
					if(filename.EndsWith(".shp"))
					{
						writeShape(filename);
					}
				}
				catch(Exception ex)
				{
					MessageBox.Show(ex.Message);
				}
			}

			/// <summary>
			/// Utility method to write shapefiles
			/// </summary>
			/// <param name="filename">Output Shapefile name</param>

			private void writeShape(string filename)
			{
				IntPtr shphandle = ShapeLib.SHPCreate(filename,ShapeLib.ShapeType.PolyLine);
						
				double[] lat = new double[m_multiline.Count];
				double[] lon = new double[m_multiline.Count];
						
				int i=0;
				foreach(MeasureLine line in m_multiline)
				{
					lat[i] = line.StartLatitude.Degrees;
					lon[i] = line.StartLongitude.Degrees;
					i++;
				}
						
				ShapeLib.SHPObject poly = ShapeLib.SHPCreateSimpleObject(ShapeLib.ShapeType.Polygon,m_multiline.Count,lon,lat,null);
				ShapeLib.SHPWriteObject(shphandle,0,poly);
				ShapeLib.SHPDestroyObject(poly);
				ShapeLib.SHPClose(shphandle);
			}

			/// <summary>
			/// Utility function to write KML strings for PolyLines/Polygons
			/// </summary>
			/// <returns></returns>
			private string writeKML()
			{
				//construct XML to send
				XmlDocument doc = new XmlDocument();
				XmlNode kmlnode = doc.CreateElement("kml");
				XmlNode node = doc.CreateElement("Placemark");
				
				XmlNode name = doc.CreateElement("name");
				name.InnerText = "New Measurement";
				node.AppendChild(name);

				XmlNode desc = doc.CreateElement("description");
				string description = "New Measurement";
				desc.InnerXml = description;
				node.AppendChild(desc);
			
				XmlNode polygon = doc.CreateElement("Polygon");
				string request = "<outerBoundaryIs><LinearRing><coordinates>";
				foreach(MeasureLine line in m_multiline)
				{
					Double lat = line.StartLatitude.Degrees;
					Double lon = line.StartLongitude.Degrees;
					request +=	 lon+","+lat+",100\n";
				}
				request += "</coordinates></LinearRing></outerBoundaryIs>";
				
				polygon.InnerXml= request;
				node.AppendChild(polygon);

				kmlnode.AppendChild(node);
				doc.AppendChild(kmlnode);
				return doc.OuterXml;
			}
	
			private void cancelButton_Click(object sender, System.EventArgs e) 
			{
				//Hide don't dispose
				this.Hide();
				// Meh.
			}
		}
		#endregion
	}
}