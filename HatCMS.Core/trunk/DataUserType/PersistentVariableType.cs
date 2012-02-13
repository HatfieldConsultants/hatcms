using System;
using System.Collections.Generic;
using System.Text;
using NHibernate;
using NHibernate.UserTypes;
using NHibernate.SqlTypes;
using System.Data;

namespace HatCMS
{
    public class PersistentVariableType : IUserType
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

        public new bool Equals(object x, object y)
        {
            if (object.ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;
            return x.Equals(y);
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
            object blobobject = (object)NHibernateUtil.BinaryBlob.NullSafeGet(rs, names[0]);

            return blobobject;
        }

        public void NullSafeSet(IDbCommand cmd, object value, int index)
        {
            NHibernateUtil.BinaryBlob.NullSafeSet(cmd, value, index);
        }

        public object Replace(object original, object target, object owner)
        {
             return original;
        }

        public Type ReturnedType
        {
            get { return typeof(object); }
        }

        public SqlType[] SqlTypes
        {
            get
            {
                SqlType[] types = new SqlType[1];
                types[0] = new NHibernate.SqlTypes.BinaryBlobSqlType();
                return types;
            }
        }

        #endregion
    }
}
