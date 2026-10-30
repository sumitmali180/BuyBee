using ModelContextProtocol.Server;
using System.ComponentModel;

namespace EMart.MCP.Server.Tools
{
    [McpServerToolType]
    public class EmartTools
    {
        [McpServerTool]
        [Description("Returns a welcome message from the EMart MCP Server.")]
        public string GetWelcomeMessage()
        {
            return "Welcome to EMart MCP Server";
        }
    }
}