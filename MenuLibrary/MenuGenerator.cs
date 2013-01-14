///////////////////////////////////////////////////////////////////////////////////////////////////
// Project      -   
// General      -   
//
// File Name    -   MenuGenerator.cs
// Description  -   Generic code that generates a menu and executes the code corresponding to the 
//					user's selection.
//
// Notes        -   Displays a menu that allows the user to determine which method to run.
//
// $History: $
///////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Threading;

namespace MenuLibrary
{
	/// <summary>
	/// Delegate used to invoke a method from a menu.  All methods that appear in menus must 
	/// conform to this signature.
	/// </summary>
	public delegate void MenuMethod();	

	/// <summary>
	/// Menu generator that uses reflection to determine which methods are to be included in the 
	/// menu.
	/// </summary>
	public class MenuGenerator
	{
		#region Nested Enums, Private Classes, etc used by Menu Generator *************************

		/// <summary>
		///	Indexes representing special menu items in the list of menu items.
		/// </summary>
		private static class MenuItemNumber
		{
			public const int Invalid = -1;
			public const int Exit = 0;
			public const int ClearScreen = 1;
		}

		/// <summary>
		/// Arguments passed into methods used to display and execute a menu.
		/// </summary>
		/// <remarks>Defined as class not struct so that it will be passed into methods by reference.  
		/// This allows the name of the main menu to be discovered in a child method and be passed 
		/// back to the calling method in the child method's menu argument.</remarks>
		private class MenuArguments
		{
			public Assembly AssemblyContainingMenuClasses;
			public string MenuNameToDisplay;
			public bool IsMainMenu;
			public bool ReturnSubMenuList;
			public bool HasSubMenu = false;

			public MenuArguments(Assembly assemblyContainingMenuClasses, string menuNameToDisplay,
				bool isMainMenu)
				: this(assemblyContainingMenuClasses, menuNameToDisplay, isMainMenu, false) { }

			public MenuArguments(Assembly assemblyContainingMenuClasses, string menuNameToDisplay,
				bool isMainMenu, bool returnSubMenuList)
			{
				AssemblyContainingMenuClasses = assemblyContainingMenuClasses;
				MenuNameToDisplay = menuNameToDisplay;
				ReturnSubMenuList = returnSubMenuList;
				IsMainMenu = isMainMenu;
			}
		}

		#endregion

		#region Class data members ****************************************************************

		// To pause menu generator while waiting for results from asynchronous tests.
		private static int _waitingForResults;

		#endregion

		#region Menu Generation and Execution *****************************************************

		/// <summary>
		/// Displays a menu then executes the method associated with the menu item the user 
		/// selected.
		/// </summary>
		public static void Run()
		{
			Assembly assemblyContainingMenuClasses = Assembly.GetCallingAssembly();
			string menuName = null;
			bool isMainMenu = true;
			MenuArguments menuArguments =
				new MenuArguments(assemblyContainingMenuClasses, menuName, isMainMenu);
			DisplayAndExecuteMenu(menuArguments);
		}

