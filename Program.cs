namespace AutoTestId.McpServer;

class Program
{
    static async Task Main(string[] args)
    {
        // Create MCP server without console logging to avoid interfering with JSON-RPC protocol
        var server = new McpServer();
        
        try
        {
            await server.RunAsync();
        }
        catch (Exception ex)
        {
            // Log to stderr instead of stdout to avoid breaking JSON-RPC
            Console.Error.WriteLine($"MCP Server Error: {ex.Message}");
            throw;
        }
    }
}
