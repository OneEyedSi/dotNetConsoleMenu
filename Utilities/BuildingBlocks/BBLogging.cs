///////////////////////////////////////////////////////////////////////////////////////////////////
// Project      -   Datacom Building Blocks
// General      -   Classes that may be used by multiple projects in multiple solutions. 
//					Higher-level classes than those in the Utilities assemblies.
//
// File Name    -   Logging.cs
// Description  -   Logging-related Routines
//
// Notes        -   
//
// $History: BBLogging.cs $
// 
// *****************  Version 9  *****************
// User: Simone       Date: 23/04/09   Time: 10:25a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// LogException: Log InnerException message if there is one.
// 
// *****************  Version 8  *****************
// User: Simone       Date: 5/04/09    Time: 3:37p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// Changed using statement from Utilities.Logging to
// Utilities.DatabaseLogging as DatabaseTraceListener has been moved to a
// new assembly and namespace.
// 
// *****************  Version 7  *****************
// User: Simone       Date: 5/04/09    Time: 2:29p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// Replaced Utilities.DataAcess.DatabaseManager with DatabaseManager2.
// 
// *****************  Version 6  *****************
// User: Simone       Date: 11/03/09   Time: 2:01p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// Static methods made thread safe.
// 
// *****************  Version 5  *****************
// User: Simone       Date: 11/03/09   Time: 1:25p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// SetupDatabaseLogging: Parameter changed from config file section name
// to SystemCoreSettings object.
// 
// *****************  Version 4  *****************
// User: Simone       Date: 10/03/09   Time: 10:38a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// MINOR: Class names have had "BB" prepended, to avoid namespace
// collisions in referencing applications which include "using"
// statements.
// 
// *****************  Version 3  *****************
// User: Simone       Date: 10/03/09   Time: 10:36a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// 
// *****************  Version 2  *****************
// User: Simone       Date: 10/03/09   Time: 8:50a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// Namespace changed from Datacom.BuildingBlocks to
// Utilities.BuildingBlocks.
// 
// *****************  Version 1  *****************
// User: Simone       Date: 10/03/09   Time: 8:44a
// Created in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// Logging-related Routines.
///////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Utilities.Miscellaneous;
using Utilities.Database;
using Utilities.Logging;

namespace Utilities.BuildingBlocks
{
	#region Logging-related Enums, Structs, Static Classes, etc ***********************************
	
	/// <summary>
	/// Helper methods to ignore when the trace listeners are determining the names of the 
	/// methods that raise log messages.
	/// </summary>
	public static class LogMethodsToIgnore
	{
		public const string LogException = "LogException";
		public const string LogStoredProcResults = "LogStoredProcResults";
	}

	#endregion

	#region Logging-related Routines **************************************************************

	/// <summary>
	/// Logging-related routines.
	/// </summary>
	public class BBLogging
	{
		#region Class Data Members ****************************************************************

		private static object _lockSetupDatabaseLogging = new object();
		private static object _lockLogStoredProcResults = new object();
		private static object _lockLogException = new object();

		#endregion

		/// <summary>
		/// Sets up the TraceListener used to write log messages to the database.
		/// </summary>
		/// <param name="traceSource">A TraceSource that raises log messages.</param>
		/// <param name="databaseListenerName">The name of the TraceListener the TraceSource uses 
		/// to write to the database.</param>
		/// <param name="configSettingsSectionName">The name of the section in the config file 
		/// that contains connection information for the database.</param>
		/// <remarks>Only need to call this once, for one of the trace sources, as the 
		/// TraceListener is shared between all the trace sources.</remarks>
		public static void SetupDatabaseLogging(TraceSource traceSource,
			string databaseListenerName, SystemCoreSettings systemSettings)
		{
			lock (_lockSetupDatabaseLogging)
			{
				DatabaseManager2 databaseManager = BBDatabase.GetDatabaseManager(systemSettings);
				DatabaseTraceListener databaseLogger =
					(DatabaseTraceListener)(traceSource.Listeners[databaseListenerName]);
				databaseLogger.DatabaseManager = databaseManager;

				Assembly callingAssembly = Assembly.GetCallingAssembly();
				AssemblyTitleAttribute assemblyTitle =
					ReflectionHelper.GetAssemblyAttribute<AssemblyTitleAttribute>(callingAssembly);
				databaseLogger.ApplicationName = assemblyTitle.Title;
				databaseLogger.TraceOutputOptions |= TraceOptions.ProcessId;
				databaseLogger.TraceOutputOptions |= TraceOptions.ThreadId;

				// Skip the following helper methods when determining the name of the method that 
				//	raised the log message.
				databaseLogger.MethodNameMethodsToIgnore.Add(LogMethodsToIgnore.LogException);
				databaseLogger.MethodNameMethodsToIgnore.Add(
					LogMethodsToIgnore.LogStoredProcResults);
			}
		}

