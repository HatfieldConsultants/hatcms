using System;
using System.Runtime.InteropServices;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using System.Collections.Specialized;
using SpellChecker.Net.Search.Spell;
using Lucene.Net.Search;
using Directory = Lucene.Net.Store.Directory;
using LuceneDictionary = SpellChecker.Net.Search.Spell.LuceneDictionary;
using IndexReader = Lucene.Net.Index.IndexReader;
using FSDirectory = Lucene.Net.Store.FSDirectory;
using IndexSearcher = Lucene.Net.Search.IndexSearcher;

namespace Hatfield.Web.Portal.Search.Lucene
{
    /// <summary>
    /// The LuceneIndexer object provides API access to start the document indexing service. 
    /// This indexing service goes through all active documents, reads their contents, and adds them to an index that is made available by the DMSKeywordSearch object.
    /// </summary>
    public class LuceneIndexer
    {

        protected static readonly object padlock = new object();
        protected IndexWriter writer;
        public static float TitleFieldBoost = (float)5.0; // title should be weighted 5 times heavier than other terms


        private string _luceneIndexDir = "";
        private IndexCreationMode _indexCreationMode = IndexCreationMode.CreateNewIndex;

        protected FSDirectory luceneIndexDir
        {
            get
            {
                return FSDirectory.GetDirectory(this._luceneIndexDir, false);
            }
        }

        public LuceneIndexer(string LuceneIndexDir, IndexCreationMode indexCreationMode)
        {
            _luceneIndexDir = LuceneIndexDir;
            _indexCreationMode = indexCreationMode;

            if (_indexCreationMode == IndexCreationMode.AppendToExistingIndex && !System.IO.File.Exists(System.IO.Path.Combine(_luceneIndexDir, "segments")))
            {
                // throw new ArgumentException("To Append to an existing index, one needs to exist already!!!");
                _indexCreationMode = IndexCreationMode.CreateNewIndex;
            }

            try
            {
                if (_indexCreationMode == IndexCreationMode.CreateNewIndex)
                    writer = new IndexWriter(luceneIndexDir, new hatWebPortalAnalyzer(), true);
                else if (_indexCreationMode == IndexCreationMode.AppendToExistingIndex)
                    writer = new IndexWriter(luceneIndexDir, new hatWebPortalAnalyzer(), false);
                else
                    throw new ArgumentException("Invalid IndexCreationMode parameter");

                writer.SetUseCompoundFile(false);
            }
            catch (Exception ex)
            {
                if (ex.Message.IndexOf(@"\_svn") > -1 || ex.Message.IndexOf(@"\.svn") > -1)
                {
                    throw new Exception("Please remove the SVN directory under \"" + luceneIndexDir + "\" - this is the only way the indexing can work");
                }
                else
                {
                    throw new Exception("Exception thrown when creating Hatfield.Web.Portal.Search.Lucene.LuceneIndexer", ex);
                }
            }

        } // constructor

        public int getNumDocsInIndex()
        {
            try
            {

                IndexSearcher searcher = new IndexSearcher(luceneIndexDir);
                try
                {
                    return searcher.MaxDoc();
                }
                finally
                {
                    searcher.Close();
                }

            }
            catch
            { }
            return -1;
        }


        /// <summary>
        /// gets the number of documents in the index. Returns -1 on error.
        /// </summary>
        /// <returns></returns>
        public static int getNumDocsInIndex(string luceneIndexDir)
        {

            return (new LuceneIndexer(luceneIndexDir, IndexCreationMode.AppendToExistingIndex)).getNumDocsInIndex(); // don't create new index
        }



