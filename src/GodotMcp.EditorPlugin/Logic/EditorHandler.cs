using Godot;

namespace GodotMcp.EditorPlugin.Logic;

public class EditorHandler
{
    public static void OpenScene(string path)
    {
        EditorInterface.Singleton.OpenSceneFromPath(path);
    }

    public static void SaveScene()
    {
        var result = EditorInterface.Singleton.SaveScene();
        if (result != Error.Ok)
        {
            throw new System.Exception($"Failed to save scene: {result}");
        }
    }

    public static void TakeScreenshot(string path, string target)
    {
        // target: "game" or "editor"
        // Capturing the editor interface
        var viewport = EditorInterface.Singleton.GetBaseControl().GetViewport();
        var texture = viewport.GetTexture();
        var image = texture.GetImage();

        image.SavePng(path);
    }
}
