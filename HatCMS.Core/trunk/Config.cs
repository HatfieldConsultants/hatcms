using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using Hatfield.Web.Portal;
using System.Globalization;

namespace HatCMS
{
    public class CmsConfig
    {
        #region Configuration Functions
        /// <summary>
        /// gets a configuration item. Configuration is stored in the web.config file in the section "Hatfield_Cms"
        /// </summary>
        private static NameValueCollection Config
        {
            get
            {
                NameValueCollection ret = (NameValueCollection)ConfigurationManager.GetSection("Hatfield_Cms");
                return ret;
            }
        }

        public static string getConfigValue(string key, string defaultValue)
        {
            if (Config != null && Config[key] != null && Config[key] != "")
            {
                return Config[key];
            }
            return defaultValue;
        }

        /// <summary>
        /// get a config value from a multi-language setting string.
        /// The format of the string is:
        /// [ key = "key_1" value = "language_1|language_2" ] where "|" is the separator.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static string getConfigValue(string key, string defaultValue, CmsLanguage lang)
        {
            string[] msgArray = getConfigValue(key, defaultValue.ToString()).Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            CmsLanguage[] langArray = CmsConfig.Languages;
            int x = CmsLanguage.IndexOf(lang.shortCode, langArray);

            if (msgArray.Length < langArray.Length || x < 0)
                return defaultValue;

            return msgArray[x];
        }

        public static bool getConfigValue(string key, bool defaultValue)
        {
            string s = getConfigValue(key, defaultValue.ToString());
            try
            {
                return Convert.ToBoolean(s);
            }
            catch
            { }
            return defaultValue;
        }

        /// <summary>
        /// get a config value from a multi-language setting string.
        /// The format of the string is:
        /// [ key = "key_1" value = "language_1|language_2" ] where "|" is the separator.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static bool getConfigValue(string key, bool defaultValue, CmsLanguage lang)
        {
            string[] msgArray = getConfigValue(key, defaultValue.ToString()).Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            CmsLanguage[] langArray = CmsConfig.Languages;
            int x = CmsLanguage.IndexOf(lang.shortCode, langArray);

            if (msgArray.Length < langArray.Length || x < 0)
                return defaultValue;

            try
            {
                return Convert.ToBoolean(msgArray[x]);
            }
            catch { }
            return defaultValue;
        }


        public static int getConfigValue(string key, int defaultValue)
        {
            string s = getConfigValue(key, defaultValue.ToString());
            try
            {
                return Convert.ToInt32(s);
            }
            catch
            { }
            return defaultValue;
        }

        /// <summary>
        /// get a config value from a multi-language setting string.
        /// The format of the string is:
        /// [ key = "key_1" value = "language_1|language_2" ] where "|" is the separator.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static int getConfigValue(string key, int defaultValue, CmsLanguage lang)
        {
            string[] msgArray = getConfigValue(key, defaultValue.ToString()).Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            CmsLanguage[] langArray = CmsConfig.Languages;
            int x = CmsLanguage.IndexOf(lang.shortCode, langArray);

            if (msgArray.Length < langArray.Length || x < 0)
                return defaultValue;

            try
            {
                return Convert.ToInt32(msgArray[x]);
            }
            catch { }
            return defaultValue;
        }

        #endregion 

        /// <summary>
        /// the template engine to use for rendering pages
        /// </summary>
        public static CmsTemplateEngineVersion TemplateEngineVersion
        {
            get 
            {
                return CmsTemplateEngineVersion.v2;                
            }

        }

        /// <summary>
        /// a list of URLs (no leading slash or app path) that the remapping engine should not process.
        /// filenames not handled by the ASP.Net runtime (such as .js or .htm files) do not need to be listed here.
        /// However, any file that you want to execute on its own which normally runs through the .Net runtime needs to be listed
        /// here. These include *.aspx, *.ashx files.
        /// </summary>
        public static string[] URLsToNotRemap
        {
            get
            {
                string toSplit = getConfigValue("URLsToNotRemap", "");
                return toSplit.Split(new char[] { CmsConfig.PerLanguageConfigSplitter }, StringSplitOptions.RemoveEmptyEntries);                                
            }
        } // URLsToNotRemap


        public enum DateInputFormat { DayMonthYear, YearMonthDay, MonthDayYear };

        /// <summary>
        /// must be a format recognized by DateTimeFormatInfo.
        /// Note: case matters!
        /// </summary>
        public static DateInputFormat dateInputFormat
        {
            get { return (DateInputFormat)Enum.Parse(typeof(DateInputFormat), CmsConfig.getConfigValue("Calendar.DateInputFormat", "MonthDayYear")); }
        }

        /// <summary>
        /// must be a format recognized by DateTimeFormatInfo.
        /// Note: case matters!
        /// </summary>
        public static string InputDateTimeFormatInfo
        {
            get
            {
                string ret = "MM/dd/yyyy";
                switch (dateInputFormat)
                {
                    case DateInputFormat.DayMonthYear: ret = "dd/MM/yyyy"; break;
                    case DateInputFormat.YearMonthDay: ret = "yyyy/MM/dd"; break;
                    case DateInputFormat.MonthDayYear: ret = "MM/dd/yyyy"; break;
                }
                return ret;
            }
        }

