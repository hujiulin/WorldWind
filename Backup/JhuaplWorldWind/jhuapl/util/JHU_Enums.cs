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
// Copyright (c) 2006 The Johns Hopkins University. 
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
	/// Summary description for JHU_Enums.
	/// </summary>
	public class JHU_Enums
	{
		/// <summary>
		/// MIL-STD 2525B Affiliation - 1 char (pos 2)
		/// Affiliation - the threat posed by the warfighting object being represented. The
		/// basic affiliation categories are pending (P), unknown (U), friend (F), neutral (N), hostile (H),
		/// Assumed friend (A), Suspect (S), Joker (J), Faker(K)
		/// 
		/// AKA Link Identity Type
		/// 
		/// </summary>
		public enum Affiliations
		{
			PENDING,
			UNKNOWN,
			ASSUMED_FRIEND,
			FRIEND,
			NEUTRAL,
			SUSPECT,
			HOSTILE,
			EXERCISE_PENDING,
			EXERCISE_UNKNOWN,
			EXERCISE_ASSUMED_FRIEND,
			EXERCISE_FRIEND,
			EXERCISE_NEUTRAL,
			JOKER,
			FAKER
		}

		/// <summary>
		/// MIL-STD 2525B Battle Dimension - 1 char (pos 3)
		/// Battle Dimension - Battle dimension defines the primary mission area for the
		/// warfighting object within the battlespace.  Unknown (Z), Space (P), Air (A), Ground (G), 
		/// Sea Surface (S), Sub-Surface (U), SOF (F) 
		/// 
		/// AKA Link Environment Category
		/// 
		/// </summary>
		public enum BattleDimensions
		{
			UNKNOWN,
			SPACE,
			AIR,
			GROUND,
			SEA_SURFACE,
			SEA_SUBSURFACE,
			SOF,
			OTHER
		}

		/// <summary>
		/// Widget Anchor Styles.  Same values as Forms AnchorStyles
		/// </summary>
		[Flags]
		public enum AnchorStyles
		{
			None = 0x0000,
			Top = 0x0001,
			Bottom = 0x0002,
			Left = 0x0004,
			Right = 0x0008,
		}

		/// <summary>
		/// Default constructor - does nada
		/// </summary>
		public JHU_Enums()
		{
		}
	}
}
