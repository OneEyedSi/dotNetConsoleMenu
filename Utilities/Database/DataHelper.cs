//
// $History: DataHelper.cs $ 
// 
// *****************  Version 1  *****************
// User: Simone       Date: 20/03/09   Time: 11:53a
// Created in $/UtilitiesClassLibrary_DENG/Utilities.DataAccess
// Sql database helper class.
// 

using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace Utilities.Database
{
	/// <summary>
	/// The DataHelper class contains helper methods for accessing databases.
	/// </summary>
	public static class DataHelper
	{
		/// <summary>
		/// Create a output SqlParameter
		/// </summary>
		/// <param name="parameterName"></param>
		/// <param name="dbType"></param>
		/// <returns></returns>
		public static SqlParameter CreateOutputParameter(string parameterName, System.Data.SqlDbType dbType)
		{
			SqlParameter param = new SqlParameter(parameterName, dbType);
			param.Direction = System.Data.ParameterDirection.Output;
			return param;
		}

		/// <summary>
		/// Create a output SqlParameter
		/// </summary>
		/// <param name="parameterName"></param>
		/// <param name="dbType"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		public static SqlParameter CreateOutputParameter(string parameterName, System.Data.SqlDbType dbType, int size)
		{
			SqlParameter param = new SqlParameter(parameterName, dbType, size);
			param.Direction = System.Data.ParameterDirection.Output;
			return param;
		}
	}
}
