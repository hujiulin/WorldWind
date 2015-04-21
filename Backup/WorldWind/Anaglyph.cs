//----------------------------------------------------------------------------
// NAME: Anaglyph Stereo 3D
// VERSION: 1.3
// DESCRIPTION: Renders stereo images (3D). Enable this plug-in while wearing Red/Cyan glasses.
// DEVELOPER: Bjorn "Mashi" Reppen, Patrick Murris
// WEBSITE: http://www.mashiharu.com
//----------------------------------------------------------------------------
//
// This file is in the Public Domain, and comes with no warranty.
//
// Revisions:
//
// 2007-01-10 (v1.3) : Fixed for WW 1.4, and simplified View transform (Patrick Murris)
// 2005-08-04 (v1.2) : Added support for older graphics cards.
// 2005-07-24 (v1.11): Fixes loss of device causes World Wind to only display a red X in main window.
// 2005-07-23 (v1.10): Colored anaglyphs, no more grayscale textures, render optimization
//
// Known issues:
//
//

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using WorldWind;
using WorldWind.Camera;
using WorldWind.Renderable;

namespace Mashi.Stereo
{
    /// <summary>
    /// The plugin (main class)
    /// </summary>
    public class AnaglyphStereo : WorldWind.PluginEngine.Plugin
    {
        /// <summary>
        /// Name displayed in layer manager
        /// </summary>
        public static string LayerName = "Anaglyph Stereo 3D";

        System.Windows.Forms.MenuItem menuItem;

        StereoLayer layer;

        /// <summary>
        /// Plugin entry point - All plugins must implement this function
        /// </summary>
        public override void Load()
        {
            menuItem = new System.Windows.Forms.MenuItem();
            menuItem.Text = "Anaglyph 3D View Mode";
            menuItem.Click += new System.EventHandler(menuItem_Click);
            ParentApplication.ViewMenu.MenuItems.Add(menuItem);


            Caps caps = Application.WorldWindow.DrawArgs.device.DeviceCaps;
            if (!caps.DestinationBlendCaps.SupportsBlendFactor ||
                    !caps.SourceBlendCaps.SupportsBlendFactor)
            {
                throw new ApplicationException("The graphics adapter is not compatible, no blend factor support.");
            }

            layer = new StereoLayer(LayerName, Application.WorldWindow);
            Application.WorldWindow.CurrentWorld.RenderableObjects.Add(layer);
            layer.IsOn = false; //turn off

            //            menuItem_Click(null, null);   //And on to trigger check and warning
            //   !!!!!!!!!=========Uncomment the above line to enable on startup===============!!!!!!!!!!!!
        }

        /// <summary>
        /// Unloads our plugin
        /// </summary>
        public override void Unload()
        {
            if (menuItem != null)
            {
                ParentApplication.ViewMenu.MenuItems.Remove(menuItem);
                menuItem.Dispose();
                menuItem = null;
            }

            if (layer != null)
            {
                Application.WorldWindow.CurrentWorld.RenderableObjects.Remove(layer);
                layer.Dispose();
                layer = null;
            }
        }
        void menuItem_Click(object sender, EventArgs e)
        {
            layer.IsOn = !layer.IsOn;
            menuItem.Checked = layer.IsOn;
        }
    }

    /// <summary>
    /// Anaglyph Stereo layer
    /// </summary>
    public class StereoLayer : RenderableObject
    {
        WorldWindow worldWin;
        CustomVertex.TransformedTextured[] windowQuad = new CustomVertex.TransformedTextured[4];
        Texture m_rightTexture;
        float m_interocularDistance = 1f;       // Distance to target factor / 100
        float m_focusAngle = 0.6f;              // Convergence angle between left and right in degree
        bool isRendering;

        /// <summary>
        /// Constructor
        /// </summary>
        public StereoLayer(string LayerName, WorldWindow worldWindow)
            : base(LayerName)
        {
            this.RenderPriority = RenderPriority.Icons;
            this.worldWin = worldWindow;

            worldWindow.DrawArgs.device.DeviceLost += new EventHandler(OnDeviceLost);
        }

        /// <summary>
        /// Opacity not available
        /// </summary>
        [Browsable(false)]
        public override byte Opacity
        {
            get
            {
                return base.Opacity;
            }
            set
            {
                base.Opacity = value;
            }
        }

        /* Not currently in use
                       /// <summary>
                       /// Color of the right eye filter.
                       /// </summary>
                       [Editor(typeof(ColorEditor),typeof(UITypeEditor))]
                       [Description("Color of the right eye filter.")]
                       public Color RightEyeColor
                       {
                               get { return m_rightEyeColor; }
                               set { m_rightEyeColor = value; }
                       }
        */

        public override bool IsOn
        {
            get
            {
                return base.IsOn;
            }
            set
            {
                if (value == isOn)
                    return;

                base.IsOn = value;
                if (isOn)
                {
                    MessageBox.Show("Prolonged use of 3D glasses may lead to problems such as headaches, dizziness, or disorientation.\nTo avoid symptoms, take rest periods to allow your eyes to recover between uses.", "Warning!");
                }
            }
        }

        /// <summary>
        /// The distance separating the two eyes - percent of distance to target.
        /// </summary>
        [Description("The relative distance separating the two eyes.")]
        public float InterocularDistance
        {
            get { return m_interocularDistance; }
            set { m_interocularDistance = value; }
        }


