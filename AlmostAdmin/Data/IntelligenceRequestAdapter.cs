using AlmostAdmin.Models;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AlmostAdmin.Data
{
    public class LuceneSearchResult
    {
        public int QuestionDbId { get; set; }
        public int QuestionProjectDbId { get; set; }
        public string QuestionText { get; set; }
        public float Score { get; internal set; }
    }
    public class LuceneProcessor //: IIntelligenceRequestsAdapter
    {
        //private object _processor;
        private const string _LuceneIndexDirectory = "LuceneData";//~/LuceneData/AlmostAdminIndex";

        public LuceneProcessor()
        {
        }
        
        //public async Task<IIntelligenceProcessedData> ProcessDataAsync(string dataToProcess)
        //{
        //    throw new NotImplementedException();
        //}

        internal List<LuceneSearchResult> Search(string questionText, int projectId, int limit = 10, string keywords = "")
        {
            using (var directory = GetDirectory())
            {
                var reader = DirectoryReader.Open(directory);
                var searcher = new IndexSearcher(reader);
                using (var analyzer = GetAnalyzer())
                {
                    var parser = new QueryParser(Lucene.Net.Util.LuceneVersion.LUCENE_48, "Text", analyzer);

                    var query = new BooleanQuery();
                    var keywordsQuery = parser.Parse(questionText);
                    var queryByProjectId = NumericRangeQuery.NewInt32Range("pId", projectId, projectId, true, true);

                    query.Add(keywordsQuery, Occur.SHOULD);
                    query.Add(queryByProjectId, Occur.MUST);

                    var docs = searcher.Search(query, 10);
                    var searchResults = new List<LuceneSearchResult>();

                    foreach (var scoreDoc in docs.ScoreDocs)
                    {
                        var doc = searcher.Doc(scoreDoc.Doc);
                        var documentData = new LuceneSearchResult
                        {
                            QuestionDbId = int.Parse(doc.Get("Id")),
                            QuestionProjectDbId = int.Parse(doc.Get("pId")),
                            QuestionText = doc.Get("Text"),
                            Score = scoreDoc.Score
                        };
                        searchResults.Add(documentData);
                    }

                    return searchResults;
                    //var keywordsQuery = parser.CreatePhraseQuery("Name", "today was a great day");// Parse("today was a great day");
                    //var termQuery = new TermQuery(new Term("Brand", "Fender"));
                    //PhraseQuery keywords2Query = parser.CreatePhraseQuery("Name", "today was a great day") as PhraseQuery;

                    //var phraseQuery = new PhraseQuery();
                    //phraseQuery.Add()
                    //foreach(var t in keywordsQuery.)
                    ////phraseQuery.Add(new Term("Name", "today"));
                    ////phraseQuery.Add(new Term("Name", "day"));
                    //query.Add(keywords2Query, Occur.SHOULD);
                    ////query.Add(termQuery, Occur.MUST_NOT);
                    ////query.Add(phraseQuery, Occur.SHOULD);
                    //var t = new Term("Name", "today was a great day");
                    //var query2 = new FuzzyQuery(t, 2);// 2
                }
            }
        }

        internal void AddDataToIndex(Question question)
        {
            // TODO: temp solution!
            var list = new List<Question>() { question };
            BuildIndexWithExistingData(list);
        }

        internal void BuildIndexWithExistingData(List<Question> questions, bool clearIndex = false)
        {
            using (var directory = GetDirectory())
            using (var analyzer = GetAnalyzer())
            {
                var indexWriterConfig = new IndexWriterConfig(Lucene.Net.Util.LuceneVersion.LUCENE_48, analyzer);

                using (var writer = new IndexWriter(directory, indexWriterConfig))//, IndexWriter.MaxFieldLength.UNLIMITED))
                {
                    if (clearIndex)
                        writer.DeleteAll();

                    foreach (var question in questions)
                    {
                        var document = MapProduct(question);
                        writer.AddDocument(document);
                    }
                    writer.Commit();
                }
            }
        }

        private Document MapProduct(Question question)
        {
            var document = new Document();
            document.Add(new Int32Field("Id", question.Id, Field.Store.YES));
            document.Add(new Int32Field("pId", question.ProjectId, Field.Store.YES)); // ProjectId
            document.Add(new TextField("Text", question.Text, Field.Store.YES)); // TODO: after debugging, this value should be NO
            return document;
        }

        internal int GetDocumentsCount()
        {
            var directory = GetDirectory();
            var allDocuments = directory.ListAll();
            var documentsCount = allDocuments.Count();
            return documentsCount;
        }

        /*
        private Query GetQuery(string keywords)
        {
            using (var analyzer = GetAnalyzer())
            {
                var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "Name", analyzer);

                var query = new BooleanQuery();
                var keywordsQuery = parser.Parse(keywords);
                var termQuery = new TermQuery(new Term("Brand", "Fender"));

                var phraseQuery = new PhraseQuery();
                phraseQuery.Add(new Term("Name", "electric"));
                phraseQuery.Add(new Term("Name", "guitar"));
                query.Add(keywordsQuery, Occur.MUST);
                query.Add(termQuery, Occur.MUST_NOT);
                query.Add(phraseQuery, Occur.SHOULD);
                return query; // +Name:ibanez -Brand:Fender Name:"electric guitar" 
            }
        }

        private Sort GetSort()
        {
            var fields = new[]
            {
                new SortField("Brand", SortField.STRING),
                SortField.FIELD_SCORE
            };
            return new Sort(fields); // sort by brand, then by score 
        }

        private Filter GetFilter()
        {
            return NumericRangeFilter.NewIntRange("Id", 2, 5, true, false); // [2; 5) range 
        }
        */
        private Lucene.Net.Store.Directory GetDirectory()
        {
            var directoryInfo = new DirectoryInfo(_LuceneIndexDirectory);

            if (!directoryInfo.Exists)
                directoryInfo.Create();

            var simpleFSDirectory = new SimpleFSDirectory(directoryInfo);
            return simpleFSDirectory;
        }

        private Analyzer GetAnalyzer()
        {
            var analyzer = new StandardAnalyzer(Lucene.Net.Util.LuceneVersion.LUCENE_48);
            return analyzer;
        }
    }
}
