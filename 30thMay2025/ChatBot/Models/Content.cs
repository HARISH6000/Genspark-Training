using ChatBot.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ChatBot.Models{
    public class Content
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";

        [JsonPropertyName("parts")]
        public List<Part> Parts { get; set; }
    }
}