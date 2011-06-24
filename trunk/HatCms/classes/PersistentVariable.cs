using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Hatfield.Web.Portal;

namespace HatCMS
{
    public class CmsPersistentVariable
    {
        private int persistentvariableid;
        
        private string _name;
        /// <summary>
        /// The unique name (key) for this variable. Names are unique in a case-insensitive manner (so Name="AAA" is the same as Name="aaa").
        /// The name must
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value.Length > 255)
                    throw new ArgumentException("Persistent Variable names can be a maximum length of 255 characters");
                if (value.Trim().Length == 0)
                    throw new ArgumentException("Persistent Variable names can not be purely whitespace or of zero length");
                _name = value;
            }
        }

        
        public ISerializable PersistedValue;

        public CmsPersistentVariable(string name, ISerializable valueToPersist)
        {
            persistentvariableid = -1;
            _name = name;
            PersistedValue = valueToPersist;
        } // constructor

        public bool SaveToDatabase()
        {
            if (persistentvariableid < 0)
            {
                return (new PersistentVariableDB()).Insert(this);
            }
            else
            {
                return (new PersistentVariableDB()).Update(this);
            }
        } // SaveToDatabase

        public static CmsPersistentVariable Fetch(string name)
        {
            return (new PersistentVariableDB()).Fetch(name);
        } // Fetch

        public static CmsPersistentVariable[] FetchAll()
        {
            return (new PersistentVariableDB()).FetchAll();
        } // FetchAll


        public static bool Delete(CmsPersistentVariable persistentVariableToDelete)
        {
            return (new PersistentVariableDB()).Delete(persistentVariableToDelete.persistentvariableid);
        } // Delete

        #region PersistentVariableDB
        private class PersistentVariableDB : Hatfield.Web.Portal.Data.MySqlDbObject
        {
            public PersistentVariableDB()
                : base(ConfigUtils.getConfigValue("ConnectionString", ""))
            { }

            private byte[] Serialize(ISerializable serializable)
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                System.IO.MemoryStream memStream = new System.IO.MemoryStream();                
                binaryFormatter.Serialize(memStream, serializable);
                memStream.Seek(0, System.IO.SeekOrigin.Begin);
                return memStream.ToArray();                    
            }

            private ISerializable DeSerialize(byte[] byteArray)
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                System.IO.MemoryStream memStream = new System.IO.MemoryStream();
                memStream.Write(byteArray,0, byteArray.Length);
                memStream.Seek(0, System.IO.SeekOrigin.Begin);
                ISerializable ret = (ISerializable)binaryFormatter.Deserialize(memStream);
                return ret;
            }

            private void OpenMySqlConnection(MySql.Data.MySqlClient.MySqlConnection Connection)
            {
                if (Connection.State == ConnectionState.Closed || Connection.State == ConnectionState.Broken)
                {
                    Connection.Open();
                }
            }

            public bool Insert(CmsPersistentVariable item)
            {
                using (MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(ConfigUtils.getConfigValue("ConnectionString", "")))
                {
                    OpenMySqlConnection(conn);
                    MySql.Data.MySqlClient.MySqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO persistentvariables (Name, PersistedValue) VALUES(@Name, @PersistedValue);";
                    cmd.Parameters.AddWithValue("@Name", item.Name);
                    cmd.Parameters.AddWithValue("@PersistedValue", Serialize(item.PersistedValue));

                    int numInserted = cmd.ExecuteNonQuery();

                    if (numInserted < 1)
                        return false;

                    int newId = Convert.ToInt32(cmd.LastInsertedId);
                    item.persistentvariableid = newId;
                    
                    return true;

                }

            } // Insert


            public bool Update(CmsPersistentVariable item)
            {
                using (MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(ConfigUtils.getConfigValue("ConnectionString", "")))
                {
                    OpenMySqlConnection(conn);

                    MySql.Data.MySqlClient.MySqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "UPDATE persistentvariables set Name = @Name, PersistedValue = @PersistedValue where PersistentVariableId = @PersistentVariableId;";
                    cmd.Parameters.AddWithValue("@Name", item.Name);
                    cmd.Parameters.AddWithValue("@PersistedValue", Serialize(item.PersistedValue));
                    cmd.Parameters.AddWithValue("@PersistentVariableId", item.persistentvariableid);

                    int numUpdated = cmd.ExecuteNonQuery();

                    if (numUpdated < 1)
                        return false;

                    return true;

                }

            } // Update


            public bool Delete(int PersistentVariableId)
            {
                if (PersistentVariableId < 0)
                    return false;

                using (MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(ConfigUtils.getConfigValue("ConnectionString", "")))
                {
                    OpenMySqlConnection(conn); 

                    MySql.Data.MySqlClient.MySqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "DELETE from persistentvariables where PersistentVariableId = @PersistentVariableId;";

                    cmd.Parameters.AddWithValue("@PersistentVariableId", PersistentVariableId);

                    int numUpdated = cmd.ExecuteNonQuery();

                    if (numUpdated < 1)
                        return false;

                    return true;

                }
            }

            private CmsPersistentVariable GetFromRow(DataRow dr)
            {
                string Name = (dr["Name"]).ToString();
                byte[] bytes = (byte[])dr["PersistedValue"];
                
                CmsPersistentVariable item = new CmsPersistentVariable(Name, DeSerialize(bytes));
                item.persistentvariableid = Convert.ToInt32(dr["PersistentVariableId"]);
                return item;
            } // GetFromRow

            public CmsPersistentVariable Fetch(string name)
            {
                if (name.Trim() == "")
                    return new CmsPersistentVariable("",null);
                

                using (MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(ConfigUtils.getConfigValue("ConnectionString", "")))
                {
                    OpenMySqlConnection(conn); 

                    string sql = "SELECT PersistentVariableId, Name, PersistedValue from persistentvariables ";
                    sql += " WHERE Name like @Name";

                    MySql.Data.MySqlClient.MySqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = sql;

                    cmd.Parameters.AddWithValue("@Name", name);

                    MySql.Data.MySqlClient.MySqlDataAdapter sqlDA = new MySql.Data.MySqlClient.MySqlDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    sqlDA.Fill(ds);

                    if (this.hasSingleRow(ds))
                    {
                        DataRow dr = ds.Tables[0].Rows[0];
                        return GetFromRow(dr);
                    }
                    return new CmsPersistentVariable("", null);


                }

            } // Fetch


            public CmsPersistentVariable[] FetchAll()
            {
                using (MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(ConfigUtils.getConfigValue("ConnectionString", "")))
                {

                    OpenMySqlConnection(conn); 

                    string sql = "SELECT PersistentVariableId, Name, PersistedValue from persistentvariables; ";
                    

                    MySql.Data.MySqlClient.MySqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = sql;

                    

                    MySql.Data.MySqlClient.MySqlDataAdapter sqlDA = new MySql.Data.MySqlClient.MySqlDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    sqlDA.Fill(ds);

                    List<CmsPersistentVariable> arrayList = new List<CmsPersistentVariable>();
                    if (this.hasRows(ds))
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            arrayList.Add(GetFromRow(dr));
                        } // foreach row
                    } // if there is data

                    return arrayList.ToArray();


                }                                
                
            } // FetchAll

        } // class PersistentVariableDB
        #endregion
    }
}
