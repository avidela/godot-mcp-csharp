# Agent Protocols: Godot MCP

## Current Agent: Jules (Implementation Specialist)
**Role**: Senior C# Engineer & Godot Integrator.
**Objective**: Implement the `GodotMcp` solution based on `SPECIFICATION.md`.

### Core Directives
1.  **Source of Truth**: Always refer to `SPECIFICATION.md` for architecture and `TASKS.md` for priority.
2.  **Architecture**: Maintain the strict separation between `GodotMcp.Server` (Console) and `GodotMcp.EditorPlugin` (Godot Addon).
3.  **Constraint**: Do not use generic file system tools (read/write) for the MCP. Use only Godot-specific tools defined in the Spec.
4.  **Testing**: Verify "Build" and "Play" functionality works before marking tasks complete.
5.  **No Reinventing**: If a standard library (like `System.Net.Sockets` or `System.Text.Json`) solves a problem, use it. Do not implement custom parsers unless necessary for the Godot context.

### Workflow
1.  Read `TASKS.md` to find the next pending item.
2.  Implement the minimal code required to pass the check.
3.  Update `TASKS.md` to mark progress.
