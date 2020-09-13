///////////////////////////////////////////////////////////////////////////////////////////////////
// Project      -   
// General      -   
//
// File Name    -   MenuAttributes.cs
// Description  -   Custom attributes used to identify methods that will be listed in the menu and 
//					the classes that contain them.
//
// Notes        -   
///////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace Gold.ConsoleMenu
{
	#region MenuClassAttribute ********************************************************************

	///<summary>
	/// Used to identify classes containing methods that will appear in the menu.
	///</summary>
	///<remarks>Methods from multiple classes can appear in the same menu, as long as they have the 
	///same menu name.</remarks>
	[AttributeUsage(AttributeTargets.Class)]
	public class MenuClassAttribute : Attribute
	{
		#region Data Members **********************************************************************

		private string _menuName;
		private string _parentMenuName;
		private int _displayOrder = MiscellaneousLiterals.DefaultDisplayOrder;

		#endregion

		#region Constructors, Destructors / Finalizers and Dispose Methods ************************

		/// <summary>
		/// Initialises the MenuClassAttribute.
		/// </summary>
		/// <param name="menuName">User-friendly but unique name for the menu the methods in this 
		/// class will be displayed in.</param>
		public MenuClassAttribute(string menuName)
		{
			_menuName = menuName;
		}

		#endregion

		#region Properties ************************************************************************

		/// <summary>
		/// User-friendly but unique name for the menu that the methods in this class will be 
		/// displayed in.
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
		/// Determines the location in the menu of the menu item generated from this class.
		/// </summary>
		public int DisplayOrder
		{
			get { return _displayOrder; }
			set { _displayOrder = value; }
		}

		#endregion
	}

	#endregion

	#region MenuMethodAttribute *****************************************************************

	///<summary>
	/// Used to identify and describe methods that will appear in the menu.
	///</summary>
	[AttributeUsage (AttributeTargets.Method)]
	public class MenuMethodAttribute : Attribute
	{
		#region Data Members **********************************************************************

		private string _description;
		private int _displayOrder = MiscellaneousLiterals.DefaultDisplayOrder;
		private bool _runAsynchronously = false;

		#endregion

		#region Constructors, Destructors / Finalizers and Dispose Methods ************************

		/// <summary>
		/// Initialises the MenuMethodAttribute.
		/// </summary>
		/// <param name="description">Short description of the method that will appear in the 
		/// auto-generated menu.</param>
		public MenuMethodAttribute(string description)
		{
			_description = description;
		}

		#endregion

		#region Properties ************************************************************************

		/// <summary>
		/// Short description of the method that will appear in the auto-generated menu.
		/// </summary>
		public string Description
		{
			get { return _description; }
		}

		/// <summary>
		/// Determines the location in the menu of the menu item generated from this method.
		/// </summary>
		public int DisplayOrder
		{
			get { return _displayOrder; }
			set { _displayOrder = value; }
		}

		/// <summary>
		/// Determines whether the method should be run asynchronously or not.
		/// </summary>
		public bool RunAsynchronously
		{
			get { return _runAsynchronously; }
			set { _runAsynchronously = value; }
		}
	
		#endregion
	}

	#endregion 
}