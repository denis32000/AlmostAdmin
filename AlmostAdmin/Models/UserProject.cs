using System.ComponentModel.DataAnnotations.Schema;

namespace AlmostAdmin.Models
{
    public class UserProject
    {
        //[Navigation("Id")]
        public string UserId { get; set; }
        public virtual User User { get; set; }

        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }
    }
}
