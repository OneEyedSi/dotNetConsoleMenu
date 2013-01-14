///////////////////////////////////////////////////////////////////////////////////////////////////
// Project      -   Utilities.Database
// General      -   Set of generic data access classes that may be useful in any project.
//
// File Name    -   DatabaseUpdater.cs
// Description  -   Represents a writer that can update, delete or insert rows into a database 
//					based on changes to a DataTable.
//
// Notes        -   Thread safe.
//
// $History: DatabaseUpdater.cs $
// 
// *****************  Version 9  *****************
// User: Simone       Date: 29/04/09   Time: 6:09p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// SetDataAdapterCommands: Set the Transaction properties of the commands
// outside the if statements, so that all commands are enlisted in the
// transaction, regardless of whether they were supplied externally or
// created on the fly.
// 
// *****************  Version 8  *****************
// User: Simone       Date: 28/04/09   Time: 12:51p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// UpdateSource: In the finally block check that command exists before
// trying to reset CommandTimeout.  If there is an exception during method
// execution the commands may not be created.
// 
// *****************  Version 7  *****************
// User: Simone       Date: 6/04/09    Time: 10:50a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// Removed DatabaseManager as a data member and property.  Replaced it
// with Connection and ConnectionString properties.  Added new
// constructors to set the Connection property.  Added Timeout and
// DefaultTimeout properties, ResetTimeoutToDefault method.  Deleted
// GetDataTable methods which are just duplicates of the methods in the
// DatabaseManager2.  UpdateSource method: Added timeout parameter, set
// the timeouts for each DataAdapter command and reset them to their
// initial values when finished.  SetDataAdapterCommands method: Dispose
// of CommandBuilder and clone commands, rather than creating a new
// DataAdapter just for the CommandBuilder.
// 
// *****************  Version 6  *****************
// User: Simone       Date: 5/04/09    Time: 10:53p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// Bug FIx - GetInsertCommand:  Predicate method should have been
// IsIdentityColumn rather than IsNotIdentityColumn.
// 
// *****************  Version 5  *****************
// User: Simone       Date: 5/04/09    Time: 10:32p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// Replaced DatabaseManager with DatabaseManager2.
// 
// *****************  Version 4  *****************
// User: Simone       Date: 5/04/09    Time: 10:30p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// Remove all references to UpdaterCommands.  Do not save DataAdapter from
// when data tables are created - generate it from scratch in UpdateSource
// method.  UpdateSource method: DataAdapter.SelectCommand generated if it
// does not exist.  Call SetDataAdapterCommands to create DataAdapter
// Insert, Update and Delete commands if necessary.  Call GetUpdateResult
// to determine the result of the database update.  Call
// AcceptDataTableChanges to reset the RowState of rows successfully
// updated in database.  Deleted GetDataAdapterClone method - not needed.
// 
// *****************  Version 2  *****************
// User: Simone       Date: 23/03/09   Time: 2:10p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// DatabaseUpdateResult enum added.  UpdateCommand and UpdateCommands
// moved to separate file and renamed Updater... (to avoid confusion with
// the UpdateCommand property of a DataAdapter).  First draft of
// UpdateSource method completed, including overloads.
// 
// *****************  Version 1  *****************
// User: Simone       Date: 20/03/09   Time: 5:20p
// Created in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// Represents a writer that can update, delete or insert rows into a
// database based on changes to a DataTable.  NOT thread-safe.
///////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using Utilities.Miscellaneous;

namespace Utilities.Database
{
	#region Enums, Structures and Static Classes used with DatabaseUpdater ************************

	/// <summary>
	/// Indicates the result of a database update.
	/// </summary>
	public enum DatabaseUpdateResult
	{
		Invalid = 0,
		Success,
		Failed,
		PartialSuccess,		// Where some rows have been updated but others have failed.
		TimedOut
	}

	#endregion

	///<summary>
	/// Represents a writer that can update, delete or insert rows in a database based on 
	/// changes to a DataTable.  
	///</summary>
	public class DatabaseUpdater
	{
		#region Data Members **********************************************************************

