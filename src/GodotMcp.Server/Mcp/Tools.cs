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
        return new List<object>
        {
            new
            {
                name = "godot_get_scene_tree",
                description = "Returns a hierarchical JSON representation of the currently active tree.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        scope = new { type = "string", description = "\"editor\" (edited scene) or \"runtime\" (playing game)." }
                    }
                }
            },
            new
            {
                name = "godot_list_files",
                description = "Lists files in the res:// directory.",
                inputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            },
            new
            {
                name = "godot_get_status",
                description = "Returns Editor status (Open/Closed), Game status (Running/Stopped), and Plugin connection status.",
                inputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            },
            new
            {
                name = "godot_build",
                description = "Triggers a build of the project.",
                inputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            },
            new
            {
                name = "godot_play",
                description = "Launches the game.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        scene_path = new { type = "string", description = "Specific scene to play. Defaults to main scene." },
                        debug = new { type = "boolean", description = "If true, ensures debugger (DAP) is attached." }
                    }
                }
            },
            new
            {
                name = "godot_stop",
                description = "Stops the running game.",
                inputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            },
            new
            {
                name = "godot_send_input",
                description = "Simulates input in the running game.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        action = new { type = "string" },
                        event_type = new { type = "string", description = "key, mouse_button, joy_button, joy_axis" },
                        data = new { type = "object" }
                    }
                }
            },
            new
            {
                name = "godot_add_node",
                description = "Adds a node to the current scene.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        parent_path = new { type = "string" },
                        type = new { type = "string", description = "Class name (e.g. Sprite2D)" },
                        name = new { type = "string" }
                    }
                }
            },
            new
            {
                name = "godot_instantiate_scene",
                description = "Instantiates a .tscn file as a child of a node.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        parent_path = new { type = "string" },
                        scene_path = new { type = "string" },
                        name = new { type = "string" }
                    }
                }
            },
            new
            {
                name = "godot_set_property",
                description = "Sets a property on a node.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        path = new { type = "string" },
                        property = new { type = "string" },
                        value = new { type = "object" }
                    }
                }
            },
            new
            {
                name = "godot_call_node_method",
                description = "Calls a method on a node.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        path = new { type = "string" },
                        method = new { type = "string" },
                        args = new { type = "array", items = new { type = "object" } }
                    }
                }
            },
            new
            {
                name = "godot_connect_signal",
                description = "Connects a signal to a target script method.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        source_path = new { type = "string" },
                        signal = new { type = "string" },
                        target_path = new { type = "string" },
                        method = new { type = "string" }
                    }
                }
            },
            new
            {
                name = "godot_open_scene",
                description = "Opens a scene in the Editor.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        path = new { type = "string" }
                    }
                }
            },
            new
            {
                name = "godot_save_scene",
                description = "Saves the currently open scene.",
                inputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            },
            new
            {
                name = "godot_screenshot",
                description = "Captures the viewport and saves it to a file.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        path = new { type = "string" },
                        target = new { type = "string", description = "\"game\" or \"editor\"" }
                    }
                }
            },
            new
            {
                name = "godot_create_resource",
                description = "Creates a new Resource file.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        type = new { type = "string" },
                        path = new { type = "string" }
                    }
                }
            },
            new
            {
                name = "godot_set_project_setting",
                description = "Modifies project.godot settings.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        name = new { type = "string" },
                        value = new { type = "object" }
                    }
                }
            },
            new
            {
                name = "godot_input_map_add",
                description = "Adds an Input Action and assigns a key/button.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        action = new { type = "string" },
                        key = new { type = "string" }
                    }
                }
            },
            new
            {
                name = "godot_filesystem_move",
                description = "Moves/Renames a file using Godot's system.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        source = new { type = "string" },
                        destination = new { type = "string" }
                    }
                }
            },
            new
            {
                name = "godot_filesystem_remove",
                description = "Removes a file via EditorFileSystem.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        path = new { type = "string" }
                    }
                }
            },
            new
            {
                name = "godot_reimport",
                description = "Triggers a reimport of a file.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        path = new { type = "string" },
                        options = new { type = "object" }
                    }
                }
            },
            new
            {
                name = "godot_debug_break",
                description = "Pause execution.",
                inputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            },
            new
            {
                name = "godot_debug_continue",
                description = "Resume execution.",
                inputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            },
            new
            {
                name = "godot_debug_step",
                description = "Step over.",
                inputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            },
            new
            {
                name = "godot_get_stack_trace",
                description = "Get current stack trace.",
                inputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            },
            new
            {
                name = "godot_set_breakpoint",
                description = "Set breakpoint.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        path = new { type = "string" },
                        line = new { type = "integer" }
                    }
                }
            }
        };
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
