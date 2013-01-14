using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Utilities.DisplayHelper
{
	/// <summary>
	/// Helper methods for displaying text and details of objects via the console.
	/// </summary>
	public class ConsoleDisplayHelper : DisplayHelper
	{
		#region Data Members **********************************************************************

		private const int _tabWidth = 4;

		#endregion

		#region Constructors, Destructors / Finalizers and Dispose Methods ************************

		#endregion

		#region Properties ************************************************************************

		#endregion

		#region Static Methods ********************************************************************

		/// <summary>
		/// Displays the details of an object - either a single object or an enumeration of objects.
		/// </summary>
		public static void ShowObject(object obj, int rootIndentLevel,
			string title, params object[] titleArgs)
		{
			ConsoleDisplayHelper objectViewer = new ConsoleDisplayHelper();
			objectViewer.DisplayObject(obj, rootIndentLevel, title, titleArgs);
		}

		/// <summary>
		/// Displays the values in a data table.
		/// </summary>
		public static void ShowDataTable(DataTable dataTable, bool displayRowState)
		{
			ConsoleDisplayHelper objectViewer = new ConsoleDisplayHelper();
			objectViewer.DisplayDataTable(dataTable, displayRowState);
		}

		/// <summary>
		/// Displays the details of an exception.
		/// </summary>
		public static void ShowException(int indentLevel, Exception exception)
		{
			ConsoleDisplayHelper objectViewer = new ConsoleDisplayHelper();
			objectViewer.DisplayException(indentLevel, exception);
		}
		
		/// <summary>
		/// Appends the specified text to the last line of text.
		/// </summary>
		/// <param name="text"></param>
		public static void ShowAppendedText(string text, bool addLeadingSpace,
			bool includeNewLine)
		{
			ConsoleDisplayHelper objectViewer = new ConsoleDisplayHelper();
			objectViewer.DisplayAppendedText(text, addLeadingSpace, includeNewLine);
		}

		/// <summary>
		/// Displays the specified text indented by the specified number of tabs.  Arguments may 
		/// be inserted into the text, as in string.Format() and Console.WriteLine().
		/// </summary>
		public static void ShowIndentedText(int indentLevel, string text, bool wrapText,
			bool includeNewLine, params object[] args)
		{
			ConsoleDisplayHelper objectViewer = new ConsoleDisplayHelper();
			objectViewer.DisplayIndentedText(indentLevel, text, wrapText, includeNewLine, args);
		}

		/// <summary>
		/// Displays the specified text indented by the specified number of tabs.  Similar to 
		/// DisplayIndentedText but if the text is of the form "header: text" then the header may 
		/// be formatted differently from the remaining text.
		/// </summary>
		public static void ShowHeadedText(int indentLevel, string text, bool wrapText,
			bool includeNewLine, params object[] args)
		{
			ConsoleDisplayHelper objectViewer = new ConsoleDisplayHelper();
			objectViewer.DisplayHeadedText(indentLevel, text, wrapText, includeNewLine, args);
		}

		/// <summary>
		/// Displays the specified text as a numbered paragraph, of the form "n) text", where n 
		/// is the paragraph number.
		/// </summary>
		public static void ShowNumberedText(int number, int indentLevel, string text,
			bool wrapText, params object[] args)
		{
			ConsoleDisplayHelper objectViewer = new ConsoleDisplayHelper();
			objectViewer.DisplayNumberedText(number, indentLevel, text, wrapText, args);
		}

		/// <summary>
		/// Displays the specified text with a double underline.
		/// </summary>
		public static void ShowTitle(string titleText)
		{
			ConsoleDisplayHelper objectViewer = new ConsoleDisplayHelper();
			objectViewer.DisplayTitle(titleText);
		}
		
		/// <summary>
		/// Displays the specified text with a single underline.
		/// </summary>
		public static void ShowSubTitle(string titleText)
		{
			ConsoleDisplayHelper objectViewer = new ConsoleDisplayHelper();
			objectViewer.DisplaySubTitle(titleText);
		}

		#endregion

		#region Public Methods ********************************************************************
		
		/// <summary>
		/// Displays the specified text indented by the specified number of tabs.  Arguments may be 
		/// inserted into the text, as in string.Format() and Console.WriteLine().
		/// </summary>
		public override void DisplayIndentedText(int indentLevel, string text, bool wrapText,
			bool includeNewLine, params object[] args)
		{
			// TODO: Implement text wrapping.  Console.BufferWidth gives the number of characters 
			//  in a line.  Could not find a property to give the tab width, hence created 
			//	home-made tabs.
			if (args != null && args.Length > 0)
			{
				text = string.Format(text, args);
			}
			int indentWidth = _tabWidth * indentLevel;
			string indentedText = new string(' ', indentWidth) + text;
			if (wrapText)
			{
				indentedText = this.WrapText(indentLevel, indentedText, (Console.BufferWidth - 1));
			}
			if (includeNewLine)
			{
				Console.WriteLine(indentedText);
			}
			else
			{
				Console.Write(indentedText);
			}
		}

		/// <summary>
		/// Displays the specified text indented by the specified number of tabs.  Similar to 
		/// DisplayIndentedText but if the text is of the form "header: text" then the header may 
		/// be formatted differently from the remaining text.
		/// </summary>
		public override void DisplayHeadedText(int indentLevel, string text, bool wrapText,
			bool includeNewLine, params object[] args)
		{
			this.DisplayIndentedText(indentLevel, text, wrapText, includeNewLine, args);
		}

		/// <summary>
		/// Appends the specified text to the last line of text.
		/// </summary>
		/// <param name="text"></param>
		public override void DisplayAppendedText(string text, bool addLeadingSpace, 
			bool includeNewLine)
		{
			if (addLeadingSpace)
			{
				text = " " + text;
			}

			if (includeNewLine)
			{
				Console.WriteLine(text);
			}
			else
			{
				Console.Write(text);
			}
		}
				
		#endregion

		#region Private and Protected Methods *****************************************************

		/// <summary>
		/// Wraps text at the last space on each line.  Second and subsequent lines are indented 
		/// one level more than the first line.
		/// </summary>
		private string WrapText(int indentLevel, string text, int numberCharsPerLine)
		{
			string wrappedText = text;
			// Indent 1 level more than first line.
			int indentWidth = _tabWidth * (indentLevel + 1);
			string indent = new string(' ', indentWidth);
			string origText = text;
			StringBuilder resultantText = new StringBuilder();
			int charPosition = 0;
			while (origText.Length > 0)
			{
				if (origText.Length < numberCharsPerLine)
				{
					resultantText.AppendLine(origText);
					origText = string.Empty;
				}
				else
				{
					charPosition = origText.Substring(0, numberCharsPerLine).LastIndexOf(' ') + 1;
					if (charPosition == 0)
					{
						charPosition = numberCharsPerLine;
					}

					resultantText.AppendLine(origText.Substring(0, charPosition));
					origText = indent + origText.Substring(charPosition);
				}
			}
			wrappedText = resultantText.ToString();
			if (wrappedText.EndsWith(Environment.NewLine))
			{
				wrappedText = wrappedText.Substring(0,
					wrappedText.LastIndexOf(Environment.NewLine));
			}
			return wrappedText;
		}

		#endregion
	}
}
