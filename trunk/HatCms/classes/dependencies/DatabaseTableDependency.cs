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

        public CmsDatabaseTableDependency(string tableName)
        {
            TableName = tableName.ToLower(); // mysql needs lower-case table names
            Columns = new DBColumnDescription[0];
        }


        public CmsDatabaseTableDependency(string tableName, string[] requiredColumnNames)
        {
            TableName = tableName.ToLower(); // mysql needs lower-case table names
            List<DBColumnDescription> columns = new List<DBColumnDescription>();
            foreach (string colName in requiredColumnNames)
            {
                DBColumnDescription c = new DBColumnDescription(colName, ExistsMode.MustExist);
                columns.Add(c);
            }
            Columns = columns.ToArray();
        }

        public CmsDatabaseTableDependency(string tableName, string[] requiredColumnNames, string[] colNamesThatMustNotExist)
        {
            TableName = tableName.ToLower(); // mysql needs lower-case table names
            List<DBColumnDescription> columns = new List<DBColumnDescription>();
            foreach (string colName in requiredColumnNames)
            {
                DBColumnDescription c = new DBColumnDescription(colName, ExistsMode.MustExist);
                columns.Add(c);
            }

            foreach (string colName in colNamesThatMustNotExist)
            {
                DBColumnDescription c = new DBColumnDescription(colName, ExistsMode.MustNotExist);
                columns.Add(c);
            }
            Columns = columns.ToArray();
        }

        public CmsDatabaseTableDependency(string tableName, DBColumnDescription[] columns)
        {
            TableName = tableName.ToLower(); // mysql needs lower-case table names
            Columns = columns;
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
