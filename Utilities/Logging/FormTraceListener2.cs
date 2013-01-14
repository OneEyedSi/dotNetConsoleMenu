///////////////////////////////////////////////////////////////////////////////////////////////////
// Project      -   Utilities.Logging
// General      -   Set of generic classes that may be used for logging in any project.
//
// File Name    -   FormTraceListener2.cs
// Description  -   A custom trace listener for writing log entries to a Windows form.
//
// Notes        -   Improvement on FormTraceListener class.  To write to a form, FormTraceListener 
//                  relied on a delegate in the form.  To ensure the delegate existed in the form 
//                  all forms using FormTraceListener had to be derived from a custom form 
//                  class - too messy.  
//
//                  This improved version uses events.  The forms it writes to do not have to be 
//                  derived from a custom form.
//
// $History: FormTraceListener2.cs $
// 
// *****************  Version 4  *****************
// User: Simone       Date: 10/12/08   Time: 9:10a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.Logging
// MINOR: Minor change to the sample formLogger_LogMessageEvent code in
// the XML comments.
// 
// *****************  Version 3  *****************
// User: Simone       Date: 11/11/08   Time: 9:32a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.Logging
// MINOR: Added comment that XML documentation comments will be output to
// a file.
// 
// *****************  Version 2  *****************
// User: Simone       Date: 23/10/08   Time: 2:36p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.Logging
// MINOR: Corrected some of the code in the example in XML documentation
// comments.
// 
// *****************  Version 1  *****************
// User: Simone       Date: 23/10/08   Time: 12:44p
// Created in $/UtilitiesClassLibrary_DENG/Utilities.Logging
// Modified version of FormTraceListener which uses events instead of
// delegates.  The forms it writes to do not have to be derived from a
// custom form class as is the case with FormTraceListener.
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
    #region Support Classes ***********************************************************************

    /// <summary>
    /// EventArgs class that is used to pass the message to log to the form.
    /// </summary>
    public class LogMessageEventArgs : EventArgs
    {
        private string _messageToLog = string.Empty;
        private LogEntryProperties _messageProperties = new LogEntryProperties();

        public string MessageToLog
        {
            set { _messageToLog = value; }
            get { return _messageToLog; }
        }

        public LogEntryProperties MessageProperties
        {
            set { _messageProperties = value; }
            get { return _messageProperties; }
        }

        public LogMessageEventArgs(string messageToLog,
            LogEntryProperties messageProperties)
        {
            _messageToLog = messageToLog;
            _messageProperties = messageProperties;
        }
    }

    #endregion

    #region Custom Trace Listener Class ***********************************************************

    // The XML comments below will be output to file Utilities.Logging.XML in the application 
    //  folder, for use as documentation.

    /// <summary>
    /// Custom trace listener that writes log entries to a Windows form.
    /// </summary>
    /// <example>Example of how to use this class:
    /// 
    /// With the exception of the changes to the config file, all the code listed below is to be 
    /// added to the form that will display the log entries: 
    /// 
    /// <list type="number">
    /// <item>
    /// <description>
    /// In the config file set up trace logging.  Specify a FormTraceListener2 trace 
    /// listener:
    /// 
    /// <code>
    /// <system.diagnostics>
    ///     <sources>
    ///         <source name="MainProcess" switchName="msgSwitch" switchType="System.Diagnostics.SourceSwitch" >
    ///             <listeners>
    ///                 <add name="FormWriter"/>
    ///                 <remove name="Default"  />
    ///             </listeners>
    ///         </source>
    ///         <source name="TimerThread" switchName="msgSwitch" switchType="System.Diagnostics.SourceSwitch" >
    ///             <listeners>
    ///                 <add name="FormWriter"/>
    ///                 <remove name="Default"  />
    ///             </listeners>
    ///         </source>
    ///     </sources>
    ///     <sharedListeners>
    ///         <!-- Type: "<Namespace>.<Class>, <DLL Name>"-->
    ///         <add name="FormWriter" type="Utilities.Logging.FormTraceListener2, Utilities.Logging" />
    ///     </sharedListeners>
    ///     <switches>
    ///         <!-- Switch values: 
    ///                             Off
    ///                             Critical
    ///                             Error           (= errors, critical)
    ///                             Warning         (= warnings, errors, critical)
    ///                             Info            (= information, warnings, errors, critical)
    ///                             Verbose         (= verbose, information, warnings, errors, critical)
    ///                             ActivityTracing (= Start, Stop, Suspend, Resume, Transfer)
    ///                             All
    ///         -->
    ///         <add name="msgSwitch" value="All" />
    ///     </switches>
    ///     <trace autoflush="true" indentsize="4"></trace>
    /// </system.diagnostics>
    /// </code>
    /// 
    /// In this case two trace sources have been added: MainProcess and TimerThread.  
    /// 
    /// Note that this config file uses .NET-2 style TraceSources and SourceSwitches.  If the .NET 
    /// uses .NET 1-style Trace.Write and TraceSwitches the config file will be slightly different. 
    /// 
    /// Systems.Diagnostics is enabled for class libraries and Windows Forms applications.  
    /// However, it is disabled for web applications and web services.  To use Systems.Diagnostics 
    /// trace logging with web applications or web services, add the following section to the 
    /// web.config file:
    /// 
    /// <code>
    /// <system.codedom>
    ///     <compilers>
    ///         <compiler language="c#;cs;csharp" extension=".cs" type="Microsoft.CSharp.CSharpCodeProvider" warningLevel="1" compilerOptions="/d:TRACE"/>
    ///     </compilers>
    /// </system.codedom>    
    /// </code>
    /// 
    /// This will compile the application with the TRACE switch set.
    /// </description>
    /// </item>
    /// 
    /// <item>
    /// <description>
    /// In the form class, add a method that will write log messages to a control on the form 
    /// (where txtMessages is the TextBox control that the log entries will be written to):
    /// <code>
    /// private void WriteLogMessage(string messageToLog)
    /// {
    ///     int startOfLinePosition = this.txtMessages.TextLength;
    ///     this.txtMessages.AppendText(messageToLog);
    ///     int lineLength = this.txtMessages.TextLength - startOfLinePosition;
    ///     this.txtMessages.Select(startOfLinePosition, lineLength);
    ///     this.txtMessages.ScrollToCaret();
    /// }    
    /// </code>
    /// </description>
    /// </item>
    /// 
    /// <item>
    /// <description>
    /// Add a delegate to call the WriteLogMessage method to writes log messages to the form.  It 
    /// must have the same signature as the WriteLogMessage method: 
    /// 
    /// <c>private delegate void MessageWriter(string messageToLog);</c>
    /// </description>
    /// </item>
    /// 
    /// <item>
    /// <description>
    /// Add an event handler in the form that will be hooked up to the FormTraceListener2 
    /// LogMessageEvent event.  This event handler will execute when the FormTraceListener2 writes 
    /// a message to the log.  The event handler uses the MessageWriter delegate to call the 
    /// WriteLogMessage method to write log messages to the form.  
    /// 
    /// The delegate is required because the LogMessageEvent will usually <b>not</b> be raised on the 
    /// form's thread.  Attempting to write directly to a control on the form from another thread 
    /// will result in an InvalidOperationException: 
    /// "Cross-thread operation not valid: Control 'txtMessages' accessed from a thread other 
    /// than the thread it was created on."
    /// 
    /// To get around the problem of writing to the form from another thread, use BeginInvoke with 
    /// the delegate.  This will write to the form from the form's own thread.
    /// 
    /// <code>
    /// private void formLogger_LogMessageEvent(object sender, LogMessageEventArgs e)
    /// {
    ///     string messageToLog = e.MessageToLog;
    ///
	///		// Add "!this.Disposing" check to avoid ObjectDisposedException if this event handler  
	///		//  runs as the form is being closed (disposed).
	///		if (!this.Disposing)
	///		{
	///			if (this.InvokeRequired)
	///			{
	///				this.BeginInvoke(new MessageWriter(this.WriteLogMessage), messageToLog);
	///			}
	///			else
	///			{
	///				this.WriteLogMessage(messageToLog);
	///			}
	///		}
    /// }
    /// </code>
    /// </description>
    /// </item>
    /// 
    /// <item>
    /// <description>
    /// Add a method to set up logging to the form.  It retrieves the FormTraceListener2 object 
    /// (which was declared in the config file) from the Listeners collection of one of the 
    /// TraceSources.  It then hooks up the FormTraceListener2 LogMessageEvent to the 
    /// formLogger_LogMessageEvent event handler declared in the form.
    /// 
    /// If multiple TraceSources are declared in the config file, any of the TraceSources which 
    /// have the FormTraceListener2 object as a listener can be used.
    /// <code>
    /// 
    /// private void SetupFormLogging()
    /// {
    ///     TraceSource mainLogSource = new TraceSource("MainProcess");
    ///     FormTraceListener2 formLogger =
    ///         (FormTraceListener2)(mainLogSource.Listeners["FormWriter"]);
    ///
    ///     formLogger.LogMessageEvent
    ///         += new FormTraceListener2.LogMessageEventHandler(formLogger_LogMessageEvent);
    ///
    ///     formLogger.TraceOutputOptions |= TraceOptions.DateTime;
    ///     formLogger.TraceOutputOptions |= TraceOptions.ThreadId;
    ///     formLogger.FormatStrings.Clear();
    ///     formLogger.FormatStrings.Add(
    ///         "{DATE:HH:mm:ss}  [Thread ID: {THREAD_ID}]  {SOURCE}  {METHOD} (ID: {EVENT_ID:000}):   {MESSAGE}");
    /// }
    /// </code>
    /// </description>
    /// </item>
    /// 
    /// <item>
    /// <description>
    /// Call the SetupFormLogging method to set up logging when the form is loaded (via the form 
    /// Load event handler):
    /// 
    /// <code>
    /// private void frmMain_Load(object sender, EventArgs e)
    /// {
    ///     // Set up form trace listener.
    ///     SetupFormLogging();
    /// }
    /// </code>
    /// </description>
    /// </item>
    /// </list>
    /// </example>
    /// 
    ///	<remarks>
    ///	The LogMessageEventArgs object, which is used to pass the message to log from the 
    ///	FormTraceListener2 object to the form, has two properties: <b>MessageToLog</b>, which is 
    ///	the string that will be written to the form, and <b>MessageProperties</b>, a 
    ///	LogEntryProperties object which contains information about the message.  The 
    ///	MessageProperties can be used to highlight different types of messages in different 
    ///	colours if the log messages are being written to a RichTextBox on the form rather than a 
    ///	simple TextBox.  
    ///	
    /// The following properties of MessageProperties may be useful in categorizing messages for 
    /// display:
    /// 
    /// <list type="bullet">
    /// <listheader>
    /// <term>MessageProperties Property</term>
    /// <description>Description of property</description>
    /// </listheader>
    /// <item>
    /// <term>Category</term>
    /// <description>For .NET 1-style Trace.Write, this is the Category name (eg Error, 
    /// Information).  For .NET 2-style Trace.Event this is the EventType name (eg Error, Resume).
    /// </description>
    /// </item>
    /// <item>
    /// <term>Source</term>
    /// <description>The name of the TraceSource that wrote the message being logged.  In the 
    /// example above, this would either be MainProcess or TimerThread.</description>
    /// </item>
    /// <item>
    /// <term>ThreadID</term>
    /// <description>The ID of the managed thread that raised the message being written to the log.
    /// </description>
    /// </item>
    /// </list>
    /// 
    /// The following code has been modified from the example above to display different types of 
    /// message in different colours:
    /// 
    /// <list type="number">
    /// <item>
    /// <description>
    /// The WriteLogMessage method that writes log messages to a control on the form has an 
    /// additional parameter, messageProperties: 
    /// 
    /// <code>
    /// private void WriteLogMessage(string messageToLog, LogEntryProperties messageProperties)
    /// {
    ///     int startOfLinePosition = this.txtMessages.TextLength;
    ///     this.txtMessages.AppendText(messageToLog);
    ///     int lineLength = this.txtMessages.TextLength - startOfLinePosition;
    ///     this.txtMessages.Select(startOfLinePosition, lineLength);
    ///     this.txtMessages.SelectionColor = GetLogTextColour(messageProperties);
    ///     this.txtMessages.ScrollToCaret();
    /// }
    /// </code>
    /// 
    /// The WriteLogMessage method calls a second method, GetLogTextColour, to determine what 
    /// colour the text should be, when it is written to the RichTextBox control:
    /// 
    /// <code>
    /// private Color GetLogTextColour(LogEntryProperties messageProperties)
    /// {
    ///     Color textColour = Color.Black;
    /// 
    ///     // Highlight different categories of log messages (eg Error, Warning) in 
    ///     //	different colours to make them stand out.  The categories used are the  
    ///     //	members of the TraceEventType enumeration, converted to text.
    ///     if (messageProperties.Category.Equals(TraceEventType.Critical.ToString(),
    ///             StringComparison.CurrentCultureIgnoreCase)
    ///         || messageProperties.Category.Equals(TraceEventType.Error.ToString(),
    ///             StringComparison.CurrentCultureIgnoreCase))
    ///     {
    ///         textColour = Color.Red;
    ///     }
    ///     else if (messageProperties.Category.Equals(TraceEventType.Warning.ToString(),
    ///         StringComparison.CurrentCultureIgnoreCase))
    ///     {
    ///         textColour = Color.Orange;
    ///     }
    ///     else if (messageProperties.Category.Equals(TraceEventType.Start.ToString(),
    ///         StringComparison.CurrentCultureIgnoreCase)
    ///    || messageProperties.Category.Equals(TraceEventType.Stop.ToString(),
    ///         StringComparison.CurrentCultureIgnoreCase)
    ///    || messageProperties.Category.Equals(TraceEventType.Suspend.ToString(),
    ///         StringComparison.CurrentCultureIgnoreCase)
    ///    || messageProperties.Category.Equals(TraceEventType.Resume.ToString(),
    ///         StringComparison.CurrentCultureIgnoreCase))
    ///     {
    ///         textColour = Color.Blue;
    ///     }
    /// 
    ///     // Can also use highlighting to differentiate between different threads.
    ///     else if (messageProperties.Source.Equals("TimerThread",
    ///             StringComparison.CurrentCultureIgnoreCase))
    ///     {
    ///         textColour = Color.DarkCyan;
    ///     }
    ///     else
    ///     {
    ///         textColour = Color.Black;
    ///     }
    /// 
    ///     return textColour;
    /// }
    /// </code>
    /// </description>
    /// </item>
    /// 
    /// <item>
    /// <description>
    /// The MessageWriter delegate must also have the messageProperties parameter added (as it 
    /// must match the signature of the WriteLogMessage method):
    /// 
    /// <c>private delegate void MessageWriter(string messageToLog, LogEntryProperties messageProperties);</c>
    /// </description>
    /// </item>
    /// 
    /// <item>
    /// <description>
    /// The formLogger_LogMessageEvent event handler in the form must be modified to pass the 
    /// extra parameters to the MessageWriter delegate and the WriteLogMessage method:
    /// 
    /// <code>
    /// private void formLogger_LogMessageEvent(object sender, LogMessageEventArgs e)
    /// {
    ///     string messageToLog = e.MessageToLog;
    ///     LogEntryProperties messageProperties = e.MessageProperties;
    ///
	///		// Add "!this.Disposing" check to avoid ObjectDisposedException if this event handler  
	///		//  runs as the form is being closed (disposed).
	///		if (!this.Disposing)
	///		{
	///			if (this.InvokeRequired)
	///			{
	///				this.BeginInvoke(new MessageWriter(this.WriteLogMessage),
	///					messageToLog, messageProperties);
	///			}
	///			else
	///			{
	///				this.WriteLogMessage(messageToLog, messageProperties);
	///			}
	///		}
    /// }
    /// </code>
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    public class FormTraceListener2 : CustomFormattedTextTraceListener
    {
        /// <summary>
        /// Delegate used to write to form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void LogMessageEventHandler(object sender, LogMessageEventArgs e);

        /// <summary>
        /// Event used to write to form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public event LogMessageEventHandler LogMessageEvent;

        #region Class Data Members ****************************************************************

        #endregion

        #region Constructors and Destructors ******************************************************

        public FormTraceListener2()
            : this(null)
        {
        }

        public FormTraceListener2(string appName)
            : base(appName)
        {
        }

        #endregion

        #region Properties ************************************************************************

        #endregion

        #region Public Methods ********************************************************************


        #endregion

        #region Private & Protected Methods *******************************************************

        /// <summary>
        /// Formats the lines of text to be output then writes them to the log.
        /// </summary>
        /// <param name="logEntryFields">Fields to write to the log.</param>
        /// <param name="doWriteLine">Determines whether to perform a WriteLine or a Write to the log.</param>
        /// <returns>True if successful.</returns>
        protected override bool WriteToCustomLog(LogEntryFields logEntryFields, bool doWriteLine)
        {
            bool isOK = false;

            FormattedLogEntry formattedLogEntry = this.GetFormattedLinesToWrite(logEntryFields, doWriteLine);

            if (formattedLogEntry.LinesToWrite.Count > 0)
            {
                isOK = this.WriteToCustomLog(formattedLogEntry, doWriteLine);
            }
            else
            {
                isOK = true;
            }
            return isOK;
        }

        /// <summary>
        /// Writes the log message to whichever forms have added themselves to the forms collection.
        /// </summary>
        /// <param name="formattedLogEntry">Structure that wraps the formatted lines of text that will be written 
        /// to the log and the category or EventType that applies to the log entry.</param>
        /// <param name="doWriteLine">Determines whether to perform a WriteLine or a Write to the log.</param>
        /// <returns>True if successful.</returns>
        private bool WriteToCustomLog(FormattedLogEntry formattedLogEntry, bool doWriteLine)
        {
            bool isOK = false;
            try
            {
                StringBuilder messageBuilder = new StringBuilder();
                foreach (string lineToWrite in formattedLogEntry.LinesToWrite)
                {
                    if (doWriteLine)
                    {
                        messageBuilder.AppendLine(lineToWrite);
                    }
                    else
                    {
                        messageBuilder.Append(lineToWrite);
                    }
                }

                LogMessageEventArgs logMessageEventArgs
                    = new LogMessageEventArgs(messageBuilder.ToString(),
                        formattedLogEntry.Properties);
                LogMessageEventHandler eventHandler = LogMessageEvent;
                if (eventHandler != null)
                {
                    eventHandler(this, logMessageEventArgs);
                }

                isOK = true;
            }
            catch
            {
                isOK = false;
            }
            return isOK;
        }

        #endregion
    }

    #endregion
}