		/// <summary>
		/// Logs the results of a call to a stored procedure in a database.
		/// </summary>
		/// <param name="logSource">TraceSource that will log the error message.</param>
		/// <param name="eventID">Unique event ID that will indicate which line of code logged 
		/// the message.</param>
		/// <param name="storedProcReturnCode">The return value from the stored procedure.  By 
		/// convention, 0 indicates success, negative integers indicate errors and positive 
		/// integers indicate warnings (minor problems that do not prevent the stored procedure 
		/// completing its function).</param>
		/// <param name="sqlErrorMessage">An error message returned from SQL Server.</param>
		/// <remarks>sqlErrorMessage: This is an empty string rather than null if there is no SQL 
		/// Server error.
		/// </remarks>
		public static void LogStoredProcResults(TraceSource logSource, int eventID,
			int storedProcReturnCode, string sqlErrorMessage)
		{
			LogStoredProcResults(logSource, eventID, string.Empty, storedProcReturnCode,
				sqlErrorMessage, string.Empty);
		}

		/// <summary>
		/// Logs the results of a call to a stored procedure in a database.
		/// </summary>
		/// <param name="logSource">TraceSource that will log the error message.</param>
		/// <param name="eventID">Unique event ID that will indicate which line of code logged 
		/// the message.</param>
		/// <param name="storedProcAction">Text that will appear at the start of the log message.
		/// </param>
		/// <param name="storedProcReturnCode">The return value from the stored procedure.  By 
		/// convention, 0 indicates success, negative integers indicate errors and positive 
		/// integers indicate warnings (minor problems that do not prevent the stored procedure 
		/// completing its function).</param>
		/// <param name="sqlErrorMessage">An error message returned from SQL Server.</param>
		/// <remarks>storedProcAction: Describe the action the stored procedure should have 
		/// performed.  This description will have the words "Success " or "Failure " pre-pended 
		/// to it in the log message, to indicate success or failure of the stored procedure.
		/// sqlErrorMessage: This is an empty string rather than null if there is no SQL 
		/// Server error.
		/// </remarks>
		public static void LogStoredProcResults(TraceSource logSource, int eventID,
			string storedProcAction, int storedProcReturnCode, string sqlErrorMessage)
		{
			LogStoredProcResults(logSource, eventID, storedProcAction, storedProcReturnCode,
				sqlErrorMessage, string.Empty);
		}

		/// <summary>
		/// Logs the results of a call to a stored procedure in a database.
		/// </summary>
		/// <param name="logSource">TraceSource that will log the error message.</param>
		/// <param name="eventID">Unique event ID that will indicate which line of code logged 
		/// the message.</param>
		/// <param name="storedProcAction">Description of the action the stored procedure 
		/// should have performed (in the past tense).
		/// </param>
		/// <param name="storedProcReturnCode">The return value from the stored procedure.  By 
		/// convention, 0 indicates success, negative integers indicate errors and positive 
		/// integers indicate warnings (minor problems that do not prevent the stored procedure 
		/// completing its function).</param>
		/// <param name="sqlErrorMessage">An error message returned from SQL Server.</param>
		/// <param name="additionalText">Additional text that can be added to the end of the 
		/// message being logged.</param>
		/// <param name="additionalTextArguments">Values that may be embedded in the additional 
		/// text, such as stored procedure output values.
		/// </param>
		/// <remarks>storedProcAction: Describe the action the stored procedure should have 
		/// performed.  This description will have the words "Success " or "Failure " pre-pended 
		/// to it in the log message, to indicate success or failure of the stored procedure.
		/// sqlErrorMessage: This is an empty string rather than null if there is no SQL 
		/// Server error.
		/// additionalText: This may contain format items (eg {0}) for embedding the stored 
		/// parameter output values in the log message.
		/// </remarks>
		public static void LogStoredProcResults(TraceSource logSource, int eventID,
			string storedProcAction, int storedProcReturnCode, string sqlErrorMessage,
			string additionalText, params object[] additionalTextArguments)
		{
			lock (_lockLogStoredProcResults)
			{
				sqlErrorMessage = sqlErrorMessage.Trim();

				TraceEventType eventType = TraceEventType.Information;
				if (storedProcReturnCode < 0 || sqlErrorMessage.Length > 0)
				{
					eventType = TraceEventType.Error;
				}
				else if (storedProcReturnCode > 0)
				{
					eventType = TraceEventType.Warning;
				}

				storedProcAction = (storedProcAction ?? string.Empty).Trim();
				if (storedProcAction.Length > 0)
				{
					// Ensure first character of action is lower case as text will be pre-pended 
					//	to it.
					string firstChar = storedProcAction.Substring(0, 1).ToLower();
					if (storedProcAction.Length > 1)
					{
						storedProcAction = firstChar + storedProcAction.Substring(1);
					}
					else
					{
						storedProcAction = firstChar;
					}

					if (eventType == TraceEventType.Error)
					{
						storedProcAction = "Failure " + storedProcAction;
					}
					else
					{
						storedProcAction = "Success " + storedProcAction;
					}
					if (!storedProcAction.EndsWith("."))
					{
						storedProcAction += ".";
					}
					storedProcAction += "  ";
				}

				string logMessage = storedProcAction;

				logMessage += string.Format("Return value: {0}.", storedProcReturnCode);
				if (sqlErrorMessage.Length > 0)
				{
					logMessage += "  SQL Server error message: " + sqlErrorMessage;
					if (!logMessage.EndsWith("."))
					{
						logMessage += ".";
					}
				}

				additionalText = (additionalText ?? string.Empty).Trim();

				// Assumption: That the number of format items in the additionalText matches the 
				//	number of additionalTextArguments supplied.
				try
				{
					if (additionalTextArguments.Length > 0)
					{
						additionalText = string.Format(additionalText, additionalTextArguments);
					}
				}
				catch { }

				if (additionalText.Length > 0)
				{
					logMessage += "  " + additionalText;
				}

				logSource.TraceEvent(eventType, eventID, logMessage);
			}
		}

