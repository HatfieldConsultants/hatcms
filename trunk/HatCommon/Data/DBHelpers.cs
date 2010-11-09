using System;
using System.Data;
using System.Collections;

namespace Hatfield.Web.Portal.Data
{
    /// <summary>
    /// Summary description for DBHelpers.
    /// </summary>
    public class DBHelpers
    {
        public DBHelpers()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public static string getPossiblyNullValue(DataRow dr, string colName, string retValIfNullOrError)
        {
            try
            {
                if (dr[colName] == System.DBNull.Value)
                    return retValIfNullOrError;

                return Convert.ToString(dr[colName]);
            }
            catch
            { }
            return retValIfNullOrError;
        }

        public static DateTime getPossiblyNullValue(DataRow dr, string colName, DateTime retValIfNullOrError)
        {
            try
            {
                if (dr[colName] == System.DBNull.Value)
                    return retValIfNullOrError;

                return Convert.ToDateTime(dr[colName]);
            }
            catch
            { }
            return retValIfNullOrError;
        }

        public static double getPossiblyNullValue(DataRow dr, string colName, double retValIfNullOrError)
        {
            try
            {
                if (dr[colName] == System.DBNull.Value)
                    return retValIfNullOrError;

                return Convert.ToDouble(dr[colName]);
            }
            catch
            { }
            return retValIfNullOrError;
        }

        public static int getPossiblyNullValue(DataRow dr, string colName, int retValIfNullOrError)
        {
            try
            {
                if (dr[colName] == System.DBNull.Value)
                    return retValIfNullOrError;

                return Convert.ToInt32(dr[colName]);
            }
            catch
            { }
            return retValIfNullOrError;
        }

        public static bool hasSingleRow(DataSet ds)
        {
            if (ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count == 1)
                return true;
            return false;
        }

        public static DataRow getSingleRow(DataSet ds)
        {
            return ds.Tables[0].Rows[0];
        }

        public static bool hasRows(DataSet ds)
        {
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                return true;
            return false;
        }

        /// <summary>
        /// by default, surrounds the return value in quotes (if the DBDialect needs them)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string dbEncode(DateTime dateTime)
        {
            return DBDialect.ToDBDateTime(dateTime);
        }

        public static string dbEncode(string str)
        {
            // one of the stupidest things that I have seen so far is that Oracle interprets an empty string
            // ('') as a NULL. So String.Empty == System.DBNull.Value when using Oracle
            // so if you have a column that can not be NULL, inserting a String.Empty will give you the error
            // "ORA-01400: cannot insert NULL into {table name}".
            // read this for more info: http://discuss.fogcreek.com/joelonsoftware4/default.asp?cmd=show&ixPost=120181
            if (DBDialect.currentDialect == DBDialect.DBDialects.Oracle9i)
            {
                if (str == "")
                    return " "; // force it to be a space (see note above)
                else
                    return str.Replace("'", "''"); // quotes are not escaped : http://www.jlcomp.demon.co.uk/faq/single_quote.html
            }
            else if (DBDialect.currentDialect == DBDialect.DBDialects.MySql)
            {
                str = str.Replace(@"\", @"\\");

                str = str.Replace("'", @"\'");

                string quoted = "\\" + '\"';
                return str.Replace("\"", quoted);
            }
            else
            {
                throw new DBDialect.InvalidDatabaseDialect();
            }
        }

        public static string dbDecode(string str)
        {
            return str.Replace(@"\'", "'");
        }

        public static string getCSVParameter(Stack parameters)
        {
            string output = "";
            object[] objects = parameters.ToArray();
            for (int i = 0; i < objects.Length; i++)
            {
                string param = "";
                if (objects[i] is DateTime)
                {

                    param = DBDialect.ToDBDateTime(Convert.ToDateTime(objects[i]));
                }
                else
                {
                    param = objects[i].ToString();
                }
                output += "\"" + param + "\"";
                if (i < objects.Length - 1)
                    output += ", ";
            } // foreach
            return output;

        } // getCSVParameter

        public static string getCSVParameter(string[] parameters)
        {
            string output = "";
            for (int i = 0; i < parameters.Length; i++)
            {
                output += "\"" + parameters[i] + "\"";
                if (i < parameters.Length - 1)
                    output += ", ";
            } // foreach
            return output;
        }

        public static string getCSVParameter(int[] parameters)
        {
            string output = "";
            for (int i = 0; i < parameters.Length; i++)
            {
                output += "\"" + parameters[i].ToString() + "\"";
                if (i < parameters.Length - 1)
                    output += ", ";
            } // foreach
            return output;
        }

        public static string getCSVParameter(DateTime[] parameters)
        {
            string output = "";
            for (int i = 0; i < parameters.Length; i++)
            {
                output += "\"" + DBDialect.ToDBDateTime(parameters[i]) + "\"";
                if (i < parameters.Length - 1)
                    output += ", ";
            } // foreach
            return output;
        }

    } // DBHelpers class
}
