using System;
using System.Web.Configuration;
using System.Collections;

namespace Hatfield.Web.Portal.Data
{
    /// <summary>
    /// Summary description for DBDialect.
    /// </summary>
    public class DBDialect
    {
        public DBDialect()
        {

        }

        public static DBDialects forcedCurrentDBDialect = DBDialects.Unknown;

        public static DBDialects currentDialect
        {
            get
            {
                if (forcedCurrentDBDialect != DBDialects.Unknown)
                    return forcedCurrentDBDialect;

                string connStr = WebConfigurationManager.AppSettings["DatabaseDialect"];
                if (connStr == null || connStr.Trim() == "")
                {
                    connStr = WebConfigurationManager.AppSettings["ConnectionString"];
                    if (connStr == null)
                        throw new InvalidDatabaseDialect();
                }

                connStr = connStr.ToLower();
                if (connStr.IndexOf("mysql") > -1)
                {
                    return DBDialects.MySql;
                }
                else if (connStr.IndexOf("oracle") > -1)
                {
                    return DBDialects.Oracle9i;
                }
                throw new InvalidDatabaseDialect();
                // return DBDialects.Unknown;
            }
        }

        public static string currentDateTime
        {
            get
            {
                if (currentDialect == DBDialects.MySql)
                {
                    return "NOW()";
                }
                else if (currentDialect == DBDialects.Oracle9i)
                {
                    return "SYSDATE";
                }
                else
                {
                    throw new InvalidDatabaseDialect();
                }
            }
        }

        public static string isNull(string columnName)
        {
            if (currentDialect == DBDialects.MySql)
            {
                return "ISNULL(" + columnName + ")";
            }
            else if (currentDialect == DBDialects.Oracle9i)
            {
                return columnName + " IS NULL";
            }
            else
            {
                throw new InvalidDatabaseDialect();
            }
        }

        public static string getIdentityQuery(string insertSql)
        {
            if (currentDialect == DBDialects.MySql)
            {
                return "SELECT @@IDENTITY as newId;";
            }
            else if (currentDialect == DBDialects.Oracle9i)
            {
                // -- get the sequence name
                //   1) we need the table name
                string select_lower = insertSql.ToLower();
                int fromIndex = select_lower.IndexOf(" into ");
                if (fromIndex == -1)
                    throw new Exception("Could not determine Sequence Name from Query: " + insertSql);
                string restOfSql = select_lower.Substring(fromIndex);
                // split based on a space
                string[] parts = restOfSql.Split(new char[] { ' ' });
                // parts[0] = ""; [1]="into" [2]=tablename
                if (parts.Length < 2)
                    throw new Exception("Could not determine Sequence Name from Query: " + insertSql);

                string fromTable = parts[2].Trim();

                string format = WebConfigurationManager.AppSettings["hatWebPortalOracle9iDialectSequenceNameFormat"];
                if (format == null || format == "")
                {
                    format = "{0}_0";
                }

                string sequenceName = String.Format(format, fromTable).ToUpper();

                // -- more info: http://searchoracle.techtarget.com/tip/1,289483,sid41_gci910621,00.html
                return "select " + sequenceName + ".currval from dual";
            }
            else
            {
                throw new InvalidDatabaseDialect();
            }
        }

        /// <summary>
        /// Encode a dateTime value in the current Dialect.		
        /// </summary>
        /// <param name="dateTimeValue"></param>
        /// <param name="includeQuotes">If the currentDialect requires quoting the output, include those quotes</param>
        /// <returns></returns>
        public static string ToDBDateTime(DateTime dateTimeValue, bool includeQuotes)
        {
            if (currentDialect == DBDialects.MySql)
            {
                string dbFormat = dateTimeValue.ToString("u");
                char[] removeChars = { 'Z' };
                if (includeQuotes)
                {
                    return "'" + dbFormat.TrimEnd(removeChars) + "'";
                }
                else
                {
                    return dbFormat.TrimEnd(removeChars);
                }
            }
            else if (currentDialect == DBDialects.Oracle9i)
            {
                string oracleDate = dateTimeValue.ToString("yyyy/MM/dd:hh:mm:sstt");

                return "to_date('" + oracleDate + "', 'yyyy/mm/dd:hh:mi:ssam')";
            }
            else
            {
                throw new InvalidDatabaseDialect();
            }
        }

        /// <summary>
        /// by default, surrounds the return value in quotes (if the DBDialect needs them)
        /// </summary>
        /// <param name="dateTimeValue"></param>
        /// <returns></returns>
        public static string ToDBDateTime(DateTime dateTimeValue)
        {
            return ToDBDateTime(dateTimeValue, true);
        }

        public enum DBDialects { Unknown, MySql, Oracle9i }
        public class InvalidDatabaseDialect : Exception
        {
            public InvalidDatabaseDialect()
                : base("Invalid database dialect - set \"DatabaseDialect\" in your web.config!")
            { }
        }

    } // DBDialect class
}
