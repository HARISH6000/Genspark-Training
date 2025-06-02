using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json; 
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration; 
using System.Collections.Generic;
using ChatBot.Models; 
using System; 

namespace ChatBot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _geminiApiKey;
        private readonly string _geminiApiUrl;
        private readonly string _geminiModel;

        public ChatController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _geminiApiKey = configuration["GeminiApi:ApiKey"];
            _geminiModel = "gemini-2.0-flash"; // Using a common free-tier model suitable for free tier
            _geminiApiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{_geminiModel}:generateContent?key={_geminiApiKey}";
        }

        
        [HttpPost("ask")]
        public async Task<ActionResult<ChatResponse>> AskFaq([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new ChatResponse { Error = "Message cannot be empty." });
            }

            try
            {
                
                string prompt = $@"
                    You are an AI assistant for HDFC Bank.
                    Your primary role is to answer frequently asked questions about HDFC Bank's products, services, policies, and general banking inquiries specific to HDFC.
                    Answer concisely and directly.
                    If the user's question is not directly related to HDFC Bank, or if you do not have specific information, politely state that you can only assist with HDFC Bank-related inquiries and suggest visiting the official HDFC Bank website (www.hdfcbank.com) or contacting HDFC Bank customer service for further assistance.
                    Do not invent information.

                    Question: {request.Message}
                ";

                
                var geminiRequest = new GeminiRequest
                {
                    Contents = new List<Content>
                    {
                        new Content
                        {
                            Role = "user",
                            Parts = new List<Part> { new Part { Text = prompt } }
                        }
                    }
                };

                
                var response = await _httpClient.PostAsJsonAsync(_geminiApiUrl, geminiRequest);

                
                if (response.IsSuccessStatusCode)
                {
                    var geminiResponse = await response.Content.ReadFromJsonAsync<GeminiResponse>();

                    
                    if (geminiResponse?.Candidates != null && geminiResponse.Candidates.Count > 0)
                    {
                        var replyText = geminiResponse.Candidates[0].Content?.Parts?[0]?.Text;
                        if (!string.IsNullOrWhiteSpace(replyText))
                        {
                            return Ok(new ChatResponse { Reply = replyText.Trim() }); 
                        }
                    }
                    return StatusCode(500, new ChatResponse { Error = "Gemini API returned an empty or unreadable response." });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Gemini API Error: {response.StatusCode} - {errorContent}");
                    return StatusCode((int)response.StatusCode, new ChatResponse { Error = $"Failed to get response from Gemini API. Status: {response.StatusCode}. Details: {errorContent}" });
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle network errors or issues connecting to the Gemini API
                Console.WriteLine($"HttpRequestException: {ex.Message}");
                return StatusCode(500, new ChatResponse { Error = $"Network error communicating with Gemini API: {ex.Message}" });
            }
            catch (Exception ex)
            {
                // Catch any other unexpected errors
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                return StatusCode(500, new ChatResponse { Error = $"An internal server error occurred: {ex.Message}" });
            }
        }
    }
}