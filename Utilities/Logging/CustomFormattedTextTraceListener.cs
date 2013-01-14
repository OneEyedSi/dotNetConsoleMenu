///////////////////////////////////////////////////////////////////////////////////////////////////
// Project      -   Utilities.Logging
// General      -   Set of generic classes that may be used for logging in any project.
//
// File Name    -   CustomFormattedTextTraceListener.cs
// File Title   -   Custom Formatted Text Trace Listener
// Description  -   Abstract base class for custom trace listeners that combine different fields 
//					(eg date, source, message) into a single formatted message.
// Notes        -   For custom trace listeners that write free text, such as listeners that write 
//					to a text file, to a Windows form or to the console.  The alternative is 
//					trace listeners that write each field discretely (eg the database listener).
//
// $History: CustomFormattedTextTraceListener.cs $
// 
// *****************  Version 3  *****************
// User: Simone       Date: 4/03/08    Time: 11:31a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.Logging
// WriteToLog: Passes new properties (inherited from base class) as
// arguments when calling GetCallingMethodName.
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
// User: Simone       Date: 24/10/07   Time: 11:57a
// Created in $/UtilitiesClassLibrary/Utilities
// Copied from version 4 in
// $/ServiceAlliance/JobsByEmail/JobsByEmailGeneric/EmailParser/Utilities.
///////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;

namespace Utilities.Logging
{
	#region Supporting Structs, Enums, etc ************************************************************************

	/// <summary>
	/// Properties that a log entry may have, that can be used to determine the formatting to apply to 
	/// the log entry.  (eg when writing log entries to a RichTextBox in a form, entries where the 
	/// category is Error may be highlighted in red).
	/// </summary>
	public struct LogEntryProperties
	{
		/// <summary>
		/// Category name that applies to the log entry, or EventType of event that raised the log entry.  
		/// Category applies to Trace.Write/WriteLine, EventType applies to TraceSource.TraceEvent/TraceData.
		/// </summary>
		public string Category;

		/// <summary>
		/// Unique event ID.
		/// </summary>
		public int EventID;

		/// <summary>
		/// The name of the source that raised the trace.
		/// </summary>
		public string Source;

		/// <summary>
		/// Name of method in parent application that wrote the message to the log.
		/// </summary>
		public string MethodThatWroteToLog;

		/// <summary>
		/// Unique ID of current process.
		/// </summary>
		public int ProcessID;

		/// <summary>
		/// Unique ID for current managed thread.
		/// </summary>
		public string ThreadID;
	}

	/// <summary>
	/// Structure that wraps the formatted lines of text that will be written to the log along with 
	/// properties that can be used to determine additional formatting (eg when writing log entries 
	/// to a RichTextBox in a form, entries where the category is Error may be highlighted in red).
	/// </summary>
	public struct FormattedLogEntry
	{
		/// <summary>
		/// List of formatted lines of text that will be written to the log.
		/// </summary>
		public List<string> LinesToWrite;

		public LogEntryProperties Properties;
	}

	#endregion

	#region CustomFormattedTextTraceListener Class ********************************************************************

	/// <summary>
	/// Abstract base class for custom trace listeners that combine different fields (eg date, source, message) 
	/// into a single formatted message.
	/// </summary>
	/// <remarks>For custom trace listeners that write free text, such as listeners that write to a text file, 
	/// to a Windows form or to the console.  The alternative is trace listeners that write each field 
	/// discretely (eg the database listener).</remarks>
	public abstract class CustomFormattedTextTraceListener : CustomTraceListener
	{
		#region Class Variables ***************************************************************************************

		private List<string> _formatStrings;	// Format strings that can be used to format the text that is written 
												//	to the log.  

		#endregion

		#region Constructors and Destructors **************************************************************************

		public CustomFormattedTextTraceListener()
			: this(null)
		{
		}

		public CustomFormattedTextTraceListener(string appName)
			: base(appName)
		{
			_formatStrings = new List<string>();
		}

		#endregion

