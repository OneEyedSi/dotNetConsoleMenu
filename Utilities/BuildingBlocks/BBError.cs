///////////////////////////////////////////////////////////////////////////////////////////////////
// Project      -   Datacom Building Blocks
// General      -   Classes that may be used by multiple projects in multiple solutions. 
//					Higher-level classes than those in the Utilities assemblies.
//
// File Name    -   Error.cs
// Description  -   Error-related Routines
//
// Notes        -   
//
// $History: BBError.cs $
// 
// *****************  Version 10  *****************
// User: Simone       Date: 5/04/09    Time: 3:53p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// MINOR: DatabaseManager2.ExecCommand renamed ExecStoredProc to make its
// function clearer.
// 
// *****************  Version 9  *****************
// User: Simone       Date: 5/04/09    Time: 2:29p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// Replaced Utilities.DataAcess.DatabaseManager with DatabaseManager2.
// 
// *****************  Version 8  *****************
// User: Simone       Date: 13/03/09   Time: 10:07a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// MINOR: Removed redundant "using" statements.
// 
// *****************  Version 7  *****************
// User: Simone       Date: 12/03/09   Time: 12:07p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// Added overloads for GetErrorMessage and GetErrorDescription.
// 
// *****************  Version 6  *****************
// User: Simone       Date: 11/03/09   Time: 2:01p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// Static methods made thread safe.
// 
// *****************  Version 5  *****************
// User: Simone       Date: 11/03/09   Time: 1:25p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// GetErrorMessage, GetErrorDescription: Parameter changed from config
// file section name to SystemCoreSettings object.
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
// Error-related Routines.
///////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Utilities.Database;
using Utilities.Miscellaneous;

namespace Utilities.BuildingBlocks
{
	#region Error-related Enums, Structs, Static Classes, etc *************************************

	/// <summary>
	/// Information relating to an error that has occurred.
	/// </summary>
	/// <typeparam name="T">The type of the enum that lists all the valid error codes.</typeparam>
	public struct ErrorInfo<T>
	{
		public ErrorInfo(T errorCode, string errorMessage, int dbErrorValue)
		{
			ErrorCode = errorCode;
			Message = errorMessage;
			DatabaseErrorValue = dbErrorValue;
		}

		public T ErrorCode;
		public string Message;
		public int DatabaseErrorValue;	// Database should use same values as the ErrorCode enum 
		//	but add additional field in case the values don't match.
	}

	#endregion

	#region Error-related Routines ****************************************************************

	/// <summary>
	/// Error-related routines.
	/// </summary>
	public class BBError
	{
		#region Class Data Members ****************************************************************

		private static object _lockGetErrorMessage_1 = new object();
		private static object _lockGetErrorMessage_2 = new object();
		private static object _lockGetErrorDescription_1 = new object();
		private static object _lockGetErrorDescription_2 = new object();
		private static object _lockValidateDBErrorValue = new object();

		#endregion

		/// <summary>
		/// Returns the error message in an ErrorInfo object.  If the error message is blank and 
		/// an error has occurred then returns the description of the error.
		/// </summary>
		/// <typeparam name="T">The type of the enum that lists all the valid error codes.</typeparam>
		/// <param name="errorInfo">ErrorInfo object containing information about an error.</param>
		/// <param name="successErrorCode">The error code that represents success.</param>
		/// <param name="defaultErrorCode">The default error that the ErrorInfo object may be set 
		/// to.</param>
		/// <param name="systemSettings">Settings read from a custom section of the config file 
		/// that contains connection information for the database.</param>
		/// <returns>An error message.</returns>
		public static string GetErrorMessage<TErrorCodeEnum>(ErrorInfo<TErrorCodeEnum> errorInfo,
			TErrorCodeEnum successErrorCode, TErrorCodeEnum defaultErrorCode,
			SystemCoreSettings systemSettings)
			where TErrorCodeEnum : IComparable, IFormattable, IConvertible
		{
			DatabaseManager2 databaseManager = null;
			lock (_lockGetErrorMessage_1)
			{
				databaseManager = BBDatabase.GetDatabaseManager(systemSettings);
				if (databaseManager == null)
				{
					return string.Empty;
				}
			}

			return BBError.GetErrorMessage<TErrorCodeEnum>(errorInfo, successErrorCode,
				defaultErrorCode, databaseManager);
		}