		private SqlConnection _connection;
		private DatabaseManager2 _databaseManager;
		private int _timeout;
		private int _defaultTimeout = -1;

		// For locking block of code to make it thread-safe.
		private object _lockObject = new object();

		#endregion

		#region Constructors, Destructors / Finalizers and Dispose Methods ************************

		/// <summary>
		/// Initializes a new instance of the DatabaseUpdater class with a DatabaseManager. 
		/// </summary>
		/// <param name="databaseManager">DatabaseManager that handles the connection to the 
		/// database.</param>
		public DatabaseUpdater(DatabaseManager2 databaseManager)
			: this(databaseManager.Connection)
		{
		}

		/// <summary>
		/// Initializes a new instance of the DatabaseUpdater class with a connection string. 
		/// </summary>
		/// <param name="connectionString">SqlConnection connection string that contains 
		/// details of the server and database to connect to, and the login details needed to 
		/// connect to it.</param>
		public DatabaseUpdater(string connectionString) 
			: this (new SqlConnection(connectionString))
		{
		}

		/// <summary>
		/// Initializes a new instance of the DatabaseUpdater class with a Connection. 
		/// </summary>
		/// <param name="connection">Connection object for connecting to the database.</param>
		public DatabaseUpdater(SqlConnection connection)
		{
			_connection = connection;
			this.ResetTimeoutToDefault();
		}

		#endregion

		#region Properties ************************************************************************

		/// <summary>
		/// Timeout period for commands, in seconds.
		/// </summary>
		public int Timeout
		{
			get { return _timeout; }
			set { _timeout = value; }
		}

		/// <summary>
		/// Default timeout period for commands, in seconds.
		/// </summary>
		public int DefaultTimeout
		{
			get 
			{
				lock (_lockObject)  // Multi-threaded protection
				{
					if (_defaultTimeout == -1)
					{
						using (SqlCommand command = new SqlCommand())
						{
							_defaultTimeout = command.CommandTimeout;
						}
					}
					return _defaultTimeout;
				}
			}
		}

		/// <summary>
		/// Connection string used to connect to database.
		/// </summary>
		public string ConnectionString
		{
			get { return _connection.ConnectionString; }
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
			_timeout = this.DefaultTimeout;
		}

		/// <summary>
		/// Updates the data source with changes that have been made to a DataTable.  
		/// </summary>
		/// <param name="dataTable">DataTable containing changes that are to be merged back into 
		/// the original data source.</param>
		/// <param name="rollbackAllOnError">If an error occurs this determines whether all 
		/// changes will be rolled back, or only those changes with errors.</param>
		/// <param name="errorMessage">Output Parameter: Error message resulting from attempting 
		/// to update the data source.</param>
		/// <returns>DatabaseUpdateResult enumeration value.</returns>
		/// <remarks>After a record in the data source is updated successfully the matching record in the 
		/// DataTable will have its AcceptChanges method called, to commit the changes.  
		/// 
		/// If the rollbackAllOnError parameter is set and an error occurs then all changes, 
		/// including those that have succeeded, will be rolled back in the data table and in 
		/// the source database.  
		/// 
		/// If an error occurs when the rollbackAllOnError parameter is cleared then rows where 
		/// the update has succeeded will be committed in the data table and in the source 
		/// database.  Data table rows where the update failed will be left in their modified 
		/// states (ie, with RowState of Added, Deleted or Modified, as appropriate) for the 
		/// client code to handle.  These rows will have their RowError and HasErrors properties 
		/// set.  
		/// 
		/// This UpdateSource method will not abort if there is any error while updating a 
		/// row in the data source.  It will continue to process all the changes in the data table.
		/// 
		/// REQUIREMENTS:
		/// This method will fail if these requirements are not met.
		/// 
		/// 1) That the table being updated has a primary key column.  
		/// 2) That the name of the data table matches the name of the table in the source 
		/// database that will be updated, and the column names in the data table and database 
		/// table match.
		/// </remarks>
		public DatabaseUpdateResult UpdateSource(DataTable dataTable, bool rollbackAllOnError,
			out string errorMessage)
		{
			lock (_lockObject)
			{
				using (SqlDataAdapter dataAdapter = new SqlDataAdapter())
				{
					return this.UpdateSource(dataTable, dataAdapter, rollbackAllOnError,
						_timeout, out errorMessage);
				}
			}
		}

