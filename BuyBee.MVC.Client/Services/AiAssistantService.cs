using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using EMart.ViewModels;

namespace EMart.Services
{
    public class AiAssistantService : IAiAssistantService
    {
        private readonly HttpClient _httpClient;
        private readonly IMcpClientService _mcpClientService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AiAssistantService> _logger;

        private const string DefaultGroqModel = "qwen/qwen3.8-27b";
        private static readonly string[] ModelCandidates = new[]
        {
            "qwen/qwen3.8-27b",
            "openai/gpt-oss-20b",
            "openai/gpt-oss-120b"
        };
        private const string GroqEndpoint = "https://api.groq.com/openai/v1/chat/completions";

        public AiAssistantService(
            HttpClient httpClient,
            IMcpClientService mcpClientService,
            IConfiguration configuration,
            ILogger<AiAssistantService> logger)
        {
            _httpClient = httpClient;
            _mcpClientService = mcpClientService;
            _configuration = configuration;
            _logger = logger;
            _httpClient.Timeout = TimeSpan.FromSeconds(60);
        }

        private string GetGroqApiKey()
        {
            var apiKey = _configuration["GROQ_API_KEY"]
                         ?? Environment.GetEnvironmentVariable("GROQ_API_KEY")
                         ?? _configuration["Groq:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                DotEnvLoader.Load();
                apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY");
            }

            return apiKey?.Trim() ?? string.Empty;
        }

        private string GetGroqModel()
        {
            return _configuration["Groq:Model"]
                   ?? Environment.GetEnvironmentVariable("GROQ_MODEL")
                   ?? DefaultGroqModel;
        }

        public async Task<AiStatusResponse> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            var apiKey = GetGroqApiKey();
            var mcpReady = await _mcpClientService.PingAsync(cancellationToken);
            var tools = await _mcpClientService.GetToolsAsync(false, cancellationToken);

            return new AiStatusResponse
            {
                GroqReady = !string.IsNullOrEmpty(apiKey),
                GroqModel = GetGroqModel(),
                McpServerReady = mcpReady,
                McpEndpoint = _mcpClientService.GetActiveEndpoint(),
                ToolCount = tools.Count,
                AvailableTools = tools.Select(t => t.Name).ToList()
            };
        }

        public async Task<AiChatResponse> ProcessChatAsync(AiChatRequest request, CancellationToken cancellationToken = default)
        {
            var apiKey = GetGroqApiKey();
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return new AiChatResponse
                {
                    Success = false,
                    Message = "Groq API Key is not configured. Please ensure GROQ_API_KEY is present in your .env or configuration.",
                    Error = "GROQ_API_KEY_MISSING"
                };
            }

            var model = GetGroqModel();
            var toolsUsed = new List<string>();
            var extractedProducts = new List<AiProductCard>();

