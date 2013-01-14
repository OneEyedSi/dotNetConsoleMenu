///////////////////////////////////////////////////////////////////////////////////////////////////
// Project      -   
// General      -   
//
// File Name    -   MainMenuMethods.cs
// Description  -   Contains the methods that will be used to generate the main menu.
//
// Notes        -   Methods to be displayed in the menu must have the following signature: 
//					public static void [[Method Name]]( )
//
//                  Decorating a class with the MenuClassAttribute will make any of its methods 
//                  that are decorated with a MenuMethodAttribute appear in a menu.
//
//                  The menu hierarchy is created via the MenuName and ParentMenuName properties 
//                  of the MenuClassAttribute.  Any class without a parent menu name will be part 
//                  of the main menu.  If the parent menu name is not used as another menu's name, 
//                  that menu will be an orphan and can never be displayed.
//
//                  Multiple classes can be combined into the same menu if they have the same menu 
//                  name and the same parent menu name.  The main menu is a special case: Multiple 
//                  classes can be combined into the main menu if they all have the same menu name 
//                  and no parent menu name.
//
//                  Menu items are displayed in the following order:
//                  1) Common menu items, that exit the menu or clear the screen, appear first;
//                  2) Menu items that open sub-menus appear next.  If a menu has multiple 
//                      sub-menus they will be sorted by sub-menu name in ascending order, by 
//                      default.  This default sort order can be overridden by the  
//                      MenuClassAttribute.DisplayOrder: The DisplayOrder property will affect 
//                      all sub-menus that have the same parent menu name.  If multiple classes are 
//                      combined into the same sub-menu only one of the classes needs the 
//                      MenuClassAttribute.DisplayOrder set.  If multiple classes are combined into 
//                      the same sub-menu and each class has a DisplayOrder with a different value, 
//                      the DisplayOrder of the first class that is loaded will be the value 
//                      selected.  The order in which the classes are loaded is indeterminate (it's 
//                      determined internally by the .NET framework);
//                  3) Menu items that run methods appear last.  If there are multiple methods
//                      the menu items that run them will be sorted by 
//                      MenuMethodAttribute.Description, by default.  This default sort order can 
//                      be overridden by the MenuMethodAttribute.DisplayOrder: The DisplayOrder 
//                      property will affect all methods of all classes that have the same menu 
//                      name.
//
// $History: $
///////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Data;
using MenuLibrary;
using Utilities.DisplayHelper;

namespace MenuDEMO
{
	/// <summary>
	/// Contains the methods that will be used to generate the main menu.
	/// </summary>
	[MenuClass("Main Menu")]
	public class MainMenuMethods
	{
		#region First Demo Test *******************************************************************

		[MenuMethod("This is the first menu item", DisplayOrder = 0)]
		public static void Method1()
		{
			Console.WriteLine("Inside Method1");
		}

		#endregion

		#region Second Demo Test ******************************************************************

		[MenuMethod("This is menu item 2", DisplayOrder = 1)]
		public static void Method2()
		{
			Console.WriteLine("Inside Method2");
		}

		#endregion

		#region Demonstrate ShowException *********************************************************

		[MenuMethod("Display details of an exception", DisplayOrder = 2)]
		public static void DisplayException()
		{
			try
			{
				InvalidOperationException exception3 =
					new InvalidOperationException("Third level exception.");
				AccessViolationException exception2 =
					new AccessViolationException("Second level exception.", exception3);
				throw new ApplicationException("Top level exception.", exception2);
			}
			catch (Exception xcp)
			{
				ConsoleDisplayHelper.ShowException(1, xcp);
			}
		}

		#endregion

		#region Demonstrate ShowObject ************************************************************

		[MenuMethod("Display the properties of an object", DisplayOrder = 3)]
		public static void DisplayAnObject()
		{
			try
			{
				MyLittleObject obj = new MyLittleObject(1, "String property");
				ConsoleDisplayHelper.ShowObject(obj, 0, "{0} to display:", obj.GetType().Name);
			}
			catch (Exception xcp)
			{
				ConsoleDisplayHelper.ShowException(1, xcp);
			}
		}

		private class MyLittleObject
		{
			private int _myInt;
			public int MyProperty
			{
				get { return _myInt; }
			}

			private string _myString;
			public string MyStringProperty
			{
				get { return _myString; }
			}

			public MyLittleObject(int myInt, string myString)
			{
				_myInt = myInt;
				_myString = myString;
			}
		}

		#endregion

		#region Demonstrate ShowException *********************************************************

		[MenuMethod("Demonstrate a Windows Forms menu system", DisplayOrder = 4)]
		public static void DisplayFormsMenu()
		{
			try
			{
				MenuForm form = new MenuForm();
				form.ShowDialog();
			}
			catch (Exception xcp)
			{
				ConsoleDisplayHelper.ShowException(1, xcp);
			}
		}

		#endregion
	}
}
