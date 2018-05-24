using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        //public string InteligenceValue { get; set; }
        public DateTime Date { get; set; }

        public bool AnsweredByHuman { get; set; }
        public bool ApprovedByHuman { get; set; }

        [NotMapped]
        public bool HasApprovedAnswer { get { return AnsweredByHuman || ApprovedByHuman; } }

        public string StatusUrl { get; set; }
        public bool AnswerToEmail { get; set; }

        public int? AnswerId { get; set; }
        public virtual Answer Answer { get; set; }

        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }

        public virtual ICollection<QuestionTag> QuestionTags { get; set; }
    }
}
