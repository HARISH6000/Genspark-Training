using ChatBot.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ChatBot.Models
{
    public class GeminiRequest
    {
        [JsonPropertyName("contents")]
        public List<Content> Contents { get; set; }

    }
}