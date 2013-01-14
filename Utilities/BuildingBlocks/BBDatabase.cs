///////////////////////////////////////////////////////////////////////////////////////////////////
// Project      -   Datacom Building Blocks
// General      -   Classes that may be used by multiple projects in multiple solutions. 
//					Higher-level classes than those in the Utilities assemblies.
//
// File Name    -   Database.cs
// Description  -   Database-related Routines
//
// Notes        -   
//
// $History: BBDatabase.cs $
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
// User: Simone       Date: 11/03/09   Time: 1:24p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// GetDatabaseManager: Parameter changed from config file section name to
// SystemCoreSettings object.
// 
// *****************  Version 4  *****************
// User: Simone       Date: 10/03/09   Time: 10:38a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// MINOR: Class names have had "BB" prepended, to avoid namespace
// collisions in referencing applications which include "using"
// statements.
// 
// *****************  Version 3  *****************
// User: Simone       Date: 10/03/09   Time: 10:21a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// MINOR: GetDatabaseManager: Settings classes renamed.
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
// Database-related Routines.
///////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Utilities.Miscellaneous;
using Utilities.Database;

namespace Utilities.BuildingBlocks
{
	#region Database-related Enums, Structs, Static Classes, etc **********************************

	/// <summary>
	/// Indicates the type of database the system is connected to, eg Dev, Test, Live.
	/// </summary>
	public enum DatabaseType
	{
		NotFound = 0,
		Invalid,
		Dev,
		Test,
		Live
	}

	/// <summary>
	/// Database metadata.
	/// </summary>
	public struct DatabaseInfo
	{
		public string DatabaseTitle;
		public int DatabaseVersion;
		public DatabaseType DatabaseType;
	}

	/// <summary>
	/// Structure to hold the enum values for common database error codes.
	/// </summary>
	/// <typeparam name="T">Type of the enum that gives the valid error codes for the parent 
	/// application.</typeparam>
	public struct CommonDatabaseErrorCode<T>
		where T : IComparable, IFormattable, IConvertible
	{
		private T _success;
		private T _generalDBError;
		private T _invalidType;
		private T _noInfo;
		private T _generalError;

		private int _successValue;
		private int _generalDBErrorValue;
		private int _invalidTypeValue;
		private int _noInfoValue;
		private int _generalErrorValue;

		public CommonDatabaseErrorCode(T success, T generalDBError, T invalidType, T noInfo, 
			T generalError)
		{
			_success = success;
			_generalDBError = generalDBError;
			_invalidType = invalidType;
			_noInfo = noInfo;
			_generalError = generalError;

			_successValue = _success.ToInt32(null);
			_generalDBErrorValue = _generalDBError.ToInt32(null);
			_invalidTypeValue = _invalidType.ToInt32(null);
			_noInfoValue = _noInfo.ToInt32(null);
			_generalErrorValue = _generalError.ToInt32(null);
		}

		public T Success
		{
			get { return _success; }
			set
			{
				_success = value;
				_successValue = _success.ToInt32(null);
			}
		}

		public T GeneralDBError
		{
			get { return _generalDBError; }
			set
			{
				_generalDBError = value;
				_generalDBErrorValue = _generalDBError.ToInt32(null);
			}
		}

		public T InvalidType
		{
			get { return _invalidType; }
			set
			{
				_invalidType = value;
				_invalidTypeValue = _invalidType.ToInt32(null);
			}
		}

		public T NoInfo
		{
			get { return _noInfo; }
			set
			{
				_noInfo = value;
				_noInfoValue = _noInfo.ToInt32(null);
			}
		}

		public T GeneralError
		{
			get { return _generalError; }
			set
			{
				_generalError = value;
				_generalErrorValue = _generalError.ToInt32(null);
			}
		}

		public int SuccessValue
		{
			get { return _successValue; }
		}

		public int GeneralDBErrorValue
		{
			get { return _generalDBErrorValue; }
		}

		public int InvalidTypeValue
		{
			get { return _invalidTypeValue; }
		}

		public int NoInfoValue
		{
			get { return _noInfoValue; }
		}

		public int GeneralErrorValue
		{
			get { return _generalErrorValue; }
		}
	}

	#endregion

	#region Database-related Static Methods *******************************************************