		/// <summary>
		/// Displays a menu or sub-menu and executes the item selected by the user.
		/// </summary>
		/// <param name="menuArguments">Arguments needed to create the menu or sub-menu.</param>
		/// <remarks>The menu will display items in the following order:
		/// 1) Exit menu command;
		/// 2) Clear screen command;
		/// 3) Items that open sub-menus;
		/// 4) Items that execute methods.
		/// The sub-menu items and the execute method items will, by default, by sorted alphabetically 
		/// by the display text.  The sort order within each of these groups can be modified using the 
		/// DisplayOrder properties of the attributes that decorate the menu classes and menu 
		/// methods, respectively.</remarks>
		private static void DisplayAndExecuteMenu(MenuArguments menuArguments)
		{
			List<MenuItem> menuItems = GetMenuItems(menuArguments);

			bool isMainMenu = menuArguments.IsMainMenu;
			bool hasSubMenu = menuArguments.HasSubMenu;

			// Don't display menu name if there is only one menu.
			string menuNameToDisplay = menuArguments.MenuNameToDisplay;
			if (isMainMenu && !hasSubMenu)
			{
				menuNameToDisplay = null;
			}

			int itemNumberSelected = MenuItemNumber.Invalid;
			while (itemNumberSelected != MenuItemNumber.Exit)
			{
				DisplayMenu(menuItems, menuNameToDisplay);

				// Use readline rather than read so can have more than 9 tests in list.
				string textEntered = Console.ReadLine();
				if (textEntered.Length == 0 || !int.TryParse(textEntered, out itemNumberSelected)
					|| itemNumberSelected > (menuItems.Count - 1)
					|| itemNumberSelected == MenuItemNumber.Invalid)
				{
					// TryParse will set itemNumberSelected to 0 (= Exit).  Reset it to the Invalid value.
					itemNumberSelected = MenuItemNumber.Invalid;
					Console.WriteLine();
					Console.WriteLine("Whoops!  You must enter one of the numbers displayed.");
				}
				else if (itemNumberSelected == MenuItemNumber.Exit)
				{
					Console.WriteLine();
					if (isMainMenu)
					{
						Console.WriteLine("Exiting menu.  Press <Enter> key to continue...");
						Console.ReadLine();
					}
					else
					{
						Console.WriteLine("Exiting sub-menu.");
					}
				}
				else if (itemNumberSelected == MenuItemNumber.ClearScreen)
				{
					Console.Clear();
				}
				else
				{
					MenuItem itemSelected = menuItems[itemNumberSelected];
					if (itemSelected.SubMenuFullName == null)
					{
						Console.WriteLine();
						string title = string.Format("Executing menu item {0} ({1}):",
							itemNumberSelected, itemSelected.DisplayText);
						MenuHelper.ShowTitle(title);
						Console.WriteLine();
					}

					RunMenuItemMethod(itemSelected);
					// Add pause so that methods involving background threads won't write output 
					//  over top of menu.
					Thread.Sleep(500);
					if (itemSelected.SubMenuFullName == null)
					{
						Console.WriteLine();
						Console.WriteLine("Press <Enter> key to continue...");
						Console.ReadLine();
					}
				}
			}
		}

		/// <summary>
		/// Gets a list of all the menu items to display in the specified menu or sub-menu.
		/// </summary>
		/// <remarks>The list returned may include menu items that call methods and menu items that 
		/// open sub-menus.</remarks>
		private static List<MenuItem> GetMenuItems(MenuArguments menuArguments)
		{
			// menuArguments will be updated with the name of the main menu, which is needed to 
			//	identify the classes representing its sub-menus.
			List<Type> menuClasses = GetMenuClasses(menuArguments);

			Assembly assemblyContainingMenuClasses = menuArguments.AssemblyContainingMenuClasses;
			string menuNameToDisplay = menuArguments.MenuNameToDisplay;
			bool isMainMenu = menuArguments.IsMainMenu;

			List<MenuItem> menuItems = new List<MenuItem>();
			foreach (Type menuClass in menuClasses)
			{
				// MenuClassAttributes decorating the menu class.  There should be only one.
				object[] classAttributes =
					menuClass.GetCustomAttributes(typeof(MenuClassAttribute), false);
				MenuClassAttribute menuClassAttribute = (MenuClassAttribute)classAttributes[0];

				List<MenuItem> classMenuItems =
						GetClassMenuItems(menuClass, menuClassAttribute, menuNameToDisplay);
				if (classMenuItems.Count > 0)
				{
					menuItems.AddRange(classMenuItems);
				}

				MenuItem classSubMenuItem = GetSubMenuItem(menuArguments, menuClassAttribute);
				if (classSubMenuItem != null)
				{
					menuArguments.HasSubMenu = true;
					Predicate<MenuItem> findFunction = 
						delegate(MenuItem menuItem)
						{
							return (string.Compare(classSubMenuItem.SubMenuFullName, 
													menuItem.SubMenuFullName, true) == 0);
						};
					// If the menu is split across multiple classes then the resulting DisplayOrder 
					//	will be determined as follows:
					//	1) If only one class has the DisplayOrder set then this will be the 
					//		resulting DisplayOrder;
					//	2) If multiple classes have the DisplayOrder set then the resulting 
					//		DisplayOrder will be that of the first class read.  The order in which 
					//		the classes are read is indeterminate.
					MenuItem existingSubMenuItem = menuItems.Find(findFunction);
					if (existingSubMenuItem == null)
					{
						menuItems.Add(classSubMenuItem);
					}
					else if (existingSubMenuItem.DisplayOrder == 
						MiscellaneousLiterals.DefaultDisplayOrder)
					{
						existingSubMenuItem.DisplayOrder = classSubMenuItem.DisplayOrder;
					}
				}
			}

			// Sort to ensure sub-menu items appear above items that call methods, and to take into 
			//	account the DisplayOrders that may have been set via attributes.
			menuItems.Sort();

			string menuText = "main menu";
			if (!isMainMenu)
			{
				menuText = "sub-menu";
			}
			bool isAsync = false; 
			string subMenuFullName = null;
			int displayOrder = 0;
			menuItems.Insert(MenuItemNumber.Exit, new MenuItem(string.Format("Exit {0}", menuText),
				null, isAsync, subMenuFullName, displayOrder));
			displayOrder = 1;
			menuItems.Insert(MenuItemNumber.ClearScreen, new MenuItem("Clear screen", null, isAsync,
				subMenuFullName, displayOrder));

			return menuItems;
		}

