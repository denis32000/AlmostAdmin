namespace AlmostAdmin.Models
{
    public class QuestionTag
    {
        public int QuestionId { get; set; }
        public virtual Question Question { get; set; }

        public int TagId { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
