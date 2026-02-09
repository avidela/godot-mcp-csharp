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
*   **Purpose**: Capturing `stdout`/`stderr` (errors), setting breakpoints, stepping, pausing execution.

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

### D. Debugging (DAP Channel)
*   **`godot_debug_break`**: Pause execution.
*   **`godot_debug_continue`**: Resume execution.
*   **`godot_debug_step`**: Step over.
*   **`godot_set_breakpoint`**: Set breakpoint at file/line.
*   **`godot_get_stack_trace`**: Get current stack trace.

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
