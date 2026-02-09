# Specification: Godot C# MCP Integration

## 1. Project Overview
A unified C# solution to enable AI assistants (via MCP) to control the Godot Engine Editor. The system allows an AI to build, debug, test, and manipulate the scene tree of a running Godot project or the Editor itself.

*   **Repository Root**: `godot-mcp-csharp/`
*   **Language**: C# (.NET 6/7/8 compatible with Godot 4.x)
*   **Architecture**: Dual-Bridge Pattern.
    *   **Component A (Server)**: A standalone MCP Server (Console App).
    *   **Component B (Plugin)**: A Godot Editor Plugin (C# Library) running inside the Editor.

## 2. Architecture: The Dual-Bridge

The system relies on two distinct communication channels between the **MCP Server** and the **Godot Editor/Engine**.

### Channel 1: Control (WebSocket)
*   **Protocol**: JSON-RPC 2.0 over WebSocket.
*   **Endpoint**: `ws://127.0.0.1:6009`
*   **Server**: Godot Editor Plugin (listens).
*   **Client**: MCP Server (connects).
*   **Purpose**: Editor control, Scene Tree inspection, Node manipulation, Build triggering.

### Channel 2: Debug (DAP - Debug Adapter Protocol)
*   **Protocol**: DAP (Debug Adapter Protocol).
*   **Endpoint**: `tcp://127.0.0.1:6006` (Default Godot Debug Port).
*   **Server**: Godot Engine (listens when game is running).
*   **Client**: MCP Server (connects).
*   **Purpose**: Capturing `stdout`/`stderr` (errors), setting breakpoints, stepping, pausing execution, and **injecting input**.

## 3. Directory Structure
The solution `GodotMcp.sln` must contain:

```text
godot-mcp-csharp/
├── GodotMcp.sln
├── src/
│   ├── GodotMcp.Server/          # Standalone MCP Server (Console App)
│   │   ├── GodotMcp.Server.csproj
│   │   ├── Program.cs            # Entry point
│   │   ├── Mcp/                  # MCP Tool Definitions
│   │   ├── Bridge/               # WebSocket Client logic
│   │   └── Debugger/             # DAP Client logic
│   │
│   └── GodotMcp.EditorPlugin/    # Godot C# Addon
│       ├── GodotMcp.EditorPlugin.csproj
│       ├── GodotMcpPlugin.cs     # Main EditorPlugin class
│       ├── Server/               # WebSocket Server logic
│       └── Logic/                # Request handlers (Scene, Build, Test)
│
└── addons/                       # Deployment folder for Godot
    └── godot_mcp_csharp/         # The compiled output goes here
        ├── plugin.cfg
        └── GodotMcp.EditorPlugin.dll
```

## 4. MCP Tool Definitions

The MCP Server must expose the following tools to the AI. Generic file system tools are **strictly excluded**.

### A. Lifecycle & Build
*   **`godot_build`**
    *   *Description*: Triggers a build of the project.
*   **`godot_play`**
    *   *Description*: Launches the game.
    *   *Params*:
        *   `scene_path` (string, optional): Specific scene to play. Defaults to main scene.
        *   `debug` (bool): If true, ensures debugger (DAP) is attached.
*   **`godot_stop`**
    *   *Description*: Stops the running game.
*   **`godot_get_status`**
    *   *Description*: Returns Editor status (Open/Closed), Game status (Running/Stopped), and Plugin connection status.

### B. Scene Inspection (Control Channel)
*   **`godot_get_scene_tree`**
    *   *Description*: Returns a hierarchical JSON representation of the currently active tree.
    *   *Params*:
        *   `scope`: "editor" (edited scene) or "runtime" (playing game).
*   **`godot_inspect_node`**
    *   *Description*: Get detailed properties, signals, and groups of a node.
    *   *Params*:
        *   `path` (string): Absolute node path (e.g., `/root/Main/Player`).
*   **`godot_list_files`**
    *   *Description*: Lists files in the `res://` directory.

### C. Scene Manipulation (Control Channel)
*   **`godot_add_node`**
    *   *Description*: Adds a node to the current scene.
    *   *Params*:
        *   `parent_path`: Path to parent.
        *   `type`: Class name (e.g., "Sprite2D").
        *   `name`: Desired name.
*   **`godot_instantiate_scene`**
    *   *Description*: Instantiates a `.tscn` file as a child of a node.
    *   *Params*:
        *   `parent_path`: Path to parent node.
        *   `scene_path`: Path to the scene file (`res://...`).
        *   `name`: Desired name (optional).
*   **`godot_set_property`**
    *   *Description*: Sets a property on a node.
    *   *Params*:
        *   `path`: Node path.
        *   `property`: Property name.
        *   `value`: Value (JSON).
    *   *Special Handling*: If `value` is a string starting with `res://`, the plugin MUST attempt to `GD.Load<Resource>()` it.
*   **`godot_call_node_method`**
    *   *Description*: Calls a method on a node. Essential for TileMaps (`set_cell`), AnimationPlayers (`play`), etc.
    *   *Params*:
        *   `path`: Node path.
        *   `method`: Method name (e.g., "set_cell").
        *   `args`: Array of arguments.
*   **`godot_connect_signal`**
    *   *Description*: Connects a signal to a target script method.
    *   *Params*:
        *   `source_path`: Path to the node emitting the signal.
        *   `signal`: Name of the signal (e.g., "pressed", "body_entered").
        *   `target_path`: Path to the node receiving the signal.
        *   `method`: Name of the function to call.

### D. Editor State Management
*   **`godot_open_scene`**
    *   *Description*: Opens a scene in the Editor (changes the active tab).
    *   *Params*:
        *   `path`: Path to the scene (`res://...`).
*   **`godot_save_scene`**
    *   *Description*: Saves the currently open scene.

### E. Asset & Project Management
*   **`godot_create_resource`**
    *   *Description*: Creates a new Resource file (e.g., Shape, Material, TileSet).
    *   *Params*:
        *   `type`: Class name (e.g., "RectangleShape2D", "StandardMaterial3D").
        *   `path`: Output path (`res://...`).
*   **`godot_set_project_setting`**
    *   *Description*: Modifies `project.godot` settings. Use this to add Autoloads (Setting: `autoload/Name`, Value: `*res://Path.tscn`).
    *   *Params*:
        *   `name`: Setting path.
        *   `value`: Value.
*   **`godot_input_map_add`**
    *   *Description*: Adds an Input Action and assigns a key/button.
    *   *Params*:
        *   `action`: Action name (e.g., "jump").
        *   `key`: Key string (e.g., "Space", "A").
*   **`godot_filesystem_move`**
    *   *Description*: Moves/Renames a file using Godot's system (updates dependencies).
    *   *Params*:
        *   `source`: `res://...`
        *   `destination`: `res://...`
*   **`godot_filesystem_remove`**
    *   *Description*: Removes a file via EditorFileSystem (updates cache).
    *   *Params*:
        *   `path`: `res://...`
*   **`godot_reimport`**
    *   *Description*: Triggers a reimport of a file, optionally with parameters.
    *   *Params*:
        *   `path`: File to reimport (`res://icon.png`).
        *   `options`: Dictionary of import options (e.g., `{ "importer": "texture", "compress/mode": 0 }`).

### F. Debugging & Testing (DAP Channel)
*   **`godot_debug_break`**: Pause execution.
*   **`godot_debug_continue`**: Resume execution.
*   **`godot_debug_step`**: Step over.
*   **`godot_set_breakpoint`**: Set breakpoint at file/line.
*   **`godot_get_stack_trace`**: Get current stack trace.
*   **`godot_send_input`**
    *   *Description*: Simulates input in the running game. Can send High-Level Actions OR Low-Level Events.
    *   *Params*:
        *   `action` (optional string): Name of action (e.g., "jump").
        *   `event_type` (optional string): "key", "mouse_button", "joy_button", "joy_axis".
        *   `data` (optional object):
            *   For "key": `{ "keycode": "Space", "pressed": true }`
            *   For "joy_button": `{ "index": 0, "pressed": true }`
            *   For "mouse_button": `{ "button_index": 1, "pressed": true, "position": "100,100" }`
    *   *Implementation*: Uses DAP `evaluate` request to call `Input.ParseInputEvent`.
*   **`godot_screenshot`**
    *   *Description*: Captures the viewport (Game or Editor) and saves it to a file.
    *   *Params*:
        *   `path`: Output path (e.g., `user://shot.png` or an absolute path).
        *   `target`: "game" (active viewport) or "editor" (editor interface).

## 5. Implementation Requirements

### 5.1 GodotMcp.EditorPlugin
*   Must inherit `EditorPlugin`.
*   On `_EnterTree`, start a WebSocket listener on port 6009.
*   **Thread Safety**: Incoming WS messages arrive on a background thread. You MUST use `CallDeferred` or `SynchronizationContext` to touch Godot Nodes.
*   **Response Format**: All WS responses must be JSON-RPC 2.0.
*   **Resource Loading**: When setting properties or calling methods, check string arguments. If they match `res://*`, load the resource.

### 5.2 GodotMcp.Server
*   **MCP Library**: Use `ModelContextProtocol.NET` (or equivalent).
*   **DAP Client**: Implement a lightweight TCP client that connects to port 6006.
    *   It must subscribe to the `output` event to capture `GD.Print` and Errors.
    *   It should forward these errors to the MCP client (AI) so it knows if its code failed.

### 5.3 Build System
*   The `EditorPlugin` project must include a `<Target Name="CopyPlugin" AfterTargets="Build">` to copy the DLL and `plugin.cfg` to the `addons/godot_mcp_csharp` folder automatically.

## 6. Implementation Reference (Technical Appendix)

### 6.1 Threading & Awaiting the Main Thread
Godot API calls MUST run on the main thread. Since WebSocket messages arrive on a background thread, use this pattern to execute logic and return values safely:

```csharp
public async Task<T> ExecuteOnMainThread<T>(Func<T> action)
{
    var tcs = new TaskCompletionSource<T>();
    
    // Defer the execution to the main thread
    Godot.Callable.From(() =>
    {
        try
        {
            var result = action();
            tcs.SetResult(result);
        }
        catch (Exception ex)
        {
            tcs.SetException(ex);
        }
    }).CallDeferred();

    return await tcs.Task;
}
```

### 6.2 Scene Tree DTO Schema
When implementing `godot_get_scene_tree`, return this structure:

```json
{
  "name": "Player",
  "class": "CharacterBody2D",
  "path": "/root/Main/Player",
  "children": [ ... ]
}
```

### 6.3 Input Simulation via DAP
To implement `godot_send_input`, the Server should send a DAP `evaluate` request. The expression string must be valid C# or GDScript running in the game.

**Example C# Expression to send:**
```csharp
Input.ParseInputEvent(new InputEventKey { Keycode = Key.Space, Pressed = true });
```
*Note: You may need to fully qualify types (e.g., `Godot.Input.ParseInputEvent`) depending on the context.*

### 6.4 Handling "res://" in Properties
When `godot_set_property` receives a string value like `"res://icon.svg"`, the Plugin must:
1.  Detect the prefix.
2.  Call `GD.Load(value)`.
3.  Set the property to the *loaded Resource object*, not the string.
