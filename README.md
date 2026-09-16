# NFTL Burner

Unity mini-game. Licensed under Apache-2.0.

## Excluded commercial plugins

Two Unity Asset Store products are **not** redistributed in this repository
because their licenses prohibit public redistribution:

- `Assets/Plugins/CodeStage/` — Anti-Cheat Toolkit (Code Stage)
- `Assets/Plugins/Beebyte/` — Beebyte Obfuscator
- `ProjectSettings/ACTkSettings.asset`

Game code references them via `Obscured*` value types and build
obfuscation attributes; no-op/passthrough stubs at
`Assets/Scripts/Stubs/LicensedPluginStubs.cs` keep the source compiling.
Re-add the real plugins from your Asset Store library to restore the
behavior. Scenes containing ACTk detector components will show missing-script
warnings until the plugin is restored.
