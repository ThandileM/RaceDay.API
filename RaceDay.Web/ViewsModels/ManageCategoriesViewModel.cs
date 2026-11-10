
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using RaceDay.Web.Models;

namespace RaceDay.Web.ViewModels
{
    public class ManageCategoriesViewModel
    {
        public int EventId { get; set; }

        public string EventName { get; set; } = "";

        [Required(ErrorMessage = "Please enter a category name.")]
        [MaxLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = "";

        [MaxLength(255, ErrorMessage = "Description cannot exceed 255 characters.")]
        public string? Description { get; set; }

        public List<Category> Categories { get; set; } = new();

        public List<SelectListItem> Events { get; set; } = new();
    }
}
