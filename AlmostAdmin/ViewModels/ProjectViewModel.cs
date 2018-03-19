using System.ComponentModel.DataAnnotations;

namespace AlmostAdmin.ViewModels
{
    public class ProjectViewModel
    {
        [Required]
        [Display(Name = "Название проекта")]
        public string Name { get; set; }
    }
}
