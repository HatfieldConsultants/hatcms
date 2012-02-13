using System;
using System.Collections.Generic;
using System.Text;
using NHibernate;
using NHibernate.UserTypes;
using NHibernate.SqlTypes;
using System.Data;

namespace HatCMS
{
    public class BoolIntExchangeType : IUserType
    {
        #region IUserType Members

        public object Assemble(object cached, object owner)
        {
            return cached;
        }

        public object DeepCopy(object value)
        {
            return value;
        }

        public object Disassemble(object value)
        {
            return value;
        }

        public int GetHashCode(object x)
        {
            return x.GetHashCode();
        }

        public bool IsMutable
        {
            get { return false; }
        }

        public object NullSafeGet(IDataReader rs, string[] names, object owner)
        {
            int dbint = (int)NHibernateUtil.Int32.NullSafeGet(rs, names[0]);

            if (dbint == 1)
                return true;
            else if (dbint == 0)
                return false;
            else
                throw new Exception("Invalid value of the boolean field. Please find EIS team member to check the database");
        }

        public void NullSafeSet(IDbCommand cmd, object value, int index)
        {
            int dbint = Convert.ToInt32((bool) value);
            NHibernateUtil.Int32.NullSafeSet(cmd, dbint, index);
            
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public Type ReturnedType
        {
            get { return typeof(bool); }
        }

        public SqlType[] SqlTypes
        {
            get
            {
                SqlType[] types = new SqlType[1];
                types[0] = new SqlType(DbType.Int32);
                return types;
            }
        }

        public new bool Equals(object x, object y)
        {
            if (object.ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;
            return x.Equals(y);
        }

        #endregion
    }
}