		/// <summary>
		/// Updates the data source with changes that have been made to a DataTable, using the 
		/// specified data adapter.  
		/// </summary>
		/// <param name="dataTable">DataTable containing changes that are to be merged back into 
		/// the original data source.</param>
		/// <param name="dataAdapter">Data adapter that will be used to update the data source.
		/// </param>
		/// <param name="rollbackAllOnError">If an error occurs this determines whether all 
		/// changes will be rolled back, or only those changes with errors.</param>
		/// <param name="timeout">Timeout, in seconds, for commands used to update database.</param>
		/// <param name="errorMessage">Output Parameter: Error message resulting from attempting 
		/// to update the data source.  Will be an empty string if there is no error.</param>
		/// <returns>DatabaseUpdateResult enumeration value.</returns>
		/// <remarks>If the DataAdapter does not already have its InsertCommand, UpdateCommand 
		/// or DeleteCommand properties set they will be automatically created.  
		/// 
		/// After a record in the data source is updated successfully the matching record in the 
		/// DataTable will have its AcceptChanges method called, to commit the changes.  
		/// 
		/// If the rollbackAllOnError parameter is set and an error occurs then all changes, 
		/// including those that have succeeded, will be rolled back in the data table and in 
		/// the source database.  
		/// 
		/// If an error occurs when the rollbackAllOnError parameter is cleared then rows where 
		/// the update has succeeded will be committed in the data table and in the source 
		/// database.  Data table rows where the update failed will be left in their modified 
		/// states (ie, with RowState of Added, Deleted or Modified, as appropriate) for the 
		/// client code to handle.  These rows will have their RowError and HasErrors properties 
		/// set.  
		/// 
		/// This UpdateSource method will not abort if there is any error while updating a 
		/// row in the data source.  It will continue to process all the changes in the data table.
		/// 
		/// The data adapter will not be disposed at the end of the method.  The client code is 
		/// responsiible for diposing the data adapter.
		/// 
		/// REQUIREMENTS:
		/// This method will fail if these requirements are not met.
		/// 
		/// 1) That the table being updated has a primary key column.  
		/// 2) That the name of the data table matches the name of the table in the source 
		/// database that will be updated, and the column names in the data table and database 
		/// table match.
		/// </remarks>
		public DatabaseUpdateResult UpdateSource(DataTable dataTable, SqlDataAdapter dataAdapter,
			bool rollbackAllOnError, int timeout, out string errorMessage)
		{
			errorMessage = string.Empty;
			lock (_lockObject)
			{
				if (_connection == null)
				{
					errorMessage = "Connection has not been set up.";
					return DatabaseUpdateResult.Failed;
				}

				if (dataTable == null)
				{
					errorMessage = "No DataTable, containing the rows to update, was supplied.";
					return DatabaseUpdateResult.Failed;
				}

				if (dataAdapter == null)
				{
					errorMessage = "No DataAdapter to perform the update on the data source.";
					return DatabaseUpdateResult.Failed;
				}

				if (dataAdapter.SelectCommand == null)
				{
					string columnListText = GetColumnListText(dataTable, true, false);
					if (columnListText == null)
					{
						errorMessage = "Unable to get a list of the columns in the data table.  "
							+ "Cannot create a SelectCommand for the DataAdapter.";
						return DatabaseUpdateResult.Failed;
					}
					string selectQuery = string.Format("SELECT {0} FROM {1};",
						columnListText, dataTable.TableName);
					SqlCommand selectCommand = new SqlCommand(selectQuery, _connection);
					dataAdapter.SelectCommand = selectCommand;
				}

				int initialSelectTimeout = dataAdapter.SelectCommand.CommandTimeout;
				int initialInsertTimeout = 0;
				int initialUpdateTimeout = 0;
				int initialDeleteTimeout = 0;
				dataAdapter.SelectCommand.CommandTimeout = timeout;

				try
				{
					// Just get changed rows from DataTable - quicker to update the data source.
					DataTable changedRows = dataTable.GetChanges();

					if (changedRows == null || changedRows.Rows.Count == 0)
					{
						return DatabaseUpdateResult.Success;
					}

					// Must set this to false otherwise AcceptChanges method of data table will be 
					//	automatically called for each row as the Update method executes.  If that 
					//	happens any newly added rows with identity values will not merge back into 
					//	the data table.  Once AcceptChanges has been called the value of the 
					//	identity column will reflect the value in the database, not the value in 
					//	the original data table.  When the Merge method is called the data adapter 
					//	will not be able to match the added row back to the equivalent row in 
					//	the original data table.  The row will be added as a new row to the data 
					//	table, instead of the original row having its identity value updated.
					dataAdapter.AcceptChangesDuringUpdate = false;

					dataAdapter.ContinueUpdateOnError = true;

					if (_connection.State == ConnectionState.Closed)
					{
						_connection.Open();
					}

					// Connection must be open to create a transaction.
					SqlTransaction transaction
						= _connection.BeginTransaction(IsolationLevel.ReadCommitted);

					// Build commands the data adapter uses to insert, delete and update records 
					//	in the source database, if they are not already specified.
					SetDataAdapterCommands(dataTable, dataAdapter, transaction);
					initialInsertTimeout = dataAdapter.InsertCommand.CommandTimeout;
					initialUpdateTimeout = dataAdapter.UpdateCommand.CommandTimeout;
					initialDeleteTimeout = dataAdapter.DeleteCommand.CommandTimeout;
					dataAdapter.InsertCommand.CommandTimeout = timeout;
					dataAdapter.UpdateCommand.CommandTimeout = timeout;
					dataAdapter.DeleteCommand.CommandTimeout = timeout;

					int numberRowsUpdated = 0;
					try
					{
						numberRowsUpdated = dataAdapter.Update(changedRows);
					}
					catch (ArgumentNullException ex)
					{
						RollbackException(transaction);
						errorMessage = "The supplied DataTable is invalid.  Exception message: "
							+ ex.Message;
						return DatabaseUpdateResult.Failed;
					}
					catch (InvalidOperationException ex)
					{
						RollbackException(transaction);
						errorMessage = "The table in the data source is invalid.  Exception message: "
							+ ex.Message;
						return DatabaseUpdateResult.Failed;
					}
					catch (DBConcurrencyException ex)
					{
						RollbackException(transaction);
						errorMessage = "Failed when executing a command on the data source: "
							+ "No records were affected by the command.  Exception message: "
							+ ex.Message;
						return DatabaseUpdateResult.Failed;
					}

					DatabaseUpdateResult result 
						= GetUpdateResult(changedRows, numberRowsUpdated, out errorMessage);

					if (rollbackAllOnError && result != DatabaseUpdateResult.Success)
					{
						RollbackException(transaction);
						dataTable.RejectChanges();
						return DatabaseUpdateResult.Failed;
					}

					transaction.Commit();

					AcceptDataTableChanges(dataTable, changedRows, result);

					return result;
				}
				catch (SqlException ex)
				{
					errorMessage = ex.Message;
					if (ex.Number == (int)SqlExceptionNumbers.TimeoutExpired
					   || ex.Number == (int)SqlExceptionNumbers.UnableToConnect)
					{
						return DatabaseUpdateResult.TimedOut;
					}

					return DatabaseUpdateResult.Failed;
				}
				catch (Exception ex)
				{
					errorMessage = ex.Message;
					return DatabaseUpdateResult.Failed;
				}
				finally
				{
					// Close connection.  Cannot use "using" block because do not want to 
					//	dispose of the connection as it may be used by other code.
					if (_connection.State != ConnectionState.Closed)
					{
						_connection.Close();
					}

					// Reset command timeouts to initial values.
					if (dataAdapter.SelectCommand != null)
					{
						dataAdapter.SelectCommand.CommandTimeout = initialSelectTimeout;
					}
					if (dataAdapter.InsertCommand != null)
					{
						dataAdapter.InsertCommand.CommandTimeout = initialInsertTimeout;
					}
					if (dataAdapter.UpdateCommand != null)
					{
						dataAdapter.UpdateCommand.CommandTimeout = initialUpdateTimeout;
					}
					if (dataAdapter.DeleteCommand != null)
					{
						dataAdapter.DeleteCommand.CommandTimeout = initialDeleteTimeout;
					}
				}
			}
		}

