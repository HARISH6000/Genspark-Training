using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ChatBot.Models
{

    public class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<Candidate> Candidates { get; set; }
    }
}