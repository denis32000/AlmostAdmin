using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AlmostAdmin.Models
{
    public class Answer
    {
        public Answer()
        {
            Questions = new List<Question>();
        }

        [Key]
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }

        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public virtual ICollection<Question> Questions { get; set; }
    }
}
