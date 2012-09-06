using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace HatCMS
{
    /// <summary>
    /// The root class for all dependency classes.
    /// </summary>
    public abstract class CmsDependency
    {
        /// <summary>
        /// Runs the validation
        /// </summary>
        /// <returns></returns>
        public abstract CmsDependencyMessage[] ValidateDependency();

        /// <summary>
        /// Gets the content hash for the dependency - used to check for duplicate dependencies based on their contents.
        /// </summary>
        /// <returns>a string containing the comparable hash for the item</returns>
        public abstract string GetContentHash();

        /// <summary>
        /// If the dependency MustExist or MustNotExist
        /// </summary>
        public enum ExistsMode 
        { 
            /// <summary>
            /// The dependency must exist
            /// </summary>
            MustExist, 
            /// <summary>
            /// the dependency must NOT exist
            /// </summary>
            MustNotExist 
        }

        /// <summary>
        /// Removes duplicate dependencies. Uses <see cref="GetContentHash"/> to determine duplicate items. If two items have the same ContentHash, they are determined to be the same.
        /// </summary>
        /// <param name="dependencies"></param>
        /// <returns></returns>
        public static CmsDependency[] RemoveDuplicates(CmsDependency[] dependencies)
        {
            Dictionary<string, CmsDependency> dict = new Dictionary<string, CmsDependency>();
            foreach (CmsDependency d in dependencies)
            {
                string hash = d.GetContentHash();
                if (!dict.ContainsKey(hash))
                    dict.Add(hash, d);
            } // foreach

            return new List<CmsDependency>(dict.Values).ToArray();
        }
    }
}
