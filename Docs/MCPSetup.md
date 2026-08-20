# Unity MCP setup

The project works without MCP through batch tests and Editor automation. MCP adds live hierarchy, component, Console, and build-setting context.

As of 2026-08-20, the official Unity MCP requires Unity 6+, the AI Assistant package, a Unity Cloud-linked project, and an active trial or eligible subscription. The current Codex session does not expose a Unity MCP tool, so connection is not yet claimed.

## Connect the official provider

1. Open the project with Unity 6000.3.21f1 and sign in.
2. Link it to a Unity Cloud project.
3. Select the AI toolbar button and install the official AI Assistant package.
4. Open `Edit > Project Settings > AI > Unity MCP`.
5. Confirm Unity Bridge is green/Running; select Start if needed.
6. Expand Integrations, choose Codex, and select Configure. If Codex is not listed, configure `%USERPROFILE%\.unity\relay\relay_win.exe --mcp` manually in Codex.
7. Return to Unity MCP settings and Accept the pending Codex client.
8. Verify with read-only calls: list the Main hierarchy, read Console warnings/errors, and inspect Android build settings.

Only after those checks should MCP write a temporary GameObject or edit a Scene. Do not install CoplayDev alongside the official provider. Use the community provider only if the official package is unavailable in this Unity account, and remove one before installing the other.

Official reference: https://unity.com/blog/unity-ai-mcp-how-to-get-started
