
using System.Text.Json.Serialization;

namespace RaceDay.Web.Models
{
    public class Enrolment
    {
        [JsonPropertyName("enrolmentId")]
        public int EnrolmentId { get; set; }

        [JsonPropertyName("participantId")]
        public int ParticipantId { get; set; }

        [JsonPropertyName("eventId")]
        public int EventId { get; set; }

        [JsonPropertyName("categoryId")]
        public int CategoryId { get; set; }

        [JsonPropertyName("statusId")]
        public int StatusId { get; set; }

        [JsonPropertyName("enrolmentDate")]
        public DateTime EnrolmentDate { get; set; }

        [JsonPropertyName("participantName")]
        public string ParticipantName { get; set; } = "";

        [JsonPropertyName("eventName")]
        public string EventName { get; set; } = "";

        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; } = "";

        [JsonPropertyName("statusName")]
        public string StatusName { get; set; } = "";
    }
}
