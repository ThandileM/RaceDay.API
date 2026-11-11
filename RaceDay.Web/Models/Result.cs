
using System.Text.Json.Serialization;

namespace RaceDay.Web.Models
{
    public class Result
    {
        [JsonPropertyName("resultId")]
        public int ResultId { get; set; }

        [JsonPropertyName("enrolmentId")]
        public int EnrolmentId { get; set; }

        [JsonPropertyName("recordedById")]
        public int RecordedById { get; set; }

        [JsonPropertyName("finishTime")]
        public TimeSpan FinishTime { get; set; }

        [JsonPropertyName("finishingPosition")]
        public int FinishingPosition { get; set; }

        [JsonPropertyName("totalFinishers")]
        public int TotalFinishers { get; set; }

        [JsonPropertyName("recordedAt")]
        public DateTime RecordedAt { get; set; }

        [JsonPropertyName("participantName")]
        public string ParticipantName { get; set; } = "";

        [JsonPropertyName("eventName")]
        public string EventName { get; set; } = "";
    }
}
