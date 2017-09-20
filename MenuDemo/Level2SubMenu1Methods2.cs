using System;
using Gold.ConsoleMenu;

namespace MenuDemo
{

	/// <summary>
	/// Contains some of the the methods that will be used to generate the level 2 sub-menu.
	/// </summary>
	[MenuClass("Level 2 Menu", ParentMenuName = "Main Menu")]
	public class Level2SubMenu1Methods2
	{
		#region First Demo Test *******************************************************************

		[MenuMethod("This is the third level 2 menu item", DisplayOrder = 2)]
		public static void Method1()
		{
			Console.WriteLine("Inside Level 2 Method 3");
		}

		#endregion

		#region Second Demo Test ******************************************************************

		[MenuMethod("This is the fourth level 2 menu item", DisplayOrder = 3)]
		public static void Method2()
		{
			Console.WriteLine("Inside Level 2 Method 4");
		}

		#endregion
	}
}
