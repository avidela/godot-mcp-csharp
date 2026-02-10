using Godot;
using System.Text.Json;

namespace GodotMcp.EditorPlugin.Logic;

public class StatusHandler
{
    public static string GetStatus()
    {
        bool isPlaying = EditorInterface.Singleton.IsPlayingScene();
        return JsonSerializer.Serialize(new {
            editor_open = true,
            game_running = isPlaying
        });
    }
}
