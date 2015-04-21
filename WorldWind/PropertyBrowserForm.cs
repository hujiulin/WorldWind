using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace WorldWind
{
	/// <summary>
	/// Property browser/edit dialog for debugging.
	/// </summary>
	public class PropertyBrowserForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.PropertyGrid propertyGrid;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.RapidFireModisManager"/> class.
		/// </summary>
		/// <param name="selected">The object to display in the browser.</param>
		/// <param name="title">Dialog title.</param>
		public PropertyBrowserForm( object selected, string title )
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			propertyGrid.SelectedObject = selected;
			this.Text = title;
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.propertyGrid = new System.Windows.Forms.PropertyGrid();
			this.SuspendLayout();
			// 
			// propertyGrid
			// 
			this.propertyGrid.CommandsVisibleIfAvailable = true;
			this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.propertyGrid.LargeButtons = false;
			this.propertyGrid.LineColor = System.Drawing.SystemColors.ScrollBar;
			this.propertyGrid.Location = new System.Drawing.Point(0, 0);
			this.propertyGrid.Name = "propertyGrid";
			this.propertyGrid.Size = new System.Drawing.Size(344, 326);
			this.propertyGrid.TabIndex = 0;
			this.propertyGrid.Text = "PropertyGrid";
			this.propertyGrid.ViewBackColor = System.Drawing.SystemColors.Window;
			this.propertyGrid.ViewForeColor = System.Drawing.SystemColors.WindowText;
			// 
			// PropertyBrowserForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(344, 326);
			this.Controls.Add(this.propertyGrid);
			this.KeyPreview = true;
			this.Name = "PropertyBrowserForm";
			this.ResumeLayout(false);

		}
		#endregion
	}
}