		#region Properties ********************************************************************************************

		/// <summary>
		/// Strings for formatting the text that is written to the log.  
		/// Each format string represents the formatting for a different line (so that each log entry can have 
		/// multiple lines).
		/// </summary>
		/// <remarks>Valid placeholders for the format strings: {DATE}, {MESSAGE}, {DETAILED_MESSAGE}, 
		/// {APPLICATION}, {SOURCE}, {METHOD}, {CATEGORY}, {EVENT_TYPE}, {EVENT_ID}, {PROCESS_ID}, {THREAD_ID}, {CALL_STACK}, 
		/// {LOGICAL_OPERATION_STACK}, {RELATED_ACTIVITY_ID}, {INDENT}.  {INDENT} adds an indent at the start of a line.  
		/// All other placeholders are fields that can be written to the log.  {CATEGORY} and {EVENT_TYPE} are two names for 
		/// the same thing; they display exactly the same information.  {CATEGORY} is a term used with Trace.Write/WriteLine 
		/// while {EVENT_TYPE} is a term used with TraceSource.TraceEvent/TraceData.  Placeholders may contain 
		/// .NET date or numeric format characters or patterns  eg  {EVENTID:000}, {PROCESSID:x}, {DATE:g}, 
		/// {DATE:yyyy-MM-dd HH:mm:ss}.</remarks>
		public List<string> FormatStrings
		{
			get { return _formatStrings; }
			set { _formatStrings = value; }
		}

		#endregion

		#region Public Methods ****************************************************************************************

		#endregion

		#region Private & Protected Methods ***************************************************************************

		/// <summary>
		/// Passes message to be logged to custom log method in derived class.
		/// </summary>
		/// <param name="message">Message to write to log.</param>
		/// <param name="category">Category name used to organise log or the type of event that raised the trace.</param>
		/// <param name="detailedMessage">Detailed error message.  Only used with Fail method.</param>
		/// <param name="eventID">Unique event ID.</param>
		/// <param name="source">The name of the source that raised the trace.</param>
		/// <param name="eventCache">A TraceEventCache object that contains the current process ID, thread ID, and stack trace information.</param>
		/// <param name="relatedActivityID">A Guid identifying a related activity.</param>
		/// <param name="doWriteLine">Determines whether to perform a WriteLine or a Write to the log.</param>
		/// <returns>True if successful.</returns>
		protected override bool WriteToLog(string message, string category, string detailedMessage, int eventID,
			string source, TraceEventCache eventCache, string relatedActivityID, bool doWriteLine)
		{
			bool isOK = false;
			try
			{
				LogEntryFields logEntryFields = new LogEntryFields();
				OptionalTraceOutputFields optionalTraceOutputFields = new OptionalTraceOutputFields();

				GetOptionalTraceOutput(eventCache, category, out optionalTraceOutputFields);

				logEntryFields.Message = message;
				logEntryFields.Category = category;
				logEntryFields.DetailedMessage = detailedMessage;
				logEntryFields.EventID = eventID;
				logEntryFields.Source = source;
				logEntryFields.MethodThatWroteToLog = 
					GetCallingMethodName(this.MethodNameClassesToIgnore, 
						this.MethodNameMethodsToIgnore);
				logEntryFields.EventDateTime = optionalTraceOutputFields.DateTime;
				logEntryFields.EventTimestamp = optionalTraceOutputFields.Timestamp;
				logEntryFields.RelatedActivityID = relatedActivityID;
				logEntryFields.ProcessID = optionalTraceOutputFields.ProcessID;
				logEntryFields.ThreadID = optionalTraceOutputFields.ThreadID;
				logEntryFields.CallStack = optionalTraceOutputFields.CallStack;
				logEntryFields.LogicalOperationStack = optionalTraceOutputFields.LogicalOperationStack;

				isOK = WriteToCustomLog(logEntryFields, doWriteLine);
			}
			catch
			{
				isOK = false;
			}
			return isOK;
		}

