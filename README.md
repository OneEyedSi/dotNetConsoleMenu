# dotNetConsoleMenu
dotNetConsoleMenu is a menu generator for .NET console applications, which uses special attributes to flag classes and methods called by the menu.

The ConsoleMenu library targets **.NET 2.0** to make it as broadly usable as possible.

## NuGet Package
If required, [install NuGet](http://docs.nuget.org/docs/start-here/installing-nuget) for Visual Studio. Then, in Visual Studio, open the solution to add the package to and use the Package Manager Console to install [Gold.ConsoleMenu](https://www.nuget.org/packages/Gold.ConsoleMenu/):

    PM> Install-Package Gold.ConsoleMenu

## A Sample Menu

```
Forms Menu
====================================================
Enter the menu item followed by [ENTER]:

    [ESC]: Exit application
    [DEL]: Clear screen
    [PG UP]: Go up to parent menu

    A) Exceptions Menu
    B) Test Menu

    1) Display About form
    2) Display encryption form
```	

The first line of the menu is the user-friendly menu name, in this case "Forms Menu".

The menu itself is made of up to three sections:

1) Menu items that are automatically added to the menu.  "Exit application" and "Clear screen" are always included.  "Go up to parent menu" is included if the menu is a sub-menu of another menu.

2) An optional sub-menu section.  In this case there are two sub-menus: "Exceptions Menu" and "Test Menu".  Selecting either menu item will display the specified sub-menu.  Sub-menus are always labelled with letters, eg "**A)** Exceptions Menu", to distinguish them from method menu items.

3) An optional method menu items section.  In this case there are two menu items: "Display About form" and "Display encryption form".  Selecting a method menu item will execute the method associated with the menu item.  Method menu items are always labelled with numbers, eg "**2)** Display encryption form", to distinguish them from sub-menu menu items.

## Getting Started
The menu generator creates menus and menu items.

### Menus
A menu is created by decorating a class with a **[MenuClass]** attribute.  You must specify a user-friendly menu name for the menu in the MenuClass attribute.  For example:

```csharp
[MenuClass("Main Menu")]
public class RootMenu
{
    ...
}
```

In this case the menu name will be "Main Menu".

#### Sub-menus
When using an attribute to create a menu you may optionally include the name of a parent menu.  This will generate a sub-menu of the specified parent menu.  For example:

```csharp
[MenuClass("Forms Menu", ParentMenuName = "Main Menu")]
public class FormsMenu
{
    ....
}
```

In this case the Main Menu will include a menu item labelled "Forms Menu".  If that menu item is selected the Forms Menu will be displayed.  

The Forms Menu will include a menu item "Go up to parent menu" which will display the Main Menu again.

### Method Menu Items
A method menu item is created by decorating a static method in a menu class with a **[MenuMethod]** attribute.  You must specify the display text for the menu item in the MenuMethod attribute.  For example:

```csharp
[MenuClass("Main Menu")]
public class RootMenu
{
	[MenuMethod("This is the first menu item")]
	public static void Method1()
	{
		...
	}
}
```
In this case the text of the menu item will be "This is the first menu item".

Only methods with a specific signature may become menu items.  The methods must be **public static void** with **no arguments**.  Any method name is acceptable for a menu item method.

### Running the Menu Generator
In the Main method of the console application, call MenuGenerator.Run():

```csharp
static void Main(string[] args)
{
	MenuGenerator.Run();
}
```

When the application is run the top-level menu (the menu without a ParentMenuName specified) will be displayed.

## Namespace
The  MenuGenerator, the MenuClassAttribute and the MenuMethodAttribute classes are all in the **Gold.ConsoleMenu** namespace.  Add the following using statement to the top of any menu classes, and to the class containing the Main method:

```csharp
using Gold.ConsoleMenu;
```

## Display Order
By default, sub-menus and method menu items are displayed in alphabetical order: Sub-menus are ordered by menu name and method menu items are ordered by their display text.

The display order of sub-menus and method menu items may be set explicitly in the attributes that define them.  Both the [MenuClass] and [MenuMethod] attributes have an optional **DisplayOrder** property.  For example:

```csharp
[MenuClass("Forms Menu", ParentMenuName = "Main Menu", DisplayOrder = 2)]
public class FormsMenu
{
	[MenuMethod("Display About form", DisplayOrder = 3)]
	public static void DisplayAbout()
	{
		...
	}
}
```

## Splitting a Menu
If a menu has many method menu items you may want to split them across multiple classes.  This can be done by simply specifying the same menu name in the [MenuClass] attribute on both classes.  For example:

```csharp
[MenuClass("Forms Menu", ParentMenuName = "Main Menu")]
public class FormsMenu1
{
    ....
}
	
[MenuClass("Forms Menu", ParentMenuName = "Main Menu")]
public class FormsMenu2
{
    ....
}
```

The menu generator will combine the menu items from the two classes into a single menu.