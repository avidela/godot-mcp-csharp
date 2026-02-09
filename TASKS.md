# Implementation Tasks

## Phase 0: Project Scaffolding
- [ ] Create `GodotMcp.sln` solution.
- [ ] Create `src/GodotMcp.Server` (Console App).
- [ ] Create `src/GodotMcp.EditorPlugin` (Class Library).
- [ ] Add `addons/godot_mcp_csharp/plugin.cfg`.
- [ ] Configure `.csproj` files (references, output paths).

## Phase 1: The Control Bridge (WebSocket)
- [ ] **Plugin**: Implement `WebSocketListener` in `GodotMcp.EditorPlugin`.
    - [ ] Handle connections on port 6009.
    - [ ] Implement basic JSON-RPC parsing.
- [ ] **Server**: Implement `WebSocketClient` in `GodotMcp.Server`.
    - [ ] Auto-connect loop (retry if Godot is closed).
- [ ] **Verify**: Send a "ping" from Server to Plugin and get "pong".

## Phase 2: Read Capabilities
- [ ] **Plugin**: Implement `SceneTraverser`.
    - [ ] Map `EditorInterface.GetEditedSceneRoot()` to a JSON DTO.
- [ ] **Server**: Implement MCP Tools `godot_get_scene_tree`.
- [ ] **Server**: Implement MCP Tools `godot_get_status`.
- [ ] **Server**: Implement MCP Tools `godot_list_files` (res://).

## Phase 3: Control Capabilities
- [ ] **Plugin**: Implement `PlayHandler`.
    - [ ] `EditorInterface.PlayMainScene()`.
    - [ ] `EditorInterface.StopPlayingScene()`.
- [ ] **Server**: Implement MCP Tools `godot_play` and `godot_stop`.

## Phase 4: The Debug Bridge (DAP)
- [ ] **Server**: Implement `DapClient` (TCP to port 6006).
    - [ ] Handle `initialize`, `attach`, `configurationDone`.
    - [ ] Listen for `output` events (stdout/stderr).
- [ ] **Server**: Expose runtime errors to the MCP Context.
- [ ] **Server**: Implement `godot_send_input` via DAP Evaluation.

## Phase 5: Edit Capabilities
- [ ] **Plugin**: Implement `NodeManipulator`.
    - [ ] `AddNode(parent, type, name)`.
    - [ ] `InstantiateScene(parent, scene_path)`.
    - [ ] `SetProperty(node, prop, value)` (Handle `res://` strings!).
    - [ ] `CallMethod(node, method, args)` (Handle TileMaps/AnimationPlayers).
    - [ ] `ConnectSignal(source, signal, target, method)`.
- [ ] **Server**: Implement MCP Tools `godot_add_node`, `godot_instantiate_scene`.
- [ ] **Server**: Implement MCP Tools `godot_set_property`, `godot_call_node_method`, `godot_connect_signal`.

## Phase 6: Editor State & Assets
- [ ] **Plugin**: Implement `EditorHandler`.
    - [ ] `OpenScene(path)`.
    - [ ] `SaveScene()`.
- [ ] **Plugin**: Implement `AssetManager`.
    - [ ] `CreateResource(type, path)`.
    - [ ] `SetProjectSetting(name, value)`.
    - [ ] `AddInputAction(action, key)`.
    - [ ] `MoveFile(src, dst)`.
    - [ ] `Reimport(path, options)`.
- [ ] **Server**: Implement MCP Tools for Editor State and Asset Management.

## Phase 7: Polish
- [ ] Add `godot_build` tool.
- [ ] Verify thread safety (no crashes on `CallDeferred`).
