using System;
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
    /// The AppLanguage class represents a language in the system. CmsPages have different representations based on the language.
    /// </summary>
    public class CmsLanguage: IComparable
    {
        /// <summary>
        /// the code used in the URL
        /// </summary>
        public string shortCode;

        public CmsLanguage(string code)
        {
            shortCode = code.ToLower();
            
        } // constructor

        private static string InvalidLanguageShortCode = "~~..Invalid..~~";        

        public bool isValidLanguage
        {
            get
            {
                return !isInvalidLanguage;
            }
        }

        public bool isInvalidLanguage
        {
            get
            {
                if (String.Compare(shortCode.Trim(), "") == 0 || String.Compare(shortCode.Trim(), InvalidLanguageShortCode, true) == 0)
                    return true;                
                else
                    return false;
            }
        }
        

        /// <summary>
        /// if langShortCodeToFind is not found, returns the Invalid language (that has .isInvalidLanguage set to TRUE)
        /// </summary>
        /// <param name="langShortCodeToFind"></param>
        /// <param name="haystack"></param>
        /// <returns></returns>
        public static CmsLanguage GetFromHaystack(string langShortCodeToFind, CmsLanguage[] haystack)
        {
            foreach (CmsLanguage l in haystack)
            {
                if (string.Compare(l.shortCode, langShortCodeToFind, true) == 0)
                    return l;
            } // foreach
            return CreateInvalidLanguage();
        }

        /// <summary>
        /// Finds the langShortCodeToFind in the haystack. If not found in the haystack, -1 is returned.
        /// </summary>
        /// <param name="langShortCodeToFind"></param>
        /// <param name="haystack"></param>
        /// <returns></returns>
        public static int IndexOf(string langShortCodeToFind, CmsLanguage[] haystack)
        {
            for(int i=0; i< haystack.Length; i++)
            {
                if (string.Compare(haystack[i].shortCode, langShortCodeToFind, true) == 0)
                    return i;
            } // foreach
            return -1;
        }

        public static CmsLanguage CreateInvalidLanguage()
        {
            return new CmsLanguage(InvalidLanguageShortCode);
        }
        


        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (obj is CmsLanguage)
            {
                return String.Compare(this.shortCode, (obj as CmsLanguage).shortCode, true);
            }
            throw new Exception("can not compare CmsLanguage object to another type.");
        }

        #endregion

        public override bool Equals(object obj)
        {
            return (CompareTo(obj) == 0);            
        }
    } // class
}

