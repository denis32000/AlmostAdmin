using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AlmostAdmin.Models
{
    public class Question
    {
        [Key]
        public int Id { get; set; }
        public string Text { get; set; }
        public string InteligenceValue { get; set; }

        public int AnswerId { get; set; }
        public Answer Answer { get; set; }
    }
}