	/// <summary>
	/// Database-related routines.
	/// </summary>
	public class BBDatabase
	{
		#region Class Data Members ****************************************************************

		private static object _lockGetDatabaseManager = new object();
		private static object _lockGetDatabaseInfo = new object();

		#endregion

		/// <summary>
		/// Gets the database manager used for connecting to a database.
		/// </summary>
		/// <param name="systemSettings">Settings read from a custom section of the config file 
		/// that contains connection information for the database.</param>
		/// <returns>Database manager object, configured to connect to the database, or null if 
		/// unable to read the connection information from the config file.</returns>
		public static DatabaseManager2 GetDatabaseManager(SystemCoreSettings systemSettings)
		{
			lock (_lockGetDatabaseManager)
			{
				if (systemSettings == null)
				{
					return null;
				}

				DatabaseCoreSettings databaseSettings = systemSettings.Database;
				string server = databaseSettings.DatabaseServer;
				string database = databaseSettings.Database;
				string login = databaseSettings.DatabaseLogin;
				string password = databaseSettings.DatabasePassword;
				DatabaseManager2 databaseManager = new DatabaseManager2(server, database, login, 
					password);
				return databaseManager;
			}
		}

		/// <summary>
		/// Retrieves information about a database that the application is currently connected to. 
		/// </summary>
		/// <param name="databaseManager">Database manager object that handles the connection to 
		/// the database.</param>
		/// <param name="commonDatabaseErrorCode">Structure holding the common database-related 
		/// error codes.</param>
		/// <param name="oErrorInfo">Output parameter: Details of any error that may have 
		/// occurred.</param>
		/// <returns>A structure containing the database metadata.</returns>
		public static DatabaseInfo GetDatabaseInfo<T>(DatabaseManager2 databaseManager,
			CommonDatabaseErrorCode<T> commonDatabaseErrorCode, out ErrorInfo<T> oErrorInfo)
			where T : IComparable, IFormattable, IConvertible
		{
			DatabaseInfo dbInfo = new DatabaseInfo();
			ErrorInfo<T> errorInfo = new ErrorInfo<T>();

			lock (_lockGetDatabaseInfo)
			{
				try
				{
					errorInfo.ErrorCode = commonDatabaseErrorCode.GeneralError;
					errorInfo.Message = string.Empty;
					// Cannot convert generic type to int directly so cheat by using object.
					object value = commonDatabaseErrorCode.Success;
					errorInfo.DatabaseErrorValue = (int)value;

					DataTable dbMetaData = databaseManager.GetDataTable("p_GetDatabaseInfo",
						out errorInfo.DatabaseErrorValue, out errorInfo.Message);

					// Check for unrecognised error code from the database.
					errorInfo.ErrorCode = BBError.ValidateDBErrorValue<T>(
						errorInfo.DatabaseErrorValue, commonDatabaseErrorCode.GeneralDBError);

					if (dbMetaData != null && dbMetaData.Rows.Count > 0)
					{
						dbInfo.DatabaseTitle = (dbMetaData.Rows[0]["DatabaseTitle"]).ToString();
						dbInfo.DatabaseVersion = (int)dbMetaData.Rows[0]["DatabaseVersion"];

						// Check that database type is valid.
						string databaseTypeText = (dbMetaData.Rows[0]["DatabaseType"]).ToString();
						DatabaseType databaseType = DatabaseType.NotFound;
						if (MiscUtilities.ValidateEnumValue<DatabaseType>(databaseTypeText,
							out databaseType))
						{
							dbInfo.DatabaseType = databaseType;
							errorInfo.ErrorCode = commonDatabaseErrorCode.Success;
						}
						else
						{
							dbInfo.DatabaseType = DatabaseType.Invalid;
							errorInfo.ErrorCode = commonDatabaseErrorCode.InvalidType;
						}
					}
					else
					{
						dbInfo.DatabaseType = DatabaseType.NotFound;
						errorInfo.ErrorCode = commonDatabaseErrorCode.NoInfo;
					}
				}
				catch
				{
					errorInfo.ErrorCode = commonDatabaseErrorCode.GeneralError;
				}
			}

			oErrorInfo = errorInfo;
			return dbInfo;
		}
	}

	#endregion
}
