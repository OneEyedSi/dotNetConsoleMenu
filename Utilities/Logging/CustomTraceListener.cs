///////////////////////////////////////////////////////////////////////////////////////////////////
// Project      -   Utilities.Logging
// General      -   Set of generic classes that may be used for logging in any project.
//
// File Name    -   CustomTraceListener.cs
// File Title   -   Custom Trace Listener
// Description  -   Abstract base class for custom trace listeners.
// Notes        -   
//
// $History: CustomTraceListener.cs $
// 
// *****************  Version 9  *****************
// User: Simone       Date: 30/01/09   Time: 3:53p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.Logging
// GetCallingMethodName, IgnoreMethod: Use MethodBase rather than
// MethodInfo as stackFrame.GetMethod can return a ConstructorInfo object
// as well as a MethodInfo object. 
// 
// *****************  Version 8  *****************
// User: Simone       Date: 2/12/08    Time: 11:22a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.Logging
// Make properties MethodNameClassesToIgnore and MethodNameMethodsToIgnore
// read-write instead of read-only.  Allows a list to be created then
// assigned to the property whereas previously the only way to add values
// to the lists was to use the Add methods of each property.
// 
// *****************  Version 7  *****************
// User: Simone       Date: 23/10/08   Time: 1:52p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.Logging
// MINOR: Some XML documentation comments were ill-formed because they
// contained the "&" symbol wshen compiled the documentation.  Replaced
// with "and".
// 
// *****************  Version 6  *****************
// User: Simone       Date: 21/07/08   Time: 6:02p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.Logging
// TraceOutputOptions property removed as it was hiding the base class
// TraceOutputOptions property.
// 
// *****************  Version 5  *****************
// User: Simone       Date: 30/05/08   Time: 2:40p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.Logging
// IgnoreMethod: classContainingMethod == this.GetType replaced by
// classContainingMethod.IsInstanceOfType(this) to cope with derived
// classes.
// 
// *****************  Version 4  *****************
// User: Simone       Date: 4/03/08    Time: 11:36a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.Logging
// MINOR: Comments added to data member variable declarations.
// 
// *****************  Version 3  *****************
// User: Simone       Date: 4/03/08    Time: 11:29a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.Logging
// Added properties MethodNameClassesToIgnore, MethodNameMethodsToIgnore.
// GetCallingMethodName: New parameters classesToIgnore, methodsToIgnore.
// While walking up through the call stack, GetCallingMethodName will now
// ignore methods and classes in these lists.  New methods IgnoreMethod
// and IsInStringList to support the changes to GetCallingMethodName.
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
// Copied from version 3 in
// $/ServiceAlliance/JobsByEmail/JobsByEmailGeneric/EmailParser/Utilities.
///////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Utilities.Logging
{
	/// <summary>
	/// Abstract base class for custom trace listeners.
	/// </summary>
	public abstract class CustomTraceListener : TraceListener
	{
		/// <summary>
		/// Structure representing all the optional fields in the trace output.
		/// </summary>
		protected struct OptionalTraceOutputFields
		{
			/// <summary>
			/// Time at which the event trace occurred.
			/// </summary>
			public DateTime DateTime;

			/// <summary>
			/// The tick count of the timer mechanism at the time the event trace occurred.
			/// </summary>
			public long Timestamp;

			/// <summary>
			/// Unique ID of current process.
			/// </summary>
			public int ProcessID;

			/// <summary>
			/// Unique ID for current managed thread.
			/// </summary>
			public string ThreadID;

			/// <summary>
			/// Call stack for current thread.
			/// </summary>
			public string CallStack;

			/// <summary>
			/// Stack containing correlation data.
			/// </summary>
			public string LogicalOperationStack;
		}

		/// <summary>
		/// Structure representing all the fields that may be included in a log entry.
		/// </summary>
		protected struct LogEntryFields
		{
			/// <summary>
			/// Message to write to log.
			/// </summary>
			public string Message;

			/// <summary>
			/// Category name used to organise log or the type of event that raised the trace.
			/// </summary>
			public string Category;

			/// <summary>
			/// Detailed error message.  Only used with Fail method.
			/// </summary>
			public string DetailedMessage;

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
			/// Time at which the event trace occurred.
			/// </summary>
			public DateTime EventDateTime;

			/// <summary>
			/// The tick count of the timer mechanism at the time the event trace occurred.
			/// </summary>
			public long EventTimestamp;

			/// <summary>
			/// A Guid identifying a related activity.
			/// </summary>
			public string RelatedActivityID;

			/// <summary>
			/// Unique ID of current process.
			/// </summary>
			public int ProcessID;

			/// <summary>
			/// Unique ID for current managed thread.
			/// </summary>
			public string ThreadID;

			/// <summary>
			/// Call stack for current thread.
			/// </summary>
			public string CallStack;

			/// <summary>
			/// Stack containing correlation data.
			/// </summary>
			public string LogicalOperationStack;
		}

		#region Class Variables ***************************************************************************************

		// Application name that all messages written by this listener will be logged against.
		private string _appName;

		// Category that Trace.Fail method logs messages against.
		private string _failCategory;      
 
		// Default category that Trace.Fail method logs messages against.
		private const string _dfltFailCategory = "Error";

		// When messages are logged against these categories a stack trace will be saved along 
		//	with the message.
		private List<string> _stackTraceCategories = new List<string>();

		// When walking the call stack to determine the method that raised the log message, 
		//	ignore methods in these classes.
		private List<string> _methodNameClassesToIgnore = new List<string>();

		// When walking the call stack to determine the method that raised the log message, 
		//	ignore these methods.
		private List<string> _methodNameMethodsToIgnore = new List<string>();

		#endregion

		#region Constructors and Destructors **************************************************************************

		public CustomTraceListener()
			: this(null)
		{
		}

		public CustomTraceListener(string appName)
			: base()
		{
			_appName = appName;
			_failCategory = _dfltFailCategory;
			_stackTraceCategories = new List<string>();
			_stackTraceCategories.Add(_failCategory);
			this.TraceOutputOptions = TraceOptions.None;
		}

		#endregion

		#region Properties ********************************************************************************************

		/// <summary>
		/// Application name that all messages written by this listener will be logged against.
		/// </summary>
		public string ApplicationName
		{
			get { return _appName; }
			set { _appName = value; }
		}

		/// <summary>
		/// Category that Fail method logs messages against.
		/// </summary>
		/// <remarks>Only applicable for Trace.Fail method.  Not used by methods of the TraceSouce object.</remarks>
		public string FailCategory
		{
			get { return _failCategory; }
			set
			{
				// Following line must be executed before setting _failCategory to the new value because it returns 
				//  the index of the list element containing the old value of _failCategory.
				int failCatIndex = _stackTraceCategories.FindIndex(MatchesFailCategory);
				if (failCatIndex > -1)
				{
					_stackTraceCategories[failCatIndex] = value;
				}
				else
				{
					_stackTraceCategories.Add(value);
				}
				_failCategory = value;
			}
		}

		/// <summary>
		/// When messages are logged against these categories a stack trace will be saved 
		/// along with the message.
		/// </summary>
		/// <remarks>Only applicable for methods of the Trace class, such as Write, WriteLine, Fail.  Not used by 
		/// methods of the TraceSouce object.</remarks>
		public List<string> StackTraceCategories
		{
			get { return _stackTraceCategories; }
			// set {}
		}

		/// <summary>
		/// List of classes in the call stack that should be skipped when walking the 
		/// stack to determine the method that raised the log message.
		/// </summary>
		/// <remarks>The method that raised the log message is determined by walking 
		/// up through the call stack until a different class is reached.  If wrapper 
		/// methods are used to write to the log, these should be ignored during the 
		/// stack walk.  This list allows classes that contain wrapper methods to be 
		/// ignored during the stack walk.</remarks>
		public List<string> MethodNameClassesToIgnore
		{
			get { return _methodNameClassesToIgnore; }
			set { _methodNameClassesToIgnore = value; }
		}

		/// <summary>
		/// List of methods in the call stack that should be skipped when walking the 
		/// stack to determine the method that raised the log message.
		/// </summary>
		/// <remarks>The method that raised the log message is determined by walking 
		/// up through the call stack until a different class is reached.  If wrapper 
		/// methods are used to write to the log, these should be ignored during the 
		/// stack walk.  </remarks>
		public List<string> MethodNameMethodsToIgnore
		{
			get { return _methodNameMethodsToIgnore; }
			set { _methodNameMethodsToIgnore = value; }
		}

		#endregion

		#region Public Methods ****************************************************************************************

		/// <summary>
		/// Writes trace and event information and a data object to the log.
		/// </summary>
		/// <param name="eventCache">A TraceEventCache object that contains the current process ID, thread ID, 
		/// and stack trace information.</param>
		/// <param name="source">The name of the source that raised the trace, eg the name of the application.</param>
		/// <param name="eventType">The type of event that raised the trace.</param>
		/// <param name="id">Unique event ID.</param>
		/// <param name="data">The trace data to write to the log.</param>
		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
		{
			WriteToLog(data, eventType.ToString(), id, source, eventCache, true);
		}

		/// <summary>
		/// Writes trace and event information and an array of data objects to the log.
		/// </summary>
		/// <param name="eventCache">A TraceEventCache object that contains the current process ID, thread ID, 
		/// and stack trace information.</param>
		/// <param name="source">The name of the source that raised the trace, eg the name of the application.</param>
		/// <param name="eventType">The type of event that raised the trace.</param>
		/// <param name="id">Unique event ID.</param>
		/// <param name="data">Array of trace data to write to the log.</param>
		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id,
			params object[] data)
		{
			StringBuilder builder = new StringBuilder();
			string dataToWrite = null;
			if (data != null)
			{
				for (int i = 0; i < data.Length; i++)
				{
					if (i != 0)
					{
						builder.Append(", ");
					}
					if (data[i] != null)
					{
						builder.Append(data[i].ToString());
					}
				}
				dataToWrite = builder.ToString();
			}
			WriteToLog(dataToWrite, eventType.ToString(), null, id, source, eventCache, null, true);
		}

		/// <summary>
		/// Writes trace and event information to the log.
		/// </summary>
		/// <param name="eventCache">A TraceEventCache object that contains the current process ID, thread ID, 
		/// and stack trace information.</param>
		/// <param name="source">The name of the source that raised the trace, eg the name of the application.</param>
		/// <param name="eventType">The type of event that raised the trace.</param>
		/// <param name="id">Unique event ID.</param>
		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
		{
			WriteToLog("", eventType.ToString(), null, id, source, eventCache, null, true);
		}

		/// <summary>
		/// Writes trace and event information and a formatted array of objects to the log.
		/// </summary>
		/// <param name="eventCache">A TraceEventCache object that contains the current process ID, thread ID, 
		/// and stack trace information.</param>
		/// <param name="source">The name of the source that raised the trace, eg the name of the application.</param>
		/// <param name="eventType">The type of event that raised the trace.</param>
		/// <param name="id">Unique event ID.</param>
		/// <param name="format">A format string that contains zero or more format items, which correspond to 
		/// objects in the args array.</param>
		/// <param name="args">An object array containing zero or more objects to format.</param>
		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id,
			string format, params object[] args)
		{
			string messageToWrite = string.Empty;
			if (args != null)
			{
				messageToWrite = string.Format(format, args);
			}
			else
			{
				messageToWrite = format;
			}
			WriteToLog(messageToWrite, eventType.ToString(), null, id, source, eventCache, null, true);
		}

		/// <summary>
		/// Writes trace and event information and a message to the log.
		/// </summary>
		/// <param name="eventCache">A TraceEventCache object that contains the current process ID, thread ID, 
		/// and stack trace information.</param>
		/// <param name="source">The name of the source that raised the trace, eg the name of the application.</param>
		/// <param name="eventType">The type of event that raised the trace.</param>
		/// <param name="id">Unique event ID.</param>
		/// <param name="message">Message to write to log.</param>
		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id,
			string message)
		{
			WriteToLog(message, eventType.ToString(), null, id, source, eventCache, null, true);
		}

		/// <summary>
		/// Writes trace and event information, a related activity identity and a message to the log.
		/// </summary>
		/// <param name="eventCache">A TraceEventCache object that contains the current process ID, thread ID, 
		/// and stack trace information.</param>
		/// <param name="source">The name of the source that raised the trace, eg the name of the application.</param>
		/// <param name="id">Unique event ID.</param>
		/// <param name="message">Message to write to log.</param>
		/// <param name="relatedActivityId">A Guid object identifying a related activity.</param>
		public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message,
			Guid relatedActivityId)
		{
			WriteToLog(message, TraceEventType.Transfer.ToString(), null, id, source, eventCache,
				relatedActivityId.ToString(), true);
		}
		/// <summary>
		/// Writes a message to the log.
		/// </summary>
		/// <param name="message">Message to write to log.</param>
		public override void Write(string message)
		{
			WriteToLog(message, null, null, 0, null, null, null, false);
		}

		/// <summary>
		/// Writes a message to the log.
		/// </summary>
		/// <param name="message">Message to write to log.</param>
		/// <param name="category">Category name used to organise log.</param>
		public override void Write(string message, string category)
		{
			WriteToLog(message, category, null, 0, null, null, null, false);
		}

		/// <summary>
		/// Writes the value of an object's ToString method to the log.
		/// </summary>
		/// <param name="objToLog">An object whose class name you want to write to the log.</param>
		/// However, when writing to a database the Write and WriteLine methods are identical.</remarks>
		public override void Write(object objToLog)
		{
			WriteToLog(objToLog, null, 0, null, null, false);
		}

		/// <summary>
		/// Writes the value of an object's ToString method to the log.
		/// </summary>
		/// <param name="objToLog">An object whose class name you want to write to the log.</param>
		/// <param name="category">Category name used to organise log.</param>
		public override void Write(object objToLog, string category)
		{
			WriteToLog(objToLog, category, 0, null, null, false);
		}

		/// <summary>
		/// Writes a message to the log.
		/// </summary>
		/// <param name="message">Message to write to log.</param>
		public override void WriteLine(string message)
		{
			WriteToLog(message, null, null, 0, null, null, null, true);
		}

		/// <summary>
		/// Writes a message to the log.
		/// </summary>
		/// <param name="message">Message to write to log.</param>
		/// <param name="category">Category name used to organise log.</param>
		public override void WriteLine(string message, string category)
		{
			WriteToLog(message, category, null, 0, null, null, null, true);
		}

		/// <summary>
		/// Writes the value of an object's ToString method to the log.
		/// </summary>
		/// <param name="objToLog">An object whose class name you want to write to the log.</param>
		public override void WriteLine(object objToLog)
		{
			WriteToLog(objToLog, null, 0, null, null, true);
		}

		/// <summary>
		/// Writes the value of an object's ToString method to the log.
		/// </summary>
		/// <param name="objToLog">An object whose class name you want to write to the log.</param>
		/// <param name="category">Category name used to organise log.</param>
		public override void WriteLine(object objToLog, string category)
		{
			WriteToLog(objToLog, category, 0, null, null, true);
		}

		/// <summary>
		/// Writes an error message to the log.
		/// </summary>
		/// <param name="message">Error message to write to log.</param>
		public override void Fail(string message)
		{
			WriteToLog(message, _failCategory, null, 0, null, null, null, true);
		}

		/// <summary>
		/// Writes an error message and a detailed error message to the log.
		/// </summary>
		/// <param name="message">Error message to write to log.</param>
		/// <param name="detailedMessage">Detailed error message to write to log.</param>
		public override void Fail(string message, string detailedMessage)
		{
			WriteToLog(message, _failCategory, detailedMessage, 0, null, null, null, true);
		}

		#endregion

		#region Private & Protected Methods ***************************************************************************

		/// <summary>
		/// Method called by delegate in _stackTraceCategories.FindIndex method.  Must return a boolean.  Checks if 
		/// list contains the fail category.
		/// </summary>
		/// <param name="s">Value of list item to check.</param>
		/// <returns>True if list item matches fail category.</returns>
		/// <remarks>Case insensitive.</remarks>
		private bool MatchesFailCategory(string s)
		{
			bool matchWasFound = false;
			if (s.ToLower() == _failCategory.ToLower())
			{
				matchWasFound = true;
			}
			return matchWasFound;
		}

		/// <summary>
		/// Writes object to log.
		/// </summary>
		/// <param name="objToLog">Object to write to log.</param>
		/// <param name="category">Category name used to organise log or the type of event that raised the trace.</param>
		/// <param name="eventID">Unique event ID.</param>
		/// <param name="source">The name of the source that raised the trace.</param>
		/// <param name="eventCache">A TraceEventCache object that contains the current process ID, thread ID, and stack trace information.</param>
		/// <param name="doWriteLine">Determines whether to perform a WriteLine or a Write to the log.</param>
		/// <returns>True if successful.</returns>
		private bool WriteToLog(object objToLog, string category, int eventID,
			string source, TraceEventCache eventCache, bool doWriteLine)
		{
			bool isOK = false;
			string messageToLog;
			try
			{
				messageToLog = objToLog.ToString();
			}
			catch
			{
				messageToLog = "<Could not log specified object - error when called ToString() on object.";
				category = _failCategory;
			}

			isOK = WriteToLog(messageToLog, category, null, eventID, source, eventCache, null, doWriteLine);
			return isOK;
		}

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
		protected virtual bool WriteToLog(string message, string category, string detailedMessage, int eventID,
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
		/// Get optional trace output.
		/// </summary>
		/// <param name="eventCache">A TraceEventCache object that contains the current process ID, thread ID, 
		/// and stack trace information.</param>
		/// <param name="category">Category name used to organise log, or the type of event that raised 
		/// the trace.</param>
		/// <param name="oOptionalTraceOutputFields">Output parameter.  Structure representing all the 
		/// optional fields in the trace output.</param>
		protected void GetOptionalTraceOutput(TraceEventCache eventCache, string category,
			out OptionalTraceOutputFields oOptionalTraceOutputFields)
		{
			DateTime eventDateTime = DateTime.MinValue;
			long eventTimestamp = 0;
			int processID = 0;
			string threadID = null;
			string callStack = null;
			string operationStackText = null;
			if (eventCache != null)
			{
				if (IsEnabled(TraceOptions.DateTime))
				{
					// TraceEventCache.DateTime is always in UTC.  Convert it to local time.
					eventDateTime = eventCache.DateTime.ToLocalTime();
				}
				if (IsEnabled(TraceOptions.Timestamp))
				{
					eventTimestamp = eventCache.Timestamp;
				}
				if (IsEnabled(TraceOptions.ProcessId))
				{
					processID = eventCache.ProcessId;
				}
				if (IsEnabled(TraceOptions.ThreadId))
				{
					threadID = eventCache.ThreadId;
				}
				if (IsEnabled(TraceOptions.Callstack))
				{
					callStack = eventCache.Callstack;
				}
				if (IsEnabled(TraceOptions.LogicalOperationStack))
				{
					StringBuilder builder = new StringBuilder();
					Stack operationStack = eventCache.LogicalOperationStack;
					bool isFirstItem = true;
					foreach (object stackItem in operationStack)
					{
						if (isFirstItem)
						{
							isFirstItem = false;
						}
						else
						{
							builder.Append(Environment.NewLine);
						}
						builder.Append(stackItem.ToString());
					}
					operationStackText = builder.ToString();
				}
			}
			// For Trace.Write/WriteLine: EventCache will be null.
			else
			{
				if (ShouldSaveStackTrace(category))
				{
					// Skip the 5 stack frames relating to this class and to Trace.Write/WriteLine/Fail or 
					//	TraceSource.TraceData/TraceEvent/TraceTransfer.
					StackTrace stackTrace = new StackTrace(5, true);
					callStack = stackTrace.ToString();
				}

				if (IsEnabled(TraceOptions.DateTime))
				{
					eventDateTime = DateTime.Now;
				}
			}
			oOptionalTraceOutputFields.DateTime = eventDateTime;
			oOptionalTraceOutputFields.Timestamp = eventTimestamp;
			oOptionalTraceOutputFields.ProcessID = processID;
			oOptionalTraceOutputFields.ThreadID = threadID;
			oOptionalTraceOutputFields.CallStack = callStack;
			oOptionalTraceOutputFields.LogicalOperationStack = operationStackText;
		}

		/// <summary>
		/// Test whether a stack trace should be saved for a specified message category.
		/// </summary>
		/// <param name="category">Category to check.</param>
		/// <returns>True if a stack trace should be saved for the specified category.</returns>
		/// <remarks>Categories may be of the form "method.category" so must check only the text 
		/// following the full stop.</remarks>
		private bool ShouldSaveStackTrace(string category)
		{
			bool saveStackTrace = false;
			if (category != null)
			{
				int fullStopIndex = category.LastIndexOf('.');
				if (fullStopIndex > -1)
				{
					category = category.Substring(fullStopIndex + 1);
				}
				category = category.ToLower();
				foreach (string stCategory in _stackTraceCategories)
				{
					if (stCategory.ToLower() == category)
					{
						saveStackTrace = true;
						break;
					}
				}
			}

			return saveStackTrace;
		}

		/// <summary>
		/// Determines whether a trace option has been set or not.
		/// </summary>
		/// <param name="option">Trace option to test.</param>
		/// <returns>True if option is set.</returns>
		private bool IsEnabled(TraceOptions option)
		{
			return ((option & this.TraceOutputOptions) != TraceOptions.None);
		}

		/// <summary>
		/// Gets the name of the method in the parent application that wrote the 
		/// message that is to be logged.</summary>
		/// <param name="classesToIgnore">List of class names to ignore when walking up through 
		/// the call stack to determine the calling method's name.</param>
		/// <param name="methodsToIgnore">List of method names to ignore when walking up through 
		/// the call stack to determine the calling method's name.</param>
		/// <returns>The method name.</returns>
		protected string GetCallingMethodName(List<string> classesToIgnore, 
			List<string> methodsToIgnore)
		{
			string methodName = string.Empty;

			bool checkClassList = (classesToIgnore != null && classesToIgnore.Count > 0);
			bool checkMethodList = (methodsToIgnore != null && methodsToIgnore.Count > 0);

			// Skip the stack frames in this class. (if original call passed in a data object rather than a message 
			//	there will be an extra frame).
			StackTrace stackTrace = new StackTrace(3);
			int i = 0;
			StackFrame stackFrame = stackTrace.GetFrame(i);
			Type classOfCallingMethod = stackFrame.GetMethod().DeclaringType;
			// Use MethodBase rather than MethodInfo as stackFrame.GetMethod can return a 
			//	ConstructorInfo object as well as a MethodInfo object.
			MethodBase methodBase = stackFrame.GetMethod();

			//while ((classOfCallingMethod == this.GetType()
			//        || classOfCallingMethod.Namespace.StartsWith("System", 
			//            StringComparison.CurrentCultureIgnoreCase))
			//    && i < stackTrace.FrameCount)
			//{
			//    i++;
			//    stackFrame = stackTrace.GetFrame(i);
			//    classOfCallingMethod = stackFrame.GetMethod().DeclaringType;
			//}
			while (IgnoreMethod(classesToIgnore, methodsToIgnore, methodBase)
					&& i < stackTrace.FrameCount)
			{
				i++;
				stackFrame = stackTrace.GetFrame(i);
				classOfCallingMethod = stackFrame.GetMethod().DeclaringType;
				methodBase = stackFrame.GetMethod();
			}
			//methodName = stackFrame.GetMethod().Name;
			methodName = methodBase.Name;

			return methodName;
		}

		/// <summary>
		/// Determines whether the method should be ignored When walking up through the stack 
		/// trace to determine the name of the method that raised the log message.
		/// </summary>
		/// <param name="classesToIgnore"></param>
		/// <param name="methodsToIgnore"></param>
		/// <param name="methodBase"></param>
		/// <returns></returns>
		private bool IgnoreMethod(List<string> classesToIgnore,
			List<string> methodsToIgnore, MethodBase methodBase)
		{
			bool ignoreMethod = false;

			Type classContainingMethod = methodBase.DeclaringType;

			bool checkClassList = (classesToIgnore != null && classesToIgnore.Count > 0);
			bool checkMethodList = (methodsToIgnore != null && methodsToIgnore.Count > 0);

			if ((checkMethodList && IsInStringList(methodsToIgnore, methodBase.Name, true))
				|| (checkClassList
					&& IsInStringList(classesToIgnore, classContainingMethod.Name, true))
				// Use IsInstanceOfType rather than classContainingMethod == this.GetType to 
				//	handle inheritance (eg classContainingMethod is CustomTraceListener but 
				//	this.GetType() is DatabaseTraceListener).
				|| (classContainingMethod.IsInstanceOfType(this))
				|| (classContainingMethod.Namespace.StartsWith("System",
					StringComparison.CurrentCultureIgnoreCase))
				)
			{
				ignoreMethod = true;
			}

			return ignoreMethod;
		}

		/// <summary>
		/// Determines whether a string list contains a given string.
		/// </summary>
		/// <remarks>Can perform case-sensitive or case-insensitive comparisons (unlike the 
		/// generic string List.Contains() method, which is only case-sensitive.</remarks>
		private bool IsInStringList(List<string> list, string itemToCheck, bool ignoreCase)
		{
			bool isInList = false;
			if (itemToCheck != null && list != null && list.Count > 0)
			{
				foreach (string element in list)
				{
					if (string.Compare(element, itemToCheck, ignoreCase) == 0)
					{
						isInList = true;
						break;
					}
				}
			}

			return isInList;
		}

		/// <summary>
		/// Method that in derived classes will actually write the message to the log.
		/// </summary>
		/// <param name="logEntryFields">Fields to write to the log.</param>
		/// <param name="doWriteLine">Determines whether to perform a WriteLine or a Write to the log.</param>
		/// <returns>True if successful.</returns>
		protected abstract bool WriteToCustomLog(LogEntryFields logEntryFields, bool doWriteLine);

		#endregion
	}
}
