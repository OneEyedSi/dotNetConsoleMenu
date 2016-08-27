using System;
using ConsoleMenu;

namespace MenuDemo
{
	/// <summary>
	/// Contains methods for the level 7 sub-menu which should never be displayed as there is no 
	/// parent menu named "Level 6 Menu".
	/// </summary>
	/// <remarks>When parsing the menus the ConsoleMenu will throw an ArgumentException if a 
	/// parent menu name does not exist as a menu.  Hence the MenuClassAttribute is commented out.
	/// To test the check for parent menu uncomment the MenuClassAttribute.</remarks>
	//[MenuClass("Level 7 Menu", ParentMenuName = "Level 6 Menu")]
	public class Level7SubMenuNoParent
	{
		#region First Demo Test *******************************************************************

		[MenuMethod("This is the first level 7 menu item")]
		public static void Method1()
		{
			Console.WriteLine("Inside Level 7 Method 1");
		}

		#endregion

		#region Second Demo Test ******************************************************************

		[MenuMethod("This is the second level 7 menu item")]
		public static void Method2()
		{
			Console.WriteLine("Inside Level 7 Method 2");
		}

		#endregion
	}
}
