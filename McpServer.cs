using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using AutoTestId.McpServer.Tools;

namespace AutoTestId.McpServer;

public class McpServer
{
    private readonly ILogger<McpServer>? _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public McpServer(ILogger<McpServer>? logger = null)
    {
        _logger = logger; 
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task RunAsync()
    {
        var stdinReader = Console.In;
        var stdoutWriter = Console.Out;

        while (true)
        {
            try
            {
                var line = await stdinReader.ReadLineAsync();
                if (line == null) break; // EOF

                if (string.IsNullOrWhiteSpace(line)) continue;

                var response = await ProcessMessage(line);
                if (response != null)
                {
                    await stdoutWriter.WriteLineAsync(response);
                    await stdoutWriter.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error processing message");
                
                var errorResponse = CreateErrorResponse(null, -32603, "Internal error", ex.Message);
                await stdoutWriter.WriteLineAsync(errorResponse);
                await stdoutWriter.FlushAsync();
            }
        }
    }

    private async Task<string?> ProcessMessage(string message)
    {
        try
        {
            var jsonMessage = JsonNode.Parse(message);
            if (jsonMessage == null) return null;

            var method = jsonMessage["method"]?.GetValue<string>();
            var id = jsonMessage["id"];

            return method switch
            {
                "initialize" => await HandleInitialize(id),
                "notifications/initialized" => null, // No response needed
                "tools/list" => await HandleToolsList(id),
                "tools/call" => await HandleToolsCall(jsonMessage, id),
                _ => CreateErrorResponse(id, -32601, "Method not found", $"Unknown method: {method}")
            };
        }
        catch (JsonException ex)
        {
            return CreateErrorResponse(null, -32700, "Parse error", ex.Message);
        }
    }

    private Task<string> HandleInitialize(JsonNode? id)
    {
        var result = new
        {
            protocolVersion = "2024-11-05",
            capabilities = new
            {
                tools = new { }
            },
            serverInfo = new
            {
                name = "AutoTestID Workflow MCP Server",
                version = "1.0.0"
            }
        };

        return Task.FromResult(CreateSuccessResponse(id, result));
    }

    private Task<string> HandleToolsList(JsonNode? id)
    {
        var tools = new[]
        {
            new
            {
                name = "autotestid_workflow",
                description = "Comprehensive AutoTestID workflow for Angular/AngularJS applications. Implements two-phase processing: 1) Strategy selection (aria-first or test-attribute-first), 2) Enterprise-grade locator implementation with ARIA attributes OR data-testid (never both). Follows accessibility-first best practices.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        html_content = new
                        {
                            type = "string",
                            description = "The HTML content to analyze and apply AutoTestID workflow to"
                        },
                        user_request = new
                        {
                            type = "string",
                            description = "Optional user request or context for the AutoTestID workflow"
                        }
                    },
                    required = new[] { "html_content" }
                }
            }
        };

        return Task.FromResult(CreateSuccessResponse(id, new { tools }));
    }

    private Task<string> HandleToolsCall(JsonNode jsonMessage, JsonNode? id)
    {
        var arguments = jsonMessage["params"]?["arguments"];
        var toolName = jsonMessage["params"]?["name"]?.GetValue<string>();

        if (toolName != "autotestid_workflow")
        {
            return Task.FromResult(CreateErrorResponse(id, -32602, "Invalid params", $"Unknown tool: {toolName}"));
        }

        var htmlContent = arguments?["html_content"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            return Task.FromResult(CreateErrorResponse(id, -32602, "Invalid params", "html_content is required"));
        }

        var userRequest = arguments?["user_request"]?.GetValue<string>() ?? "Apply AutoTestID workflow";

        try
        {
            var logger = (ILogger?)_logger ?? NullLogger.Instance;
            var result = TestIdTools.AddTestIDFromPrompt(logger, htmlContent);
            
            var toolResult = new
            {
                content = new[]
                {
                    new
                    {
                        type = "text",
                        text = result
                    }
                }
            };

            return Task.FromResult(CreateSuccessResponse(id, toolResult));
        }
        catch (Exception ex)
        {
            return Task.FromResult(CreateErrorResponse(id, -32603, "Internal error", $"Error processing AutoTestID workflow: {ex.Message}"));
        }
    }

    private string CreateSuccessResponse(JsonNode? id, object result)
    {
        var response = new
        {
            jsonrpc = "2.0",
            id = id?.GetValue<object>(),
            result
        };

        return JsonSerializer.Serialize(response, _jsonOptions);
    }

    private string CreateErrorResponse(JsonNode? id, int code, string message, string? data = null)
    {
        var error = new
        {
            code,
            message,
            data
        };

        var response = new
        {
            jsonrpc = "2.0",
            id = id?.GetValue<object>(),
            error
        };

        return JsonSerializer.Serialize(response, _jsonOptions);
    }
}