            try
            {
                // 1. Fetch available tools from MCP
                var openAiTools = await _mcpClientService.ConvertToolsToOpenAiFormatAsync(cancellationToken);

                // 2. Build conversation history
                var messagesArray = new JsonArray();

                // System Prompt
                var systemPrompt = BuildSystemPrompt(request.ContextProductId, request.ContextUrl);
                messagesArray.Add(new JsonObject
                {
                    ["role"] = "system",
                    ["content"] = systemPrompt
                });

                // User / Assistant messages
                if (request.Messages != null)
                {
                    // Keep last 15 messages to prevent context overflow
                    var recentMessages = request.Messages.TakeLast(15);
                    foreach (var msg in recentMessages)
                    {
                        var role = string.Equals(msg.Role, "assistant", StringComparison.OrdinalIgnoreCase) 
                            ? "assistant" 
                            : "user";

                        messagesArray.Add(new JsonObject
                        {
                            ["role"] = role,
                            ["content"] = msg.Content ?? string.Empty
                        });
                    }
                }

                // 3. Multi-turn execution loop for tool calls
                const int maxTurns = 5;
                string assistantFinalContent = string.Empty;

                for (int turn = 0; turn < maxTurns; turn++)
                {
                    var requestBody = new JsonObject
                    {
                        ["model"] = model,
                        ["messages"] = messagesArray.DeepClone(),
                        ["max_completion_tokens"] = 2048
                    };

                    if (openAiTools.Count > 0)
                    {
                        var toolsClone = new JsonArray();
                        foreach (var tool in openAiTools)
                        {
                            toolsClone.Add(tool.DeepClone());
                        }
                        requestBody["tools"] = toolsClone;
                        requestBody["tool_choice"] = "auto";
                    }

                    HttpResponseMessage httpResponse = null!;
                    string responseBody = string.Empty;

                    // Try current model, fall back to other candidates if rate-limited (429)
                    var modelsToTry = new List<string> { model };
                    foreach (var c in ModelCandidates)
                    {
                        if (!modelsToTry.Contains(c)) modelsToTry.Add(c);
                    }

                    bool callSucceeded = false;
                    foreach (var attemptModel in modelsToTry)
                    {
                        requestBody["model"] = attemptModel;
                        var httpRequest = new HttpRequestMessage(HttpMethod.Post, GroqEndpoint)
                        {
                            Content = new StringContent(requestBody.ToJsonString(), Encoding.UTF8, "application/json")
                        };
                        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                        httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
                        responseBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                        if (httpResponse.IsSuccessStatusCode)
                        {
                            model = attemptModel;
                            callSucceeded = true;
                            break;
                        }

                        if (httpResponse.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            _logger.LogWarning("Model {AttemptModel} rate-limited (429). Trying fallback candidate...", attemptModel);
                            continue;
                        }

                        break;
                    }

                    if (!callSucceeded)
                    {
                        _logger.LogError("Groq API error ({StatusCode}): {Body}", httpResponse?.StatusCode, responseBody);
                        return new AiChatResponse
                        {
                            Success = false,
                            Message = $"AI Service communication error ({httpResponse?.StatusCode}). Please check Groq service or try again.",
                            Error = responseBody
                        };
                    }

                    using var doc = JsonDocument.Parse(responseBody);
                    var root = doc.RootElement;
                    var choice = root.GetProperty("choices")[0];
                    var messageEl = choice.GetProperty("message");

                    var finishReason = choice.TryGetProperty("finish_reason", out var frEl) ? frEl.GetString() : null;

                    // Check for tool calls
                    if (messageEl.TryGetProperty("tool_calls", out var toolCallsEl) && toolCallsEl.ValueKind == JsonValueKind.Array && toolCallsEl.GetArrayLength() > 0)
                    {
                        // Add assistant message with tool calls to message history
                        var assistantMsgObj = new JsonObject
                        {
                            ["role"] = "assistant"
                        };

                        if (messageEl.TryGetProperty("content", out var cEl) && cEl.ValueKind == JsonValueKind.String)
                        {
                            assistantMsgObj["content"] = cEl.GetString();
                        }
                        else
                        {
                            assistantMsgObj["content"] = null;
                        }

                        var toolCallsArray = new JsonArray();
                        foreach (var tc in toolCallsEl.EnumerateArray())
                        {
                            toolCallsArray.Add(JsonNode.Parse(tc.GetRawText()));
                        }
                        assistantMsgObj["tool_calls"] = toolCallsArray;
                        messagesArray.Add(assistantMsgObj);

                        // Execute each tool call
                        foreach (var tc in toolCallsEl.EnumerateArray())
                        {
                            var callId = tc.GetProperty("id").GetString() ?? "";
                            var functionEl = tc.GetProperty("function");
                            var funcName = functionEl.GetProperty("name").GetString() ?? "";
                            var argsStr = functionEl.GetProperty("arguments").GetString() ?? "{}";

                            if (!toolsUsed.Contains(funcName))
                            {
                                toolsUsed.Add(funcName);
                            }

                            _logger.LogInformation("Executing MCP Tool: {ToolName} with args: {Args}", funcName, argsStr);

                            JsonElement argsElement;
                            try
                            {
                                using var argsDoc = JsonDocument.Parse(argsStr);
                                argsElement = argsDoc.RootElement.Clone();
                            }
                            catch
                            {
                                using var emptyDoc = JsonDocument.Parse("{}");
                                argsElement = emptyDoc.RootElement.Clone();
                            }

                            var toolResult = await _mcpClientService.CallToolAsync(funcName, argsElement, cancellationToken);
                            var outputText = toolResult.IsError 
                                ? $"Error: {toolResult.ErrorMessage ?? "Tool execution failed"}"
                                : toolResult.RawResult;

                            // Attempt to extract products from tool output
                            ExtractProductsFromToolOutput(funcName, outputText, extractedProducts);

                            // Append tool response
                            messagesArray.Add(new JsonObject
                            {
                                ["role"] = "tool",
                                ["tool_call_id"] = callId,
                                ["content"] = outputText
                            });
                        }

                        // Continue the loop to get assistant's response to the tool results
                        continue;
                    }

                    // No more tool calls: capture final assistant response
                    if (messageEl.TryGetProperty("content", out var contentEl))
                    {
                        assistantFinalContent = contentEl.GetString() ?? string.Empty;
                    }
                    break;
                }

                if (string.IsNullOrWhiteSpace(assistantFinalContent) && extractedProducts.Count > 0)
                {
                    assistantFinalContent = "Here are the relevant products I found from our catalog:";
                }

                // Deduplicate extracted products
                var uniqueProducts = extractedProducts
                    .GroupBy(p => p.ProductId)
                    .Select(g => g.First())
                    .ToList();

                return new AiChatResponse
                {
                    Success = true,
                    Message = assistantFinalContent,
                    Products = uniqueProducts,
                    ToolsUsed = toolsUsed
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while processing AI chat request");
                return new AiChatResponse
                {
                    Success = false,
                    Message = "I apologize, but an unexpected error occurred while processing your request. Please try again.",
                    Error = ex.Message
                };
            }
        }

