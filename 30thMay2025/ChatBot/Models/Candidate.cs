using System.Text.Json.Serialization;

namespace ChatBot.Models
{
    public class Candidate
    {
        [JsonPropertyName("content")]
        public Content Content { get; set; }

    }
}