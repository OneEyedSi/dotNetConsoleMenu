using System;
using Gold.ConsoleMenu;

namespace MenuDemo
{
	/// <summary>
	/// Contains some of the the methods that will be used to generate the level 2 sub-menu.
	/// </summary>
	[MenuClass("Level 2 Menu", ParentMenuName = "Main Menu", DisplayOrder = 2)]
	public class Level2SubMenu1Methods1
	{
		#region First Demo Test *******************************************************************

		[MenuMethod("This is the first level 2 menu item", DisplayOrder = 1)]
		public static void Method1()
		{
			Console.WriteLine("Inside Level 2 Method 1");
		}

		#endregion

		#region Second Demo Test ******************************************************************

		[MenuMethod("This is the second level 2 menu item", DisplayOrder = 4)]
		public static void Method2()
		{
			Console.WriteLine("Inside Level 2 Method 2");
		}

		#endregion
	}
}
