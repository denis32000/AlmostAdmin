using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AlmostAdmin.Models
{
    public class Project
    {
        public Project()
        {
            UserProjects = new List<UserProject>();
            Questions = new List<Question>();
            //Answers = new List<Answer>();
            //Tags = new List<Tag>();
        }

        [Key]
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }

        public virtual ICollection<UserProject> UserProjects { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
        //public virtual ICollection<Answer> Answers { get; set; }
        //public virtual ICollection<Tag> Tags { get; set; }
    }
}