		#endregion

		#region Event Handlers ********************************************************************

		#endregion

		#region Private and Protected Methods *****************************************************

		/// <summary>
		/// Sets the commands used by the data adapter to insert, delete and update records 
		/// in the source database, if they are not already set.
		/// </summary>
		/// <param name="dataTable">DataTable containing changes that are to be merged back into 
		/// the original source database.</param>
		/// <param name="dataAdapter">DataAdapter that will be used to update the source database.
		/// </param>	
		/// <param name="transaction">Transaction that the commands will be enlisted in, so that 
		/// the changes can be rolled back on error.</param>
		/// <remarks>ASSUMPTIONS:
		/// 1) The data table passed in as an argument already has its schema filled by the 
		/// data adapter.
		/// 2) SelectCommand property has already been set for the data adapter.  The data adapter 
		/// will use the SelectCommand to query the database to determine the schema of the 
		/// table that will be updated.
		/// 3) The schema of the table to be updated must include a primary key column or a 
		/// unique column.  This will allow the DataAdapter to uniquely identify each row that is 
		/// being updated.</remarks>
		private void SetDataAdapterCommands(DataTable dataTable, SqlDataAdapter dataAdapter,
			SqlTransaction transaction)
		{
			// The CommandBuilder must be disposed otherwise it will recreate the commands when 
			//	the DataAdapter Update method is called, even though the CommandBuilder variable 
			//	is only scoped within this method.  If it recreates the InsertCommand 
			//	the customizations added will be lost.
			using (SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter))
			{
				// All 4 commands (Select, Insert, Delete and Update) must be enlisted in the 
				//	transaction.  Select must be enlisted in the transaction before creating 
				//	any of the other commands, otherwise an exception is thrown.
				dataAdapter.SelectCommand.Transaction = transaction;

				if (dataAdapter.DeleteCommand == null)
				{
					// When the CommandBuilder is disposed, the commands it created will also be 
					//	disposed.  So clone the created commands to prevent them from being 
					//	disposed with the CommandBuilder.
					dataAdapter.DeleteCommand = commandBuilder.GetDeleteCommand(true).Clone();
				}

				if (dataAdapter.UpdateCommand == null)
				{
					dataAdapter.UpdateCommand = commandBuilder.GetUpdateCommand(true).Clone();
				}

				if (dataAdapter.InsertCommand == null)
				{
					// Build own InsertCommand so can extract any newly created identity column 
					//	values from source database and merge them back into the data table.
					dataAdapter.InsertCommand = GetInsertCommand(dataTable, commandBuilder).Clone();
				}

				dataAdapter.DeleteCommand.Transaction = transaction;
				dataAdapter.UpdateCommand.Transaction = transaction;
				dataAdapter.InsertCommand.Transaction = transaction;
			}
		}

