namespace AlmostAdmin.Models
{
    public class QuestionTag
    {
        public int QuestionId { get; set; }
        public Question Question { get; set; }

        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}
