using System;
using System.Collections.Generic;
using System.Text;

using WorldWind;
using WorldWind.NewWidgets;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace NASA.Plugins
{
    public class Compass3D : WorldWind.PluginEngine.Plugin
    {
        string basePath = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
        FormWidget m_form = null;
        Compass3DWidget m_compass = null;
        System.Windows.Forms.MenuItem m_menuItem = null;
        WorldWind.NewWidgets.WidgetMenuButton m_toolbarItem = null;
        
        public override void Load()
        {
            m_menuItem = new System.Windows.Forms.MenuItem("Compass");
            m_menuItem.Click += new EventHandler(m_menuItem_Click);
            m_menuItem.Checked = World.Settings.ShowCompass;
            ParentApplication.ToolsMenu.MenuItems.Add(m_menuItem);
            
            m_form = new FormWidget("Compass");
            m_form.ClientSize = new System.Drawing.Size(200, 200);
            m_form.Location = new System.Drawing.Point(0, 400);
            m_form.BackgroundColor = World.Settings.WidgetBackgroundColor;
            m_form.AutoHideHeader = true;
            m_form.VerticalScrollbarEnabled = false;
            m_form.HorizontalScrollbarEnabled = false;
            m_form.BorderEnabled = false;

            m_form.OnResizeEvent += new FormWidget.ResizeHandler(m_form_OnResizeEvent);
            m_form.OnVisibleChanged += new VisibleChangedHandler(m_form_OnVisibleChanged);
            
            m_compass = new Compass3DWidget();
            m_compass.Location = new System.Drawing.Point(5, 0);
            m_compass.Font = new System.Drawing.Font("Ariel", 10.0f, System.Drawing.FontStyle.Bold);
            m_compass.ParentWidget = m_form;
            m_form_OnResizeEvent(m_form, m_form.WidgetSize);
            
            m_form.ChildWidgets.Add(m_compass);
            m_form.Visible = World.Settings.ShowCompass;

            DrawArgs.NewRootWidget.ChildWidgets.Add(m_form);

            m_toolbarItem = new WorldWind.NewWidgets.WidgetMenuButton(
                    "Compass 3D",
                    basePath + "\\Data\\Icons\\Interface\\compass2.png",
                    m_form);

            ParentApplication.WorldWindow.MenuBar.AddToolsMenuButton(m_toolbarItem);


            base.Load();
        }

        public override void Unload()
        {
            if (m_form != null)
            {
                DrawArgs.NewRootWidget.ChildWidgets.Remove(m_form);
                m_form.Dispose();
                m_form = null;
            }

            ParentApplication.ToolsMenu.MenuItems.Remove(m_menuItem);

            if (m_toolbarItem != null)
            {
                ParentApplication.WorldWindow.MenuBar.RemoveToolsMenuButton(m_toolbarItem);
                m_toolbarItem.Dispose();
                m_toolbarItem = null;
            }

            base.Unload();
        }

        void m_menuItem_Click(object sender, EventArgs e)
        {
            m_menuItem.Checked = !m_menuItem.Checked;
            if (m_form != null)
                m_form.Visible = m_menuItem.Checked;
        }

        void m_form_OnVisibleChanged(object o, bool state)
        {
            m_menuItem.Checked = state;
            World.Settings.ShowCompass = state;
        }

        void m_form_OnResizeEvent(object IWidget, System.Drawing.Size size)
        {
            if (m_form != null && m_compass != null)
            {
                m_compass.ClientSize = new System.Drawing.Size(m_form.WidgetSize.Width - 10, m_form.WidgetSize.Height - 20);
            }
        }
    }

    public class Compass3DWidget : IWidget
    {
        System.Drawing.Point m_Location = new System.Drawing.Point(0, 0);
        System.Drawing.Size m_Size = new System.Drawing.Size(200, 200);
        bool m_Visible = true;
        bool m_Enabled = true;
        IWidget m_ParentWidget = null;
        object m_Tag = null;
        System.Drawing.Color m_ForeColor = System.Drawing.Color.White;
        string m_Name = "";
        System.Drawing.Font m_localFont = null;
        Font m_drawingFont = null;

        /// <summary>
        /// CountHeight property value
        /// </summary>
        protected bool m_countHeight = true;

        /// <summary>
        /// CountWidth property value
        /// </summary>
        protected bool m_countWidth = true;

        public Compass3DWidget()
        {

        }

        #region Properties
        public System.Drawing.Font Font
        {
            get { return m_localFont; }
            set
            {
                m_localFont = value;
                if (m_drawingFont != null)
                {
                    m_drawingFont.Dispose();
                    m_drawingFont = new Font(DrawArgs.Device, m_localFont);
                }
            }
        }
        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                m_Name = value;
            }
        }
        public System.Drawing.Color ForeColor
        {
            get
            {
                return m_ForeColor;
            }
            set
            {
                m_ForeColor = value;
            }
        }
        #endregion

        #region IWidget Members

        public IWidget ParentWidget
        {
            get
            {
                return m_ParentWidget;
            }
            set
            {
                m_ParentWidget = value;
            }
        }

        public bool Visible
        {
            get
            {
                return m_Visible;
            }
            set
            {
                m_Visible = value;
            }
        }

        public object Tag
        {
            get
            {
                return m_Tag;
            }
            set
            {
                m_Tag = value;
            }
        }

        public IWidgetCollection ChildWidgets
        {
            get
            {
                return null;
            }
            set
            {

            }
        }

        public System.Drawing.Size ClientSize
        {
            get
            {
                return m_Size;
            }
            set
            {
                m_Size = value;
            }
        }

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

        public System.Drawing.Point ClientLocation
        {
            get
            {
                return m_Location;
            }
            set
            {
                m_Location = value;
            }
        }

        public System.Drawing.Point AbsoluteLocation
        {
            get
            {
                if (m_ParentWidget != null)
                {
                    return new System.Drawing.Point(
                        m_Location.X + m_ParentWidget.ClientLocation.X,
                        m_Location.Y + m_ParentWidget.ClientLocation.Y);

                }
                else
                {
                    return m_Location;
                }
            }
        }


        /// New IWidget properties

        /// <summary>
        /// Location of this widget relative to the client area of the parent
        /// </summary>
        public System.Drawing.Point Location
        {
            get { return m_Location; }
            set { m_Location = value; }
        }

        /// <summary>
        /// Size of widget in pixels
        /// </summary>
        public System.Drawing.Size WidgetSize
        {
            get { return m_Size; }
            set { m_Size = value; }
        }


        /// <summary>
        /// Whether this widget should count for height calculations - HACK until we do real layout
        /// </summary>
        public bool CountHeight
        {
            get { return m_countHeight; }
            set { m_countHeight = value; }
        }


        /// <summary>
        /// Whether this widget should count for width calculations - HACK until we do real layout
        /// </summary>
        public bool CountWidth
        {
            get { return m_countWidth; }
            set { m_countWidth = value; }
        }

        public void Initialize(DrawArgs drawArgs)
        {
        }

        Viewport viewport = new Viewport();
                    
        public void Render(DrawArgs drawArgs)
        {
            try
            {
                if (m_Visible)
                {
                    if (m_compassTexture == null)
                    {
                        m_compassTexture = ImageHelper.LoadTexture(
                        System.IO.Path.GetDirectoryName(
                            System.Windows.Forms.Application.ExecutablePath) + "\\Data\\Icons\\Interface\\compass-simple.png");
                    }

                    if (m_localFont != null && m_drawingFont == null)
                    {
                        m_drawingFont = new Font(drawArgs.device, m_localFont);
                    }

					string displayString = string.Format("{0:f0}°", drawArgs.WorldCamera.Heading.Degrees);
                    //DrawTextFormat drawTextFormat = DrawTextFormat.Center | DrawTextFormat.VerticalCenter;

                    drawArgs.device.RenderState.ZBufferEnable = false;
                    drawArgs.device.TextureState[0].ColorOperation = TextureOperation.SelectArg1;
                    drawArgs.device.TextureState[0].ColorArgument1 = TextureArgument.Diffuse;
                    drawArgs.device.TextureState[0].AlphaOperation = TextureOperation.SelectArg1;
                    drawArgs.device.TextureState[0].AlphaArgument1 = TextureArgument.Diffuse;

                    viewport.X = AbsoluteLocation.X;
                    viewport.Y = AbsoluteLocation.Y;
                    viewport.Width = (drawArgs.device.Viewport.Width < m_Size.Width ? drawArgs.device.Viewport.Width : m_Size.Width);
                    viewport.Height = (drawArgs.device.Viewport.Height < m_Size.Height ? drawArgs.device.Viewport.Height : m_Size.Height);
                    viewport.MinZ = drawArgs.WorldCamera.Viewport.MinZ;
                    viewport.MaxZ = drawArgs.WorldCamera.Viewport.MaxZ;

                    Viewport savedViewport = drawArgs.device.Viewport;
                    //drawArgs.device.Viewport = viewport;

                    RenderCompass(drawArgs);

                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle(
                        AbsoluteLocation.X + viewport.Width / 2 - 7,
                        AbsoluteLocation.Y + viewport.Height / 2,
                        viewport.Width / 2,
                        viewport.Height / 2);

                    if (m_drawingFont == null)
                    {
                        drawArgs.defaultDrawingFont.DrawText(
                            null,
                            displayString,
                            rect,
                            DrawTextFormat.Left,//drawTextFormat,
                            m_ForeColor);
                    }
                    else
                    {
                        m_drawingFont.DrawText(
                            null,
                            displayString,
                            rect,
                            DrawTextFormat.Left,
                            m_ForeColor);
                    }

                    //drawArgs.device.Viewport = savedViewport;
                }
            }
            catch (Exception ex)
            {
                Utility.Log.Write(ex);
            }
        }

        #endregion

        int samples = 16;
        CustomVertex.PositionColored[] m_nVertices = null;
        CustomVertex.PositionColored[] m_wVertices = null;
        CustomVertex.PositionColored[] m_eVertices = null;
        CustomVertex.PositionColored[] m_sVertices = null;
        CustomVertex.PositionColored[] m_arrowVertices = null;
        CustomVertex.PositionColoredTextured[] m_compassVertices = null;
        float outerRadius = 1f;
        float innerRadius = 0.7f;
        Texture m_compassTexture = null;
        private void CreateVertices(ref CustomVertex.PositionColored[] vertices, float startAngle, float endAngle, int steps, int color)
        {
            float sweepAngle = endAngle - startAngle;

            vertices = new CustomVertex.PositionColored[(steps + 1) * 2];
            for (int i = 0; i <= steps; i++)
            {
                float angle = ((float)i / (float)steps) * sweepAngle + startAngle;
                vertices[2 * i].X = (float)Math.Cos(angle) * outerRadius;
                vertices[2 * i].Y = (float)Math.Sin(angle) * outerRadius;
                vertices[2 * i].Z = 0;
                vertices[2 * i].Color = color;


                vertices[2 * i + 1].X = (float)Math.Cos(angle) * innerRadius;
                vertices[2 * i + 1].Y = (float)Math.Sin(angle) * innerRadius;
                vertices[2 * i + 1].Z = 0;
                vertices[2 * i + 1].Color = color;

            }
            
        }

        private void CreateArrows(ref CustomVertex.PositionColored[] vertices)
        {
            float y_offset_top = 1.6f;
            float y_offset_bottom = 1.2f;
            float x_offset = 0.10f;

            int red = System.Drawing.Color.Red.ToArgb();
            int gray = System.Drawing.Color.Gray.ToArgb();

            vertices = new CustomVertex.PositionColored[3 * 4];
            vertices[0].X = 0;
            vertices[0].Y = y_offset_top;
            vertices[0].Z = 0;
            vertices[0].Color = red;

            vertices[1].X = x_offset;
            vertices[1].Y = y_offset_bottom;
            vertices[1].Z = 0;
            vertices[1].Color = red;

            vertices[2].X = -x_offset;
            vertices[2].Y = y_offset_bottom;
            vertices[2].Z = 0;
            vertices[2].Color = red;
            ///////////////////
            vertices[3].X = -y_offset_top;
            vertices[3].Y = 0;
            vertices[3].Z = 0;
            vertices[3].Color = gray;

            vertices[4].X = -y_offset_bottom;
            vertices[4].Y = x_offset;
            vertices[4].Z = 0;
            vertices[4].Color = gray;

            vertices[5].X = -y_offset_bottom;
            vertices[5].Y = -x_offset;
            vertices[5].Z = 0;
            vertices[5].Color = gray;
            //////////////
            vertices[6].X = 0;
            vertices[6].Y = -y_offset_top;
            vertices[6].Z = 0;
            vertices[6].Color = gray;

            vertices[7].X = -x_offset;
            vertices[7].Y = -y_offset_bottom;
            vertices[7].Z = 0;
            vertices[7].Color = gray;

            vertices[8].X = x_offset;
            vertices[8].Y = -y_offset_bottom;
            vertices[8].Z = 0;
            vertices[8].Color = gray;
            //////////////////
            vertices[9].X = y_offset_top;
            vertices[9].Y = 0;
            vertices[9].Z = 0;
            vertices[9].Color = gray;

            vertices[10].X = y_offset_bottom;
            vertices[10].Y = -x_offset;
            vertices[10].Z = 0;
            vertices[10].Color = gray;

            vertices[11].X = y_offset_bottom;
            vertices[11].Y = x_offset;
            vertices[11].Z = 0;
            vertices[11].Color = gray;
        }

        private void CreateBitmapVertices(DrawArgs drawArgs)
        {
            int compassColor = System.Drawing.Color.White.ToArgb();

            m_compassVertices = new CustomVertex.PositionColoredTextured[4];
            m_compassVertices[0].X = -outerRadius;
            m_compassVertices[0].Y = outerRadius;
            m_compassVertices[0].Z = 0;
            m_compassVertices[0].Color = compassColor;
            m_compassVertices[0].Tu = 0;
            m_compassVertices[0].Tv = 0;

            m_compassVertices[1].X = -outerRadius;
            m_compassVertices[1].Y = -outerRadius;
            m_compassVertices[1].Z = 0;
            m_compassVertices[1].Color = compassColor;
            m_compassVertices[1].Tu = 0;
            m_compassVertices[1].Tv = 1;

            m_compassVertices[2].X = outerRadius;
            m_compassVertices[2].Y = outerRadius;
            m_compassVertices[2].Z = 0;
            m_compassVertices[2].Color = compassColor;
            m_compassVertices[2].Tu = 1;
            m_compassVertices[2].Tv = 0;

            m_compassVertices[3].X = outerRadius;
            m_compassVertices[3].Y = -outerRadius;
            m_compassVertices[3].Z = 0;
            m_compassVertices[3].Color = compassColor;
            m_compassVertices[3].Tu = 1;
            m_compassVertices[3].Tv = 1;
        }

        private void RenderCompass(DrawArgs drawArgs)
        {
            /*if (m_nVertices == null)
                CreateVertices(ref m_nVertices, 0.25f * (float)Math.PI, 0.75f * (float)Math.PI, samples, System.Drawing.Color.Red.ToArgb());
            if (m_wVertices == null)
                CreateVertices(ref m_wVertices, 0.78f * (float)Math.PI, 1.22f * (float)Math.PI, samples, System.Drawing.Color.Gray.ToArgb());
            if (m_sVertices == null)
                CreateVertices(ref m_sVertices, 1.25f * (float)Math.PI, 1.75f * (float)Math.PI, samples, System.Drawing.Color.Gray.ToArgb());
            if (m_eVertices == null)
                CreateVertices(ref m_eVertices, 1.78f * (float)Math.PI, 2.22f * (float)Math.PI, samples, System.Drawing.Color.Gray.ToArgb());
            if (m_arrowVertices == null)
                CreateArrows(ref m_arrowVertices);
            */

            if (m_compassVertices == null)
                CreateBitmapVertices(drawArgs);

            drawArgs.device.Transform.World = Matrix.RotationZ((float)drawArgs.WorldCamera.Heading.Radians);
            drawArgs.device.Transform.World *= Matrix.RotationX(0.7f * (float)drawArgs.WorldCamera.Tilt.Radians);
            
            drawArgs.device.Transform.View = Matrix.LookAtLH(
                new Vector3(0, 0, -1.5f),
                new Vector3(0, 0, 0),
                new Vector3(0, 1, 0));

            bool lighting = drawArgs.device.RenderState.Lighting;
            drawArgs.device.RenderState.Lighting = false;
            drawArgs.device.Transform.Projection = Matrix.PerspectiveFovLH(0.45f * (float)Math.PI, (float)m_Size.Width / (float)m_Size.Height, 0.0f, 10.0f);
            Cull cull = drawArgs.device.RenderState.CullMode; 
            drawArgs.device.RenderState.CullMode = Cull.None;
            /*drawArgs.device.VertexFormat = CustomVertex.PositionColored.Format;
            
            drawArgs.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, m_nVertices.Length - 2, m_nVertices);
            drawArgs.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, m_wVertices.Length - 2, m_wVertices);
            drawArgs.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, m_eVertices.Length - 2, m_eVertices);
            drawArgs.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, m_sVertices.Length - 2, m_sVertices);
            drawArgs.device.DrawUserPrimitives(PrimitiveType.TriangleList, m_arrowVertices.Length / 3, m_arrowVertices);
            */
            drawArgs.device.VertexFormat = CustomVertex.PositionColoredTextured.Format;
            drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Modulate;
            drawArgs.device.TextureState[0].ColorArgument1 = TextureArgument.Diffuse;
            drawArgs.device.TextureState[0].ColorArgument2 = TextureArgument.TextureColor;
            drawArgs.device.TextureState[0].AlphaOperation = TextureOperation.Modulate;
            drawArgs.device.TextureState[0].AlphaArgument1 = TextureArgument.Diffuse;
            drawArgs.device.TextureState[0].AlphaArgument2 = TextureArgument.TextureColor;
            drawArgs.device.SetTexture(0, m_compassTexture);
            drawArgs.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, m_compassVertices.Length - 2, m_compassVertices);

            drawArgs.device.RenderState.Lighting = lighting;
            drawArgs.device.RenderState.CullMode = cull;
            drawArgs.device.Transform.World = drawArgs.WorldCamera.WorldMatrix;
            drawArgs.device.Transform.View = drawArgs.WorldCamera.ViewMatrix;
            drawArgs.device.Transform.Projection = drawArgs.WorldCamera.ProjectionMatrix;
        }
    }

}