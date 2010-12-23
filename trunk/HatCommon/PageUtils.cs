using System;
using System.Text;
using System.Web;
using System.Drawing;
using System.Collections.Specialized;
using System.Collections;
using System.Text.RegularExpressions;

namespace Hatfield.Web.Portal
{
	/// <summary>
	/// Summary description for PageUtils.
	/// </summary>
	public class PageUtils
	{
		public PageUtils()
		{
			throw new Exception("Error: do not instantiate Hatfield.Web.Portal.PageUtils - all methods are static.");
		}

        /// <summary>
        /// gets the currently executing ApplicationPath. Returned value always ends in a trailing '/'
        /// </summary>
        public static string ApplicationPath
        {
            get
            {
                return getApplicationPath(System.Web.HttpContext.Current);
            }
        }

        /// <summary>
        /// gets the currently executing ApplicationPath. Returned value always ends in a trailing '/'
        /// </summary>
        public static string getApplicationPath(HttpContext context)
        {
            try
            {                
                string apath = context.Request.ApplicationPath;
                if (!apath.EndsWith("/"))
                    apath = apath + "/";
                return apath;
            }
            catch
            { }
            return "/";

        }

        public static string MaxUploadFileSize
        {
            get
            {
                return getMaxUploadFileSize(System.Web.HttpContext.Current);
            }
        }

        public static long getMaxUploadFileSizeInBytes(HttpContext context)
        {
            try
            {
                string webConfigFN = context.Server.MapPath("~/web.config");
                System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.Load(webConfigFN);
                System.Xml.XmlNodeList nodes = xmlDoc.GetElementsByTagName("httpRuntime");
                if (nodes.Count == 1)
                {
                    System.Xml.XmlNode runtimeNode = nodes[0];
                    long maxLength = Convert.ToInt64(runtimeNode.Attributes["maxRequestLength"].InnerText);
                    // maxLength is in KB
                    maxLength = maxLength * 1024; // now in bytes

                    return maxLength;
                }
            }
            catch
            { }
            // default is 4 MB
            return 4 * 1048576;
        }

        public static string getMaxUploadFileSize(HttpContext context)
        {
            return StringUtils.formatFileSize(getMaxUploadFileSizeInBytes(context));            
        }

        //http://www.codeproject.com/useritems/Sending_Mails_From_C_.asp
        /// <summary>
        /// Validates an email address using a regular expression.
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public static bool isValidEmailAddress(string emailAddress)
        {

            Regex exp = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");

            Match m = exp.Match(emailAddress);

            if (m.Success && m.Value.Equals(emailAddress)) 
                return true;
            else return false;

        }

		/// <summary>
		/// gets a variable from a Form regardless if the form used a "Post" or "Get" method 
		/// </summary>
		/// <param name="name">the parameter to return the posted value for</param>
		/// <param name="defaultReturn">the default return value if the parameter was not found</param>
		/// <returns>the value of the posted form variable</returns>
		public static string getFromForm(string name, string defaultReturn)
		{
			try
			{
				System.Web.HttpRequest req = HttpContext.Current.Request;
				if (req.HttpMethod.ToUpper() == "GET")
				{
					if (req.QueryString[name] != null && req.QueryString[name] != "")
					{
						return req.QueryString[name];
					}				
				}
				else if (req.HttpMethod.ToUpper() == "POST")
				{	
					if (req.Form[name] != null && req.Form[name] != "")
					{
						return req.Form[name];
					}

				}
				return defaultReturn;
			} 
			catch
			{
				return defaultReturn;
			}
		} // getFromForm

		/// <summary>
		/// gets a variable from a Form regardless if the form used a "Post" or "Get" method 
		/// </summary>
		/// <param name="name">the parameter to return the posted value for</param>
		/// <param name="defaultValue">the default return value if the parameter was not found</param>
		/// <returns>the value of the posted form variable</returns>
		public static int getFromForm(string name, int defaultValue)
		{	
			string s = getFromForm(name,defaultValue.ToString());
			if (s != defaultValue.ToString())
			{
				try
				{
					return Convert.ToInt32(s);
				}
				catch
				{
					return defaultValue;
				}
			}
			return defaultValue;

		} // getFromForm

        /// <summary>
        /// gets a variable from a Form regardless if the form used a "Post" or "Get" method 
        /// </summary>
        /// <param name="name">the parameter to return the posted value for</param>
        /// <param name="defaultValue">the default return value if the parameter was not found</param>
        /// <returns>the value of the posted form variable</returns>
		public static long getFromForm(string name, long defaultValue)
		{	
			string s = getFromForm(name,defaultValue.ToString());
			if (s != defaultValue.ToString())
			{
				try
				{
					return Convert.ToInt64(s);
				}
				catch
				{
					return defaultValue;
				}
			}
			return defaultValue;

		} // getFromForm