		/// <summary>
		/// Builds a command for inserting new records into the source database.
		/// </summary>
		/// <param name="dataTable">DataTable with the same schema as the source table in the 
		/// underlying database.</param>
		/// <returns>A command for inserting new records into a source database.</returns>
		/// <remarks>ASSUMPTIONS:
		/// 1) That the data table will have the same table name and column names as the 
		/// table in the underlying database that is to be updated.
		/// 
		/// The insert command that is returned is a modification of the standard command 
		/// generated by a CommandBuilder.  If the table to be updated contains an identity 
		/// column, an output parameter will be added to the insert command.  This output 
		/// parameter will extract any newly created identity column value from source 
		/// database and merge it back into the data table.</remarks>
		private SqlCommand GetInsertCommand(DataTable dataTable, SqlCommandBuilder commandBuilder)
		{
			SqlCommand insertCommand = commandBuilder.GetInsertCommand();

			List<DataColumn> identityColumns = GetColumnList(dataTable, 
				new Predicate<DataColumn>(this.IsIdentityColumn));
			if (identityColumns == null || identityColumns.Count <= 0)
			{
				return insertCommand;
			}

			DataColumn identityColumn = identityColumns[0];
			string parameterName = "@IdentityValue";

			// COMMENT OUT THIS CODE WHEN RUNNING THE APPLICATION NORMALLY.
			// UNCOMMENT IT FOR TESTING, TO GENERATE A RUNTIME ERROR WHEN THE DataAdapter.Update 
			//	METHOD IS CALLED.  
			//	No exception will be thrown but the RowError and HasErrors properties of any 
			//	row that is being added will be set.
			//	The reason it fails is that a SqlDbType should be passed into the parameter 
			//	constructor, not a System.Type.
			//SqlParameter oprmIdentity = new SqlParameter(parameterName, identityColumn.DataType);
			//oprmIdentity.Direction = ParameterDirection.Output;
			//oprmIdentity.SourceColumn = identityColumn.ColumnName;			 
			// END OF TEST CODE.

			// UNCOMMENT THIS CODE WHEN RUNNING THE APPLICATION NORMALLY.
			// COMMENT IT OUT FOR TESTING.
			SqlParameter oprmIdentity = new SqlParameter();
			oprmIdentity.ParameterName = parameterName;
			oprmIdentity.Direction = ParameterDirection.Output;
			oprmIdentity.DbType = ConvertSystemTypeToDbType(identityColumn.DataType);
			oprmIdentity.SourceColumn = identityColumn.ColumnName;
			// END OF PRODUCTION CODE.

			insertCommand.UpdatedRowSource = UpdateRowSource.OutputParameters;

			// SCOPE_IDENTITY() is a SQL Server function that returns the last identity value 
			//	inserted into the database within the current execution scope.  Better than 
			//	@@IDENTITY which is not confined to the current scope.
			if (!insertCommand.CommandText.Trim().EndsWith(";"))
			{
				insertCommand.CommandText += ";";
			}
			insertCommand.CommandText += string.Format(" SET {0} = SCOPE_IDENTITY();",
				parameterName);
			insertCommand.Parameters.Add(oprmIdentity);

			return insertCommand;
		}

