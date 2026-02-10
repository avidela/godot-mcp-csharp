using Godot;

namespace GodotMcp.EditorPlugin.Logic;

public class PlayHandler
{
    public static void Play(string scenePath = null, bool debug = true)
    {
        // Note: EditorInterface.PlayMainScene() and PlayCustomScene() launch with debugger attached by default in Editor.

        if (string.IsNullOrEmpty(scenePath))
        {
            EditorInterface.Singleton.PlayMainScene();
        }
        else
        {
            EditorInterface.Singleton.PlayCustomScene(scenePath);
        }
    }

    public static void Stop()
    {
        EditorInterface.Singleton.StopPlayingScene();
    }
}
