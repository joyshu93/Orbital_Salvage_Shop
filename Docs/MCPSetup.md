# Unity automation boundary

Last reviewed: 2026-08-21 (KST)

## Current decision: no Unity MCP

Unity MCP is not installed or authorized for this project. Until the developer deliberately enables Unity's official Authorized Agentic Access:

- Codex may edit repository files, tests, documentation, and PowerShell automation without operating Unity.
- The human developer opens Unity, invokes menu commands, runs `scripts/test-unity.ps1`, and runs `scripts/build-android.ps1`.
- Codex must not launch or control Unity Editor/Hub, invoke Unity in batch mode or through a command-line interface, read the live Unity Console, or operate Unity through a community MCP.
- Build and test results are accepted only when the human provides the output or committed report. Codex may then diagnose the supplied result and prepare a patch.
- ADB/logcat work on a human-built Android artifact is separate from operating Unity, but device access still requires the developer's authorization and must not expose private tester data.

This is a conservative project workflow based on Unity Terms of Service section 17.2 and its definition of `Authorized Agentic Access`. It is an operational boundary, not legal advice.

## If agentic Unity automation is reconsidered

Use only Unity's official MCP path and re-check the current terms and documentation first:

1. Confirm the Unity account, plan, editor version, AI Assistant package, Unity Cloud project, Codex client, relay, and gateway are all currently authorized by Unity.
2. Record the approval date, Unity terms version, package version, account/plan basis, and authorized client in this file.
3. Connect through `Edit > Project Settings > AI > Unity MCP` and accept the Codex client in Unity.
4. Begin with read-only hierarchy, Console, and Android build-setting checks.
5. Permit scene or prefab writes only after a disposable test and repository review.
6. Revoke the connection if authorization, subscription, gateway, or allowlist status changes.

Do not use a community MCP as a fallback. No MCP is the fallback.

Official references:

- Unity Terms of Service: https://unity.com/legal/terms-of-service
- Official Unity MCP setup: https://unity.com/blog/unity-ai-mcp-how-to-get-started
