using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EMart.Services
{
    public class McpClientService : IMcpClientService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<McpClientService> _logger;

        private List<McpToolDefinition>? _cachedTools;
        private DateTime _cacheExpiration = DateTime.MinValue;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private string? _activeEndpoint;

        public McpClientService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<McpClientService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _httpClient.Timeout = TimeSpan.FromSeconds(25);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/event-stream");
            _httpClient.DefaultRequestHeaders.Add("ngrok-skip-browser-warning", "true");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BuybeeClient/1.0");
        }

        public string GetActiveEndpoint()
        {
            if (!string.IsNullOrEmpty(_activeEndpoint))
            {
                return _activeEndpoint;
            }

            var ngrokUrl = _configuration["Mcp:NgrokUrl"] 
                           ?? Environment.GetEnvironmentVariable("MCP_SERVER_URL")
                           ?? "https://prodigy-landmark-drier.ngrok-free.dev/mcp";

            return ngrokUrl;
        }

        private async Task<string> ResolveWorkingEndpointAsync(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(_activeEndpoint))
            {
                return _activeEndpoint;
            }

            var endpoints = new List<string>();

            var ngrok = _configuration["Mcp:NgrokUrl"] 
                        ?? Environment.GetEnvironmentVariable("MCP_SERVER_URL")
                        ?? "https://prodigy-landmark-drier.ngrok-free.dev/mcp";
            if (!string.IsNullOrWhiteSpace(ngrok)) endpoints.Add(ngrok.Trim());

            var local = _configuration["Mcp:LocalUrl"] ?? "http://localhost:5259/mcp";
            if (!string.IsNullOrWhiteSpace(local) && !endpoints.Contains(local.Trim()))
            {
                endpoints.Add(local.Trim());
            }

            foreach (var endpoint in endpoints)
            {
                try
                {
                    var req = new HttpRequestMessage(HttpMethod.Post, endpoint)
                    {
                        Content = new StringContent(
                            JsonSerializer.Serialize(new { jsonrpc = "2.0", method = "tools/list", id = 1 }),
                            Encoding.UTF8,
                            "application/json")
                    };

                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    cts.CancelAfter(TimeSpan.FromSeconds(5));

                    var resp = await _httpClient.SendAsync(req, cts.Token);
                    if (resp.IsSuccessStatusCode)
                    {
                        _activeEndpoint = endpoint;
                        _logger.LogInformation("Resolved working MCP Server endpoint: {Endpoint}", _activeEndpoint);
                        return _activeEndpoint;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Endpoint test failed for {Endpoint}: {Message}", endpoint, ex.Message);
                }
            }

            // Default to ngrok
            _activeEndpoint = endpoints.FirstOrDefault() ?? "http://localhost:5259/mcp";
            return _activeEndpoint;
        }

        public async Task<List<McpToolDefinition>> GetToolsAsync(bool forceRefresh = false, CancellationToken cancellationToken = default)
        {
            if (!forceRefresh && _cachedTools != null && DateTime.UtcNow < _cacheExpiration)
            {
                return _cachedTools;
            }

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (!forceRefresh && _cachedTools != null && DateTime.UtcNow < _cacheExpiration)
                {
                    return _cachedTools;
                }

                var endpoint = await ResolveWorkingEndpointAsync(cancellationToken);
                var requestBody = JsonSerializer.Serialize(new
                {
                    jsonrpc = "2.0",
                    method = "tools/list",
                    id = 1
                });

                var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
                };

                var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                var parsedData = ExtractDataPayload(rawResponse);

                if (string.IsNullOrEmpty(parsedData))
                {
                    throw new InvalidOperationException("Empty or invalid SSE payload received from MCP server.");
                }

                using var doc = JsonDocument.Parse(parsedData);
                var root = doc.RootElement;

                var tools = new List<McpToolDefinition>();

                if (root.TryGetProperty("result", out var resultEl) &&
                    resultEl.TryGetProperty("tools", out var toolsEl) &&
                    toolsEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var toolEl in toolsEl.EnumerateArray())
                    {
                        var name = toolEl.GetProperty("name").GetString() ?? string.Empty;
                        var desc = toolEl.TryGetProperty("description", out var descEl) ? descEl.GetString() ?? "" : "";
                        var schema = toolEl.TryGetProperty("inputSchema", out var schemaEl) ? schemaEl.Clone() : default;

                        tools.Add(new McpToolDefinition
                        {
                            Name = name,
                            Description = desc,
                            InputSchema = schema
                        });
                    }
                }

                _cachedTools = tools;
                _cacheExpiration = DateTime.UtcNow.AddMinutes(15);
                return tools;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve tools from MCP server.");
                // If we had cached tools before, return them as fallback
                if (_cachedTools != null)
                {
                    return _cachedTools;
                }
                return new List<McpToolDefinition>();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<McpToolCallResult> CallToolAsync(string toolName, JsonElement arguments, CancellationToken cancellationToken = default)
        {
            try
            {
                var endpoint = await ResolveWorkingEndpointAsync(cancellationToken);
                var requestPayload = new
                {
                    jsonrpc = "2.0",
                    method = "tools/call",
                    paramsObj = new
                    {
                        name = toolName,
                        arguments = arguments
                    },
                    id = DateTime.UtcNow.Ticks
                };

                // Create JSON-RPC body manually to guarantee "params" key
                var jsonNode = new JsonObject
                {
                    ["jsonrpc"] = "2.0",
                    ["method"] = "tools/call",
                    ["params"] = new JsonObject
                    {
                        ["name"] = toolName,
                        ["arguments"] = JsonNode.Parse(arguments.GetRawText()) ?? new JsonObject()
                    },
                    ["id"] = DateTime.UtcNow.Ticks
                };

                var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = new StringContent(jsonNode.ToJsonString(), Encoding.UTF8, "application/json")
                };

                var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                var parsedData = ExtractDataPayload(rawResponse);

                if (string.IsNullOrEmpty(parsedData))
                {
                    return new McpToolCallResult
                    {
                        ToolName = toolName,
                        IsError = true,
                        ErrorMessage = "No data returned from MCP tool call."
                    };
                }

                using var doc = JsonDocument.Parse(parsedData);
                var root = doc.RootElement;

                if (root.TryGetProperty("error", out var errorEl))
                {
                    return new McpToolCallResult
                    {
                        ToolName = toolName,
                        IsError = true,
                        ErrorMessage = errorEl.ToString()
                    };
                }

                if (root.TryGetProperty("result", out var resultEl))
                {
                    bool isError = resultEl.TryGetProperty("isError", out var isErrEl) && isErrEl.GetBoolean();
                    var textBuilder = new StringBuilder();

                    if (resultEl.TryGetProperty("content", out var contentEl) && contentEl.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in contentEl.EnumerateArray())
                        {
                            if (item.TryGetProperty("text", out var textEl))
                            {
                                textBuilder.Append(textEl.GetString());
                            }
                        }
                    }

                    return new McpToolCallResult
                    {
                        ToolName = toolName,
                        RawResult = textBuilder.ToString(),
                        IsError = isError
                    };
                }

                return new McpToolCallResult
                {
                    ToolName = toolName,
                    RawResult = parsedData,
                    IsError = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling MCP tool {ToolName}", toolName);
                return new McpToolCallResult
                {
                    ToolName = toolName,
                    IsError = true,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<List<JsonObject>> ConvertToolsToOpenAiFormatAsync(CancellationToken cancellationToken = default)
        {
            var mcpTools = await GetToolsAsync(false, cancellationToken);
            var openAiTools = new List<JsonObject>();

            foreach (var tool in mcpTools)
            {
                try
                {
                    JsonNode? schemaNode = null;
                    if (tool.InputSchema.ValueKind != JsonValueKind.Undefined)
                    {
                        schemaNode = JsonNode.Parse(tool.InputSchema.GetRawText());
                    }

                    schemaNode ??= new JsonObject { ["type"] = "object", ["properties"] = new JsonObject() };

                    var toolObj = new JsonObject
                    {
                        ["type"] = "function",
                        ["function"] = new JsonObject
                        {
                            ["name"] = tool.Name,
                            ["description"] = tool.Description,
                            ["parameters"] = schemaNode
                        }
                    };

                    openAiTools.Add(toolObj);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to convert tool {ToolName} to OpenAI schema", tool.Name);
                }
            }

            return openAiTools;
        }

        public async Task<bool> PingAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var tools = await GetToolsAsync(false, cancellationToken);
                return tools.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        private static string ExtractDataPayload(string sseOrJson)
        {
            if (string.IsNullOrWhiteSpace(sseOrJson))
            {
                return string.Empty;
            }

            var trimmed = sseOrJson.Trim();
            if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
            {
                return trimmed;
            }

            var lines = trimmed.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                {
                    return line.Substring(5).Trim();
                }
            }

            return trimmed;
        }
    }
}
