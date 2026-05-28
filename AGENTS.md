# Agent Instructions — Unity Performance Settings

Unity diagnostic harness that exposes Quest's engine-agnostic performance controls (target framerate, FFR, resolution scale, dynamic resolution, CPU/GPU levels, dual-core mode, processor favor) as live in-headset toggles plus an on-demand CPU/GPU load generator. Meant to be paired with OVR Metrics Tool or MQDH Performance Analyzer.

## Source-of-truth files (read these first, do not duplicate their contents in this file)

For setup, build steps, SDK versions, and project layout, read:

- `README.md` — official setup, controls reference, build-time settings
- `ProjectSettings/ProjectVersion.txt` — Unity editor version
- `Packages/manifest.json` — Unity package versions (Meta XR Core SDK, Interaction SDK, etc.)
- `Assets/Plugins/Android/AndroidManifest.xml` — package id, permissions, `com.oculus.supportedDevices`
- `Assets/PerformanceSettings/Scripts/Editor/IntroEditorWindow.cs` — source of truth for the Meta/Unity Performance editor window and build-time toggles
- `.gitattributes` — Git LFS (required)
- `LICENSE` — license terms (MIT; TMP files under Unity Companion License)

## Quest / Horizon-specific notes

- Not a template — its purpose is empirical measurement on-device with OVR Metrics Tool or MQDH Performance Analyzer attached.
- Menus are rendered as compositor layers by design; FFR's visible effect therefore only shows on the world outside the menus. Do not "fix" this.
- GPU level 5 requires dynamic resolution to be enabled on Quest 2 / Pro.
- `com.oculus.supportedDevices` in `AndroidManifest.xml` gates access to some performance controls; copy this entry when porting settings to another project.

# Meta Quest tooling

This is a Meta Quest / Horizon OS sample. The bespoke intro above is the source of truth for what this project is and how it's built — use it (and the files it points at) instead of restating facts from memory.

When the user asks anything about Quest device behavior, build / deploy / debug / capture flows, on-device performance, or Horizon OS APIs, reach for these tools instead of generic Unity answers:

- **`hzdb`** — Quest-aware ADB wrapper (device list, install / launch / stop, logs, screenshots, Perfetto traces, on-device docs search). Already wired up as an MCP server via `.mcp.json`, `.vscode/mcp.json`, and `.cursor/mcp.json`. Also runnable directly: `npx -y @meta-quest/hzdb <subcommand>`.
- **Meta Quest Agentic Tools** — the full skill set, including Unity-specific skills: <https://github.com/meta-quest/agentic-tools>. Install per your client (Claude Code: `/plugin install meta-vr@meta-quest`; Gemini CLI: `gemini extensions install https://github.com/meta-quest/agentic-tools`; Cursor / VS Code: install the **Meta Horizon** extension from the Marketplace).

A few behavior expectations:

- **Read this repo's files first.** Before answering anything project-specific, read `README.md` and whichever source-of-truth files the intro above points at. Don't restate their contents in chat — quote or link instead.
- **Use `hzdb` for device-side work.** Anything that touches an attached Quest (install, launch, logs, screenshot, capture, manifest inspection) goes through `hzdb`, not raw `adb`.
- **Check live Horizon OS docs before answering API questions.** `hzdb docs search "..."` queries the live docs; training data on Horizon OS APIs goes stale fast.
- **Don't fabricate SDK / engine versions.** If a version isn't visible in this repo's files, say so rather than guessing.
