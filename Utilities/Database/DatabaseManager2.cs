///////////////////////////////////////////////////////////////////////////////////////////////////
// Project      -   Utilities.Database
// General      -   Set of generic data access classes that may be useful in any project.
//
// File Name    -   DatabaseManager2.cs
// Description  -   For simplifying database operations (eg running SQL code, running stored 
//                  procedures).
//
// Notes        -   Replaces DatabaseManager.  Does not add any new functionality but code is 
//					cleaner and easier to maintain.  Less confusing overloads for each method.  
//					This is not a drop-in replacement for DatabaseManager as the method signatures 
//					have changed.   
//
// $History: DatabaseManager2.cs $
// 
// *****************  Version 9  *****************
// User: Simone       Date: 20/08/09   Time: 12:52p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// Bug Fixes by Derek Hu: PerformAction: Now only attempts to read value
// of prmReturnValue for CommandType.StoredProcedure.  ConnectionString
// property: Was returning _connection.ConnectionString but that did not
// include the password.  So now store the connection string in a
// _connectionstring data member which includes the password.
// 
// *****************  Version 8  *****************
// User: Simone       Date: 9/07/09    Time: 3:33p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// Previous change caused runtime exception: DataTable.DataSet property
// not set automatically when data table created.  Need to add data table
// to data set, set EnforceConstraints off, then remove from data set.
// Must remove from data set as some client code adds data table to
// another data set, and data table cannot be member of two data sets
// simultaneously.
// 
// *****************  Version 7  *****************
// User: Simone       Date: 8/07/09    Time: 3:54p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// PerformAction: Turn off enforsement of constraints on data table as
// seems to be buggy.  Google 'ADO.NET "Failed to enable constraints"' to
// see all the problems people have.
// 
// *****************  Version 6  *****************
// User: Simone       Date: 28/04/09   Time: 12:58p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// PerformAction: Calls new GetDataTableName to set the name of the
// DataTable (if ADO.NET creates name automatically it can get it wrong,
// eg if querying a view it can set the DataTable name to the name of the
// table underlying the view.  This can be a problem when updating the
// database with changes to the DataTable).
// 
// *****************  Version 5  *****************
// User: Simone       Date: 6/04/09    Time: 10:17a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// MINOR: Change to comment.
// 
// *****************  Version 4  *****************
// User: Simone       Date: 5/04/09    Time: 10:31p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// 
// *****************  Version 3  *****************
// User: Simone       Date: 5/04/09    Time: 3:53p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// MINOR: DatabaseManager2.ExecCommand renamed ExecStoredProc to make its
// function clearer.
// 
// *****************  Version 2  *****************
// User: Simone       Date: 5/04/09    Time: 2:46p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// MINOR: Correct mistake in comment.
// 
// *****************  Version 1  *****************
// User: Simone       Date: 5/04/09    Time: 2:35p
// Created in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// For simplifying database operations (eg running SQL code, running
// stored procedures).
///////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Utilities.Database
{
	///<summary>
	/// For simplifying database operations (eg running SQL code, running stored procedures).
	///</summary>
	/// <remarks>To update a database based on changes to a DataTable, use the DataUpdater 
	/// class.</remarks>
	public class DatabaseManager2 : IDisposable
	{
		/// <summary>
		/// Actions that the database manager can perform on the database.
		/// </summary>
		private enum ActionToPerform
		{
			Invalid = 0,
			GetDataTable,
			GetScalar,
			ExecCommand
		}

		#region Data Members **********************************************************************

		private System.Data.SqlClient.SqlConnection _connection;
		private string _server;
		private string _database;
		private string _login;
		private int _timeout;
		private int _maxNumberOfAttempts;
		private string _connectionstring;

		// Use the lock object for protecting against concurrency issues.
		private object _lockObject = new object();

		#endregion

		#region Constructors, Destructors / Finalizers and Dispose Methods ************************

		/// <summary>
		/// Initializes a new instance of the DatabaseManager2 class.  Creates a connection 
		/// to the database using integrated security.
		/// </summary>
		/// <param name="server">Server to connect to.</param>
		/// <param name="database">Database to connect to.</param>
		/// <remarks>Exceptions will bubble up to calling procedure.</remarks>
		public DatabaseManager2(string server, string database)
			: this(server, database, null, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the DatabaseManager2 class.  Creates a connection 
		/// to the database using SQL Server authentication.
		/// </summary>
		/// <param name="server">Server to connect to.</param>
		/// <param name="database">Database to connect to.</param>
		/// <param name="login">SQL Server login.</param>
		/// <param name="pwd">SQL Server password.</param>
		/// <remarks>Exceptions will bubble up to calling procedure.</remarks>
		public DatabaseManager2(string server, string database, string login, string pwd)
		{
			_server = server;
			_database = database;
			_login = login;
			_connectionstring = BuildConnectionString(server, database, login, pwd);
			_connection = new SqlConnection(_connectionstring);
			ResetTimeoutToDefault();
			_maxNumberOfAttempts = 1;
		}

		/// <summary>
		/// Initializes a new instance of the DatabaseManager2 class.  Creates a connection 
		/// to the database using a connection string.
		/// </summary>
		/// <param name="connectionString">SqlConnection connection string that contains 
		/// details of the server and database to connect to, and the login details needed to 
		/// connect to it.</param>
		/// <remarks>Exceptions will bubble up to calling procedure.</remarks>
		public DatabaseManager2(string connectionString)
		{
			_connectionstring = connectionString;
			_connection = new SqlConnection(connectionString);
			_server = _connection.DataSource;
			_database = _connection.Database;
			_login = this.GetLoginFromConnectionString(connectionString);
			ResetTimeoutToDefault();
			_maxNumberOfAttempts = 1;
		}

		/// <summary>
		/// Releases the resources used by the DatabaseManager2 instance.
		/// </summary>
		public void Dispose()
		{
			lock (_lockObject)  // Multi-threaded protection
			{
				if (_connection != null)
					_connection.Dispose();
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
			get { return _maxNumberOfAttempts; }
			set { _maxNumberOfAttempts = value; }
		}

		/// <summary>
		/// Connection string used to connect to database.
		/// </summary>
		public string ConnectionString
		{
			get { return _connectionstring; }
			// set {}            
		}

		/// <summary>
		/// Connection used to connect to database.
		/// </summary>
		public SqlConnection Connection
		{
			get { return _connection; }
			// set {}  
		}

		#endregion

		#region Public Methods ********************************************************************

		/// <summary>
		/// Reset timeout to default value.
		/// </summary>
		public void ResetTimeoutToDefault()
		{
			lock (_lockObject)  // Multi-threaded protection
			{
				using (SqlCommand command = new SqlCommand())
				{
					_timeout = command.CommandTimeout;
				}
			}
		}

		/// <summary>
		/// Close the connection if it is open.
		/// </summary>
		public void CloseConnection()
		{
			if (_connection.State != ConnectionState.Closed)
			{
				_connection.Close();
			}
		}

		#region GetDataTable **********************************************************************

		/// <summary>
		/// Gets a DataTable based on a SQL select query which does not contain parameters.
		/// </summary>
		/// <param name="sqlQuery">SQL select query that returns a record set.</param>
		/// <param name="errorMessage">Output parameter that returns any error message.</param>
		/// <returns>A DataTable.  If there is an error then returns null.</returns>
		public DataTable GetDataTable(string sqlQuery, out string errorMessage)
		{
			int dummyReturnValue;
			return this.GetDataTable(sqlQuery, CommandType.Text, null, _timeout,
				_maxNumberOfAttempts, true, out dummyReturnValue, out errorMessage);
		}

		/// <summary>
		/// Gets a DataTable based on a parameterized SQL select query.
		/// </summary>
		/// <param name="sqlQuery">Parameterized SQL select query that returns a record set.</param>
		/// <param name="aParams">Array of parameters for the select query.  May be input, output 
		/// or input-output parameters.</param>
		/// <param name="errorMessage">Output parameter that returns any error message.</param>
		/// <returns>A DataTable.  If there is an error then returns null.</returns>
		public DataTable GetDataTable(string sqlQuery, SqlParameter[] parameters,
			out string errorMessage)
		{
			int dummyReturnValue;
			return this.GetDataTable(sqlQuery, CommandType.Text, parameters, _timeout,
				_maxNumberOfAttempts, true, out dummyReturnValue, out errorMessage);
		}

		/// <summary>
		/// Gets a DataTable based on a stored procedure which does not have any parameters.
		/// </summary>
		/// <param name="storedProcName">Stored procedure that returns a recordset.</param>
		/// <param name="returnValue">Output Parameter: Stored procedure return value.</param>
		/// <param name="errorMessage">Output Parameter: Error message resulting from executing 
		/// stored procedure.</param>
		/// <returns>A DataTable.  If there is an error then returns null.</returns>
		public DataTable GetDataTable(string storedProcName,
			out int returnValue, out string errorMessage)
		{
			return this.GetDataTable(storedProcName, CommandType.StoredProcedure, null,
				_timeout, _maxNumberOfAttempts, true, out returnValue, out errorMessage);
		}

		/// <summary>
		/// Gets a DataTable based on a stored procedure which has parameters.
		/// </summary>
		/// <param name="storedProcName">Stored procedure that returns recordset.</param>
		/// <param name="parameters">Array of parameters for the stored procedure.</param>
		/// <param name="returnValue">Output Parameter: Stored procedure return value.</param>
		/// <param name="errorMessage">Output Parameter: Error message resulting from executing 
		/// stored procedure.</param>
		/// <returns>A DataTable.  If there is an error then returns null.</returns>
		public DataTable GetDataTable(string storedProcName, SqlParameter[] parameters,
			out int returnValue, out string errorMessage)
		{
			return this.GetDataTable(storedProcName, CommandType.StoredProcedure, parameters,
				_timeout, _maxNumberOfAttempts, true, out returnValue, out errorMessage);
		}

		/// <summary>
		/// Gets a DataTable based on either a SQL select query or on a stored procedure.
		/// </summary>
		/// <param name="commandText">SQL select query or name of a stored procedure, as appropriate.
		/// </param>
		/// <param name="commandType">Command type: Text for a SQL select query or StoredProcedure 
		/// for a stored procedure.</param>
		/// <param name="parameters">Array of parameters to be passed to the select query or the 
		/// stored procedure.</param>
		/// <param name="timeout">Command timeout, in seconds.</param>
		/// <param name="maxNumberOfAttempts">Maximum number of attempts to execute command 
		/// (used when command times out).</param>
		/// <param name="setupIdentityColumn">If set, sets up any identity column in the data 
		/// table to auto-generate new values if a row is added.  The auto-generated values must 
		/// be negative numbers, to avoid collisions with the identity values created in SQL 
		/// Server.</param>
		/// <param name="returnValue">Output Parameter: Stored procedure return value.  Special 
		/// values returned if an exception occurs, for both a stored procedure and a SQL select 
		/// query. If no exception occurs returns 0 for SQL select query.</param>
		/// <param name="errorMessage">Output Parameter: Error message resulting from executing 
		/// the select query or stored procedure.</param>		
		/// <returns>A DataTable.</returns>
		/// <remarks>Must set up the auto-increment seed before populating the data table, 
		/// otherwise the previous maximum (positive) value in the column will be used as the 
		/// seed (regardless of the seed value that is explicitly set).  This will lead to 
		/// collisions with the values created in SQL Server.</remarks>
		public DataTable GetDataTable(string commandText, CommandType commandType,
			SqlParameter[] parameters, int timeout, int maxNumberOfAttempts,
			bool setupIdentityColumn,
			out int returnValue, out string errorMessage)
		{
			DataTable outputDataTable;
			int dummyScalar;
			this.PerformAction<int>(commandText, commandType, ActionToPerform.GetDataTable,
				parameters, timeout, maxNumberOfAttempts, setupIdentityColumn,
				out returnValue, out errorMessage, out outputDataTable, out dummyScalar);
			return outputDataTable;
		}

		#endregion

		#region GetScalar *************************************************************************

		/// <summary>
		/// Executes a SQL query that returns a scalar value.
		/// </summary>
		/// <param name="sqlQuery">The SQL query to execute.
		/// </param>
		/// <param name="errorMessage">Output Parameter: Error message resulting from executing 
		/// the SQL query.</param>		
		/// <typeparam name="T">Data type of scalar value that will be returned.</typeparam>
		/// <returns>Scalar value of type T, or the default value for type T if there is an error.
		/// </returns>
		/// <remarks>The scalar value returned is the value in the first column of the first row 
		/// of the result set of the SQL query.  Any further columns or rows in the 
		/// result set will be ignored. 
		/// If the scalar value is to be returned from the SQL query via an output parameter, 
		/// rather than in the result set, use an overload of the ExecSql 
		/// method which takes a parameter array as an argument.</remarks>
		public T GetScalar<T>(string sqlQuery, out string errorMessage)
		{
			int dummyReturnValue;
			return this.GetScalar<T>(sqlQuery, CommandType.Text, null,
				_timeout, _maxNumberOfAttempts, out dummyReturnValue, out errorMessage);
		}

		/// <summary>
		/// Executes a parameterized SQL query that returns a scalar value.
		/// </summary>
		/// <param name="sqlQuery">The SQL query to execute.
		/// </param>
		/// <param name="parameters">Array of parameters to be passed to the SQL query.</param>
		/// <param name="errorMessage">Output Parameter: Error message resulting from executing 
		/// the SQL query.</param>		
		/// <typeparam name="T">Data type of scalar value that will be returned.</typeparam>
		/// <returns>Scalar value of type T, or the default value for type T if there is an error.
		/// </returns>
		/// <remarks>The scalar value returned is the value in the first column of the first row 
		/// of the result set of the SQL query.  Any further columns or rows in the 
		/// result set will be ignored. 
		/// If the scalar value is to be returned from the SQL query via an output parameter, 
		/// rather than in the result set, use an overload of the ExecSql 
		/// method which takes a parameter array as an argument.</remarks>
		public T GetScalar<T>(string sqlQuery, SqlParameter[] parameters,
			out string errorMessage)
		{
			int dummyReturnValue;
			return this.GetScalar<T>(sqlQuery, CommandType.Text, parameters,
				_timeout, _maxNumberOfAttempts, out dummyReturnValue, out errorMessage);
		}

		/// <summary>
		/// Executes a stored procedure that returns a scalar value.
		/// </summary>
		/// <param name="storedProcName">Name of the stored procedure to execute.
		/// </param>
		/// <param name="returnValue">Output Parameter: Stored procedure return value.  Special 
		/// values returned if an exception occurs.</param>
		/// <param name="errorMessage">Output Parameter: Error message resulting from executing 
		/// the stored procedure.</param>		
		/// <typeparam name="T">Data type of scalar value that will be returned.</typeparam>
		/// <returns>Scalar value of type T, or the default value for type T if there is an error.
		/// </returns>
		/// <remarks>The scalar value returned is the value in the first column of the first row 
		/// of the result set of the stored procedure.  Any further columns or rows in the 
		/// result set will be ignored. 
		/// If the scalar value is to be returned from the stored procedure via an output 
		/// parameter, rather than in the result set, use an overload of the ExecStoredProc 
		/// method which takes a parameter array as an argument.</remarks>
		public T GetScalar<T>(string storedProcName,
			out int returnValue, out string errorMessage)
		{
			return this.GetScalar<T>(storedProcName, CommandType.StoredProcedure, null,
				_timeout, _maxNumberOfAttempts, out returnValue, out errorMessage);
		}

		/// <summary>
		/// Executes a parameterized stored procedure that returns a scalar value.
		/// </summary>
		/// <param name="storedProcName">Name of the stored procedure to execute.
		/// </param>
		/// <param name="parameters">Array of parameters to be passed to the stored procedure.</param>
		/// <param name="returnValue">Output Parameter: Stored procedure return value.  Special 
		/// values returned if an exception occurs.</param>
		/// <param name="errorMessage">Output Parameter: Error message resulting from executing 
		/// the stored procedure.</param>		
		/// <typeparam name="T">Data type of scalar value that will be returned.</typeparam>
		/// <returns>Scalar value of type T, or the default value for type T if there is an error.
		/// </returns>
		/// <remarks>The scalar value returned is the value in the first column of the first row 
		/// of the result set of the stored procedure.  Any further columns or rows in the 
		/// result set will be ignored. 
		/// If the scalar value is to be returned from the stored procedure via an output 
		/// parameter, rather than in the result set, use an overload of the ExecStoredProc 
		/// method which takes a parameter array as an argument.</remarks>
		public T GetScalar<T>(string storedProcName, SqlParameter[] parameters,
			out int returnValue, out string errorMessage)
		{
			return this.GetScalar<T>(storedProcName, CommandType.StoredProcedure, parameters,
				_timeout, _maxNumberOfAttempts, out returnValue, out errorMessage);
		}

		/// <summary>
		/// Executes either a stored procedure or a SQL query that returns a scalar value.
		/// </summary>
		/// <param name="commandText">SQL query or name of a stored procedure, as appropriate.
		/// </param>
		/// <param name="commandType">Command type: Text for a SQL query or StoredProcedure 
		/// for a stored procedure.</param>
		/// <param name="parameters">Array of parameters to be passed to the SQL query or the 
		/// stored procedure.</param>
		/// <param name="timeout">Command timeout, in seconds.</param>
		/// <param name="maxNumberOfAttempts">Maximum number of attempts to execute command 
		/// (used when command times out).</param>
		/// <param name="returnValue">Output Parameter: Stored procedure return value.  Special 
		/// values returned if an exception occurs, for both a stored procedure and a SQL  
		/// query. If no exception occurs returns 0 for SQL query.</param>
		/// <param name="errorMessage">Output Parameter: Error message resulting from executing 
		/// the SQL query or stored procedure.</param>		
		/// <typeparam name="T">Data type of scalar value that will be returned.</typeparam>
		/// <returns>Scalar value of type T, or the default value for type T if there is an error.
		/// </returns>
		/// <remarks>The scalar value returned is the value in the first column of the first row 
		/// of the result set of the SQL query or stored procedure.  Any further columns or rows 
		/// in the result set will be ignored. 
		/// If the scalar value is to be returned from the SQL query or stored procedure via an 
		/// output parameter, rather than in the result set, use an overload of the ExecCommand, 
		/// ExecStoredProc or ExecSql methods which take a parameter array as an argument.</remarks>
		public T GetScalar<T>(string commandText, CommandType commandType,
			SqlParameter[] parameters, int timeout, int maxNumberOfAttempts,
			out int returnValue, out string errorMessage)
		{
			DataTable dummyDataTable;
			T outputScalar;
			this.PerformAction<T>(commandText, commandType, ActionToPerform.GetScalar,
				parameters, timeout, maxNumberOfAttempts, false,
				out returnValue, out errorMessage, out dummyDataTable, out outputScalar);
			return outputScalar;
		}

		#endregion

		#region ExecCommand ***********************************************************************

		/// <summary>
		/// Executes a SQL command.  If the SQL command returns a recordset the recordset is 
		/// ignored.
		/// </summary>
		/// <param name="sqlCommand">SQL command to execute.
		/// </param>
		/// <param name="errorMessage">Output Parameter: Error message resulting from executing 
		/// the SQL command.</param>		
		/// <returns>true if the SQL command executed successfully, otherwise false.</returns>
		/// <remarks>Intended for SQL commands that perform an action rather than returning 
		/// a recordset.</remarks>
		public bool ExecSql(string sqlCommand,
			out string errorMessage)
		{
			int dummyReturnValue;
			return this.ExecCommand(sqlCommand, CommandType.Text, null,
				_timeout, _maxNumberOfAttempts, out dummyReturnValue, out errorMessage);
		}

		/// <summary>
		/// Executes a parameterized SQL command.  If the SQL command returns a 
		/// recordset the recordset is ignored.
		/// </summary>
		/// <param name="sqlCommand">SQL command to execute.
		/// </param>
		/// <param name="parameters">Array of parameters to be passed to the SQL command.</param>
		/// <param name="errorMessage">Output Parameter: Error message resulting from executing 
		/// the SQL command.</param>		
		/// <returns>true if the SQL command executed successfully, otherwise false.</returns>
		/// <remarks>Intended for SQL commands that perform an action rather than returning 
		/// a recordset.  May also be used to get a scalar value, where the scalar value is 
		/// returned via an output parameter rather than in the resultant record set.  In that 
		/// case the output parameter should be included in the parameters array.</remarks>
		public bool ExecSql(string sqlCommand, SqlParameter[] parameters,
			out string errorMessage)
		{
			int dummyReturnValue;
			return this.ExecCommand(sqlCommand, CommandType.Text, parameters,
				_timeout, _maxNumberOfAttempts, out dummyReturnValue, out errorMessage);
		}

		/// <summary>
		/// Executes a stored procedure.  If the stored procedure returns a recordset the 
		/// recordset is ignored.
		/// </summary>
		/// <param name="storedProcName">Name of the stored procedure to execute.
		/// </param>
		/// <param name="returnValue">Output Parameter: Stored procedure return value.  Special 
		/// values returned if an exception occurs.</param>
		/// <param name="errorMessage">Output Parameter: Error message resulting from executing 
		/// the stored procedure.</param>		
		/// <returns>true if the stored procedure executed successfully, otherwise false.</returns>
		/// <remarks>Intended for stored procedures that perform an action rather than returning 
		/// a recordset.</remarks>
		public bool ExecStoredProc(string storedProcName,
			out int returnValue, out string errorMessage)
		{
			return this.ExecCommand(storedProcName, CommandType.StoredProcedure, null,
				_timeout, _maxNumberOfAttempts, out returnValue, out errorMessage);
		}

		/// <summary>
		/// Executes a parameterized stored procedure.  If the stored procedure returns a 
		/// recordset the recordset is ignored.
		/// </summary>
		/// <param name="storedProcName">Name of the stored procedure to execute.
		/// </param>
		/// <param name="parameters">Array of parameters to be passed to the stored procedure.</param>
		/// <param name="returnValue">Output Parameter: Stored procedure return value.  Special 
		/// values returned if an exception occurs.</param>
		/// <param name="errorMessage">Output Parameter: Error message resulting from executing 
		/// the stored procedure.</param>		
		/// <returns>true if the stored procedure executed successfully, otherwise false.</returns>
		/// <remarks>Intended for stored procedures that perform an action rather than returning 
		/// a recordset.  May also be used to get a scalar value, where the scalar value is 
		/// returned via an output parameter rather than in the resultant record set.  In that 
		/// case the output parameter should be included in the parameters array.</remarks>
		public bool ExecStoredProc(string storedProcName, SqlParameter[] parameters,
			out int returnValue, out string errorMessage)
		{
			return this.ExecCommand(storedProcName, CommandType.StoredProcedure, parameters,
				_timeout, _maxNumberOfAttempts, out returnValue, out errorMessage);
		}

		/// <summary>
		/// Executes a SQL command or stored procedure.  If the command or stored procedure 
		/// returns a recordset the recordset is ignored.
		/// </summary>
		/// <param name="commandText">SQL query or name of a stored procedure, as appropriate.
		/// </param>
		/// <param name="commandType">Command type: Text for a SQL command or StoredProcedure 
		/// for a stored procedure.</param>
		/// <param name="parameters">Array of parameters to be passed to the SQL command or the 
		/// stored procedure.</param>
		/// <param name="timeout">Command timeout, in seconds.</param>
		/// <param name="maxNumberOfAttempts">Maximum number of attempts to execute command 
		/// (used when command times out).</param>
		/// <param name="returnValue">Output Parameter: Stored procedure return value.  Special 
		/// values returned if an exception occurs, for both a stored procedure and a SQL  
		/// command. If no exception occurs returns 0 for SQL command.</param>
		/// <param name="errorMessage">Output Parameter: Error message resulting from executing 
		/// the SQL command or stored procedure.</param>		
		/// <returns>true if the command executed successfully, otherwise false.</returns>
		/// <remarks>Intended for SQL command and stored procedures that perform an action 
		/// rather than returning a recordset.  May also be used to get a scalar value, where the 
		/// scalar value is returned via an output parameter rather than in the resultant record 
		/// set.  In that case the output parameter should be included in the parameters array.</remarks>
		public bool ExecCommand(string commandText, CommandType commandType,
			SqlParameter[] parameters, int timeout, int maxNumberOfAttempts,
			out int returnValue, out string errorMessage)
		{
			DataTable dummyDataTable;
			int dummyScalar;
			return this.PerformAction<int>(commandText, commandType, ActionToPerform.ExecCommand,
				parameters, timeout, maxNumberOfAttempts, false,
				out returnValue, out errorMessage, out dummyDataTable, out dummyScalar);
		}

		#endregion

		#endregion

		#region Event Handlers ********************************************************************


		#endregion

		#region Private and Protected Methods *****************************************************

		/// <summary>
		/// Builds connection string from parameters.
		/// </summary>
		/// <param name="server">SQL Server that the DatabaseManager will connect to.</param>
		/// <param name="database">Database that the DatabaseManager will connect to.</param>
		/// <param name="login">User name for logging into the SQL Server.</param>
		/// <param name="password">Password to use when logging into SQL Server.</param>
		/// <returns>Connection string if successful, "ERR" if not.</returns>
		/// <remarks>If login is null or blank will use integrated security.  If server or database 
		/// is null or blank cannot construct a valid connection string so returns an error.</remarks>
		private string BuildConnectionString(string server, string database, string login,
			string password)
		{
			string connectionString = "";
			if (server != null && database != null && server.Length > 0 && database.Length > 0)
			{
				// Enclose all values in quotes to prevent injection attacks.
				connectionString = "data source=\"" + server
					+ "\";initial catalog=\"" + database + "\";"
					+ "persist security info=False;";

				// If login is null or blank use integrated security.
				if (login == null || login.Length == 0)
				{
					connectionString += "Integrated Security=SSPI;";
				}
				else
				{
					if (password == null)
					{
						password = "";
					}
					connectionString += "user id=\"" + login + "\";password=\"" + password + "\";";
				}
			}
			else connectionString = "ERR";
			return connectionString;
		}

		/// <summary>
		/// Reads the login from the connection string.
		/// </summary>
		/// <param name="connectionString">SqlConnection connection string to read.</param>
		/// <returns>The value of the "user id" key in a SqlConnection connection string.  If 
		/// there is no user id in the connection string, or if the connection string is null, 
		/// then null is returned.</returns>
		private string GetLoginFromConnectionString(string connectionString)
		{
			if (string.IsNullOrEmpty(connectionString))
			{
				return null;
			}

			string[] connectionElements = connectionString.Split(new char[] { ';' });
			foreach (string element in connectionElements)
			{
				if (element.Trim().StartsWith("user id", StringComparison.CurrentCultureIgnoreCase))
				{
					string[] subElements = element.Split(new char[] { '=' });
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
		/// Performs an action on the database.
		/// </summary>
		/// <param name="commandText">SQL query or name of a stored procedure to execute, as 
		/// appropriate.
		/// </param>
		/// <param name="commandType">Command type: Text for a SQL command or StoredProcedure 
		/// for a stored procedure.</param>
		/// <param name="actionToPerform">The type of action to perform on the database: Either 
		/// GetDataTable, GetScalar, or ExecCommand.</param>
		/// <param name="parameters">Array of parameters to be passed to the SQL command or the 
		/// stored procedure.</param>
		/// <param name="timeout">Command timeout, in seconds.</param>
		/// <param name="maxNumberOfAttempts">Maximum number of attempts to execute command 
		/// (used when command times out).</param>		
		/// <param name="setupDataTableIdentityColumn">Used only if the action is GetDataTable.  
		/// If set, sets up any identity column in the data table to auto-generate new values 
		/// if a row is added.  The auto-generated values must be negative numbers, to avoid 
		/// collisions with the identity values created in SQL Server.</param>
		/// <param name="returnValue">Output Parameter: Stored procedure return value.  Special 
		/// values returned if an exception occurs, for both a stored procedure and a SQL  
		/// command. If no exception occurs returns 0 for SQL command.</param>
		/// <param name="errorMessage">Output Parameter: Error message resulting from executing 
		/// the SQL command or stored procedure.</param>
		/// <param name="outputDataTable">The data table returned if the action is GetDataTable.  
		/// For other actions or if there is an error, null is returned.</param>
		/// <param name="outputScalar">The scalar value, of type T, returned if the action is 
		/// GetScalar.  For other actions, or if there is an error, the default value for type T 
		/// is returned.</param>
		/// <typeparam name="T">Data type of scalar value that will be returned.  If this method 
		/// is used to get a data table or execute a command then any data type can be used.</typeparam>		
		/// <returns>true if the action was performed successfully, otherwise false.</returns>
		/// <remarks>This method can be used to return a data table of records or a scalar value 
		/// from the database.  It can also be used to execute a command against the database 
		/// which does not return anything.
		/// 
		/// For the GetScalar action, the scalar value returned is the value in the first column 
		/// of the first row of the result set of the SQL query or stored procedure.  Any further 
		/// columns or rows in the result set will be ignored. 
		/// If the scalar value is to be returned from the SQL query or stored procedure via an 
		/// output parameter, rather than in the result set, use the ExecCommand action that 
		/// includes an output parameter in the parameter array argument.</remarks>
		private bool PerformAction<T>(string commandText, CommandType commandType,
			ActionToPerform actionToPerform,
			SqlParameter[] parameters, int timeout, int maxNumberOfAttempts,
			bool setupDataTableIdentityColumn,
			out int returnValue, out string errorMessage,
			out DataTable outputDataTable, out T outputScalar)
		{
			bool wasExecutedOK = false;
			returnValue = (int)DatabaseManagerReturnValues.GeneralError;
			errorMessage = string.Empty;
			string dataTableName = string.Empty;
			if (actionToPerform == ActionToPerform.GetDataTable)
			{
				dataTableName = this.GetDataTableName(commandText, commandType);
			}
			outputDataTable = new DataTable(dataTableName);
			outputScalar = default(T);

			if (actionToPerform == ActionToPerform.Invalid)
			{
				errorMessage = "Invalid action.  Action aborted.";
				outputDataTable = null;
				return false;
			}

			lock (_lockObject)  // Multi-threaded protection
			{
				if (_connection != null)
				{
					SqlCommand command = null;
					try
					{
						using (command = new SqlCommand(commandText, _connection))
						{
							command.CommandType = commandType;
							command.CommandTimeout = timeout;

							// Only used for stored procs.
							SqlParameter prmReturnValue
									= new SqlParameter("@RETURN_VALUE", SqlDbType.Int, 4);

							if (commandType == CommandType.StoredProcedure
								|| (parameters != null && parameters.Length > 0))
							{
								if (commandType == CommandType.StoredProcedure)
								{
									prmReturnValue.Direction = ParameterDirection.ReturnValue;
									command.Parameters.Add(prmReturnValue);
								}
								if (parameters != null && parameters.Length > 0)
								{
									command.Parameters.AddRange(parameters);
								}
							}

							if (_connection.State != ConnectionState.Open)
							{
								_connection.Open();
							}

							// dataAdapter used to GetDataTable.
							SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
							DataSet tempDataSet = new DataSet();
							if (actionToPerform == ActionToPerform.GetDataTable)
							{
								dataAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
								dataAdapter.FillSchema(outputDataTable, SchemaType.Source);
								// Turn off enforsement of constraints on data table as seems to 
								//	be buggy.  Google 'ADO.NET "Failed to enable constraints"' to 
								//	see all the problems people have.
								tempDataSet.Tables.Add(outputDataTable);
								tempDataSet.EnforceConstraints = false;
								if (setupDataTableIdentityColumn)
								{
									SetupDataTableIdentityColumn(outputDataTable);
								}
							}

							int tryCount = 1;
							bool hasTimedOut = false;
							while (tryCount == 1 || (hasTimedOut && tryCount <= maxNumberOfAttempts))
							{
								hasTimedOut = false;
								try
								{
									switch (actionToPerform)
									{
										case ActionToPerform.GetDataTable:
											dataAdapter.Fill(outputDataTable);
											break;
										case ActionToPerform.ExecCommand:
											command.ExecuteNonQuery();
											if (commandType == CommandType.StoredProcedure)
											{
												returnValue = (int)prmReturnValue.Value;
											}
											break;
										case ActionToPerform.GetScalar:
											object objReturned = command.ExecuteScalar();
											outputScalar = ConvertObjectToType<T>(objReturned,
												ref errorMessage);
											break;
									}
								}
								catch (SqlException ex)
								{
									// Note the "<" whereas it is "<=" in the enclosing loop 
									//	(ie If fails the final time through the loop do not attempt 
									//	to loop again.  Instead re-throw the exception for the 
									//	outer catch block to deal with).
									if (tryCount < maxNumberOfAttempts
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

							if (actionToPerform == ActionToPerform.GetDataTable)
							{
								// Client code expects returned data table to not be part of a data 
								//	set (some of the client code adds the data table to a data set 
								//	and a data table cannot be a member of more than one data set).  
								//	So remove from data set now that the table has been loaded.
								tempDataSet.Tables.Remove(outputDataTable);
							}

							if (commandType == CommandType.StoredProcedure)
							{
								returnValue = (int)prmReturnValue.Value;
							}
							else
							{
								returnValue = (int)DatabaseManagerReturnValues.Success;
							}
						}

						wasExecutedOK = true;
					}
					catch (SqlException ex)
					{
						errorMessage = ex.Message;
						if (ex.Number == (int)SqlExceptionNumbers.TimeoutExpired
						   || ex.Number == (int)SqlExceptionNumbers.UnableToConnect)
						{
							returnValue = (int)DatabaseManagerReturnValues.TimeoutExpired;
						}
						else
						{
							returnValue = (int)DatabaseManagerReturnValues.GeneralError;
						}
						outputDataTable = null;
					}
					catch (Exception ex)
					{
						errorMessage = ex.Message;
						returnValue = (int)DatabaseManagerReturnValues.GeneralError;
						outputDataTable = null;
					}
					finally
					{
						// Must remove parameters from the command's Parameters collection in case 
						//	the parameters are going to reused in another command (otherwise get 
						//	"The SqlParameter is already contained by another SqlParameterCollection." 
						//	error).  Calling command Dispose method is not enough to avoid this 
						//	error.
						command.Parameters.Clear();

						if (_connection.State != ConnectionState.Closed)
						{
							_connection.Close();
						}
					}
				}

				return wasExecutedOK;
			}
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

		/// <summary>
		/// Converts an object to a specified data type.
		/// </summary>
		/// <typeparam name="T">Type to convert the object to.</typeparam>
		/// <param name="objectToConvert">Object to convert.</param>
		/// <param name="errorMessage">If the conversion succeeds the supplied error message will 
		/// be returned unchanged.  If the conversion fails a conversion error message will be 
		/// appended to the supplied error message.</param>
		/// <returns>A value of type T, converted from an object.  If the object to convert is 
		/// null, or the conversion fails, the default value for type T will be returned.</returns>
		private T ConvertObjectToType<T>(object objectToConvert, ref string errorMessage)
		{
			if (objectToConvert == null)
			{
				return default(T);
			}
			try
			{
				return (T)objectToConvert;
			}
			catch (Exception ex)
			{
				string conversionError = string.Format("{0}: {1}",
					ex.GetType().Name, ex.Message);
				if (errorMessage.Trim().Length == 0)
				{
					errorMessage = conversionError;
				}
				else
				{
					errorMessage += "  " + conversionError;
				}
				return default(T);
			}
		}

		/// <summary>
		/// Extracts the name for a DataTable from the command text that will be used to retrieve 
		/// the data.
		/// </summary>
		/// <param name="commandText">SQL query or name of a stored procedure to execute, as 
		/// appropriate.
		/// </param>
		/// <param name="commandType">Command type: Text for a SQL command or StoredProcedure 
		/// for a stored procedure.</param>
		/// <returns>The name of the view, function or table in the FROM clause of a SQL statement, 
		/// the name of a table for TableDirect commands, or an empty string for StoredProcedure 
		/// commands.</returns>
		/// <remarks>Deals with the problem of ADO.NET determining the name of the table, view, etc 
		/// incorrectly when it does it automatically.  This can happen when the select statement 
		/// queries a view.  ADO.NET may set the DataTable name to the name of the underlying 
		/// table, rather than the name of the view.  This can cause a problem when updating the 
		/// database with changes to the DataTable.  The update will be attempted on the underlying 
		/// table, not the view, which may be problematic if the view has instead-of triggers to 
		/// perform specific actions for insert, delete and update.</remarks>
		private string GetDataTableName(string commandText, CommandType commandType)
		{
			if (commandType == CommandType.TableDirect)
			{
				return commandText;
			}
			if (commandType == CommandType.StoredProcedure)
			{
				return string.Empty;
			}

			// commandType must be CommandType.Text so commandText should be a SQL select statement.
			string fromClauseMarker = "FROM";
			int textPosition = commandText.IndexOf(fromClauseMarker,
				StringComparison.CurrentCultureIgnoreCase);
			if (textPosition <= 0)
			{
				return string.Empty;
			}

			string fromClause
				= commandText.Substring(textPosition + fromClauseMarker.Length).Trim();

			if (fromClause.Length == 0)
			{
				return string.Empty;
			}

			// If from clause contains a join then cannot uniquely identify the table.
			if (fromClause.IndexOf(" JOIN ", StringComparison.CurrentCultureIgnoreCase) > 0)
			{
				return string.Empty;
			}

			char endCharacter;
			string tableName = string.Empty;

			// If the first character is either a '"' or a '[' then the table name may contain a 
			//	space.  In that case the table name is everything up to the closing '"' or ']'.
			if (fromClause.StartsWith("\"", StringComparison.CurrentCultureIgnoreCase)
				|| fromClause.StartsWith("[", StringComparison.CurrentCultureIgnoreCase))
			{
				endCharacter = '"';
				if (fromClause.StartsWith("[", StringComparison.CurrentCultureIgnoreCase))
				{
					endCharacter = ']';
				}
				textPosition = fromClause.IndexOf(endCharacter, 1);
				if (textPosition <= 0)
				{
					return fromClause.Substring(1);
				}
				tableName = fromClause.Substring(1, (textPosition - 1));
			}
			// "Normal" table name - cannot contain a space.
			else
			{
				endCharacter = ' ';
				textPosition = fromClause.IndexOf(endCharacter, 1);
				if (textPosition <= 0)
				{
					tableName = fromClause;
				}
				else
				{
					// NOTE: Different Substring arguments than for quoted table name, above.
					tableName = fromClause.Substring(0, textPosition);
				}
			}

			// Table name may contain a space but must otherwise follow the rules for SQL Server
			//	identifiers:
			//	Starts with either:
			//		- a letter, as defined by the Unicode Standard 3.2 (a-z, A-Z, plus letters 
			//				from other languages)
			//		- an underscore (_)
			//		- an "at" sign (@)
			//		- a number sign (#)
			//	Subsequent characters can be:
			//		- a letter, as defined by the Unicode Standard 3.2
			//		- a decimal number, from either Basic Latin or other languages
			//		- an underscore (_)
			//		- an "at" sign (@)
			//		- a number sign (#)
			//		- a dollar sign ($)			

			// If the commandText is invalid SQL this will be picked up elsewhere.  Therefore 
			//	only need to worry about valid SQL which may have invalid identifier characters 
			//	in the text extracted from the FROM clause.  These invalid characters are:
			//		- a decimal-point (.), used in multi-part identifiers to separate the parts of 
			//				the identifier
			//		- an opening parenthesis ( ( ), which may follow a function name.

			// Take only the last part of a multi-part identifier as the table name.
			string[] nameParts = tableName.Split(new char[] { '.' });
			tableName = nameParts[nameParts.Length - 1];

			// Take only the name up to the opening parenthesis.
			nameParts = tableName.Split(new char[] { '(' });
			tableName = nameParts[0].Trim(); ;

			return tableName;
		}

		#endregion
	}
}