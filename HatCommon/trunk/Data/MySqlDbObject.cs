using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Web.Configuration;
using System.Collections;
using System.Collections.Generic;
using log4net;
using log4net.Config;


namespace Hatfield.Web.Portal.Data
{
    /// <summary>
    /// DbObject is the class from which all classes in the Data Services
    /// Tier inherit. The core functionality of establishing a connection
    /// with the database and executing simple stored procedures is also
    /// provided by this base class.
    /// </summary>
    public abstract class MySqlDbObject : IDBObject
    {
        protected MySqlConnection Connection;
        private static readonly ILog log = LogManager.GetLogger(typeof(MySqlDbObject));
        
        /// <summary>
        /// creates a new DbObject without the ConnectionString set.
        /// You must use doConnection() to start this connection.
        /// </summary>
        public MySqlDbObject()
        {
            /*
            connectionString = WebConfigurationManager.AppSettings["ConnectionString"];
            Connection = new MySqlConnection( connectionString );
            */
        }

        private string connectionString;
        /// <summary>
        /// A parameterized constructor, it allows us to take a connection
        /// string as a constructor argument, automatically instantiating
        /// a new connection.
        /// </summary>
        /// <param name="newConnectionString">Connection String to the associated database</param>
        public MySqlDbObject(string newConnectionString)
        {
            connectionString = newConnectionString;
            Connection = new MySqlConnection(connectionString);
        }

        public string getPossiblyNullValue(DataRow dr, string colName, string retValIfNullOrError)
        {
            return DBHelpers.getPossiblyNullValue(dr, colName, retValIfNullOrError);
        }

        public DateTime getPossiblyNullValue(DataRow dr, string colName, DateTime retValIfNullOrError)
        {
            return DBHelpers.getPossiblyNullValue(dr, colName, retValIfNullOrError);
        }

        public double getPossiblyNullValue(DataRow dr, string colName, double retValIfNullOrError)
        {
            return DBHelpers.getPossiblyNullValue(dr, colName, retValIfNullOrError);
        }

        public int getPossiblyNullValue(DataRow dr, string colName, int retValIfNullOrError)
        {
            return DBHelpers.getPossiblyNullValue(dr, colName, retValIfNullOrError);
        }

        public string[] dbEncode(string[] strings)
        {
            ArrayList a = new ArrayList();
            foreach (string s in strings)
                a.Add(dbEncode(s));
            return (string[])a.ToArray(typeof(string));
        }

        public string dbEncode(string str)
        {
            return DBHelpers.dbEncode(str);
        }

        public string dbEncodeNullableValue(string str, string nullSymbol)
        {
            return DBHelpers.dbEncodeNullableValue(str, nullSymbol);
        }

        public string dbEncode(DateTime dateTime)
        {
            return DBHelpers.dbEncode(dateTime);
        }


        /// <summary>
        /// Protected property that exposes the connection string
        /// to inheriting classes. Read-Only.
        /// </summary>
        protected string ConnectionString
        {
            get
            {
                return connectionString;
            }
        }

        /// <summary>
        /// a nasty way to get around various protection measures
        /// </summary>
        /// <param name="newConnectionString"></param>
        protected void doConnection(string newConnectionString)
        {
            connectionString = newConnectionString;
            Connection = new MySqlConnection(connectionString);
        }

        /// <summary>
        /// Private routine allowed only by this base class, it automates the task
        /// of building a SqlCommand object designed to obtain a return value from
        /// the stored procedure.
        /// </summary>
        /// <param name="storedProcName">Name of the stored procedure in the DB, eg. sp_DoTask</param>
        /// <param name="parameters">Array of IDataParameter objects containing parameters to the stored proc</param>
        /// <returns>Newly instantiated SqlCommand instance</returns>
        private MySqlCommand BuildIntCommand(string storedProcName, IDataParameter[] parameters)
        {
            MySqlCommand command = BuildQueryCommand(storedProcName, parameters);

            command.Parameters.Add(new MySqlParameter("ReturnValue",
                MySqlDbType.Int32,
                4, /* Size */
                ParameterDirection.ReturnValue,
                false, /* is nullable */
                0, /* byte precision */
                0, /* byte scale */
                string.Empty,
                DataRowVersion.Default,
                null));

            return command;
        }


        /// <summary>
        /// Builds a SqlCommand designed to return a SqlDataReader, and not
        /// an actual integer value.
        /// </summary>
        /// <param name="storedProcName">Name of the stored procedure</param>
        /// <param name="parameters">Array of IDataParameter objects</param>
        /// <returns></returns>
        private MySqlCommand BuildQueryCommand(string storedProcName, IDataParameter[] parameters)
        {
            MySqlCommand command = new MySqlCommand(storedProcName, Connection);
            command.CommandType = CommandType.StoredProcedure;

            foreach (MySqlParameter parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }

            return command;

        }

