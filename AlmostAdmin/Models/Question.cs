using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AlmostAdmin.Models
{
    public class Question
    {
        public Question()
        {
            QuestionTags = new List<QuestionTag>();
        }

        [Key]
        public int Id { get; set; }
        public string Text { get; set; }
        public string InteligenceValue { get; set; }

        public DateTime Date { get; set; }
        public bool AnsweredByHuman { get; set; }
        
        public int? AnswerId { get; set; }
        public Answer Answer { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public virtual ICollection<QuestionTag> QuestionTags { get; set; }
    }
}
