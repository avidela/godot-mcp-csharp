using GodotMcp.Server.Bridge;
using GodotMcp.Server.Debugger;
using GodotMcp.Server.Mcp;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GodotMcp.Server;

public class Program
{
    public static async Task Main(string[] args)
    {
        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        using var client = new WebSocketClient();
        using var dap = new DapClient();

        // Background connection loop for WebSocket
        _ = Task.Run(async () =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                if (!client.IsConnected)
                {
                    try
                    {
                        await client.ConnectAsync(cts.Token);
                    }
                    catch
                    {
                        try { await Task.Delay(5000, cts.Token); } catch { }
                    }
                }
                else
                {
                    try { await Task.Delay(1000, cts.Token); } catch { }
                }
            }
        });

        // Background connection loop for DAP
        _ = Task.Run(async () =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                if (!dap.IsConnected)
                {
                    try
                    {
                        // DAP only works when game is running.
                        // We should try to connect periodically.
                        await dap.ConnectAsync(cts.Token);
                    }
                    catch
                    {
                        try { await Task.Delay(5000, cts.Token); } catch { }
                    }
                }
                else
                {
                    try { await Task.Delay(1000, cts.Token); } catch { }
                }
            }
        });

        // Start MCP Server handling Stdio
        var server = new McpServer(client, dap);
        try
        {
            await server.RunAsync(cts.Token);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Server Error: {ex.Message}");
        }
    }
}
