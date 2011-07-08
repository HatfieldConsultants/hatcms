using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Hatfield.Web.Portal
{
    public class PerRequestCache
    {
        private static bool cacheIsAvailable()
        {
            if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Items != null)
                return true;
            return false;
        }

        public static object GetFromCache(string key, object returnOnErrorOrInvalid)
        {
            if (cacheIsAvailable())
            {
                if (System.Web.HttpContext.Current.Items.Contains(key))
                    return System.Web.HttpContext.Current.Items[key];
            }
            return returnOnErrorOrInvalid;
        } // GetFromCache

        public static bool CacheContains(string key)
        {
            if (cacheIsAvailable() && System.Web.HttpContext.Current.Items.Contains(key))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// if the key already exists, the value will be overwritten
        /// </summary>
        /// <param name="key"></param>
        /// <param name="objToAdd"></param>
        /// <returns></returns>
        public static bool AddToCache(string key, object objToAdd)
        {
            if (cacheIsAvailable())
            {
                System.Web.HttpContext.Current.Items.Add(key, objToAdd);
                return true;
            }
            return false;
        }
    }
}
