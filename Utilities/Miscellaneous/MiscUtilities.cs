///////////////////////////////////////////////////////////////////////////////////////////////////
// Project      -   Utilities
// General      -   Set of generic classes that may be useful in any project.
//
// File Name    -   MiscUtilities.cs
// File Title   -   Miscellaneous Utilities
// Description  -   Utility methods that may be useful in different projects.
// Notes        -   
//
// $History: MiscUtilities.cs $
// 
// *****************  Version 12  *****************
// User: Simonfi      Date: 20/03/12   Time: 11:30a
// Updated in $/UtilitiesClassLibrary/Utilities
// few bug fixes for management console. make sure correct validation is
// done for transport forms before saving.
// 
// *****************  Version 10  *****************
// User: Simone       Date: 3/04/09    Time: 8:04p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities
// Added an overload of the JoinStringList method that includes a
// predicate parameter for filtering the list.
// 
// *****************  Version 9  *****************
// User: Simone       Date: 27/03/09   Time: 10:47a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities
// MINOR: Moved statements defining lock objects next to the methods they
// lock.
// 
// *****************  Version 8  *****************
// User: Simone       Date: 11/03/09   Time: 2:45p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities
// Make static methods thread safe.
// 
// *****************  Version 7  *****************
// User: Simone       Date: 21/02/09   Time: 1:11a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities
// JoinStringList method added. 
// 
// *****************  Version 6  *****************
// User: Simone       Date: 14/01/09   Time: 10:59a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities
// Added ReplaceLast method.
// 
// *****************  Version 5  *****************
// User: Simone       Date: 18/07/08   Time: 3:19p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities
// ReadDataTableCell: Code to deal with boolean values moved to new method
// ConvertBoolObject.  ConvertBoolObject now called by both overloads of
// ReadDataTableCell.
// 
// *****************  Version 4  *****************
// User: Simone       Date: 18/07/08   Time: 3:00p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities
// ReadDataTableCell: Modified to deal with boolean values where database
// value is 1, 0, "true", "false", "yes", etc.
// 
// *****************  Version 3  *****************
// User: Simone       Date: 11/07/08   Time: 6:00p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities
// Added IsNullOrBlank method.
// 
// *****************  Version 1  *****************
// User: Simone       Date: 13/02/08   Time: 3:58p
// Created in $/UtilitiesClassLibrary/UtilitiesClassLibrary/Utilities
// 
// *****************  Version 1  *****************
// User: Simone       Date: 24/10/07   Time: 12:07p
// Created in $/UtilitiesClassLibrary/Utilities
// Copied from version 4 in
// $/ServiceAlliance/Interfaces/WebServiceInterfaces/JobsWebServices/Utili
// ties.
///////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Utilities.Miscellaneous
{
	/// <summary>
	/// Utility methods that may be useful in different projects.
	/// </summary>
	public class MiscUtilities
	{
        public const int KeyCharPaste = 0x16;

		#region Data Members **********************************************************************


		#endregion

        public static string GetCharOrPastedText(char ch)
        {
            return ch == (char)KeyCharPaste ? Clipboard.GetText() : ch.ToString();
        }

		/// <summary>
		/// Checks if the specified string is an integer or not.
		/// </summary>
		/// <param name="test">String to test.</param>
		/// <returns>True if the string is a positive integer.</returns>
		/// <remarks>For .NET 1.1 (2.0 has TryParse().</remarks>
		public static bool IsInteger(string test)
		{
			return IsNumeric(test, true);
		}

		/// <summary>
		/// Checks if the specified string is numeric or not.  
		/// </summary>
		/// <param name="test">String to test.</param>
		/// <returns>True if the string is a positive number.</returns>
		/// <remarks>ASSUMPTION: Decimal separator is '.'.  For .NET 1.1 (2.0 has TryParse().</remarks>
		public static bool IsNumeric(string test)
		{
			return IsNumeric(test, false);
		}

		/// <summary>
		/// Checks if the specified string is numeric or not.  Optionally can test whether string is an 
		/// integer.
		/// </summary>
		/// <param name="test">String to test.</param>
		/// <param name="isInt">Whether the string should be an integer.  If false will allow decimal 
		/// point as well as digits in the string.</param>
		/// <returns>True if the string is a positive number.</returns>
		/// <remarks>ASSUMPTION: Decimal separator is '.'.  For .NET 1.1 (2.0 has TryParse().</remarks>
		private static object _lockIsNumeric = new object();
		public static bool IsNumeric(string test, bool isInt)
		{
			lock (_lockIsNumeric)
			{
				bool returnVal = true;
				for (int i = 0; i < test.Length; i++)
				{
					if (test[i] > '9' || test[i] < '0')
					{
						if (isInt || test[i] != '.')
						{
							returnVal = false;
						}
					}
				}

				return returnVal;
			}
		}

        public static bool IsAlphaNumeric(string test)
        {
            Regex match = new Regex("^[a-zA-Z0-9]*$");
            return match.IsMatch(test);
        }

		/// <summary>
		/// Converts an object to a decimal.  Null objects and objects which represent null values 
		/// in a table are converted to 0.
		/// </summary>
		/// <param name="objToConvert">Object to convert.</param>
		/// <returns>Decimal value.</returns>
		/// <remarks>Only works in .NET 2.0.</remarks>
		private static object _lockToDecimal = new object();
		public static decimal ToDecimal(object objToConvert)
		{
			lock (_lockToDecimal)
			{
				decimal returnVal = 0M;

				//// CHECK THAT OBJECT IMPLEMENTS THE IConvertible INTERFACE.
				//Type objType = objToConvert.GetType();
				//if (objType.GetInterface("IConvertible") != null)
				//{
				//    // Don't need to test for null, Convert handles nulls.
				//    if (objToConvert != DBNull.Value && decimal.TryParse(objToConvert.ToString, returnVal)
				//    {
				//        returnVal = Convert.ToDecimal(objToConvert);
				//    }
				//}

				if (objToConvert != null)
				{
					decimal.TryParse(objToConvert.ToString(), out returnVal);
				}

				return returnVal;
			}
		}

		/// <summary>
		/// Converts an object to an integer.  Null objects and objects which represent null values 
		/// in a table are converted to 0.
		/// </summary>
		/// <param name="objToConvert">Object to convert.</param>
		/// <returns>Integer value.</returns>
		/// <remarks>Only works in .NET 2.0.</remarks>
		private static object _lockToInteger = new object();
		public static int ToInteger(object objToConvert)
		{
			lock (_lockToInteger)
			{
				int returnVal = 0;

				if (objToConvert != null)
				{
					int.TryParse(objToConvert.ToString(), out returnVal);
				}

				return returnVal;
			}
		}

		/// <summary>
		/// Checks a specified string to see if it is null.  If so, returns the default string instead.
		/// </summary>
		/// <param name="input">The string to test.</param>
		/// <param name="dflt">The default string that will be returned if the input string is null.</param>
		/// <returns>The input string or, if that is null, the default string.</returns>
		/// <remarks>If the default string is null an empty string will be returned.</remarks>
		private static object _lockCoalesce = new object();
		public static string Coalesce(string input, string dflt)
		{
			lock (_lockCoalesce)
			{
				if (dflt == null)
				{
					dflt = string.Empty;
				}
				if (input == null)
				{
					input = dflt;
				}
				return input;
			}
		}

		/// <summary>
		/// Similar to built-in string.IsNullOrEmpty method, except that it treats strings made up 
		/// of just blank spaces to be the same as an empty string.
		/// </summary>
		/// <param name="stringToTest">String to test.</param>
		/// <returns>true if the string is null, empty or made up only of blank spaces.  false 
		/// otherwise.</returns>
		public static bool IsNullOrBlank(string stringToTest)
		{
			return (stringToTest == null || stringToTest.Trim().Length == 0);
		}

		/// <summary>
		/// Reads a value from a cell in a data table row and outputs the value.  The value will 
		/// be converted to the specified data type.
		/// </summary>
		/// <typeparam name="T">Data type of value that will be output.</typeparam>
		/// <param name="row">Data table row to be read.</param>
		/// <param name="columnIndex">Zero-based index of cell in row to be read.</param>
		/// <returns>Value in specified cell of data row.</returns>
		private static object _lockReadDataTableCell_1 = new object();
		public static T ReadDataTableCell<T>(DataRow row, int columnIndex)
		{
			lock (_lockReadDataTableCell_1)
			{
				object objCellValue = null;
				T cellValue = default(T);

				if (row != null && columnIndex < row.ItemArray.Length)
				{
					if (row[columnIndex] != DBNull.Value)
					{
						objCellValue = row[columnIndex];
						try
						{
							ConvertBoolObject<T>(ref objCellValue);
							cellValue = (T)objCellValue;
						}
						catch
						{
							// Do nothing - default value already set.
						}
					}
				}
				return cellValue;
			}
		}

		/// <summary>
		/// Reads a value from a cell in a data table row and outputs the value.  The value will 
		/// be converted to the specified data type.
		/// </summary>
		/// <typeparam name="T">Data type of value that will be output.</typeparam>
		/// <param name="row">Data table row to be read.</param>
		/// <param name="columnName">Column name of cell in row to be read.</param>
		/// <returns>Value in specified cell of data row.</returns>
		private static object _lockReadDataTableCell_2 = new object();
		public static T ReadDataTableCell<T>(DataRow row, string columnName)
		{
			lock (_lockReadDataTableCell_2)
			{
				object objCellValue = null;
				T cellValue = default(T);

				if (row != null)
				{
					// Put try-catch at this level to pick up exceptions if an invalid column name 
					//	is used.
					try
					{
						if (row[columnName] != DBNull.Value)
						{
							objCellValue = row[columnName];
							ConvertBoolObject<T>(ref objCellValue);
							cellValue = (T)objCellValue;
						}
					}
					catch
					{
						// Do nothing - default value already set.
					}
				}
				return cellValue;
			}
		}

		/// <summary>
		/// Converts objects containing 1 or 0, or text like "true", "false", "yes", "no", etc, to 
		/// bool value, if the type parameter is bool.
		/// </summary>
		/// <typeparam name="T">Indicates whether the object should be a bool or not.</typeparam>
		/// <param name="rObject">Object to convert, if the type parameter is a bool.</param>
		/// <remarks>Can have a problem casting an object to bool.  Sometimes will not recognise 
		/// 1 and 0 as true and false.  So explicitly convert object to bool.</remarks>
		private static void ConvertBoolObject<T>(ref object rObject)
		{
			lock (_lockToDecimal)
			{
				if (typeof(T) == typeof(bool))
				{
					Type objType = rObject.GetType();

					if (objType == typeof(sbyte) || objType == typeof(byte)
						|| objType == typeof(short) || objType == typeof(ushort)
						|| objType == typeof(int) || objType == typeof(uint)
						|| objType == typeof(long) || objType == typeof(ulong))
					{
						// Cannot cast to long so Convert instead.
						long numericValue = Convert.ToInt64(rObject);
						if (numericValue == 1 || numericValue == -1)
						{
							rObject = true;
						}
						else
						{
							rObject = false;
						}
					}
					else if (objType == typeof(string))
					{
						string stringValue = ((string)rObject).ToLower();
						if (stringValue == "t" || stringValue == "true"
							|| stringValue == "y" || stringValue == "yes")
						{
							rObject = true;
						}
						else
						{
							rObject = false;
						}
					}
				}
			}
		}

		/// <summary>
		/// Checks whether a value is a valid member of an enum.  Value to check must be a string that 
		/// may contain either an integer value or the name of an enum value.
		/// </summary>
		/// <typeparam name="T">Enum type that the value should be a member of.</typeparam>
		/// <param name="valueToValidate">Value to validate.  May contain either an integer value or 
		/// the name of an enum value. eg "0", "1", "Low", "Medium".</param>
		/// <param name="oConvertedValue">If the value is a member of the enum then the converted 
		/// value will be passed to this output parameter.  If the value is not a member of the enum 
		/// then this output parameter will default to the first enum value.</param>
		/// <returns>True if value is a member of the enum, otherwise false.</returns>
		/// <remarks>For text values, performs a case-insensitive comparison between the text 
		/// and the members of the enum.  NOTE: Cannot constrain the type parameter to be an enum.</remarks>
		private static object _lockValidateEnumValue_1 = new object();
		public static bool ValidateEnumValue<T>(string valueToValidate, out T oConvertedValue)
			where T : IComparable, IFormattable, IConvertible
		{
			lock (_lockValidateEnumValue_1)
			{
				bool isValid = false;
				T convertedValue = default(T);
				Type typeT = typeof(T);

				// Check if type parameter is an enum.
				if (typeT.IsEnum)
				{

					// Value to check is an integer.
					int numericValue = 0;
					if (int.TryParse(valueToValidate, out numericValue))
					{
						if (Enum.IsDefined(typeT, numericValue))
						{
							convertedValue = (T)Enum.Parse(typeT, valueToValidate);
							isValid = true;
						}
					}
					// Value to check is text.
					else
					{
						// Cannot use Enum.IsDefined() as that is case sensitive.  Iterating 
						//	through all members of an enum should be ok as enums typically have 
						//	only a few members, rather than dozens or hundreds.
						string[] enumNames = Enum.GetNames(typeT);
						foreach (string enumName in enumNames)
						{
							if (enumName.Equals(valueToValidate,
								StringComparison.CurrentCultureIgnoreCase))
							{
								convertedValue = (T)Enum.Parse(typeT, valueToValidate, true);
								isValid = true;
								break;
							}
						}
					}
				}

				oConvertedValue = convertedValue;
				return isValid;
			}
		}

		/// <summary>
		/// Checks whether an integer is a valid member of an enum.
		/// </summary>
		/// <typeparam name="T">Enum type that the integer should be a member of.</typeparam>
		/// <param name="valueToValidate">Integer to validate.</param>
		/// <param name="oConvertedValue">If the integer is a member of the enum then the converted 
		/// value will be passed to this output parameter.  If the integer is not a member of the 
		/// enum then this output parameter will default to the first enum value.</param>
		/// <returns>True if integer is a member of the enum, otherwise false.</returns>
		/// <remarks>Cannot constrain the type parameter to be an enum.</remarks>
		private static object _lockValidateEnumValue_2 = new object();
		public static bool ValidateEnumValue<T>(int valueToValidate, out T oConvertedValue)
			where T : IComparable, IFormattable, IConvertible
		{
			lock (_lockValidateEnumValue_2)
			{
				bool isValid = false;
				T convertedValue = default(T);
				Type typeT = typeof(T);

				// Check if type parameter is an enum.
				if (typeT.IsEnum)
				{

					if (Enum.IsDefined(typeT, valueToValidate))
					{
						// Cannot cast int as generic type so use Enum.Parse instead.
						string stringToValidate = valueToValidate.ToString();
						convertedValue = (T)Enum.Parse(typeT, stringToValidate, true);
						isValid = true;
					}
				}

				oConvertedValue = convertedValue;
				return isValid;
			}
		}

		/// <summary>
		/// Converts null values to DBNull.Value, for passing to a database.
		/// </summary>
		/// <typeparam name="T">The type of the input value.</typeparam>
		/// <param name="inputValue">The input value.</param>
		/// <returns>The input value if it not null or DBNull.Value if it is null.</returns>
		private static object _lockSetDBNull_1 = new object();
		public static object SetDBNull<T>(T inputValue)
			where T : class
		{
			lock (_lockSetDBNull_1)
			{
				if (inputValue == null)
				{
					return DBNull.Value;
				}
				else
				{
					return inputValue;
				}
			}
		}

		/// <summary>
		/// Converts null values to DBNull.Value, for passing to a database.
		/// </summary>
		/// <param name="inputValue">The input value.</param>
		/// <returns>The input value if it not null or DBNull.Value if it is null.</returns>
		private static object _lockSetDBNull_2 = new object();
		public static object SetDBNull(int? inputValue)
		{
			lock (_lockSetDBNull_2)
			{
				if (inputValue == null)
				{
					return DBNull.Value;
				}
				else
				{
					return inputValue;
				}
			}
		}

		/// <summary>
		/// Replace the last occurrence of a string within another string.
		/// </summary>
		/// <param name="textToOperateOn">The text containing the string that will be replaced.</param>
		/// <param name="oldValue">The string to replace.</param>
		/// <param name="newValue">The replacement string.</param>
		/// <returns>string with only the last occurrence of a sub-string replaced.</returns>
		private static object _lockReplaceLast = new object();
		public static string ReplaceLast(string textToOperateOn, string oldValue, string newValue)
		{
			lock (_lockReplaceLast)
			{
				int oldValueLastIndex = textToOperateOn.LastIndexOf(oldValue);
				if (oldValueLastIndex == -1)
				{
					return textToOperateOn;
				}

				string leftText = textToOperateOn.Substring(0, oldValueLastIndex);
				string rightText = textToOperateOn.Substring(oldValueLastIndex,
					(textToOperateOn.Length - oldValueLastIndex));
				rightText = rightText.Replace(oldValue, newValue);

				return leftText + rightText;
			}
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
		public static string JoinStringList(string separator, List<string> list)
		{
			return JoinStringList(separator, list, null);
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
		private static object _lockJoinStringList = new object();
		public static string JoinStringList(string separator, List<string> list,
			Predicate<string> match)
		{
			lock (_lockJoinStringList)
			{
				if (list == null)
				{
					throw new ArgumentNullException("list");
				}

				List<string> workingList = list;
				if (match != null)
				{
					workingList = list.FindAll(match);
				}

				if (workingList.Count == 0)
				{
					return string.Empty;
				}

				string[] array = workingList.ToArray();
				return string.Join(separator, array);
			}
		}

        /// <summary>
        /// Build a collection of resources contained within the specified assembly and resource directory.
        /// </summary>
        /// <param name="resourceDirectory">Directory in which the resources are located.</param>
        /// <returns>A collection containing all resources in the specified directory</returns>
        public static IDictionary<string, byte[]> GetResources(Assembly assembly, string resourceDirectory)
        {
            IDictionary<string, byte[]> resources = new Dictionary<string, byte[]>();

            string[] resourceNames = assembly.GetManifestResourceNames();

            foreach (string resourceName in resourceNames)
            {
                string actualDirName = string.Format(
                    "{0}.{1}",
                    assembly.GetName().Name,
                    resourceDirectory);

                if (resourceName.StartsWith(actualDirName) == false)
                {
                    continue;
                }

                Stream stream = assembly.GetManifestResourceStream(resourceName);
                byte[] data = new byte[stream.Length];
                stream.Read(data, 0, (int)stream.Length);

                string resourceFileName = resourceName.Replace(actualDirName + ".", string.Empty);
                resources.Add(resourceFileName, data);
            }
            return resources;
        }
	}
}