		/// <summary>
		/// Gets the classes that contain menu methods.
		/// </summary>
		/// <remarks>Done in a separate method from getting the menu items so the name of the main 
		/// menu can be retrieved.  The main menu name is needed to identify the sub-menus of the 
		/// main menu.  When iterating through the classes in the assembly there is no guarantee 
		/// that the main menu class will be reached before the sub-menu classes so have to make 
		/// two passes through all the classes - one to get the main menu name and the second to 
		/// find the sub-menu classes.</remarks>
		private static List<Type> GetMenuClasses(MenuArguments menuArguments)
		{
			Assembly assemblyContainingMenuClasses = menuArguments.AssemblyContainingMenuClasses;
			string menuNameToDisplay = menuArguments.MenuNameToDisplay;

			List<Type> menuClasses = new List<Type>();

			Type[] assemblyClasses = assemblyContainingMenuClasses.GetTypes();
			foreach (Type assemblyClass in assemblyClasses)
			{
				object[] classAttributes =
					assemblyClass.GetCustomAttributes(typeof(MenuClassAttribute), false);

				// Class is decorated with the MenuClassAttribute, ie it represents a menu.
				if (classAttributes != null && classAttributes.Length > 0)
				{
					menuClasses.Add(assemblyClass);

					// Find the name of the main menu, if that is being displayed - need it to 
					//	find the sub-menus which have the main menu as their parent.
					// Update the menuArguments with this name, to pass it back to the calling 
					//	method.
					MenuClassAttribute menuClassAttribute = (MenuClassAttribute)classAttributes[0];
					if (menuNameToDisplay == null && menuClassAttribute.ParentMenuName == null)
					{
						menuNameToDisplay = menuClassAttribute.MenuName;
						menuArguments.MenuNameToDisplay = menuNameToDisplay;
					}
				}
			}

			return menuClasses;
		}

		/// <summary>
		/// Returns menu items that call methods in the specified class, if the class is part of 
		/// the menu that is being displayed.
		/// </summary>
		private static List<MenuItem> GetClassMenuItems(Type menuClass, 
			MenuClassAttribute menuClassAttribute, string menuNameToDisplay)
		{
			List<MenuItem> menuItems = new List<MenuItem>();
			Type menuMethodAttributeType = typeof(MenuMethodAttribute);
			string classMenuName = menuClassAttribute.MenuName;
			string classParentMenuName = menuClassAttribute.ParentMenuName;

			// menuNameToDisplay and classParentMenuName should both be null for the main menu.
			if ((menuNameToDisplay == null && classParentMenuName == null)
						|| string.Compare(menuNameToDisplay, classMenuName, true) == 0)
			{
				// Methods that appear in the menu must be both public and static.
				foreach (MethodInfo method
					in menuClass.GetMethods(BindingFlags.Public | BindingFlags.Static))
				{
					if (method.IsDefined(menuMethodAttributeType, false))
					{
						object[] methodAttributes
							= method.GetCustomAttributes(menuMethodAttributeType, false);
						MenuMethodAttribute menuMethodAttribute
							= (MenuMethodAttribute)methodAttributes[0];
						string menuDisplayText = menuMethodAttribute.Description;
						bool runAsynchronously = menuMethodAttribute.RunAsynchronously;
						MenuMethod menuMethod = 
							(MenuMethod)Delegate.CreateDelegate(typeof(MenuMethod), method);
						string subMenuFullName = null;
						int displayOrder = menuMethodAttribute.DisplayOrder;
						menuItems.Add(new MenuItem(menuDisplayText, menuMethod, runAsynchronously,
							subMenuFullName, displayOrder));
					}
				}
			}

			return menuItems;
		}

