using Godot;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GodotMcp.EditorPlugin.Logic;

public class SceneTraverser
{
    private class NodeDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("class")]
        public string Class { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("children")]
        public List<NodeDto> Children { get; set; } = new();
    }

    public static string GetSceneTree(string scope)
    {
        Node root = null;
        if (scope == "editor")
        {
            root = EditorInterface.Singleton.GetEditedSceneRoot();
        }
        else
        {
            // Default to editor for now
            root = EditorInterface.Singleton.GetEditedSceneRoot();
        }

        if (root == null)
        {
            return JsonSerializer.Serialize(new { error = "No edited scene found" });
        }

        var dto = BuildDto(root);
        return JsonSerializer.Serialize(dto);
    }

    private static NodeDto BuildDto(Node node)
    {
        var dto = new NodeDto
        {
            Name = node.Name,
            Class = node.GetClass(),
            Path = node.GetPath().ToString()
        };

        foreach (Node child in node.GetChildren())
        {
            dto.Children.Add(BuildDto(child));
        }

        return dto;
    }
}
