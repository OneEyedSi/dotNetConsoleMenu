///////////////////////////////////////////////////////////////////////////////////////////////////
// Project      -   Utilities.Database
// General      -   Set of generic data access classes that may be useful in any project.
//
// File Name    -   DatabaseManager.cs
// Description  -   For simplifying database operations (eg running SQL code, running stored 
//                  procedures).
//
// Notes        -   Deprecated in favour of the new class DatabaseManager2.  For new development 
//					use DatabaseManager2 rather than DatabaseManager.  DatabaseManager is only 
//					being retained for backwards compatibility.
//
// $History: DatabaseManager.cs $ 
// 
// *****************  Version 13  *****************
// User: Simone       Date: 9/07/09    Time: 3:25p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// Previous change caused runtime exception: DataTable.DataSet property
// not set automatically when data table created.  Need to add data table
// to data set, set EnforceConstraints off, then remove from data set.
// Must remove from data set as some client code adds data table to
// another data set, and data table cannot be member of two data sets
// simultaneously.
//
// *****************  Version 12  *****************
// User: Simone       Date: 8/07/09    Time: 3:54p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// GetDataTable: Turn off enforsement of constraints on data table as
// seems to be buggy.  Google 'ADO.NET "Failed to enable constraints"' to
// see all the problems people have.
// 
// *****************  Version 9  *****************
// User: Reganp       Date: 15/06/09   Time: 12:49p
// Updated in $/UtilitiesClassLibrary/Utilities.DataAccess/Deprecated
// Created a new overload of GetScalar to pass in SQL Select query and a
// parameter collection.
// 
// *****************  Version 8  *****************
// User: Brentfo      Date: 14/05/09   Time: 2:23p
// Updated in $/UtilitiesClassLibrary/Utilities.DataAccess/Deprecated
// Added another overload for ExecSQL() to take parameters
// 
// *****************  Version 11  *****************
// User: Simone       Date: 5/04/09    Time: 2:34p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// DatabaseManager deprecated, replaced by new DatabaseManager2.
// DatabaseManagerReturnValues and SqlExceptionNumbers enums moved to new
// DatabaseConstants.cs file.
// 
// *****************  Version 10  *****************
// User: Simone       Date: 3/04/09    Time: 7:51p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// GetDataTable overloads for a select query and a stored procedure
// combined into one by adding a CommandType parameter.
// setupIdentityColumn parameter added to GetDataTable method.
// SetupDataTableIdentityColumn method added, to set the seed and step
// values for an auto-increment column in a data table.  Used when a data
// table may have rows added that will later be updated to the source
// database.
// 
// *****************  Version 9  *****************
// User: Simone       Date: 23/03/09   Time: 2:11p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// Made enum SqlExceptionNumbers public rather than private, so it can be
// used by DatabaseUpdater.
// 
// *****************  Version 8  *****************
// User: Simone       Date: 20/03/09   Time: 5:16p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// Shell of UpdateSource method moved to DatabaseUpdater class.
// 
// *****************  Version 7  *****************
// User: Simone       Date: 20/03/09   Time: 5:10p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// New constructor that takes a connection string as an argument.
// GetLoginFromConnString method added.  Two new overloads of GetDataTable
// added with dataAdapter output parameters (to be used by new DataUpdater
// class).
// 
// *****************  Version 6  *****************
// User: Simone       Date: 20/03/09   Time: 11:50a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// Merge in changes from other versions of DatabaseManager.cs (copy of
// changes to Utilities.DataAccess.DatabaseManager in DSL Chch VSS
// repository - version 5).
// 
// *****************  Version 5  *****************
// User: Simone       Date: 9/12/08    Time: 10:59a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// Made the DatabaseManager class thread safe. (copy of changes to
// Utilities.DataAccess.DatabaseManager in DSL Chch VSS repository -
// version 4).
// 
// *****************  Version 4  *****************
// User: Simone       Date: 3/09/08    Time: 9:43a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// Bug Fix - ExecSQL: cmd object was declared inside try block so could not
// clear parameters in finally block.  Moved cmd declaration outside try.
// 
// *****************  Version 3  *****************
// User: Simone       Date: 3/09/08    Time: 9:33a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// Added overload for ExecSQL, allowing client code to pass in SQL
// parameters.
// 
// *****************  Version 1  *****************
// User: Simone       Date: 13/02/08   Time: 11:02a
// Created in $/UtilitiesClassLibrary/Utilities.DataAccess
// Utilities.DataAccess split out of original Utilities project.
// DatabaseManager.cs moved from Utilities project (unchanged).
// 
// *****************  Version 1  *****************
// User: Simone       Date: 24/10/07   Time: 11:58a
// Created in $/UtilitiesClassLibrary/Utilities
// Copied from version 2 in
// $/ServiceAlliance/Interfaces/WebServiceInterfaces/JobsWebServices/Utili
// ties.
///////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Utilities.Database
{
	/// <summary>
	/// Handles database tasks such as retrieving and entering data.
	/// </summary>
	/// <remarks>To update a database based on changes to a DataTable, use the DataUpdater 
	/// class.</remarks>
	public class DatabaseManager : IDisposable
	{
		#region Data Members **********************************************************************

		private System.Data.SqlClient.SqlConnection _conn;
		private string _server;
		private string _database;
		private string _login;
		private int _timeout;
		private int _maxNumAttempts;

		// Use the mutex lock for protecting against concurrency issues
		private object mutexLock = new object();

		#endregion

		#region Constructors, Destructors / Finalizers and Dispose Methods ************************

		/// <summary>
		/// Initializes a new instance of the DatabaseManager class.  Creates a connection 
		/// to the database using integrated security.
		/// </summary>
		/// <param name="server">Server to connect to.</param>
		/// <param name="database">Database to connect to.</param>
		/// <remarks>Exceptions will bubble up to calling procedure.</remarks>
		public DatabaseManager(string server, string database)
			: this(server, database, null, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the DatabaseManager class.  Creates a connection 
		/// to the database using SQL Server authentication.
		/// </summary>
		/// <param name="server">Server to connect to.</param>
		/// <param name="database">Database to connect to.</param>
		/// <param name="login">SQL Server login.</param>
		/// <param name="pwd">SQL Server password.</param>
		/// <remarks>Exceptions will bubble up to calling procedure.</remarks>
		public DatabaseManager(string server, string database, string login, string pwd)
		{
			_server = server;
			_database = database;
			_login = login;
			string connString = BuildConnString(server, database, login, pwd);
			_conn = new SqlConnection(connString);
			ResetTimeoutToDefault();
			_maxNumAttempts = 1;
		}

		/// <summary>
		/// Initializes a new instance of the DatabaseManager class.  Creates a connection 
		/// to the database using a connection string.
		/// </summary>
		/// <param name="connectionString">SqlConnection connection string that contains 
		/// details of the server and database to connect to, and the login details needed to 
		/// connect to it.</param>
		/// <remarks>Exceptions will bubble up to calling procedure.</remarks>
		public DatabaseManager(string connectionString)
		{
			_conn = new SqlConnection(connectionString);
			_server = _conn.DataSource;
			_database = _conn.Database;
			_login = this.GetLoginFromConnString(connectionString);
			ResetTimeoutToDefault();
			_maxNumAttempts = 1;
		}

		/// <summary>
		/// Releases the resources used by the DatabaseManager instance.
		/// </summary>
		public void Dispose()
		{
			lock (mutexLock)  // Multi-threaded protection
			{
				if (_conn != null)
					_conn.Dispose();
			}
		}

		#endregion

		#region Properties ************************************************************************

		/// <summary>
		/// SQL Server that database manager is logged into.
		/// </summary>
		public string Server
		{
			get { return _server; }
			// set {}
		}

		/// <summary>
		/// Database that database manager is connected to.
		/// </summary>
		public string Database
		{
			get { return _database; }
			// set {}
		}

		/// <summary>
		/// Login used to connect to database.
		/// </summary>
		public string Login
		{
			get { return _login; }
			// set {}            
		}

		/// <summary>
		/// Timeout period for commands, in seconds.
		/// </summary>
		public int Timeout
		{
			get { return _timeout; }
			set { _timeout = value; }
		}

		/// <summary>
		/// Maximum number of attempts to perform SQL command.
		/// </summary>
		/// <remarks>In case the database is busy and the SQL command times out, this property allows the 
		/// command to be retried.</remarks>
		public int MaxNumberOfAttempts
		{
			get { return _maxNumAttempts; }
			set { _maxNumAttempts = value; }
		}

		/// <summary>
		/// Connection string used to connect to database.
		/// </summary>
		public string ConnectionString
		{
			get { return _conn.ConnectionString; }
			// set {}            
		}

		/// <summary>
		/// Connection used to connect to database.
		/// </summary>
		public SqlConnection Connection
		{
			get { return _conn; }
			// set {}  
		}

		#endregion

		#region Public Methods ********************************************************************

		/// <summary>
		/// Reset timeout to default value.
		/// </summary>
		public void ResetTimeoutToDefault()
		{
			lock (mutexLock)  // Multi-threaded protection
			{
				using (SqlCommand cmd = new SqlCommand())
				{
					_timeout = cmd.CommandTimeout;
				}
			}
		}

		/// <summary>
		/// Get datatable based on a SQL SELECT query.
		/// </summary>
		/// <param name="SQL">SQL SELECT query.</param>
		/// <returns></returns>
		public DataTable GetDataTable(string SQL)
		{
			int dummyReturnValue;
			string dummyErrorMessage;
			return this.GetDataTable(SQL, CommandType.Text, null, _timeout, _maxNumAttempts, false,
				out dummyReturnValue, out dummyErrorMessage);
		}

		/// <summary>
		/// Get datatable based on a SQL SELECT query.
		/// </summary>
		/// <param name="SQL">SQL SELECT query.</param>
		/// <param name="errorMessage">Output parameter that returns any error message.</param>
		/// <returns></returns>
		public DataTable GetDataTable(string SQL, out string errorMessage)
		{
			int dummyReturnValue;
			return this.GetDataTable(SQL, CommandType.Text, null, _timeout, _maxNumAttempts, false,
				out dummyReturnValue, out errorMessage);
		}

		/// <summary>
		/// Get datatable based on a SQL SELECT query.
		/// </summary>
		/// <param name="SQL">SQL SELECT query.</param>
		/// <param name="timeout">Command timeout, in seconds.</param>
		/// <param name="maxNumAttempts">Maximum number of attempts to execute command (used when command times out).</param>
		/// <param name="errorMessage">Output parameter that returns any error message.</param>
		/// <returns></returns>
		public DataTable GetDataTable(string SQL, int timeout, int maxNumAttempts,
			out string errorMessage)
		{
			int dummyReturnValue;
			return this.GetDataTable(SQL, CommandType.Text, null, timeout, maxNumAttempts, false,
				out dummyReturnValue, out errorMessage);
		}

		/// <summary>
		/// Get datatable based on a SQL SELECT query.
		/// </summary>
		/// <param name="SQL">SQL SELECT query.</param>
		/// <param name="timeout">Command timeout, in seconds.</param>
		/// <param name="aParams">input parameters</param>
		/// <param name="maxNumAttempts">Maximum number of attempts to execute command 
		/// (used when command times out).</param>
		/// <param name="errorMessage">Output parameter that returns any error message.</param>
		/// <returns></returns>
		public DataTable GetDataTable(string SQL, int timeout, SqlParameter[] aParams,
			int maxNumAttempts, out string errorMessage)
		{
			int dummyReturnValue;
			return this.GetDataTable(SQL, CommandType.Text, aParams, timeout, maxNumAttempts, false,
				out dummyReturnValue, out errorMessage);
		}

		/// <summary>
		/// Get datatable based on stored procedure.
		/// </summary>
		/// <param name="storedProc">Stored procedure that returns recordset.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <returns>A datatable.</returns>
		public DataTable GetDataTable(string storedProc, out int returnVal)
		{
			string dummyErrorMessage;
			return this.GetDataTable(storedProc, CommandType.StoredProcedure, null,
				_timeout, _maxNumAttempts, false,
				out returnVal, out dummyErrorMessage);
		}

		/// <summary>
		/// Get datatable based on stored procedure.
		/// </summary>
		/// <param name="storedProc">Stored procedure that returns recordset.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>A datatable.</returns>
		public DataTable GetDataTable(string storedProc, out int returnVal, out string errorMessage)
		{
			return this.GetDataTable(storedProc, CommandType.StoredProcedure, null,
				_timeout, _maxNumAttempts, false,
				out returnVal, out errorMessage);
		}

		/// <summary>
		/// Get datatable based on stored procedure.
		/// </summary>
		/// <param name="storedProc">Stored procedure that returns recordset.</param>
		/// <param name="timeout">Command timeout, in seconds.</param>
		/// <param name="maxNumAttempts">Maximum number of attempts to execute command (used when command times out).</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>A datatable.</returns>
		public DataTable GetDataTable(string storedProc, int timeout, int maxNumAttempts,
			out int returnVal, out string errorMessage)
		{
			return this.GetDataTable(storedProc, CommandType.StoredProcedure, null,
				timeout, maxNumAttempts, false,
				out returnVal, out errorMessage);
		}

		/// <summary>
		/// Get datatable based on stored procedure.
		/// </summary>
		/// <param name="storedProc">Stored procedure that returns recordset.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Only input parameters 
		/// can be included.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <returns>A datatable.</returns>
		public DataTable GetDataTable(string storedProc,
			System.Data.SqlClient.SqlParameter[] aParams, out int returnVal)
		{
			string dummyErrorMessage;
			return this.GetDataTable(storedProc, CommandType.StoredProcedure, aParams,
				_timeout, _maxNumAttempts, false,
				out returnVal, out dummyErrorMessage);
		}

		/// <summary>
		/// Get datatable based on stored procedure.
		/// </summary>
		/// <param name="storedProc">Stored procedure that returns recordset.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Only input parameters 
		/// can be included.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>A datatable.</returns>
		public DataTable GetDataTable(string storedProc,
			System.Data.SqlClient.SqlParameter[] aParams, out int returnVal, out string errorMessage)
		{
			return this.GetDataTable(storedProc, CommandType.StoredProcedure, aParams,
				_timeout, _maxNumAttempts, false,
				out returnVal, out errorMessage);
		}

		/// <summary>
		/// Get datatable based on stored procedure.
		/// </summary>
		/// <param name="storedProc">Stored procedure that returns recordset.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Passed by reference so 
		/// output parameters can be included.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <returns>A datatable.</returns>
		public DataTable GetDataTable(string storedProc,
			ref System.Data.SqlClient.SqlParameter[] aParams, out int returnVal)
		{
			string dummyErrorMessage;
			return this.GetDataTable(storedProc, CommandType.StoredProcedure, aParams,
				_timeout, _maxNumAttempts, false,
				out returnVal, out dummyErrorMessage);
		}

		/// <summary>
		/// Get datatable based on stored procedure.
		/// </summary>
		/// <param name="storedProc">Stored procedure that returns recordset.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Passed by reference so 
		/// output parameters can be included.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>A datatable.</returns>
		public DataTable GetDataTable(string storedProc,
			ref System.Data.SqlClient.SqlParameter[] aParams, out int returnVal,
			out string errorMessage)
		{
			return this.GetDataTable(storedProc, CommandType.StoredProcedure, aParams,
				_timeout, _maxNumAttempts, false,
				out returnVal, out errorMessage);
		}

		/// <summary>
		/// Get datatable based on stored procedure.
		/// </summary>
		/// <param name="storedProc">Stored procedure that returns recordset.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Only input parameters 
		/// can be included.</param>
		/// <param name="timeout">Command timeout, in seconds.</param>
		/// <param name="maxNumAttempts">Maximum number of attempts to execute command (used when command times out).</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>A datatable.</returns>
		public DataTable GetDataTable(string storedProc,
			System.Data.SqlClient.SqlParameter[] aParams, int timeout, int maxNumAttempts,
			out int returnVal, out string errorMessage)
		{
			return this.GetDataTable(storedProc, CommandType.StoredProcedure, aParams,
				timeout, maxNumAttempts, false,
				out returnVal, out errorMessage);
		}

		/// <summary>
		/// Get datatable based on stored procedure.
		/// </summary>
		/// <param name="storedProc">Stored procedure that returns recordset.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Passed by reference so 
		/// output parameters can be included.</param>
		/// <param name="timeout">Command timeout, in seconds.</param>
		/// <param name="maxNumAttempts">Maximum number of attempts to execute command (used when command times out).</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>A datatable.</returns>
		public DataTable GetDataTable(string storedProc,
			ref System.Data.SqlClient.SqlParameter[] aParams, int timeout, int maxNumAttempts,
			out int returnVal, out string errorMessage)
		{
			return this.GetDataTable(storedProc, CommandType.StoredProcedure, aParams,
				timeout, maxNumAttempts, false,
				out returnVal, out errorMessage);
		}

		/// <summary>
		/// Get datatable based on either SQL select query or on stored procedure.
		/// </summary>
		/// <param name="commandText">SQL select query or name of a stored procedure, as appropriate.
		/// </param>
		/// <param name="commandType">Command type: Text for a SQL select query or StoredProcedure 
		/// for a stored procedure.</param>
		/// <param name="aParams">Array of parameters to be passed to the select query or the 
		/// stored procedure.</param>
		/// <param name="timeout">Command timeout, in seconds.</param>
		/// <param name="maxNumAttempts">Maximum number of attempts to execute command 
		/// (used when command times out).</param>
		/// <param name="setupIdentityColumn">If set, sets up any identity column in the data 
		/// table to auto-generate new values if a row is added.  The auto-generated values must 
		/// be negative numbers, to avoid collisions with the identity values created in SQL 
		/// Server.</param>
		/// <param name="returnVal">Output Parameter: Stored procedure return value.  Special 
		/// values returned if an exception occurs. If no exception occurs returns 0 for 
		/// SQL select query.</param>
		/// <param name="errorMessage">Output Parameter: Error message resulting from executing 
		/// the select query or stored procedure.</param>		
		/// <returns>A DataTable.</returns>
		/// <remarks>Must set up the auto-increment seed before populating the data table, 
		/// otherwise the previous maximum (positive) value in the column will be used as the 
		/// seed (regardless of the seed value that is explicitly set).  This will lead to 
		/// collisions with the values created in SQL Server.</remarks>
		public DataTable GetDataTable(string commandText, CommandType commandType,
			SqlParameter[] aParams, int timeout, int maxNumAttempts, bool setupIdentityColumn,
			out int returnVal, out string errorMessage)
		{

			DataTable returnDataTable = new DataTable();
			returnVal = (int)DatabaseManagerReturnValues.GeneralError;
			string oErrorMessage = string.Empty;

			lock (mutexLock)  // Multi-threaded protection
			{
				if (_conn != null)
				{
					SqlCommand cmd = null;
					try
					{
						cmd = new SqlCommand(commandText, _conn);
						cmd.CommandType = commandType;
						cmd.CommandTimeout = timeout;

						// Only used for stored procs.
						SqlParameter prmReturnVal
								= new SqlParameter("@RETURN_VALUE", SqlDbType.Int, 4);

						if (commandType == CommandType.StoredProcedure
							|| (aParams != null && aParams.Length > 0))
						{
							if (commandType == CommandType.StoredProcedure)
							{
								prmReturnVal.Direction = ParameterDirection.ReturnValue;
								cmd.Parameters.Add(prmReturnVal);
							}
							if (aParams != null && aParams.Length > 0)
							{
								cmd.Parameters.AddRange(aParams);
							}
						}

						SqlDataAdapter da = new SqlDataAdapter(cmd);
						da.MissingSchemaAction = MissingSchemaAction.AddWithKey;

						if (_conn.State != ConnectionState.Open)
						{
							_conn.Open();
						}

						da.FillSchema(returnDataTable, SchemaType.Source);
						// Turn off enforcement of constraints on data table as seems to 
						//	be buggy.  Google 'ADO.NET "Failed to enable constraints"' to 
						//	see all the problems people have.
						DataSet returnDataSet = new DataSet();
						returnDataSet.Tables.Add(returnDataTable);
						returnDataSet.EnforceConstraints = false;
						if (setupIdentityColumn)
						{
							SetupDataTableIdentityColumn(returnDataTable);
						}

						int tryCount = 1;
						bool hasTimedOut = false;
						while (tryCount == 1 || (hasTimedOut && tryCount <= maxNumAttempts))
						{
							hasTimedOut = false;
							try
							{
								da.Fill(returnDataTable);
							}
							catch (SqlException ex)
							{
								// Note the "<" whereas it is "<=" in the enclosing loop 
								//	(ie If fails the final time through the loop do not attempt 
								//	to loop again.  Instead re-throw the exception for the 
								//	outer catch block to deal with).
								if (tryCount < maxNumAttempts
								   && (ex.Number == (int)SqlExceptionNumbers.TimeoutExpired
									  || ex.Number == (int)SqlExceptionNumbers.UnableToConnect))
								{
									hasTimedOut = true;
								}
								else
								{
									throw ex;
								}
							}
							tryCount++;
						}
						// Client code expects returned data table to not be part of a data set 
						//	(some of the client code adds the data table to a data set and a 
						//	data table cannot be a member of more than one data set).  So remove 
						//	from data set now that the table has been loaded.
						returnDataSet.Tables.Remove(returnDataTable);

						if (commandType == CommandType.StoredProcedure)
						{
							returnVal = (int)prmReturnVal.Value;
						}
						else
						{
							returnVal = (int)DatabaseManagerReturnValues.Success;
						}
					}
					catch (SqlException ex)
					{
						oErrorMessage = ex.Message;
						if (ex.Number == (int)SqlExceptionNumbers.TimeoutExpired
						   || ex.Number == (int)SqlExceptionNumbers.UnableToConnect)
						{
							returnVal = (int)DatabaseManagerReturnValues.TimeoutExpired;
						}
						else
						{
							returnVal = (int)DatabaseManagerReturnValues.GeneralError;
						}
						returnDataTable = null;
					}
					catch (Exception ex)
					{
						oErrorMessage = ex.Message;
						returnVal = (int)DatabaseManagerReturnValues.GeneralError;
						returnDataTable = null;
					}
					finally
					{
						// Must remove parameters from the command's Parameters collection in case 
						//	the parameters are going to reused in another command (otherwise get 
						//	"The SqlParameter is already contained by another SqlParameterCollection." 
						//	error).
						cmd.Parameters.Clear();

						if (_conn.State != ConnectionState.Closed)
						{
							_conn.Close();
						}
					}
				}
			}

			errorMessage = oErrorMessage;
			return returnDataTable;
		}

		/// <summary>
		/// Execute the specified stored procedure.  If the stored procedure returns a recordset the 
		/// recordset is ignored.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <returns>Stored procedure return value.</returns>
		/// <remarks>Intended for stored procedures that perform an action rather than returning a recordset.
		/// The connection will be closed when the method ends.</remarks>
		public int ExecStoredProc(string storedProc)
		{
			SqlParameter[] aParams = null;
			string dummy = null;
			return ExecStoredProc(storedProc, ref aParams, false, _timeout, _maxNumAttempts,
				out dummy);
		}

		/// <summary>
		/// Execute the specified stored procedure.  If the stored procedure returns a recordset the 
		/// recordset is ignored.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>Stored procedure return value.</returns>
		/// <remarks>Intended for stored procedures that perform an action rather than returning a recordset.
		/// The connection will be closed when the method ends.</remarks>
		public int ExecStoredProc(string storedProc, out string errorMessage)
		{
			SqlParameter[] aParams = null;
			return ExecStoredProc(storedProc, ref aParams, false, _timeout, _maxNumAttempts,
				out errorMessage);
		}

		/// <summary>
		/// Execute the specified stored procedure.  If the stored procedure returns a recordset the 
		/// recordset is ignored.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <returns>Stored procedure return value.</returns>
		/// <remarks>Intended for stored procedures that perform an action rather than returning a recordset.</remarks>
		public int ExecStoredProc(string storedProc, bool leaveConnOpen)
		{
			SqlParameter[] aParams = null;
			string dummy = null;
			return ExecStoredProc(storedProc, ref aParams, leaveConnOpen, _timeout, _maxNumAttempts,
				out dummy);
		}

		/// <summary>
		/// Execute the specified stored procedure.  If the stored procedure returns a recordset the 
		/// recordset is ignored.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>Stored procedure return value.</returns>
		/// <remarks>Intended for stored procedures that perform an action rather than returning a recordset.</remarks>
		public int ExecStoredProc(string storedProc, bool leaveConnOpen, out string errorMessage)
		{
			SqlParameter[] aParams = null;
			return ExecStoredProc(storedProc, ref aParams, leaveConnOpen, _timeout, _maxNumAttempts,
				out errorMessage);
		}

		/// <summary>
		/// Execute the specified stored procedure.  If the stored procedure returns a recordset the 
		/// recordset is ignored.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="timeout">Command timeout, in seconds.</param>
		/// <param name="maxNumAttempts">Maximum number of attempts to execute command (used when command times out).</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>Stored procedure return value.</returns>
		/// <remarks>Intended for stored procedures that perform an action rather than returning a recordset.</remarks>
		public int ExecStoredProc(string storedProc, bool leaveConnOpen,
			int timeout, int maxNumAttempts, out string errorMessage)
		{
			SqlParameter[] aParams = null;
			return ExecStoredProc(storedProc, ref aParams, leaveConnOpen, timeout, maxNumAttempts,
				out errorMessage);
		}

		/// <summary>
		/// Execute the specified stored procedure.  If the stored procedure returns a recordset the 
		/// recordset is ignored.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Only input parameters 
		/// can be included.</param>
		/// <returns>Stored procedure return value.</returns>
		/// <remarks>Intended for stored procedures that perform an action rather than returning a recordset.
		/// The connection will be closed when the method ends.</remarks>
		public int ExecStoredProc(string storedProc, System.Data.SqlClient.SqlParameter[] aParams)
		{
			string dummy = null;
			return ExecStoredProc(storedProc, ref aParams, false, _timeout, _maxNumAttempts,
				out dummy);
		}

		/// <summary>
		/// Execute the specified stored procedure.  If the stored procedure returns a recordset the 
		/// recordset is ignored.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Only input parameters 
		/// can be included.</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>Stored procedure return value.</returns>
		/// <remarks>Intended for stored procedures that perform an action rather than returning a recordset.
		/// The connection will be closed when the method ends.</remarks>
		public int ExecStoredProc(string storedProc, System.Data.SqlClient.SqlParameter[] aParams,
			out string errorMessage)
		{
			return ExecStoredProc(storedProc, ref aParams, false, _timeout, _maxNumAttempts,
				out errorMessage);
		}

		/// <summary>
		/// Execute the specified stored procedure.  If the stored procedure returns a recordset the 
		/// recordset is ignored.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Passed by reference so 
		/// output parameters can be included.</param>
		/// <returns>Stored procedure return value.</returns>
		/// <remarks>Intended for stored procedures that perform an action rather than returning a recordset.
		/// The connection will be closed when the method ends.</remarks>
		public int ExecStoredProc(string storedProc, ref System.Data.SqlClient.SqlParameter[] aParams)
		{
			string dummy = null;
			return ExecStoredProc(storedProc, ref aParams, false, _timeout, _maxNumAttempts,
				out dummy);
		}

		/// <summary>
		/// Execute the specified stored procedure.  If the stored procedure returns a recordset the 
		/// recordset is ignored.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Passed by reference so 
		/// output parameters can be included.</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>Stored procedure return value.</returns>
		/// <remarks>Intended for stored procedures that perform an action rather than returning a recordset.
		/// The connection will be closed when the method ends.</remarks>
		public int ExecStoredProc(string storedProc, ref System.Data.SqlClient.SqlParameter[] aParams,
			out string errorMessage)
		{
			return ExecStoredProc(storedProc, ref aParams, false, _timeout, _maxNumAttempts,
				out errorMessage);
		}

		/// <summary>
		/// Execute the specified stored procedure.  If the stored procedure returns a recordset the 
		/// recordset is ignored.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Only input parameters 
		/// can be included.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <returns>Stored procedure return value.</returns>
		/// <remarks>Intended for stored procedures that perform an action rather than returning a recordset.</remarks>
		public int ExecStoredProc(string storedProc, System.Data.SqlClient.SqlParameter[] aParams,
			bool leaveConnOpen)
		{
			string dummy = null;
			return ExecStoredProc(storedProc, ref aParams, leaveConnOpen, _timeout, _maxNumAttempts,
				out dummy);
		}

		/// <summary>
		/// Execute the specified stored procedure.  If the stored procedure returns a recordset the 
		/// recordset is ignored.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Only input parameters 
		/// can be included.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>Stored procedure return value.</returns>
		/// <remarks>Intended for stored procedures that perform an action rather than returning a recordset.</remarks>
		public int ExecStoredProc(string storedProc, System.Data.SqlClient.SqlParameter[] aParams,
			bool leaveConnOpen, out string errorMessage)
		{
			return ExecStoredProc(storedProc, ref aParams, leaveConnOpen, _timeout, _maxNumAttempts,
				out errorMessage);
		}

		/// <summary>
		/// Execute the specified stored procedure.  If the stored procedure returns a recordset the 
		/// recordset is ignored.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Passed by reference so 
		/// output parameters can be included.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <returns>Stored procedure return value.</returns>
		/// <remarks>Intended for stored procedures that perform an action rather than returning a recordset.</remarks>
		public int ExecStoredProc(string storedProc, ref System.Data.SqlClient.SqlParameter[] aParams,
			bool leaveConnOpen)
		{
			string dummy = null;
			return ExecStoredProc(storedProc, ref aParams, leaveConnOpen, _timeout, _maxNumAttempts,
				out dummy);
		}

		/// <summary>
		/// Execute the specified stored procedure.  If the stored procedure returns a recordset the 
		/// recordset is ignored.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Passed by reference so 
		/// output parameters can be included.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// <returns>Stored procedure return value.</returns>
		/// <remarks>Intended for stored procedures that perform an action rather than returning a recordset.</remarks>
		public int ExecStoredProc(string storedProc, ref System.Data.SqlClient.SqlParameter[] aParams,
			bool leaveConnOpen, out string errorMessage)
		{
			return ExecStoredProc(storedProc, ref aParams, leaveConnOpen, _timeout, _maxNumAttempts,
				out errorMessage);
		}

		/// <summary>
		/// Execute the specified stored procedure.  If the stored procedure returns a recordset the 
		/// recordset is ignored.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Only input parameters 
		/// can be included.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="timeout">Command timeout, in seconds.</param>
		/// <param name="maxNumAttempts">Maximum number of attempts to execute command (used when command times out).</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>Stored procedure return value.</returns>
		/// <remarks>Intended for stored procedures that perform an action rather than returning a recordset.</remarks>
		public int ExecStoredProc(string storedProc, System.Data.SqlClient.SqlParameter[] aParams,
			bool leaveConnOpen, int timeout, int maxNumAttempts, out string errorMessage)
		{
			return ExecStoredProc(storedProc, ref aParams, leaveConnOpen, timeout, maxNumAttempts,
				out errorMessage);
		}

		/// <summary>
		/// Execute the specified stored procedure.  If the stored procedure returns a recordset the 
		/// recordset is ignored.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Passed by reference so 
		/// output parameters can be included.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="timeout">Command timeout, in seconds.</param>
		/// <param name="maxNumAttempts">Maximum number of attempts to execute command (used when command times out).</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>Stored procedure return value.</returns>
		/// <remarks>Intended for stored procedures that perform an action rather than returning a recordset.</remarks>
		public int ExecStoredProc(string storedProc, ref System.Data.SqlClient.SqlParameter[] aParams,
			bool leaveConnOpen, int timeout, int maxNumAttempts, out string errorMessage)
		{
			int returnVal = (int)DatabaseManagerReturnValues.GeneralError;
			string oErrorMessage = string.Empty;

			lock (mutexLock)  // Multi-threaded protection
			{
				if (_conn != null)
				{
					SqlCommand cmd = null;
					try
					{
						using (cmd = new SqlCommand(storedProc, _conn))
						{
							cmd.CommandType = CommandType.StoredProcedure;
							cmd.CommandTimeout = timeout;

							SqlParameter prmReturnVal = new SqlParameter("@RETURN_VALUE", SqlDbType.Int, 4);
							prmReturnVal.Direction = ParameterDirection.ReturnValue;
							cmd.Parameters.Add(prmReturnVal);

							if (aParams != null)
							{
								foreach (SqlParameter prm in aParams)
								{
									cmd.Parameters.Add(prm);
								}
							}

							int tryCount = 1;
							bool hasTimedOut = false;

							// Only open the connection if it is not already open.
							if (_conn.State != ConnectionState.Open)
							{
								_conn.Open();
							}
							while (tryCount == 1 || (hasTimedOut && tryCount <= maxNumAttempts))
							{
								hasTimedOut = false;
								try
								{
									cmd.ExecuteNonQuery();
									returnVal = (int)prmReturnVal.Value;
								}
								catch (SqlException xcp)
								{
									// Note the "<" whereas it is "<=" in the enclosing loop (ie If fails the final 
									//	time through the loop do not attempt to loop again.  Instead re-throw the 
									//	exception for the outer catch block to deal with).
									if (tryCount < maxNumAttempts
									   && (xcp.Number == (int)SqlExceptionNumbers.TimeoutExpired
										  || xcp.Number == (int)SqlExceptionNumbers.UnableToConnect))
									{
										hasTimedOut = true;
									}
									else
									{
										throw xcp;
									}
								}
								tryCount++;
							}
						}
					}
					catch (SqlException xcp)
					{
						oErrorMessage = xcp.Message;
						if (xcp.Number == (int)SqlExceptionNumbers.TimeoutExpired
						   || xcp.Number == (int)SqlExceptionNumbers.UnableToConnect)
						{
							returnVal = (int)DatabaseManagerReturnValues.TimeoutExpired;
						}
						else
						{
							returnVal = (int)DatabaseManagerReturnValues.GeneralError;
						}
					}
					catch (Exception xcp)
					{
						oErrorMessage = xcp.Message;
						returnVal = (int)DatabaseManagerReturnValues.GeneralError;
					}
					finally
					{
						// Must remove parameters from the command's Parameters collection in case 
						//	the parameters are going to reused in another command (otherwise get 
						//	"The SqlParameter is already contained by another SqlParameterCollection." 
						//	error).
						cmd.Parameters.Clear();

						// Close connection if leaveConnOpen is false.
						if (!leaveConnOpen
						   && _conn.State != ConnectionState.Closed
						   )
						{
							_conn.Close();
						}
					}
				}
			}

			errorMessage = oErrorMessage;
			return returnVal;
		}

		/// <summary>
		/// Executes the specified SQL statement.  If the command returns a recordset the recordset is 
		/// ignored.
		/// </summary>
		/// <param name="SQL">SQL statement to execute.</param>
		/// <returns>True if successful.</returns>
		/// <remarks>Intended for SQL statements that perform an action rather than returning a recordset.
		/// The connection will be closed when the method ends.</remarks>
		public bool ExecSQL(string SQL)
		{
			string dummy = null;
			return ExecSQL(SQL, false, _timeout, _maxNumAttempts, out dummy);
		}

      /// <summary>
      /// Executes the specified SQL statement.  If the command returns a recordset the recordset is 
      /// ignored.
      /// </summary>
      /// <param name="SQL">SQL statement to execute.</param>
      /// <param name="parameters">The SQL parameters for the query.</param>
      /// <returns>True if successful.</returns>
      /// <remarks>Intended for SQL statements that perform an action rather than returning a recordset.
      /// The connection will be closed when the method ends.</remarks>
      public bool ExecSQL(string SQL, SqlParameter[] parameters)
      {
         string dummy = null;
         return ExecSQL(SQL, false, parameters, _timeout, _maxNumAttempts, out dummy);
      }

		/// <summary>
		/// Executes the specified SQL statement.  If the command returns a recordset the recordset is 
		/// ignored.
		/// </summary>
		/// <param name="SQL">SQL statement to execute.</param>
		/// <param name="errorMessage">Error message resulting from executing SQL statement (output parameter).</param>
		/// <returns>True if successful.</returns>
		/// <remarks>Intended for SQL statements that perform an action rather than returning a recordset.
		/// The connection will be closed when the method ends.</remarks>
		public bool ExecSQL(string SQL, out string errorMessage)
		{
			return ExecSQL(SQL, false, _timeout, _maxNumAttempts, out errorMessage);
		}

		/// <summary>
		/// Executes the specified SQL statement.  If the command returns a recordset the recordset is 
		/// ignored.
		/// </summary>
		/// <param name="SQL">SQL statement to execute.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <returns>True if successful.</returns>
		/// <remarks>Intended for SQL statements that perform an action rather than returning a recordset.</remarks>
		public bool ExecSQL(string SQL, bool leaveConnOpen)
		{
			string dummy = null;
			return ExecSQL(SQL, leaveConnOpen, _timeout, _maxNumAttempts, out dummy);
		}

		/// <summary>
		/// Executes the specified SQL statement.  If the command returns a recordset the recordset is 
		/// ignored.
		/// </summary>
		/// <param name="SQL">SQL statement to execute.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="errorMessage">Error message resulting from executing SQL statement (output parameter).</param>
		/// <returns>True if successful.</returns>
		/// <remarks>Intended for SQL statements that perform an action rather than returning a recordset.</remarks>
		public bool ExecSQL(string SQL, bool leaveConnOpen, out string errorMessage)
		{
			return ExecSQL(SQL, leaveConnOpen, _timeout, _maxNumAttempts, out errorMessage);
		}

		/// <summary>
		/// Executes the specified SQL statement.  If the command returns a recordset the recordset is 
		/// ignored.
		/// </summary>
		/// <param name="SQL">SQL statement to execute.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="timeout">Command timeout, in seconds.</param>
		/// <param name="maxNumAttempts">Maximum number of attempts to execute command (used when command times out).</param>
		/// <param name="errorMessage">Error message resulting from executing SQL statement (output parameter).</param>
		/// <returns>True if successful.</returns>
		/// <remarks>Intended for SQL statements that perform an action rather than returning a recordset.</remarks>
		public bool ExecSQL(string SQL, bool leaveConnOpen, int timeout, int maxNumAttempts,
			out string errorMessage)
		{
			return this.ExecSQL(SQL, leaveConnOpen, null, timeout, maxNumAttempts,
				out errorMessage);
		}


		/// <summary>
		/// Executes the specified SQL statement.  If the command returns a recordset the recordset is 
		/// ignored.
		/// </summary>
		/// <param name="SQL">SQL statement to execute.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="aParams">The parameters for the query.</param>
		/// <param name="timeout">Command timeout, in seconds.</param>
		/// <param name="maxNumAttempts">Maximum number of attempts to execute command (used when command times out).</param>
		/// <param name="errorMessage">Error message resulting from executing SQL statement (output parameter).</param>
		/// <returns>True if successful.</returns>
		/// <remarks>Intended for SQL statements that perform an action rather than returning a recordset.</remarks>
		public bool ExecSQL(string SQL, bool leaveConnOpen, SqlParameter[] aParams, int timeout,
			int maxNumAttempts, out string errorMessage)
		{
			bool wasExecutedOK = false;
			string oErrorMessage = string.Empty;

			lock (mutexLock)  // Multi-threaded protection
			{
				if (_conn != null)
				{
					SqlCommand cmd = null;
					try
					{
						cmd = new SqlCommand(SQL, _conn);
						cmd.CommandType = CommandType.Text;
						cmd.CommandTimeout = timeout;

						if (aParams != null)
						{
							foreach (SqlParameter param in aParams)
							{
								cmd.Parameters.Add(param);
							}
						}

						int tryCount = 1;
						bool hasTimedOut = false;

						// Only open the connection if it is not already open.
						if (_conn.State != ConnectionState.Open)
						{
							_conn.Open();
						}

						while (tryCount == 1 || (hasTimedOut && tryCount <= maxNumAttempts))
						{
							hasTimedOut = false;
							try
							{
								cmd.ExecuteNonQuery();
								wasExecutedOK = true;
							}
							catch (SqlException xcp)
							{
								// Note the "<" whereas it is "<=" in the enclosing loop (ie If fails the final 
								//	time through the loop do not attempt to loop again.  Instead re-throw the 
								//	exception for the outer catch block to deal with).
								if (tryCount < maxNumAttempts
								   && (xcp.Number == (int)SqlExceptionNumbers.TimeoutExpired
									  || xcp.Number == (int)SqlExceptionNumbers.UnableToConnect))
								{
									hasTimedOut = true;
								}
								else
								{
									throw xcp;
								}
							}
							tryCount++;
						}
					}
					catch (Exception xcp)
					{
						oErrorMessage = xcp.Message;
						wasExecutedOK = false;
					}
					finally
					{
						// Must remove parameters from the command's Parameters collection in case 
						//	the parameters are going to reused in another command (otherwise get 
						//	"The SqlParameter is already contained by another SqlParameterCollection." 
						//	error).
						cmd.Parameters.Clear();

						// Close connection if leaveConnOpen is false.
						if (!leaveConnOpen
						   && _conn.State != ConnectionState.Closed
						   )
						{
							_conn.Close();
						}
					}
				}
			}
			errorMessage = oErrorMessage;
			return wasExecutedOK;
		}

		/// <summary>
		/// Executes the specified SQL SELECT query and returns a scalar value from it.
		/// </summary>
		/// <param name="SQL">SQL SELECT query.</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the SELECT query 
		/// result set.  Any further columns or rows will be ignored.  The connection will be closed when the method ends.</remarks>
		public T GetScalar<T>(string SQL)
		{
			string dummy = null;
			return GetScalar<T>(SQL, false, _timeout, _maxNumAttempts, out dummy);
		}

		/// <summary>
		/// Executes the specified SQL SELECT query and returns a scalar value from it.
		/// </summary>
		/// <param name="SQL">SQL SELECT query.</param>
		/// <param name="errorMessage">Error message resulting from executing SQL statement (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the SELECT query 
		/// result set.  Any further columns or rows will be ignored.  The connection will be closed when the method ends.</remarks>
		public T GetScalar<T>(string SQL, out string errorMessage)
		{
			return GetScalar<T>(SQL, false, _timeout, _maxNumAttempts, out errorMessage);
		}

		/// <summary>
		/// Executes the specified SQL SELECT query and returns a scalar value from it.
		/// </summary>
		/// <param name="SQL">SQL SELECT query.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the SELECT query 
		/// result set.  Any further columns or rows will be ignored.</remarks>
		public T GetScalar<T>(string SQL, bool leaveConnOpen)
		{
			string dummy = null;
			return GetScalar<T>(SQL, leaveConnOpen, _timeout, _maxNumAttempts, out dummy);
		}

		/// <summary>
		/// Executes the specified SQL SELECT query and returns a scalar value from it.
		/// </summary>
		/// <param name="SQL">SQL SELECT query.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="errorMessage">Error message resulting from executing SQL statement (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the SELECT query 
		/// result set.  Any further columns or rows will be ignored.</remarks>
		public T GetScalar<T>(string SQL, bool leaveConnOpen, out string errorMessage)
		{
			return GetScalar<T>(SQL, leaveConnOpen, _timeout, _maxNumAttempts, out errorMessage);
		}

		/// <summary>
		/// Executes the specified SQL SELECT query and returns a scalar value from it.
		/// </summary>
		/// <param name="SQL">SQL SELECT query.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="timeout">Command timeout, in seconds.</param>
		/// <param name="maxNumAttempts">Maximum number of attempts to execute command (used when command times out).</param>
		/// <param name="errorMessage">Error message resulting from executing SQL statement (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the SELECT query 
		/// result set.  Any further columns or rows will be ignored.</remarks>
		public T GetScalar<T>(string SQL, bool leaveConnOpen, int timeout, int maxNumAttempts,
			out string errorMessage)
		{
			SqlParameter[] aParams = null;
			object objReturnValue = GetScalar(SQL, leaveConnOpen, timeout, maxNumAttempts, aParams, out errorMessage);
			T returnValue = default(T);
			try
			{
				returnValue = (T)objReturnValue;
			}
			catch
			{
				// Do nothing - default value already set.
			}
			return returnValue;
		}

		/// <summary>
		/// Executes the specified SQL SELECT query and returns a scalar value from it.
		/// </summary>
		/// <param name="SQL">SQL SELECT query.</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the SELECT query 
		/// result set.  Any further columns or rows will be ignored.  The connection will be closed when the method ends.</remarks>
		public object GetScalar(string SQL)
		{
			string dummy = null;
			SqlParameter[] aParams = null;
			return GetScalar(SQL, false, _timeout, _maxNumAttempts, aParams, out dummy);
		}

		/// <summary>
		/// Executes the specified SQL SELECT query and returns a scalar value from it.
		/// </summary>
		/// <param name="SQL">SQL SELECT query.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Only input parameters 
		/// can be included.</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the SELECT query 
		/// result set.  Any further columns or rows will be ignored.  The connection will be closed when the method ends.</remarks>
		public object GetScalar(string SQL, SqlParameter[] aParams)
		{
			string dummy = null;
			return GetScalar(SQL, false, _timeout, _maxNumAttempts, aParams, out dummy);
		}

		/// <summary>
		/// Executes the specified SQL SELECT query and returns a scalar value from it.
		/// </summary>
		/// <param name="SQL">SQL SELECT query.</param>
		/// <param name="errorMessage">Error message resulting from executing SQL statement (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the SELECT query 
		/// result set.  Any further columns or rows will be ignored.  The connection will be closed when the method ends.</remarks>
		public object GetScalar(string SQL, out string errorMessage)
		{
			SqlParameter[] aParams = null;
			return GetScalar(SQL, false, _timeout, _maxNumAttempts, aParams, out errorMessage);
		}

		/// <summary>
		/// Executes the specified SQL SELECT query and returns a scalar value from it.
		/// </summary>
		/// <param name="SQL">SQL SELECT query.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the SELECT query 
		/// result set.  Any further columns or rows will be ignored.</remarks>
		public object GetScalar(string SQL, bool leaveConnOpen)
		{
			string dummy = null;
			SqlParameter[] aParams = null;
			return GetScalar(SQL, leaveConnOpen, _timeout, _maxNumAttempts, aParams, out dummy);
		}

		/// <summary>
		/// Executes the specified SQL SELECT query and returns a scalar value from it.
		/// </summary>
		/// <param name="SQL">SQL SELECT query.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="errorMessage">Error message resulting from executing SQL statement (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the SELECT query 
		/// result set.  Any further columns or rows will be ignored.</remarks>
		public object GetScalar(string SQL, bool leaveConnOpen, out string errorMessage)
		{
			SqlParameter[] aParams = null;
			return GetScalar(SQL, leaveConnOpen, _timeout, _maxNumAttempts, aParams, out errorMessage);
		}

		/// <summary>
		/// Executes the specified SQL SELECT query and returns a scalar value from it.
		/// </summary>
		/// <param name="SQL">SQL SELECT query.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="timeout">Command timeout, in seconds.</param>
		/// <param name="maxNumAttempts">Maximum number of attempts to execute command (used when command times out).</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Only input parameters 
		/// can be included.</param>
		/// <param name="errorMessage">Error message resulting from executing SQL statement (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the SELECT query 
		/// result set.  Any further columns or rows will be ignored.</remarks>
		public object GetScalar(string SQL, bool leaveConnOpen, int timeout, int maxNumAttempts, SqlParameter[] aParams,
			out string errorMessage)
		{
			object objReturned = null;
			string oErrorMessage = string.Empty;

			lock (mutexLock)  // Multi-threaded protection
			{
				if (_conn != null)
				{
					try
					{
						SqlCommand cmd = new SqlCommand(SQL, _conn);
						cmd.CommandType = CommandType.Text;
						cmd.CommandTimeout = timeout;

						if (aParams != null)
						{
							foreach (SqlParameter prm in aParams)
							{
								cmd.Parameters.Add(prm);
							}
						}

						int tryCount = 1;
						bool hasTimedOut = false;

						// Only open the connection if it is not already open.
						if (_conn.State != ConnectionState.Open)
						{
							_conn.Open();
						}

						while (tryCount == 1 || (hasTimedOut && tryCount <= maxNumAttempts))
						{
							hasTimedOut = false;
							try
							{
								objReturned = cmd.ExecuteScalar();
							}
							catch (SqlException xcp)
							{
								// Note the "<" whereas it is "<=" in the enclosing loop (ie If fails the final 
								//	time through the loop do not attempt to loop again.  Instead re-throw the 
								//	exception for the outer catch block to deal with).
								if (tryCount < maxNumAttempts
								   && (xcp.Number == (int)SqlExceptionNumbers.TimeoutExpired
									  || xcp.Number == (int)SqlExceptionNumbers.UnableToConnect))
								{
									hasTimedOut = true;
								}
								else
								{
									throw xcp;
								}
							}
							tryCount++;
						}
					}
					catch (Exception xcp)
					{
						oErrorMessage = xcp.Message;
						objReturned = null;
					}
					finally
					{
						// Close connection if leaveConnOpen is false.
						if (!leaveConnOpen
						   && _conn.State != ConnectionState.Closed
						   )
						{
							_conn.Close();
						}
					}
				}
			}

			errorMessage = oErrorMessage;
			return objReturned;
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <typeparam name="T">Data type of value that will be output.</typeparam>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.  The connection will be closed when the method ends.</remarks>
		public T GetScalar<T>(string storedProc, out int returnVal)
		{
			SqlParameter[] aParams = null;
			string dummy = null;
			return GetScalar<T>(storedProc, ref aParams, false, _timeout, _maxNumAttempts,
				out returnVal, out dummy);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <typeparam name="T">Data type of value that will be output.</typeparam>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.  The connection will be closed when the method ends.</remarks>
		public T GetScalar<T>(string storedProc, out int returnVal, out string errorMessage)
		{
			SqlParameter[] aParams = null;
			return GetScalar<T>(storedProc, ref aParams, false, _timeout, _maxNumAttempts,
				out returnVal, out errorMessage);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <typeparam name="T">Data type of value that will be output.</typeparam>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.</remarks>
		public T GetScalar<T>(string storedProc, bool leaveConnOpen, out int returnVal)
		{
			SqlParameter[] aParams = null;
			string dummy = null;
			return GetScalar<T>(storedProc, ref aParams, leaveConnOpen, _timeout, _maxNumAttempts,
				out returnVal, out dummy);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <typeparam name="T">Data type of value that will be output.</typeparam>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.</remarks>
		public T GetScalar<T>(string storedProc, bool leaveConnOpen, out int returnVal, out string errorMessage)
		{
			SqlParameter[] aParams = null;
			return GetScalar<T>(storedProc, ref aParams, leaveConnOpen, _timeout, _maxNumAttempts,
				out returnVal, out errorMessage);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <typeparam name="T">Data type of value that will be output.</typeparam>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="timeout">Command timeout, in seconds.</param>
		/// <param name="maxNumAttempts">Maximum number of attempts to execute command (used when command times out).</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.</remarks>
		public T GetScalar<T>(string storedProc, bool leaveConnOpen,
			int timeout, int maxNumAttempts, out int returnVal, out string errorMessage)
		{
			SqlParameter[] aParams = null;
			return GetScalar<T>(storedProc, ref aParams, leaveConnOpen, timeout, maxNumAttempts,
				out returnVal, out errorMessage);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <typeparam name="T">Data type of value that will be output.</typeparam>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Only input parameters 
		/// can be included.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.  The connection will be closed when the method ends.</remarks>
		public T GetScalar<T>(string storedProc, System.Data.SqlClient.SqlParameter[] aParams,
			out int returnVal)
		{
			string dummy = null;
			return GetScalar<T>(storedProc, ref aParams, false, _timeout, _maxNumAttempts,
				out returnVal, out dummy);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <typeparam name="T">Data type of value that will be output.</typeparam>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Only input parameters 
		/// can be included.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.  The connection will be closed when the method ends.</remarks>
		public T GetScalar<T>(string storedProc, System.Data.SqlClient.SqlParameter[] aParams,
			out int returnVal, out string errorMessage)
		{
			return GetScalar<T>(storedProc, ref aParams, false, _timeout, _maxNumAttempts,
				out returnVal, out errorMessage);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <typeparam name="T">Data type of value that will be output.</typeparam>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Passed by reference so 
		/// output parameters can be included.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.  The connection will be closed when the method ends.</remarks>
		public T GetScalar<T>(string storedProc, ref System.Data.SqlClient.SqlParameter[] aParams,
			out int returnVal)
		{
			string dummy = null;
			return GetScalar<T>(storedProc, ref aParams, false, _timeout, _maxNumAttempts,
				out returnVal, out dummy);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <typeparam name="T">Data type of value that will be output.</typeparam>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Passed by reference so 
		/// output parameters can be included.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.  The connection will be closed when the method ends.</remarks>
		public T GetScalar<T>(string storedProc, ref System.Data.SqlClient.SqlParameter[] aParams,
			out int returnVal, out string errorMessage)
		{
			return GetScalar<T>(storedProc, ref aParams, false, _timeout, _maxNumAttempts,
				out returnVal, out errorMessage);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <typeparam name="T">Data type of value that will be output.</typeparam>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Only input parameters 
		/// can be included.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.</remarks>
		public T GetScalar<T>(string storedProc, System.Data.SqlClient.SqlParameter[] aParams,
			bool leaveConnOpen, out int returnVal)
		{
			string dummy = null;
			return GetScalar<T>(storedProc, ref aParams, leaveConnOpen, _timeout, _maxNumAttempts,
				out returnVal, out dummy);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <typeparam name="T">Data type of value that will be output.</typeparam>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Passed by reference so 
		/// output parameters can be included.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.</remarks>
		public T GetScalar<T>(string storedProc, ref System.Data.SqlClient.SqlParameter[] aParams,
			bool leaveConnOpen, out int returnVal)
		{
			string dummy = null;
			return GetScalar<T>(storedProc, ref aParams, leaveConnOpen, _timeout, _maxNumAttempts,
				out returnVal, out dummy);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <typeparam name="T">Data type of value that will be output.</typeparam>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Only input parameters 
		/// can be included.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.</remarks>
		public T GetScalar<T>(string storedProc, System.Data.SqlClient.SqlParameter[] aParams,
			bool leaveConnOpen, out int returnVal, out string errorMessage)
		{
			return GetScalar<T>(storedProc, ref aParams, leaveConnOpen, _timeout, _maxNumAttempts,
				out returnVal, out errorMessage);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <typeparam name="T">Data type of value that will be output.</typeparam>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Only input parameters 
		/// can be included.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.</remarks>
		public T GetScalar<T>(string storedProc, ref System.Data.SqlClient.SqlParameter[] aParams,
			bool leaveConnOpen, out int returnVal, out string errorMessage)
		{
			return GetScalar<T>(storedProc, ref aParams, leaveConnOpen, _timeout, _maxNumAttempts,
				out returnVal, out errorMessage);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <typeparam name="T">Data type of value that will be output.</typeparam>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Only input parameters 
		/// can be included.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="timeout">Command timeout, in seconds.</param>
		/// <param name="maxNumAttempts">Maximum number of attempts to execute command (used when command times out).</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.</remarks>
		public T GetScalar<T>(string storedProc, System.Data.SqlClient.SqlParameter[] aParams,
			bool leaveConnOpen, int timeout, int maxNumAttempts, out int returnVal, out string errorMessage)
		{
			return GetScalar<T>(storedProc, ref aParams, leaveConnOpen, timeout, maxNumAttempts,
				out returnVal, out errorMessage);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <typeparam name="T">Data type of value that will be output.</typeparam>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Passed by reference so 
		/// output parameters can be included.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="timeout">Command timeout, in seconds.</param>
		/// <param name="maxNumAttempts">Maximum number of attempts to execute command (used when command times out).</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.</remarks>
		public T GetScalar<T>(string storedProc, ref System.Data.SqlClient.SqlParameter[] aParams,
			bool leaveConnOpen, int timeout, int maxNumAttempts, out int returnVal, out string errorMessage)
		{
			object objReturnValue = GetScalar(storedProc, ref aParams, leaveConnOpen, timeout, maxNumAttempts,
				out returnVal, out errorMessage);
			T returnValue = default(T);
			try
			{
				returnValue = (T)objReturnValue;
			}
			catch
			{
				// Do nothing - default value already set.
			}
			return returnValue;
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.  The connection will be closed when the method ends.</remarks>
		public object GetScalar(string storedProc, out int returnVal)
		{
			SqlParameter[] aParams = null;
			string dummy = null;
			return GetScalar(storedProc, ref aParams, false, _timeout, _maxNumAttempts,
				out returnVal, out dummy);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.  The connection will be closed when the method ends.</remarks>
		public object GetScalar(string storedProc, out int returnVal, out string errorMessage)
		{
			SqlParameter[] aParams = null;
			return GetScalar(storedProc, ref aParams, false, _timeout, _maxNumAttempts,
				out returnVal, out errorMessage);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.</remarks>
		public object GetScalar(string storedProc, bool leaveConnOpen, out int returnVal)
		{
			SqlParameter[] aParams = null;
			string dummy = null;
			return GetScalar(storedProc, ref aParams, leaveConnOpen, _timeout, _maxNumAttempts,
				out returnVal, out dummy);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.</remarks>
		public object GetScalar(string storedProc, bool leaveConnOpen, out int returnVal, out string errorMessage)
		{
			SqlParameter[] aParams = null;
			return GetScalar(storedProc, ref aParams, leaveConnOpen, _timeout, _maxNumAttempts,
				out returnVal, out errorMessage);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="timeout">Command timeout, in seconds.</param>
		/// <param name="maxNumAttempts">Maximum number of attempts to execute command (used when command times out).</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.</remarks>
		public object GetScalar(string storedProc, bool leaveConnOpen,
			int timeout, int maxNumAttempts, out int returnVal, out string errorMessage)
		{
			SqlParameter[] aParams = null;
			return GetScalar(storedProc, ref aParams, leaveConnOpen, timeout, maxNumAttempts,
				out returnVal, out errorMessage);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Only input parameters 
		/// can be included.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.  The connection will be closed when the method ends.</remarks>
		public object GetScalar(string storedProc, System.Data.SqlClient.SqlParameter[] aParams,
			out int returnVal)
		{
			string dummy = null;
			return GetScalar(storedProc, ref aParams, false, _timeout, _maxNumAttempts,
				out returnVal, out dummy);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Only input parameters 
		/// can be included.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.  The connection will be closed when the method ends.</remarks>
		public object GetScalar(string storedProc, System.Data.SqlClient.SqlParameter[] aParams,
			out int returnVal, out string errorMessage)
		{
			return GetScalar(storedProc, ref aParams, false, _timeout, _maxNumAttempts,
				out returnVal, out errorMessage);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Passed by reference so 
		/// output parameters can be included.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.  The connection will be closed when the method ends.</remarks>
		public object GetScalar(string storedProc, ref System.Data.SqlClient.SqlParameter[] aParams,
			out int returnVal)
		{
			string dummy = null;
			return GetScalar(storedProc, ref aParams, false, _timeout, _maxNumAttempts,
				out returnVal, out dummy);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Passed by reference so 
		/// output parameters can be included.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.  The connection will be closed when the method ends.</remarks>
		public object GetScalar(string storedProc, ref System.Data.SqlClient.SqlParameter[] aParams,
			out int returnVal, out string errorMessage)
		{
			return GetScalar(storedProc, ref aParams, false, _timeout, _maxNumAttempts,
				out returnVal, out errorMessage);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Only input parameters 
		/// can be included.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.</remarks>
		public object GetScalar(string storedProc, System.Data.SqlClient.SqlParameter[] aParams,
			bool leaveConnOpen, out int returnVal)
		{
			string dummy = null;
			return GetScalar(storedProc, ref aParams, leaveConnOpen, _timeout, _maxNumAttempts,
				out returnVal, out dummy);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Only input parameters 
		/// can be included.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.</remarks>
		public object GetScalar(string storedProc, System.Data.SqlClient.SqlParameter[] aParams,
			bool leaveConnOpen, out int returnVal, out string errorMessage)
		{
			return GetScalar(storedProc, ref aParams, leaveConnOpen, _timeout, _maxNumAttempts,
				out returnVal, out errorMessage);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Passed by reference so 
		/// output parameters can be included.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.</remarks>
		public object GetScalar(string storedProc, ref System.Data.SqlClient.SqlParameter[] aParams,
			bool leaveConnOpen, out int returnVal)
		{
			string dummy = null;
			return GetScalar(storedProc, ref aParams, leaveConnOpen, _timeout, _maxNumAttempts,
				out returnVal, out dummy);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Passed by reference so 
		/// output parameters can be included.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.</remarks>
		public object GetScalar(string storedProc, ref System.Data.SqlClient.SqlParameter[] aParams,
			bool leaveConnOpen, out int returnVal, out string errorMessage)
		{
			return GetScalar(storedProc, ref aParams, leaveConnOpen, _timeout, _maxNumAttempts,
				out returnVal, out errorMessage);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Only input parameters 
		/// can be included.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="timeout">Command timeout, in seconds.</param>
		/// <param name="maxNumAttempts">Maximum number of attempts to execute command (used when command times out).</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.</remarks>
		public object GetScalar(string storedProc, System.Data.SqlClient.SqlParameter[] aParams,
			bool leaveConnOpen, int timeout, int maxNumAttempts, out int returnVal, out string errorMessage)
		{
			return GetScalar(storedProc, ref aParams, leaveConnOpen, timeout, maxNumAttempts,
				out returnVal, out errorMessage);
		}

		/// <summary>
		/// Executes the specified stored procedure and returns a scalar value from it.
		/// </summary>
		/// <param name="storedProc">Stored procedure to execute.</param>
		/// <param name="aParams">Array of parameters for stored procedure.  Passed by reference so 
		/// output parameters can be included.</param>
		/// <param name="leaveConnOpen">Determines whether connection should be left open when the method ends.</param>
		/// <param name="timeout">Command timeout, in seconds.</param>
		/// <param name="maxNumAttempts">Maximum number of attempts to execute command (used when command times out).</param>
		/// <param name="returnVal">Stored procedure return value (output parameter).</param>
		/// <param name="errorMessage">Error message resulting from executing stored procedure 
		/// (output parameter).</param>
		/// <returns>Scalar value.</returns>
		/// <remarks>Scalar value returned is the value in the first column of the first row of the stored procedure 
		/// result set.  Any further columns or rows will be ignored.</remarks>
		public object GetScalar(string storedProc, ref System.Data.SqlClient.SqlParameter[] aParams,
			bool leaveConnOpen, int timeout, int maxNumAttempts, out int returnVal, out string errorMessage)
		{
			object objReturned = null;
			returnVal = (int)DatabaseManagerReturnValues.GeneralError;
			string oErrorMessage = string.Empty;

			lock (mutexLock)  // Multi-threaded protection
			{
				if (_conn != null)
				{
					SqlCommand cmd = null;
					try
					{
						cmd = new SqlCommand(storedProc, _conn);
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.CommandTimeout = timeout;

						SqlParameter prmReturnVal = new SqlParameter("@RETURN_VALUE", SqlDbType.Int, 4);
						prmReturnVal.Direction = ParameterDirection.ReturnValue;
						cmd.Parameters.Add(prmReturnVal);

						if (aParams != null)
						{
							foreach (SqlParameter prm in aParams)
							{
								cmd.Parameters.Add(prm);
							}
						}

						int tryCount = 1;
						bool hasTimedOut = false;

						// Only open the connection if it is not already open.
						if (_conn.State != ConnectionState.Open)
						{
							_conn.Open();
						}

						while (tryCount == 1 || (hasTimedOut && tryCount <= maxNumAttempts))
						{
							hasTimedOut = false;
							try
							{
								objReturned = cmd.ExecuteScalar();
								returnVal = (int)prmReturnVal.Value;
							}
							catch (SqlException xcp)
							{
								// Note the "<" whereas it is "<=" in the enclosing loop (ie If fails the final 
								//	time through the loop do not attempt to loop again.  Instead re-throw the 
								//	exception for the outer catch block to deal with).
								if (tryCount < maxNumAttempts
								   && (xcp.Number == (int)SqlExceptionNumbers.TimeoutExpired
									  || xcp.Number == (int)SqlExceptionNumbers.UnableToConnect))
								{
									hasTimedOut = true;
								}
								else
								{
									throw xcp;
								}
							}
							tryCount++;
						}
					}
					catch (SqlException xcp)
					{
						oErrorMessage = xcp.Message;
						if (xcp.Number == (int)SqlExceptionNumbers.TimeoutExpired
						   || xcp.Number == (int)SqlExceptionNumbers.UnableToConnect)
						{
							returnVal = (int)DatabaseManagerReturnValues.TimeoutExpired;
						}
						else
						{
							returnVal = (int)DatabaseManagerReturnValues.GeneralError;
						}
						objReturned = null;
					}
					catch (Exception xcp)
					{
						oErrorMessage = xcp.Message;
						returnVal = (int)DatabaseManagerReturnValues.GeneralError;
						objReturned = null;
					}
					finally
					{
						// Must remove parameters from the command's Parameters collection in case 
						//	the parameters are going to reused in another command (otherwise get 
						//	"The SqlParameter is already contained by another SqlParameterCollection." 
						//	error).
						cmd.Parameters.Clear();

						// Close connection if leaveConnOpen is false.
						if (!leaveConnOpen
						   && _conn.State != ConnectionState.Closed
						   )
						{
							_conn.Close();
						}
					}
				}
			}

			errorMessage = oErrorMessage;
			return objReturned;
		}

		/// <summary>
		/// Close the connection if it is open.
		/// </summary>
		public void CloseConnection()
		{
			if (_conn.State != ConnectionState.Closed)
			{
				_conn.Close();
			}
		}

		#endregion

		#region Private and Protected Methods *****************************************************

		/// <summary>
		/// Build connection string from parameters.
		/// </summary>
		/// <returns>Connection string if successful, "ERR" if not.</returns>
		/// <remarks>If login is null or blank use integrated security.  If server or database is null or blank cannot construct a valid connection string so return an error.</remarks>
		private string BuildConnString(string server, string database, string login, string pwd)
		{
			string connString = "";
			if (server != null && database != null && server.Length > 0 && database.Length > 0)
			{
				// Enclose all values in quotes to prevent injection attacks.
				connString = "data source=\"" + server + "\";initial catalog=\"" + database + "\";"
					+ "persist security info=False;";

				// IF LOGIN IS NULL OR BLANK USE INTEGRATED SECURITY.
				if (login == null || login.Length == 0)
				{
					connString += "Integrated Security=SSPI;";
				}
				else
				{
					if (pwd == null)
					{
						pwd = "";
					}
					connString += "user id=\"" + login + "\";password=\"" + pwd + "\";";
				}
			}
			else connString = "ERR";
			return connString;
		}

		/// <summary>
		/// Reads the login from the connection string.
		/// </summary>
		/// <param name="connectionString">SqlConnection connection string to read.</param>
		/// <returns>The value of the "user id" key in a SqlConnection connection string.  If 
		/// there is no user id in the connection string, or if the connection string is null, 
		/// then null is returned.</returns>
		private string GetLoginFromConnString(string connectionString)
		{
			if (string.IsNullOrEmpty(connectionString))
			{
				return null;
			}

			string[] connElements = connectionString.Split(new char[] {';'});
			foreach (string element in connElements)
			{
				if (element.Trim().StartsWith("user id", StringComparison.CurrentCultureIgnoreCase))
				{
					string[] subElements = element.Split(new char[] {'='});
					if (subElements.Length > 1)
					{
						// Values in a connection string may be enclosed in single or double 
						//	quotes.  Remove them.
						return subElements[1].Replace("\"", string.Empty)
											.Replace("'", string.Empty)
											.Trim();
					}
					// If there is no "=" sign something is wrong with the connection string so 
					//	abort.
					return null;
				}
			}
			return null;
		}

		/// <summary>
		/// Sets up the identity column in a data table to auto-generate values when new rows are 
		/// added.
		/// </summary>
		/// <param name="dataTable">DataTable to set up.</param>
		/// <returns>DataTable with the identity column set to auto-generate negative numbers.
		/// </returns>
		/// <remarks>When updating a data table, each row requires a unique identifier.  
		/// Unique row identifiers may be generated automatically by adding an identity column 
		/// to a table.  If a row is added to a local data table and the data source is 
		/// updated, the remote SQL Server will auto-generate an identity column's values.  
		/// The row in the local data table will not have that auto-generated value, as it 
		/// was generated on the remote server.  However, the local data table still needs a 
		/// unique row identifier.  The column in the local data table can be set to 
		/// auto-generate its own values.  The column in the local table should be set to 
		/// generate negative numbers so that there is no chance of a collision with the 
		/// identity values generated on the SQL Server when the SQL Server values are merged 
		/// back into the local data table.</remarks>
		private void SetupDataTableIdentityColumn(DataTable dataTable)
		{
			if (dataTable == null)
			{
				return;
			}

			foreach (DataColumn column in dataTable.Columns)
			{
				if (column.AutoIncrement)
				{
					column.AutoIncrementSeed = -1;
					column.AutoIncrementStep = -1;
					column.ReadOnly = true;

					// Each table can have only one identity column so abort loop once it's found.
					break;
				}
			}
		}

		#endregion
	}
}
