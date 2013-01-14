using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Utilities.DisplayHelper
{
	/// <summary>
	/// Helper methods for displaying text and details of objects via a Windows Forms TextBox 
	/// control.
	/// </summary>
	public class TextBoxDisplayHelper : DisplayHelper
	{
		#region Nested Classes, Enums, etc ********************************************************

		/// <summary>
		/// Type of text to display in the text box.  Used to format different types of text 
		/// differently.
		/// </summary>
		protected enum TextType
		{
			Normal = 0,
			Headed,
			Title,
			SubTitle
		}

		/// <summary>
		/// Method used to format different types of text.
		/// </summary>
		public delegate void FormatTextMethod(int indentLevel, int startOfTextPosition,
			int highlightedTextLength, TextType textType);

		#endregion

		#region Data Members **********************************************************************

		protected const int _tabWidth = 4;

		private TextBoxBase _textBox;

		#endregion

		#region Constructors, Destructors / Finalizers and Dispose Methods ************************

		public TextBoxDisplayHelper(TextBoxBase textBox)
		{
			this._textBox = textBox;				
		}

		#endregion

		#region Properties ************************************************************************

		/// <summary>
		/// The text box that will display the object details.
		/// </summary>
		public TextBoxBase TextBox
		{
			get { return _textBox; }
			set { _textBox = value; }
		}

		#endregion

		#region Static Methods ********************************************************************

		/// <summary>
		/// Displays the details of an object - either a single object or an enumeration of objects 
		/// - in the specified text box.
		/// </summary>
		public static void ShowObject(TextBoxBase textBox, object obj, int rootIndentLevel,
			string title, params object[] titleArgs)
		{
			TextBoxDisplayHelper objectViewer = new TextBoxDisplayHelper(textBox);
			objectViewer.DisplayObject(obj, rootIndentLevel, title, titleArgs);
		}

		/// <summary>
		/// Displays the values in a data table in the specified text box.
		/// </summary>
		public static void ShowDataTable(TextBoxBase textBox, DataTable dataTable, 
			bool displayRowState)
		{
			TextBoxDisplayHelper objectViewer = new TextBoxDisplayHelper(textBox);
			objectViewer.DisplayDataTable(dataTable, displayRowState);
		}

		/// <summary>
		/// Displays the details of an exception in the specified text box.
		/// </summary>
		public static void ShowException(TextBoxBase textBox, int indentLevel, Exception exception)
		{
			TextBoxDisplayHelper objectViewer = new TextBoxDisplayHelper(textBox);
			objectViewer.DisplayException(indentLevel, exception);
		}

		/// <summary>
		/// Appends the specified text to the last line of text.
		/// </summary>
		/// <param name="text"></param>
		public static void ShowAppendedText(TextBoxBase textBox, string text, bool addLeadingSpace,
			bool includeNewLine)
		{
			TextBoxDisplayHelper objectViewer = new TextBoxDisplayHelper(textBox);
			objectViewer.DisplayAppendedText(text, addLeadingSpace, includeNewLine);
		}

		/// <summary>
		/// Displays the specified text indented by the specified number of tabs.  Arguments may 
		/// be inserted into the text, as in string.Format() and Console.WriteLine().
		/// </summary>
		public static void ShowIndentedText(TextBoxBase textBox, int indentLevel, string text, 
			bool wrapText, bool includeNewLine, params object[] args)
		{
			TextBoxDisplayHelper objectViewer = new TextBoxDisplayHelper(textBox);
			objectViewer.DisplayIndentedText(indentLevel, text, wrapText, includeNewLine, args);
		}

		/// <summary>
		/// Displays the specified text indented by the specified number of tabs.  Similar to 
		/// DisplayIndentedText but if the text is of the form "header: text" then the header may 
		/// be formatted differently from the remaining text.
		/// </summary>
		public static void ShowHeadedText(TextBoxBase textBox, int indentLevel, string text, 
			bool wrapText, bool includeNewLine, params object[] args)
		{
			TextBoxDisplayHelper objectViewer = new TextBoxDisplayHelper(textBox);
			objectViewer.DisplayHeadedText(indentLevel, text, wrapText, includeNewLine, args);
		}

		/// <summary>
		/// Displays the specified text as a numbered paragraph, of the form "n) text", where n 
		/// is the paragraph number.
		/// </summary>
		public static void ShowNumberedText(TextBoxBase textBox, int number, int indentLevel, 
			string text, bool wrapText, params object[] args)
		{
			TextBoxDisplayHelper objectViewer = new TextBoxDisplayHelper(textBox);
			objectViewer.DisplayNumberedText(number, indentLevel, text, wrapText, args);
		}

		/// <summary>
		/// Displays the specified text with a double underline.
		/// </summary>
		public static void ShowTitle(TextBoxBase textBox, string titleText)
		{
			TextBoxDisplayHelper objectViewer = new TextBoxDisplayHelper(textBox);
			objectViewer.DisplayTitle(titleText);
		}

		/// <summary>
		/// Displays the specified text with a single underline.
		/// </summary>
		public static void ShowSubTitle(TextBoxBase textBox, string titleText)
		{
			TextBoxDisplayHelper objectViewer = new TextBoxDisplayHelper(textBox);
			objectViewer.DisplaySubTitle(titleText);
		}

		#endregion

		#region Public Methods ********************************************************************

		/// <summary>
		/// Writes the specified text to the results textbox indented by the specified number of tabs.  
		/// Arguments may be inserted into the text, as in string.Format() and Console.WriteLine().
		/// </summary>
		public override void DisplayIndentedText(int indentLevel, string text, bool wrapText,
			bool includeNewLine, params object[] args)
		{
			FormatTextMethod formatTextMethod = new FormatTextMethod(this.FormatText);
			this.DisplayIndentedText(formatTextMethod, TextType.Normal, indentLevel, text, 
				wrapText, includeNewLine, args);
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
		/// Appends the specified text to the last line of text in the results textbox.
		/// </summary>
		/// <param name="text"></param>
		public override void DisplayAppendedText(string text, bool addLeadingSpace, 
			bool includeNewLine)
		{
			if (addLeadingSpace)
			{
				text = " " + text;
			}

			this.TextBox.AppendText(text);

			if (includeNewLine)
			{
				this.TextBox.AppendText(Environment.NewLine);
			}
		}

		#endregion

		#region Private and Protected Methods *****************************************************

		/// <summary>
		/// Writes the specified text to the results textbox indented by the specified number of tabs.  
		/// Arguments may be inserted into the text, as in string.Format() and Console.WriteLine().
		/// </summary>
		protected virtual void DisplayIndentedText(FormatTextMethod formatTextMethod, 
			TextType textType, int indentLevel, string text, bool wrapText, bool includeNewLine, 
			params object[] args)
		{
			if (!string.IsNullOrEmpty(text) && text.Trim().Length > 0
				&& args != null && args.Length > 0)
			{
				text = string.Format(text, args);
			}
			int indentWidth = _tabWidth * indentLevel;
			string indentedText = new string(' ', indentWidth) + text;

			int startOfTextPosition = this.TextBox.TextLength + indentWidth;
			int highlightedTextLength = text.IndexOf(" (type:");
			if (highlightedTextLength == -1)
			{
				highlightedTextLength = text.IndexOf(':');
			}
			if (highlightedTextLength == -1)
			{
				highlightedTextLength = text.Length;
			}

			this.TextBox.AppendText(indentedText);

			formatTextMethod(indentLevel, startOfTextPosition, highlightedTextLength, textType);

			if (includeNewLine)
			{
				this.TextBox.AppendText(Environment.NewLine);
			}
		}

		/// <summary>
		/// Formats the text just added to the text box.  Only has an effect on a RichTextBox.
		/// </summary>
		protected virtual void FormatText(int indentLevel, int startOfTextPosition,
			int highlightedTextLength, TextType textType)
		{
			return;
		}

		#endregion
	}
}
