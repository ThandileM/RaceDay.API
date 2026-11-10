
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RaceDay.Web.ViewModels
{
    public class CreateEnrolmentViewModel
    {
        [Required]
        public int EventId { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category.")]
        [Display(Name = "Event Category")]
        public int CategoryId { get; set; }

        public string EventName { get; set; } = "";

        public List<SelectListItem> Categories { get; set; } = new();
    }
}
