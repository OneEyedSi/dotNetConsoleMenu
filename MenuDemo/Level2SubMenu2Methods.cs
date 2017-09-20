using System;
using Gold.ConsoleMenu;

namespace MenuDemo
{
	/// <summary>
	/// Contains the methods that will be used to generate the level 2 sub-menu 2.
	/// </summary>
	[MenuClass("Level 2 Menu 2", ParentMenuName = "Main Menu", DisplayOrder = 1)]
	public class Level2SubMenu2Methods
	{
		#region First Demo Test *******************************************************************

		[MenuMethod("This is the first level 2 menu 2 item")]
		public static void Method1()
		{
			Console.WriteLine("Inside Level 2.2 Method 1");
		}

		#endregion

		#region Second Demo Test ******************************************************************

		[MenuMethod("This is the second level 2 menu 2 item")]
		public static void Method2()
		{
			Console.WriteLine("Inside Level 2.2 Method 2");
		}

		#endregion
	}
}
