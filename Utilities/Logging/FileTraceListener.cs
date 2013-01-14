///////////////////////////////////////////////////////////////////////////////////////////////////
// Project      -   Utilities.Logging
// General      -   Set of generic classes that may be used for logging in any project.
//
// File Name    -   FileTraceListener.cs
// File Title   -   File Trace Listener
// Description  -   A custom trace listener that writes log entries to a text file.
// Notes        -   Similar to the built-in TextWriterTraceListener class except that it allows the 
//					format of the text that is written to be specified.
//
// $History: FileTraceListener.cs $
// 
// *****************  Version 5  *****************
// User: Simone       Date: 5/12/08    Time: 12:17p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.Logging
// Bug Fix: Any path information was being stripped out of _logFileName.
// Removed property FullFileName.  Now application path will be prepended
// to _logFileName if the value supplied has no path or a relative path.
// Method GetFullFileName renamed GetLogFileNameWithPath.
// 
// *****************  Version 4  *****************
// User: Simone       Date: 2/12/08    Time: 5:02p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.Logging
// GetFullFileName: Calls new method GetDefaultFileName if the log
// filename has not been supplied.
// 
// *****************  Version 3  *****************
// User: Simone       Date: 2/12/08    Time: 12:27p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.Logging
// WriteToCustomLog: When adding date suffix use new ReplaceLast method
// instead of string.Replace.  To handle paths that contain "." - Only
// want to replace the "." before the file extension, not every "." in the
// path.
// 
// *****************  Version 1  *****************
// User: Simone       Date: 13/02/08   Time: 3:58p
// Created in $/UtilitiesClassLibrary/UtilitiesClassLibrary/Utilities.Logging
// 
// *****************  Version 1  *****************
// User: Simone       Date: 13/02/08   Time: 11:09a
// Created in $/UtilitiesClassLibrary/Utilities.Logging
// Utilities.Logging split out of original Utilities project.  Class file
// moved from Utilities project unchanged.
// 
// *****************  Version 1  *****************
// User: Simone       Date: 24/10/07   Time: 12:11p
// Created in $/UtilitiesClassLibrary/Utilities
// Copied from version 3 in
// $/ServiceAlliance/JobsByEmail/JobsByEmailGeneric/EmailParser/Utilities.
///////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace Utilities.Logging
{
	/// <summary>
	/// Custom trace listener that writes log entries to a text file.
	/// </summary>
	public class FileTraceListener : CustomFormattedTextTraceListener
	{
		#region Class Variables *******************************************************************

		private string _logFileName = null;			// The file name of the log file that will be 
		//	written to.  May be just the file name or 
		//	may include path (rooted or partial) as 
		//	well.  Excludes any date suffix.
		private bool _fileNameIncludesDate = false;	// Add a date suffix to the log file name. 
		//	eg MyLogFile_20070219.log

		// The following lists are used to determine which log entries should be emphasized.  
		//	eg Emphasize all log entries with categories Critical or Error.
		private List<string> _categoriesToEmphasize = new List<string>();
		private List<string> _sourcesToEmphasize = new List<string>();
		private List<string> _methodsToEmphasize = new List<string>();
		private List<int> _eventIDsToEmphasize = new List<int>();
		private List<int> _processIDsToEmphasize = new List<int>();
		private List<string> _threadIDsToEmphasize = new List<string>();

		#endregion

		#region Constructors and Destructors ******************************************************

		public FileTraceListener(string LogFileName)
			: this(null, LogFileName)
		{
		}

		public FileTraceListener(string appName, string LogFileName)
			: base(appName)
		{
			_logFileName = this.GetLogFileNameWithPath(LogFileName);
		}

		#endregion

		#region Properties ************************************************************************

		/// <summary>
		/// The file name of the log file that will be written to.  Excludes any date suffix to 
		/// the log filename.  
		/// </summary>
		/// <remarks>If the filename supplied does not include a path, or only includes a relative 
		/// path, the path to the application folder will be prepended to the filename.</remarks>
		public string FileName
		{
			get { return _logFileName; }
			set
			{
				_logFileName = this.GetLogFileNameWithPath(value);
			}
		}

		/// <summary>
		/// Add a date suffix to the log file name.  eg MyLogFile_20070219.log
		/// </summary>
		public bool FileNameIncludesDate
		{
			get { return _fileNameIncludesDate; }
			set { _fileNameIncludesDate = value; }
		}

		/// <summary>
		/// Categories (aka EventTypes) that will be emphasized or highlighted in the log file. 
		/// </summary>
		public List<string> CategoriesToEmphasize
		{
			get { return _categoriesToEmphasize; }
			set { _categoriesToEmphasize = value; }
		}

		/// <summary>
		/// Sources that will be emphasized or highlighted in the log file. 
		/// </summary>
		public List<string> SourcesToEmphasize
		{
			get { return _sourcesToEmphasize; }
			set { _sourcesToEmphasize = value; }
		}

		/// <summary>
		/// Messages written from specified methods will be emphasized or highlighted in the log 
		/// file. 
		/// </summary>
		public List<string> MethodsToEmphasize
		{
			get { return _methodsToEmphasize; }
			set { _methodsToEmphasize = value; }
		}

		/// <summary>
		/// Messages with specified thread IDs will be emphasized or highlighted in the log file. 
		/// </summary>
		public List<string> ThreadIDsToEmphasize
		{
			get { return _threadIDsToEmphasize; }
			set { _threadIDsToEmphasize = value; }
		}

		/// <summary>
		/// Messages with specified event IDs will be emphasized or highlighted in the log file. 
		/// </summary>
		public List<int> EventIDsToEmphasize
		{
			get { return _eventIDsToEmphasize; }
			set { _eventIDsToEmphasize = value; }
		}

		/// <summary>
		/// Messages with specified process IDs will be emphasized or highlighted in the log file. 
		/// </summary>
		public List<int> ProcessIDsToEmphasize
		{
			get { return _processIDsToEmphasize; }
			set { _processIDsToEmphasize = value; }
		}

		#endregion

		#region Public Methods ********************************************************************


		#endregion

		#region Private & Protected Methods *******************************************************

		/// <summary>
		/// Formats the lines of text to be output then writes them to the log.
		/// </summary>
		/// <param name="logEntryFields">Fields to write to the log.</param>
		/// <param name="doWriteLine">Determines whether to perform a WriteLine or a Write to the 
		/// log.</param>
		/// <returns>True if successful.</returns>
		protected override bool WriteToCustomLog(LogEntryFields logEntryFields, bool doWriteLine)
		{
			bool isOK = false;

			FormattedLogEntry formattedLogEntry = this.GetFormattedLinesToWrite(logEntryFields,
				doWriteLine);

			if (formattedLogEntry.LinesToWrite.Count > 0)
			{
				isOK = this.WriteToCustomLog(formattedLogEntry, doWriteLine, _logFileName,
					_fileNameIncludesDate);
			}
			else
			{
				isOK = true;
			}
			return isOK;
		}

		/// <summary>
		/// Method that actually writes the log message to the file.
		/// </summary>
		/// <param name="formattedLogEntry">Structure that includes the formatted lines of text 
		/// that will be written to the log and the category or EventType that applies to the 
		/// log entry.</param>
		/// <param name="doWriteLine">Determines whether to perform a WriteLine or a Write to the 
		/// log.</param>
		/// <param name="logFileName">The full name, including path, of the file that the log 
		/// entry will be written to.  Excludes any date suffix to the filename.</param>
		/// <param name="fileNameIncludesDate">true if the log filename should include a date, eg 
		/// C:\Logs\MyLogFile_20070219.log.  false if the log filename should not include a date, 
		/// eg C:\Logs\MyLogFile.log.</param>
		/// <returns>true if successful, otherwise false.</returns>
		private bool WriteToCustomLog(FormattedLogEntry formattedLogEntry, bool doWriteLine,
			string logFileName, bool fileNameIncludesDate)
		{
			bool isOK = false;
			try
			{
				// Add date suffix to file name, if required.			
				if (fileNameIncludesDate)
				{
					string dateText = DateTime.Now.ToString("_yyyyMMdd.");
					// Use home-made ReplaceLast method, rather than built-in string.Replace, 
					//	to deal with paths that contain a "." .  Only want to replace the "." 
					//	before the file extension, not every "." in the path.
					logFileName = this.ReplaceLast(logFileName, ".", dateText);
				}

				StreamWriter writer = null;
				if (File.Exists(logFileName))
				{
					writer = File.AppendText(logFileName);
				}
				else
				{
					writer = new StreamWriter(logFileName);
				}

				// Emphasize any log entry (by writing it in upper case) if its category is one of 
				//	the categories to emphasize, or its source is one of the sources to emphasize, 
				//	etc.  
				//	NB: cannot use List.Contains method for string comparisons - this is 
				//	case-sensitive and want to do a case-insensitive comparison.
				LogEntryProperties logEntryProperties = formattedLogEntry.Properties;
				StringComparer caseInsensitiveComparer = StringComparer.CurrentCultureIgnoreCase;
				bool emphasizeLogEntry
					= (_categoriesToEmphasize.BinarySearch(logEntryProperties.Category,
							caseInsensitiveComparer) >= 0)
						|| (_sourcesToEmphasize.BinarySearch(logEntryProperties.Source,
							caseInsensitiveComparer) >= 0)
						|| (_methodsToEmphasize.BinarySearch(logEntryProperties.MethodThatWroteToLog,
							caseInsensitiveComparer) >= 0)
						|| (_threadIDsToEmphasize.BinarySearch(logEntryProperties.ThreadID,
							caseInsensitiveComparer) >= 0)
						|| (_eventIDsToEmphasize.Contains(logEntryProperties.EventID))
						|| (_processIDsToEmphasize.Contains(logEntryProperties.ProcessID));

				string textToWrite = string.Empty;
				foreach (string lineToWrite in formattedLogEntry.LinesToWrite)
				{
					if (emphasizeLogEntry)
					{
						textToWrite = lineToWrite.ToUpper();
					}
					else
					{
						textToWrite = lineToWrite;
					}

					if (doWriteLine)
					{
						writer.WriteLine(textToWrite);
					}
					else
					{
						writer.Write(textToWrite);
					}
				}

				writer.Flush();
				writer.Close();
				writer.Dispose();
				isOK = true;
			}
			catch
			{
				isOK = false;
			}
			return isOK;
		}

		/// <summary>
		///	Returns the full file name of the log file, including a full path.  Excludes any date 
		/// suffix to the log file name.  If the log filename passed in is blank or null it will 
		/// be set to the application name with a ".log" extension.  If the log filename passed in 
		/// does not include a path, or only includes a relative path, the path to the application 
		/// folder will be prepended to the filename.
		/// </summary>
		/// <returns>File name and full path to the log file, excluding any date suffix in the 
		/// file name.</returns>
		private string GetLogFileNameWithPath(string logFileName)
		{
			// If no file name was specified use the application name with a ".log" extension as 
			//	the log file.
			//	Use System.Reflection.Assembly rather than Windows.Forms.Application so that this 
			//	will work with any .NET application, Windows Forms or not.
			if (logFileName == null || logFileName.Trim().Length == 0)
			{
				logFileName = GetDefaultFileName();
			}

			// If file name is a partial path or a file name without a path, prepend the path to 
			//	the folder the application is running in.
			if (!Path.IsPathRooted(logFileName))
			{
				string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				logFileName = Path.Combine(appPath, logFileName);
			}

			// Create folder that log file will be saved to, if required.
			string logPath = Path.GetDirectoryName(logFileName);
			if (!Directory.Exists(logPath))
			{
				Directory.CreateDirectory(logPath);
			}

			return logFileName;
		}

		/// <summary>
		/// Replace the last occurrence of a string within another string.
		/// </summary>
		/// <param name="textToOperateOn">The text containing the string that will be replaced.</param>
		/// <param name="oldValue">The string to replace.</param>
		/// <param name="newValue">The replacement string.</param>
		/// <returns>string with only the last occurrence of a sub-string replaced.</returns>
		private string ReplaceLast(string textToOperateOn, string oldValue, string newValue)
		{
			int oldValueLastIndex = textToOperateOn.LastIndexOf(oldValue);
			if (oldValueLastIndex == -1)
			{
				return textToOperateOn;
			}

			string leftText = textToOperateOn.Substring(0, oldValueLastIndex);
			string rightText = textToOperateOn.Substring(oldValueLastIndex,
				(textToOperateOn.Length - oldValueLastIndex));
			rightText = rightText.Replace(oldValue, newValue);

			return leftText + rightText;
		}

		/// <summary>
		/// Returns the default file name, excluding path, for the log file.  Uses the application 
		/// name with a ".log" extension.
		/// </summary>
		private string GetDefaultFileName()
		{
			string assemblyFullFileName = null;
			if (Assembly.GetEntryAssembly() != null)
			{
				assemblyFullFileName = Assembly.GetEntryAssembly().Location;
			}

			// Sometimes the entry assembly is null so walk the call stack to find the name of 
			//	the calling assembly.
			else
			{
				StackTrace stackTrace = new StackTrace(3);
				StackFrame stackFrame = stackTrace.GetFrame(0);
				Type classContainingMethod = stackFrame.GetMethod().DeclaringType;
				Assembly thisAssembly = classContainingMethod.Assembly;
				Assembly testAssembly = thisAssembly;
				int i = 0;
				while (i < stackTrace.FrameCount
						&& (testAssembly == thisAssembly
							|| classContainingMethod.Namespace.StartsWith("System",
								StringComparison.CurrentCultureIgnoreCase)))
				{
					i++;
					stackFrame = stackTrace.GetFrame(i);
					classContainingMethod = stackFrame.GetMethod().DeclaringType;
					testAssembly = classContainingMethod.Assembly;
				}
				assemblyFullFileName = testAssembly.Location;
			}
			return Path.GetFileNameWithoutExtension(assemblyFullFileName) + ".log";
		}

		#endregion
	}
}