		/// <summary>
		/// Determines the result of the attempt to update the source database with changes to the 
		/// data table.
		/// </summary>
		/// <param name="changedRows">The rows from the data table that were changed.</param>
		/// <param name="numberRowsUpdatedInSource">The number of rows that were updated in the 
		/// source database.</param>
		/// <param name="errorMessage">Output parameter: Any error message as a result of 
		/// attempting to update the source database.</param>
		/// <param name="result">DatabaseUpdateResult indicating whether the update attempt on the 
		/// source database was a success, a failure, or a partial success (some rows were 
		/// successfully updated but others were not).</param>
		private DatabaseUpdateResult GetUpdateResult(DataTable changedRows,
			int numberRowsUpdatedInSource, out string errorMessage)
		{
			errorMessage = string.Empty;

			if (!changedRows.HasErrors)
			{
				return DatabaseUpdateResult.Success;
			}

			DatabaseUpdateResult result = DatabaseUpdateResult.Failed;

			DataRow[] errorRows = changedRows.GetErrors();
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("The following errors were encountered while attempting "
				+ "to update the data source:");
			foreach (DataRow row in errorRows)
			{
				sb.AppendLine(row.RowError);
			}
			errorMessage = sb.ToString();

			// Partial Success: Some of the row updates have succeeded, some have failed.
			if (numberRowsUpdatedInSource > 0
				&& numberRowsUpdatedInSource < changedRows.Rows.Count)
			{
				result = DatabaseUpdateResult.PartialSuccess;
			}

			return result;
		}

