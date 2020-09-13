using System;
using System.Reflection;
using System.Threading;

namespace Gold.ConsoleMenu
{
	/// <summary>
	/// Delegate used to invoke a method from a menu.  All methods that appear in menus must 
	/// conform to this signature.
	/// </summary>
	public delegate void MenuMethod();	

	/// <summary>
	/// Menu generator that builds and displays a hierarchy of menus.
	/// </summary>
	public class MenuGenerator
	{
		#region Class data members ****************************************************************

		// To pause menu generator while waiting for results from asynchronous tests.
		private static int _waitingForResults;

		#endregion

		#region Menu Generation and Execution *****************************************************

		/// <summary>
		/// Generates the main menu from the classes in the assembly that called this method.
		/// </summary>
		public static void Run()
		{
			MenuGenerator.Run(Assembly.GetCallingAssembly());
		}
		
		/// <summary>
		/// Generates the main menu from the classes in the specified assembly.
		/// </summary>
		public static void Run(Assembly assemblyContainingMenuClasses)
		{
			Menu.LoadMenusFromAssembly(assemblyContainingMenuClasses);
			DisplayAndExecuteMenu(Menu.MainMenu);
		}

		/// <summary>
		/// Displays a menu and executes the item selected by the user.
		/// </summary>
		/// <param name="menu">The menu to display.</param>
		/// <returns>true if exiting the application based on the user input, otherwise false.</returns>
		/// <remarks>The menu will display items in the following order:
		/// 1) Menu items common to all menus;
		/// 2) Go up to parent menu (applicable to sub-menus only);
		/// 3) Sub-menus;
		/// 4) Menu items that execute methods.
		/// The sub-menu items and the execute method items will, by default, be sorted 
		/// alphabetically by the display text.  The sort order within each of these groups can be 
		/// modified using the DisplayOrder properties of the attributes that decorate the menu 
		/// classes and menu methods, respectively.</remarks>
		private static bool DisplayAndExecuteMenu(Menu menu)
		{
			string userInput = null;
			bool exitingApp = false;

			while (!exitingApp && userInput != MenuItemKey.GoUpToParentMenu)
			{
				DisplayMenu(menu);

				userInput = GetUserInput();
				string userInputValidationError = GetUserInputValidationError(menu, userInput);
				if (userInputValidationError != null)
				{
					Console.WriteLine();
					Console.WriteLine(userInputValidationError);
					Console.WriteLine();
					Console.WriteLine("Press [ENTER] key to continue...");
					Console.ReadLine();
					userInput = null;
					continue;
				}

				if (userInput == MenuItemKey.ExitApp)
				{
					exitingApp = true;
					Console.WriteLine();
					Console.WriteLine("Exiting application.  Press [ENTER] key to continue...");
					Console.ReadLine();
					continue;
				}

				if (userInput == MenuItemKey.ClearScreen)
				{
					Console.Clear();
					continue;
				}

				if (userInput == MenuItemKey.GoUpToParentMenu)
				{
					continue;
				}

				if (menu.SubMenus.Count > 0)
				{
					Menu selectedSubMenu = 
						menu.SubMenus.Find(delegate(Menu subMenu)
											{
												return (subMenu.Key.ToUpper() == userInput.ToUpper());
											});
					if (selectedSubMenu != null)
					{
						exitingApp = DisplayAndExecuteMenu(selectedSubMenu);
						continue;
					}
				}

				if (menu.MenuItems != null && menu.MenuItems.Count > 0)
				{
					MenuItem selectedMenuItem =
						menu.MenuItems.Find(delegate(MenuItem menuItem)
											{
												return (menuItem.Key == userInput);
											});
					if (selectedMenuItem != null)
					{
						RunMenuItemMethod(selectedMenuItem);
						// Add pause so that methods involving background threads won't write output 
						//  over top of menu.
						Thread.Sleep(500);

						Console.WriteLine();
						Console.WriteLine("Press [ENTER] key to continue...");
						Console.ReadLine();
						continue;
					}
				}
			}

			return exitingApp;
		}

		/// <summary>
		/// Displays the menu or sub-menu.
		/// </summary>
		/// <remarks>The menu will display items in the following order:
		/// 1) Menu items common to all menus;
		/// 2) Go up to parent menu (applicable to sub-menus only);
		/// 3) Sub-menus;
		/// 4) Menu items that execute methods.
		/// The sub-menu items and the execute method items will, by default, be sorted 
		/// alphabetically by the display text.  The sort order within each of these groups can be 
		/// modified using the DisplayOrder properties of the attributes that decorate the menu 
		/// classes and menu methods, respectively.</remarks>
		private static void DisplayMenu(Menu menu)
		{
			// Don't display menu name if there is only one menu.
			string menuNameToDisplay = menu.MenuName;
			if (menu.IsMainMenu && (menu.SubMenus == null || menu.SubMenus.Count == 0))
			{
				menuNameToDisplay = null;
			}

			Console.WriteLine();
			if (!string.IsNullOrEmpty(menuNameToDisplay) && menuNameToDisplay.Trim().Length > 0)
			{
				Console.WriteLine(menuNameToDisplay);
			}
			Console.WriteLine("====================================================");
			Console.WriteLine("Enter the menu item followed by [ENTER]:");
			Console.WriteLine();

			bool wrapText = true;
			bool includeNewLine = true;
			foreach (MenuItem item in menu.CommonMenuItems)
			{
				MenuHelper.ShowIndentedText(1, "{0}: {1}", wrapText, includeNewLine,
					item.Key, item.DisplayText);
			}

			if (menu.ParentMenuName != null)
			{
				MenuHelper.ShowIndentedText(1, "{0}: Go up to parent menu", wrapText, includeNewLine,
					MenuItemKey.GoUpToParentMenu);
			}

			if (menu.SubMenus.Count > 0)
			{
				Console.WriteLine();
			}
			foreach (Menu subMenu in menu.SubMenus)
			{
				MenuHelper.ShowHeadedText(subMenu.Key, 1, subMenu.MenuName, wrapText);
			}

			if (menu.MenuItems == null || menu.MenuItems.Count == 0)
			{
				return;
			}
			Console.WriteLine();
			foreach (MenuItem item in menu.MenuItems)
			{
				MenuHelper.ShowHeadedText(item.Key, 1, item.DisplayText, wrapText);
			}
		}

