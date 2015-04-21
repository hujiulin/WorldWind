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

namespace jhuapl.util
{
	/// <summary>
	/// Summary description for WidgetCollection.
	/// </summary>
	public class JHU_WidgetCollection : jhuapl.util.IWidgetCollection
	{
		System.Collections.ArrayList m_ChildWidgets = new System.Collections.ArrayList();
		
		public JHU_WidgetCollection()
		{
		}

		#region Methods
		public void BringToFront(int index)
		{
			jhuapl.util.IWidget currentWidget = m_ChildWidgets[index] as jhuapl.util.IWidget;
			if(currentWidget != null)
			{
				m_ChildWidgets.RemoveAt(index);
				m_ChildWidgets.Insert(0, currentWidget);
			}
		}

		public void BringToFront(jhuapl.util.IWidget widget)
		{
			int foundIndex = -1;

			for(int index = 0; index < m_ChildWidgets.Count; index++)
			{
				jhuapl.util.IWidget currentWidget = m_ChildWidgets[index] as jhuapl.util.IWidget;
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

		public void Add(jhuapl.util.IWidget widget)
		{
			m_ChildWidgets.Add(widget);
		}

		public void Clear()
		{
			m_ChildWidgets.Clear();
		}

		public void Insert(jhuapl.util.IWidget widget, int index)
		{
			if(index <= m_ChildWidgets.Count)
			{
				m_ChildWidgets.Insert(index, widget);
			}
			//probably want to throw an indexoutofrange type of exception
		}

		public jhuapl.util.IWidget RemoveAt(int index)
		{
			if(index < m_ChildWidgets.Count)
			{
				jhuapl.util.IWidget oldWidget = m_ChildWidgets[index] as jhuapl.util.IWidget;
				m_ChildWidgets.RemoveAt(index);
				return oldWidget;
			}
			else
			{
				return null;
			}
		}

		public void Remove(jhuapl.util.IWidget widget)
		{
			int foundIndex = -1;

			for(int index = 0; index < m_ChildWidgets.Count; index++)
			{
				jhuapl.util.IWidget currentWidget = m_ChildWidgets[index] as jhuapl.util.IWidget;
				if(currentWidget != null)
				{		
					if(currentWidget == widget)
					{
						foundIndex = index;
						break;
					}
				}	
			}

			if(foundIndex >= 0)
			{
				m_ChildWidgets.RemoveAt(foundIndex);
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
		public jhuapl.util.IWidget this[int index]
		{
			get
			{
				return m_ChildWidgets[index] as jhuapl.util.IWidget;
			}
			set
			{
				m_ChildWidgets[index] = value;
			}
		}
		#endregion

	}
}
