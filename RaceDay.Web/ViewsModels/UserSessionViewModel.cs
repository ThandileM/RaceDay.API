using RaceDay.Web.ViewModels;

namespace RaceDay.Web.ViewModels
{
    public class UserSessionViewModel
    {
        public int UserId { get; set; }
        public string UserRole { get; set; } = "";
        public int LoginCount { get; set; }
    }
}
