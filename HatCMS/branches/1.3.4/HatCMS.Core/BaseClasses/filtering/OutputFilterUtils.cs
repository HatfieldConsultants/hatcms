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
        /// Gets all Output Filters from all objects defined in this assembly. The result is cached on a per-request basis.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<CmsOutputFilterScope, CmsOutputFilterInfo[]> GetAllOutputFilterInfos()
        {                        
            string cacheKey = "GetAllOutputFilterInfos";
            if (PerRequestCache.CacheContains(cacheKey))
                return (Dictionary<CmsOutputFilterScope, CmsOutputFilterInfo[]>)PerRequestCache.GetFromCache(cacheKey, new Dictionary<CmsOutputFilterScope, CmsOutputFilterInfo[]>());


            System.Type[] filterTypes = Hatfield.Web.Portal.AssemblyHelpers.LoadAllAssembliesAndGetAllSubclassesOf(typeof(BaseCmsOutputFilter));

            List<CmsOutputFilterInfo> allFilters = new List<CmsOutputFilterInfo>();

            try
            {
                foreach (Type type in filterTypes)
                {
                    try
                    {
                        BaseCmsOutputFilter filter = (BaseCmsOutputFilter)type.Assembly.CreateInstance(type.FullName);
                        CmsOutputFilterInfo info = filter.getOutputFilterInfo();
                        allFilters.Add(info);
                        
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

            Dictionary<CmsOutputFilterScope, CmsOutputFilterInfo[]> ret = toDictionary(allFilters);

            PerRequestCache.AddToCache(cacheKey, ret);            

            return ret;
        } // GetAlloutputFilters

        /// <summary>
        /// convert the CmsOutputFilter <paramref name="allFilters"/> array to a dictionary, organized by scope.
        /// </summary>
        /// <param name="allFilters"></param>
        /// <returns></returns>
        private static Dictionary<CmsOutputFilterScope, CmsOutputFilterInfo[]> toDictionary(List<CmsOutputFilterInfo> allFilters)
        {
            Dictionary<CmsOutputFilterScope, List<CmsOutputFilterInfo>> temp = new Dictionary<CmsOutputFilterScope,List<CmsOutputFilterInfo>>();
            
            // create the dictionary keys
            foreach (CmsOutputFilterScope scope in Enum.GetValues(typeof(CmsOutputFilterScope)))
            {
                temp[scope] = new List<CmsOutputFilterInfo>();                
            }
            // add the filters
            foreach (CmsOutputFilterInfo f in allFilters)
            {
                temp[f.Scope].Add(f);
            } // foreach
            // convert the List<> to array[]

            Dictionary<CmsOutputFilterScope, CmsOutputFilterInfo[]> ret = new Dictionary<CmsOutputFilterScope, CmsOutputFilterInfo[]>();
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
            Dictionary<CmsOutputFilterScope, CmsOutputFilterInfo[]> allFilters = GetAllOutputFilterInfos();
            string filteredHtml = htmlToFilter;

            // -- run all global placeholder filters
            foreach (CmsOutputFilterInfo filterToRun in allFilters[CmsOutputFilterScope.AllPlaceholders])
            {
                filteredHtml = filterToRun.RunFilter(pageBeingFiltered, filteredHtml);
            } // foreach filter

            // -- filter specific placeholders
            foreach (CmsOutputFilterInfo filterToRun in allFilters[CmsOutputFilterScope.SpecifiedPlaceholderTypes])
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
            Dictionary<CmsOutputFilterScope, CmsOutputFilterInfo[]> allFilters = GetAllOutputFilterInfos();
            string filteredHtml = pageHtmlToFilter;

            // -- run all global placeholder filters
            foreach (CmsOutputFilterInfo filterToRun in allFilters[CmsOutputFilterScope.PageHtmlOutput])
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