        private bool isInIndex(IndexableFileInfo fileInfo)
        {
            IndexSearcher searcher = new IndexSearcher(this.luceneIndexDir);

            try
            {

                BooleanQuery bq = new BooleanQuery();
                bq.Add(new TermQuery(new Term("filename", fileInfo.Filename)), BooleanClause.Occur.MUST);

                bq.Add(new TermQuery(new Term("LastModified", DateTools.DateToString(fileInfo.LastModified, DateTools.Resolution.SECOND))), BooleanClause.Occur.MUST);

                Hits hits = searcher.Search(bq);
                int count = hits.Length();

                if (count > 0)
                    return true;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            finally
            {
                searcher.Close();
            }
            return false;
        }


        public void addFileInfoToIndex(IndexableFileInfo fileInfo)
        {

            if ((_indexCreationMode == IndexCreationMode.AppendToExistingIndex) && isInIndex(fileInfo))
                return;

            bool fileExistsOnDisk = System.IO.File.Exists(fileInfo.Filename);

            if (fileExistsOnDisk && fileInfo.Contents == "")
            {
                fileInfo.Contents = IFilterFileContents.getFileContents(fileInfo.Filename);
            }

            Document doc = new Document();
            /* From http://www.webreference.com/programming/lucene/2/
                * Field.Keyword Isn't analyzed, but is indexed and stored in the index verbatim. This type is suitable for fields whose original value should be preserved in its entirety, such as URLs, file system paths, dates, personal names, Social Security numbers, telephone numbers, and so on. For example, we used the file system path in Indexer (listing 1.1) as a Keyword field.
                * Field.UnIndexed Is neither analyzed nor indexed, but its value is stored in the index as is. This type is suitable for fields that you need to display with search results (such as a URL or database primary key), but whose values you'll never search directly. Since the original value of a field of this type is stored in the index, this type isn't suitable for storing fields with very large values, if index size is an issue.
                * Field.UnStored The opposite of UnIndexed. This field type is analyzed and indexed but isn't stored in the index. It's suitable for indexing a large amount of text that doesn't need to be retrieved in its original form, such as bodies of web pages, or any other type of text document
                * Field.Text Is analyzed, and is indexed. This implies that fields of this type can be searched against, but be cautious about the field size. If the data indexed is a String, it's also stored; but if the data (as in our Indexer example) is from a Reader, it isn't stored. This is often a source of confusion, so take note of this difference when using Field.Text.				 
            */

            // -- add fields to the document								
            // doc.Add(Field.Keyword("docId", dmsDoc.DocumentId.ToString()));		

            doc.Add(new Field("contents", fileInfo.Contents, Field.Store.YES, Field.Index.TOKENIZED)); // can be searched and is analyzed
            doc.Add(new Field("filename", fileInfo.Filename, Field.Store.YES, Field.Index.UN_TOKENIZED)); // can be searched, but is not analyzed
            doc.Add(new Field("filenameParams", fileInfo.FilenameParameters, Field.Store.YES, Field.Index.NO)); // can not be searched
            doc.Add(new Field("contentIsPageSummary", Convert.ToString(fileInfo.ContentIsPageSummary), Field.Store.YES, Field.Index.NO));

            doc.Add(new Field("SectionName", fileInfo.SectionName, Field.Store.YES, Field.Index.UN_TOKENIZED));


            doc.Add(new Field("LastModified", DateTools.DateToString(fileInfo.LastModified, DateTools.Resolution.SECOND), Field.Store.YES, Field.Index.UN_TOKENIZED));

            Field titleField = new Field("title", fileInfo.Title, Field.Store.YES, Field.Index.TOKENIZED);
            titleField.SetBoost(TitleFieldBoost); // default value is 1.0
            doc.Add(titleField);

            if (fileExistsOnDisk)
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(new System.IO.FileInfo(fileInfo.Filename).Directory.FullName);
                doc.Add(new Field("directparentdirectory", di.FullName, Field.Store.YES, Field.Index.UN_TOKENIZED));
                while (di != null)
                {
                    doc.Add(new Field("parentdirectory", di.FullName, Field.Store.YES, Field.Index.UN_TOKENIZED));
                    di = di.Parent;
                }


                string ext = System.IO.Path.GetExtension(fileInfo.Filename);
                ext = ext.ToLower();
                if (ext.StartsWith("."))
                    ext = ext.Substring(1);
                doc.Add(new Field("filetype", ext, Field.Store.YES, Field.Index.UN_TOKENIZED));
            }

            doc.Add(new Field("dateIndexed", DateTools.DateToString(DateTime.Now, DateTools.Resolution.SECOND), Field.Store.YES, Field.Index.NO));
            // -- add the document to the index
            writer.AddDocument(doc);


        } // addDMSDocToIndex


        protected enum OptimizeMode { DoNotOptimize, DoOptimization };
        protected void CloseIndexWriter(OptimizeMode optimizeMode)
        {
            if (optimizeMode == OptimizeMode.DoOptimization)
            {
                writer.Optimize();
            }
            writer.Close();

        }// Close Index Writer

        protected static bool indexing = false;


        public delegate void onAddFileToIndex(IndexableFileInfo currentFile, double percentComplete);

        public static void doIndex(string LuceneIndexDir, string SpellingIndexDir, IndexCreationMode indexCreationMode, IndexableFileInfo[] fileInfos, object ThreadState)
        {
            doIndex(LuceneIndexDir, SpellingIndexDir, indexCreationMode, fileInfos, ThreadState, null);
        }

        public enum IndexCreationMode { CreateNewIndex, AppendToExistingIndex };

        public static void doIndex(string LuceneIndexDir, string SpellingIndexDir, IndexCreationMode indexCreationMode, IndexableFileInfo[] fileInfos, object ThreadState, onAddFileToIndex AddFileToIndex)
        {
            if (indexing)
                return;
            lock (padlock)
            {
                indexing = true;


                LuceneIndexer indexer = new LuceneIndexer(LuceneIndexDir, indexCreationMode); // create new index
                try
                {

                    for (int i = 0; i < fileInfos.Length; i++)
                    {
                        IndexableFileInfo fi = fileInfos[i];
                        if (AddFileToIndex != null)
                            AddFileToIndex(fi, (double)i / (double)fileInfos.Length);

                        indexer.addFileInfoToIndex(fi);
                    } // foreach
                }
                finally
                {
                    indexer.CloseIndexWriter(OptimizeMode.DoNotOptimize);
                }
                if (indexCreationMode == IndexCreationMode.AppendToExistingIndex)
                {
                    removeAllDuplicateAndDeletedFiles(fileInfos, LuceneIndexDir, indexCreationMode);
                }
                try
                {
                    doSpellCheckerIndexing(LuceneIndexDir, SpellingIndexDir);
                }
                catch
                { }



                indexing = false;

            }
        } // doIndex

