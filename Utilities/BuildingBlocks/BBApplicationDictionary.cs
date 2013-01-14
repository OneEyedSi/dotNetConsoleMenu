///////////////////////////////////////////////////////////////////////////////////////////////////
// Project      -   Datacom Building Blocks
// General      -   Classes that may be used by multiple projects in multiple solutions. 
//					Higher-level classes than those in the Utilities assemblies.
//
// File Name    -   ApplicationDictionary.cs
// Description  -   Methods to add and retrieve data from the application dictionary.
//
// Notes        -   
//
// $History: BBApplicationDictionary.cs $
// 
// *****************  Version 9  *****************
// User: Simone       Date: 23/04/09   Time: 4:49p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// Add SetDictionaryValue overloads which return ErrorInfo structure as
// output parameter.
// 
// *****************  Version 8  *****************
// User: Simone       Date: 5/04/09    Time: 3:53p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// MINOR: DatabaseManager2.ExecCommand renamed ExecStoredProc to make its
// function clearer.
// 
// *****************  Version 7  *****************
// User: Simone       Date: 5/04/09    Time: 2:29p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// Replaced Utilities.DataAcess.DatabaseManager with DatabaseManager2.
// 
// *****************  Version 6  *****************
// User: Simone       Date: 12/03/09   Time: 12:26p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// Removed the DictionaryKey static class that was specific to a
// particular project.
// 
// *****************  Version 5  *****************
// User: Simone       Date: 12/03/09   Time: 11:15a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// MINOR: Renamed type parameter "U" to "TErrorCodeEnum".
// 
// *****************  Version 4  *****************
// User: Simone       Date: 10/03/09   Time: 10:57a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// MINOR: Dictionary class renamed BBApplicationDictionary.
// 
// *****************  Version 3  *****************
// User: Simone       Date: 10/03/09   Time: 10:37a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// 
// *****************  Version 2  *****************
// User: Simone       Date: 10/03/09   Time: 8:50a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// Namespace changed from Datacom.BuildingBlocks to
// Utilities.BuildingBlocks.
// 
// *****************  Version 1  *****************
// User: Simone       Date: 10/03/09   Time: 8:43a
// Created in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// Methods to add and retrieve data from the application dictionary.
///////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using Utilities.Miscellaneous;
using Utilities.Database;

namespace Utilities.BuildingBlocks
{
	#region Dictionary-related Enums, Structs, Static Classes, etc ********************************

	/// <summary>
	/// Units of measure used in the Dictionary.
	/// </summary>
	public enum UnitOfMeasure
	{
		Unknown = 0,
		Each,
		Units,
		Text,
		Seconds,
		Minutes,
		Hours,
		Days,
		Weeks,
		Months,
		Years,
		Flag,
		DateTime,
		Date,
		Time
	}

	/// <summary>
	/// Specifies the type of data stored in an entry in the Dictionary.
	/// </summary>
	public enum DictionaryValueType
	{
		String,
		Numeric,
		DateTime,
		Binary
	}

	/// <summary>
	/// Miscellaneous literals used in the dictionary methods.
	/// </summary>
	public static class DictionaryLiteral
	{
		public const int InitialValue = -999;
	}

	#endregion

	#region Dictionary Class **********************************************************************

