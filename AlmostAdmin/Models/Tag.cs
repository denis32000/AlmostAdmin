using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AlmostAdmin.Models
{
    public class Tag
    {
        public Tag()
        {
            QuestionTags = new List<QuestionTag>();
        }

        [Key]
        public int Id { get; set; }
        public string Text { get; set; }

        //public int ProjectId { get; set; }
        //public Project Project { get; set; }

        public virtual ICollection<QuestionTag> QuestionTags { get; set; }
    }
}