        private static string BuildSystemPrompt(int? contextProductId, string? contextUrl)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are BeeBot, the intelligent and friendly AI Personal Shopping Assistant for Buybee (an e-commerce platform).");
            sb.AppendLine("Your goal is to provide exceptional, delightful, and accurate shopping assistance to customers.");
            sb.AppendLine();
            sb.AppendLine("Key Rules & Capabilities:");
            sb.AppendLine("1. All prices are in Indian Rupees (INR, ₹). Format prices clearly (e.g. ₹29,990).");
            sb.AppendLine("2. You have access to real-time tools connected to the Buybee MCP Server. ALWAYS use these tools when customers ask for products, categories, stock levels, brands, price filters, or product details.");
            sb.AppendLine("   - `search_products`: search items by keywords, category, brand, price range, stock availability.");
            sb.AppendLine("   - `get_product` / `get_product_details`: get detailed specifications and image information.");
            sb.AppendLine("   - `check_product_stock`: check real-time stock quantity, status, and inventory.");
            sb.AppendLine("   - `get_categories` / `search_categories`: discover store categories.");
            sb.AppendLine("   - `find_products_by_category`, `find_products_by_brand`, `find_products_by_price_range`: specialized filtering.");
            sb.AppendLine("3. When tools return products, summarize the best matches, highlight key features, specifications, and prices. The user interface will automatically display interactive product cards for the products you find, so you don't need to write repetitive URLs, but you SHOULD mention their names, prices, and standout features.");
            sb.AppendLine("4. If a customer is looking for recommendations, budget-friendly options, or comparisons, provide helpful, well-structured suggestions with pros/cons.");
            sb.AppendLine("5. Maintain an enthusiastic, polite, and helpful tone. Use markdown formatting (bold, bullet points) to make recommendations easy to read.");
            sb.AppendLine("6. Loyalty & Rewards (BeeCoins): Buybee customers earn 5 BeeCoins per ₹100 spent (5% back). 1 BeeCoin = ₹1.00 instant discount at checkout! Customers can view their passbook at `/BeeCoins` and redeem coins during checkout.");
            sb.AppendLine("7. Smart Reorder Hub ('Buy Again'): Customers can reorder daily staples, groceries (Tata Tea, Parle-G, Maggi noodles), personal care (Dove Shampoo, Colgate) in 1 click at `/Reorder`. If a customer asks 'What should I reorder?' or 'Buy again', enthusiastically recommend essential consumable pantry and personal care items and mention the 1-click Reorder Hub!");
            sb.AppendLine("8. Live Order Tracking: Customers can track active shipments with live interactive route maps, delivery executive contact, and secure delivery OTP at `/Order/Track/{orderId}`.");

            if (contextProductId.HasValue && contextProductId.Value > 0)
            {
                sb.AppendLine($"[Current Page Context]: The customer is currently viewing Product ID: {contextProductId.Value}. If relevant, you can check its details or suggest complementary items.");
            }

