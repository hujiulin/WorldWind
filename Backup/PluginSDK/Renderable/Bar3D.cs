using System;

using System.Collections.Generic;

using System.Text;



using WorldWind;

using Utility;



using Microsoft.DirectX;

using Microsoft.DirectX.Direct3D;



namespace WorldWind.Renderable
{

    public class Bar3D : RenderableObject
    {

        double m_distanceAboveSurface = 0;

        double m_latitude = 0;

        double m_longitude = 0;

        double m_height = 0;

        int m_color = 0;

        float m_currentVerticalExaggeration = World.Settings.VerticalExaggeration;

        Point3d m_cartesianPoint = null;



        private bool m_useScaling = false;

        private double m_scalarMinimum = 0;

        private double m_scalarMaximum = 1;

        private double m_currentPercent = 0;

        private double m_targetScalar = 1;

        private float m_scaleX = 1;

        private float m_scaleY = 1;



        public double Latitude
        {

            get { return m_latitude; }

            set
            {

                m_latitude = value;

                UpdateCartesianPoint();

            }

        }



        public double Longitude
        {

            get { return m_longitude; }

            set
            {

                m_longitude = value;

                UpdateCartesianPoint();

            }

        }



        public double DistanceAboveSurface
        {

            get { return m_distanceAboveSurface; }

            set
            {

                m_distanceAboveSurface = value;

                UpdateCartesianPoint();

            }

        }



        public double Height
        {

            get { return m_height; }

            set { m_height = value; }

        }



        public bool UseScaling
        {

            get { return m_useScaling; }

            set { m_useScaling = value; }

        }



        public double ScalarMinimum
        {

            get { return m_scalarMinimum; }

            set { m_scalarMinimum = value; }

        }



        public double ScalarMaximum
        {

            get { return m_scalarMaximum; }

            set { m_scalarMaximum = value; }

        }



        public double ScalarValue
        {

            get { return m_targetScalar; }

            set { m_targetScalar = value; }

        }



        public float ScaleX
        {

            get { return m_scaleX; }

            set { m_scaleX = value; }

        }



        public float ScaleY
        {

            get { return m_scaleY; }

            set { m_scaleY = value; }

        }



        public Bar3D(

            string name,

            World parentWorld,

            double latitude,

            double longitude,

            double distanceAboveSurface,

            double height,

            System.Drawing.Color color)

            : base(name, parentWorld)
        {

            m_latitude = latitude;

            m_longitude = longitude;

            m_distanceAboveSurface = distanceAboveSurface;

            m_height = height;

            m_color = color.ToArgb();



            UpdateCartesianPoint();

        }



        private void UpdateCartesianPoint()
        {

            m_cartesianPoint = MathEngine.SphericalToCartesianD(

                Angle.FromDegrees(m_latitude), Angle.FromDegrees(m_longitude), m_world.EquatorialRadius + m_distanceAboveSurface * World.Settings.VerticalExaggeration);



            m_currentVerticalExaggeration = World.Settings.VerticalExaggeration;

        }



        public override void Initialize(DrawArgs drawArgs)
        {

            isInitialized = true;

        }



        public override void Update(DrawArgs drawArgs)
        {

            if (!isInitialized)

                Initialize(drawArgs);

        }



        public override void Render(DrawArgs drawArgs)
        {

            RenderBar(drawArgs);



        }



        public override void Dispose()
        {



        }



        public override bool PerformSelectionAction(DrawArgs drawArgs)
        {

            return false;

        }



        public double RenderedHeight = 0;



        private void RenderBar(DrawArgs drawArgs)
        {

            bool lighting = drawArgs.device.RenderState.Lighting;
            drawArgs.device.RenderState.Lighting = false;

            Matrix translation = Matrix.Translation(

                    (float)(m_cartesianPoint.X - drawArgs.WorldCamera.ReferenceCenter.X),

                    (float)(m_cartesianPoint.Y - drawArgs.WorldCamera.ReferenceCenter.Y),

                    (float)(m_cartesianPoint.Z - drawArgs.WorldCamera.ReferenceCenter.Z)

                        );





            if (m_extrudeVertices == null)
            {

                CreateExtrude(drawArgs.device);

            }



            if (m_useScaling)
            {

                double targetPercent = (m_targetScalar - m_scalarMinimum) / (m_scalarMaximum - m_scalarMinimum);

                if (m_currentPercent != targetPercent)
                {

                    double delta = Math.Abs(targetPercent - m_currentPercent);

                    delta *= 0.1;

                    if (m_currentPercent < targetPercent)

                        m_currentPercent += delta;

                    else

                        m_currentPercent -= delta;

                }



                if (getColor(m_currentPercent).ToArgb() != m_extrudeVertices[0].Color)
                {

                    UpdateColor(m_currentPercent);

                }

                RenderedHeight = m_currentPercent * World.Settings.VerticalExaggeration * m_height;

                drawArgs.device.Transform.World = Matrix.Scaling(m_scaleX, m_scaleY, (float)-m_currentPercent * World.Settings.VerticalExaggeration * (float)m_height);

            }

            else
            {

                if (m_color != m_extrudeVertices[0].Color)
                {

                    UpdateColor(m_color);

                }

                RenderedHeight = m_height * World.Settings.VerticalExaggeration;

                drawArgs.device.Transform.World = Matrix.Scaling(m_scaleX, m_scaleY, (float)-m_height * World.Settings.VerticalExaggeration);

            }



            drawArgs.device.Transform.World *= Matrix.RotationY((float)-MathEngine.DegreesToRadians(90));

            drawArgs.device.Transform.World *= Matrix.RotationY((float)-MathEngine.DegreesToRadians(m_latitude));

            drawArgs.device.Transform.World *= Matrix.RotationZ((float)MathEngine.DegreesToRadians(m_longitude));

            drawArgs.device.Transform.World *= translation;



            drawArgs.device.VertexFormat = CustomVertex.PositionColored.Format;

            drawArgs.device.TextureState[0].ColorOperation = TextureOperation.SelectArg1;

            drawArgs.device.TextureState[0].ColorArgument1 = TextureArgument.Diffuse;

            drawArgs.device.TextureState[0].AlphaArgument1 = TextureArgument.Diffuse;

            drawArgs.device.TextureState[0].AlphaOperation = TextureOperation.SelectArg1;

            drawArgs.device.DrawIndexedUserPrimitives(PrimitiveType.TriangleList, 0, m_extrudeVertices.Length, m_extrudeIndices.Length / 3, m_extrudeIndices, true, m_extrudeVertices);

            drawArgs.device.DrawIndexedUserPrimitives(PrimitiveType.LineList, 0, m_outlineVertices.Length, m_outlineIndices.Length / 2, m_outlineIndices, true, m_outlineVertices);

            drawArgs.device.RenderState.Lighting = lighting;
        }



