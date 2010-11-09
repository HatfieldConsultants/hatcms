using System;
using System.Collections.Generic;
using System.Text;

namespace Hatfield.Web.Portal.Data
{
    public class DatabaseColumnDescriptor
    {
        public string columnName;
        public Type columnDataType;
        public int columnMaxLength;
        public bool isNullable;
        public string defaultValue;

        public DatabaseColumnDescriptor(string ColumnName, Type ColumnDataType, int ColumnMaxLength, bool IsNullable, string DefaultValue)
        {
            this.columnName = ColumnName;
            this.columnDataType = ColumnDataType;
            this.columnMaxLength = ColumnMaxLength;
            this.isNullable = IsNullable;
            this.defaultValue = DefaultValue;

        }

    }

}
