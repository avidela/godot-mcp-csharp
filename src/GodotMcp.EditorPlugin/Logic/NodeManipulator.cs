using Godot;
using GodotMcp.EditorPlugin.Utils;
using System;
using System.Text.Json;

namespace GodotMcp.EditorPlugin.Logic;

public class NodeManipulator
{
    public static void AddNode(string parentPath, string type, string name)
    {
        var root = EditorInterface.Singleton.GetEditedSceneRoot();
        if (root == null) throw new Exception("No edited scene");

        var parent = root.GetNodeOrNull(parentPath);
        if (parent == null && !parentPath.StartsWith("/root"))
        {
             parent = root.GetNodeOrNull(parentPath);
        }

        if (parent == null) throw new Exception($"Parent not found: {parentPath}");

        var instanceVar = ClassDB.Instantiate(type);
        if (instanceVar.Obj == null) throw new Exception($"Failed to instantiate type {type}");

        var node = instanceVar.As<Node>();
        if (node == null) throw new Exception($"Instantiated object is not a Node: {type}");

        node.Name = name;
        parent.AddChild(node);
        node.Owner = root;
    }

    public static void InstantiateScene(string parentPath, string scenePath, string name)
    {
        var root = EditorInterface.Singleton.GetEditedSceneRoot();
        if (root == null) throw new Exception("No edited scene");

        var parent = root.GetNodeOrNull(parentPath);
        if (parent == null) throw new Exception($"Parent not found: {parentPath}");

        var scene = GD.Load<PackedScene>(scenePath);
        if (scene == null) throw new Exception($"Failed to load scene {scenePath}");

        var node = scene.Instantiate();
        if (!string.IsNullOrEmpty(name)) node.Name = name;

        parent.AddChild(node);
        node.Owner = root;
    }

    public static void SetProperty(string path, string property, JsonElement value)
    {
        var root = EditorInterface.Singleton.GetEditedSceneRoot();
        if (root == null) throw new Exception("No edited scene");

        var node = root.GetNodeOrNull(path);
        if (node == null) throw new Exception($"Node not found: {path}");

        Variant variantValue = VariantUtils.ConvertToVariant(value);
        node.Set(property, variantValue);
    }

    public static string CallMethod(string path, string method, JsonElement args)
    {
         var root = EditorInterface.Singleton.GetEditedSceneRoot();
         if (root == null) throw new Exception("No edited scene");

         var node = root.GetNodeOrNull(path);
         if (node == null) throw new Exception($"Node not found: {path}");

         Variant[] vArgs = Array.Empty<Variant>();
         if (args.ValueKind == JsonValueKind.Array)
         {
             var list = new System.Collections.Generic.List<Variant>();
             foreach (var arg in args.EnumerateArray())
             {
                 list.Add(VariantUtils.ConvertToVariant(arg));
             }
             vArgs = list.ToArray();
         }

         var result = node.Call(method, vArgs);
         return result.ToString();
    }

    public static void ConnectSignal(string sourcePath, string signal, string targetPath, string method)
    {
         var root = EditorInterface.Singleton.GetEditedSceneRoot();
         if (root == null) throw new Exception("No edited scene");

         var source = root.GetNodeOrNull(sourcePath);
         var target = root.GetNodeOrNull(targetPath);

         if (source == null) throw new Exception($"Source not found: {sourcePath}");
         if (target == null) throw new Exception($"Target not found: {targetPath}");

         var callable = new Callable(target, method);
         if (!source.IsConnected(signal, callable))
         {
             source.Connect(signal, callable);
         }
    }
}
