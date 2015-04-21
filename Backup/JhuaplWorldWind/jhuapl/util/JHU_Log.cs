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
using System.IO;
using System.Text;
using Utility;

namespace jhuapl.util
{

	/// <summary>
	/// Debug log functionality.  Writes output to a CSV file.
	/// 
	/// Each entry is of the format:
	/// <list>
	/// <listheader><term>Column</term><description>Description</description></listheader>
	/// <item><term>Timestamp</term><description>When log entry was entered.</description></item>
	/// <item><term>Level</term><description>This is the entry level.  Used to filter for speed.  If the level is greater than the current logging level it is dropped</description></item>
	/// <item><term>Category</term><description>This is a 4 character category string for filtering.</description></item>
	/// <item><term>Latitude</term><description>This is the latitude recorded for this entry.</description></item>
	/// <item><term>Longitude</term><description>This is the longitude recorded for this entry.</description></item>
	/// <item><term>Altitude</term><description>This is the altitude recorded for this entry.</description></item>
	/// <item><term>Message></term><description>This is the test message of the entry.  Try not to use commas in here.</description></item>
	/// </list>
	/// </summary>
	public class JHU_Log
	{
		static StreamWriter m_logWriter;
		static string m_logFileName = "";
		static string m_logPath = "";
		static string m_logFilePath = "";
		static int m_logLevel = 0;
		static bool m_autoIndent = true;

		/// <summary>
		/// Severity - not currently used
		/// </summary>
		public enum Severity
		{
			UNK,
			SUCC,
			INFO,
			WARN,
			ERR,
			FAIL
		}

		public static int LogLevel
		{
			get { return m_logLevel; }
			set { m_logLevel = value; }
		}

		public static string LogPath
		{
			get
			{
				if (m_logPath.Trim() == "")
					return JHU_Globals.getInstance().BasePath + "\\log";
				else
					return m_logPath;
			}
			set { m_logPath = value; }
		}

		public static bool AutoIndent
		{
			get { return m_autoIndent; }
			set { m_autoIndent = value; }
		}

		/// <summary>
		/// Static class (Only static members)
		/// </summary>
		private JHU_Log()
		{}

		/// <summary>
		/// Static constructor.
		/// </summary>
		static JHU_Log()
		{
		}

		/// <summary>
		/// If there was an open log file it closes it. 
		/// </summary>
		public static void Close()
		{
			if (m_logWriter != null)
			{
				m_logWriter.Close();
				m_logWriter = null;
			}
		}

		/// <summary>
		/// Opens a new log file.  Closes any old ones.
		/// </summary>
		/// <returns>if the open was successful.</returns>
		public static bool Open()
		{
			bool status = true;
			lock (typeof(JHU_Log))
			{
				try
				{
					// if we have been opened close us
					Close();

					// if the directory doesn't exist create it
					Directory.CreateDirectory(LogPath);

					if (m_logFileName.Trim() == "")
						m_logFileName = "CollabSpace";

					string name = m_logFileName;

					name += "-" + DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss") + ".csv"; 

					m_logFilePath = Path.Combine( LogPath, name );

					m_logWriter = new StreamWriter(m_logFilePath, true);
					m_logWriter.AutoFlush = true;

					string logLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}{8}",
						"Timestamp (UTC)",
						"Category",
						"Level",
						"Lat",
						"Lon",
						"Alt",
						"Name",
						"[Optional Indenting] ", 
						"Message");
					m_logWriter.WriteLine( logLine );
				}
				catch (Exception caught)
				{
					// Log an error to the World Wind log - might rethrow an error if it can't log.
					Log.Write(caught);
					status = false;
				}
			}
			return status;
		}

		/// <summary>
		/// Opens a new log file.  Closes any old ones.
		/// </summary>
		/// <param name="name">The name of the file to open.  A timestamp and .csv suffix is always appended</param>
		/// <returns>if the open was successful.</returns>
		public static bool Open(string name)
		{
			m_logFileName = name; 

			return Open();
		}

		/// <summary>
		/// Logs a message to the CollabSpace log
		/// </summary>
		/// <param name="level">The log level for this message.  If greater than the 
		/// current logging level it gets dropped on the floor.</param>
		/// <param name="category">1 to 4 character long tag for categorizing the log entries.
		/// If the category is longer than 4 characters it will be clipped.</param>
		/// <param name="lat">The latitude for this log item</param>
		/// <param name="lon">The longitude for this log item</param>
		/// <param name="alt">The altitude for this log item</param>
		/// <param name="name">The name of the object most relevant to his log message</param>
		/// <param name="message">The actual log messages to be written.</param>
		public static void Write(int level, string category, double lat, double lon, double alt, string name, string message )
		{
			if (level <= m_logLevel)
			{
				if (m_logWriter == null)
				{
					// open a new file.  Will have same name as previous if any with a new timestamp
					Open();
				}

				try
				{
					lock(m_logWriter)
					{
						StringBuilder indent = new StringBuilder("");

						if (m_autoIndent)
						{
							for (int i = 0; i < level; i++)
							{
								indent.Append("\t");
							}
						}

						string logLine = string.Format("{0},{1},{2:00},{3:r},{4:r},{5:r},{6},{7}{8}",
							DateTime.UtcNow.ToString("u"),
							category.PadRight(4,' ').Substring(0,4),
							level,
							lat,
							lon,
							alt,
							name,
							indent.ToString(), 
							message );
						m_logWriter.WriteLine( logLine );
					}
				}
				catch (Exception caught)
				{
					// Log an error to the World Wind log - might rethrow an error if it can't log.
					Log.Write(caught);
				}
			}
		}
		
		/// <summary>
		/// Logs a message to theCollabSpace log as an UNK category and 0 level and no LLA.
		/// </summary>
		/// <param name="message">The actual log messages to be written.</param>
		public static void Write( string message )
		{
			Write( 0, "UNK", 0.0, 0.0, 0.0, "UNK", message );
		}

		/// <summary>
		/// Logs a message to the CollabSpace log with specified category and level but zero'd LLA
		/// </summary>
		/// <param name="level">The log level for this message.  If greater than the 
		/// current logging level it gets dropped on the floor.</param>
		/// <param name="category">1 to 4 character long tag for categorizing the log entries.
		/// If the category is longer than 4 characters it will be clipped.</param>
		/// <param name="message">The actual log messages to be written.</param>
		public static void Write( int level, string category, string name, string message )
		{
			Write( level, category, 0.0, 0.0, 0.0, name, message );
		}
	}
}
