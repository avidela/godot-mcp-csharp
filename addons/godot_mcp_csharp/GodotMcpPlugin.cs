using Godot;
using GodotMcp.EditorPlugin.Server;
using GodotMcp.EditorPlugin.Utils;

#if TOOLS
[Tool]
#endif
public partial class GodotMcpPlugin : EditorPlugin
{
    private WebSocketListener _listener;

    public override void _EnterTree()
    {
        MainThreadDispatcher.Initialize(this);
        _listener = new WebSocketListener();
        _listener.Start();
        GD.Print("GodotMcpPlugin: Server Started");
    }

    public override void _ExitTree()
    {
        _listener?.Stop();
        GD.Print("GodotMcpPlugin: Server Stopped");
    }
}