        private static void removeAllDuplicateAndDeletedFiles(IndexableFileInfo[] fileInfos, string LuceneIndexDir, IndexCreationMode indexCreationMode)
        {
            if (indexCreationMode != IndexCreationMode.AppendToExistingIndex)
                return;

            IndexReader reader = IndexReader.Open(LuceneIndexDir);
            try
            {
                int numDocs = reader.NumDocs();
                for (int i = 0; i < numDocs; i++)
                {
                    Document docToCheck = reader.Document(i);
                    bool removeDocFromIndex = true;
                    string filenameField = docToCheck.GetField("filename").StringValue();
                    string lastModified = (docToCheck.GetField("LastModified").StringValue());

                    foreach (IndexableFileInfo fi in fileInfos)
                    {
                        if (String.Compare(fi.Filename, filenameField, true) == 0 && DateTools.DateToString(fi.LastModified, DateTools.Resolution.SECOND) == lastModified)
                        {
                            removeDocFromIndex = false;
                            break;
                        }
                    } // foreach

                    if (removeDocFromIndex)
                    {
                        reader.DeleteDocument(i);
                        if (!reader.HasDeletions())
                            throw new Exception("error: deletion failed!!");
                    }


                } // for each lucene doc

            }
            finally
            {
                reader.Close();
            }
            LuceneIndexer indexer = new LuceneIndexer(LuceneIndexDir, indexCreationMode); // open up the index again
            indexer.CloseIndexWriter(OptimizeMode.DoOptimization); // just to optimize the index (which removes deleted items).
        }

        private static void doSpellCheckerIndexing(string LuceneIndexDir, string SpellCheckerIndexDir)
        {
            try
            {
                // http://lucene.apache.org/java/2_2_0/api/org/apache/lucene/search/spell/SpellChecker.html
                FSDirectory spellCheckerIndexDir = FSDirectory.GetDirectory(SpellCheckerIndexDir, false);
                FSDirectory indexDir = FSDirectory.GetDirectory(LuceneIndexDir, false);

                SpellChecker.Net.Search.Spell.SpellChecker spellchecker = new SpellChecker.Net.Search.Spell.SpellChecker(spellCheckerIndexDir);
                spellchecker.ClearIndex();
                // SpellChecker.Net.Search.Spell.SpellChecker spellchecker = new SpellChecker.Net.Search.Spell.SpellChecker (global::Lucene.Net.Store.Directory SpellChecker(spellIndexDirectory);

                IndexReader r = IndexReader.Open(indexDir);
                try
                {
                    // To index a field of a user index:            
                    Dictionary dict = new SpellChecker.Net.Search.Spell.LuceneDictionary(r, "title");

                    spellchecker.IndexDictionary(dict);
                }
                finally
                {
                    r.Close();
                }

            }
            catch (Exception ex)
            {
                Console.Write("Could not create spell-checking index" + ex.Message);
            }
        }

        public static bool isCurrentlyIndexing
        {
            get
            {
                return indexing;
            }
        }


    } // DMSIndexer object

    public class IndexableFileInfo
    {
        public string Filename = "";
        public string FilenameParameters = "";
        public string Title = "";
        public string SectionName = "";
        /// <summary>
        /// if the filename exists and the contents is empty, the filename will be queried using IFilter
        /// </summary>
        public string Contents = "";
        public bool ContentIsPageSummary = false;
        public double Score = double.NaN;
        public DateTime LastModified = DateTime.MinValue;

        public IndexableFileInfo(string filename, string filenameParameters, string title, string contents, string sectionName, DateTime lastModified, bool contentIsPageSummary)
        {
            Filename = filename;
            Title = title;
            Contents = contents;
            SectionName = sectionName;
            FilenameParameters = filenameParameters;
            ContentIsPageSummary = contentIsPageSummary;
            Score = double.NaN;
            LastModified = lastModified;
        }


        public IndexableFileInfo(string filename, string filenameParameters, string title, string contents, string sectionName, DateTime lastModified, bool contentIsPageSummary, double score)
        {
            Filename = filename;
            Title = title;
            Contents = contents;
            SectionName = sectionName;
            FilenameParameters = filenameParameters;
            ContentIsPageSummary = contentIsPageSummary;
            LastModified = lastModified;
            Score = score;
        }
    } // hatWebPortalIndexableFileInfo object
}