        /// <summary>
        /// Runs a stored procedure, can only be called by those classes deriving
        /// from this base. It returns an integer indicating the return value of the
        /// stored procedure, and also returns the value of the RowsAffected aspect
        /// of the stored procedure that is returned by the ExecuteNonQuery method.
        /// </summary>
        /// <param name="storedProcName">Name of the stored procedure</param>
        /// <param name="parameters">Array of IDataParameter objects</param>
        /// <param name="rowsAffected">Number of rows affected by the stored procedure.</param>
        /// <returns>An integer indicating return value of the stored procedure</returns>
        public int RunProcedure(string storedProcName, IDataParameter[] parameters, out int rowsAffected)
        {
            int result;

            if (Connection.State == ConnectionState.Closed ||
                Connection.State == ConnectionState.Broken)
            {
                Connection.Open();
            }
            MySqlCommand command = BuildIntCommand(storedProcName, parameters);
            rowsAffected = command.ExecuteNonQuery();
            result = (int)command.Parameters["ReturnValue"].Value;
            Connection.Close();
            return result;
        }
        /// <summary>
        /// Will run a stored procedure, can only be called by those classes deriving
        /// from this base. It returns a SqlDataReader containing the result of the stored
        /// procedure.
        /// </summary>
        /// <param name="storedProcName">Name of the stored procedure</param>
        /// <param name="parameters">Array of parameters to be passed to the procedure</param>
        /// <returns>A newly instantiated SqlDataReader object</returns>
        public MySqlDataReader RunProcedure(string storedProcName, IDataParameter[] parameters)
        {
            MySqlDataReader returnReader;

            if (Connection.State == ConnectionState.Closed ||
                Connection.State == ConnectionState.Broken)
            {
                Connection.Open();
            }
            MySqlCommand command = BuildQueryCommand(storedProcName, parameters);
            command.CommandType = CommandType.StoredProcedure;

            returnReader = command.ExecuteReader();
            //Connection.Close();
            return returnReader;
        }

        /// <summary>
        /// Creates a DataSet by running the stored procedure and placing the results
        /// of the query/proc into the given returned dataset with the given tablename.
        /// </summary>
        /// <param name="storedProcName"></param>
        /// <param name="parameters"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        /// 
        public DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, string tableName)
        {
            DataSet dataSet = new DataSet();
            if (Connection.State == ConnectionState.Closed ||
                Connection.State == ConnectionState.Broken)
            {
                Connection.Open();
            }
            MySqlDataAdapter sqlDA = new MySqlDataAdapter();
            sqlDA.SelectCommand = BuildQueryCommand(storedProcName, parameters);
            sqlDA.Fill(dataSet, tableName);
            Connection.Close();

            return dataSet;
        }
        //		protected DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, DataSet dataSet)
        //		{
        //			//DataSet dataSet = new DataSet();
        //			Connection.Open();
        //			MySqlDataAdapter sqlDA = new MySqlDataAdapter();
        //	
        //
        //			sqlDA.SelectCommand = BuildQueryCommand( storedProcName, parameters);
        //			sqlDA.Fill( dataSet );
        //
        //			Connection.Close();
        //
        //			return dataSet;
        //		}

        /// <summary>
        /// Takes an -existing- dataset and fills the given table name with the results
        /// of the stored procedure.
        /// </summary>
        /// <param name="storedProcName"></param>
        /// <param name="parameters"></param>
        /// <param name="dataSet"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public void RunProcedure(string storedProcName, IDataParameter[] parameters, DataSet dataSet, string tableName)
        {
            Connection.Open();
            MySqlDataAdapter sqlDA = new MySqlDataAdapter();
            sqlDA.SelectCommand = BuildIntCommand(storedProcName, parameters);
            sqlDA.Fill(dataSet, tableName);
            //sqlDA.Fill( dataSet );
            Connection.Close();
        }

        public string getCSVParameter(string[] parameters)
        {
            return DBHelpers.getCSVParameter(parameters);
        }

        public string getCSVParameter(int[] parameters)
        {
            return DBHelpers.getCSVParameter(parameters);
        }

        public string getCSVParameter(DateTime[] parameters)
        {
            return DBHelpers.getCSVParameter(parameters);
        }

        public string getCSVParameter(Stack parameters)
        {
            return DBHelpers.getCSVParameter(parameters);
        }

        public bool hasSingleRow(DataSet ds)
        {
            return DBHelpers.hasSingleRow(ds);
        }

        public DataRow getSingleRow(DataSet ds)
        {
            return DBHelpers.getSingleRow(ds);
        }

        public bool hasRows(DataSet ds)
        {
            return DBHelpers.hasRows(ds);
        }

