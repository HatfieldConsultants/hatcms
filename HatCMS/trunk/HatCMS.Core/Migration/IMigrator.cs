using System;
using System.Collections.Generic;
using System.Text;

namespace HatCMS.Core.Migration
{
    interface IMigrator
    {
        //Migrate the database to the latest version
        void Migrate();
        //Decide if the database if up to date
        bool IsOutDated();
    }
}