		/// <summary>
		/// Gets the user input when they select the menu item.
		/// </summary>
		/// <returns>A string representing the text the user entered.  Valid control characters, 
		/// such as the Escape character, are converted to text codes, such as "[ESC]".  Trailing 
		/// [ENTER] characters are not included in the user input string returned.</returns>
		/// <remarks>The method will continue to read the user input key strokes until the 
		/// [ENTER] key is pressed.  The algorithm is very simple, for example pressing the 
		/// [BACKSPACE] key will not remove the previously pressed key stroke.</remarks>
		private static string GetUserInput()
		{
			ConsoleKeyInfo cki;
			string resultantString = "";

			do
			{
				// Use ReadKey to pick up the control characters, such as the [ESC] key.
				cki = Console.ReadKey(true);

				// [SHIFT] is ok but not [CTRL] or [ALT].
				// Still allows [CTRL]+C to break out of application.
				if ((cki.Modifiers & (ConsoleModifiers.Alt | ConsoleModifiers.Control)) != 0)
				{
					// Will display the combination character, eg [CTRL]+key may display as a 
					//	single non-ASCII character.
					Console.WriteLine(cki.KeyChar);
					return UserInputCode.InvalidModifier;
				}

				switch (cki.Key)
				{
					case ConsoleKey.Escape:
						resultantString += UserInputCode.Escape;
						Console.Write(UserInputCode.Escape);
						break;
					case ConsoleKey.Delete:
						resultantString += UserInputCode.Delete;
						Console.Write(UserInputCode.Delete);
						break;
					case ConsoleKey.PageUp:
						resultantString += UserInputCode.PageUp;
						Console.Write(UserInputCode.PageUp);
						break;
					case ConsoleKey.Enter:
						Console.WriteLine();
						return resultantString;
					default:
						char pressedChar = cki.KeyChar;
						Console.Write(pressedChar);
						if (!Char.IsLetterOrDigit(pressedChar))
						{
							Console.WriteLine();
							return UserInputCode.InvalidChar;
						}
						resultantString += cki.KeyChar.ToString();
						break;
				}
			} while (true);

			return "";
		}

		/// <summary>
		/// Checks whether the text entered by the user is a valid menu item and, if not, 
		/// returns the appropriate error message.
		/// </summary>
		/// <returns>null if the user input is valid, otherwise an error message.</returns>
		private static string GetUserInputValidationError(Menu menu, string userInput)
		{
			if (userInput == UserInputCode.InvalidModifier)
			{
				return "Invalid control character.  [CTRL] or [ALT] may not be used.";
			}
			if (userInput == UserInputCode.InvalidChar)
			{
				string subMenuCommand = (menu.IsMainMenu ? "" : "[PG UP], ");
				return string.Format("Invalid character.  Only valid characters are "
					+ "[ESC], [DEL], {0}and ASCII alphanumeric characters.", subMenuCommand);
			}
			if (userInput == UserInputCode.PageUp && menu.IsMainMenu)
			{
				return "Invalid menu item.  The main menu has no parent menu.";
			}
			if (userInput == UserInputCode.Escape || userInput == UserInputCode.Delete
				|| userInput == UserInputCode.PageUp)
			{
				return null;
			}

			bool hasSubMenus = (menu.SubMenus.Count > 0);
			bool matchingSubMenuFound = hasSubMenus
				&& menu.SubMenus.Exists(delegate(Menu subMenu)
										{
											return (subMenu.Key.ToUpper() == userInput.ToUpper());
										});
			if (matchingSubMenuFound)
			{
				return null;
			}

			bool hasMenuItems = (menu.MenuItems != null && menu.MenuItems.Count > 0);
			bool matchingMenuItemFound = hasMenuItems
				&& menu.MenuItems.Exists(delegate(MenuItem menuItem)
										{
											return (menuItem.Key == userInput);
										});
			if (!matchingMenuItemFound)
			{				
				string commandKeyChoice = (hasSubMenus || hasMenuItems ? "either " : "");
				string commandKeys = (menu.IsMainMenu ? "[ESC] or [DEL]" : "[ESC], [DEL] or [PG UP]");
				string commandKeySeparator =
					(!hasSubMenus && !hasMenuItems	? ""
													: hasSubMenus && hasMenuItems ? ", " : " or ");
				string subMenuText = (hasSubMenus ? "a valid sub-menu" : "");
				string subMenuSeparator = (hasSubMenus && hasMenuItems ? " or " : "");
				string menuItemText = (hasMenuItems ? "a valid menu item" : "");
				return string.Format("Please enter {0}a valid command key ({1}){2}{3}{4}{5}.",
					commandKeyChoice, commandKeys, commandKeySeparator, 
					subMenuText, subMenuSeparator, menuItemText);
			}

			return null;
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