        public static string InputDateTimeDatePickerFormat
        {
            get
            {
                string ret = "mdy";
                switch (dateInputFormat)
                {
                    case DateInputFormat.DayMonthYear: ret = "dmy"; break;
                    case DateInputFormat.YearMonthDay: ret = "ymd"; break;
                    case DateInputFormat.MonthDayYear: ret = "mdy"; break;
                }
                return ret;
            }
        }

        public static DateTime parseDateInDateInputFormat(string dateStr, DateTime returnOnErrorOrInvalid)
        {
            try
            {
                int m = -1; int d = -1; int y = -1;

                dateStr = dateStr.Trim();
                string[] dateParts = dateStr.Split(new char[] { '/', '.', '-', ' ' });

                int mIndex = 0;
                int dIndex = 1;
                int yIndex = 2;

                switch (dateInputFormat)
                {
                    case DateInputFormat.DayMonthYear:
                        dIndex = 0; mIndex = 1; yIndex = 2;
                        break;
                    case DateInputFormat.MonthDayYear:
                        dIndex = 1; mIndex = 2; yIndex = 2;
                        break;
                    case DateInputFormat.YearMonthDay:
                        dIndex = 2; mIndex = 1; yIndex = 0;
                        break;
                    default:
                        throw new ArgumentException("Invalid DateInputFormat found!");
                }


                m = Convert.ToInt32(dateParts[mIndex]);
                d = Convert.ToInt32(dateParts[dIndex]);
                y = Convert.ToInt32(dateParts[yIndex]);

                return new DateTime(y, m, d);
            }
            catch { }
            return returnOnErrorOrInvalid;
        }

        /// <summary>
        /// the character used to split config entry values that are defined on a per-language basis.
        /// </summary>
        public static char PerLanguageConfigSplitter
        {
            get
            {
                return '|';
            }
        }

        /// <summary>
        /// There must always be at least one entry in the Languages array. 
        /// </summary>
        public static CmsLanguage[] Languages
        {
            get
            {
                string cacheKey = "hatCmsConfig.Languages";
                if (PerRequestCache.CacheContains(cacheKey))
                {
                    return (CmsLanguage[])PerRequestCache.GetFromCache(cacheKey, new CmsLanguage[0] );
                }
                string toSplit = getConfigValue("languages", "");
                string[] parts = toSplit.Split(new char[] { PerLanguageConfigSplitter }, StringSplitOptions.RemoveEmptyEntries);
                List<CmsLanguage> ret = new List<CmsLanguage>();
                List<string> prevCodes = new List<string>();
                foreach (string langCode in parts)
                {
                    string c = langCode.Trim().ToLower().Split(new char[]{'-'})[0];
                    if (c != "" && prevCodes.IndexOf(c) < 0)
                    {
                        ret.Add(new CmsLanguage(c));
                        prevCodes.Add(c);
                    }
                } // foreach
                CmsLanguage[] arr = ret.ToArray();

                if (arr.Length < 1)
                    throw new ArgumentException("Error: you need to have at least one language defined!");                    

                PerRequestCache.AddToCache(cacheKey, arr);
                return arr;
            }
        }

        /// <summary>
        /// There must be at least one language / culture info in web.config (e.g. en-US)
        /// http://msdn.microsoft.com/en-us/library/kx54z3k7%28v=VS.80%29.aspx
        /// </summary>
        public static CultureInfo[] CultureInformation
        {
            get
            {
                string cacheKey = "hatCmsConfig.CultureInformation";
                if (PerRequestCache.CacheContains(cacheKey))
                {
                    return (CultureInfo[])PerRequestCache.GetFromCache(cacheKey, new CultureInfo[0]);
                }
                string toSplit = getConfigValue("languages", "");
                string[] parts = toSplit.Split(new char[] { PerLanguageConfigSplitter }, StringSplitOptions.RemoveEmptyEntries);
                List<CultureInfo> ret = new List<CultureInfo>();
                List<string> prevCodes = new List<string>();
                foreach (string cultureCode in parts)
                {
                    string c = cultureCode.Trim().ToLower();
                    if (c != "" && prevCodes.IndexOf(c) < 0)
                    {
                        ret.Add(new CultureInfo(c));
                        prevCodes.Add(c);
                    }
                } // foreach
                CultureInfo[] arr = ret.ToArray();

                if (arr.Length < 1)
                    throw new ArgumentException("Error: you need to have at least one language / culture info defined!");

                PerRequestCache.AddToCache(cacheKey, arr);
                return arr;
            }
        }

        public static bool UseLanguageShortCodeInPageUrls
        {
            get
            {
                // use the language path in the URL if there's more than one language configured 
                if (Languages.Length > 1)
                    return true;
                else
                    return false;
            }
        }

        private static string DEFAULT_USER_FILES_PATH = CmsContext.ApplicationPath + "UserFiles/";
        /// <summary>
        /// the root URL that User uploaded files are found at.
        /// Eg: /UserFiles/
        /// Note: returned path always ends in a slash (/)
        /// </summary>
        public static string UserFilesPath
        {
            get
            {
                string s = CmsConfig.getConfigValue("FCKeditor:UserFilesPath", DEFAULT_USER_FILES_PATH);

                if (!s.EndsWith("/"))
                    s += "/";

                if (s.StartsWith("~/"))
                {
                    s = s.Substring(2); // remove "~/"
                    s = CmsContext.ApplicationPath + s; // replace ~/ with ApplicationPath
                }

                return s;
            }
        }

    }
}