		/// <summary>
		/// Logs an error message with details of an exception.
		/// </summary>
		/// <param name="logSource">TraceSource that will log the error message.</param>
		/// <param name="eventID">Unique event ID that will indicate which line of code logged 
		/// the message.</param>
		/// <param name="ex">Exception object.</param>
		public static void LogException(TraceSource logSource, int eventID, Exception ex)
		{
			LogException(logSource, eventID, ex, null);
		}

		/// <summary>
		/// Logs an error message with details of an exception.
		/// </summary>
		/// <param name="logSource">TraceSource that will log the error message.</param>
		/// <param name="eventID">Unique event ID that will indicate which line of code logged 
		/// the message.</param>
		/// <param name="ex">Exception object.</param>
		/// <param name="additionalText">Additional text to add to end of exception message.  
		/// May contain string format items, as per string.Format, to allow arguments to be 
		/// embedded in the text.</param>
		/// <param name="arguments">Object array containing zero or more arguments to embed 
		/// in the additional text (as per string.Format).</param>
		public static void LogException(TraceSource logSource, int eventID, Exception ex,
			string additionalText, params object[] arguments)
		{
			lock (_lockLogException)
			{
				if (additionalText != null)
				{
					additionalText = additionalText.Trim();
					if (additionalText.Length > 0 && !additionalText.EndsWith("."))
					{
						additionalText += ".";
					}
					if (arguments.Length > 0)
					{
						additionalText = string.Format(additionalText, arguments);
					}
				}
				else
				{
					additionalText = string.Empty;
				}

				string exceptionMessage = string.Empty;
				if (ex != null)
				{
					string exceptionName = ex.GetType().Name;
					if (!exceptionName.EndsWith("Exception"))
					{
						exceptionName += " Exception";
					}
					exceptionMessage = string.Format("{0}: {1}", exceptionName, ex.Message);
					if (ex.InnerException != null)
					{
						exceptionMessage = string.Format("{0}  Inner Exception - {1}: {2}",
							exceptionMessage, ex.InnerException.GetType().Name, 
							ex.InnerException.Message);
					}
				}

				string logMessage = string.Empty;
				if (additionalText.Length > 0 && exceptionMessage.Length > 0)
				{
					logMessage = additionalText + "  " + exceptionMessage;
				}
				else
				{
					logMessage = additionalText + exceptionMessage;
				}
				logSource.TraceEvent(TraceEventType.Error, eventID, logMessage);

				// Write a second error message with the Exception StackTrace.  Although log messages 
				//	can already include stack traces, they only show a call to the method the 
				//	exception occurred in.  To see what line of code the exception occurred on, log 
				//	the Exception StackTrace.
				if (ex != null)
				{
					logSource.TraceEvent(TraceEventType.Error, eventID,
						"Stack Trace:{0}{1}", Environment.NewLine, ex.StackTrace);
				}
			}
		}
	}

	#endregion
}