        /// <summary>
        /// The convergence angle between left and right view in degree.
        /// </summary>
        [Description("The relative distance separating the two eyes.")]
        public float FocusAngle
        {
            get { return m_focusAngle; }
            set { m_focusAngle = value; }
        }

        #region RenderableObject

        /// <summary>
        /// This is where we do our rendering
        /// Called from UI thread = UI code safe in this function
        /// </summary>
        public override void Render(DrawArgs drawArgs)
        {
            if (!isInitialized)
                Initialize(drawArgs);

            if (!isInitialized)
                return;

            if (isRendering)
                // Avoid recursion when we render our right image from inside render
                return;
            isRendering = true;

            try
            {
                Device device = drawArgs.device;
                CameraBase camera = drawArgs.WorldCamera;
                Matrix View = camera.ViewMatrix;

                float eyeDist = m_interocularDistance / 100 * (float)camera.Distance;
                device.Transform.View *= Matrix.Translation(eyeDist, 0f, 0f);
                device.Transform.View *= Matrix.RotationY((float)MathEngine.DegreesToRadians(m_focusAngle));

                RenderRightEye(drawArgs, m_rightTexture, camera);

                // Combine results
                device.RenderState.ZBufferEnable = false;
                device.VertexFormat = CustomVertex.TransformedTextured.Format;
                device.TextureState[0].ColorOperation = TextureOperation.SelectArg1;
                device.TextureState[0].ColorArgument1 = TextureArgument.TextureColor;
                device.RenderState.ColorWriteEnable = ColorWriteEnable.Green | ColorWriteEnable.Blue;
                device.SetTexture(0, m_rightTexture);
                device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, windowQuad);

                // Reset state
                device.SetTexture(0, null);
                device.RenderState.ColorWriteEnable = ColorWriteEnable.RedGreenBlueAlpha;
            }
            finally
            {
                isRendering = false;
            }
        }

        /// <summary>
        /// Render view from right eye
        /// </summary>
        void RenderRightEye(DrawArgs drawArgs, Texture t, CameraBase camera)
        {
            Device device = drawArgs.device;
            using (Surface outputSurface = t.GetSurfaceLevel(0))
            {
                // Render to our texture
                Surface backBuffer = device.GetRenderTarget(0);
                device.SetRenderTarget(0, outputSurface);

                // Clear
                System.Drawing.Color backgroundColor = System.Drawing.Color.Black;
                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, backgroundColor, 1.0f, 0);

                // Render world
                worldWin.CurrentWorld.Render(drawArgs);

                // Restore our original back buffer and world transform
                device.SetRenderTarget(0, backBuffer);

            }
        }

        /// <summary>
        /// RenderableObject abstract member (needed)
        /// OBS: Worker thread (don't update UI directly from this thread)
        /// </summary>
        public override void Initialize(DrawArgs drawArgs)
        {
            Device device = drawArgs.device;

            // Get the back buffer description
            try
            {
                SurfaceDescription backBufferDescription;
                using (Surface backBuffer = device.GetBackBuffer(0, 0, BackBufferType.Mono))
                    backBufferDescription = backBuffer.Description;

                // Bottom left
                windowQuad[0].Y = backBufferDescription.Height;
                windowQuad[0].Tv = 1;

                // Bottom right
                windowQuad[1].X = backBufferDescription.Width;
                windowQuad[1].Y = backBufferDescription.Height;
                windowQuad[1].Tu = 1;
                windowQuad[1].Tv = 1;

                // Top left (all 0s)

                // Triangle 2 (uses previous 2 vertices as first 2 points (TriangleStrip))

                // Top right
                windowQuad[3].X = backBufferDescription.Width;
                windowQuad[3].Tu = 1;

                if (m_rightTexture == null || m_rightTexture.Disposed)
                    m_rightTexture = new Texture(drawArgs.device,
                            backBufferDescription.Width, backBufferDescription.Height,
                            1, Usage.RenderTarget, backBufferDescription.Format, Pool.Default);

                isInitialized = true;
            }
            catch (DirectXException)
            {
                // All user created D3DPOOL_DEFAULT surfaces must be freed before Reset can succeed.
                Dispose();
            }
        }

        /// <summary>
        /// RenderableObject abstract member (needed)
        /// OBS: Worker thread (don't update UI directly from this thread)
        /// </summary>
        public override void Update(DrawArgs drawArgs)
        {
        }

        /// <summary>
        /// RenderableObject abstract member (needed)
        /// OBS: Worker thread (don't update UI directly from this thread)
        /// </summary>
        public override void Dispose()
        {
            isInitialized = false;
            if (m_rightTexture != null)
            {
                m_rightTexture.Dispose();
                m_rightTexture = null;
            }
        }

        /// <summary>
        /// Gets called when user left clicks.
        /// RenderableObject abstract member (needed)
        /// Called from UI thread = UI code safe in this function
        /// </summary>
        public override bool PerformSelectionAction(DrawArgs drawArgs)
        {
            return false;
        }

        #endregion

        /// <summary>
        /// Direct3D device was lost or is resizing
        /// </summary>
        public void OnDeviceLost(object sender, EventArgs e)
        {
            // Our textures are in default pool, we'll need new ones.
            Dispose();
        }
    }
}