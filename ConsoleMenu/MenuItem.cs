using System;

namespace ConsoleMenu
{
	/// <summary>
	/// An item to display in a menu or sub-menu, along with the method the item will execute if 
	/// selected.
	/// </summary>
	/// <remarks>Implements IComparable<MenuItem> to allow sorting of lists of MenuItems.</remarks>
	public class MenuItem : IComparable<MenuItem>
	{
		private string _displayText;
		private MenuMethod _method;
		private bool _isAsync;
		private string _subMenuFullName;
		private int _displayOrder;
		private string _key;

		public MenuItem(string displayText, MenuMethod method, bool isAsync, string subMenuFullName, 
			int displayOrder) : this (displayText, method, isAsync, subMenuFullName, displayOrder, 
										null)
		{
		}

		public MenuItem(string displayText, MenuMethod method, bool isAsync, string subMenuFullName, 
			int displayOrder, string hotKey)
		{
			_displayText = displayText;
			_method = method;
			_isAsync = isAsync;
			_subMenuFullName = subMenuFullName;
			_displayOrder = displayOrder;
			_key = hotKey;
		}

		public string DisplayText
		{
			get { return _displayText; }
			set { _displayText = value; }
		}

		public MenuMethod Method
		{
			get { return _method; }
			set { _method = value; }
		}

		public bool IsAsync
		{
			get { return _isAsync; }
			set { _isAsync = value; }
		}

		/// <summary>
		/// Full name of the sub-menu that the menu item represents.
		/// </summary>
		/// <remarks>Full name is of the form: [[Parent Menu Name]].[[Sub-menu Name]]</remarks>
		public string SubMenuFullName
		{
			get { return _subMenuFullName; }
			set { _subMenuFullName = value; }
		}

		/// <summary>
		/// Determines the location of the menu item in the menu.
		/// </summary>
		public int DisplayOrder
		{
			get { return _displayOrder; }
			set { _displayOrder = value; }
		}

		/// <summary>
		/// The key the user will enter to execute this menu item.
		/// </summary>
		public string Key
		{
			get { return _key; }
			set { _key = value; }
		}
		public void InvokeMethod()
		{
			Method();
		}

		// Implements IComparable.CompareTo().
		public int CompareTo(MenuItem other)
		{
			if (other == null)
			{
				return 1;
			}

			// Only menu items that open a sub-menu will have a sub-menu name.  Want these menu 
			//	items to appear at the top of the menu item list, before the menu items that 
			//	execute methods.
			bool thisOpensSubMenu = !string.IsNullOrEmpty(this.SubMenuFullName);
			bool otherOpensSubMenu = !string.IsNullOrEmpty(other.SubMenuFullName);
			if (thisOpensSubMenu && !otherOpensSubMenu)
			{
				return -1;
			}
			if (!thisOpensSubMenu && otherOpensSubMenu)
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
			return string.Compare(this.DisplayText, other.DisplayText, ignoreCase);
		}
	}
}