		/// <summary>
		/// Returns the lines of text that will be written to the log.  Reads format strings and replaces the 
		/// placeholders with the data that is to be written to the log.  
		/// </summary>
		/// <param name="logEntryFields">Fields to write to the log.</param>
		/// <param name="doWriteLine">Determines whether to perform a WriteLine or a Write to the log.</param>
		/// <returns>FormattedLogEntry structure that wraps the formatted lines of text that will be written to the log 
		/// and the category or EventType that applies to the log entry.</returns>
		/// <remarks>If no format strings have been set, default format strings will be used.</remarks>
		protected virtual FormattedLogEntry GetFormattedLinesToWrite(LogEntryFields logEntryFields, bool doWriteLine)
		{
			FormattedLogEntry logEntry = new FormattedLogEntry();
			List<string> linesToWrite = new List<string>();
			List<string> formatStrings = new List<string>();

			// If no formatting strings have been defined, use defaults.
			if (_formatStrings.Count == 0)
			{
				formatStrings.Add("{DATE:yyyy-MM-dd HH:mm:ss} {CATEGORY} {METHOD} (ID: {EVENT_ID:000}): {MESSAGE}");
				formatStrings.Add("{INDENT}Process ID: {PROCESS_ID}; Thread ID: {THREAD_ID}.");
				formatStrings.Add("{INDENT}Detailed Failure Message: {DETAILED_MESSAGE}");
				formatStrings.Add("{INDENT}Call Stack: {CALL_STACK}");
				formatStrings.Add("{INDENT}Logical Operation Stack: {LOGICAL_OPERATION_STACK}");
				formatStrings.Add("{INDENT}Related Activity ID: {RELATED_ACTIVITY_ID}");
			}
			else
			{
				formatStrings = _formatStrings;
			}

			LogEntryProperties logEntryProperties = new LogEntryProperties();
			logEntryProperties.Category = logEntryFields.Category;
			logEntryProperties.Source = logEntryFields.Source;
			logEntryProperties.MethodThatWroteToLog = logEntryFields.MethodThatWroteToLog;
			logEntryProperties.EventID = logEntryFields.EventID;
			logEntryProperties.ThreadID = logEntryFields.ThreadID;
			logEntryProperties.ProcessID = logEntryFields.ProcessID;
			logEntry.Properties = logEntryProperties;

			logEntry.LinesToWrite = new List<string>();
			string lineToWrite = string.Empty;
			foreach (string formatString in formatStrings)
			{
				bool wasDataInserted = false;
				bool morePlaceholdersToCheck = true;
				lineToWrite = formatString;
				string spaces = new string(' ', this.IndentSize);
				lineToWrite = lineToWrite.Replace("{INDENT}", spaces);
				lineToWrite = ReplaceFormattedPlaceholder<DateTime>(lineToWrite, "{DATE}", logEntryFields.EventDateTime,
					DateTime.MinValue, ref wasDataInserted, ref morePlaceholdersToCheck);
				lineToWrite = ReplaceTextPlaceholder(lineToWrite, "{MESSAGE}", logEntryFields.Message,
					ref wasDataInserted, ref morePlaceholdersToCheck);
				lineToWrite = ReplaceTextPlaceholder(lineToWrite, "{DETAILED_MESSAGE}", logEntryFields.DetailedMessage,
					ref wasDataInserted, ref morePlaceholdersToCheck);
				lineToWrite = ReplaceTextPlaceholder(lineToWrite, "{APPLICATION}", this.ApplicationName,
					ref wasDataInserted, ref morePlaceholdersToCheck);
				lineToWrite = ReplaceTextPlaceholder(lineToWrite, "{SOURCE}", logEntryFields.Source,
					ref wasDataInserted, ref morePlaceholdersToCheck);
				lineToWrite = ReplaceTextPlaceholder(lineToWrite, "{METHOD}", logEntryFields.MethodThatWroteToLog,
					ref wasDataInserted, ref morePlaceholdersToCheck);
				lineToWrite = ReplaceTextPlaceholder(lineToWrite, "{CATEGORY}", logEntryFields.Category,
					ref wasDataInserted, ref morePlaceholdersToCheck);
				lineToWrite = ReplaceTextPlaceholder(lineToWrite, "{EVENT_TYPE}", logEntryFields.Category,
					ref wasDataInserted, ref morePlaceholdersToCheck);
				lineToWrite = ReplaceFormattedPlaceholder<Int32>(lineToWrite, "{EVENT_ID}", logEntryFields.EventID, 0,
					ref wasDataInserted, ref morePlaceholdersToCheck);
				lineToWrite = ReplaceFormattedPlaceholder<Int32>(lineToWrite, "{PROCESS_ID}", logEntryFields.ProcessID, 0,
					ref wasDataInserted, ref morePlaceholdersToCheck);
				lineToWrite = ReplaceTextPlaceholder(lineToWrite, "{THREAD_ID}", logEntryFields.ThreadID,
					ref wasDataInserted, ref morePlaceholdersToCheck);
				lineToWrite = ReplaceTextPlaceholder(lineToWrite, "{CALL_STACK}", logEntryFields.CallStack,
					ref wasDataInserted, ref morePlaceholdersToCheck);
				lineToWrite = ReplaceTextPlaceholder(lineToWrite, "{LOGICAL_OPERATION_STACK}", logEntryFields.LogicalOperationStack,
					ref wasDataInserted, ref morePlaceholdersToCheck);
				lineToWrite = ReplaceTextPlaceholder(lineToWrite, "{RELATED_ACTIVITY_ID}", logEntryFields.RelatedActivityID,
					ref wasDataInserted, ref morePlaceholdersToCheck);
				if (wasDataInserted)
				{
					logEntry.LinesToWrite.Add(lineToWrite);
				}
			}

			return logEntry;
		}

