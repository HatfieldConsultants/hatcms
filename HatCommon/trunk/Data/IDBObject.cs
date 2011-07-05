using System;
using System.Data;
using System.Collections;

namespace Hatfield.Web.Portal.Data
{
    /// <summary>
    /// Summary description for DBObjectBase.
    /// </summary>
    public interface IDBObject
    {
        string dbEncode(string str);

        int RunProcedure(string storedProcName, IDataParameter[] parameters, out int rowsAffected);

        // IDataReader	RunProcedure(string storedProcName, IDataParameter[] parameters );
        DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, string tableName);
        void RunProcedure(string storedProcName, IDataParameter[] parameters, DataSet dataSet, string tableName);
        DataSet RunSelectQuery(string sql);
        int RunInsertQuery(string sql);
        int RunUpdateQuery(string sql);

        string getCSVParameter(string[] parameters);
        string getCSVParameter(int[] parameters);
        string getCSVParameter(DateTime[] parameters);
        string getCSVParameter(Stack parameters);

        double getPossiblyNullValue(DataRow dr, string colName, double retValIfNullOrError);
        int getPossiblyNullValue(DataRow dr, string colName, int retValIfNullOrError);
        DateTime getPossiblyNullValue(DataRow dr, string colName, DateTime retValIfNullOrError);
        DatabaseColumnDescriptor[] getTableDescription(string tableName);

    } // DBObjectBase
}
