
using System.ComponentModel.DataAnnotations;
using RaceDay.Web.Models;

namespace RaceDay.Web.ViewModels
{
    public class ManageResultsViewModel
    {
        public int EventId { get; set; }

        public string EventName { get; set; } = "";

        public List<Enrolment> Enrolments { get; set; } = new();

        public List<Result> Results { get; set; } = new();

        [Range(1, int.MaxValue,
            ErrorMessage = "Please select a confirmed participant.")]
        public int EnrolmentId { get; set; }

        [Required]
        public TimeSpan FinishTime { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Finishing position must be at least 1.")]
        public int FinishingPosition { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Total finishers must be at least 1.")]
        public int TotalFinishers { get; set; }
    }
}
