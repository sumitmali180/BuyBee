using System.Text.Json;
using System.Text.Json.Nodes;

namespace EMart.Services
{
    public class McpToolDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public JsonElement InputSchema { get; set; }
    }

    public class McpToolCallResult
    {
        public string ToolName { get; set; } = string.Empty;
        public string RawResult { get; set; } = string.Empty;
        public bool IsError { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public interface IMcpClientService
    {
        Task<List<McpToolDefinition>> GetToolsAsync(bool forceRefresh = false, CancellationToken cancellationToken = default);
        Task<McpToolCallResult> CallToolAsync(string toolName, JsonElement arguments, CancellationToken cancellationToken = default);
        Task<List<JsonObject>> ConvertToolsToOpenAiFormatAsync(CancellationToken cancellationToken = default);
        Task<bool> PingAsync(CancellationToken cancellationToken = default);
        string GetActiveEndpoint();
    }
}
