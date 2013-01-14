///////////////////////////////////////////////////////////////////////////////////////////////////
// Project      -   Utilities.DatabaseLogging
// General      -   Set of generic classes that may be used for writing log messages to a 
//					database from any project.
//
// File Name    -   DatabaseTraceListener.cs
// Description  -   A custom trace listener that writes log entries to a table in a database.
//
// Notes        -   Requires stored procedure p_SaveLogMessage to exist in the database
//					that is being written to.  The stored procedure must have the specified 
//					parameters.
//
// $History: DatabaseTraceListener.cs $
// 
// *****************  Version 3  *****************
// User: Simonfi      Date: 21/06/11   Time: 9:29a
// Updated in $/UtilitiesClassLibrary/Utilities.DatabaseLogging
// get rid of some compiler warnings.
// 
// *****************  Version 3  *****************
// User: Simone       Date: 5/04/09    Time: 3:53p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DatabaseLogging
// MINOR: DatabaseManager2.ExecCommand renamed ExecStoredProc to make its
// function clearer.
// 
// *****************  Version 2  *****************
// User: Simone       Date: 5/04/09    Time: 3:38p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DatabaseLogging
// MINOR: Change namespace to DatabaseLogging.
// 
// *****************  Version 1  *****************
// User: Simone       Date: 5/04/09    Time: 3:21p
// Created in $/UtilitiesClassLibrary_DENG/Utilities.DatabaseLogging
// A custom trace listener that writes log entries to a table in a
// database.  Split out of Utilities.Logging so that Utilities.Logging
// does not need to reference Utilities.DataAccess if log messages are not
// going to be written to a database.
// 
// *****************  Version 4  *****************
// User: Simone       Date: 5/04/09    Time: 2:22p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.Logging
// Updated to use new Utilities.DataAccess.DatabaseManager2 class in place
// of DatabaseManager.
// 
// *****************  Version 3  *****************
// User: Simone       Date: 25/02/09   Time: 6:29p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.Logging
// MINOR: WriteToCustomLog: SQL parameters given meaningful names.
// 
// *****************  Version 1  *****************
// User: Simone       Date: 13/02/08   Time: 3:58p
// Created in $/UtilitiesClassLibrary/UtilitiesClassLibrary/Utilities.Logging
// 
// *****************  Version 2  *****************
// User: Simone       Date: 13/02/08   Time: 11:13a
// Updated in $/UtilitiesClassLibrary/Utilities.Logging
// MINOR: Added Notes to comments at top of file.
// 
// *****************  Version 1  *****************
// User: Simone       Date: 13/02/08   Time: 11:09a
// Created in $/UtilitiesClassLibrary/Utilities.Logging
// Utilities.Logging split out of original Utilities project.  Class file
// moved from Utilities project unchanged.
// 
// *****************  Version 1  *****************
// User: Simone       Date: 24/10/07   Time: 11:58a
// Created in $/UtilitiesClassLibrary/Utilities
// Copied from version 2 in
// $/ServiceAlliance/Interfaces/WebServiceInterfaces/JobsWebServices/Utili
// ties.
///////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using Utilities.Database;
using Utilities.Logging;

namespace Utilities.Logging
{
    /// <summary>
    /// Custom trace listener that writes log entries to a table in a database.
    /// </summary>
    public class DatabaseTraceListener : CustomTraceListener
	{
		#region Class Variables ***************************************************************************************

		private DatabaseManager2 _databaseManager; // Handles the connection to the database.

		#endregion

		#region Constructors and Destructors **************************************************************************

        /// <summary>
        /// Initializes a new instance of the DatabaseTraceListener class.
        /// </summary>
		public DatabaseTraceListener() : this(null, null)
		{
		}

        /// <summary>
        /// Initializes a new instance of the DatabaseTraceListener class.
        /// </summary>
        /// <param name="dbManager">the <see cref="DatabaseManager2"/> to use.</param>
		public DatabaseTraceListener(DatabaseManager2 dbManager) : this(null, dbManager)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DatabaseTraceListener class.
        /// </summary>
        /// <param name="appName">The name of the application to trace.</param>
        /// <param name="dbManager">The <see cref="DatabaseManager2"/> to use.</param>
		public DatabaseTraceListener(string appName, DatabaseManager2 dbManager)
			: base(appName)
        {
			_databaseManager = dbManager;
		}

		#endregion

		#region Properties ********************************************************************************************

		/// <summary>
		/// Database Manager object that performs all actions on database.
		/// </summary>
		public DatabaseManager2 DatabaseManager
		{
			get { return _databaseManager; }
			set { _databaseManager = value; }
		}

		#endregion

		#region Public Methods ****************************************************************************************

		#endregion

		#region Private & Protected Methods ***************************************************************************

		/// <summary>
		/// Method that actually writes the log message to the database.
		/// </summary>
		/// <param name="logEntryFields">Fields to write to the log.</param>
		/// <param name="doWriteLine">Determines whether to perform a WriteLine or a Write to the log.</param>
		/// <returns>True if successful.</returns>
		protected override bool WriteToCustomLog(LogEntryFields logEntryFields, bool doWriteLine)
		//protected override bool WriteToCustomLog(string message, string category, string detailedMessage, int eventID,
		//    string source, string methodThatWroteToLog, DateTime eventDateTime, long eventTimestamp,
		//    string relatedActivityID, int processID, string threadID,
		//    string callStack, string logicalOperationStack, bool doWriteLine)
		{
			bool isOK = false;
			try
			{
				SqlParameter prmMessage = new SqlParameter("@Message", logEntryFields.Message);
				SqlParameter prmDetailedMessage = new SqlParameter("@DetailedMessage", 
					logEntryFields.DetailedMessage);
				SqlParameter prmCategory = new SqlParameter("@Category", logEntryFields.Category);
				SqlParameter prmEventID = new SqlParameter("@EventID", logEntryFields.EventID);
				SqlParameter prmProcedureName = new SqlParameter("@ProcedureName", 
					logEntryFields.MethodThatWroteToLog);
				SqlParameter prmSource = new SqlParameter("@Source", logEntryFields.Source);
				SqlParameter prmApplication = new SqlParameter("@Application", 
					this.ApplicationName);
				SqlParameter prmProcessID = new SqlParameter("@ProcessID", 
					logEntryFields.ProcessID);
				SqlParameter prmThreadID = new SqlParameter("@ThreadID", logEntryFields.ThreadID);
				SqlParameter prmRelActivityID = new SqlParameter("@RelatedActivityID", 
					logEntryFields.RelatedActivityID);
				SqlParameter prmCallStack = new SqlParameter("@CallStack", 
					logEntryFields.CallStack);
				SqlParameter prmLogicalOpStack = new SqlParameter("@LogicalOperationStack", 
					logEntryFields.LogicalOperationStack);
				SqlParameter[] prms = { 
											prmMessage, 
											prmDetailedMessage, 
											prmCategory, 
											prmEventID, 
											prmProcedureName, 
											prmSource, 
											prmApplication,
											prmProcessID, 
											prmThreadID, 
											prmRelActivityID, 
											prmCallStack, 
											prmLogicalOpStack
										};
				string sqlErrorMessage;
				int storedProcReturnVal;
				_databaseManager.ExecStoredProc("p_SaveLogMessage", prms, 
					out storedProcReturnVal, out sqlErrorMessage);
				isOK = (storedProcReturnVal == 0);
			}
			catch
			{
				isOK = false;
			}
            return isOK;
		}
		
		#endregion
	}
}
