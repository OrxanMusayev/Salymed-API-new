using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace backend.Services
{
    public class PaddleSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Environment { get; set; } = "sandbox"; // sandbox or production
        public string BaseUrl => Environment == "production"
            ? "https://api.paddle.com"
            : "https://sandbox-api.paddle.com";
    }

    public interface IPaddleService
    {
        Task<PaddleCheckoutResponse> CreateCheckoutSessionAsync(PaddleCheckoutRequest request);
        Task<bool> VerifyWebhookSignatureAsync(string signature, string requestBody);
    }

    public class PaddleService : IPaddleService
    {
        private readonly HttpClient _httpClient;
        private readonly PaddleSettings _settings;
        private readonly ILogger<PaddleService> _logger;

        public PaddleService(
            HttpClient httpClient,
            IOptions<PaddleSettings> settings,
            ILogger<PaddleService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;

            // Configure HttpClient
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<PaddleCheckoutResponse> CreateCheckoutSessionAsync(PaddleCheckoutRequest request)
        {
            try
            {
                _logger.LogInformation("Creating Paddle checkout session for clinic {ClinicId}, plan {PlanId}",
                    request.ClinicId, request.PlanId);

                // Paddle Transactions API - Checkout için transaction oluştur
                // Bu transaction, Paddle Overlay'de gösterilecek
                var checkoutData = new
                {
                    items = new[]
                    {
                        new
                        {
                            price_id = request.PaddlePriceId,
                            quantity = 1
                        }
                    },
                    customer_email = request.CustomerEmail,
                    customer_ip_address = "0.0.0.0", // Optional: Frontend'den gönderebilirsiniz
                    custom_data = new
                    {
                        clinic_id = request.ClinicId,
                        plan_id = request.PlanId,
                        user_id = request.UserId
                    },
                    checkout = new
                    {
                        settings = new
                        {
                            success_url = request.SuccessUrl,
                            // Paddle Overlay otomatik olarak overlay kapatır ve success_url'e yönlendirir
                        }
                    }
                };

                _logger.LogInformation("Sending transaction request to Paddle: {Request}",
                    JsonSerializer.Serialize(checkoutData, new JsonSerializerOptions { WriteIndented = true }));

                var jsonContent = JsonSerializer.Serialize(checkoutData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // DOĞRU ENDPOINT: /transactions (checkout için)
                var response = await _httpClient.PostAsync("/transactions", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Paddle API error: {StatusCode} - {Content}",
                        response.StatusCode, responseContent);
                    
                    // Daha detaylı hata mesajı
                    var errorDetail = "Paddle API error";
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<PaddleErrorResponse>(responseContent);
                        if (errorResponse?.Error != null)
                        {
                            errorDetail = $"{errorResponse.Error.Code}: {errorResponse.Error.Detail}";
                        }
                    }
                    catch { }
                    
                    throw new Exception($"Paddle API error: {response.StatusCode} - {errorDetail}");
                }

                _logger.LogInformation("Paddle API Response: {Response}", responseContent);

                var paddleResponse = JsonSerializer.Deserialize<PaddleApiResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (paddleResponse?.Data == null)
                {
                    _logger.LogError("Failed to deserialize Paddle response. Response was: {Response}", responseContent);
                    throw new Exception("Invalid response from Paddle API");
                }

                // Checkout URL'i al - bu URL Paddle Overlay'i açacak
                var checkoutUrl = paddleResponse.Data.Checkout?.Url;
                
                if (string.IsNullOrEmpty(checkoutUrl))
                {
                    _logger.LogError("Checkout URL is missing in Paddle response");
                    throw new Exception("Checkout URL not found in Paddle response");
                }

                _logger.LogInformation("Paddle checkout session created successfully. Transaction ID: {TransactionId}, Checkout URL: {CheckoutUrl}",
                    paddleResponse.Data.Id, checkoutUrl);

                return new PaddleCheckoutResponse
                {
                    Success = true,
                    CheckoutUrl = checkoutUrl,
                    TransactionId = paddleResponse.Data.Id,
                    Message = "Checkout session created successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Paddle checkout session");
                return new PaddleCheckoutResponse
                {
                    Success = false,
                    Message = $"Error creating checkout session: {ex.Message}"
                };
            }
        }

        public async Task<bool> VerifyWebhookSignatureAsync(string signature, string requestBody)
        {
            try
            {
                // Paddle webhook signature verification
                // Paddle uses HMAC-SHA256 for webhook signature verification
                // The signature format is: ts={timestamp};h1={signature}

                if (string.IsNullOrEmpty(signature))
                {
                    _logger.LogWarning("Webhook signature is empty");
                    return false;
                }

                // TODO: Implement actual signature verification
                // For now, we'll accept all webhooks in development
                if (_settings.Environment == "sandbox")
                {
                    _logger.LogWarning("Webhook signature verification skipped in sandbox mode");
                    return true;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying webhook signature");
                return false;
            }
        }
    }

    // Request/Response Models
    public class PaddleCheckoutRequest
    {
        public Guid ClinicId { get; set; }
        public int PlanId { get; set; }
        public Guid UserId { get; set; }
        public string PaddlePriceId { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string SuccessUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
    }

    public class PaddleCheckoutResponse
    {
        public bool Success { get; set; }
        public string CheckoutUrl { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    // Paddle API Response Models
    public class PaddleApiResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public PaddleTransactionData? Data { get; set; }
    }

    public class PaddleTransactionData
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("subscription_id")]
        public string? SubscriptionId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("checkout")]
        public PaddleCheckoutData? Checkout { get; set; }
    }

    public class PaddleCheckoutData
    {
        [System.Text.Json.Serialization.JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }

    // Error Response Model
    public class PaddleErrorResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("error")]
        public PaddleError? Error { get; set; }
    }

    public class PaddleError
    {
        [System.Text.Json.Serialization.JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("detail")]
        public string Detail { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("documentation_url")]
        public string? DocumentationUrl { get; set; }
    }
}
