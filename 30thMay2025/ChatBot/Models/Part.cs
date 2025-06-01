using System.Text.Json.Serialization;

namespace ChatBot.Models
{
    public class Part
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}