/*
 * Authoer: Jiulin Hu (tohujiulin@126.com Beijing Normal University)
 * Version: 1.0
 * Description: Data Visulation Demo from AcitViz
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Kitware.VTK;

namespace DataVisualization
{
    public class MainPaltform : Form
    {
        #region Common parameters
        private string m_FileName;
        private vtkRenderWindow m_RenderWindow = null;
        private vtkRenderer m_Renderer = null;
        private vtkProp3D imgProp = null;
        private vtkRenderWindowInteractor Interactor = null;
        private vtkCamera m_Camera = null;
        private bool isWander = false;
        #endregion

        #region Slicer parameters
        private vtkRenderWindow m_SliceRenderWindow = null;
        private vtkRenderer m_SliceRenderer = null;
        private vtkImageActor m_SliceImageActor = null;
        private vtkImageClip m_SliceClip = null;
        #endregion

        #region Event monitor parameters
        private Kitware.VTK.vtkObject.vtkObjectEventHandler InteractorHandler = null;
        private Kitware.VTK.vtkInteractorStyleUser UserStyle = null;
        private Kitware.VTK.vtkObject.vtkObjectEventHandler UserHandler = null;
        private Kitware.VTK.vtkOutputWindow ErrorWindow = null;
        private Kitware.VTK.vtkObject.vtkObjectEventHandler ErrorHandler = null;
        #endregion

        #region Paltform
        // Constructor
        public MainPaltform()
        {
            InitializeComponent();
        }
        // Trigger hook events
        private void MainPaltform_Load(object sender, EventArgs e)
        {
            m_RenderWindow = renderWindowControl1.RenderWindow;
            m_Renderer = m_RenderWindow.GetRenderers().GetFirstRenderer();
            this.HookEvents();

        }

        // Release hook resource
        private void MainPaltform_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.UnhookEvents();
        }

        // Release resource
        private void MainPaltform_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (imgProp != null)
            {
                imgProp.Dispose();
            }
            if (this.renderWindowControl1 != null)
            {
                this.renderWindowControl1.Dispose();
            }
            if (this.renderWindowControl2 != null)
            {
                this.renderWindowControl2.Dispose();
            }
            if (this.Interactor != null)
            {
                this.Interactor.Dispose();
            }
            System.GC.Collect();
        }

        // Resize window
        private void MainPaltform_SizeChanged(object sender, EventArgs e)
        {
            // Render1: 459, 399 Panle2: 327, 399
            this.renderWindowControl1.Width = (int)(459.0 * this.Size.Width / 800);
            //this.renderWindowControl1.Height = (int)(399.0 * this.Size.Height / 600);

            this.panel2.Width = (int)(327.0 * this.Size.Width / 800);
            //this.panel2.Height = (int)(399.0 * this.Size.Height / 600);
        }

        #endregion

        #region File
        // Open file
        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Get the name of the file you want to open from the dialog 
                    m_FileName = openFileDialog.FileName;

                    //Look at known file types to see if they are readable
                    if (m_FileName.Contains(".png")
                        || m_FileName.Contains(".jpg")
                        || m_FileName.Contains(".jpeg")
                        || m_FileName.Contains(".tif")
                        || m_FileName.Contains(".slc")
                        || m_FileName.Contains(".dicom")
                        || m_FileName.Contains(".minc")
                        || m_FileName.Contains(".bmp")
                        || m_FileName.Contains(".pmn"))
                    {
                        RenderImage();
                    }
                    else if (m_FileName.Contains(".raw"))
                    {
                        RenderRaw();
                    }
                    //.vtk files need a DataSetReader instead of a ImageReader2
                    //some .vtk files need a different kind of reader, but this
                    //will read most and serve our purposes
                    else if (m_FileName.Contains(".vtk"))
                    {
                        RenderVTK();
                    }
                    else if (m_FileName.Contains(".vti"))
                    {
                        RenderVTI();
                    }
                    else if (m_FileName.Contains(".vtu"))
                    {
                        RenderVTU();
                    }
                    else if (m_FileName.Contains(".vtp"))
                    {
                        RenderVTP();
                    }
                    else
                    {
                        MessageBox.Show("Warning: not support the format file.\n" +
                                        "Support: image, .raw, .vtk, .vti, .vtu, .vtp\n");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

        }
        
        #region Render for file format
        private void RenderImage()
        {
            //Get rid of any props already there
            if (imgProp != null)
            {
                m_Renderer.RemoveActor(imgProp);
                imgProp.Dispose();
                imgProp = null;
            }

            Kitware.VTK.vtkImageReader2 rdr =
            Kitware.VTK.vtkImageReader2Factory.CreateImageReader2(m_FileName);
            rdr.SetFileName(m_FileName);
            rdr.Update();
            imgProp = vtkImageActor.New();
            ((vtkImageActor)imgProp).SetInput(rdr.GetOutput());
            rdr.Dispose();

            m_Renderer.AddActor(imgProp);
            //Reset the camera to show the image
            //Equivilant of pressing 'r'
            m_Renderer.ResetCamera();
            //Rerender the screen
            //NOTE: sometimes you have to drag the mouse
            //a little before the image shows up
            renderWindowControl1.RenderWindow.Render();
            m_Renderer.Render();

        }
        private void RenderRaw()
        {
            // Read the file
            vtkParticleReader reader = vtkParticleReader.New();
            reader.SetFileName(m_FileName);
            reader.SetDataByteOrderToBigEndian();
            reader.Update();
            //MessageBox.Show("NumberOfPieces: " + reader.GetOutput().GetNumberOfPieces());

            // Visualize
            vtkPolyDataMapper mapper = vtkPolyDataMapper.New();
            mapper.SetInputConnection(reader.GetOutputPort());
            mapper.SetScalarRange(4, 9);
            mapper.SetPiece(1);

            vtkActor actor = vtkActor.New();
            actor.SetMapper(mapper);
            actor.GetProperty().SetPointSize(4);
            actor.GetProperty().SetColor(1, 0, 0);
            // add our actor to the renderer
            m_Renderer.AddActor(actor);
        }
        private void RenderVTK()
        {
            //Get rid of any props already there
            if (imgProp != null)
            {
                m_Renderer.RemoveActor(imgProp);
                imgProp.Dispose();
                imgProp = null;
            }

            vtkDataSetReader dataReader = vtkDataSetReader.New();
            vtkDataSetMapper dataMapper = vtkDataSetMapper.New();
            imgProp = vtkActor.New();
            dataReader.SetFileName(m_FileName);
            dataReader.Update();
            dataMapper.SetInput(dataReader.GetOutput());
            ((vtkActor)imgProp).SetMapper(dataMapper);
            dataMapper.Dispose();
            dataMapper = null;
            dataReader.Dispose();
            dataReader = null;

            m_Renderer.AddActor(imgProp);
            //Reset the camera to show the image
            //Equivilant of pressing 'r'
            m_Renderer.ResetCamera();
            //Rerender the screen
            //NOTE: sometimes you have to drag the mouse
            //a little before the image shows up
            renderWindowControl1.RenderWindow.Render();
            m_Renderer.Render();

        }
        private void RenderVTI()
        {
            // reader
            // Read all the data from the file
            vtkXMLImageDataReader reader = vtkXMLImageDataReader.New();
            if (reader.CanReadFile(m_FileName) == 0)
            {
                MessageBox.Show("Cannot read file \"" + m_FileName + "\"", "Error", MessageBoxButtons.OK);
                return;
            }
            vtkVolume vol = vtkVolume.New();
            vtkColorTransferFunction ctf = vtkColorTransferFunction.New();
            vtkPiecewiseFunction spwf = vtkPiecewiseFunction.New();
            vtkPiecewiseFunction gpwf = vtkPiecewiseFunction.New();

            reader.SetFileName(m_FileName);
            reader.Update(); // here we read the file actually

            // mapper
            vtkFixedPointVolumeRayCastMapper mapper = vtkFixedPointVolumeRayCastMapper.New();
            mapper.SetInputConnection(reader.GetOutputPort());

            // actor
            vtkActor actor = vtkActor.New();
            //actor.SetMapper(mapper);
            actor.GetProperty().SetRepresentationToWireframe();

            // add our actor to the renderer
            //Set the color curve for the volume
            ctf.AddHSVPoint(0, .67, .07, 1);
            ctf.AddHSVPoint(94, .67, .07, 1);
            ctf.AddHSVPoint(139, 0, 0, 0);
            ctf.AddHSVPoint(160, .28, .047, 1);
            ctf.AddHSVPoint(254, .38, .013, 1);

            //Set the opacity curve for the volume
            spwf.AddPoint(84, 0);
            spwf.AddPoint(151, .1);
            spwf.AddPoint(255, 1);

            //Set the gradient curve for the volume
            gpwf.AddPoint(0, .2);
            gpwf.AddPoint(10, .2);
            gpwf.AddPoint(25, 1);

            vol.GetProperty().SetColor(ctf);
            vol.GetProperty().SetScalarOpacity(spwf);
            vol.GetProperty().SetGradientOpacity(gpwf);

            vol.SetMapper(mapper);

            //Go through the Graphics Pipeline
            m_Renderer.AddVolume(vol);
            m_Renderer.ResetCamera();
            //renderer.AddActor(actor);
            RenderSlicer();
        }
        private void RenderVTU()
        {
            // reader
            vtkXMLUnstructuredGridReader reader = vtkXMLUnstructuredGridReader.New();
            reader.SetFileName(m_FileName);
            reader.Update(); // here we read the file actually

            // mapper
            vtkDataSetMapper gridMapper = vtkDataSetMapper.New();
            gridMapper.SetInputConnection(reader.GetOutputPort());

            // actor
            vtkActor gridActor = vtkActor.New();
            gridActor.SetMapper(gridMapper);

            // add our actor to the renderer
            m_Renderer.AddActor(gridActor);

            // reposition the camera, so that actor can be fully seen
            m_Renderer.ResetCamera();
        }
        private void RenderVTP() 
        {
            // reader
            // Read all the data from the file
            vtkXMLPolyDataReader reader = vtkXMLPolyDataReader.New();
            reader.SetFileName(m_FileName);
            reader.Update(); // here we read the file actually

            // mapper
            vtkDataSetMapper mapper = vtkDataSetMapper.New();
            mapper.SetInputConnection(reader.GetOutputPort());

            // actor
            vtkActor actor = vtkActor.New();
            actor.SetMapper(mapper);

            // add our actor to the renderer
            m_Renderer.AddActor(actor);
            m_Renderer.ResetCamera();
        }
        #endregion

        #endregion

        #region View
        private void eventWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.eventWindowToolStripMenuItem.Checked = !this.eventWindowToolStripMenuItem.Checked;
            this.textEvents.Visible = this.eventWindowToolStripMenuItem.Checked;
        }
        
        private void dWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.dWindowToolStripMenuItem.Checked = !this.dWindowToolStripMenuItem.Checked;
            this.panel2.Visible = this.dWindowToolStripMenuItem.Checked;
            if (this.panel2.Visible)
            {
                this.renderWindowControl1.Dock = DockStyle.Left;
            }
            else
            {
                this.renderWindowControl1.Dock = DockStyle.Fill;
            }
        }

        #endregion

        #region Event monitor
        void ErrorWindow_ErrorHandler(Kitware.VTK.vtkObject sender, Kitware.VTK.vtkObjectEventArgs e)
        {
            string s = "unknown";
            if (e.CallData != IntPtr.Zero)
            {
                s = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(e.CallData);
            }

            System.Diagnostics.Debug.Write(System.String.Format(
              "ErrorWindow_ErrorHandler called: sender='{0}' e='{1}' s='{2}'", sender, e, s));
        }

        private void HookErrorWindowEvents()
        {
            if (null == this.ErrorWindow)
            {
                this.ErrorWindow = Kitware.VTK.vtkOutputWindow.GetInstance();
                this.ErrorHandler = new Kitware.VTK.vtkObject.vtkObjectEventHandler(ErrorWindow_ErrorHandler);

                this.ErrorWindow.ErrorEvt += this.ErrorHandler;
            }
        }

        public void HookEvents()
        {
            this.HookErrorWindowEvents();

            this.Interactor = this.renderWindowControl1.RenderWindow.GetInteractor();
            this.InteractorHandler = new Kitware.VTK.vtkObject.vtkObjectEventHandler(Interactor_AnyEventHandler);
            this.Interactor.AnyEvt += this.InteractorHandler;

            // Give our own style a higher priority than the built-in one
            // so that we see the events first:
            //
            float builtInPriority = this.Interactor.GetInteractorStyle().GetPriority();

            this.UserStyle = Kitware.VTK.vtkInteractorStyleUser.New();
            this.UserStyle.SetPriority(0.5f);
            this.UserStyle.SetInteractor(this.Interactor);

            this.UserHandler = new Kitware.VTK.vtkObject.vtkObjectEventHandler(UserStyle_MultipleEventHandler);

            // Keyboard events:
            this.UserStyle.KeyPressEvt += this.UserHandler;
            this.UserStyle.CharEvt += this.UserHandler;
            this.UserStyle.KeyReleaseEvt += this.UserHandler;
        }

        public void UnhookEvents()
        {
            this.UserStyle.KeyPressEvt -= this.UserHandler;
            this.UserStyle.CharEvt -= this.UserHandler;
            this.UserStyle.KeyReleaseEvt -= this.UserHandler;

            this.Interactor.AnyEvt -= this.InteractorHandler;

            this.UserHandler = null;
            this.UserStyle = null;
            this.InteractorHandler = null;
            this.Interactor = null;
        }

        void PrintEvent(Kitware.VTK.vtkObject sender, Kitware.VTK.vtkObjectEventArgs e)
        {
            int[] pos = this.Interactor.GetEventPosition();
            string keysym = this.Interactor.GetKeySym();
            sbyte keycode = this.Interactor.GetKeyCode();

            string line = String.Format("{0} ({1},{2}) ('{3}',{4}) {5} data='0x{6:x8}'{7}",
              Kitware.VTK.vtkCommand.GetStringFromEventId(e.EventId),
              pos[0], pos[1],
              keysym, keycode,
              e.Caller.GetClassName(), e.CallData.ToInt32(), System.Environment.NewLine);

            System.Diagnostics.Debug.Write(line);
            this.textEvents.AppendText(line);
        }

        void UserStyle_MultipleEventHandler(Kitware.VTK.vtkObject sender, Kitware.VTK.vtkObjectEventArgs e)
        {
            string keysym = this.Interactor.GetKeySym();

            Kitware.VTK.vtkCommand.EventIds eid = (Kitware.VTK.vtkCommand.EventIds)e.EventId;

            switch (eid)
            {
                case Kitware.VTK.vtkCommand.EventIds.KeyPressEvent:
                case Kitware.VTK.vtkCommand.EventIds.CharEvent:
                case Kitware.VTK.vtkCommand.EventIds.KeyReleaseEvent:
                    if (keysym == "f")
                    {
                        // Temporarily disable the interactor, so that the built-in 'f'
                        // handler does not get called:
                        //
                        this.Interactor.Disable();

                        // Turn on the timer, so we can re-enable the interactor
                        // after the processing of this event is over (one tenth
                        // of a second later...)
                        //
                        this.timer1.Enabled = true;
                    }
                    if (keysym.ToLower() == "j" || keysym.ToLower() == "k" || keysym.ToLower() == "l" || keysym.ToLower() == "i")
                    {
                        WanderCamera(keysym);
                    }
                    break;
            }

            this.PrintEvent(sender, e);
        }

        void Interactor_AnyEventHandler(Kitware.VTK.vtkObject sender, Kitware.VTK.vtkObjectEventArgs e)
        {
            this.PrintEvent(sender, e);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // Re-enable the interactor:
            //
            this.Interactor.Enable();

            // Disable the timer, so it's not continually firing:
            //
            this.timer1.Enabled = false;
        }

        #endregion

        #region Slicer control
        void RenderSlicer()
        {
            //Create all the objects for the pipeline
            vtkXMLImageDataReader reader = vtkXMLImageDataReader.New();
            vtkImageActor iactor = vtkImageActor.New();
            vtkImageClip clip = vtkImageClip.New();
            vtkContourFilter contour = vtkContourFilter.New();
            vtkPolyDataMapper mapper = vtkPolyDataMapper.New();
            vtkActor actor = vtkActor.New();
            vtkInteractorStyleImage style = vtkInteractorStyleImage.New();

            vtkRenderer renderer = renderWindowControl2.RenderWindow.GetRenderers().GetFirstRenderer();

            //Read the Image
            reader.SetFileName(m_FileName);

            //Go through the visulization pipeline
            iactor.SetInput(reader.GetOutput());
            renderer.AddActor(iactor);
            reader.Update();
            int[] extent = reader.GetOutput().GetWholeExtent();
            iactor.SetDisplayExtent(extent[0], extent[1], extent[2], extent[3],
                        (extent[4] + extent[5]) / 2,
                        (extent[4] + extent[5]) / 2);

            clip.SetInputConnection(reader.GetOutputPort());
            clip.SetOutputWholeExtent(extent[0], extent[1], extent[2], extent[3],
                        (extent[4] + extent[5]) / 2,
                        (extent[4] + extent[5]) / 2);

            contour.SetInputConnection(clip.GetOutputPort());
            contour.SetValue(0, 100);

            mapper.SetInputConnection(contour.GetOutputPort());
            mapper.SetScalarVisibility(1);

            //Go through the graphics pipeline
            actor.SetMapper(mapper);
            actor.GetProperty().SetColor(0, 1, 0);

            renderer.AddActor(actor);

            //Give a new style to the interactor
            //vtkRenderWindowInteractor iren = renderWindowControl2.RenderWindow.GetInteractor();
            //iren.SetInteractorStyle(style);


            //Update global variables
            this.trackBar1.Maximum = extent[5];
            this.trackBar1.Minimum = extent[4];
            //this.Interactor = iren;
            this.m_SliceRenderWindow = renderWindowControl2.RenderWindow;
            this.m_SliceRenderer = renderer;
            this.m_SliceClip = clip;
            this.m_SliceImageActor = iactor;

            renderer.ResetCamera();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (m_SliceImageActor != null)
            {
                int[] lastPos = this.Interactor.GetLastEventPosition();
                int[] size = this.m_SliceRenderWindow.GetSize();
                int[] dim = this.m_SliceImageActor.GetInput().GetDimensions();

                int newSlice = (int)(trackBar1.Value);

                if (newSlice >= 0 && newSlice < dim[2])
                {
                    this.m_SliceClip.SetOutputWholeExtent(0, dim[0] - 1, 0, dim[1] - 1, newSlice, newSlice);
                    this.m_SliceImageActor.SetDisplayExtent(0, dim[0] - 1, 0, dim[1] - 1, newSlice, newSlice);
                    this.m_SliceRenderer.ResetCameraClippingRange();
                    this.m_SliceRenderWindow.Render();
                }
            }
        }
        #endregion 

        #region Render window
        // Set background color
        private void backgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog1 = new System.Windows.Forms.ColorDialog();
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                m_Renderer.SetBackground(colorDialog1.Color.R / 255.0, colorDialog1.Color.G / 255.0, colorDialog1.Color.B / 255.0);
            }
        }
        // ResetCamera
        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (null != m_Renderer)
            {
                m_Renderer.ResetCamera();
                this.m_Renderer.ResetCameraClippingRange();
                this.m_RenderWindow.Render();
            }
        }
        // Set wander
        private void wanderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //camera.Zoom(10.0f); // front back
            //camera.Pitch(-10.0f); // up down
            //camera.Yaw(10.0f); // left right

            isWander = !isWander;
            this.wanderToolStripMenuItem.Checked = isWander;
            if (isWander)
            {
                m_Camera = m_Renderer.GetActiveCamera();
            }
        }
        // Wander in area
        private void WanderCamera(string dir)
        {
            if (isWander && null != m_Camera)
            {
                switch (dir)
                {
                    case "i":
                        m_Camera.Pitch(0.4f);
                        break;
                    case "k":
                        m_Camera.Pitch(-0.4f);
                        break;
                    case "j":
                        m_Camera.Yaw(0.4f);
                        break;
                    case "l":
                        m_Camera.Yaw(-0.4f);
                        break;
                    default:
                        break;
                }
                this.m_Renderer.ResetCameraClippingRange();
                this.m_RenderWindow.Render();
            }
        }
        #endregion

        #region Desiner
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.OpenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eventWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renderWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backgroundColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wanderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textEvents = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.renderWindowControl2 = new Kitware.VTK.RenderWindowControl();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.renderWindowControl1 = new Kitware.VTK.RenderWindowControl();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.renderWindowToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(792, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // OpenToolStripMenuItem
            // 
            this.OpenToolStripMenuItem.Name = "OpenToolStripMenuItem";
            this.OpenToolStripMenuItem.Size = new System.Drawing.Size(41, 20);
            this.OpenToolStripMenuItem.Text = "Open";
            this.OpenToolStripMenuItem.Click += new System.EventHandler(this.OpenToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.eventWindowToolStripMenuItem,
            this.dWindowToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(41, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // eventWindowToolStripMenuItem
            // 
            this.eventWindowToolStripMenuItem.Checked = true;
            this.eventWindowToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.eventWindowToolStripMenuItem.Name = "eventWindowToolStripMenuItem";
            this.eventWindowToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.eventWindowToolStripMenuItem.Text = "Event Window";
            this.eventWindowToolStripMenuItem.Click += new System.EventHandler(this.eventWindowToolStripMenuItem_Click);
            // 
            // dWindowToolStripMenuItem
            // 
            this.dWindowToolStripMenuItem.Checked = true;
            this.dWindowToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.dWindowToolStripMenuItem.Name = "dWindowToolStripMenuItem";
            this.dWindowToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.dWindowToolStripMenuItem.Text = "2D Window";
            this.dWindowToolStripMenuItem.Click += new System.EventHandler(this.dWindowToolStripMenuItem_Click);
            // 
            // renderWindowToolStripMenuItem
            // 
            this.renderWindowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.backgroundColorToolStripMenuItem,
            this.resetToolStripMenuItem,
            this.wanderToolStripMenuItem});
            this.renderWindowToolStripMenuItem.Name = "renderWindowToolStripMenuItem";
            this.renderWindowToolStripMenuItem.Size = new System.Drawing.Size(89, 20);
            this.renderWindowToolStripMenuItem.Text = "RenderWindow";
            // 
            // backgroundColorToolStripMenuItem
            // 
            this.backgroundColorToolStripMenuItem.Name = "backgroundColorToolStripMenuItem";
            this.backgroundColorToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.backgroundColorToolStripMenuItem.Text = "BackgroundColor";
            this.backgroundColorToolStripMenuItem.Click += new System.EventHandler(this.backgroundColorToolStripMenuItem_Click);
            // 
            // resetToolStripMenuItem
            // 
            this.resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            this.resetToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.resetToolStripMenuItem.Text = "ResetCamera";
            this.resetToolStripMenuItem.Click += new System.EventHandler(this.resetToolStripMenuItem_Click);
            // 
            // wanderToolStripMenuItem
            // 
            this.wanderToolStripMenuItem.Name = "wanderToolStripMenuItem";
            this.wanderToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.wanderToolStripMenuItem.Text = "Wander";
            this.wanderToolStripMenuItem.Click += new System.EventHandler(this.wanderToolStripMenuItem_Click);
            // 
            // textEvents
            // 
            this.textEvents.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textEvents.Location = new System.Drawing.Point(0, 423);
            this.textEvents.Multiline = true;
            this.textEvents.Name = "textEvents";
            this.textEvents.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textEvents.Size = new System.Drawing.Size(792, 143);
            this.textEvents.TabIndex = 5;
            this.textEvents.WordWrap = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.renderWindowControl1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 24);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(792, 399);
            this.panel1.TabIndex = 6;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.renderWindowControl2);
            this.panel2.Controls.Add(this.trackBar1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(465, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(327, 399);
            this.panel2.TabIndex = 1;
            // 
            // renderWindowControl2
            // 
            this.renderWindowControl2.AddTestActors = false;
            this.renderWindowControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.renderWindowControl2.Location = new System.Drawing.Point(0, 0);
            this.renderWindowControl2.Name = "renderWindowControl2";
            this.renderWindowControl2.Size = new System.Drawing.Size(327, 354);
            this.renderWindowControl2.TabIndex = 3;
            this.renderWindowControl2.TestText = null;
            // 
            // trackBar1
            // 
            this.trackBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.trackBar1.Location = new System.Drawing.Point(0, 354);
            this.trackBar1.Maximum = 100;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(327, 45);
            this.trackBar1.TabIndex = 4;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // renderWindowControl1
            // 
            this.renderWindowControl1.AddTestActors = false;
            this.renderWindowControl1.Dock = System.Windows.Forms.DockStyle.Left;
            this.renderWindowControl1.Location = new System.Drawing.Point(0, 0);
            this.renderWindowControl1.Name = "renderWindowControl1";
            this.renderWindowControl1.Size = new System.Drawing.Size(459, 399);
            this.renderWindowControl1.TabIndex = 0;
            this.renderWindowControl1.TestText = null;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // MainPaltform
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(792, 566);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.textEvents);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "MainPaltform";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "三维海洋数据可视化";
            this.Load += new System.EventHandler(this.MainPaltform_Load);
            this.SizeChanged += new System.EventHandler(this.MainPaltform_SizeChanged);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainPaltform_FormClosed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainPaltform_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem OpenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem eventWindowToolStripMenuItem;
        public System.Windows.Forms.TextBox textEvents;
        private System.Windows.Forms.Panel panel1;
        private Kitware.VTK.RenderWindowControl renderWindowControl1;
        public System.Windows.Forms.Panel panel2;
        private Kitware.VTK.RenderWindowControl renderWindowControl2;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripMenuItem dWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renderWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem backgroundColorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wanderToolStripMenuItem;

        #endregion

    }
    #region Plugin
    public class DataVisual : WorldWind.PluginEngine.Plugin
    {
        string basePath = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
        MainPaltform m_form = null;
        System.Windows.Forms.MenuItem m_menuItem = null;

        public override void Load()
        {
            m_menuItem = new System.Windows.Forms.MenuItem("3DDataVisualization");
            m_menuItem.Click += new EventHandler(m_menuItem_Click);
            ParentApplication.ToolsMenu.MenuItems.Add(m_menuItem);

            m_form = new MainPaltform();
            m_form.ClientSize = new System.Drawing.Size(800, 600);
            m_form.Location = new System.Drawing.Point(0, 400);



            base.Load();
        }

        public override void Unload()
        {
            if (m_form != null)
            {
                m_form.Dispose();
                m_form = null;
            }

            ParentApplication.ToolsMenu.MenuItems.Remove(m_menuItem);

            base.Unload();
        }

        void m_menuItem_Click(object sender, EventArgs e)
        {
            m_menuItem.Checked = !m_menuItem.Checked;
            if (m_form != null)
            {
                m_form.Visible = m_menuItem.Checked;
            }
            else
            {
                m_form = new MainPaltform();
                m_form.Visible = m_menuItem.Checked;
            }
        }
    }
    #endregion

}