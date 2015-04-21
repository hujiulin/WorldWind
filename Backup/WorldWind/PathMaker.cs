using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System;
using WorldWind;
using WorldWind.Renderable;
using Utility;

namespace WorldWind
{
	/// <summary>
	/// Summary description for PathMaker.
	/// </summary>
	public class PathMaker : System.Windows.Forms.Form
	{
		System.Timers.Timer timer;
		PathLine curPathLine;

		Microsoft.DirectX.Vector2 lastPosition;
		float curHeight;

		System.Collections.ArrayList curPathList = new ArrayList();

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button buttonStart;
		private System.Windows.Forms.Button buttonStop;
		private System.Windows.Forms.Button buttonSave;
		private WorldWindow worldWindow;
		string saveDirPath = Path.Combine(MainApplication.DirectoryPath, Path.Combine("Data","User"));
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textBoxName;
		private System.Windows.Forms.NumericUpDown numericUpDownHeight;
		private System.Windows.Forms.ListBox listBoxPaths;
		private System.Windows.Forms.NumericUpDown numericUpDownFrequency;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.PathMaker"/> class.
		/// </summary>
		/// <param name="ww"></param>
		public PathMaker(WorldWindow ww)
		{
			InitializeComponent();

			this.worldWindow = ww;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			e.Cancel = true;
			this.Visible = false;
			this.worldWindow.Focus();
			base.OnClosing (e);
		}

