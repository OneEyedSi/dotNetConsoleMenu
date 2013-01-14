using System;
using System.Collections.Generic;
using System.Text;
using MenuLibrary;

namespace MenuDEMO
{
	/// <summary>
	/// Contains some of the the methods that will be used to generate the level 4 sub-menu.
	/// </summary>
	[MenuClass("Level 4 Menu", ParentMenuName="Level 3 Menu")]
	public class Level4SubMenuMethods
	{
		#region First Demo Test *******************************************************************

		[MenuMethod("This is the first level 4 menu item")]
		public static void Method1()
		{
			Console.WriteLine("Inside Level 4 Method 1");
		}

		#endregion

		#region Second Demo Test ******************************************************************

		[MenuMethod("This is the second level 4 menu item")]
		public static void Method2()
		{
			Console.WriteLine("Inside Level 4 Method 2");
		}

		#endregion
	}
}
