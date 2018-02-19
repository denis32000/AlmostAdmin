using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AlmostAdmin.Models
{
    public class Project
    {
        public Project()
        {
            Users = new List<User>();
            Questions = new List<Question>();
            Answers = new List<Answer>();
            Tags = new List<Tag>();
        }

        [Key]
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }

        public ICollection<User> Users { get; set; }
        public ICollection<Question> Questions { get; set; }
        public ICollection<Answer> Answers { get; set; }
        public ICollection<Tag> Tags { get; set; }
    }
}