		/// <summary>
		/// Replaces a placeholder in a format string with specified text.  If the replacement text is null an empty string 
		/// will be inserted instead.
		/// </summary>
		/// <param name="formatString">Format string containing placeholder to replace.</param>
		/// <param name="placeHolder">Placeholder to replace.</param>
		/// <param name="replacementText">Text to replace placeholder with.</param>
		/// <param name="rWasDataInserted">Set if replacement text was inserted into the format string.</param>
		/// <param name="rMorePlaceholdersToCheck">Set if there are more placeholders to replace after this one.</param>
		/// <returns>Format string with placeholder replaced with replacement text.</returns>
		private string ReplaceTextPlaceholder(string formatString, string placeHolder, string replacementText,
			ref bool rWasDataInserted, ref bool rMorePlaceholdersToCheck)
		{
			string returnText = string.Empty;

			if (rMorePlaceholdersToCheck)
			{
				formatString = formatString ?? string.Empty;
				placeHolder = placeHolder ?? string.Empty;
				int placeHolderPosition = formatString.IndexOf(placeHolder, StringComparison.CurrentCultureIgnoreCase);
				if (placeHolderPosition >= 0)
				{
					// If there is no replacement text remove the placeholder and trim any spaces between the placeholder 
					//	and any following text.
					if (replacementText == null || replacementText.Length == 0)
					{
						string leftString = string.Empty;
						if (placeHolderPosition > 0)
						{
							leftString = formatString.Substring(0, placeHolderPosition);
						}
						string rightString = (formatString.Substring(placeHolderPosition + placeHolder.Length)).TrimStart();
						returnText = leftString + rightString;
					}
					else
					{
						returnText = formatString.Replace(placeHolder, replacementText);
						rWasDataInserted = true;
					}					
				}
				// Placeholder not found in format string - return format string unchanged.
				else
				{
					returnText = formatString;
				}
				rMorePlaceholdersToCheck = returnText.Contains("{") && returnText.Contains("}");
			}
			else
			{
				returnText = formatString;
			}
			return returnText;
		}		

