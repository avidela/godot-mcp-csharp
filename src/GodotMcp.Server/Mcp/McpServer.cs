using GodotMcp.Server.Bridge;
using GodotMcp.Server.Debugger;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GodotMcp.Server.Mcp;

public class McpServer
{
    private readonly WebSocketClient _bridge;
    private readonly DapClient _dap;
    private readonly Stream _input;
    private readonly Stream _output;

    public McpServer(WebSocketClient bridge, DapClient dap)
    {
        _bridge = bridge;
        _dap = dap;
        _input = Console.OpenStandardInput();
        _output = Console.OpenStandardOutput();
    }

    public async Task RunAsync(CancellationToken token)
    {
        using var reader = new StreamReader(_input, Encoding.UTF8);

        while (!token.IsCancellationRequested)
        {
            string line = await reader.ReadLineAsync();
            if (line == null) break;

            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                await HandleMessageAsync(line);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error handling message: {ex.Message}");
            }
        }
    }

    private async Task HandleMessageAsync(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.TryGetProperty("method", out var methodProp))
        {
            string method = methodProp.GetString();
            object id = null;
            if (root.TryGetProperty("id", out var idProp))
            {
                if (idProp.ValueKind == JsonValueKind.Number) id = idProp.GetInt64();
                else if (idProp.ValueKind == JsonValueKind.String) id = idProp.GetString();
            }

            object result = null;

            try
            {
                switch (method)
                {
                    case "initialize":
                        result = new
                        {
                            protocolVersion = "2024-11-05",
                            capabilities = new { tools = new object() },
                            serverInfo = new { name = "godot-mcp-server", version = "1.0.0" }
                        };
                        break;
                    case "notifications/initialized":
                    case "initialized":
                        return;
                    case "tools/list":
                        result = new { tools = Tools.GetTools() };
                        break;
                    case "tools/call":
                        if (root.TryGetProperty("params", out var paramsProp))
                        {
                            result = await Tools.HandleToolCallAsync(_bridge, _dap, paramsProp);
                        }
                        else
                        {
                            throw new Exception("Missing params");
                        }
                        break;
                    case "ping":
                        result = new { };
                        break;
                    default:
                         if (id != null)
                         {
                             throw new Exception($"Method not found: {method}");
                         }
                         return;
                }

                if (id != null)
                {
                    var response = new { jsonrpc = "2.0", result = result, id = id };
                    string responseJson = JsonSerializer.Serialize(response);
                    Console.WriteLine(responseJson);
                    Console.Out.Flush();
                }
            }
            catch (Exception ex)
            {
                if (id != null)
                {
                    var response = new { jsonrpc = "2.0", error = new { code = -32603, message = ex.Message }, id = id };
                    Console.WriteLine(JsonSerializer.Serialize(response));
                    Console.Out.Flush();
                }
            }
        }
    }
}
