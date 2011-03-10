using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace HatCMS
{
    /// <summary>
    /// A dependecy that requires that a table exists in the database, with specified columns.
    /// </summary>
    public class CmsDatabaseTableDependency : CmsDependency
    {        
        /// <summary>
        /// Describes a column used in a table.
        /// </summary>
        public class DBColumnDescription
        {
            public string ColumnName = "";
            public DbType ColumnType = DbType.String; 
            public int ColumnLength = Int32.MinValue;
            public ExistsMode existsMode = ExistsMode.MustExist;

            public DBColumnDescription(string colName)
            {
                ColumnName = colName;
            }

            public DBColumnDescription(string colName, ExistsMode _ExistsMode)
            {
                ColumnName = colName;
                existsMode = _ExistsMode;
            }

            public DBColumnDescription(string colName, DbType colType)
            {
                ColumnName = colName;
                ColumnType = colType;
            }

            public DBColumnDescription(string colName, DbType colType, int colLen)
            {
                ColumnName = colName;
                ColumnType = colType;
                ColumnLength = colLen;
            }

            public static DBColumnDescription[] GetByExistsMode(DBColumnDescription[] haystack, ExistsMode existsModeToFind)
            {
                List<DBColumnDescription> ret = new List<DBColumnDescription>();
                foreach (DBColumnDescription c in haystack)
                {
                    if (c.existsMode == existsModeToFind)
                        ret.Add(c);
                }
                return ret.ToArray();
            }
        }

        public string TableName = "";
        public DBColumnDescription[] Columns;

        public CmsDatabaseTableDependency(string mysqlCreateTableStatement)
        {
            initFromMySqlCreateStatement(mysqlCreateTableStatement);
        }

        public CmsDatabaseTableDependency(string mysqlCreateTableStatement, string[] colNamesThatMustNotExist)
        {
            initFromMySqlCreateStatement(mysqlCreateTableStatement);
            List<DBColumnDescription> columns = new List<DBColumnDescription>();
            foreach (string colName in colNamesThatMustNotExist)
            {
                DBColumnDescription c = new DBColumnDescription(colName, ExistsMode.MustNotExist);
                columns.Add(c);
            }
            Columns = columns.ToArray();
        }


        protected static string RemoveAtStartAndEnd(string toRemoveAtStartAndEnd, string removeFrom)
        {
            string ret = removeFrom.Trim();
            if (ret.StartsWith(toRemoveAtStartAndEnd, StringComparison.CurrentCultureIgnoreCase))
                ret = ret.Substring(toRemoveAtStartAndEnd.Length); // remove beginning
            if (ret.EndsWith(toRemoveAtStartAndEnd, StringComparison.CurrentCultureIgnoreCase))
                ret = ret.Substring(0, ret.Length - toRemoveAtStartAndEnd.Length);
            
            return ret;
        }

        protected void initFromMySqlCreateStatement(string mysqlCreateTableStatement)
        {
                       
            if (mysqlCreateTableStatement.IndexOf("CREATE TABLE", StringComparison.CurrentCultureIgnoreCase) < 0)
                throw new ArgumentException("Error: you have not specified a CREATE TABLE statement for CmsDatabaseTableDependency("+mysqlCreateTableStatement+")");
            
            // -- 1: table name
            int indexFirstOpenBracket = mysqlCreateTableStatement.IndexOf("(", StringComparison.CurrentCultureIgnoreCase);
            int indexLastCloseBracket = mysqlCreateTableStatement.LastIndexOf(")", StringComparison.CurrentCultureIgnoreCase);
            string tNameStartsAfter = "CREATE TABLE ";
            int tNameStartsAfterIndex = mysqlCreateTableStatement.IndexOf(tNameStartsAfter) + tNameStartsAfter.Length;
            string tName = mysqlCreateTableStatement.Substring(tNameStartsAfterIndex, (indexFirstOpenBracket - 1 ) - tNameStartsAfterIndex);            
            tName = RemoveAtStartAndEnd("`", tName);
            this.TableName = tName.ToLower();
            

            // -- column statements
            string allColumnStatements = mysqlCreateTableStatement.Substring(indexFirstOpenBracket, indexLastCloseBracket - indexFirstOpenBracket); 
            // remove first open bracket
            allColumnStatements = allColumnStatements.Substring(1);
            string[] colStatements = allColumnStatements.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<DBColumnDescription> dbColumnDescriptions = new List<DBColumnDescription>();
            foreach (string rawColStatement in colStatements)
            {
                string colStatement = rawColStatement.Trim();
                int firstTickIndex = colStatement.IndexOf("`", StringComparison.CurrentCultureIgnoreCase);
                int lastTickIndex = colStatement.LastIndexOf("`", StringComparison.CurrentCultureIgnoreCase);

                bool isKey = false;
                if (colStatement.IndexOf("PRIMARY KEY", StringComparison.CurrentCultureIgnoreCase) >= 0)
                    isKey = true;
                else if (colStatement.IndexOf("UNIQUE KEY", StringComparison.CurrentCultureIgnoreCase) >= 0)
                    isKey = true;
                else if (colStatement.IndexOf("KEY `", StringComparison.CurrentCultureIgnoreCase) >= 0)
                    isKey = true;                

                if (!isKey && firstTickIndex >= 0 && lastTickIndex > firstTickIndex)
                {
                    string cName = colStatement.Substring(0, lastTickIndex);
                    cName = RemoveAtStartAndEnd("`", cName);

                    DBColumnDescription c = new DBColumnDescription(cName, ExistsMode.MustExist);
                    dbColumnDescriptions.Add(c);
                }
            } // foreach colStatement            

            this.Columns = dbColumnDescriptions.ToArray();
        }

        public override string GetContentHash()
        {
            StringBuilder ret = new StringBuilder();
            ret.Append(TableName.ToLower());
            foreach (DBColumnDescription c in Columns)
            {
                string s = c.ColumnName + Enum.GetName(typeof(DbType), c.ColumnType) + Enum.GetName(typeof(ExistsMode), c.existsMode) + c.ColumnLength.ToString();
                ret.Append(s.Trim().ToLower());
            }
            return ret.ToString();
        }


        
        public override CmsDependencyMessage[] ValidateDependency()
        {
            return new dbTestdb().TestTable(TableName, Columns);
        }

        private class dbTestdb : Hatfield.Web.Portal.Data.MySqlDbObject
        {
            public dbTestdb()
                : base(ConfigurationManager.AppSettings["ConnectionString"])
            { }

   

            private CmsDependencyMessage[] TestColumn(DBColumnDescription col, DataRowCollection rows, string tableName)
            {
                List<CmsDependencyMessage> ret = new List<CmsDependencyMessage>();
                bool colFound = false;
                foreach (DataRow dr in rows)
                {
                    string fieldName = dr["Field"].ToString();
                    string fieldType = dr["Type"].ToString();

                    if (String.Compare(fieldName, col.ColumnName, true) == 0)
                    {
                        ///TODO: add comparison of (optional) column types
                        colFound = true;
                        
                        break;
                    }
                } // foreach

                if (col.existsMode == ExistsMode.MustExist && ! colFound)                
                    ret.Add(CmsDependencyMessage.Error(tableName+" table does not have a required column named '"+col.ColumnName+"'"));
                else if (col.existsMode == ExistsMode.MustNotExist && colFound)
                    ret.Add(CmsDependencyMessage.Error(tableName + " table has a column named '" + col.ColumnName + "' that must be removed."));

                return ret.ToArray();
            }

            public CmsDependencyMessage[] TestTable(string TableName, DBColumnDescription[] Columns)
            {
                List<CmsDependencyMessage> ret = new List<CmsDependencyMessage>();

                try
                {
                    string sql = "DESCRIBE `"+TableName+"`;";
                    DataSet ds = this.RunSelectQuery(sql);
                    if (hasRows(ds))
                    {
                        if (Columns.Length > 0)
                        {
                            foreach (DBColumnDescription col in Columns)
                            {
                                ret.AddRange(TestColumn(col, ds.Tables[0].Rows, TableName));
                            } // foreach
                        }
                    }
                    else
                    {
                        ret.Add(CmsDependencyMessage.Error("Database table '"+TableName+"' does not exist, or has no columns."));
                    }
                }
                catch(Exception ex)
                {
                    ret.Add(CmsDependencyMessage.Error(ex));
                }

                return ret.ToArray();
            } // TestTable

        }// class dbTestdb

        
    }
}
