using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace Utilities.DisplayHelper
{
	/// <summary>
	/// Helper methods for displaying text and details of objects.
	/// </summary>
	public abstract class DisplayHelper
	{
		#region Nested Classes and Structs ********************************************************

		/// <summary>
		/// Helper structure for displaying objects.
		/// </summary>
		public struct ObjectArguments
		{
			public object Object;
			public int RootIndentLevel;
			public string Title;
			public object[] TitleArgs;
			public string ObjectName;
			public Dictionary<Type, int> RecursionTypeCount;

			public ObjectArguments(object obj, int rootIndentLevel,
				string title, params object[] titleArgs)
				: this(obj, rootIndentLevel, title, null, new Dictionary<Type, int>(), titleArgs)
			{ }

			public ObjectArguments(object obj, int rootIndentLevel,
				string title, string objectName, Dictionary<Type, int> recursionTypeCount,
				params object[] titleArgs)
			{
				Object = obj;
				RootIndentLevel = rootIndentLevel;
				Title = title;
				TitleArgs = titleArgs;
				ObjectName = objectName;
				RecursionTypeCount = recursionTypeCount;
			}
		}

		/// <summary>
		/// Structure that supports DisplayObjectMember.
		/// </summary>
		private struct MemberDetails
		{
			public string Name;
			public MemberTypes MemberType;
			public Type Type;
			public object Value;
			public Dictionary<Type, int> RecursionTypeCount;

			public MemberDetails(string name, MemberTypes memberType, Type type, object value,
				Dictionary<Type, int> recursionTypeCount)
			{
				Name = name;
				MemberType = memberType;
				Type = type;
				Value = value;
				RecursionTypeCount = recursionTypeCount;
			}
		}

		#endregion

		#region Data Members **********************************************************************

		private const string _nullDisplayText = "[NULL]";
		private const string _emptyEnumerableDisplayText = "[NO ITEMS]";
		private const string _maxRecursionDepthText = "[MAX RECURSION DEPTH REACHED]";
		private const int _maxRecursionDepth = 5;
		private const int _tabWidth = 4;

		#endregion

		#region Constructors, Destructors / Finalizers and Dispose Methods ************************

		#endregion

		#region Properties ************************************************************************

		#endregion

		#region Public Methods ********************************************************************

		/// <summary>
		/// Appends the specified text to the last line of text.
		/// </summary>
		/// <param name="text"></param>
		public abstract void DisplayAppendedText(string text, bool addLeadingSpace,
			bool includeNewLine);

		/// <summary>
		/// Displays the specified text indented by the specified number of tabs.  Arguments may 
		/// be inserted into the text, as in string.Format() and Console.WriteLine().
		/// </summary>
		public abstract void DisplayIndentedText(int indentLevel, string text, bool wrapText,
			bool includeNewLine, params object[] args);

		/// <summary>
		/// Displays the specified text indented by the specified number of tabs.  Similar to 
		/// DisplayIndentedText but if the text is of the form "header: text" then the header may 
		/// be formatted differently from the remaining text.
		/// </summary>
		public virtual void DisplayHeadedText(int indentLevel, string text, bool wrapText,
			bool includeNewLine, params object[] args)
		{
			this.DisplayIndentedText(indentLevel, text, wrapText, includeNewLine, args);
		}

		/// <summary>
		/// Displays the specified text as a numbered paragraph, of the form "n) text", where n 
		/// is the paragraph number.
		/// </summary>
		public virtual void DisplayNumberedText(int number, int indentLevel, string text, 
			bool wrapText, params object[] args)
		{

			text = string.Format("{0}) {1}", number, text);
			this.DisplayIndentedText(indentLevel, text, wrapText, true, args);
		}

		/// <summary>
		/// Displays the specified text with a double underline.
		/// </summary>
		public virtual void DisplayTitle(string titleText)
		{
			this.DisplayTitle(titleText, '=');
		}

		/// <summary>
		/// Displays the specified text with a single underline.
		/// </summary>
		public virtual void DisplaySubTitle(string titleText)
		{
			this.DisplayTitle(titleText, '-');
		}

		/// <summary>
		/// Displays the specified text with the specified underline.
		/// </summary>
		public virtual void DisplayTitle(string titleText, char underlineChar)
		{
			int titleLength = titleText.Length;
			bool wrapText = false;
			bool includeNewLine = true;
			this.DisplayIndentedText(0, titleText, wrapText, includeNewLine);
			this.DisplayIndentedText(0, new string(underlineChar, titleLength), 
				wrapText, includeNewLine);
		}

		#endregion

		#region Protected Methods *****************************************************************

		/// <summary>
		/// Displays the details of an object - either a single object or an enumeration of objects.
		/// </summary>
		protected virtual void DisplayObject(object obj, int rootIndentLevel,
			string title, params object[] titleArgs)
		{
			if (!(obj is string) && obj is IEnumerable)
			{
				this.DisplayEnumerableObjects((IEnumerable)obj, rootIndentLevel, title, titleArgs);
				return;
			}
			this.DisplaySingleObject(obj, rootIndentLevel, title, titleArgs);
		}

		/// <summary>
		/// Displays the details of each value or object in an Enumerable collection.
		/// </summary>
		protected virtual void DisplayEnumerableObjects(IEnumerable enumerableObjects,
			int rootIndentLevel, string title, params object[] titleArgs)
		{
			int indentLevel = rootIndentLevel;

			bool listIsNull = (enumerableObjects == null);

			if (title != null && title.Trim().Length > 0)
			{
				// Do not write newline if list is null - will append to line.
				bool includeNewLine = !listIsNull;
				this.DisplayHeadedText(indentLevel, title, false, includeNewLine,
					titleArgs);
				indentLevel++;
			}

			if (listIsNull)
			{
				// Only add leading space if a title was written.
				bool addLeadingSpace = (indentLevel > rootIndentLevel);
				this.DisplayAppendedText(_nullDisplayText, addLeadingSpace, true);
				return;
			}

			bool noObjects = true;
			foreach (object obj in enumerableObjects)
			{
				noObjects = false;
				break;
			}
			if (noObjects)
			{
				this.DisplayAppendedText(_emptyEnumerableDisplayText, true, true);
				return;
			}

			int i = 0;
			foreach (object obj in enumerableObjects)
			{
				string itemTitle = string.Format("{0}[{1}]:", obj.GetType().Name, i);
				if (obj.GetType().IsValueType)
				{
					this.DisplayHeadedText(indentLevel, itemTitle + " " + obj.ToString(),
						false, true);
				}
				else
				{
					this.DisplaySingleObject(obj, indentLevel, "{0}[{1}]:", obj.GetType().Name, i);
				}
				i++;
			}
		}

		/// <summary>
		/// Displays the details of a single object.
		/// </summary>
		protected virtual void DisplaySingleObject(object obj, int rootIndentLevel,
			string title, params object[] titleArgs)
		{
			DisplayHelper.ObjectArguments objectArguments =
				new DisplayHelper.ObjectArguments(obj, rootIndentLevel, title, titleArgs);
			this.DisplaySingleObject(objectArguments);
		}

		/// <summary>
		/// Displays the values in a data table.
		/// </summary>
		protected void DisplayDataTable(DataTable dataTable, bool displayRowState)
		{
			int columnSpacing = 3;	// Spaces between each column.
			bool wrapText = false;
			bool includeNewLine = true;
			if (dataTable == null)
			{
				this.DisplayIndentedText(0, "[DATATABLE IS NULL]", wrapText, includeNewLine);
				return;
			}

			string rowStateHeaderText = "Row State";
			int numberColumns = dataTable.Columns.Count;

			// Work out maximum column widths needed for each column.
			// ASSUMPTION: Monospaced font will be used, so each character or space will 
			//	occupy the same width.
			Dictionary<int, int> columnWidths = new Dictionary<int, int>();
			for (int i = 0; i < numberColumns; i++)
			{
				columnWidths[i] = dataTable.Columns[i].ColumnName.Length;
			}
			if (displayRowState)
			{
				columnWidths[numberColumns] = rowStateHeaderText.Length;
			}
			
			foreach (DataRow row in dataTable.Rows)
			{
				for (int i = 0; i < numberColumns; i++)
				{
					if (row[i].ToString().Length > columnWidths[i])
					{
						columnWidths[i] = row[i].ToString().Length;
					}
				}
				if (displayRowState && row.RowState.ToString().Length > columnWidths[numberColumns])
				{
					columnWidths[numberColumns] = row.RowState.ToString().Length;
				}
			}

			// Display column names and row values.
			StringBuilder sb = new StringBuilder();
			string displayText;
			for (int i = 0; i < numberColumns; i++)
			{
				displayText = 
					dataTable.Columns[i].ColumnName.PadRight(columnWidths[i] + columnSpacing);
				sb.Append(displayText);
			}
			if (displayRowState)
			{
				displayText = 
					rowStateHeaderText.PadRight(columnWidths[numberColumns] + columnSpacing);
				sb.Append(displayText);
			}
			string headerText = sb.ToString();
			this.DisplaySubTitle(headerText);

			if (dataTable.Rows.Count <= 0)
			{
				this.DisplayIndentedText(0, "[NO ROWS IN DATATABLE]", wrapText, includeNewLine);
				return;
			}

			foreach (DataRow row in dataTable.Rows)
			{
				sb = new StringBuilder();
				for (int i = 0; i < numberColumns; i++)
				{
					displayText =
						row[i].ToString().PadRight(columnWidths[i] + columnSpacing);
					sb.Append(displayText);
				}
				if (displayRowState)
				{
					displayText =
						row.RowState.ToString().PadRight(columnWidths[numberColumns] + columnSpacing);
					sb.Append(displayText);
				}
				this.DisplayIndentedText(0, sb.ToString().TrimEnd(), wrapText, includeNewLine);
			}		
		}

		/// <summary>
		/// Displays the details of an exception.
		/// </summary>
		protected void DisplayException(int indentLevel, Exception exception)
		{
			this.DisplayException(indentLevel, exception, false);
		}

		#endregion

		#region Private Methods *******************************************************************

		/// <summary>
		/// Displays the details of a single object.
		/// </summary>
		private void DisplaySingleObject(ObjectArguments objectArgs)
		{
			object obj = objectArgs.Object;
			int rootIndentLevel = objectArgs.RootIndentLevel;
			string title = objectArgs.Title;
			string objectName = objectArgs.ObjectName;
			Dictionary<Type, int> recursionTypeCount = objectArgs.RecursionTypeCount;
			object[] titleArgs = objectArgs.TitleArgs;

			int indentLevel = rootIndentLevel;

			bool objectIsNull = (obj == null);

			if (title != null && title.Trim().Length > 0)
			{
				// Do not write newline if object is null - will append to line.
				bool includeNewLine = !objectIsNull;
				this.DisplayHeadedText(indentLevel, title, false, includeNewLine,
					titleArgs);
				indentLevel++;
			}

			if (objectIsNull)
			{
				// Only add leading space if a title was written.
				bool addLeadingSpace = (indentLevel > rootIndentLevel);
				this.DisplayAppendedText(_nullDisplayText, addLeadingSpace, true);
				return;
			}

			List<MemberDetails> memberDetails = new List<MemberDetails>();
			PropertyInfo[] properties = obj.GetType().GetProperties();
			foreach (PropertyInfo property in properties)
			{
				memberDetails.Add(new MemberDetails(property.Name, property.MemberType,
					property.PropertyType, property.GetValue(obj, null),
					new Dictionary<Type, int>(recursionTypeCount)));
			}

			FieldInfo[] fields = obj.GetType().GetFields();
			foreach (FieldInfo field in fields)
			{
				memberDetails.Add(new MemberDetails(field.Name, field.MemberType,
					field.FieldType, field.GetValue(obj),
					new Dictionary<Type, int>(recursionTypeCount)));
			}

			for (int i = 0; i < memberDetails.Count; i++)
			{
				MemberDetails member = memberDetails[i];
				DisplayObjectMember(member, obj, indentLevel);
			}
		}

		/// <summary>
		/// Displays the details of a property or field of an object.
		/// </summary>
		/// <remarks>Helper method for DisplaySingleObject.</remarks>
		private void DisplayObjectMember(MemberDetails memberDetails, object parentObj,
			int rootIndentLevel)
		{
			int indentLevel = rootIndentLevel;

			string memberName = memberDetails.Name;
			object memberValue = memberDetails.Value;
			Type memberType = memberDetails.Type;
			bool memberValueIsNull = (memberValue == null);
			bool memberIsEnumerable = !(memberValue is string) && memberValue is IEnumerable;
			bool memberIsEmptyEnumerable = false;
			if (!memberValueIsNull && memberIsEnumerable)
			{
				memberIsEmptyEnumerable = true;
				IEnumerable enumerable = memberValue as IEnumerable;
				foreach (object item in enumerable)
				{
					memberIsEmptyEnumerable = false;
					break;
				}
			}
			Dictionary<Type, int> recursionTypeCount = memberDetails.RecursionTypeCount;
			if (recursionTypeCount.ContainsKey(memberType))
			{
				recursionTypeCount[memberType]++;
			}
			else
			{
				recursionTypeCount[memberType] = 1;
			}
			bool maxRecursionDepthReached = (recursionTypeCount[memberType] >= _maxRecursionDepth);

			string displayValue = (memberValue == null) ? _nullDisplayText : memberValue.ToString();
			string displayMemberType =
				(memberDetails.MemberType == MemberTypes.Field) ? " (field)" : string.Empty;

			if (memberType.IsValueType || memberType == typeof(string))
			{
				this.DisplayHeadedText(indentLevel, "{0}{1}: {2}", false, true,
					memberName, displayMemberType, displayValue);
				return;
			}

			// Do not write newline if member value is null, member is an empty collection or 
			//	maximum recursion depth has been reached - will append to line.
			bool includeNewLine = (!memberValueIsNull && !memberIsEmptyEnumerable && !maxRecursionDepthReached);
			string displayFormat;
			if (displayMemberType.ToLower().Contains("field"))
			{
				displayFormat = "{0} (field, type: {1}):";
			}
			else
			{
				displayFormat = "{0} (type: {1}):";
			}
			this.DisplayHeadedText(indentLevel, displayFormat, false, includeNewLine,
				memberName, memberType.FullName);
			indentLevel++;

			if (maxRecursionDepthReached)
			{
				this.DisplayAppendedText(_maxRecursionDepthText, true, true);
				return;
			}

			if (memberValueIsNull)
			{
				this.DisplayAppendedText(_nullDisplayText, true, true);
				return;
			}

			if (memberIsEmptyEnumerable)
			{
				this.DisplayAppendedText(_emptyEnumerableDisplayText, true, true);
				return;
			}

			if (memberIsEnumerable)
			{
				int i = 0;
				IEnumerable enumerable = memberValue as IEnumerable;
				foreach (object item in enumerable)
				{
					if (item.GetType().IsValueType || item is string)
					{
						this.DisplayHeadedText(indentLevel, "{0}[{1}]: {2}", false, true,
							memberName, i, item.ToString());
					}
					else
					{
						this.DisplayHeadedText(indentLevel, "{0}[{1}] (type: {2}):", false, true,
							memberName, i, item.GetType().FullName);

						ObjectArguments itemObjectArguments =
							new ObjectArguments(item, indentLevel + 1, null);
						DisplaySingleObject(itemObjectArguments);
					}
					i++;
				}

				return;
			}

			ObjectArguments objectArguments =
				new ObjectArguments(memberValue, indentLevel, null, memberName, recursionTypeCount);
			DisplaySingleObject(objectArguments);
		}

		/// <summary>
		/// Displays the details of an exception.
		/// </summary>
		private void DisplayException(int indentLevel, Exception exception, bool isInnerException)
		{
			string textToDisplay = string.Empty;
			if (isInnerException)
			{
				textToDisplay += "Inner Exception - ";
			}
			textToDisplay += string.Format("{0}: {1}", exception.GetType().Name, exception.Message);

			this.DisplayHeadedText(indentLevel, textToDisplay, true, true);

			if (exception.InnerException != null)
			{
				this.DisplayException((indentLevel + 1), exception.InnerException, true);
			}
		}

		#endregion
	}
}
