using Godot;
using GodotMcp.EditorPlugin.Utils;
using System;
using System.Text.Json;

namespace GodotMcp.EditorPlugin.Logic;

public class AssetManager
{
    public static void CreateResource(string type, string path)
    {
        var instanceVar = ClassDB.Instantiate(type);
        if (instanceVar.Obj == null) throw new Exception($"Failed to instantiate type {type}");

        var res = instanceVar.As<Resource>();
        if (res == null) throw new Exception($"Instantiated object is not a Resource: {type}");

        var error = ResourceSaver.Save(res, path);
        if (error != Error.Ok) throw new Exception($"Failed to save resource: {error}");
    }

    public static void SetProjectSetting(string name, JsonElement value)
    {
        Variant variantValue = VariantUtils.ConvertToVariant(value);
        ProjectSettings.SetSetting(name, variantValue);
        ProjectSettings.Save();
    }

    public static void AddInputAction(string action, string key)
    {
        if (!InputMap.HasAction(action))
        {
            InputMap.AddAction(action);
        }

        var evt = new InputEventKey();
        evt.Keycode = OS.FindKeycodeFromString(key);
        InputMap.ActionAddEvent(action, evt);

        // Note: This only affects runtime/editor session InputMap.
        // Persisting to project.godot requires manual ProjectSettings manipulation which is complex.
    }

    public static void MoveFile(string src, string dst)
    {
        using var dir = DirAccess.Open("res://");
        if (dir.Rename(src, dst) != Error.Ok) throw new Exception("Failed to rename/move file");

        EditorInterface.Singleton.GetResourceFilesystem().Scan();
    }

    public static void RemoveFile(string path)
    {
        using var dir = DirAccess.Open("res://");
        if (dir.Remove(path) != Error.Ok) throw new Exception("Failed to remove file");

        EditorInterface.Singleton.GetResourceFilesystem().Scan();
    }

    public static void Reimport(string path, JsonElement options)
    {
        // Note: options handling skipped for simplicity as API requires complex config manipulation
        EditorInterface.Singleton.GetResourceFilesystem().ReimportFiles(new string[] { path });
    }
}
