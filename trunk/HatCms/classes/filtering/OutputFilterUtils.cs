using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Reflection;
using System.Web.Caching;

using Hatfield.Web.Portal;

namespace HatCMS
{
    /// <summary>
    /// Utility functions used to assist with calling and using CmsOutputFilters. <seealso cref="CmsOutputFilter"/>
    /// </summary>
    public class CmsOutputFilterUtils
    {
        /// <summary>
        /// To define a filter in a class, create a function CmsOutputFilter[] getOutputFilters(){};
        /// Gets all Output Filters from all objects defined in this assembly. The result is cached in HttpContext.Current.Cache
        /// </summary>
        /// <returns></returns>
        private static Dictionary<CmsOutputFilterScope, CmsOutputFilter[]> GetAllOutputFilters()
        {            
            Assembly callingAssembly = Assembly.GetExecutingAssembly();

            // the process for fetching all filters
            Cache cache = System.Web.HttpContext.Current.Cache;
            string cacheKey = "AllOutputFilters" + callingAssembly.FullName;
            if (cache[cacheKey] != null)
                return (Dictionary<CmsOutputFilterScope, CmsOutputFilter[]>)cache[cacheKey];


            System.Type[] assemblyTypes = callingAssembly.GetTypes();

            List<CmsOutputFilter> allFilters = new List<CmsOutputFilter>();

            try
            {

                foreach (Type type in assemblyTypes)
                {
                    try
                    {
                        // type.IsClass == true && !type.IsAbstract &&
                        if (type.GetMethod("getOutputFilters") != null)
                        {
                            CmsOutputFilter[] filters = (CmsOutputFilter[])ExecuteDynamicCode.InvokeMethod(callingAssembly.Location, type.Name, "getOutputFilters", new object[0]);

                            allFilters.AddRange(filters);
#if DEBUG
                            if (filters.Length > 0)
                                Console.Write(type.Name + " had " + filters.Length + " filters");
#endif
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.Message);
                    }


                } // foreach type
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

            Dictionary<CmsOutputFilterScope, CmsOutputFilter[]> ret = toDictionary(allFilters);

            cache.Insert(cacheKey, ret, new CacheDependency(callingAssembly.Location));

            return ret;
        } // GetAlloutputFilters

        /// <summary>
        /// convert the CmsOutputFilter <paramref name="allFilters"/> array to a dictionary, organized by scope.
        /// </summary>
        /// <param name="allFilters"></param>
        /// <returns></returns>
        private static Dictionary<CmsOutputFilterScope, CmsOutputFilter[]> toDictionary(List<CmsOutputFilter> allFilters)
        {
            Dictionary<CmsOutputFilterScope, List<CmsOutputFilter>> temp = new Dictionary<CmsOutputFilterScope,List<CmsOutputFilter>>();
            
            // create the dictionary keys
            foreach (CmsOutputFilterScope scope in Enum.GetValues(typeof(CmsOutputFilterScope)))
            {
                temp[scope] = new List<CmsOutputFilter>();                
            }
            // add the filters
            foreach (CmsOutputFilter f in allFilters)
            {
                temp[f.Scope].Add(f);
            } // foreach
            // convert the List<> to array[]

            Dictionary<CmsOutputFilterScope, CmsOutputFilter[]> ret = new Dictionary<CmsOutputFilterScope, CmsOutputFilter[]>();
            foreach (CmsOutputFilterScope scope in temp.Keys)
            {
                ret.Add(scope, temp[scope].ToArray());
            } // foreach
            return ret;
        }        

        /// <summary>
        /// Run all filters that execute on placeholders of a particular <paramref name="placeholderName"/>.
        /// </summary>
        /// <param name="placeholderName"></param>
        /// <param name="pageBeingFiltered"></param>
        /// <param name="htmlToFilter"></param>
        /// <returns></returns>
        public static string RunPlaceholderFilters(string placeholderName, CmsPage pageBeingFiltered, string htmlToFilter)
        {
            Dictionary<CmsOutputFilterScope, CmsOutputFilter[]> allFilters = GetAllOutputFilters();
            string filteredHtml = htmlToFilter;

            // -- run all global placeholder filters
            foreach (CmsOutputFilter filterToRun in allFilters[CmsOutputFilterScope.AllPlaceholders])
            {
                filteredHtml = filterToRun.RunFilter(pageBeingFiltered, filteredHtml);
            } // foreach filter

            // -- filter specific placeholders
            foreach (CmsOutputFilter filterToRun in allFilters[CmsOutputFilterScope.SpecifiedPlaceholderTypes])
            {
                if (StringUtils.IndexOf(filterToRun.SpecificPlaceholderNamesOrControlPathsToFilter, placeholderName, StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    filteredHtml = filterToRun.RunFilter(pageBeingFiltered, filteredHtml);
                }
            } // foreach filter

            return filteredHtml;
        }

        /// <summary>
        /// Run all page output filters.
        /// </summary>
        /// <param name="pageBeingFiltered"></param>
        /// <param name="pageHtmlToFilter"></param>
        /// <returns></returns>
        public static string RunPageOutputFilters(CmsPage pageBeingFiltered, string pageHtmlToFilter)
        {
            Dictionary<CmsOutputFilterScope, CmsOutputFilter[]> allFilters = GetAllOutputFilters();
            string filteredHtml = pageHtmlToFilter;

            // -- run all global placeholder filters
            foreach (CmsOutputFilter filterToRun in allFilters[CmsOutputFilterScope.PageHtmlOutput])
            {
                filteredHtml = filterToRun.RunFilter(pageBeingFiltered, filteredHtml);
            } // foreach filter
            

            return filteredHtml;
        }

        #region RunPageOutputFiltersStream class

        public class PageResponseOutputFilter : Stream
        {
            // idea taken from http://aspalliance.com/71_Modifying_Page_Output
            private Stream _sink;
            private long _position;
            StringBuilder oOutput = new StringBuilder();
            CmsPage _page;

            public PageResponseOutputFilter(Stream sink, CmsPage page)
            {
                _sink = sink;
                _page = page;
            }

            // The following members of Stream must be overriden.
            public override bool CanRead
            {
                get { return true; }
            }

            public override bool CanSeek
            {
                get { return true; }
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            public override long Length
            {
                get { return 0; }
            }

            public override long Position
            {
                get { return _position; }
                set { _position = value; }
            }

            public override long Seek(long offset, System.IO.SeekOrigin direction)
            {
                return _sink.Seek(offset, direction);
            }

            public override void SetLength(long length)
            {
                _sink.SetLength(length);
            }

            public override void Close()
            {
                _sink.Close();
            }

            public override void Flush()
            {
                _sink.Flush();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return _sink.Read(buffer, offset, count);
            }

            // The Write method actually does the filtering.
            public override void Write(byte[] buffer, int offset, int count)
            {                                

                //Get a string version of the buffer
                string szBuffer = System.Text.UTF8Encoding.UTF8.GetString(buffer, offset, count);

                //Look for the end of the HTML file                                
                if (szBuffer.IndexOf("</html>", StringComparison.CurrentCultureIgnoreCase) > -1)
                {
                    //Append the last buffer of data
                    oOutput.Append(szBuffer);

                    //Get back the complete response for the client
                    string szCompleteBuffer = oOutput.ToString();

                    // go through each registered filter and run it.
                    szCompleteBuffer = CmsOutputFilterUtils.RunPageOutputFilters(_page, szCompleteBuffer);


                    // write out data
                    byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(szCompleteBuffer);
                    _sink.Write(data, 0, data.Length);
                }
                else
                {
                    oOutput.Append(szBuffer);
                }
            } // Write

        }
        #endregion
        
        
    }
}

