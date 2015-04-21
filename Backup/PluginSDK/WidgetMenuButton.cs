using System;
using System.Collections.Generic;
using System.Text;

using WorldWind.Menu;

namespace WorldWind
{
    public class WidgetMenuButton : MenuButton
    {
        WorldWind.Widgets.IWidget m_widget = null;

        public WidgetMenuButton(
			string name,
			string iconFilePath,
			WorldWind.Widgets.IWidget widget) : base(iconFilePath)
		{
			this.Description = name;
            m_widget = widget;
		}

        public override void Update(DrawArgs drawArgs)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override bool IsPushed()
        {
            return m_widget.Visible;
        }

        public override void SetPushed(bool isPushed)
        {
            m_widget.Visible = isPushed;
        }

        public override bool OnMouseWheel(System.Windows.Forms.MouseEventArgs e)
        {
            return false;
        }

        public override void OnKeyDown(System.Windows.Forms.KeyEventArgs keyEvent)
        {
        }

        public override void OnKeyUp(System.Windows.Forms.KeyEventArgs keyEvent)
        {
        }

        public override bool OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            return false;
        }

        public override bool OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            return false;
        }

        public override bool OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            return false;
        }

        public override void Render(DrawArgs drawArgs)
        {
            if (!m_widget.Visible)
            {
                SetPushed(false);
            }
        }
    }
}
