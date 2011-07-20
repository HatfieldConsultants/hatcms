using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Hatfield.Web.Portal
{
    public class ConfigUtils
    {

        /// <summary>
        /// gets a value from the web.config's appSettings area.
        /// </summary>
        /// <param name="key">the configuration item's key name</param>
        /// <param name="defaultValue">the value to return if the configuration value is not found, or is blank</param>
        /// <returns>the configuration item's value, or defaultValue if the configuration value is not found</returns>
        public static string getConfigValue(string key, string defaultValue)
        {
            System.Collections.Specialized.NameValueCollection Config = System.Web.Configuration.WebConfigurationManager.AppSettings;
            if (Config != null && Config[key] != null && Config[key] != "")
            {
                return Config[key];
            }
            return defaultValue;
        }

        /// <summary>
        /// gets a value from the web.config's appSettings area.
        /// </summary>
        /// <param name="key">the configuration item's key name</param>
        /// <param name="defaultValue">the value to return if the configuration value is not found, is blank, or cannot be parsed</param>
        /// <returns>the configuration item's value, or defaultValue if the configuration value is not found, is blank or cannot be parsed</returns>
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
        /// gets a value from the web.config's appSettings area.
        /// </summary>
        /// <param name="key">the configuration item's key name</param>
        /// <param name="defaultValue">the value to return if the configuration value is not found, is blank, or cannot be parsed</param>
        /// <returns>the configuration item's value, or defaultValue if the configuration value is not found, is blank or cannot be parsed</returns>
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
        /// gets a value from the web.config's appSettings area.
        /// </summary>
        /// <param name="key">the configuration item's key name</param>
        /// <param name="defaultValue">the value to return if the configuration value is not found, is blank, or cannot be parsed</param>
        /// <returns>the configuration item's value, or defaultValue if the configuration value is not found, is blank or cannot be parsed</returns>
        public static double getConfigValue(string key, double defaultValue)
        {
            string s = getConfigValue(key, defaultValue.ToString());
            try
            {
                return Convert.ToDouble(s);
            }
            catch
            { }
            return defaultValue;
        }
    } // ConfigUtils
}
