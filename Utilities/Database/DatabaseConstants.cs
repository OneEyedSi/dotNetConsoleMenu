///////////////////////////////////////////////////////////////////////////////////////////////////
// Project      -   Utilities.Database
// General      -   Set of generic data access classes that may be useful in any project.
//
// File Name    -   DatabaseConstants.cs
// Description  -   Constants, enums and static classes that may be used in multiple data access 
//					classes.
//
// Notes        -   
//
// $History: DatabaseConstants.cs $
// 
// *****************  Version 1  *****************
// User: Simone       Date: 5/04/09    Time: 2:43p
// Created in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// Constants, enums and static classes that may be used in multiple data
// access classes.
///////////////////////////////////////////////////////////////////////////////////////////////////

namespace Utilities.Database
{
	/// <summary>
	/// Common return values used in the database managers (note that return values may be set 
	/// by the stored procedures that are being called by the database manager methods).
	/// </summary>
	public enum DatabaseManagerReturnValues
	{
		Success = 0,
		GeneralError = -1,
		TimeoutExpired = -25
	}

	/// <summary>
	/// Lists the values of the SqlException.Number property for different errors.
	/// </summary>
	/// <remarks>See System.Data.SqlClient.TdsParser.ProcessNetLibError in .NET 1.1.</remarks>
	/// <remarks>See System.Data.SqlClient.TdsEnums in .NET 2.0.</remarks>
	public enum SqlExceptionNumbers
	{
		TimeoutExpired = -2,
		UnableToConnect = 2
	}
}
