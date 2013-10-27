///////////////////////////////////////////////////////////////////////////////////////////////////
// Project      -   
// General      -   
//
// File Name    -   MenuAttributes.cs
// Description  -   Enums, string literals, etc, that may be used in this or other projects.
//
// Notes        -   
//
// $History: $
///////////////////////////////////////////////////////////////////////////////////////////////////

namespace MenuLibrary
{
	/// <summary>
	/// Miscellaneous literals.
	/// </summary>
	public static class MiscellaneousLiterals
	{
		public const int DefaultDisplayOrder = -99;
	}

	/// <summary>
	/// Keys for common menu items that may appear in multiple menus.
	/// </summary>
	/// <remarks>These keys are the values displayed in the menu that the user enters to run the 
	/// method or menu associated with menu item.</remarks>
	public static class MenuItemKey
	{
		public const string ExitApp = "[ESC]";
		public const string ClearScreen = "[DEL]";
		public const string GoUpToParentMenu = "[PG UP]";
	}

	/// <summary>
	/// Codes representing special keys or invalid text that a user may enter when selecting a 
	/// menu item.
	/// </summary>
	public static class UserInputCode
	{
		public const string Escape = "[ESC]";
		public const string Delete = "[DEL]";
		public const string PageUp = "[PG UP]";
		public const string InvalidModifier = "[INVALID MODIFIER]";
		public const string InvalidChar = "[INVALID CHAR]";
	}
}