		/// <summary>
		/// Returns the error message in an ErrorInfo object.  If the error message is blank and 
		/// an error has occurred then returns the description of the error.
		/// </summary>
		/// <typeparam name="TErrorCodeEnum">The type of the enum that lists all the valid error 
		/// codes.</typeparam>
		/// <param name="errorInfo">ErrorInfo object containing information about an error.</param>
		/// <param name="successErrorCode">The error code that represents success.</param>
		/// <param name="defaultErrorCode">The default error that the ErrorInfo object may be set 
		/// to.</param>
		/// <param name="systemSettings">Settings read from a custom section of the config file 
		/// that contains connection information for the database.</param>
		/// <returns>An error message.</returns>
		public static string GetErrorMessage<TErrorCodeEnum>(ErrorInfo<TErrorCodeEnum> errorInfo,
			TErrorCodeEnum successErrorCode, TErrorCodeEnum defaultErrorCode,
			DatabaseManager2 databaseManager)
			where TErrorCodeEnum : IComparable, IFormattable, IConvertible
		{
			lock (_lockGetErrorMessage_2)
			{
				string errorMessage = (errorInfo.Message ?? string.Empty).Trim();

				if (errorInfo.Message.Length == 0
					&& errorInfo.ErrorCode.Equals(successErrorCode)
					&& errorInfo.ErrorCode.Equals(defaultErrorCode))
				{
					errorMessage = BBError.GetErrorDescription<TErrorCodeEnum>(errorInfo.ErrorCode, 
						databaseManager);
				}

				return errorMessage;
			}
		}

		/// <summary>
		/// Gets the description of a specified error.
		/// </summary>
		/// <typeparam name="TErrorCodeEnum">The type of the enum that lists all the valid error 
		/// codes.</typeparam>
		/// <param name="codeToLookup">Error code whose description is to be returned.</param>
		/// <param name="systemSettings">Settings read from a custom section of the config file 
		/// that contains connection information for the database.</param>
		/// <returns>The description for the specified error code.  If the specified error code is 
		/// not found, returns an empty string.</returns>
		public static string GetErrorDescription<TErrorCodeEnum>(TErrorCodeEnum codeToLookup,
			SystemCoreSettings systemSettings)
			where TErrorCodeEnum : IComparable, IFormattable, IConvertible
		{
			DatabaseManager2 databaseManager = null;
			lock (_lockGetErrorDescription_1)
			{
				databaseManager = BBDatabase.GetDatabaseManager(systemSettings);
				if (databaseManager == null)
				{
					return string.Empty;
				}
			}
			return GetErrorDescription<TErrorCodeEnum>(codeToLookup, databaseManager);
		}

		/// <summary>
		/// Gets the description of a specified error.
		/// </summary>
		/// <typeparam name="TErrorCodeEnum">The type of the enum that lists all the valid error 
		/// codes.</typeparam>
		/// <param name="codeToLookup">Error code whose description is to be returned.</param>
		/// <param name="databaseManager">DatabaseManager that handles the connection to the 
		/// database.</param>
		/// <returns>The description for the specified error code.  If the specified error code is 
		/// not found, returns an empty string.</returns>
		public static string GetErrorDescription<TErrorCodeEnum>(TErrorCodeEnum codeToLookup,
			DatabaseManager2 databaseManager)
			where TErrorCodeEnum : IComparable, IFormattable, IConvertible
		{
			lock (_lockGetErrorDescription_2)
			{
				try
				{
					SqlParameter prmError = new SqlParameter("@ErrorCode", codeToLookup);
					SqlParameter oprmDescription = new SqlParameter("@oDescription",
						SqlDbType.VarChar, 100);
					oprmDescription.Direction = ParameterDirection.Output;
					SqlParameter[] aParams = new SqlParameter[] {	prmError, 
																oprmDescription };
					int dummyReturnValue;
					string dummyErrorMessage;
					databaseManager.ExecStoredProc("p_GetErrorDescription", aParams,
						out dummyReturnValue, out dummyErrorMessage);

					if (oprmDescription.Value != DBNull.Value)
					{
						return (string)oprmDescription.Value;
					}

					return string.Empty;
				}
				catch
				{
					return string.Empty;
				}
			}
		}

		/// <summary>
		/// Checks to see if the error value returned from a call to the database is a valid 
		/// error code.
		/// </summary>
		/// <typeparam name="TErrorCodeEnum">Type of the error code enum that the database error 
		/// value should be a member of.</typeparam>
		/// <param name="dbErrorValue">Error value returned from a call to the database.</param>
		/// <param name="defaultErrorCode">Error code that is returned if the database error 
		/// value is not valid.</param>
		/// <returns>Error code enum.  If the database error value is valid it is converted to 
		/// an error code enum.  If it is not valid the default error code is returned.</returns>
		public static TErrorCodeEnum ValidateDBErrorValue<TErrorCodeEnum>(int dbErrorValue,
			TErrorCodeEnum defaultErrorCode)
			where TErrorCodeEnum : IComparable, IFormattable, IConvertible
		{
			lock (_lockValidateDBErrorValue)
			{
				TErrorCodeEnum errorCode = default(TErrorCodeEnum);
				if (MiscUtilities.ValidateEnumValue<TErrorCodeEnum>(dbErrorValue, out errorCode))
				{
					return errorCode;
				}

				return defaultErrorCode;
			}
		}
	}

	#endregion
}
