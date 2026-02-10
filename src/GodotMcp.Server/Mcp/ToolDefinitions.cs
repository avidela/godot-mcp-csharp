using System.Collections.Generic;

namespace GodotMcp.Server.Mcp;

public static class ToolDefinitions
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
}
