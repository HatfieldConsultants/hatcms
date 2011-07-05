using System;
using System.IO;
using System.Collections.Specialized;
using System.Collections;
using System.Collections.Generic;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Analysis;
using Lucene.Net.Highlight;
using SpellChecker.Net.Search.Spell;
using Lucene.Net.Index;
using Directory = Lucene.Net.Store.Directory;
using LuceneDictionary = SpellChecker.Net.Search.Spell.LuceneDictionary;
using IndexReader = Lucene.Net.Index.IndexReader;
using FSDirectory = Lucene.Net.Store.FSDirectory;

namespace Hatfield.Web.Portal.Search.Lucene
{
    /// <summary>
    /// Summary description for DMSKeywordSearch.
    /// </summary>
    public class LuceneKeywordSearch
    {
        private string luceneIndexDir = "";
        private string spellingIndexDir = "";

        public LuceneKeywordSearch(string LuceneIndexDir, string SpellingIndexDir)
        {
            luceneIndexDir = LuceneIndexDir;
            spellingIndexDir = SpellingIndexDir;
        } // constructor

        /// <summary>
        /// Searches the keyword index using the keywordQuery. 
        /// 
        /// See http://www.dotlucene.net/documentation/QuerySyntax.html  for the format of the keywordQuery.
        /// 
        /// This function will return a fully-filled array of IndexableFileInfo objects.
        /// </summary>
        /// <param name="keywordQuery"></param>
        /// <param name="queryForHighlighter"></param>
        /// <returns></returns>
        public IndexableFileInfo[] doSearch(string keywordQuery, string queryForHighlighter)
        {
            IndexSearcher searcher;
            IndexReader indexReader;

            try
            {
                FSDirectory indexDir = FSDirectory.GetDirectory(this.luceneIndexDir, false);
                indexReader = IndexReader.Open(indexDir);
                searcher = new IndexSearcher(indexReader);
            }
            catch
            {
                // if the luceneIndexDir does not contain index files (yet), IndexSearcher
                // throws a nice Exception.
                return new IndexableFileInfo[0];
            }
            List<IndexableFileInfo> arrayList = new List<IndexableFileInfo>();
            try
            {
                string Query = keywordQuery;
                if (Query == String.Empty)
                    return new IndexableFileInfo[0];

                string HighlighterQuery = queryForHighlighter;
                // -- weirdly enough, when the query is empty, an exception is thrown during the QueryParser.Parse
                //    this hack gets around that.
                if (HighlighterQuery == String.Empty)
                    HighlighterQuery = Guid.NewGuid().ToString();

                // parse the query, "text" is the default field to search
                // note: use the StandardAnalyzer! (the SimpleAnalyzer doesn't work correctly when searching by fields that are integers!)
                // MultiFieldQueryParser queryParser = new MultiFieldQueryParser(new string[] { "title", "contents" }, new hatWebPortalAnalyzer());
                MultiFieldQueryParser queryParser = new MultiFieldQueryParser(new string[] { "title", "contents" }, new SimpleAnalyzer());
                queryParser.SetDefaultOperator(QueryParser.AND_OPERATOR);

                Query query = queryParser.Parse(Query);

                QueryParser highlightQueryParser = new QueryParser("contents", new hatWebPortalAnalyzer());

                Query highlighterQuery = highlightQueryParser.Parse(HighlighterQuery);

                query = searcher.Rewrite(query); // is this needed?? " Expert: called to re-write queries into primitive queries."

                // search
                Hits hits = searcher.Search(query, Sort.RELEVANCE);

                // create highlighter
                Highlighter highlighter = new Highlighter(new SimpleHTMLFormatter("<strong>", "</strong>"), new QueryScorer(highlighterQuery));

                // -- go through hits and return results                                

                for (int i = 0; i < hits.Length(); i++)
                {
                    Document d = hits.Doc(i);
                    string filename = d.Get("filename");
                    string plainText = d.Get("contents");
                    string title = d.Get("title");
                    string sectionName = d.Get("SectionName");
                    string filenameParams = d.Get("filenameParams");
                    bool contentIsPageSummary = Convert.ToBoolean(d.Get("contentIsPageSummary"));
                    double score = Convert.ToDouble(hits.Score(i));
                    DateTime lastModified = DateTools.StringToDate(d.Get("LastModified"));

                    TokenStream tokenStream = new hatWebPortalAnalyzer().TokenStream("contents", new StringReader(plainText));

                    string fragment = plainText;
                    if (!contentIsPageSummary)
                        fragment = highlighter.GetBestFragments(tokenStream, plainText, 2, "...");

                    IndexableFileInfo newHit = new IndexableFileInfo(filename, filenameParams, title, fragment, sectionName, lastModified, contentIsPageSummary, score);
                    arrayList.Add(newHit);
                } // for
            }
            finally
            {
                searcher.Close();
                indexReader.Close();
            }


            return arrayList.ToArray();

        } // SearchActiveDocument


