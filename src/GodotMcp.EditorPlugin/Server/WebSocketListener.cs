using Godot;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GodotMcp.EditorPlugin.Server;

public class WebSocketListener
{
    private HttpListener _listener;
    private CancellationTokenSource _cts;

    public void Start()
    {
        try
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://127.0.0.1:6009/");
            _listener.Start();
            _cts = new CancellationTokenSource();

            GD.Print("GodotMcp: WebSocket Server started on port 6009");
            Task.Run(() => AcceptConnections(_cts.Token));
        }
        catch (Exception ex)
        {
            GD.PrintErr($"GodotMcp: Failed to start WebSocket Server: {ex.Message}");
        }
    }

    public void Stop()
    {
        _cts?.Cancel();
        _listener?.Stop();
        _listener?.Close();
        GD.Print("GodotMcp: WebSocket Server stopped");
    }

    private async Task AcceptConnections(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    _ = Task.Run(() => ProcessWebSocketRequest(context, token));
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
            catch (HttpListenerException)
            {
                // Listener stopped
                break;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"GodotMcp: Accept Error: {ex.Message}");
            }
        }
    }

    private async Task ProcessWebSocketRequest(HttpListenerContext context, CancellationToken token)
    {
        WebSocketContext wsContext = null;
        try
        {
            wsContext = await context.AcceptWebSocketAsync(subProtocol: null);
            WebSocket webSocket = wsContext.WebSocket;
            GD.Print("GodotMcp: Client connected");

            byte[] buffer = new byte[1024 * 64]; // 64KB buffer
            while (webSocket.State == WebSocketState.Open && !token.IsCancellationRequested)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token);
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    // Handle via JsonRpcHandler
                    string response = await JsonRpcHandler.Handle(message);

                    if (!string.IsNullOrEmpty(response))
                    {
                        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                        await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, token);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if (!token.IsCancellationRequested)
                GD.PrintErr($"GodotMcp: Connection Error: {ex.Message}");
        }
        finally
        {
            wsContext?.WebSocket?.Dispose();
            GD.Print("GodotMcp: Client disconnected");
        }
    }
}