		/// <summary>
		/// Replaces a placeholder in a format string with a specified value.  Applies formatting to the value  
		/// if a numerical or date (as appropriate) formatting character or pattern is supplied.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="formatString">Format string containing placeholder to replace.</param>
		/// <param name="placeHolder">Placeholder to replace. May contain a numeric or date formatting character 
		/// or pattern (uses the same syntax as built-in formatting characters and patterns).</param>
		/// <param name="replacementValue">Value to replace placeholder with.</param>
		/// <param name="defaultValue">Default value for the replacement value.  If the replacement value equals the 
		/// default, assumes that no value was assigned to the replacement value.</param>
		/// <param name="rWasDataInserted">Set if replacement text was inserted into the format string.</param>
		/// <param name="rMorePlaceholdersToCheck">Set if there are more placeholders to replace after this one.</param>
		/// <returns>Format string with placeholder replaced with value.</returns>
		/// <remarks>Assumptions: 1) Placeholder ends in a "}".  2) A numeric or date format character or pattern, 
		/// if supplied, will use the same syntax as .NET. eg {EVENTID:000}, {PROCESSID:x}, {DATE:g}, 
		/// {DATE:yyyy-MM-dd HH:mm:ss} </remarks>
		private string ReplaceFormattedPlaceholder<T>(string formatString, string placeHolder, T replacementValue,
			T defaultValue, ref bool rWasDataInserted, ref bool rMorePlaceholdersToCheck)
		{
			string returnText = string.Empty;

			if (rMorePlaceholdersToCheck)
			{
				string formatPattern = null;
				formatString = formatString ?? string.Empty;
				placeHolder = placeHolder ?? string.Empty;
				placeHolder = placeHolder.Replace("}", "");
				int placeHolderPosition = formatString.IndexOf(placeHolder, StringComparison.CurrentCultureIgnoreCase);
				if (placeHolderPosition >= 0)
				{
					string leftString = string.Empty;
					if (placeHolderPosition > 0)
					{
						leftString = formatString.Substring(0, placeHolderPosition);
					}
					string rightString = (formatString.Substring(placeHolderPosition + placeHolder.Length)).TrimStart();

					// Pick up any format pattern embedded in the placeholder.
					if (rightString.StartsWith(":"))
					{
						int endPosition = rightString.IndexOf("}");
						if (endPosition > 1)
						{
							formatPattern = rightString.Substring(1, endPosition - 1);
							if (endPosition + 1 < rightString.Length)
							{
								rightString = rightString.Substring(endPosition + 1);
							}
							else
							{
								rightString = string.Empty;
							}
						}
					}
					// If no format pattern strip the "}" from the start of the rightString.
					else
					{
						rightString = rightString.Substring(1);
					}

					// No value was supplied to replace the placeholder - just remove the placeholder and any spaces  
					//	between it and the following text.
					if (replacementValue.Equals(defaultValue))
					{
						returnText = leftString + rightString.TrimStart();
					}
					// Value was supplied - replace the placeholder.
					else
					{
						string replacementText = null;
						if (formatPattern == null)
						{
							replacementText = replacementValue.ToString();
						}
						else
						{
							try
							{
								if (replacementValue is int)
								{
									int replacementInt = Convert.ToInt16(replacementValue);
									replacementText = replacementInt.ToString(formatPattern);
								}
								else if (replacementValue is DateTime)
								{
									DateTime replacementDate = Convert.ToDateTime(replacementValue);
									replacementText = replacementDate.ToString(formatPattern);
								}
								else
								{
									replacementText = replacementValue.ToString();
								}
							}
							catch
							{
								replacementText = replacementValue.ToString();
							}
						}
						returnText = leftString + replacementText + rightString;
						rWasDataInserted = true;
					}
				}
				// Placeholder not found in format string - return format string unchanged.
				else
				{
					returnText = formatString;
				}
				rMorePlaceholdersToCheck = returnText.Contains("{") && returnText.Contains("}");
			}
			else
			{
				returnText = formatString;
			}

			return returnText;
		}

		#endregion
	}

	#endregion
}
