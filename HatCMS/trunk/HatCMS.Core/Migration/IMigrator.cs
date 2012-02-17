using System;
using System.Collections.Generic;
using System.Text;

namespace HatCMS.Core.Migration
{
    interface IMigrator
    {
        void Migrate();
        IList<long> GetAvailableVersions();

    }
}