            return sb.ToString();
        }

        private static void ExtractProductsFromToolOutput(string toolName, string rawOutput, List<AiProductCard> targetList)
        {
            if (string.IsNullOrWhiteSpace(rawOutput)) return;

            try
            {
                var trimmed = rawOutput.Trim();

                // If output is a JSON array
                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    using var doc = JsonDocument.Parse(trimmed);
                    foreach (var item in doc.RootElement.EnumerateArray())
                    {
                        var product = ParseProductElement(item);
                        if (product != null)
                        {
                            targetList.Add(product);
                        }
                    }
                }
                // If output is a single JSON object
                else if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
                {
                    using var doc = JsonDocument.Parse(trimmed);
                    var root = doc.RootElement;

                    // Could be {"product": {...}, "images": [...]} or direct product
                    if (root.TryGetProperty("product", out var nestedProduct))
                    {
                        var product = ParseProductElement(nestedProduct);
                        if (product != null)
                        {
                            targetList.Add(product);
                        }
                    }
                    else
                    {
                        var product = ParseProductElement(root);
                        if (product != null)
                        {
                            targetList.Add(product);
                        }
                    }
                }
            }
            catch
            {
                // Silently skip if output is not a product JSON (e.g. categories, welcome message, etc.)
            }
        }

        private static AiProductCard? ParseProductElement(JsonElement el)
        {
            try
            {
                if (el.ValueKind != JsonValueKind.Object) return null;

                // Needs at least a productId or productName
                int productId = 0;
                if (el.TryGetProperty("productId", out var idEl) && idEl.TryGetInt32(out var idVal))
                {
                    productId = idVal;
                }
                else if (el.TryGetProperty("ProductId", out var idEl2) && idEl2.TryGetInt32(out var idVal2))
                {
                    productId = idVal2;
                }

                string name = "";
                if (el.TryGetProperty("productName", out var nameEl)) name = nameEl.GetString() ?? "";
                else if (el.TryGetProperty("ProductName", out var nameEl2)) name = nameEl2.GetString() ?? "";

                if (productId <= 0 && string.IsNullOrWhiteSpace(name))
                {
                    return null;
                }

                decimal price = 0;
                if (el.TryGetProperty("price", out var pEl) && pEl.TryGetDecimal(out var pVal)) price = pVal;
                else if (el.TryGetProperty("Price", out var pEl2) && pEl2.TryGetDecimal(out var pVal2)) price = pVal2;

                int quantity = 0;
                if (el.TryGetProperty("quantity", out var qEl) && qEl.TryGetInt32(out var qVal)) quantity = qVal;
                else if (el.TryGetProperty("Quantity", out var qEl2) && qEl2.TryGetInt32(out var qVal2)) quantity = qVal2;
                else if (el.TryGetProperty("availableQuantity", out var aqEl) && aqEl.TryGetInt32(out var aqVal)) quantity = aqVal;

                string? sku = null;
                if (el.TryGetProperty("sku", out var skuEl)) sku = skuEl.GetString();
                else if (el.TryGetProperty("Sku", out var skuEl2)) sku = skuEl2.GetString();

                string? brand = null;
                if (el.TryGetProperty("brandName", out var bEl)) brand = bEl.GetString();
                else if (el.TryGetProperty("BrandName", out var bEl2)) brand = bEl2.GetString();

                string? category = null;
                if (el.TryGetProperty("categoryName", out var cEl)) category = cEl.GetString();
                else if (el.TryGetProperty("CategoryName", out var cEl2)) category = cEl2.GetString();

                string? image = null;
                if (el.TryGetProperty("thumbnailImage", out var imgEl)) image = imgEl.GetString();
                else if (el.TryGetProperty("ThumbnailImage", out var imgEl2)) image = imgEl2.GetString();
                else if (el.TryGetProperty("imageUrl", out var imgEl3)) image = imgEl3.GetString();

                string? desc = null;
                if (el.TryGetProperty("description", out var dEl)) desc = dEl.GetString();
                else if (el.TryGetProperty("Description", out var dEl2)) desc = dEl2.GetString();

                bool isActive = true;
                if (el.TryGetProperty("isActive", out var actEl)) isActive = actEl.GetBoolean();

                return new AiProductCard
                {
                    ProductId = productId,
                    Sku = sku,
                    ProductName = name,
                    BrandName = brand,
                    CategoryName = category,
                    Price = price,
                    Quantity = quantity,
                    ThumbnailImage = image,
                    Description = desc,
                    IsActive = isActive
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
