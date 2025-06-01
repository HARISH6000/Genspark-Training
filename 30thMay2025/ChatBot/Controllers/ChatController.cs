using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json; // For ReadFromJsonAsync and PostAsJsonAsync
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration; // To access appsettings.json
using System.Collections.Generic;
using ChatBot.Models; // Import your custom models
using System; // Required for Console.WriteLine

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
            // Retrieve API key and other settings from appsettings.json
            _geminiApiKey = configuration["GeminiApi:ApiKey"];
            _geminiModel = "gemini-2.0-flash"; // Using a common free-tier model suitable for free tier
            _geminiApiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{_geminiModel}:generateContent?key={_geminiApiKey}";

            // You can set a base address for HttpClient if all Gemini calls use the same base URL
            // _httpClient.BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1beta/");
        }

        /// <summary>
        /// Chats with the HDFC Bank FAQ chatbot.
        /// </summary>
        /// <param name="request">The chat request containing the user's message.</param>
        /// <returns>A chat response with the chatbot's reply or an error message.</returns>
        [HttpPost("ask")]
        public async Task<ActionResult<ChatResponse>> AskFaq([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new ChatResponse { Error = "Message cannot be empty." });
            }

            try
            {
                // **THE KEY CHANGE IS HERE: Constructing the prompt for the HDFC Bank persona**
                string prompt = $@"
                    You are an AI assistant for HDFC Bank.
                    Your primary role is to answer frequently asked questions about HDFC Bank's products, services, policies, and general banking inquiries specific to HDFC.
                    Answer concisely and directly.
                    If the user's question is not directly related to HDFC Bank, or if you do not have specific information, politely state that you can only assist with HDFC Bank-related inquiries and suggest visiting the official HDFC Bank website (www.hdfcbank.com) or contacting HDFC Bank customer service for further assistance.
                    Do not invent information.

                    Question: {request.Message}
                ";

                // Prepare the Gemini API request payload
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
                    // You can add generationConfig here if you want more control over the response,
                    // e.g., temperature for creativity, maxOutputTokens for length.
                    // generationConfig = new GenerationConfig { Temperature = 0.5, MaxOutputTokens = 150 }
                };

                // Send the request to the Gemini API
                var response = await _httpClient.PostAsJsonAsync(_geminiApiUrl, geminiRequest);

                // Check if the API call was successful
                if (response.IsSuccessStatusCode)
                {
                    var geminiResponse = await response.Content.ReadFromJsonAsync<GeminiResponse>();

                    // Extract the text from the Gemini response
                    if (geminiResponse?.Candidates != null && geminiResponse.Candidates.Count > 0)
                    {
                        var replyText = geminiResponse.Candidates[0].Content?.Parts?[0]?.Text;
                        if (!string.IsNullOrWhiteSpace(replyText))
                        {
                            return Ok(new ChatResponse { Reply = replyText.Trim() }); // Trim to remove potential leading/trailing whitespace
                        }
                    }
                    return StatusCode(500, new ChatResponse { Error = "Gemini API returned an empty or unreadable response." });
                }
                else
                {
                    // Read and log the error response from Gemini
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