        private System.Drawing.Color getColor(double percent)
        {

            double min = 0;

            double max = 1;



            double red = 1.0;

            double green = 1.0;

            double blue = 1.0;





            //TODO: make this a function and abstract to allow multiple gradient mappings

            double dv;



            if (percent < min)

                percent = min;

            if (percent > max)

                percent = max;



            dv = max - min;



            if (percent < (min + 0.25 * dv))
            {

                red = 0;

                green = 4 * (percent - min) / dv;

            }

            else if (percent < (min + 0.5 * dv))
            {

                red = 0;

                blue = 1 + 4 * (min + 0.25 * dv - percent) / dv;

            }

            else if (percent < (min + 0.75 * dv))
            {

                red = 4 * (percent - min - 0.5 * dv) / dv;

                blue = 0;

            }

            else
            {

                green = 1 + 4 * (min + 0.75 * dv - percent) / dv;

                blue = 0;

            }



            return System.Drawing.Color.FromArgb((int)(255 * red), (int)(255 * green), (int)(255 * blue));

        }



        private void UpdateColor(double percent)
        {

            int color = getColor(percent).ToArgb();

            UpdateColor(color);

        }



        private void UpdateColor(int color)
        {

            for (int i = 0; i < m_extrudeVertices.Length; i++)
            {

                m_extrudeVertices[i].Color = color;

            }

        }



        short[] m_extrudeIndices = null;

        CustomVertex.PositionColored[] m_extrudeVertices = null;



        short[] m_outlineIndices = null;

        CustomVertex.PositionColored[] m_outlineVertices = null;



        private void CreateExtrude(Device device)
        {

            m_extrudeVertices = new CustomVertex.PositionColored[8];

            m_outlineVertices = new CustomVertex.PositionColored[8];

            int c = System.Drawing.Color.Red.ToArgb();

            int outline = System.Drawing.Color.DarkGray.ToArgb();



            m_extrudeVertices[0] = new CustomVertex.PositionColored(-1, -1, 1, c);

            m_extrudeVertices[1] = new CustomVertex.PositionColored(-1, 1, 1, c);

            m_extrudeVertices[2] = new CustomVertex.PositionColored(1, -1, 1, c);

            m_extrudeVertices[3] = new CustomVertex.PositionColored(1, 1, 1, c);



            m_extrudeVertices[4] = new CustomVertex.PositionColored(-1, -1, 0, c);

            m_extrudeVertices[5] = new CustomVertex.PositionColored(-1, 1, 0, c);

            m_extrudeVertices[6] = new CustomVertex.PositionColored(1, -1, 0, c);

            m_extrudeVertices[7] = new CustomVertex.PositionColored(1, 1, 0, c);



            m_outlineVertices[0] = new CustomVertex.PositionColored(-1, -1, 1, outline);

            m_outlineVertices[1] = new CustomVertex.PositionColored(-1, 1, 1, outline);

            m_outlineVertices[2] = new CustomVertex.PositionColored(1, -1, 1, outline);

            m_outlineVertices[3] = new CustomVertex.PositionColored(1, 1, 1, outline);



            m_outlineVertices[4] = new CustomVertex.PositionColored(-1, -1, 0, outline);

            m_outlineVertices[5] = new CustomVertex.PositionColored(-1, 1, 0, outline);

            m_outlineVertices[6] = new CustomVertex.PositionColored(1, -1, 0, outline);

            m_outlineVertices[7] = new CustomVertex.PositionColored(1, 1, 0, outline);



            m_extrudeIndices = new short[] {
 
                // top face
 
                0, 1, 2,
 
                2, 1, 3,
 
                // bottom face
 
                4, 5, 6,
 
                6, 5, 7,
 
                // front face
 
                4, 0, 6,
 
                6, 0, 2,
 
                // back face
 
                7, 3, 5,
 
                5, 3, 1,
 
                // left face
 
                5, 1, 4,
 
                4, 1, 0,
 
                // right face
 
                6, 2, 7,
 
                7, 2, 3
 
            };



            m_outlineIndices = new short[] {
 
                0,1, 1,3, 3,2, 2,0,
 
                4,5, 5,7, 7,6, 6,4,
 
                0,4, 2,6, 3,7, 1,5 };



        }

    }

}

