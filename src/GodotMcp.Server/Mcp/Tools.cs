using GodotMcp.Server.Bridge;
using GodotMcp.Server.Debugger;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace GodotMcp.Server.Mcp;

public static class Tools
{
    public static object GetTools()
    {
        return ToolDefinitions.GetTools();
    }

    public static async Task<object> HandleToolCallAsync(WebSocketClient bridge, DapClient dap, JsonElement paramsElement)
    {
        if (!paramsElement.TryGetProperty("name", out var nameProp))
             throw new Exception("Missing tool name");

        string name = nameProp.GetString();
        JsonElement arguments = default;
        if (paramsElement.TryGetProperty("arguments", out var argsProp))
            arguments = argsProp;

        switch (name)
        {
            case "godot_get_scene_tree":
            {
                string scope = "editor";
                if (arguments.ValueKind == JsonValueKind.Object && arguments.TryGetProperty("scope", out var scopeProp))
                {
                    scope = scopeProp.GetString() ?? "editor";
                }
                var result = await bridge.SendRequestAsync("godot_get_scene_tree", new { scope });
                return new { content = new[] { new { type = "text", text = result.ToString() } } };
            }
            case "godot_list_files":
            {
                var result = await bridge.SendRequestAsync("godot_list_files");
                return new { content = new[] { new { type = "text", text = result.ToString() } } };
            }
            case "godot_get_status":
            {
                var result = await bridge.SendRequestAsync("godot_get_status");
                return new { content = new[] { new { type = "text", text = result.ToString() } } };
            }
            case "godot_build":
            {
                var result = await bridge.SendRequestAsync("godot_build");
                return new { content = new[] { new { type = "text", text = result.ToString() } } };
            }
            case "godot_play":
            {
                string scenePath = null;
                bool debug = true;
                if (arguments.ValueKind == JsonValueKind.Object)
                {
                    if (arguments.TryGetProperty("scene_path", out var sceneProp) && sceneProp.ValueKind == JsonValueKind.String)
                         scenePath = sceneProp.GetString();
                    if (arguments.TryGetProperty("debug", out var debugProp) && (debugProp.ValueKind == JsonValueKind.True || debugProp.ValueKind == JsonValueKind.False))
                         debug = debugProp.GetBoolean();
                }
                var result = await bridge.SendRequestAsync("godot_play", new { scene_path = scenePath, debug });
                return new { content = new[] { new { type = "text", text = result.ToString() } } };
            }
            case "godot_stop":
            {
                var result = await bridge.SendRequestAsync("godot_stop");
                return new { content = new[] { new { type = "text", text = result.ToString() } } };
            }
            case "godot_send_input":
            {
                string expression = BuildInputExpression(arguments);
                await dap.SendInputAsync(expression);
                return new { content = new[] { new { type = "text", text = "Input sent" } } };
            }
            case "godot_add_node":
            {
                 var result = await bridge.SendRequestAsync("godot_add_node", arguments);
                 return new { content = new[] { new { type = "text", text = result.ToString() } } };
            }
            case "godot_instantiate_scene":
            {
                 var result = await bridge.SendRequestAsync("godot_instantiate_scene", arguments);
                 return new { content = new[] { new { type = "text", text = result.ToString() } } };
            }
            case "godot_set_property":
            {
                 var result = await bridge.SendRequestAsync("godot_set_property", arguments);
                 return new { content = new[] { new { type = "text", text = result.ToString() } } };
            }
            case "godot_call_node_method":
            {
                 var result = await bridge.SendRequestAsync("godot_call_node_method", arguments);
                 return new { content = new[] { new { type = "text", text = result.ToString() } } };
            }
            case "godot_connect_signal":
            {
                 var result = await bridge.SendRequestAsync("godot_connect_signal", arguments);
                 return new { content = new[] { new { type = "text", text = result.ToString() } } };
            }
            case "godot_open_scene":
            {
                 var result = await bridge.SendRequestAsync("godot_open_scene", arguments);
                 return new { content = new[] { new { type = "text", text = result.ToString() } } };
            }
            case "godot_save_scene":
            {
                 var result = await bridge.SendRequestAsync("godot_save_scene");
                 return new { content = new[] { new { type = "text", text = result.ToString() } } };
            }
            case "godot_screenshot":
            {
                 var result = await bridge.SendRequestAsync("godot_screenshot", arguments);
                 return new { content = new[] { new { type = "text", text = result.ToString() } } };
            }
            case "godot_create_resource":
            {
                 var result = await bridge.SendRequestAsync("godot_create_resource", arguments);
                 return new { content = new[] { new { type = "text", text = result.ToString() } } };
            }
            case "godot_set_project_setting":
            {
                 var result = await bridge.SendRequestAsync("godot_set_project_setting", arguments);
                 return new { content = new[] { new { type = "text", text = result.ToString() } } };
            }
            case "godot_input_map_add":
            {
                 var result = await bridge.SendRequestAsync("godot_input_map_add", arguments);
                 return new { content = new[] { new { type = "text", text = result.ToString() } } };
            }
            case "godot_filesystem_move":
            {
                 var result = await bridge.SendRequestAsync("godot_filesystem_move", arguments);
                 return new { content = new[] { new { type = "text", text = result.ToString() } } };
            }
            case "godot_filesystem_remove":
            {
                 var result = await bridge.SendRequestAsync("godot_filesystem_remove", arguments);
                 return new { content = new[] { new { type = "text", text = result.ToString() } } };
            }
            case "godot_reimport":
            {
                 var result = await bridge.SendRequestAsync("godot_reimport", arguments);
                 return new { content = new[] { new { type = "text", text = result.ToString() } } };
            }
            case "godot_debug_break":
            {
                await dap.SendRequestAsync("pause", new { threadId = 1 });
                return new { content = new[] { new { type = "text", text = "Paused" } } };
            }
            case "godot_debug_continue":
            {
                await dap.SendRequestAsync("continue", new { threadId = 1 });
                return new { content = new[] { new { type = "text", text = "Resumed" } } };
            }
            case "godot_debug_step":
            {
                await dap.SendRequestAsync("next", new { threadId = 1 });
                return new { content = new[] { new { type = "text", text = "Stepped" } } };
            }
            case "godot_get_stack_trace":
            {
                var trace = await dap.SendRequestAsync("stackTrace", new { threadId = 1 });
                return new { content = new[] { new { type = "text", text = trace.ToString() } } };
            }
            case "godot_set_breakpoint":
            {
                string path = arguments.GetProperty("path").GetString();
                int line = arguments.GetProperty("line").GetInt32();
                await dap.SendRequestAsync("setBreakpoints", new
                {
                    source = new { path = path },
                    breakpoints = new[] { new { line = line } }
                });
                return new { content = new[] { new { type = "text", text = "Breakpoint set" } } };
            }
            default:
                throw new Exception($"Unknown tool: {name}");
        }
    }

    private static string BuildInputExpression(JsonElement args)
    {
        if (args.TryGetProperty("event_type", out var typeProp) && typeProp.GetString() == "key")
        {
            if (args.TryGetProperty("data", out var dataProp))
            {
                string keycode = "Space";
                bool pressed = true;
                if (dataProp.TryGetProperty("keycode", out var keyProp)) keycode = keyProp.GetString();
                if (dataProp.TryGetProperty("pressed", out var pressedProp)) pressed = pressedProp.GetBoolean();

                // GDScript expression for InputEventKey
                return $"var ev = InputEventKey.new(); ev.keycode = KEY_{keycode.ToUpper()}; ev.pressed = {pressed.ToString().ToLower()}; Input.parse_input_event(ev)";
            }
        }

        return "print(\"Input not supported yet or invalid arguments\")";
    }
}