        public string getSpellingSuggestion(string query)
        {
            FSDirectory indexDir = FSDirectory.GetDirectory(this.spellingIndexDir, false);
            SpellChecker.Net.Search.Spell.SpellChecker spellchecker = new SpellChecker.Net.Search.Spell.SpellChecker(indexDir);
            IndexReader my_lucene_reader = IndexReader.Open(indexDir);
            string[] words = query.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> allSuggestions = new List<string>();
            foreach (string word in words)
            {
                string[] suggestions = spellchecker.SuggestSimilar(word, 1);
                if (suggestions.Length > 0)
                    allSuggestions.Add(suggestions[0]);
                else
                    allSuggestions.Add(word);
            }

            string completeSuggestion = String.Join(" ", allSuggestions.ToArray());
            return completeSuggestion;
        }

        public IndexableFileInfo[] getRelatedFiles(string title, int maxResultsToReturn)
        {
            // http://blogs.intesoft.net/post/2008/04/NHibernateSearch-using-LuceneNET-Full-Text-Index-(Part-3).aspx
            Analyzer analyzer = new StandardAnalyzer();
            BooleanQuery query = new BooleanQuery();

            if (title.Trim() != "")
            {
                Query titleQ = Similarity.Net.SimilarityQueries.FormSimilarQuery(title, analyzer, "title", null);
                titleQ.SetBoost(LuceneIndexer.TitleFieldBoost);
                query.Add(titleQ, BooleanClause.Occur.SHOULD);

                Query contents = Similarity.Net.SimilarityQueries.FormSimilarQuery(title, analyzer, "contents", null);
                query.Add(contents, BooleanClause.Occur.SHOULD);

            }


            // avoid the page being similar to itself!
            // query.Add(new TermQuery(new Term("title", title)), BooleanClause.Occur.MUST_NOT);


            /// IndexReader ir = ...
            /// IndexSearcher is = ...
            /// <b>
            /// MoreLikeThis mlt = new MoreLikeThis(ir);
            /// Reader target = ... </b><em>// orig source of doc you want to find similarities to</em><b>
            /// Query query = mlt.Like( target);
            /// </b>
            /// Hits hits = is.Search(query);

            FSDirectory indexDir = FSDirectory.GetDirectory(this.luceneIndexDir, false);
            IndexSearcher searcher;
            try
            {
                searcher = new IndexSearcher(indexDir);
            }
            catch
            {
                // if the luceneIndexDir does not contain index files (yet), IndexSearcher
                // throws a nice Exception.
                return new IndexableFileInfo[0];
            }


            List<IndexableFileInfo> arrayList = new List<IndexableFileInfo>();

            Hits hits = searcher.Search(query);
            try
            {
                int num = Math.Min(maxResultsToReturn, hits.Length());

                for (int i = 0; i < num; i++)
                {
                    Document d = hits.Doc(i);
                    string filename = d.Get("filename");
                    string plainText = d.Get("contents");
                    string doctitle = d.Get("title");
                    string filenameParams = d.Get("filenameParams");
                    bool contentIsPageSummary = Convert.ToBoolean(d.Get("contentIsPageSummary"));
                    DateTime lastModified = DateTools.StringToDate(d.Get("LastModified"));
                    string fragment = plainText;
                    string sectionName = d.Get("SectionName");

                    IndexableFileInfo newHit = new IndexableFileInfo(filename, filenameParams, doctitle, fragment, sectionName, lastModified, contentIsPageSummary);
                    arrayList.Add(newHit);
                } // for
            }
            finally
            {
                searcher.Close();
            }

            return arrayList.ToArray();

        }

    } // DMSKeywordSearch object
}

