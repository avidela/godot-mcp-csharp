using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GodotMcp.Server.Bridge;

public class WebSocketClient : IDisposable
{
    private ClientWebSocket _client;
    private const string ServerUrl = "ws://127.0.0.1:6009/";
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private readonly ConcurrentDictionary<string, TaskCompletionSource<JsonElement>> _pendingRequests = new();
    private int _idCounter = 0;

    public bool IsConnected => _client?.State == WebSocketState.Open;

    public async Task ConnectAsync(CancellationToken token)
    {
        _client = new ClientWebSocket();
        try
        {
            await _client.ConnectAsync(new Uri(ServerUrl), token);
            Console.Error.WriteLine("Connected to GodotMcp Plugin.");

            _ = Task.Run(() => ReceiveLoop(_cts.Token), _cts.Token);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Connection failed: {ex.Message}");
            throw;
        }
    }

    private async Task ReceiveLoop(CancellationToken token)
    {
        var buffer = new byte[1024 * 1024]; // 1MB buffer
        while (_client.State == WebSocketState.Open && !token.IsCancellationRequested)
        {
            try
            {
                var result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), token);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token);
                    Console.Error.WriteLine("Connection closed by server.");
                }
                else
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    HandleMessage(message);
                }
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                    Console.Error.WriteLine($"Receive error: {ex.Message}");
            }
        }
    }

    private void HandleMessage(string message)
    {
        try
        {
            using var doc = JsonDocument.Parse(message);
            var root = doc.RootElement;

            if (root.TryGetProperty("id", out var idProp))
            {
                string id = idProp.ToString();
                if (_pendingRequests.TryRemove(id, out var tcs))
                {
                    if (root.TryGetProperty("error", out var error))
                    {
                        tcs.SetException(new Exception($"RPC Error: {error}"));
                    }
                    else if (root.TryGetProperty("result", out var result))
                    {
                        tcs.SetResult(result.Clone());
                    }
                    else
                    {
                        tcs.SetResult(default);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error parsing response: {ex.Message}");
        }
    }

    public async Task<JsonElement> SendRequestAsync(string method, object @params = null)
    {
        if (_client == null || _client.State != WebSocketState.Open)
        {
            throw new InvalidOperationException("WebSocket is not connected.");
        }

        string id = Interlocked.Increment(ref _idCounter).ToString();
        var tcs = new TaskCompletionSource<JsonElement>();
        _pendingRequests.TryAdd(id, tcs);

        var request = new
        {
            jsonrpc = "2.0",
            method = method,
            @params = @params,
            id = id
        };

        string json = JsonSerializer.Serialize(request);
        var buffer = Encoding.UTF8.GetBytes(json);

        try
        {
            await _client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch
        {
            _pendingRequests.TryRemove(id, out _);
            throw;
        }

        return await tcs.Task;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _client?.Dispose();
    }
}
