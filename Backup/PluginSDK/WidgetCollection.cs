using System;

namespace WorldWind.Widgets
{
	/// <summary>
	/// Summary description for WidgetCollection.
	/// </summary>
	public class WidgetCollection : IWidgetCollection
	{
		System.Collections.ArrayList m_ChildWidgets = new System.Collections.ArrayList();
		
		public WidgetCollection()
		{

		}

		#region Methods
		public void BringToFront(int index)
		{
			IWidget currentWidget = m_ChildWidgets[index] as IWidget;
			if(currentWidget != null)
			{
				m_ChildWidgets.RemoveAt(index);
				m_ChildWidgets.Insert(0, currentWidget);
			}
		}

		public void BringToFront(IWidget widget)
		{
			int foundIndex = -1;

			for(int index = 0; index < m_ChildWidgets.Count; index++)
			{
				IWidget currentWidget = m_ChildWidgets[index] as IWidget;
				if(currentWidget != null)
				{		
					if(currentWidget == widget)
					{
						foundIndex = index;
						break;
					}
				}	
			}

			if(foundIndex > 0)
			{
				BringToFront(foundIndex);
			}	
		}

		public void Add(IWidget widget)
		{
			m_ChildWidgets.Add(widget);
		}

		public void Clear()
		{
			m_ChildWidgets.Clear();
		}

		public void Insert(IWidget widget, int index)
		{
			if(index <= m_ChildWidgets.Count)
			{
				m_ChildWidgets.Insert(index, widget);
			}
			//probably want to throw an indexoutofrange type of exception
		}

		public IWidget RemoveAt(int index)
		{
			if(index < m_ChildWidgets.Count)
			{
				IWidget oldWidget = m_ChildWidgets[index] as IWidget;
				m_ChildWidgets.RemoveAt(index);
				return oldWidget;
			}
			else
			{
				return null;
			}
		}
		#endregion

		#region Properties
		public int Count
		{
			get
			{
				return m_ChildWidgets.Count;
			}
		}
		#endregion

		#region Indexers
		public IWidget this[int index]
		{
			get
			{
				return m_ChildWidgets[index] as IWidget;
			}
			set
			{
				m_ChildWidgets[index] = value;
			}
		}
		#endregion

	}
}
