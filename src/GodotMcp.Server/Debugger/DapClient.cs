using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GodotMcp.Server.Debugger;

public class DapClient : IDisposable
{
    private TcpClient _client;
    private NetworkStream _stream;
    private const int DebugPort = 6006;
    private const string DebugHost = "127.0.0.1";
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private int _seq = 0;
    private readonly ConcurrentDictionary<int, TaskCompletionSource<JsonElement>> _pendingRequests = new();

    public bool IsConnected => _client?.Connected ?? false;

    public async Task ConnectAsync(CancellationToken token)
    {
        _client = new TcpClient();
        try
        {
            await _client.ConnectAsync(DebugHost, DebugPort, token);
            _stream = _client.GetStream();
            Console.Error.WriteLine("Connected to Godot DAP.");

            _ = Task.Run(() => ReadLoop(_cts.Token), _cts.Token);

            await SendRequestAsync("initialize", new
            {
                 adapterID = "godot-mcp",
                 linesStartAt1 = true,
                 columnsStartAt1 = true,
                 pathFormat = "path"
            });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"DAP Connection failed: {ex.Message}");
            throw;
        }
    }

    private async Task ReadLoop(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested && _client.Connected)
            {
                string header = await ReadHeaderAsync(token);
                if (header == null) break;

                int contentLength = ParseContentLength(header);
                if (contentLength > 0)
                {
                    string json = await ReadContentAsync(contentLength, token);
                    HandleMessage(json);
                }
            }
        }
        catch (Exception ex)
        {
            if (!token.IsCancellationRequested)
                Console.Error.WriteLine($"DAP Read Error: {ex.Message}");
        }
    }

    private async Task<string> ReadHeaderAsync(CancellationToken token)
    {
        StringBuilder header = new StringBuilder();
        byte[] buffer = new byte[1];
        while (!token.IsCancellationRequested)
        {
            int read = await _stream.ReadAsync(buffer, 0, 1, token);
            if (read == 0) return null;

            char c = (char)buffer[0];
            header.Append(c);

            if (header.ToString().EndsWith("\r\n\r\n"))
            {
                return header.ToString();
            }
        }
        return null;
    }

    private int ParseContentLength(string header)
    {
        string[] lines = header.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
        {
            if (line.StartsWith("Content-Length: "))
            {
                if (int.TryParse(line.Substring("Content-Length: ".Length), out int length))
                {
                    return length;
                }
            }
        }
        return 0;
    }

    private async Task<string> ReadContentAsync(int length, CancellationToken token)
    {
        byte[] buffer = new byte[length];
        int offset = 0;
        while (offset < length && !token.IsCancellationRequested)
        {
            int read = await _stream.ReadAsync(buffer, offset, length - offset, token);
            if (read == 0) throw new EndOfStreamException();
            offset += read;
        }
        return Encoding.UTF8.GetString(buffer);
    }

    private void HandleMessage(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Check if response
            if (root.TryGetProperty("type", out var typeProp))
            {
                string type = typeProp.GetString();
                if (type == "response")
                {
                    if (root.TryGetProperty("request_seq", out var reqSeqProp))
                    {
                        int reqSeq = reqSeqProp.GetInt32();
                        if (_pendingRequests.TryRemove(reqSeq, out var tcs))
                        {
                             if (root.TryGetProperty("success", out var successProp) && !successProp.GetBoolean())
                             {
                                 string msg = "Unknown Error";
                                 if (root.TryGetProperty("message", out var msgProp)) msg = msgProp.GetString();
                                 tcs.SetException(new Exception(msg));
                             }
                             else
                             {
                                 if (root.TryGetProperty("body", out var bodyProp))
                                     tcs.SetResult(bodyProp.Clone());
                                 else
                                     tcs.SetResult(default);
                             }
                        }
                    }
                }
                else if (type == "event")
                {
                    if (root.TryGetProperty("event", out var eventProp) && eventProp.GetString() == "output")
                    {
                        if (root.TryGetProperty("body", out var bodyProp) && bodyProp.TryGetProperty("output", out var outputProp))
                        {
                            string output = outputProp.GetString();
                            Console.Error.Write($"[Godot Output] {output}");
                        }
                    }

                    if (root.TryGetProperty("event", out var evt) && evt.GetString() == "initialized")
                    {
                         // We don't await these as they are fired by event
                         _ = SendRequestAsync("attach", new { });
                         _ = SendRequestAsync("configurationDone", new { });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error handling DAP message: {ex.Message}");
        }
    }

    public async Task<JsonElement> SendRequestAsync(string command, object args)
    {
        if (_client == null || !_client.Connected) throw new InvalidOperationException("Not connected");

        int seq = Interlocked.Increment(ref _seq);
        var tcs = new TaskCompletionSource<JsonElement>();
        _pendingRequests.TryAdd(seq, tcs);

        var request = new
        {
            seq = seq,
            type = "request",
            command = command,
            arguments = args
        };

        string json = JsonSerializer.Serialize(request);
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
        string header = $"Content-Length: {jsonBytes.Length}\r\n\r\n";
        byte[] headerBytes = Encoding.ASCII.GetBytes(header);

        try
        {
            await _stream.WriteAsync(headerBytes, 0, headerBytes.Length);
            await _stream.WriteAsync(jsonBytes, 0, jsonBytes.Length);
            await _stream.FlushAsync();
        }
        catch
        {
            _pendingRequests.TryRemove(seq, out _);
            throw;
        }

        return await tcs.Task;
    }

    public async Task SendInputAsync(string expression)
    {
        await SendRequestAsync("evaluate", new
        {
            expression = expression,
            context = "repl"
        });
    }

    public void Dispose()
    {
        _cts.Cancel();
        _client?.Dispose();
    }
}
