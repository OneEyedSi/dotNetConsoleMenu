using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace Utilities.Database
{
    public class Sql
    {
        public static SqlParameter AddParameter(List<SqlParameter> parameters, string parameterName, SqlDbType dbType, int size, ParameterDirection parameterDirection, object value)
        {
            SqlParameter sqlParameter = Sql.AddParameter(parameters, parameterName, dbType, size, parameterDirection);
            sqlParameter.Value = value;
            return sqlParameter;
        }

        public static SqlParameter AddParameter(List<SqlParameter> parameters, string parameterName, SqlDbType dbType, int size, ParameterDirection parameterDirection)
        {
            SqlParameter sqlParameter = new SqlParameter(parameterName, dbType, size);
            sqlParameter.Direction = parameterDirection;
            parameters.Add(sqlParameter);
            return sqlParameter;
        }

        public static SqlParameter AddParameter(List<SqlParameter> parameters, string parameterName, SqlDbType dbType, ParameterDirection parameterDirection)
        {
            SqlParameter sqlParameter = new SqlParameter(parameterName, dbType);
            sqlParameter.Direction = parameterDirection;
            parameters.Add(sqlParameter);
            return sqlParameter;
        }

        public static SqlParameter AddParameter(List<SqlParameter> parameters, string parameterName, object value)
        {
            SqlParameter sqlParameter = new SqlParameter(parameterName, value);
            parameters.Add(sqlParameter);
            return sqlParameter;
        }

        public static string GetString(SqlParameter parameter)
        {
            if (parameter.Value != DBNull.Value)
            {
                return parameter.Value.ToString();
            }
            return null;
        }

        public static int GetInteger(SqlParameter parameter)
        {
            if (parameter.Value != DBNull.Value)
            {
                return (int)parameter.Value;
            }
            return default(int);
        }

        public static DateTime? GetDateTime(object obj)
        {
            SqlParameter parameter = obj as SqlParameter;
            if (parameter != null)
            {
                obj = parameter.Value;
            }
            if (obj != DBNull.Value)
            {
                return (DateTime)obj;
            }
            return null;
        }

        public static double GetDouble(SqlParameter parameter)
        {
            if (parameter.Value != DBNull.Value)
            {
                return (double)parameter.Value;
            }
            return default(double);
        }

        public static bool GetBool(object obj)
        {
            SqlParameter parameter = obj as SqlParameter;
            if (parameter != null)
            {
                obj = parameter.Value;
            }
            if (obj != DBNull.Value)
            {
                return (bool)obj;
            }
            return false;
        }
    }
}
