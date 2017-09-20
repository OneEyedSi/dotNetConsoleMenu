using System;
using Gold.ConsoleMenu;

namespace MenuDemo
{
	/// <summary>
	/// Contains the methods that will be used to generate the level 3 sub-menu.
	/// </summary>
	[MenuClass("Level 3 Menu", ParentMenuName = "Level 2 Menu", DisplayOrder = 2)]
	public class Level3SubMenu1Methods
	{
		#region First Demo Test *******************************************************************

		[MenuMethod("This is the first level 3 menu item")]
		public static void Method1()
		{
			Console.WriteLine("Inside Level 3 Method 1");
		}

		#endregion

		#region Second Demo Test ******************************************************************

		[MenuMethod("This is the second level 3 menu item")]
		public static void Method2()
		{
			Console.WriteLine("Inside Level 3 Method 2");
		}

		#endregion
	}
}