        /// <summary>
        /// gets a variable from a Form regardless if the form used a "Post" or "Get" method.
        /// Example Enum usage:
        /// <example>
        /// public enum EditFormPageTab { AddField, FieldSettings, FormSettings }
        /// EditFormPageTab currentPageTab = (EditFormPageTab) PageUtils.getFromForm("t", typeof(EditFormPageTab), EditFormPageTab.AddField);
        /// </example>
        /// </summary>
        /// <param name="name">the parameter to return the posted value for</param>
        /// <param name="defaultValue">the default return value if the parameter was not found</param>
        /// <returns>the value of the posted form variable</returns>
        public static object getFromForm(string name, Type enumType, Enum defaultValue)
        {
            string s = getFromForm(name, defaultValue.ToString());
            if (s != defaultValue.ToString())
            {
                try
                {
                    object ret = Enum.Parse(enumType, s, true);
                    return ret;
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

		
		/// <summary>
		/// gets a variable from a Form regardless if the form used a "Post" or "Get" method 
		/// </summary>
		/// <param name="name">the parameter to return the posted value for</param>
		/// <param name="defaultValue">the default return value if the parameter was not found</param>
		/// <returns>the value of the posted form variable</returns>
		public static DateTime getFromForm(string name, DateTime defaultValue)
		{	
			string s = getFromForm(name,defaultValue.ToString());
			if (s != defaultValue.ToString())
			{
				try
				{
					return Convert.ToDateTime(s);
				}
				catch
				{
					return defaultValue;
				}
			}
			return defaultValue;

		} // getFromForm

		/// <summary>
		/// gets a variable from a Form regardless if the form used a "Post" or "Get" method 
		/// </summary>
		/// <param name="name">the parameter to return the posted value for</param>
		/// <param name="defaultValue">the default return value if the parameter was not found</param>
		/// <returns>the value of the posted form variable</returns>
		public static bool getFromForm(string name, bool defaultValue)
		{
			string s = getFromForm(name,defaultValue.ToString());
			if (s != defaultValue.ToString())
			{
				try
				{
					return Convert.ToBoolean(s);
				}
				catch
				{
                    try // add to handle Convert.ToBoolean("0")
                    {
                        return Convert.ToBoolean(Convert.ToInt32(s));
                    }
                    catch { }
					return defaultValue;
				}
			}
			return defaultValue;
		} // getFromForm

		/// <summary>
		/// gets a variable from a Form regardless if the form used a "Post" or "Get" method 
		/// </summary>
		/// <param name="name">the parameter to return the posted value for</param>
		/// <param name="defaultValue">the default return value if the parameter was not found</param>
		/// <returns>the value of the posted form variable</returns>
		public static double getFromForm(string name, double defaultValue)
		{
			string s = getFromForm(name,defaultValue.ToString());
			if (s != defaultValue.ToString())
			{
				try
				{
					return Convert.ToDouble(s);
				}
				catch
				{
					return defaultValue;
				}
			}
			return defaultValue;
		}

		public static string[] getFromForm(string name)
		{
			string val = getFromForm(name,"");
			string[] parts = val.Split(new char[] {','});
			ArrayList arrayList = new ArrayList();
			foreach(string s in parts)
			{
				if (s.Trim() != "")
					arrayList.Add(s);
			}
			string[] ret = new string[arrayList.Count];
			arrayList.CopyTo(ret);
			return ret;
		}


		public static string ColorToHTMLRGB(System.Drawing.Color color)
		{
			string ret =  color.R.ToString("x2")+color.G.ToString("x2")+color.B.ToString("x2");
			return ret;
		}

		public static string MimeTypeLookup(string extension)
		{
			// listing from http://www.webmaster-toolkit.com/mime-types.shtml
			string mime = "";
			if (!extension.StartsWith("."))
				extension = "."+extension;
            switch (extension.Trim().ToLower())
            {
                #region this is a really long list
                case ".3dm": mime = "x-world/x-3dmf"; break;
                case ".3dmf": mime = "x-world/x-3dmf"; break;
                case ".a": mime = "application/octet-stream"; break;
                case ".aab": mime = "application/x-authorware-bin"; break;
                case ".aam": mime = "application/x-authorware-map"; break;
                case ".aas": mime = "application/x-authorware-seg"; break;
                case ".abc": mime = "text/vnd"; break;
                case ".acgi": mime = "text/html"; break;
                case ".afl": mime = "video/animaflex"; break;
                case ".ai": mime = "application/postscript"; break;
                case ".aif": mime = "audio/aiff"; break;
                case ".aifc": mime = "audio/aiff"; break;
                case ".aiff": mime = "audio/aiff"; break;
                case ".aim": mime = "application/x-aim"; break;
                case ".aip": mime = "text/x-audiosoft-intra"; break;
                case ".ani": mime = "application/x-navi-animation"; break;
                case ".aos": mime = "application/x-nokia-9000-communicator-add-on-software"; break;
                case ".aps": mime = "application/mime"; break;
                case ".arc": mime = "application/octet-stream"; break;
                case ".arj": mime = "application/arj"; break;
                case ".art": mime = "image/x-jg"; break;
                case ".asf": mime = "video/x-ms-asf"; break;
                case ".asm": mime = "text/x-asm"; break;
                case ".asp": mime = "text/asp"; break;
                case ".asx": mime = "application/x-mplayer2"; break;
                case ".au": mime = "audio/basic"; break;
                case ".avi": mime = "video/avi"; break;
                case ".avs": mime = "video/avs-video"; break;
                case ".bcpio": mime = "application/x-bcpio"; break;
                case ".bin": mime = "application/octet-stream"; break;
                case ".bm": mime = "image/bmp"; break;
                case ".bmp": mime = "image/bmp"; break;
                case ".boo": mime = "application/book"; break;
                case ".book": mime = "application/book"; break;
                case ".boz": mime = "application/x-bzip2"; break;
                case ".bsh": mime = "application/x-bsh"; break;
                case ".bz": mime = "application/x-bzip"; break;
                case ".bz2": mime = "application/x-bzip2"; break;
                case ".c": mime = "text/plain"; break;
                case ".c++": mime = "text/plain"; break;
                case ".cc": mime = "text/plain"; break;
                case ".ccad": mime = "application/clariscad"; break;
                case ".cco": mime = "application/x-cocoa"; break;
                case ".cdf": mime = "application/cdf"; break;
                case ".cer": mime = "application/pkix-cert"; break;
                case ".cha": mime = "application/x-chat"; break;
                case ".chat": mime = "application/x-chat"; break;
                case ".class": mime = "application/java"; break;
                case ".com": mime = "application/octet-stream"; break;
                case ".conf": mime = "text/plain"; break;
                case ".cpio": mime = "application/x-cpio"; break;
                case ".cpp": mime = "text/x-c"; break;
                case ".cpt": mime = "application/x-compactpro"; break;
                case ".crl": mime = "application/pkcs-crl"; break;
                case ".crt": mime = "application/x-x509-ca-cert"; break;
                case ".csh": mime = "application/x-csh"; break;
                case ".css": mime = "text/css"; break;
                case ".cxx": mime = "text/plain"; break;
                case ".dcr": mime = "application/x-director"; break;
                case ".deepv": mime = "application/x-deepv"; break;
                case ".def": mime = "text/plain"; break;
                case ".der": mime = "application/x-x509-ca-cert"; break;
                case ".dif": mime = "video/x-dv"; break;
                case ".dir": mime = "application/x-director"; break;
                case ".dl": mime = "video/dl"; break;
                case ".doc": mime = "application/msword"; break;
                case ".dot": mime = "application/msword"; break;
                case ".dp": mime = "application/commonground"; break;
                case ".drw": mime = "application/drafting"; break;
                case ".dump": mime = "application/octet-stream"; break;
                case ".dv": mime = "video/x-dv"; break;
                case ".dvi": mime = "application/x-dvi"; break;
                case ".dwf": mime = "drawing/x-dwf (old)"; break;
                case ".dwg": mime = "application/acad"; break;
                case ".dxf": mime = "application/dxf"; break;
                case ".dxr": mime = "application/x-director"; break;
                case ".el": mime = "text/x-scriptcase"; break;
                case ".elc": mime = "application/x-bytecodecase"; break;
                case ".env": mime = "application/x-envoy"; break;
                case ".eps": mime = "application/postscript"; break;
                case ".es": mime = "application/x-esrehber"; break;
                case ".etx": mime = "text/x-setext"; break;
                case ".evy": mime = "application/envoy"; break;
                case ".exe": mime = "application/octet-stream"; break;
                case ".f": mime = "text/x-fortran"; break;
                case ".f77": mime = "text/x-fortran"; break;
                case ".f90": mime = "text/x-fortran"; break;
                case ".fif": mime = "image/fif"; break;
                case ".fli": mime = "video/fli"; break;
                case ".flo": mime = "image/florian"; break;
                case ".fmf": mime = "video/x-atomic3d-feature"; break;
                case ".for": mime = "text/x-fortran"; break;
                case ".frl": mime = "application/freeloader"; break;
                case ".funk": mime = "audio/make"; break;
                case ".g": mime = "text/plain"; break;
                case ".g3": mime = "image/g3fax"; break;
                case ".gif": mime = "image/gif"; break;
                case ".gl": mime = "video/gl"; break;
                case ".gsd": mime = "audio/x-gsm"; break;
                case ".gsm": mime = "audio/x-gsm"; break;
                case ".gsp": mime = "application/x-gsp"; break;
                case ".gss": mime = "application/x-gss"; break;
                case ".gtar": mime = "application/x-gtar"; break;
                case ".gz": mime = "application/x-compressed"; break;
                case ".gzip": mime = "application/x-gzip"; break;
                case ".h": mime = "text/plain"; break;
                case ".hdf": mime = "application/x-hdf"; break;
                case ".help": mime = "application/x-helpfile"; break;
                case ".hh": mime = "text/x-h"; break;
                case ".hlb": mime = "text/x-script"; break;
                case ".hlp": mime = "application/hlp"; break;
                case ".hqx": mime = "application/binhex"; break;
                case ".hta": mime = "application/hta"; break;
                case ".htc": mime = "text/x-component"; break;
                case ".htm": mime = "text/html"; break;
                case ".html": mime = "text/html"; break;
                case ".htmls": mime = "text/html"; break;
                case ".htt": mime = "text/webviewhtml"; break;
                case ".htx": mime = "text/html"; break;
                case ".ice": mime = "x-conference/x-cooltalk"; break;
                case ".ico": mime = "image/x-icon"; break;
                case ".idc": mime = "text/plain"; break;
                case ".ief": mime = "image/ief"; break;
                case ".iefs": mime = "image/ief"; break;
                case ".iges": mime = "application/iges"; break;
                case ".igs": mime = "application/iges"; break;
                case ".ima": mime = "application/x-ima"; break;
                case ".imap": mime = "application/x-httpd-imap"; break;
                case ".inf": mime = "application/inf"; break;
                case ".ins": mime = "application/x-internett-signup"; break;
                case ".ip": mime = "application/x-ip2"; break;
                case ".isu": mime = "video/x-isvideo"; break;
                case ".it": mime = "audio/it"; break;
                case ".iv": mime = "application/x-inventor"; break;
                case ".ivr": mime = "i-world/i-vrml"; break;
                case ".ivy": mime = "application/x-livescreen"; break;
                case ".jam": mime = "audio/x-jam"; break;
                case ".jav": mime = "text/x-java-source"; break;
                case ".java": mime = "text/x-java-source"; break;
                case ".jcm": mime = "application/x-java-commerce"; break;
                case ".jfif": mime = "image/jpeg"; break;
                case ".jfif-tbnl": mime = "image/jpeg"; break;
                case ".jpe": mime = "image/jpeg"; break;
                case ".jpeg": mime = "image/jpeg"; break;
                case ".jpg": mime = "image/jpeg"; break;
                case ".jps": mime = "image/x-jps"; break;
                case ".js": mime = "application/x-javascript"; break;
                case ".jut": mime = "image/jutvision"; break;
                case ".kar": mime = "audio/midi"; break;
                case ".ksh": mime = "application/x-ksh"; break;
                case ".la": mime = "audio/nspaudio"; break;
                case ".lam": mime = "audio/x-liveaudio"; break;
                case ".latex": mime = "application/x-latex"; break;
                case ".lha": mime = "application/lha"; break;
                case ".lhx": mime = "application/octet-stream"; break;
                case ".list": mime = "text/plain"; break;
                case ".lma": mime = "audio/nspaudio"; break;
                case ".log": mime = "text/plain"; break;
                case ".lsp": mime = "application/x-lisp"; break;
                case ".lst": mime = "text/plain"; break;
                case ".lsx": mime = "text/x-la-asf"; break;
                case ".ltx": mime = "application/x-latex"; break;
                case ".lzh": mime = "application/x-lzh"; break;
                case ".lzx": mime = "application/lzx"; break;
                case ".m": mime = "text/x-m"; break;
                case ".m1v": mime = "video/mpeg"; break;
                case ".m2a": mime = "audio/mpeg"; break;
                case ".m2v": mime = "video/mpeg"; break;
                case ".m3u": mime = "audio/x-mpequrl"; break;
                case ".man": mime = "application/x-troff-man"; break;
                case ".map": mime = "application/x-navimap"; break;
                case ".mar": mime = "text/plain"; break;
                case ".mbd": mime = "application/mbedlet"; break;
                case ".mc$": mime = "application/x-magic-cap-package-1case"; break;
                case ".mcd": mime = "application/mcad"; break;
                case ".mcf": mime = "image/vasa"; break;
                case ".mcp": mime = "application/netmc"; break;
                case ".me": mime = "application/x-troff-me"; break;
                case ".mht": mime = "message/rfc822"; break;
                case ".mhtml": mime = "message/rfc822"; break;
                case ".mid": mime = "audio/midi"; break;
                case ".midi": mime = "audio/midi"; break;
                case ".mif": mime = "application/x-frame"; break;
                case ".mime": mime = "message/rfc822"; break;
                case ".mjpg": mime = "video/x-motion-jpeg"; break;
                case ".mm": mime = "application/base64"; break;
                case ".mme": mime = "application/base64"; break;
                case ".mod": mime = "audio/mod"; break;
                case ".moov": mime = "video/quicktime"; break;
                case ".mov": mime = "video/quicktime"; break;
                case ".movie": mime = "video/x-sgi-movie"; break;
                case ".mp2": mime = "audio/mpeg"; break;
                case ".mp3": mime = "audio/mpeg3"; break;
                case ".mpa": mime = "audio/mpeg"; break;
                case ".mpc": mime = "application/x-project"; break;
                case ".mpe": mime = "video/mpeg"; break;
                case ".mpeg": mime = "video/mpeg"; break;
                case ".mpg": mime = "video/mpeg"; break;
                case ".mpga": mime = "audio/mpeg"; break;
                case ".mpt": mime = "application/x-project"; break;
                case ".mpv": mime = "application/x-project"; break;
                case ".mpx": mime = "application/x-project"; break;
                case ".mrc": mime = "application/marc"; break;
                case ".ms": mime = "application/x-troff-ms"; break;
                case ".mv": mime = "video/x-sgi-movie"; break;
                case ".my": mime = "audio/make"; break;
                case ".nap": mime = "image/naplps"; break;
                case ".naplps": mime = "image/naplps"; break;
                case ".nc": mime = "application/x-netcdf"; break;
                case ".nif": mime = "image/x-niff"; break;
                case ".niff": mime = "image/x-niff"; break;
                case ".nix": mime = "application/x-mix-transfer"; break;
                case ".nsc": mime = "application/x-conference"; break;
                case ".nvd": mime = "application/x-navidoc"; break;
                case ".o": mime = "application/octet-stream"; break;
                case ".oda": mime = "application/oda"; break;
                case ".omc": mime = "application/x-omc"; break;
                case ".omcd": mime = "application/x-omcdatamaker"; break;
                case ".omcr": mime = "application/x-omcregerator"; break;
                case ".p": mime = "text/x-pascal"; break;
                case ".p10": mime = "application/pkcs10"; break;
                case ".p12": mime = "application/pkcs-12"; break;
                case ".p7a": mime = "application/x-pkcs7-signature"; break;
                case ".p7c": mime = "application/pkcs7-mime"; break;
                case ".p7m": mime = "application/pkcs7-mime"; break;
                case ".p7r": mime = "application/x-pkcs7-certreqresp"; break;
                case ".p7s": mime = "application/pkcs7-signature"; break;
                case ".part": mime = "application/pro_eng"; break;
                case ".pas": mime = "text/pascal"; break;
                case ".pbm": mime = "image/x-portable-bitmap"; break;
                case ".pcl": mime = "application/x-pcl"; break;
                case ".pct": mime = "image/x-pict"; break;
                case ".pcx": mime = "image/x-pcx"; break;
                case ".pdb": mime = "chemical/x-pdb"; break;
                case ".pdf": mime = "application/pdf"; break;
                case ".pfunk": mime = "audio/make"; break;
                case ".pgm": mime = "image/x-portable-graymap"; break;
                case ".pic": mime = "image/pict"; break;
                case ".pict": mime = "image/pict"; break;
                case ".pkg": mime = "application/x-newton-compatible-pkg"; break;
                case ".pl": mime = "text/plain"; break;
                case ".plx": mime = "application/x-pixclscript"; break;
                case ".pm": mime = "image/x-xpixmap"; break;
                case ".pm4": mime = "application/x-pagemaker"; break;
                case ".pm5": mime = "application/x-pagemaker"; break;
                case ".png": mime = "image/png"; break;
                case ".pnm": mime = "image/x-portable-anymap"; break;
                case ".pot": mime = "application/mspowerpoint"; break;
                case ".pov": mime = "model/x-pov"; break;
                case ".ppa": mime = "application/mspowerpoint"; break;
                case ".ppm": mime = "image/x-portable-pixmap"; break;
                case ".pps": mime = "application/mspowerpoint"; break;
                case ".ppt": mime = "application/mspowerpoint"; break;
                case ".ppz": mime = "application/mspowerpoint"; break;
                case ".pre": mime = "application/x-freelance"; break;
                case ".prt": mime = "application/pro_eng"; break;
                case ".ps": mime = "application/postscript"; break;
                case ".psd": mime = "application/octet-stream"; break;
                case ".pvu": mime = "paleovu/x-pv"; break;
                case ".pwz": mime = "application/mspowerpoint"; break;
                case ".py": mime = "text/html"; break;
                case ".pyc": mime = "applicaiton/x-bytecodecase"; break;
                case ".qd3": mime = "x-world/x-3dmf"; break;
                case ".qd3d": mime = "x-world/x-3dmf"; break;
                case ".qif": mime = "image/x-quicktime"; break;
                case ".qt": mime = "video/quicktime"; break;
                case ".qtc": mime = "video/x-qtc"; break;
                case ".qti": mime = "image/x-quicktime"; break;
                case ".qtif": mime = "image/x-quicktime"; break;
                case ".ra": mime = "audio/x-pn-realaudio"; break;
                case ".ram": mime = "audio/x-pn-realaudio"; break;
                case ".ras": mime = "image/cmu-raster"; break;
                case ".rast": mime = "image/cmu-raster"; break;
                case ".rgb": mime = "image/x-rgb"; break;
                case ".rm": mime = "audio/x-pn-realaudio"; break;
                case ".rmi": mime = "audio/mid"; break;
                case ".rmm": mime = "audio/x-pn-realaudio"; break;
                case ".rmp": mime = "audio/x-pn-realaudio"; break;
                case ".rng": mime = "application/ringing-tones"; break;
                case ".roff": mime = "application/x-troff"; break;
                case ".rpm": mime = "audio/x-pn-realaudio-plugin"; break;
                case ".rt": mime = "text/richtext"; break;
                case ".rtf": mime = "application/rtf"; break;
                case ".rtx": mime = "text/richtext"; break;
                case ".s": mime = "text/x-asm"; break;
                case ".s3m": mime = "audio/s3m"; break;
                case ".saveme": mime = "application/octet-stream"; break;
                case ".sbk": mime = "application/x-tbook"; break;
                case ".scm": mime = "video/x-scm"; break;
                case ".sdml": mime = "text/plain"; break;
                case ".sdp": mime = "application/sdp"; break;
                case ".sdr": mime = "application/sounder"; break;
                case ".sea": mime = "application/sea"; break;
                case ".set": mime = "application/set"; break;
                case ".sgm": mime = "text/sgml"; break;
                case ".sgml": mime = "text/sgml"; break;
                case ".sh": mime = "application/x-bsh"; break;
                case ".shar": mime = "application/x-bsh"; break;
                case ".shtml": mime = "text/html"; break;
                case ".sid": mime = "audio/x-psid"; break;
                case ".sit": mime = "application/x-sit"; break;
                case ".skd": mime = "application/x-koan"; break;
                case ".skm": mime = "application/x-koan"; break;
                case ".skp": mime = "application/x-koan"; break;
                case ".skt": mime = "application/x-koan"; break;
                case ".sl": mime = "application/x-seelogo"; break;
                case ".smi": mime = "application/smil"; break;
                case ".smil": mime = "application/smil"; break;
                case ".snd": mime = "audio/basic"; break;
                case ".sol": mime = "application/solids"; break;
                case ".spc": mime = "application/x-pkcs7-certificates"; break;
                case ".spl": mime = "application/futuresplash"; break;
                case ".spr": mime = "application/x-sprite"; break;
                case ".sprite": mime = "application/x-sprite"; break;
                case ".src": mime = "application/x-wais-source"; break;
                case ".ssi": mime = "text/x-server-parsed-html"; break;
                case ".ssm": mime = "application/streamingmedia"; break;
                case ".step": mime = "application/step"; break;
                case ".stl": mime = "application/sla"; break;
                case ".stp": mime = "application/step"; break;
                case ".sv4cpio": mime = "application/x-sv4cpio"; break;
                case ".sv4crc": mime = "application/x-sv4crc"; break;
                case ".svf": mime = "image/x-dwg"; break;
                case ".svr": mime = "application/x-world"; break;
                case ".swf": mime = "application/x-shockwave-flash"; break;
                case ".t": mime = "application/x-troff"; break;
                case ".talk": mime = "text/x-speech"; break;
                case ".tar": mime = "application/x-tar"; break;
                case ".tbk": mime = "application/toolbook"; break;
                case ".tcl": mime = "application/x-tcl"; break;
                case ".tex": mime = "application/x-tex"; break;
                case ".texi": mime = "application/x-texinfo"; break;
                case ".texinfo": mime = "application/x-texinfo"; break;
                case ".text": mime = "text/plain"; break;
                case ".tgz": mime = "application/gnutar"; break;
                case ".tif": mime = "image/tiff"; break;
                case ".tiff": mime = "image/tiff"; break;
                case ".tr": mime = "application/x-troff"; break;
                case ".tsi": mime = "audio/tsp-audio"; break;
                case ".tsp": mime = "audio/tsplayer"; break;
                case ".tsv": mime = "text/tab-separated-values"; break;
                case ".turbot": mime = "image/florian"; break;
                case ".txt": mime = "text/plain"; break;
                case ".uil": mime = "text/x-uil"; break;
                case ".uni": mime = "text/uri-list"; break;
                case ".unis": mime = "text/uri-list"; break;
                case ".unv": mime = "application/i-deas"; break;
                case ".uri": mime = "text/uri-list"; break;
                case ".uris": mime = "text/uri-list"; break;
                case ".ustar": mime = "application/x-ustar"; break;
                case ".uu": mime = "text/x-uuencode"; break;
                case ".uue": mime = "text/x-uuencode"; break;
                case ".vcd": mime = "application/x-cdlink"; break;
                case ".vcs": mime = "text/x-vcalendar"; break;
                case ".vda": mime = "application/vda"; break;
                case ".vdo": mime = "video/vdo"; break;
                case ".vew": mime = "application/groupwise"; break;
                case ".viv": mime = "video/vivo"; break;
                case ".vivo": mime = "video/vivo"; break;
                case ".vmd": mime = "application/vocaltec-media-desc"; break;
                case ".vmf": mime = "application/vocaltec-media-file"; break;
                case ".voc": mime = "audio/voc"; break;
                case ".vos": mime = "video/vosaic"; break;
                case ".vox": mime = "audio/voxware"; break;
                case ".vqe": mime = "audio/x-twinvq-plugin"; break;
                case ".vqf": mime = "audio/x-twinvq"; break;
                case ".vql": mime = "audio/x-twinvq-plugin"; break;
                case ".vrml": mime = "model/vrml"; break;
                case ".vrt": mime = "x-world/x-vrt"; break;
                case ".vsd": mime = "application/x-visio"; break;
                case ".vst": mime = "application/x-visio"; break;
                case ".vsw": mime = "application/x-visio"; break;
                case ".w6w": mime = "application/msword"; break;
                case ".wav": mime = "audio/wav"; break;
                case ".wb1": mime = "application/x-qpro"; break;
                case ".wiz": mime = "application/msword"; break;
                case ".wk1": mime = "application/x-123"; break;
                case ".wmf": mime = "windows/metafile"; break;
                case ".word": mime = "application/msword"; break;
                case ".wp": mime = "application/wordperfect"; break;
                case ".wp5": mime = "application/wordperfect"; break;
                case ".wp6": mime = "application/wordperfect"; break;
                case ".wpd": mime = "application/wordperfect"; break;
                case ".wq1": mime = "application/x-lotus"; break;
                case ".wri": mime = "application/mswrite"; break;
                case ".wrl": mime = "model/vrml"; break;
                case ".wrz": mime = "model/vrml"; break;
                case ".wsc": mime = "text/scriplet"; break;
                case ".wsrc": mime = "application/x-wais-source"; break;
                case ".wtk": mime = "application/x-wintalk"; break;
                case ".xbm": mime = "image/x-xbitmap"; break;
                case ".xdr": mime = "video/x-amt-demorun"; break;
                case ".xgz": mime = "xgl/drawing"; break;
                case ".xl": mime = "application/excel"; break;
                case ".xla": mime = "application/excel"; break;
                case ".xlb": mime = "application/excel"; break;
                case ".xlc": mime = "application/excel"; break;
                case ".xld": mime = "application/excel"; break;
                case ".xlk": mime = "application/excel"; break;
                case ".xll": mime = "application/excel"; break;
                case ".xlm": mime = "application/excel"; break;
                case ".xls": mime = "application/excel"; break;
                case ".xlt": mime = "application/excel"; break;
                case ".xlv": mime = "application/excel"; break;
                case ".xlw": mime = "application/excel"; break;
                case ".xm": mime = "audio/xm"; break;
                case ".xml": mime = "text/xml"; break;
                case ".xmz": mime = "xgl/movie"; break;
                case ".xpm": mime = "image/xpm"; break;
                case ".x-png": mime = "image/png"; break;
                case ".xsr": mime = "video/x-amt-showrun"; break;
                case ".xwd": mime = "image/x-xwd"; break;
                case ".z": mime = "application/x-compressed"; break;
                case ".zip": mime = "application/x-compressed"; break;
                case ".zoo": mime = "application/octet-stream"; break;

                // Office 2007 document file types (source: http://www.webdeveloper.com/forum/showthread.php?t=162526)
                case ".docm": mime = "application/vnd.ms-word.document.macroEnabled.12"; break;
                case ".docx": mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"; break;
                case ".dotm": mime = "application/vnd.ms-word.template.macroEnabled.12"; break;
                case ".dotx": mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.template"; break;
                case ".potm": mime = "application/vnd.ms-powerpoint.template.macroEnabled.12"; break;
                case ".potx": mime = "application/vnd.openxmlformats-officedocument.presentationml.template"; break;
                case ".ppam": mime = "application/vnd.ms-powerpoint.addin.macroEnabled.12"; break;
                case ".ppsm": mime = "application/vnd.ms-powerpoint.slideshow.macroEnabled.12"; break;
                case ".ppsx": mime = "application/vnd.openxmlformats-officedocument.presentationml.slideshow"; break;
                case ".pptm": mime = "application/vnd.ms-powerpoint.presentation.macroEnabled.12"; break;
                case ".pptx": mime = "application/vnd.openxmlformats-officedocument.presentationml.presentation"; break;
                case ".xlam": mime = "application/vnd.ms-excel.addin.macroEnabled.12"; break;
                case ".xlsb": mime = "application/vnd.ms-excel.sheet.binary.macroEnabled.12"; break;
                case ".xlsm": mime = "application/vnd.ms-excel.sheet.macroEnabled.12"; break;
                case ".xlsx": mime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"; break;
                case ".xltm": mime = "application/vnd.ms-excel.template.macroEnabled.12"; break;
                case ".xltx": mime = "application/vnd.openxmlformats-officedocument.spreadsheetml.template"; break;

                // Google Earth Data files
                case ".kml": mime = "application/vnd.google-earth.kml+xml"; break;
                case ".kmz": mime = "application/vnd.google-earth.kmz"; break;

                // Open Office : http://framework.openoffice.org/documentation/mimetypes/mimetypes.html
                case ".odt": mime = "application/vnd.oasis.opendocument.text "; break; // OpenDocument Text
                case ".ott": mime = "application/vnd.oasis.opendocument.text-template"; break; // OpenDocument Text Template
                case ".oth": mime = "application/vnd.oasis.opendocument.text-web"; break; // HTML Document Template
                case ".odm": mime = "application/vnd.oasis.opendocument.text-master"; break; // OpenDocument Master Document
                case ".odg": mime = "application/vnd.oasis.opendocument.graphics"; break; // OpenDocument Drawing
                case ".otg": mime = "application/vnd.oasis.opendocument.graphics-template"; break; // OpenDocument Drawing Template
                case ".odp": mime = "application/vnd.oasis.opendocument.presentation"; break; // OpenDocument Presentation
                case ".otp": mime = "application/vnd.oasis.opendocument.presentation-template"; break; // OpenDocument Presentation Template
                case ".ods": mime = "application/vnd.oasis.opendocument.spreadsheet"; break; // OpenDocument Spreadsheet
                case ".ots": mime = "application/vnd.oasis.opendocument.spreadsheet-template"; break; // OpenDocument Spreadsheet Template
                case ".odc": mime = "application/vnd.oasis.opendocument.chart"; break; // OpenDocument Chart
                case ".odf": mime = "application/vnd.oasis.opendocument.formula"; break; // OpenDocument Formula
                case ".odb": mime = "application/vnd.oasis.opendocument.database"; break; // OpenDocument Database
                case ".odi": mime = "application/vnd.oasis.opendocument.image"; break; // OpenDocument Image
                case ".oxt": mime = "application/vnd.openofficeorg.extension"; break; // OpenOffice.org extension (since OOo 2.1)

                #endregion
                default: mime = ""; break;
            } // switch
			return mime;
		}

        public static string getHiddenInputHtml(string FormName, string id, string val)
		{
			string html = "<input type=\"hidden\" name=\""+FormName+"\" value=\""+val+"\" id=\""+id+"\" />";
			return html;
		}

        public static string getHiddenInputHtml(string FormName, int val)
        {
            return getHiddenInputHtml(FormName, val.ToString());
        }

		public static string getHiddenInputHtml(string FormName, string val)
		{
			string html = "<input type=\"hidden\" name=\""+FormName+"\" value=\""+val+"\" />";
			return html;
		}

        public static string getTextAreaHtml(string FormName, string id, string contents, int cols, int rows)
        {
            string html = "<textarea name=\"" + FormName + "\" id=\"" + id + "\" rows=\"" + rows.ToString() + "\" cols=\"" + cols.ToString() + "\">" + contents + "</textarea>";            
            return html;
        }

        /// <summary>
        /// Create a HTML TEXTAREA tag string
        /// </summary>
        /// <param name="FormName"></param>
        /// <param name="id"></param>
        /// <param name="val"></param>
        /// <param name="size"></param>
        /// <param name="maxLength"></param>
        /// <param name="cssClass"></param>
        /// <returns></returns>
        public static string getTextAreaHtml(string FormName, string id, string contents, int cols, int rows, string cssClass)
        {
            StringBuilder sb = new StringBuilder("<textarea");
            sb.Append(" name=\"" + FormName + "\"");
            sb.Append(" id=\"" + id + "\"");
            sb.Append(" rows=\"" + rows.ToString() + "\"");
            sb.Append(" cols=\"" + cols.ToString() + "\"");

            if (cssClass != "")
                sb.Append(" class=\"" + cssClass + "\"");

            sb.Append(">");
            sb.Append(contents);
            sb.Append("</textarea>");
            return sb.ToString();
        }
        
        public static string getInputTextHtml(string FormName, string id, string val, int size, int maxLength)
		{
			string html = "<input type=\"text\" id=\""+id+"\" name=\""+FormName+"\" value=\""+val+"\" size=\""+size.ToString()+"\" maxlength=\""+maxLength.ToString()+"\" />";
			return html;
		}

        /// <summary>
        /// Create a HTML INPUT tag string
        /// </summary>
        /// <param name="FormName"></param>
        /// <param name="id"></param>
        /// <param name="val"></param>
        /// <param name="size"></param>
        /// <param name="maxLength"></param>
        /// <param name="cssClass"></param>
        /// <param name="eventArray"></param>
        /// <returns></returns>
        public static string getInputTextHtml(string FormName, string id, string val, int size, int maxLength, string cssClass, JavascriptEvent[] eventArray)
        {
            StringBuilder sb = new StringBuilder("<input type=\"text\"");
            sb.Append(" id=\"" + id + "\"");
            sb.Append(" name=\"" + FormName + "\"");
            sb.Append(" value=\"" + val + "\"");
            sb.Append(" size=\"" + size.ToString() + "\"");
            sb.Append(" maxlength=\"" + maxLength.ToString() + "\"");

            if (cssClass != "") 
                sb.Append(" class=\"" + cssClass + "\"");

            foreach (JavascriptEvent ev in eventArray)
                sb.Append(" " + ev.EventName.ToString() + "=\"" + ev.EventCode + "\"");

            sb.Append(" />");

            return sb.ToString();
        }

        public static string getCheckboxHtml(string displayText, string FormName, string id, string val, bool check)
		{
            return getCheckboxHtml(displayText, FormName, id, val, check, "");
        }

        public static string getCheckboxHtml(string displayText, string FormName, string id, string val, bool check, string onClick)
        {
            return getCheckboxHtml(displayText, FormName, id, val, check, "", false);
        }

        public static string getCheckboxHtml(string displayText, string FormName, string id, string val, bool check, string onClick, bool disabled)
        {
            return getCheckboxHtml(displayText, FormName, id, val, check, onClick, disabled, "");
        }

        public static string getCheckboxHtml(string displayText, string FormName, string id, string val, bool check, string onClick, bool disabled, string onkeydown)
		{
            StringBuilder html = new StringBuilder("<input type=\"checkbox\" id=\"" + id + "\" name=\"" + FormName + "\" value=\"" + val + "\" ");
			if (check)
				html.Append(" checked=\"checked\"");
            if (disabled)
                html.Append(" disabled=\"disabled\"");
            if (onClick != "")
                html.Append(" onClick=\""+onClick+"\"");
            if (onkeydown != "")
                html.Append(" onkeydown=\"" + onkeydown + "\"");
            html.Append(" />");
            if (displayText != "")
                html.Append("<label for=\"" + id + "\">" + displayText + "</label>");
			return html.ToString();
		}

        public static string getRadioButtonHtml(string displayText, string FormName, string id, string val, bool check)
        {
            return getRadioButtonHtml(displayText, FormName, id, val, check, "");
        }

		public static string getRadioButtonHtml(string displayText, string FormName, string id, string val, bool check, string onClick)
		{
			string doClick = "";
			if (onClick != "")
				doClick = " onClick=\""+onClick+"\" ";
			string html = "";
			html = html + "<input type=\"radio\" id=\""+id+"\" name=\""+FormName+"\" value=\""+val+"\" "+doClick;
			if (check)
				html = html + "checked ";
			html = html +" />";
			html = html + "<label for=\""+id+"\">"+displayText+"</label>";
			return html;
		}

        public static string getRadioListHtml(string FormName, string idBase, string[] options, string selectedValue, string onClick, string seperator)
        {
            int id_count = 1;
            StringBuilder sb = new StringBuilder();
            foreach (string key in options)
            {
                string val = System.Web.HttpContext.Current.Server.HtmlEncode(key);
                bool sel = false;
                if (selectedValue == key)
                    sel = true;

                sb.Append(getRadioButtonHtml(val, FormName, idBase + id_count.ToString(), key, sel, onClick));
                sb.Append(seperator);

                id_count++;
            }
            return sb.ToString();
        }
        
        public static string getRadioListHtml(string FormName, string idBase, NameValueCollection options, string selectedValue, string onClick, string seperator)
		{
			int id_count = 1;
			StringBuilder sb = new StringBuilder();
			foreach(string key in options.Keys)
			{
				string val = System.Web.HttpContext.Current.Server.HtmlEncode(options[key]);
				bool sel = false;
				if (selectedValue == key)
					sel = true;
				
				sb.Append(getRadioButtonHtml(val, FormName, idBase+id_count.ToString(), key, sel, onClick));
				sb.Append(seperator);

				id_count++;
			}
			return sb.ToString();

		}

        public static string getDropDownHtml(string FormName, string id, System.Type enumType, object selectedValue)
        {
            return getDropDownHtml(FormName, id, Enum.GetNames(enumType), Enum.GetName(enumType,selectedValue), "");
        }


		/// <summary>
		/// the key is the dropdown's value, options[key] is the display text
		/// </summary>
		/// <param name="FormName"></param>
		/// <param name="id"></param>
		/// <param name="options"></param>
		/// <param name="selectedValue"></param>
		/// <returns></returns>
        public static string getDropDownHtml(string FormName, string id, NameValueCollection options, string selectedValue)
		{
			return getDropDownHtml(FormName, id, options, selectedValue, "", "");
		}

        /// <summary>
        /// the key is the dropdown's value, options[key] is the display text
        /// </summary>
        /// <param name="FormName"></param>
        /// <param name="id"></param>
        /// <param name="options"></param>
        /// <param name="selectedValue"></param>
        /// <param name="onChangeEvent"></param>
        /// <param name="cssClassName"></param>
        /// <returns></returns>
		public static string getDropDownHtml(string FormName, string id, NameValueCollection options, string selectedValue, string onChangeEvent, string cssClassName)
		{
			string onChange = "";
			if (onChangeEvent != "")
			{
                onChange = " onChange=\"" + onChangeEvent + "\" ";
			}

            string cssClass = "";
            if (cssClassName != "")
                cssClass = " class=\"" + cssClassName + "\" ";

            StringBuilder html = new StringBuilder();
            html.Append("<select name=\"" + FormName + "\" " + onChange + cssClass + " id=\"" + id + "\" >");
			foreach(string key in options.Keys)
			{
				string val = System.Web.HttpContext.Current.Server.HtmlEncode(options[key]);
				if (selectedValue == key)
				{
                    html.Append("<option value=\"" + key + "\" selected=\"selected\" > " + val + " </option>" + Environment.NewLine);
				}
				else
				{
                    html.Append("<option value=\"" + key + "\"> " + val + " </option>" + Environment.NewLine);
				}
			} // foreach
			html.Append("</select>");
			return html.ToString();
		}

		public static string getDropDownHtml(string FormName, string id, string[] options, string selectedValue)
		{
			NameValueCollection nvcOptions = new NameValueCollection();
			foreach(string o in options)
			{
				nvcOptions.Add(o,o);
			}
			return getDropDownHtml(FormName,id,nvcOptions,selectedValue);
		}

		public static string getDropDownHtml(string FormName, string id, string[] options, string selectedValue, string onChangeEvent)
		{
			NameValueCollection nvcOptions = new NameValueCollection();
			foreach(string o in options)
			{
				nvcOptions.Add(o,o);
			}
			return getDropDownHtml(FormName,id,nvcOptions,selectedValue, onChangeEvent, "");
		}

        public static string getDropDownHtml(string FormName, string id, string[] options, string selectedValue, string onChangeEvent, string cssClassName)
        {
            NameValueCollection nvcOptions = new NameValueCollection();
            foreach (string o in options)
            {
                nvcOptions.Add(o, o);
            }
            return getDropDownHtml(FormName, id, nvcOptions, selectedValue, onChangeEvent, cssClassName);
        }

		public static string StripTags(string text)
		{
			return Regex.Replace(text, @"<(.|\n)*?>", string.Empty);
		}

        /// <summary>
        /// Renders a UserControl to a string
        /// </summary>
        /// <param name="context">The current HttpContext</param>
        /// <param name="controlPath">the URL to the ASCX file (eg "/hatCms/Controls/Header.ascx")</param>
        /// <returns></returns>
        public static string RenderUserControl(System.Web.HttpContext context, System.Web.UI.UserControl control)
        {

            // modified from: http://jamesewelch.wordpress.com/2008/07/11/how-to-render-a-aspnet-user-control-within-a-web-service-and-return-the-generated-html/

            if (context.Session == null)
                throw new ArgumentException("context.Session is null - ensure that your handler (ie ASHX page) instantiates IRequiresSessionState");

            System.Web.UI.Page pageHolder = new System.Web.UI.Page();

            pageHolder.Controls.Add(control);

            System.IO.StringWriter output = new System.IO.StringWriter();
            context.Server.Execute(pageHolder, output, true);
            return output.ToString();
        }

        /// <summary>
        /// Renders a UserControl to a string
        /// </summary>
        /// <param name="context">The current HttpContext</param>
        /// <param name="controlPath">the URL to the ASCX file (eg "/hatCms/Controls/Header.ascx")</param>
        /// <returns></returns>
        public static string RenderUserControl(System.Web.HttpContext context, string controlPath)
        {

            if (context.Session == null)
                throw new ArgumentException("context.Session is null - ensure that your handler (ie ASHX page) instantiates IRequiresSessionState");

            System.Web.UI.Page pageHolder = new System.Web.UI.Page();

            
            System.Web.UI.UserControl viewControl = (System.Web.UI.UserControl)pageHolder.LoadControl(controlPath);

            return RenderUserControl(context, viewControl);

        }

        public static bool ClientIsMakingOfflineVersion
        {
            get
            {
                if (System.Web.HttpContext.Current != null)
                {
                    HttpContext _context = System.Web.HttpContext.Current;
                    if (_context.Request.ServerVariables["HTTP_USER_AGENT"] != null &&
                        _context.Request.ServerVariables["HTTP_USER_AGENT"].IndexOf("HTTrack") > -1)
                        return true;
                    else
                        return false;
                }
                return false;
            }
        }

        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        public static string getOnloadJavascript(string onLoadJSFunctionName)
        {
            StringBuilder js = new StringBuilder();
            string EOL = Environment.NewLine;
            js.Append("			if( window.addEventListener ) {" + EOL);
            js.Append("				window.addEventListener( 'load', " + onLoadJSFunctionName + ", false );" + EOL);
            js.Append("			} else if( document.addEventListener ) {" + EOL);
            js.Append("				document.addEventListener('load' , " + onLoadJSFunctionName + ", false );" + EOL);
            js.Append("			} else if( window.attachEvent ) {" + EOL);
            js.Append("				window.attachEvent( 'onload', " + onLoadJSFunctionName + " );" + EOL);
            js.Append("			} else {" + EOL);
            js.Append("				window.onload = " + onLoadJSFunctionName + ";" + EOL);
            js.Append("			}" + EOL);
            return js.ToString();
        } // getOnloadJavascript

	}
}