		protected override void OnKeyUp(System.Windows.Forms.KeyEventArgs e) 
		{
			switch(e.KeyCode) 
			{
				case Keys.Escape:
					Close();
					e.Handled = true;
					break;
				case Keys.F4:
					if(e.Modifiers==Keys.Control)
					{
						Close();
						e.Handled = true;
					}
					break;
			}

			base.OnKeyUp(e);
		}
		
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.textBoxName = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.buttonStart = new System.Windows.Forms.Button();
			this.buttonStop = new System.Windows.Forms.Button();
			this.buttonSave = new System.Windows.Forms.Button();
			this.numericUpDownFrequency = new System.Windows.Forms.NumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			this.numericUpDownHeight = new System.Windows.Forms.NumericUpDown();
			this.label3 = new System.Windows.Forms.Label();
			this.listBoxPaths = new System.Windows.Forms.ListBox();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownFrequency)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownHeight)).BeginInit();
			this.SuspendLayout();
			// 
			// textBoxName
			// 
			this.textBoxName.Location = new System.Drawing.Point(73, 23);
			this.textBoxName.Name = "textBoxName";
			this.textBoxName.Size = new System.Drawing.Size(200, 20);
			this.textBoxName.TabIndex = 0;
			this.textBoxName.Text = "";
			this.textBoxName.TextChanged += new System.EventHandler(this.textBoxName_TextChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(17, 26);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(40, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "Name";
			// 
			// buttonStart
			// 
			this.buttonStart.Location = new System.Drawing.Point(32, 208);
			this.buttonStart.Name = "buttonStart";
			this.buttonStart.TabIndex = 2;
			this.buttonStart.Text = "Start";
			this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
			// 
			// buttonStop
			// 
			this.buttonStop.Location = new System.Drawing.Point(120, 208);
			this.buttonStop.Name = "buttonStop";
			this.buttonStop.TabIndex = 3;
			this.buttonStop.Text = "Stop";
			this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
			// 
			// buttonSave
			// 
			this.buttonSave.Location = new System.Drawing.Point(208, 208);
			this.buttonSave.Name = "buttonSave";
			this.buttonSave.TabIndex = 5;
			this.buttonSave.Text = "Save";
			// 
			// numericUpDownFrequency
			// 
			this.numericUpDownFrequency.Location = new System.Drawing.Point(16, 56);
			this.numericUpDownFrequency.Maximum = new System.Decimal(new int[] {
																										 30,
																										 0,
																										 0,
																										 0});
			this.numericUpDownFrequency.Minimum = new System.Decimal(new int[] {
																										 1,
																										 0,
																										 0,
																										 0});
			this.numericUpDownFrequency.Name = "numericUpDownFrequency";
			this.numericUpDownFrequency.Size = new System.Drawing.Size(56, 20);
			this.numericUpDownFrequency.TabIndex = 6;
			this.numericUpDownFrequency.Value = new System.Decimal(new int[] {
																									  10,
																									  0,
																									  0,
																									  0});
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(72, 56);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(192, 23);
			this.label2.TabIndex = 7;
			this.label2.Text = "Points / second";
			// 
			// numericUpDownHeight
			// 
			this.numericUpDownHeight.Increment = new System.Decimal(new int[] {
																										100,
																										0,
																										0,
																										0});
			this.numericUpDownHeight.Location = new System.Drawing.Point(16, 80);
			this.numericUpDownHeight.Maximum = new System.Decimal(new int[] {
																									 100000000,
																									 0,
																									 0,
																									 0});
			this.numericUpDownHeight.Name = "numericUpDownHeight";
			this.numericUpDownHeight.Size = new System.Drawing.Size(56, 20);
			this.numericUpDownHeight.TabIndex = 8;
			this.numericUpDownHeight.Value = new System.Decimal(new int[] {
																								  100,
																								  0,
																								  0,
																								  0});
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(72, 80);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(192, 23);
			this.label3.TabIndex = 9;
			this.label3.Text = "Distance above surface (m)";
			// 
			// listBoxPaths
			// 
			this.listBoxPaths.Location = new System.Drawing.Point(16, 104);
			this.listBoxPaths.Name = "listBoxPaths";
			this.listBoxPaths.Size = new System.Drawing.Size(264, 95);
			this.listBoxPaths.TabIndex = 10;
			this.listBoxPaths.KeyUp += new System.Windows.Forms.KeyEventHandler(this.listBoxPaths_KeyUp);
			// 
			// PathMaker
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 243);
			this.Controls.Add(this.listBoxPaths);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.numericUpDownHeight);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.numericUpDownFrequency);
			this.Controls.Add(this.buttonSave);
			this.Controls.Add(this.buttonStop);
			this.Controls.Add(this.buttonStart);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.textBoxName);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.KeyPreview = true;
			this.Name = "PathMaker";
			this.Text = "PathMaker";
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownFrequency)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownHeight)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion


		private int getAvailableLineNumber()
		{
			this.listBoxPaths.Sorted = true;

			if(this.listBoxPaths.Items.Count > 0)
			{
				string lastNumber = this.listBoxPaths.Items[this.listBoxPaths.Items.Count-1] as string;
				int nextNumber = Int32.Parse(lastNumber.Split('.')[0], CultureInfo.InvariantCulture) + 1;
				return nextNumber;
			}
			else
				return 0;
		}

		private void buttonStart_Click(object sender, System.EventArgs e)
		{
			try
			{
				this.listBoxPaths.Enabled = false;
				this.textBoxName.Enabled = false;
				this.buttonStop.Enabled = true;
				this.buttonStart.Enabled = false;
				this.buttonSave.Enabled = false;
				this.numericUpDownHeight.Enabled = false;
				this.numericUpDownFrequency.Enabled = false;

				World.Settings.ShowCrosshairs = true;

				this.curPathLine = new PathLine(this.textBoxName.Text + " - " + this.getAvailableLineNumber() + ".wwb",
					this.worldWindow.CurrentWorld,
					null,
					(float)this.numericUpDownHeight.Value,
					System.Drawing.Color.Red);

				this.worldWindow.CurrentWorld.RenderableObjects.Add(this.curPathLine);
				this.curPathLine.IsOn = true;

				this.curHeight = (float)this.numericUpDownHeight.Value;

				this.lastPosition = new Microsoft.DirectX.Vector2((float)this.worldWindow.DrawArgs.WorldCamera.Latitude.Degrees, (float)this.worldWindow.DrawArgs.WorldCamera.Longitude.Degrees);
				this.curPathLine.AddPointToPath(this.lastPosition.X, this.lastPosition.Y, false, (float)this.numericUpDownHeight.Value);

				if(this.timer == null)
				{
					this.timer = new System.Timers.Timer((double)this.numericUpDownFrequency.Value);
					this.timer.Elapsed+=new System.Timers.ElapsedEventHandler(timer_Elapsed);
				}
				else
				{
					this.timer.Interval = (double)this.numericUpDownFrequency.Value;
				}
				this.timer.Start();
			}
			catch(Exception caught)
			{
				Log.Write(caught);
			}
		}

		private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			Microsoft.DirectX.Vector2 curPosition = new Microsoft.DirectX.Vector2((float)this.worldWindow.DrawArgs.WorldCamera.Latitude.Degrees, (float)this.worldWindow.DrawArgs.WorldCamera.Longitude.Degrees);

			if(this.curPathLine != null && this.lastPosition != curPosition)
				this.curPathLine.AddPointToPath(curPosition.X, curPosition.Y, false, this.curHeight);
		}

		private void buttonStop_Click(object sender, System.EventArgs e)
		{
			try
			{
				this.timer.Stop();
				string saveFileName = this.saveDirPath + "/" + this.textBoxName.Text + "/" + this.getAvailableLineNumber() + ".wwb";
				if(this.curPathLine != null)
				{
					this.curPathLine.SaveToFile(saveFileName);

					if(File.Exists(saveFileName))
						this.listBoxPaths.Items.Add(Path.GetFileName(saveFileName));

					this.curPathList.Add(this.curPathLine);
					this.curPathLine = null;
				}
				this.listBoxPaths.Enabled = true;
				this.textBoxName.Enabled = true;
				this.buttonStart.Enabled = true;
				this.buttonStop.Enabled = false;
				this.buttonSave.Enabled = true;
				this.numericUpDownHeight.Enabled = true;
				this.numericUpDownFrequency.Enabled = true;
			}
			catch(Exception caught)
			{
				Log.Write(caught);
			}
		}

		private void textBoxName_TextChanged(object sender, System.EventArgs e)
		{
			if(this.curPathList.Count > 0)
			{
				foreach(PathLine pl in this.curPathList)
				{
					this.worldWindow.CurrentWorld.RenderableObjects.Remove(pl.Name);
					pl.Dispose();
				}

				this.curPathList.Clear();
			}

			this.listBoxPaths.Items.Clear();
			DirectoryInfo inDir = new DirectoryInfo(this.saveDirPath + "/" + this.textBoxName.Text);

			if(inDir.Exists)
			{
				foreach(FileInfo file in inDir.GetFiles("*.wwb"))
				{
					this.listBoxPaths.Items.Add(file.Name);
					PathLine newPathLine = new PathLine(this.textBoxName.Text + " - " + file.Name, this.worldWindow.CurrentWorld, file.FullName, (float)this.numericUpDownHeight.Value, System.Drawing.Color.Red);
					newPathLine.IsOn = true;
					this.curPathList.Add(newPathLine);
					this.worldWindow.CurrentWorld.RenderableObjects.Add(newPathLine);
				}
			}
		}

		private void listBoxPaths_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if(e.KeyCode == System.Windows.Forms.Keys.Delete)
			{
				if(this.listBoxPaths.SelectedItem != null)
				{
					string fileName = this.saveDirPath + "/" + this.textBoxName.Text + "/" + this.listBoxPaths.SelectedItem as string;
					try
					{
						this.worldWindow.CurrentWorld.RenderableObjects.Remove(this.textBoxName.Text + " - " + this.listBoxPaths.SelectedItem as string);
						File.Delete(fileName);
					}
					catch
					{
					}
					this.listBoxPaths.Items.RemoveAt(this.listBoxPaths.SelectedIndex);
				}
			}
		}
	}
}
