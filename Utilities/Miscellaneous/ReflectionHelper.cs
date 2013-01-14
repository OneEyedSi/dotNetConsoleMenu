///////////////////////////////////////////////////////////////////////////////////////////////////
// Project      -   Utilities
// General      -   Set of generic classes that may be useful in any project.
//
// File Name    -   ReflectionHelper.cs
// File Title   -   Reflection Helper
// Description  -   Routines that return metadata about assemblies, types, members, etc.
//
// Notes        -   
//
// $History: ReflectionHelper.cs $
// 
// *****************  Version 3  *****************
// User: Simone       Date: 11/03/09   Time: 2:45p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities
// Make static methods thread safe.
// 
// *****************  Version 1  *****************
// User: Simone       Date: 13/02/08   Time: 3:58p
// Created in $/UtilitiesClassLibrary/UtilitiesClassLibrary/Utilities
// 
// *****************  Version 1  *****************
// User: Simone       Date: 24/10/07   Time: 12:08p
// Created in $/UtilitiesClassLibrary/Utilities
// Copied from version 2 in
// $/ServiceAlliance/Interfaces/WebServiceInterfaces/JobsWebServices/Utili
// ties.
///////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Utilities.Miscellaneous
{
	/// <summary>
	/// Routines that return metadata about assemblies, types, members, etc.
	/// </summary>
	public class ReflectionHelper
	{
		#region Data Members **********************************************************************

		private static object _lockGetAssemblyAttribute = new object();

		#endregion

		/// <summary>
		/// Gets a specified attribute of the given assembly.  
		/// </summary>
		/// <typeparam name="T">The type of the attribute to return.</typeparam>
		/// <param name="callingMethod">The assembly which has the attribute that is to be returned.</param>
		/// <returns>The specified attribute of the given assembly.</returns>
		/// <remarks>Effectively wraps the Assembly.GetCustomAttributes() method so that a specified type 
		/// is returned rather than an object.</remarks>
		public static T GetAssemblyAttribute<T>(Assembly assembly)
			where T : Attribute
		{
			lock (_lockGetAssemblyAttribute)
			{
				Type typeT = typeof(T);
				T assemblyAttribute = default(T);

				object[] assemblyAttributes = assembly.GetCustomAttributes(typeT, true);
				if (assemblyAttributes.Length > 0)
				{
					assemblyAttribute = (T)assemblyAttributes[0];
				}
				return assemblyAttribute;
			}
		}
	}
}
