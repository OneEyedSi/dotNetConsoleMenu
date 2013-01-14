using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Utilities.DisplayHelper
{
	/// <summary>
	/// Helper methods for displaying text and details of objects via a Windows Forms RichTextBox 
	/// control.
	/// </summary>
	public class RichTextBoxDisplayHelper : TextBoxDisplayHelper
	{
		#region Nested Classes, Enums, etc ********************************************************

		#endregion

		#region Data Members **********************************************************************

		#endregion

		#region Constructors, Destructors / Finalizers and Dispose Methods ************************

		public RichTextBoxDisplayHelper(RichTextBox textBox) : base(textBox) { }

		#endregion

		#region Properties ************************************************************************

		#endregion

		#region Static Methods ********************************************************************

		/// <summary>
		/// Displays the details of an object - either a single object or an enumeration of objects 
		/// - in the specified rich text box.
		/// </summary>
		public static void ShowObject(RichTextBox textBox, object obj, int rootIndentLevel,
			string title, params object[] titleArgs)
		{
			RichTextBoxDisplayHelper objectViewer = new RichTextBoxDisplayHelper(textBox);
			objectViewer.DisplayObject(obj, rootIndentLevel, title, titleArgs);
		}

		/// <summary>
		/// Displays the values in a data table in the specified rich text box.
		/// </summary>
		public static void ShowDataTable(RichTextBox textBox, DataTable dataTable,
			bool displayRowState)
		{
			RichTextBoxDisplayHelper objectViewer = new RichTextBoxDisplayHelper(textBox);
			objectViewer.DisplayDataTable(dataTable, displayRowState);
		}

		/// <summary>
		/// Displays the details of an exception in the specified rich text box.
		/// </summary>
		public static void ShowException(RichTextBox textBox, int indentLevel, Exception exception)
		{
			RichTextBoxDisplayHelper objectViewer = new RichTextBoxDisplayHelper(textBox);
			objectViewer.DisplayException(indentLevel, exception);
		}

		/// <summary>
		/// Appends the specified text to the last line of text.
		/// </summary>
		/// <param name="text"></param>
		public static void ShowAppendedText(RichTextBox textBox, string text, bool addLeadingSpace,
			bool includeNewLine)
		{
			RichTextBoxDisplayHelper objectViewer = new RichTextBoxDisplayHelper(textBox);
			objectViewer.DisplayAppendedText(text, addLeadingSpace, includeNewLine);
		}

		/// <summary>
		/// Displays the specified text indented by the specified number of tabs.  Arguments may 
		/// be inserted into the text, as in string.Format() and Console.WriteLine().
		/// </summary>
		public static void ShowIndentedText(RichTextBox textBox, int indentLevel, string text,
			bool wrapText, bool includeNewLine, params object[] args)
		{
			RichTextBoxDisplayHelper objectViewer = new RichTextBoxDisplayHelper(textBox);
			objectViewer.DisplayIndentedText(indentLevel, text, wrapText, includeNewLine, args);
		}

		/// <summary>
		/// Displays the specified text indented by the specified number of tabs.  Similar to 
		/// DisplayIndentedText but if the text is of the form "header: text" then the header may 
		/// be formatted differently from the remaining text.
		/// </summary>
		public static void ShowHeadedText(RichTextBox textBox, int indentLevel, string text,
			bool wrapText, bool includeNewLine, params object[] args)
		{
			RichTextBoxDisplayHelper objectViewer = new RichTextBoxDisplayHelper(textBox);
			objectViewer.DisplayHeadedText(indentLevel, text, wrapText, includeNewLine, args);
		}

		/// <summary>
		/// Displays the specified text as a numbered paragraph, of the form "n) text", where n 
		/// is the paragraph number.
		/// </summary>
		public static void ShowNumberedText(RichTextBox textBox, int number, int indentLevel,
			string text, bool wrapText, params object[] args)
		{
			RichTextBoxDisplayHelper objectViewer = new RichTextBoxDisplayHelper(textBox);
			objectViewer.DisplayNumberedText(number, indentLevel, text, wrapText, args);
		}

		/// <summary>
		/// Displays the specified text with a double underline.
		/// </summary>
		public static void ShowTitle(RichTextBox textBox, string titleText)
		{
			RichTextBoxDisplayHelper objectViewer = new RichTextBoxDisplayHelper(textBox);
			objectViewer.DisplayTitle(titleText);
		}

		/// <summary>
		/// Displays the specified text with a single underline.
		/// </summary>
		public static void ShowSubTitle(RichTextBox textBox, string titleText)
		{
			RichTextBoxDisplayHelper objectViewer = new RichTextBoxDisplayHelper(textBox);
			objectViewer.DisplaySubTitle(titleText);
		}

		#endregion

		#region Public Methods ********************************************************************

		/// <summary>
		/// Displays the specified text indented by the specified number of tabs.  Similar to 
		/// DisplayIndentedText but if the text is of the form "header: text" then the header may 
		/// be formatted differently from the remaining text.
		/// </summary>
		public override void DisplayHeadedText(int indentLevel, string text, bool wrapText,
			bool includeNewLine, params object[] args)
		{
			FormatTextMethod formatTextMethod = new FormatTextMethod(this.FormatText);
			this.DisplayIndentedText(formatTextMethod, TextType.Headed, indentLevel, text,
				wrapText, includeNewLine, args);
		}

		/// <summary>
		/// Displays the specified text with a double underline.
		/// </summary>
		public override void DisplayTitle(string titleText)
		{
			this.DisplayTitle(titleText, TextType.Title);
		}

		/// <summary>
		/// Displays the specified text with a single underline.
		/// </summary>
		public override void DisplaySubTitle(string titleText)
		{
			this.DisplayTitle(titleText, TextType.SubTitle);
		}

		#endregion

		#region Private and Protected Methods *****************************************************

		/// <summary>
		/// Displays the specified text formatted appropriately for the text type.
		/// </summary>
		protected void DisplayTitle(string titleText, TextType textType)
		{
			int titleLength = titleText.Length;
			bool wrapText = false;
			bool includeNewLine = true;
			FormatTextMethod formatTextMethod = new FormatTextMethod(FormatText);
			this.DisplayIndentedText(formatTextMethod, textType, 0, titleText, wrapText,
				includeNewLine);
		}

		/// <summary>
		/// Formats the text just added to the rich text box.
		/// </summary>
		protected override void FormatText(int indentLevel, int startOfTextPosition,
			int highlightedTextLength, TextType textType)
		{
			RichTextBox textBox = (RichTextBox)this.TextBox;
			textBox.Select(startOfTextPosition, highlightedTextLength);
			if (textType == TextType.Title)
			{
				textBox.SelectedText = textBox.SelectedText.ToUpper();

				// Appears that have to select the text again after assigning to SelectedText.
				textBox.Select(startOfTextPosition, highlightedTextLength);
			}
			FontStyle fontStyle = FontStyle.Regular;
			if (textType == TextType.Headed)
			{
				fontStyle = FontStyle.Bold;
			}
			else if (textType == TextType.Title || textType == TextType.SubTitle)
			{
				fontStyle = FontStyle.Bold | FontStyle.Underline;
			}			
			textBox.SelectionFont = new Font(textBox.SelectionFont, fontStyle);
			textBox.SelectionColor = this.GetDisplayColour(indentLevel);
			textBox.ScrollToCaret();
		}

		/// <summary>
		/// Colours used to display the object members and their members.
		/// </summary>
		private Color GetDisplayColour(int indentLevel)
		{
			Color[] colours = { Color.Black, Color.DarkBlue, 
								Color.Teal, Color.DarkOliveGreen, 
								Color.Sienna, Color.SteelBlue, 
								Color.ForestGreen, Color.DarkGoldenrod };

			int indentColourIndex = indentLevel % colours.Length;

			return colours[indentColourIndex];
		}

		#endregion
	}
}
