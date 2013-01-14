///////////////////////////////////////////////////////////////////////////////////////////////////
// Project      -   
// General      -   
//
// File Name    -   MenuHelper.cs
// Description  -   Utility methods useful when running methods from a menu.
//
// Notes        -   
//
// $History: $
///////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Threading;

namespace MenuLibrary
{
	/// <summary>
	/// Utility methods useful when running methods from a menu.
	/// </summary>
	public static class MenuHelper
	{
		#region Data members **********************************************************************

		private const int _tabWidth = 4;

		#endregion

		#region Public Methods ********************************************************************

		/// <summary>
		/// Displays a message asking user to wait for results of a method.
		/// </summary>
		public static void ResultsPauseMessage(bool isAsync)
		{
			string text = string.Empty;
			if (isAsync)
			{
				text = "This method returns its results asynchronously.  ";
			}
			text += "Please wait for the results...";

			string lineOfChars = new string('*', text.Length);

			Console.WriteLine(lineOfChars);
			Console.WriteLine(text);
			Console.WriteLine(lineOfChars);
			Console.WriteLine();
		}

		/// <summary>
		/// Displays the specified text with a double underline.
		/// </summary>
		public static void ShowTitle(string titleText)
		{
			ShowTitle(titleText, '=');
		}

		/// <summary>
		/// Displays the specified text with a single underline.
		/// </summary>
		public static void ShowSubTitle(string titleText)
		{
			ShowTitle(titleText, '-');
		}

		/// <summary>
		/// Displays the specified text with the specified underline.
		/// </summary>
		public static void ShowTitle(string titleText, char underlineChar)
		{
			int titleLength = titleText.Length;
			bool wrapText = false;
			bool includeNewLine = true;
			ShowIndentedText(0, titleText, wrapText, includeNewLine);
			ShowIndentedText(0, new string(underlineChar, titleLength),
				wrapText, includeNewLine);
		}

		/// <summary>
		/// Displays the specified text as a numbered paragraph, of the form "n) text", where n 
		/// is the paragraph number.
		/// </summary>
		public static void ShowNumberedText(int number, int indentLevel, string text,
			bool wrapText, params object[] args)
		{

			text = string.Format("{0}) {1}", number, text);
			ShowIndentedText(indentLevel, text, wrapText, true, args);
		}

		/// <summary>
		/// Displays the specified text indented by the specified number of tabs.  Arguments may be 
		/// inserted into the text, as in string.Format() and Console.WriteLine().
		/// </summary>
		public static void ShowIndentedText(int indentLevel, string text, bool wrapText,
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
				indentedText = WrapText(indentLevel, indentedText, (Console.BufferWidth - 1));
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

		#endregion

		#region Private and Protected Methods *****************************************************

		/// <summary>
		/// Wraps text at the last space on each line.  Second and subsequent lines are indented 
		/// one level more than the first line.
		/// </summary>
		private static string WrapText(int indentLevel, string text, int numberCharsPerLine)
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
