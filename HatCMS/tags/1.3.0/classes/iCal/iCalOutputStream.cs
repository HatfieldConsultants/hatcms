using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace DDay.iCal
{
    /// <summary>
    /// This class implements an output encoding for iCal data. This class is needed because regular UTF8 output encoding
    /// includes the bom (ef bb bf) of utf files. These characters make the file un-readable, so must be removed.
    /// <para>
    /// Source: http://www.dotnet247.com/247reference/msgs/6/34399.aspx
    /// Reference: http://www.officekb.com/Uwe/Forum.aspx/outlook-calendar/17966/iCal-import-fails 
    /// Reference: http://msdn.microsoft.com/en-us/library/dd374101(VS.85).aspx
    /// </para>
    /// </summary>
    public class iCalOutputEncoding : System.Text.UTF8Encoding
    {
        public static iCalOutputEncoding Instance = new iCalOutputEncoding();

        private static byte[] _preamble = new byte[0];

        public override byte[] GetPreamble()
        {
            return _preamble;
        }
    }
}
