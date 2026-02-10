using Godot;
using System.Collections.Generic;
using System.Text.Json;

namespace GodotMcp.EditorPlugin.Logic;

public class FileHandler
{
    public static string ListFiles()
    {
        var files = new List<string>();
        // Godot 4 DirAccess
        // Access static method if available or use Open
        using var dir = DirAccess.Open("res://");
        if (dir != null)
        {
             ListFilesRecursive("res://", files);
        }
        return JsonSerializer.Serialize(files);
    }

    private static void ListFilesRecursive(string path, List<string> files)
    {
        using var dir = DirAccess.Open(path);
        if (dir == null) return;

        dir.ListDirBegin(); // Defaults to include hidden?
        string fileName = dir.GetNext();
        while (fileName != "")
        {
            if (dir.CurrentIsDir())
            {
                if (fileName != "." && fileName != "..")
                {
                    string subPath = path.EndsWith("/") ? path + fileName : path + "/" + fileName;
                    ListFilesRecursive(subPath, files);
                }
            }
            else
            {
                string filePath = path.EndsWith("/") ? path + fileName : path + "/" + fileName;
                files.Add(filePath);
            }
            fileName = dir.GetNext();
        }
    }
}