        public DataSet RunSelectQuery(string sql)
        {
            DataSet dataSet = new DataSet();

#if DEBUG
            /*
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();

            string callingFunctionName = "";
            if (st.FrameCount > 1)
            {
                System.Diagnostics.StackFrame sf = st.GetFrame(1);
                System.Reflection.MethodBase method = sf.GetMethod();
                callingFunctionName = method.DeclaringType.Name + "." + method.Name + "()";
            }
            Console.Write(callingFunctionName);
            string line = Environment.NewLine + "---"+Environment.NewLine;
            line += DateTime.Now.ToString("d MMM HH:mm:ss tt") + " - " + callingFunctionName;
            line += Environment.NewLine + sql + Environment.NewLine;
            line += "---" + Environment.NewLine;
            
            System.IO.File.AppendAllText("c:\\temp\\querylog.txt", line);
            */
#endif

            try
            {
                if (Connection.State == ConnectionState.Closed ||
                    Connection.State == ConnectionState.Broken)
                {
                    Connection.Open();
                }

                MySqlDataAdapter sqlDA = new MySqlDataAdapter();
                sqlDA.SelectCommand = new MySqlCommand(sql, Connection);

                sqlDA.Fill(dataSet);

            }
            catch (Exception ex)
            {
                log.Error("failed query: " + sql);
                throw ex;
            }
            finally
            {
                Connection.Close();
            }
            return dataSet;

        } // RunQuery

        /// <summary>
        /// executes an SQL INSERT query, and returns the ID of this item, or -1 on error.
        /// </summary>
        /// <param name="sql">the SQL to run</param>
        /// <returns></returns>
        public int RunInsertQuery(string sql)
        {
            try
            {
                // -- open the connection
                if (Connection.State == ConnectionState.Closed ||
                    Connection.State == ConnectionState.Broken)
                {
                    Connection.Open();
                }

                // -- run the insert command
                MySqlCommand cmd = Connection.CreateCommand();

                cmd.CommandText = sql;
                int numInserted = cmd.ExecuteNonQuery();

                if (numInserted < 1)
                    return -1;

                return Convert.ToInt32(cmd.LastInsertedId);
            }
            catch (Exception ex)
            {
                log.Error("failed query: " + sql);
                Console.Write(ex.Message);
                throw ex;

            }
            finally
            {
                Connection.Close();
            }
            return -1;


        } // RunInsertQuery

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <returns>Returns the number of rows Affected</returns>
        public int RunUpdateQuery(string sql)
        {

            MySqlCommand cmd;

            try
            {

                if (Connection.State == ConnectionState.Closed ||
                    Connection.State == ConnectionState.Broken)
                {
                    Connection.Open();
                }


                cmd = Connection.CreateCommand();
                cmd.CommandText = sql;
                                
                int result = cmd.ExecuteNonQuery();

                return result;
            }
            catch (Exception e)
            {
                log.Error("failed query: " + sql);
                throw e;
            }

            finally
            {
                if (Connection.State != ConnectionState.Closed)
                    Connection.Close();
            }

        } // RunUpdateQuery


        public void unsetAutoCommit()
        {
            this.RunUpdateQuery("SET AUTOCOMMIT=0;");
        }

        // start a transaction
        public void StartTransaction()
        {
            this.RunUpdateQuery("START TRANSACTION;");
        }

        // rollback a transaction
        public void RollbackTransaction()
        {
            this.RunUpdateQuery("ROLLBACK;");
        }

        // end/commit a transaction
        public void CommitTransaction()
        {
            this.RunUpdateQuery("COMMIT;");
        }


        public DatabaseColumnDescriptor[] getTableDescription(string tableName)
        {
            string sql = "Describe " + tableName + ";";
            DataSet ds = this.RunSelectQuery(sql);
            List<DatabaseColumnDescriptor> arrayList = new List<DatabaseColumnDescriptor>();


            if (this.hasRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string columnName = dr["field"].ToString();
                    string TypeValue = dr["type"].ToString();
                    string[] typeArray = TypeValue.Split('(');
                    int maxLength = -1;
                    Type columType = getTypeFromDatabaseType(typeArray[0]);
                    if (typeArray.Length > 1)
                    {
                        string mLength = typeArray[1].Split(')')[0];
                        maxLength = Convert.ToInt32(mLength);
                    }
                    bool nullAble = IsColumnNullable(dr["Null"].ToString());
                    string defaultValue = dr["default"].ToString();
                    DatabaseColumnDescriptor column = new DatabaseColumnDescriptor(columnName, columType, maxLength, nullAble, defaultValue);
                    arrayList.Add(column);
                } // foreach row

            } // if there is data
            return arrayList.ToArray();
        }

        private bool IsColumnNullable(string value)
        {
            if (value.Equals("NO"))
            {
                return false;
            }
            return true;
        }

        private Type getTypeFromDatabaseType(string databaseType)
        {
            string dbType = databaseType.ToLower().Trim();
            Type typeMatch;
            switch (dbType)
            {
                case "smallint":
                    typeMatch = typeof(int);
                    break;
                case "int":
                    typeMatch = typeof(int);
                    break;
                case "varchar":
                    typeMatch = typeof(string);
                    break;
                case "char":
                    typeMatch = typeof(string);
                    break;
                case "datetime":
                    typeMatch = typeof(DateTime);
                    break;
                case "double":
                    typeMatch = typeof(double);
                    break;
                case "text":
                    typeMatch = typeof(string);
                    break;

                default:
                    throw new Exception("getTypeFromDatabaseType: Type Not Found (" + dbType + ")");

            }
            return typeMatch;
        } // getTypeFromDatabaseType

    } // DbObject class
}
