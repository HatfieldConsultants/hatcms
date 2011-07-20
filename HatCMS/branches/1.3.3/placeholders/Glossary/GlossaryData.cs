using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Web;
using System.Text;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Hatfield.Web.Portal;

namespace HatCMS.Placeholders
{
    /// <summary>
    /// data holding class for a single Glossary term
    /// </summary>
    public class GlossaryData
    {
        public int Id = -1;
        
        /// <summary>
        /// The foreign key linking this glossary item to the placeholder.
        /// </summary>
        public int placeholderGlossaryId = -1;
        public bool isAcronym = false;
        public string description = "";
        public string word = "";

        private static string jsonEncode(string s)
        {
            s = s.Replace(Environment.NewLine, " ");
            s = s.Replace("\n", " ");
            s = s.Replace("\r", " ");            
            return StringUtils.AddSlashes(s);
        }

        public static string ToJSON(GlossaryData[] items)
        {
            StringBuilder json = new StringBuilder();
            /*
             {
                "1" : { "id": 1, "isAcronym": false, "word": "Abiotic", "text": "Non living or not containing any living organisms (Dougherty 2000-2001)."},
                "2" : { "id": 2, "isAcronym": false, "word": "Abiotic", "text": "Non living or not containing any living organisms (Dougherty 2000-2001)."}
             }
            */

            List<string> lines = new List<string>();
            foreach (GlossaryData item in items)
            {
                string line = "\"" + item.Id + "\" : { \"id\": " + item.Id + ", \"isAcronym\": " + item.isAcronym.ToString() + ", \"word\": \"" + jsonEncode(item.word) + "\", \"text\": \"" + jsonEncode(item.description) + "\"}";
                lines.Add(line);
            } // foreach

            json.Append("{");
            json.Append(String.Join(", ", lines.ToArray()));
            
            json.Append("}");

            return json.ToString();
        }
   

        public static bool ArrayContainsStartChar(GlossaryData[] items, char c)
        {
            foreach (GlossaryData item in items)
            {
                if (item.word.Length > 0 && String.Compare(item.word[0].ToString(), c.ToString(), true) == 0)
                    return true;
            } // foreach
            return false;
        }

        public static GlossaryData[] getItemsStartingWithChar(GlossaryData[] items, char c)
        {
            List<GlossaryData> ret = new List<GlossaryData>();
            foreach (GlossaryData item in items)
            {
                if (item.word.Length > 0 && String.Compare(item.word[0].ToString(), c.ToString(), true) == 0)
                    ret.Add(item);
            } // foreach
            return ret.ToArray();
        }

        public static GlossaryData[] SortByWordLength(GlossaryData[] itemsToSort, SortDirection sortDir)
        {
            List<GlossaryData> arr = new List<GlossaryData>(itemsToSort);
            arr.Sort(new LengthComparer(sortDir));
            return arr.ToArray();
        }

        private class LengthComparer : Comparer<GlossaryData>
        {
            SortDirection dir;
            public LengthComparer(SortDirection sortDir)
            {
                dir = sortDir;
            }
            public override int Compare(GlossaryData x, GlossaryData y)
            {
                if (dir == SortDirection.Ascending)
                    return x.word.Length.CompareTo(y.word.Length);
                else
                    return y.word.Length.CompareTo(x.word.Length);
            }
        }

    }
}