		/// <summary>
		/// Accepts the changes made to the data table after the source database has been updated.
		/// </summary>
		/// <param name="dataTable">Data table that was the source of the changes made to the 
		/// database.</param>
		/// <param name="changedRows">The rows from the data table that were changed.</param>
		/// <param name="databaseUpdateResult">The result of attempting to update the database 
		/// with the changes from the data table.</param>
		/// <remarks>This method will only be called if the update was successful or if the 
		/// rollbackAllOnError parameter of the UpdateSource method is set to false.  If there 
		/// is an error and rollbackAllOnError is set to true, all the changes would have been 
		/// rolled back and this method would never be called.
		/// 
		/// Changes will be accepted only for those rows in the data table where the update of 
		/// the source database succeeded.  Rows where the database update failed will NOT have 
		/// their changes either accepted or rejected.  ie the RowStates of the rows where the 
		/// database update failed will be left as either Added, Modified, or Deleted, as 
		/// appropriate.  It will be the responsibility of the client code to deal with these 
		/// rows.  The rows which failed will also have their HasErrors properties set.</remarks>
		private void AcceptDataTableChanges(DataTable dataTable, DataTable changedRows,
			DatabaseUpdateResult databaseUpdateResult)
		{
			// Merge the changed rows back into the original data table.  For any rows that 
			//	were added this will update the value of any identity column to match the value 
			//	in the database.
			dataTable.Merge(changedRows);

			// After the merge any identity column values will have been updated so it is okay 
			//	to AcceptChanges.  This will reset the RowState of any changed rows.
			if (databaseUpdateResult == DatabaseUpdateResult.Success)
			{
				dataTable.AcceptChanges();
			}
			else if (databaseUpdateResult == DatabaseUpdateResult.PartialSuccess)
			{
				// Cannot use foreach to iterate through the dataTable.Rows collection - fails 
				//	on the next iteration of the loop after a deleted row has changes accepted
				//	(ie after deleted row is removed from the collection).
				DataRow[] rows = new DataRow[dataTable.Rows.Count];
				dataTable.Rows.CopyTo(rows, 0);
				foreach (DataRow row in rows)
				{
					if (!row.HasErrors)
					{
						row.AcceptChanges();
					}
				}
			}
			// If all rows failed (DatabaseUpdateResult.Failed) would not want any of the rows
			//	accepted, so do nothing.
		}

		/// <summary>
		/// Returns a comma-separated list of the names of the columns in a data table.
		/// </summary>
		/// <param name="dataTable">Data table whose column names will be read.</param>
		/// <param name="includeIdentityColumn">If true, any identity column in the table 
		/// will be included in the list.  If false, any identity column will be excluded.</param>
		/// <param name="isParameterList">If true, the column names will have "@" prefixes 
		/// added, to make them into a list of parameters.  If false the "@" prefixes will 
		/// be excluded.</param>
		/// <returns>null if the data table is null, otherwise a comma-separated string.</returns>
		private string GetColumnListText(DataTable dataTable, bool includeIdentityColumn,
			bool isParameterList)
		{
			if (dataTable == null)
			{
				return null;
			}

			Predicate<DataColumn> filterFunction = null;
			if (!includeIdentityColumn)
			{
				filterFunction = new Predicate<DataColumn>(this.IsNotIdentityColumn);
			}

			List<DataColumn> columnList = GetColumnList(dataTable, filterFunction);

			List<string> columnNameList = columnList.ConvertAll<string>(
				new Converter<DataColumn, string>(GetDataColumnName));

			string leadingParameterSymbol = string.Empty;
			string separator = ", ";
			if (isParameterList)
			{
				leadingParameterSymbol = "@";
				separator += "@";
			}
			return leadingParameterSymbol + JoinStringList(separator, columnNameList);
		}

		/// <summary>
		/// Gets a list of the columns in a data table, that may or may not be filtered.
		/// </summary>
		/// <param name="dataTable">Data table whose columns are to be retrieved.</param>
		/// <param name="match">Predicate used to filter columns.  If set to null, all 
		/// columns are returned.</param>
		/// <returns>null if the data table is null, otherwise a DataColumn list.</returns>
		private List<DataColumn> GetColumnList(DataTable dataTable,
			Predicate<DataColumn> match)
		{
			if (dataTable == null)
			{
				return null;
			}

			DataColumn[] columns = new DataColumn[dataTable.Columns.Count];
			dataTable.Columns.CopyTo(columns, 0);
			List<DataColumn> columnList = new List<DataColumn>(columns);
			if (match == null)
			{
				return columnList;
			}
			return columnList.FindAll(match);
		}

