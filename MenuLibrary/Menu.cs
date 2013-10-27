using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MenuLibrary
{
	/// <summary>
	/// Represents a menu that will be displayed.
	/// </summary>
	/// <remarks>Implements IComparable<Menu> to allow sorting of lists of sub-menus.</remarks>
	public class Menu : IComparable<Menu>
	{
		#region Data Members **********************************************************************

		// Properties of this menu.
		private string _menuName;
		private string _parentMenuName;
		private int _displayOrder;
		private string _key;	// Key is the value the user enters to open this menu when it 
								//	appears as a sub-menu of a parent menu.  The key used in the 
								//	static Menus collection is the MenuName, not the Key, since 
								//	MenuName does not change but Key will depend on what other 
								//	sub-menus there are in a particular parent menu.
		private bool _isMainMenu;
		private List<Type> _menuClasses = new List<Type>();

		// Items that will appear in the menu.
		private List<MenuItem> _commonMenuItems;
		private Menu _parentMenu = null;
		private List<Menu> _subMenus = new List<Menu>();
		private List<MenuItem> _menuItems = null;	// Lazy load when required.

		// Items relating to the menu hierarchy.
		private static Dictionary<string, Menu> _menus = new Dictionary<string, Menu>();
		private static Menu _mainMenu;

		#endregion

		#region Constructors, Destructors / Finalizers and Dispose Methods ************************

		/// <summary>
		/// Initialises the Menu.
		/// </summary>
		public Menu(string menuName, string parentMenuName, int displayOrder, Type initialType)
		{
			_menuName = menuName;
			_parentMenuName = parentMenuName;
			_displayOrder = displayOrder;
			_menuClasses.Add(initialType);
		}

		#endregion

		#region Properties of this menu ***********************************************************

		/// <summary>
		/// Name that uniquely identifies the menu.
		/// </summary>
		public string MenuName
		{
			get { return _menuName; }
		}

		/// <summary>
		/// For sub-menus, the name of the parent menu.
		/// </summary>
		/// <remarks>For the top-level menu, this property should be null.</remarks>
		public string ParentMenuName
		{
			get { return _parentMenuName; }
			set { _parentMenuName = value; }
		}

		/// <summary>
		/// Determines the location in the parent menu that this sub-menu will appear in.
		/// </summary>
		public int DisplayOrder
		{
			get { return _displayOrder; }
			set { _displayOrder = value; }
		}

		/// <summary>
		/// The key the user will enter to open this menu.
		/// </summary>
		public string Key
		{
			get { return _key; }
			set { _key = value; }
		}

		/// <summary>
		/// Indicates whether this menu is the top-level menu or not.
		/// </summary>
		public bool IsMainMenu
		{
			get { return _isMainMenu; }
			set { _isMainMenu = value; }
		}

		/// <summary>
		/// The class or classes that contain the methods that represent the menu items for this 
		/// menu.
		/// </summary>
		public List<Type> MenuClasses
		{
			get { return _menuClasses; }
		}

		#endregion	

		#region Items that will be displayed in this menu *****************************************

		/// <summary>
		/// The common menu items that will be displayed by all menus.
		/// </summary>
		public List<MenuItem> CommonMenuItems
		{
			get
			{
				// Lazy load the menu items.
				if (_commonMenuItems == null)
				{
					_commonMenuItems = Menu.GetCommonMenuItems();
				}
				return _commonMenuItems;
			}
		}

		/// <summary>
		/// The sub-menus of this menu.
		/// </summary>
		public List<Menu> SubMenus
		{
			get { return _subMenus; }
		}

		/// <summary>
		/// The parent menu of this menu.
		/// </summary>
		public Menu ParentMenu
		{
			get { return _parentMenu; }
			set { _parentMenu = value; }
		}

		/// <summary>
		/// The menu items to display in this menu (apart from the sub-menus).
		/// </summary>
		public List<MenuItem> MenuItems
		{
			get
			{
				// Lazy load the menu items.
				if (_menuItems == null)
				{
					_menuItems = this.GetMenuItems();
				}
				return _menuItems;
			}
		}

		#endregion

		#region Static Properties and Methods relating to the menu hierarchy **********************

		/// <summary>
		/// All the menus in an application.
		/// </summary>
		public static Dictionary<string, Menu> Menus
		{
			get { return _menus; }
		}

		/// <summary>
		/// The Main Menu of the application.
		/// </summary>
		public static Menu MainMenu
		{
			get { return _mainMenu; }
			set { _mainMenu = value; }
		}

		/// <summary>
		/// Builds the menu hierarchy from the classes in the specified assembly.
		/// </summary>
		public static void LoadMenusFromAssembly(Assembly assemblyContainingMenuClasses)
		{
			Type[] assemblyClasses = assemblyContainingMenuClasses.GetTypes();
			foreach (Type assemblyClass in assemblyClasses)
			{
				Menu.ParseClassIntoMenu(assemblyClass);
			}

			Menu.BuildMenuHierarchy();
		}

		/// <summary>
		/// Checks if the class represents a menu and, if so, adds it to the Menus collection.
		/// </summary>
		/// <param name="menuClass"></param>
		public static void ParseClassIntoMenu(Type menuClass)
		{
			object[] menuAttributes =
				menuClass.GetCustomAttributes(typeof(MenuClassAttribute), false);
			if (menuAttributes == null || menuAttributes.Length == 0)
			{
				// Type doesn't represent a menu.
				return;
			}

			// Class should be decorated with only one MenuClassAttribute.
			MenuClassAttribute menuClassAttribute = (MenuClassAttribute)menuAttributes[0];
			string menuName = menuClassAttribute.MenuName;
			string parentMenuName = menuClassAttribute.ParentMenuName;
			int displayOrder = menuClassAttribute.DisplayOrder;

			// Multiple classes can combine to make a single menu.  So try to retrieve an existing 
			//	menu with the same name.
			Menu menu = null;
			Dictionary<string, Menu> menus = Menu.Menus;
			if (Menu.Menus.TryGetValue(menuName, out menu))
			{
				menu.MenuClasses.Add(menuClass);
			}
			else
			{
				menu = new Menu(menuName, parentMenuName, displayOrder, menuClass);
				Menu.Menus.Add(menuName, menu);
			}
		}

		/// <summary>
		/// Builds a hierarchy of parent and child menus with a single top-level Main Menu.
		/// </summary>
		public static void BuildMenuHierarchy()
		{
			bool mainMenuAlreadyFound = false;
			foreach (Menu menu in Menu.Menus.Values)
			{
				if (string.IsNullOrEmpty(menu.ParentMenuName)
					|| menu.ParentMenuName.Trim().Length == 0)
				{
					if (mainMenuAlreadyFound)
					{
						throw new ArgumentException("Cannot have more than one top level menu.  "
							+ "Check the menu classes for two menus without parent menu names.");
					}
					mainMenuAlreadyFound = true;
					menu.IsMainMenu = true;
					Menu.MainMenu = menu;
					continue;
				}

				Menu parentMenu = null;
				if (!Menu.Menus.TryGetValue(menu.ParentMenuName, out parentMenu))
				{
					string errorMessage = string.Format("Parent menu '{0}' does not exist.  "
						+ "Check the ParentMenuName for menu '{1}'.",
						menu.ParentMenuName, menu.MenuName);
					throw new ArgumentException(errorMessage);
				}

				menu.ParentMenu = parentMenu;
				parentMenu.SubMenus.Add(menu);
			}

			Menu.SetSubMenusKeys(Menu.MainMenu);
		}

		/// <summary>
		/// Sets the display key for each sub-menu.  The display key will be used by the user to 
		/// select a sub-menu to display.
		/// </summary>
		private static void SetSubMenusKeys(Menu parentMenu)
		{
			List<Menu> subMenus = parentMenu.SubMenus;
			if (subMenus == null || subMenus.Count == 0)
			{
				return;
			}

			subMenus.Sort();
			for (int i = 0; i < subMenus.Count; i++)
			{
				Menu subMenu = subMenus[i];
				subMenu.Key = Menu.ConvertIntToLetters(i + 1);
				Menu.SetSubMenusKeys(subMenu);
			}
		}

		/// <summary>
		/// Converts an integer in the range 1 to 702 into letters.
		/// </summary>
		/// <returns>"A" where integer = 1, "B" where integer = 2,... "AA" where integer = 27, 
		/// "AB" where integer = 28,... "ZY" where integer = 701, "ZZ" where integer = 702.</returns>
		private static string ConvertIntToLetters(int intToConvert)
		{
			// ASSUMPTION: That intToConvert cannot be greater than 27 * 26 
			//  (A, B, C,... AA, AB, AC,... ZX, ZY, ZZ)
			if (intToConvert < 1 || intToConvert > (27 * 26))
			{
				throw new ArgumentException("Integer to convert must be between 1 and 702.");
			}

			int leadingDigit = (intToConvert - 1) / 26;
			int trailingDigit = ((intToConvert - 1) % 26) + 1;

			string resultingLetters = "";
			if (leadingDigit > 0)
			{
				resultingLetters = ConvertIntToLetters(leadingDigit);
			}

			// Find the ASCII code for letter "A" then return the offset from that character.
			resultingLetters += (char)((char)'A' + (trailingDigit - 1));

			return resultingLetters;
		}

		#endregion

		#region IComparable Methods ***************************************************************

		// Implements IComparable.CompareTo().
		public int CompareTo(Menu other)
		{
			if (other == null)
			{
				return 1;
			}
			if (this.DisplayOrder < other.DisplayOrder)
			{
				return -1;
			}
			if (this.DisplayOrder > other.DisplayOrder)
			{
				return 1;
			}

			bool ignoreCase = true;
			return string.Compare(this.MenuName, other.MenuName, ignoreCase);
		}

		#endregion

		#region Private and Protected Methods *****************************************************

		/// <summary>
		/// Gets the common menu items that appear in every menu.
		/// </summary>
		/// <returns></returns>
		private static List<MenuItem> GetCommonMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>();

			MenuMethod menuMethod = null;
			bool runAsynchronously = false;
			string subMenuFullName = null;
			int displayOrder = 0;
			menuItems.Add(new MenuItem("Exit application", menuMethod, runAsynchronously, 
				subMenuFullName, displayOrder, MenuItemKey.ExitApp));
			
			displayOrder = 1;
			menuItems.Add(new MenuItem("Clear screen", menuMethod, runAsynchronously, 
				subMenuFullName, displayOrder, MenuItemKey.ClearScreen));

			menuItems.Sort();
			return menuItems;
		}

		/// <summary>
		/// Gets the menu items to display in this menu.
		/// </summary>
		/// <remarks>These are only the menu items that execute methods.  The menu items that open 
		/// sub-menus are built in the static method BuildMenuHierarchy.</remarks>
		private List<MenuItem> GetMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>();

			Type menuMethodAttributeType = typeof(MenuMethodAttribute);

			foreach (Type menuClass in this.MenuClasses)
			{
				// Methods that appear in the menu must be both public and static.
				foreach (MethodInfo method
					in menuClass.GetMethods(BindingFlags.Public | BindingFlags.Static))
				{
					if (method.IsDefined(menuMethodAttributeType, false))
					{
						object[] methodAttributes
							= method.GetCustomAttributes(menuMethodAttributeType, false);

						// Method should be decorated with only one MenuMethodAttribute.
						MenuMethodAttribute menuMethodAttribute
							= (MenuMethodAttribute)methodAttributes[0];

						string menuDisplayText = menuMethodAttribute.Description;
						bool runAsynchronously = menuMethodAttribute.RunAsynchronously;
						MenuMethod menuMethod =
							(MenuMethod)Delegate.CreateDelegate(typeof(MenuMethod), method);
						int displayOrder = menuMethodAttribute.DisplayOrder;
						menuItems.Add(new MenuItem(menuDisplayText, menuMethod, runAsynchronously,
							null, displayOrder));
					}
				}
			}

			this.SetMenuItemsKeys(ref menuItems);

			return menuItems;
		}

		/// <summary>
		/// Sets the display key for each menu item.  The display key will be used by the user to 
		/// execute the menu item.
		/// </summary>
		/// <remarks>This method only sets the keys for the menu items that execute methods.  The 
		/// menu items that open sub-menus have their keys set in the static method SetSubMenusKeys.</remarks>
		private void SetMenuItemsKeys(ref List<MenuItem> menuItems)
		{
			menuItems.Sort();
			for (int i = 0; i < menuItems.Count; i++)
			{
				MenuItem menuItem = menuItems[i];
				menuItem.Key = (i + 1).ToString();
			}
		}

		#endregion
	}
}