		/// <summary>
		/// Returns a menu item that will open a sub-menu, if selected by the user.
		/// </summary>
		private static MenuItem GetSubMenuItem(MenuArguments menuArguments, MenuClassAttribute menuClassAttribute)
		{
			Assembly assemblyContainingMenuClasses = menuArguments.AssemblyContainingMenuClasses;
			string menuNameToDisplay = menuArguments.MenuNameToDisplay;

			Type menuMethodAttributeType = typeof(MenuMethodAttribute);
			string classMenuName = menuClassAttribute.MenuName;
			string classParentMenuName = menuClassAttribute.ParentMenuName;

			// Parent menu name should only be null for main menu.
			if (classParentMenuName == null)
			{
				return null;
			}

			// Only interested in class that has the current menu as its parent, ie classes 
			//	representing sub-menus.
			if (string.Compare(menuNameToDisplay, classParentMenuName, true) != 0)
			{
				return null;
			}

			string menuItemText = "Sub-menu: " + classMenuName;
			MenuMethod menuMethod = new MenuMethod(delegate()
									{
										bool isMainMenu = false;
										MenuArguments subMenuArguments = 
											new MenuArguments(assemblyContainingMenuClasses,
												classMenuName, isMainMenu);
										DisplayAndExecuteMenu(subMenuArguments);
									});
			string subMenuFullName = string.Format("{0}.{1}", classParentMenuName, classMenuName);
			int displayOrder = menuClassAttribute.DisplayOrder;
			return new MenuItem(menuItemText, menuMethod, false, subMenuFullName, displayOrder);
		}

		/// <summary>
		/// Displays the menu or sub-menu.
		/// </summary>
		private static void DisplayMenu(List<MenuItem> menuItems, string menuNameToDisplay)
		{
			Console.WriteLine();
			if (!string.IsNullOrEmpty(menuNameToDisplay) && menuNameToDisplay.Trim().Length > 0)
			{
				Console.WriteLine(menuNameToDisplay);
			}
			Console.WriteLine("====================================================");
			Console.WriteLine("Enter menu item number followed by <Enter>:");

			int i = 0;
			foreach (MenuItem item in menuItems)
			{
				MenuHelper.ShowNumberedText(i, 1, item.DisplayText, true);
				i++;
			}
		}

		/// <summary>
		/// Runs the method corresponding to the specified menu item.  Pauses for results, if 
		/// required for an async method.
		/// </summary>
		private static void RunMenuItemMethod(MenuItem menuItem)
		{
			double minsTillAbort = 5;
			int millisecToPause = 1000;
			DateTime startTime = DateTime.Now;
			DateTime abortTime = startTime.AddMinutes(minsTillAbort);
			if (menuItem.IsAsync)
			{
				// Flag indicating that application is waiting on method results should be cleared 
				//	by the method or by the asynchronous callback method that is handling the results. 
				Interlocked.Exchange(ref _waitingForResults, 1);

				menuItem.InvokeMethod();

				while (_waitingForResults > 0 && DateTime.Now < abortTime)
				{
					Thread.Sleep(millisecToPause);
				}
				Interlocked.Exchange(ref _waitingForResults, 0);
			}
			else
			{
				menuItem.InvokeMethod();
			}
		}

		/// <summary>
		/// Event handler for displaying the results of an async method call (to be called by the 
		/// async method that was executed from the menu).
		/// </summary>
		public static void AsynEvents_HaveResponse(string XMLresponse)
		{
			MenuHelper.ShowTitle("RESPONSE STRING:");
			Console.WriteLine("{0}", XMLresponse);
			Console.WriteLine();

			// To tell menu generator that the results have been returned so it can continue executing.
			Interlocked.Exchange(ref _waitingForResults, 0);
		}

		#endregion
	}
}