		/// <summary>
		/// Indicates whether a data column is an identity column.
		/// </summary>
		/// <param name="column">DataColumn to check.</param>
		/// <returns>true if the column is an identity column, false otherwise.</returns>
		/// <remarks>Used as a predicate in the DataColumn List FindAll method.</remarks>
		private bool IsIdentityColumn(DataColumn column)
		{
			return column.AutoIncrement;
		}

		/// <summary>
		/// Indicates whether a data column is NOT an identity column.
		/// </summary>
		/// <param name="column">DataColumn to check.</param>
		/// <returns>true if the column is not an identity column, false otherwise.</returns>
		/// <remarks>Used as a predicate in the DataColumn List FindAll method.</remarks>
		private bool IsNotIdentityColumn(DataColumn column)
		{
			return !column.AutoIncrement;
		}

		/// <summary>
		/// Extracts the name from a DataColumn object.
		/// </summary>
		/// <param name="column">DataColumn whose name is to be extracted.</param>
		/// <returns>null if the DataColumn object is null, otherwise a string representing 
		/// the name of the DataColumn.</returns>
		/// <remarks>This method can be passed to the ConvertAll method of a DataColumn List, 
		/// to convert it to a string List containing the DataColumn names.</remarks>
		private string GetDataColumnName(DataColumn column)
		{
			if (column == null)
			{
				return null;
			}

			return column.ColumnName;
		}

		/// <summary>
		/// Performs a Join, similar to a string.Join but takes a string List rather than an array 
		/// as an argument.
		/// </summary>
		/// <param name="separator">Separator between the elements of the joined string.</param>
		/// <param name="list">string List whose elements will be joined.</param>
		/// <exception cref="ArgumentNullException">list parameter is null.</exception>
		/// <returns>string containing the concatenated elements of the string List, with adjacent 
		/// elements separated by the separator text.  If the list contains no elements 
		/// string.Empty is returned.</returns>
		private string JoinStringList(string separator, List<string> list)
		{
			if (list == null)
			{
				throw new ArgumentNullException("list");
			}

			if (list.Count == 0)
			{
				return string.Empty;
			}

			string[] array = list.ToArray();
			return string.Join(separator, array);
		}

		/// <summary>
		/// Converts a System.Type to a DbType enum value.
		/// </summary>
		/// <param name="typeToConvert">System.Type to convert.</param>
		/// <returns>DbType that is equivalent to the System.Type supplied.  Returns DbType.String 
		/// by default if unable to convert the System.Type.</returns>
		/// <remarks>Useful for converting DataColumn.Type to DbType that can be used in a 
		/// SqlParameter.  Conversion does not work for SqlDbType.  However, SqlParameters can use 
		/// either DbType or SqlDbType.</remarks>
		private DbType ConvertSystemTypeToDbType(Type typeToConvert)
		{
			// Books Online says it's better to use the GetConverter overload with the object 
			//	argument rather than the overload with the type argument.  Can use any DbType 
			//	value for the argument.
			System.ComponentModel.TypeConverter converter
				= System.ComponentModel.TypeDescriptor.GetConverter(DbType.Int32);
			// Cannot check converter.CanConvertFrom(typeToConvert) because it will say conversion 
			//	cannot be done.  Just do it.
			try
			{
				return (DbType)converter.ConvertFrom(typeToConvert.Name);
			}
			catch (Exception ex)
			{
				return DbType.String;
			}
		}

		/// <summary>
		/// Rollback the transaction that was updating the source database.
		/// </summary>
		/// <param name="transaction">Transaction to roll back.</param>
		/// <remarks>try - catch needed because the Rollback method will throw an exception if 
		/// the transaction has already been committed or rolled back.</remarks>
		private void RollbackException(SqlTransaction transaction)
		{
			try
			{
				if (transaction != null)
				{
					transaction.Rollback();
				}
			}
			catch { }
		}

		#endregion
	}
}