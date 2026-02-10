using Godot;
using GodotMcp.EditorPlugin.Logic;
using GodotMcp.EditorPlugin.Utils;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GodotMcp.EditorPlugin.Server;

public static class JsonRpcHandler
{
    private class JsonRpcRequest
    {
        [JsonPropertyName("jsonrpc")]
        public string JsonRpc { get; set; }

        [JsonPropertyName("method")]
        public string Method { get; set; }

        [JsonPropertyName("params")]
        public JsonElement Params { get; set; }

        [JsonPropertyName("id")]
        public object Id { get; set; }
    }

    private class JsonRpcResponse
    {
        [JsonPropertyName("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";

        [JsonPropertyName("result")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object Result { get; set; }

        [JsonPropertyName("error")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object Error { get; set; }

        [JsonPropertyName("id")]
        public object Id { get; set; }
    }

    public static async Task<string> Handle(string json)
    {
        JsonRpcRequest request = null;
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            request = JsonSerializer.Deserialize<JsonRpcRequest>(json, options);
        }
        catch (Exception)
        {
            return CreateErrorResponse(null, -32700, "Parse error");
        }

        if (request == null) return CreateErrorResponse(null, -32600, "Invalid Request");

        try
        {
            object result = null;

            switch (request.Method)
            {
                case "ping":
                    result = "pong";
                    break;

                case "godot_get_scene_tree":
                    string scope = "editor";
                    if (request.Params.ValueKind == JsonValueKind.Object && request.Params.TryGetProperty("scope", out var scopeProp))
                    {
                        scope = scopeProp.GetString() ?? "editor";
                    }

                    string jsonTree = await MainThreadDispatcher.ExecuteOnMainThread(() => SceneTraverser.GetSceneTree(scope));
                    using (var doc = JsonDocument.Parse(jsonTree))
                    {
                        result = doc.RootElement.Clone();
                    }
                    break;

                case "godot_list_files":
                    string filesJson = await MainThreadDispatcher.ExecuteOnMainThread(() => FileHandler.ListFiles());
                    using (var doc = JsonDocument.Parse(filesJson))
                    {
                        result = doc.RootElement.Clone();
                    }
                    break;

                case "godot_get_status":
                    string statusJson = await MainThreadDispatcher.ExecuteOnMainThread(() => StatusHandler.GetStatus());
                    using (var doc = JsonDocument.Parse(statusJson))
                    {
                        result = doc.RootElement.Clone();
                    }
                    break;

                case "godot_build":
                    bool built = await MainThreadDispatcher.ExecuteOnMainThread(() =>
                    {
                        Godot.Collections.Array output = new Godot.Collections.Array();
                        int exitCode = OS.Execute("dotnet", new string[] { "build" }, output, true);
                        if (exitCode != 0)
                        {
                            GD.PrintErr("Build failed");
                            foreach(var line in output) GD.PrintErr(line.ToString());
                        }
                        return exitCode == 0;
                    });
                    result = built ? "Build Success" : "Build Failed";
                    break;

                case "godot_play":
                    string scenePath = null;
                    bool debug = true;
                    if (request.Params.ValueKind == JsonValueKind.Object)
                    {
                        if (request.Params.TryGetProperty("scene_path", out var sceneProp) && sceneProp.ValueKind == JsonValueKind.String)
                             scenePath = sceneProp.GetString();
                        if (request.Params.TryGetProperty("debug", out var debugProp) && (debugProp.ValueKind == JsonValueKind.True || debugProp.ValueKind == JsonValueKind.False))
                             debug = debugProp.GetBoolean();
                    }

                    await MainThreadDispatcher.ExecuteOnMainThread(() => PlayHandler.Play(scenePath, debug));
                    result = "OK";
                    break;

                case "godot_stop":
                    await MainThreadDispatcher.ExecuteOnMainThread(() => PlayHandler.Stop());
                    result = "OK";
                    break;

                case "godot_add_node":
                    if (request.Params.ValueKind == JsonValueKind.Object)
                    {
                         string p = request.Params.GetProperty("parent_path").GetString();
                         string t = request.Params.GetProperty("type").GetString();
                         string n = request.Params.GetProperty("name").GetString();
                         await MainThreadDispatcher.ExecuteOnMainThread(() => NodeManipulator.AddNode(p, t, n));
                         result = "OK";
                    }
                    else throw new Exception("Invalid params");
                    break;

                case "godot_instantiate_scene":
                    if (request.Params.ValueKind == JsonValueKind.Object)
                    {
                         string p = request.Params.GetProperty("parent_path").GetString();
                         string s = request.Params.GetProperty("scene_path").GetString();
                         string n = null;
                         if (request.Params.TryGetProperty("name", out var nProp)) n = nProp.GetString();
                         await MainThreadDispatcher.ExecuteOnMainThread(() => NodeManipulator.InstantiateScene(p, s, n));
                         result = "OK";
                    }
                    else throw new Exception("Invalid params");
                    break;

                case "godot_set_property":
                    if (request.Params.ValueKind == JsonValueKind.Object)
                    {
                         string p = request.Params.GetProperty("path").GetString();
                         string prop = request.Params.GetProperty("property").GetString();
                         JsonElement val = request.Params.GetProperty("value");
                         await MainThreadDispatcher.ExecuteOnMainThread(() => NodeManipulator.SetProperty(p, prop, val));
                         result = "OK";
                    }
                    else throw new Exception("Invalid params");
                    break;

                case "godot_call_node_method":
                    if (request.Params.ValueKind == JsonValueKind.Object)
                    {
                         string p = request.Params.GetProperty("path").GetString();
                         string m = request.Params.GetProperty("method").GetString();
                         JsonElement args = request.Params.GetProperty("args");
                         string res = await MainThreadDispatcher.ExecuteOnMainThread(() => NodeManipulator.CallMethod(p, m, args));
                         result = res;
                    }
                    else throw new Exception("Invalid params");
                    break;

                case "godot_connect_signal":
                    if (request.Params.ValueKind == JsonValueKind.Object)
                    {
                         string src = request.Params.GetProperty("source_path").GetString();
                         string sig = request.Params.GetProperty("signal").GetString();
                         string trg = request.Params.GetProperty("target_path").GetString();
                         string met = request.Params.GetProperty("method").GetString();
                         await MainThreadDispatcher.ExecuteOnMainThread(() => NodeManipulator.ConnectSignal(src, sig, trg, met));
                         result = "OK";
                    }
                    else throw new Exception("Invalid params");
                    break;

                case "godot_open_scene":
                    if (request.Params.ValueKind == JsonValueKind.Object)
                    {
                        string p = request.Params.GetProperty("path").GetString();
                        await MainThreadDispatcher.ExecuteOnMainThread(() => EditorHandler.OpenScene(p));
                        result = "OK";
                    }
                    else throw new Exception("Invalid params");
                    break;

                case "godot_save_scene":
                    await MainThreadDispatcher.ExecuteOnMainThread(() => EditorHandler.SaveScene());
                    result = "OK";
                    break;

                case "godot_screenshot":
                    if (request.Params.ValueKind == JsonValueKind.Object)
                    {
                        string p = request.Params.GetProperty("path").GetString();
                        string t = request.Params.GetProperty("target").GetString();
                        await MainThreadDispatcher.ExecuteOnMainThread(() => EditorHandler.TakeScreenshot(p, t));
                        result = "OK";
                    }
                    else throw new Exception("Invalid params");
                    break;

                case "godot_create_resource":
                    if (request.Params.ValueKind == JsonValueKind.Object)
                    {
                        string t = request.Params.GetProperty("type").GetString();
                        string p = request.Params.GetProperty("path").GetString();
                        await MainThreadDispatcher.ExecuteOnMainThread(() => AssetManager.CreateResource(t, p));
                        result = "OK";
                    }
                    else throw new Exception("Invalid params");
                    break;

                case "godot_set_project_setting":
                    if (request.Params.ValueKind == JsonValueKind.Object)
                    {
                        string n = request.Params.GetProperty("name").GetString();
                        JsonElement v = request.Params.GetProperty("value");
                        await MainThreadDispatcher.ExecuteOnMainThread(() => AssetManager.SetProjectSetting(n, v));
                        result = "OK";
                    }
                    else throw new Exception("Invalid params");
                    break;

                case "godot_input_map_add":
                    if (request.Params.ValueKind == JsonValueKind.Object)
                    {
                        string a = request.Params.GetProperty("action").GetString();
                        string k = request.Params.GetProperty("key").GetString();
                        await MainThreadDispatcher.ExecuteOnMainThread(() => AssetManager.AddInputAction(a, k));
                        result = "OK";
                    }
                    else throw new Exception("Invalid params");
                    break;

                case "godot_filesystem_move":
                    if (request.Params.ValueKind == JsonValueKind.Object)
                    {
                        string s = request.Params.GetProperty("source").GetString();
                        string d = request.Params.GetProperty("destination").GetString();
                        await MainThreadDispatcher.ExecuteOnMainThread(() => AssetManager.MoveFile(s, d));
                        result = "OK";
                    }
                    else throw new Exception("Invalid params");
                    break;

                case "godot_filesystem_remove":
                    if (request.Params.ValueKind == JsonValueKind.Object)
                    {
                        string p = request.Params.GetProperty("path").GetString();
                        await MainThreadDispatcher.ExecuteOnMainThread(() => AssetManager.RemoveFile(p));
                        result = "OK";
                    }
                    else throw new Exception("Invalid params");
                    break;

                case "godot_reimport":
                    if (request.Params.ValueKind == JsonValueKind.Object)
                    {
                        string p = request.Params.GetProperty("path").GetString();
                        JsonElement opts = default;
                        if (request.Params.TryGetProperty("options", out var oProp)) opts = oProp;
                        await MainThreadDispatcher.ExecuteOnMainThread(() => AssetManager.Reimport(p, opts));
                        result = "OK";
                    }
                    else throw new Exception("Invalid params");
                    break;

                default:
                    return CreateErrorResponse(request.Id, -32601, "Method not found");
            }

            return JsonSerializer.Serialize(new JsonRpcResponse { Result = result, Id = request.Id });
        }
        catch (Exception ex)
        {
            return CreateErrorResponse(request.Id, -32603, $"Internal error: {ex.Message}");
        }
    }

    private static string CreateErrorResponse(object id, int code, string message)
    {
        var response = new
        {
            jsonrpc = "2.0",
            error = new { code, message },
            id
        };
        return JsonSerializer.Serialize(response);
    }
}