	/// <summary>
	/// Methods for saving entries to, and retrieving entries from, the Dictionary.
	/// </summary>	
	/// <typeparam name="TErrorCodeEnum">The type of the enum that lists all the valid error codes.</typeparam>
	/// <remarks>Event IDs for trace logging: IDs 100400 - 100499 reserved for this class.</remarks>
	public class BBApplicationDictionary<TErrorCodeEnum>
		where TErrorCodeEnum : IComparable, IFormattable, IConvertible
	{
		#region Data Members **********************************************************************

		DatabaseManager2 _databaseManager;
		CommonDatabaseErrorCode<TErrorCodeEnum> _commonDatabaseErrorCode;

		#endregion

		#region Constructors, Destructors / Finalizers and Dispose Methods ************************

		/// <summary>
		/// Initializes a new instance of the ServiceDeskObject class.
		/// </summary>
		/// <param name="databaseManager">DatabaseManager that handles the connection with a  
		/// database containing a dictionary table.</param>
		/// <param name="commonDatabaseErrorCode">Structure to hold the enum values for common 
		/// database error codes.</param>
		public BBApplicationDictionary(DatabaseManager2 databaseManager, 
			CommonDatabaseErrorCode<TErrorCodeEnum> commonDatabaseErrorCode)
		{
			_databaseManager = databaseManager;
			_commonDatabaseErrorCode = commonDatabaseErrorCode;
		}

		#endregion

		#region Public Methods ********************************************************************

		/// <summary>
		/// Gets value from Dictionary.  If value not found returns default value. 
		/// </summary>
		/// <param name="key">Key of dictionary entry to look up.</param>
		/// <param name="defaultValue">Default value to return if dictionary entry is not 
		/// found.</param>
		/// <param name="logSource">Trace Source, used for logging.</param>
		/// <returns>Value read from dictionary or, if that value is not found, the default 
		/// value.</returns>
		public int GetValueWithDefault(string key, int defaultValue,
			TraceSource logSource)
		{
			return GetValueWithDefault(key, null, defaultValue, logSource);
		}
		/// <summary>
		/// Gets value from Dictionary.  If value not found returns default value. 
		/// </summary>
		/// <param name="key">Key of dictionary entry to look up.</param>
		/// <param name="interfaceCode">Code that uniquely identifies which Service Desk system 
		/// is connected to this interface (eg National Help Desk, NZ Post).  For dictionary 
		/// entries that differ from one system to another (eg email alert recipients).</param>
		/// <param name="defaultValue">Default value to return if dictionary entry is not 
		/// found.</param>
		/// <param name="logSource">Trace Source, used for logging.</param>
		/// <returns>Value read from dictionary or, if that value is not found, the default 
		/// value.</returns>
		public int GetValueWithDefault(string key, string interfaceCode, int defaultValue,
			TraceSource logSource)
		{
			decimal dblDefaultValue = Convert.ToDecimal(defaultValue);
			decimal workingValue = GetValueWithDefault(key, interfaceCode, dblDefaultValue,
				logSource);
			return Convert.ToInt32(workingValue);
		}

		/// <summary>
		/// Gets value from Dictionary.  If value not found returns default value. 
		/// </summary>
		/// <param name="key">Key of dictionary entry to look up.</param>
		/// <param name="defaultValue">Default value to return if dictionary entry is not 
		/// found.</param>
		/// <param name="logSource">Trace Source, used for logging.</param>
		/// <returns>Value read from dictionary or, if that value is not found, the default 
		/// value.</returns>
		public decimal GetValueWithDefault(string key, decimal defaultValue,
			TraceSource logSource)
		{
			return GetValueWithDefault(key, null, defaultValue, logSource);
		}

		/// <summary>
		/// Gets value from Dictionary.  If value not found returns default value. 
		/// </summary>
		/// <param name="key">Key of dictionary entry to look up.</param>
		/// <param name="interfaceCode">Code that uniquely identifies which Service Desk system 
		/// is connected to this interface (eg National Help Desk, NZ Post).  For dictionary 
		/// entries that differ from one system to another (eg email alert recipients).</param>
		/// <param name="defaultValue">Default value to return if dictionary entry is not 
		/// found.</param>
		/// <param name="logSource">Trace Source, used for logging.</param>
		/// <returns>Value read from dictionary or, if that value is not found, the default 
		/// value.</returns>
		public decimal GetValueWithDefault(string key, string interfaceCode,
			decimal defaultValue, TraceSource logSource)
		{
			decimal returnValue = 0M;
			decimal workingValue = 0M;
			bool isNull = false;
			UnitOfMeasure uom = new UnitOfMeasure();
			ErrorInfo<TErrorCodeEnum> errorInfo = new ErrorInfo<TErrorCodeEnum>(_commonDatabaseErrorCode.GeneralError,
				string.Empty, _commonDatabaseErrorCode.SuccessValue);

			try
			{
				workingValue = GetDictionaryValue<decimal>(key, interfaceCode, _databaseManager,
					out isNull, out uom, out errorInfo);
				if (errorInfo.ErrorCode.Equals(_commonDatabaseErrorCode.Success))
				{
					// Convert time intervals to seconds.
					switch (uom)
					{
						case UnitOfMeasure.Hours:
							workingValue *= 3600M;
							break;
						case UnitOfMeasure.Minutes:
							workingValue *= 60M;
							break;
						default:
							// Do nothing.
							break;
					}
				}
			}
			catch (Exception ex)
			{
				workingValue = 0M;
				BBLogging.LogException(logSource, 100100, ex);
			}

			if (isNull || (int)workingValue == DictionaryLiteral.InitialValue)
			{
				returnValue = defaultValue;
				logSource.TraceEvent(TraceEventType.Verbose, 100105,
					"Value of dictionary entry '{0}': Not found or NULL. Using default value: {1}.",
					key, returnValue);
			}
			else
			{
				returnValue = workingValue;
				logSource.TraceEvent(TraceEventType.Verbose, 100110,
					"Value of dictionary entry '{0}': {1}.",
					key, returnValue);
			}

			return returnValue;
		}

		/// <summary>
		/// Gets the value of an entry in the default Dictionary.
		/// </summary>
		/// <typeparam name="T">The data type of the value retrieved from the dictionary.</typeparam>
		/// <param name="key">Key of entry to look up.</param>
		/// <param name="oUnitOfMeasure">Unit of measure of dictionary entry (eg min, sec).</param>
		/// <param name="oErrorInfo">Error information indicating whether the value was 
		/// successfully retrieved from the database or not.</param>
		/// <returns>Value of dictionary entry.</returns>
		public T GetDictionaryValue<T>(string key,
			out UnitOfMeasure oUnitOfMeasure, out ErrorInfo<TErrorCodeEnum> oErrorInfo)
		{
			bool isNull = false;
			T returnValue = GetDictionaryValue<T>(key, null, _databaseManager,
				out isNull, out oUnitOfMeasure, out oErrorInfo);
			return returnValue;
		}

		/// <summary>
		/// Gets the value of an entry in the Dictionary.
		/// </summary>
		/// <typeparam name="T">The data type of the value retrieved from the dictionary.</typeparam>
		/// <param name="key">Key of entry to look up.</param>
		/// <param name="interfaceCode">Code that uniquely identifies which Service Desk system 
		/// is connected to this interface (eg National Help Desk, NZ Post).  For dictionary 
		/// entries that differ from one system to another (eg email alert recipients).</param>
		/// <param name="oUnitOfMeasure">Unit of measure of dictionary entry (eg min, sec).</param>
		/// <param name="oErrorInfo">Error information indicating whether the value was 
		/// successfully retrieved from the database or not.</param>
		/// <returns>Value of dictionary entry.</returns>
		public T GetDictionaryValue<T>(string key, string interfaceCode,
			out UnitOfMeasure oUnitOfMeasure, out ErrorInfo<TErrorCodeEnum> oErrorInfo)
		{
			bool isNull = false;
			T returnValue = GetDictionaryValue<T>(key, interfaceCode, _databaseManager,
				out isNull, out oUnitOfMeasure, out oErrorInfo);
			return returnValue;
		}

		/// <summary>
		/// Sets the value of an entry in the default Dictionary.
		/// </summary>
		/// <typeparam name="T">The data type of the value to set.</typeparam>
		/// <param name="key">Key of entry to set.</param>
		/// <param name="value">The value to set the dictionary entry to.  For value types, this 
		/// is the nullable version of the value type.</param>
		/// <typeparam name="T">The type of the dictionary entry.</typeparam>
		/// <returns>true if value of dictionary entry is successfully set, otherwise false.</returns>
		public bool SetDictionaryValue<T>(string key, T value)
		{
			ErrorInfo<TErrorCodeEnum> errorInfo = SetDictionaryValue<T>(key, null, value, 
				_databaseManager);
			return (errorInfo.ErrorCode.Equals(_commonDatabaseErrorCode.Success));
		}

		/// <summary>
		/// Sets the value of an entry in the default Dictionary and returns any error information.
		/// </summary>
		/// <typeparam name="T">The data type of the value to set.</typeparam>
		/// <param name="key">Key of entry to set.</param>
		/// <param name="value">The value to set the dictionary entry to.  For value types, this 
		/// is the nullable version of the value type.</param>
		/// <typeparam name="T">The type of the dictionary entry.</typeparam>
		/// <returns>true if value of dictionary entry is successfully set, otherwise false.</returns>
		public bool SetDictionaryValue<T>(string key, T value, 
			out ErrorInfo<TErrorCodeEnum> oErrorInfo)
		{
			ErrorInfo<TErrorCodeEnum> errorInfo = SetDictionaryValue<T>(key, null, value, 
				_databaseManager);
			oErrorInfo = errorInfo;
			return (errorInfo.ErrorCode.Equals(_commonDatabaseErrorCode.Success));
		}

		/// <summary>
		/// Sets the value of an entry in the Dictionary.
		/// </summary>
		/// <typeparam name="T">The data type of the value to set.</typeparam>
		/// <param name="key">Key of entry to set.</param>
		/// <param name="interfaceCode">Code that uniquely identifies which Service Desk system 
		/// is connected to this interface (eg National Help Desk, NZ Post).  For dictionary 
		/// entries that differ from one system to another (eg email alert recipients).  If set to 
		/// null the value will be saved in the default dictionary.</param>
		/// <param name="value">The value to set the dictionary entry to.  For value types, this 
		/// is the nullable version of the value type.</param>
		/// <param name="oErrorInfo">Error information indicating whether the value was 
		/// successfully retrieved from the database or not.</param>
		/// <typeparam name="T">The type of the dictionary entry.</typeparam>
		/// <returns>true if value of dictionary entry is successfully set, otherwise false.</returns>
		public bool SetDictionaryValue<T>(string key, string interfaceCode, T value)
		{
			ErrorInfo<TErrorCodeEnum> errorInfo = SetDictionaryValue<T>(key, interfaceCode, value,
				_databaseManager);
			return (errorInfo.ErrorCode.Equals(_commonDatabaseErrorCode.Success));
		}

		/// <summary>
		/// Sets the value of an entry in the Dictionary and returns any error information.
		/// </summary>
		/// <typeparam name="T">The data type of the value to set.</typeparam>
		/// <param name="key">Key of entry to set.</param>
		/// <param name="interfaceCode">Code that uniquely identifies which Service Desk system 
		/// is connected to this interface (eg National Help Desk, NZ Post).  For dictionary 
		/// entries that differ from one system to another (eg email alert recipients).  If set to 
		/// null the value will be saved in the default dictionary.</param>
		/// <param name="value">The value to set the dictionary entry to.  For value types, this 
		/// is the nullable version of the value type.</param>
		/// <param name="oErrorInfo">Error information indicating whether the value was 
		/// successfully retrieved from the database or not.</param>
		/// <typeparam name="T">The type of the dictionary entry.</typeparam>
		/// <returns>true if value of dictionary entry is successfully set, otherwise false.</returns>
		public bool SetDictionaryValue<T>(string key, string interfaceCode, T value,
			out ErrorInfo<TErrorCodeEnum> oErrorInfo)
		{
			ErrorInfo<TErrorCodeEnum> errorInfo = SetDictionaryValue<T>(key, interfaceCode, value,
				_databaseManager);
			oErrorInfo = errorInfo;
			return (errorInfo.ErrorCode.Equals(_commonDatabaseErrorCode.Success));
		}

		#endregion

		#region Private and Protected Methods *****************************************************

		/// <summary>
		/// Gets the value of an entry in the Dictionary.
		/// </summary>
		/// <typeparam name="T">The data type of the value retrieved from the dictionary.</typeparam>
		/// <param name="key">Key of entry to look up.</param>
		/// <param name="interfaceCode">Code that uniquely identifies which Service Desk system 
		/// is connected to this interface (eg National Help Desk, NZ Post).  For dictionary 
		/// entries that differ from one system to another (eg email alert recipients).</param>
		/// <param name="databaseManager">DatabaseManager that handles the connection with a  
		/// database containing a dictionary table.</param>
		/// <param name="oUnitOfMeasure">Unit of measure of the dictionary entry (eg min, sec).</param>
		/// <param name="oErrorInfo">Error information indicating whether the value was 
		/// successfully retrieved from the dictionary or not.</param>
		/// <returns>Value of dictionary entry.  For value types, this is the nullable version of 
		/// the value type.</returns>
		private T GetDictionaryValue<T>(string key, string interfaceCode,
			DatabaseManager2 databaseManager, out bool oIsNull,
			out UnitOfMeasure oUnitOfMeasure, out ErrorInfo<TErrorCodeEnum> oErrorInfo)
		{
			object objReturned = null;
			UnitOfMeasure uom = new UnitOfMeasure();
			ErrorInfo<TErrorCodeEnum> errorInfo = new ErrorInfo<TErrorCodeEnum>(_commonDatabaseErrorCode.GeneralError, 
				string.Empty, _commonDatabaseErrorCode.SuccessValue);
			try
			{
				SqlParameter prmKey = new SqlParameter("@Key", key);
				SqlParameter prmInterface = new SqlParameter("@InterfaceCode", interfaceCode);
				SqlParameter oprmUoM = new SqlParameter("@oUnitOfMeasure",
					SqlDbType.NVarChar, 10);
				oprmUoM.Direction = ParameterDirection.Output;
				SqlParameter oprmTextValue = new SqlParameter("@oTextValue",
					SqlDbType.NVarChar, 256);
				oprmTextValue.Direction = ParameterDirection.Output;
				SqlParameter oprmNumericValue = new SqlParameter("@oNumericValue",
					SqlDbType.Decimal);
				oprmNumericValue.Direction = ParameterDirection.Output;
				SqlParameter oprmDateValue = new SqlParameter("@oDateTimeValue",
					SqlDbType.DateTime);
				oprmDateValue.Direction = ParameterDirection.Output;
				// Must set size of binary value output parameter else .NET sets it to 0 and get 
				//	an exception.  Set to arbitrary large value.
				SqlParameter oprmBinaryValue = new SqlParameter("@oBinaryValue",
					SqlDbType.VarBinary, 1000000);
				oprmBinaryValue.Direction = ParameterDirection.Output;
				SqlParameter[] prms = { 
										prmKey, 
										prmInterface, 
										oprmUoM, 
										oprmTextValue, 
										oprmNumericValue, 
										oprmDateValue, 
										oprmBinaryValue 
										};
				databaseManager.ExecStoredProc("p_GetDictionaryEntry", prms, 
					out errorInfo.DatabaseErrorValue, out errorInfo.Message);

				// Check for unrecognised error code from the database.
				errorInfo.ErrorCode 
					= BBError.ValidateDBErrorValue<TErrorCodeEnum>(errorInfo.DatabaseErrorValue,
					_commonDatabaseErrorCode.GeneralDBError);

				// TODO: Modify to allow this method to return binary values from the database.
				if (errorInfo.ErrorCode.Equals(_commonDatabaseErrorCode.Success))
				{
					if (oprmUoM.Value != DBNull.Value)
					{
						uom = ConvertUnitsOfMeasure((string)oprmUoM.Value);
					}

					if (typeof(T) == typeof(string))
					{
						objReturned = oprmTextValue.Value;
					}
					else if (typeof(T) == typeof(decimal))
					{
						objReturned = oprmNumericValue.Value;
					}
					else if (typeof(T) == typeof(DateTime))
					{
						objReturned = oprmDateValue.Value;
					}
				}
			}
			catch (Exception)
			{
				objReturned = null;
				errorInfo.ErrorCode = _commonDatabaseErrorCode.GeneralError;
			}

			oUnitOfMeasure = uom;
			oErrorInfo = errorInfo;

			if (objReturned == DBNull.Value || objReturned == null)
			{
				oIsNull = true;
				return default(T);
			}

			oIsNull = false;
			return (T)objReturned;
		}

		/// <summary>
		/// Sets the value of an entry in the Dictionary.
		/// </summary>
		/// <typeparam name="T">The data type of the value to set.</typeparam>
		/// <param name="key">Key of entry to set.</param>
		/// <param name="interfaceCode">Code that uniquely identifies which Service Desk system 
		/// is connected to this interface (eg National Help Desk, NZ Post).  For dictionary 
		/// entries that differ from one system to another (eg email alert recipients).  If set to 
		/// null the value will be saved in the default dictionary.</param>
		/// <param name="value">The value to set the dictionary entry to.  For value types, this 
		/// is the nullable version of the value type.</param>
		/// <param name="databaseManager">DatabaseManager that handles the connection with a  
		/// database containing a dictionary table.</param>
		/// <typeparam name="T">The type of the dictionary entry.</typeparam>
		/// <returns>ErrorInfo object indicating whether the value of the dictionary entry was 
		/// successfully set or not.</returns>
		private ErrorInfo<TErrorCodeEnum> SetDictionaryValue<T>(string key, string interfaceCode,
			T value, DatabaseManager2 databaseManager)
		{
			ErrorInfo<TErrorCodeEnum> errorInfo = new ErrorInfo<TErrorCodeEnum>(_commonDatabaseErrorCode.GeneralError, 
				string.Empty, _commonDatabaseErrorCode.SuccessValue);
			try
			{
				string parameterName = null;
				if (typeof(T) == typeof(int) || typeof(T) == typeof(decimal))
				{
					parameterName = "@NumericValue";
				}
				else if (typeof(T) == typeof(DateTime))
				{
					parameterName = "@DateTimeValue";
				}
				else if (typeof(T) == typeof(byte[]))
				{
					parameterName = "@BinaryValue";
				}
				else
				{
					parameterName = "@TextValue";
				}
				SqlParameter prmKey = new SqlParameter("@Key", key);
				SqlParameter prmInterface = new SqlParameter("@InterfaceCode", interfaceCode);
				SqlParameter prmValue = new SqlParameter(parameterName, value);
				SqlParameter[] prms = { 
										prmKey, 
										prmInterface, 
										prmValue 
										};
				databaseManager.ExecStoredProc("p_SetDictionaryEntry", prms,
					out errorInfo.DatabaseErrorValue, out errorInfo.Message);

				// Check for unrecognised error code from the database.
				errorInfo.ErrorCode = BBError.ValidateDBErrorValue(errorInfo.DatabaseErrorValue,
					_commonDatabaseErrorCode.GeneralDBError);
			}
			catch (Exception ex)
			{
				errorInfo.ErrorCode = _commonDatabaseErrorCode.GeneralError;
				errorInfo.Message = ex.Message;
			}

			return errorInfo;
		}

		/// <summary>
		/// Convert the units of measure text stored in the dictionary to a UnitsOfMeasure enum 
		/// value.
		/// </summary>
		/// <param name="uomText">Units of measure text to convert.</param>
		/// <returns>UnitsOfMeasure enum value.</returns>
		private UnitOfMeasure ConvertUnitsOfMeasure(string uomText)
		{
			UnitOfMeasure uom = UnitOfMeasure.Unknown;

			try
			{
				uom = (UnitOfMeasure)Enum.Parse(typeof(UnitOfMeasure), uomText, true);
			}
			catch
			{
				uomText = uomText.ToLower();
				switch (uomText)
				{
					case "s":
					case "sec":
					case "secs":
					case "second":
						uom = UnitOfMeasure.Seconds;
						break;
					case "m":
					case "mi":
					case "min":
					case "mins":
					case "minute":
						uom = UnitOfMeasure.Minutes;
						break;
					case "h":
					case "hr":
					case "hrs":
					case "hour":
						uom = UnitOfMeasure.Hours;
						break;
					case "d":
					case "day":
						uom = UnitOfMeasure.Days;
						break;
					case "w":
					case "wk":
					case "week":
						uom = UnitOfMeasure.Weeks;
						break;
					case "mon":
					case "mons":
					case "mth":
					case "mths":
					case "month":
						uom = UnitOfMeasure.Months;
						break;
					case "y":
					case "yr":
					case "yrs":
					case "year":
						uom = UnitOfMeasure.Years;
						break;
					case "ea":
						uom = UnitOfMeasure.Each;
						break;
					case "unit":
						uom = UnitOfMeasure.Units;
						break;
					case "txt":
						uom = UnitOfMeasure.Text;
						break;
					case "f":
					case "flg":
						uom = UnitOfMeasure.Flag;
						break;
					case "dt":
					case "date-time":
						uom = UnitOfMeasure.DateTime;
						break;
					case "dat":
						uom = UnitOfMeasure.Date;
						break;
					case "tm":
						uom = UnitOfMeasure.Time;
						break;
				}
			}

			return uom;
		}

		#endregion
	}

	#endregion
}
