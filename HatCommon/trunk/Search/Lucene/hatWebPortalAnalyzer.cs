using System;
using System.Text;
using System.IO;
using Lucene.Net.Analysis.Standard;
using LuceneFilters.KStemmer;
using TokenStream = Lucene.Net.Analysis.TokenStream;
using Lucene.Net.Analysis;


namespace Hatfield.Web.Portal.Search.Lucene
{
    /// <summary>
    /// Summary description for TigerDMSAnalyzer.
    /// </summary>
    public class hatWebPortalAnalyzer : StandardAnalyzer
    {
        public hatWebPortalAnalyzer()
            : base(new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" })
        // public TigerDMSAnalyzer() : base( )
        { }

        public override TokenStream TokenStream(string strFieldName, TextReader reader)
        {

            // -- note: LowerCaseTokenizer doesn't seem to work for me (StandardTokenizer does work): JS
            // return new KStemFilter(new Lucene.Net.Analysis.LowerCaseTokenizer(reader));
            return new KStemFilter(new StandardTokenizer(reader));


        }
    }
}

