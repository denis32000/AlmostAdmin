namespace AlmostAdmin.Data
{
    public class LuceneSearchResult
    {
        public int QuestionDbId { get; set; }
        public int QuestionProjectDbId { get; set; }
        public string QuestionText { get; set; }
        public float Score { get; internal set; }
    }
}
