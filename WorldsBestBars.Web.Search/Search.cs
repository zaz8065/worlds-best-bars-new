using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace WorldsBestBars.Web.Search
{
    public static partial class Lucene
    {
        static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Lucene));

        static object indexLock = new object();

        static Directory indexStore;
        static Analyzer indexAnalyser;
        static Searcher indexSearcher;

        static Lucene()
        {
            Log.Info("Initialising Lucene Indexer from: " + Config.Storage);

            indexStore = new RAMDirectory();// new SimpleFSDirectory(new System.IO.DirectoryInfo(Config.Storage));
            indexAnalyser = new CustomAnalyser();

#if RE_INIT_ON_START
            foreach (var file in System.IO.Directory.GetFiles(Config.Storage)) { System.IO.File.Delete(file); }

            var writer = new IndexWriter(indexStore, indexAnalyser, true, IndexWriter.MaxFieldLength.UNLIMITED);
            writer.Close();
#endif
            IsIndexEmpty = true; // System.IO.Directory.GetFiles(Config.Storage).Length == 0;
            if (IsIndexEmpty)
            {
                using (var writer = new IndexWriter(indexStore, indexAnalyser, true, IndexWriter.MaxFieldLength.UNLIMITED))
                { }
            }
        }

        public static bool IsIndexEmpty { get; private set; }

        public static Model.SearchResult[] Search(string searchString, string type = null, string filter = null)
        {
            if (type != null)
            {
                searchString += " +type:" + type;
            }

            if (filter != null)
            {
                searchString += " +list:" + filter;
            }

            var parser = new QueryParser(Version.LUCENE_29, "text", indexAnalyser);
            var query = parser.Parse(searchString);
            indexSearcher = indexSearcher ?? new IndexSearcher(indexStore, true);

            var result = new List<Model.SearchResult>();

            var topDocs = indexSearcher.Search(query, null, 50);
            for (var i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                var doc = indexSearcher.Doc(topDocs.ScoreDocs[i].Doc);

                result.Add(new Model.SearchResult()
                {
                    Id = System.Guid.Parse(doc.GetField("id").StringValue),
                    Type = doc.GetField("type").StringValue,
                    Score = (double)topDocs.ScoreDocs[i].Score,
                    Name = doc.GetField("name").StringValue
                });
            }

            return result.ToArray();
        }

        private static void ResetSearcher()
        {
            if (indexSearcher != null)
            {
                indexSearcher.Dispose();
                indexSearcher = null;
            }
        }

        public static void DeleteFromIndex(System.Guid id)
        {
            lock (indexLock)
            {
                using (var writer = new IndexWriter(indexStore, indexAnalyser, false, IndexWriter.MaxFieldLength.UNLIMITED))
                {
                    writer.WriteLockTimeout = 100;

                    writer.DeleteDocuments(new Term("id", id.ToString()));

                    writer.Commit();
                    writer.Optimize();
                }

                ResetSearcher();
            }
        }

        public static void AddToIndex(IEnumerable<Model.DatabaseObject> entities)
        {
            lock (indexLock)
            {
                using (var writer = new IndexWriter(indexStore, indexAnalyser, false, IndexWriter.MaxFieldLength.UNLIMITED))
                {
                    writer.WriteLockTimeout = 100;

                    foreach (var entity in entities)
                    {
                        writer.AddToIndex(entity);
                    }

                    writer.Commit();
                    writer.Optimize();
                }

                ResetSearcher();
            }
        }

        public static void UpdateIndex(Model.DatabaseObject entity)
        {
            lock (indexLock)
            {
                using (var writer = new IndexWriter(indexStore, indexAnalyser, false, IndexWriter.MaxFieldLength.UNLIMITED))
                {
                    writer.WriteLockTimeout = 100;

                    writer.DeleteDocuments(new Term("id", entity.Id.ToString()));

                    writer.AddToIndex(entity);

                    writer.Commit();
                    writer.Optimize();
                }

                ResetSearcher();
            }
        }

        private static void AddToIndex(this IndexWriter writer, Model.DatabaseObject entity)
        {
            if (entity.Url == null) { return; }

            const char Space = ' ';

            var document = new Document();
            var searchText = string.Empty;

            if (entity is Model.Bar)
            {
                var _entity = (Model.Bar)entity;
                searchText = string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}", Space, _entity.Name, _entity.Intro, _entity.Address, _entity.Website, _entity.Phone, _entity.Email, _entity.Fax);

                if (_entity.Lists != null)
                {
                    foreach (var list in _entity.Lists)
                    {
                        if (list.Searchable)
                        {
                            document.Add(new Field("list", list.Key, Field.Store.YES, Field.Index.ANALYZED));
                        }
                    }
                }

                if (_entity.Parent != null)
                {
                    searchText += Space + _entity.Parent.Name;
                }
            }
            else if (entity is Model.Document)
            {
                var _entity = (Model.Document)entity;

                if (_entity.Redirect != null) { return; }

                searchText = string.Format("{1}{0}{2}{0}{3}", Space, _entity.Name, _entity.Synopsis, _entity.Content);
            }
            else if (entity is Model.Location)
            {
                var _entity = (Model.Location)entity;
                searchText = _entity.Synopsis + Space + _entity.Intro;
                for (var i = 0; i < 7; i++)
                {
                    searchText += Space + entity.Url.Replace('/', Space).Replace('-', Space);
                }
                for (var i = 0; i < 10; i++)
                {
                    searchText += Space + entity.Name;
                }
            }
            else if (entity is Model.User)
            {
                var _entity = (Model.User)entity;
                searchText = string.Format("{1}{0}{2}{0}{3}", Space, _entity.Name, _entity.Biography, _entity.Title);
            }
            else
            {
                // does not belong in the index.
                return;
            }

            if (entity.Url != null)
            {
                searchText += Space + entity.Url.Replace('/', Space).Replace('-', Space);
            }

            document.Add(new Field("text", searchText, Field.Store.YES, Field.Index.ANALYZED));

            document.Add(new Field("id", entity.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            document.Add(new Field("name", entity.Name, Field.Store.YES, Field.Index.ANALYZED));
            document.Add(new Field("type", entity.GetType().Name, Field.Store.YES, Field.Index.ANALYZED));

            writer.AddDocument(document);
        }

        public static class Config
        {
            static Config()
            {
                var search = ConfigurationManager.GetSection("worldsbestbars/search") as NameValueCollection;
                Storage = search["Storage"];
            }

            public static string Storage { get; private set; }
        }

        private class CustomAnalyser : Analyzer
        {
            public override TokenStream TokenStream(string fieldName, System.IO.TextReader reader)
            {
                TokenStream result = new StandardTokenizer(Version.LUCENE_29, reader);
                result = new StandardFilter(result);
                result = new LowerCaseFilter(result);
                result = new ASCIIFoldingFilter(result);
                return result;
            }
        }
    }
}