# Mod Settings & Diagnostics — STS2 RitsuLib Mod

---

## Mod Settings UI

RitsuLib provides a built-in settings page system. Register a page after your backing
data store is registered:

```csharp
// In Entry.Init(), after BeginModDataRegistration block

RitsuLibFramework.RegisterModSettings("MyMod", page => page
    .WithTitle(ModSettingsText.Literal("My Mod"))
    .WithModDisplayName(ModSettingsText.Literal("My Mod"))
    .AddSection("general", section => section
        .WithTitle(ModSettingsText.Literal("General"))
        .AddToggle(
            "enabled",
            ModSettingsText.Literal("Enable My Mod"),
            new ModSettingsValueBinding<MySettings, bool>(
                "MyMod",
                "settings",
                SaveScope.Global,
                s => s.Enabled,
                (s, value) => s.Enabled = value))
        .AddIntSlider(
            "volume",
            ModSettingsText.Literal("Volume"),
            new ModSettingsValueBinding<MySettings, int>(
                "MyMod",
                "settings",
                SaveScope.Global,
                s => s.Volume,
                (s, value) => s.Volume = value),
            minValue: 0,
            maxValue: 100)));
```

Text helpers:
- `ModSettingsText.Literal("text")` — fixed string (use for mod-internal labels)
- `ModSettingsText.I18N(key)` — I18N-backed string for localizable UI
- `ModSettingsText.LocString(locString)` — wraps a game `LocString`

The settings binding automatically calls `Save()` when the player changes a value.
Use `SaveScope.Global` for settings that should apply across all game profiles.

---

## DataStore + Settings: Full Pattern

```csharp
// 1. Define your settings model
public sealed class MySettings
{
    public bool Enabled { get; set; } = true;
    public int Volume { get; set; } = 80;
}

// 2. Register data store
using (RitsuLibFramework.BeginModDataRegistration("MyMod"))
{
    var store = RitsuLibFramework.GetDataStore("MyMod");
    store.Register(
        key: "settings",
        fileName: "settings.json",
        scope: SaveScope.Global,
        defaultFactory: () => new MySettings(),
        autoCreateIfMissing: true);
}

// 3. Register settings UI (reads from same store)
RitsuLibFramework.RegisterModSettings("MyMod", page => page
    .WithTitle(ModSettingsText.Literal("My Mod"))
    .AddSection("general", section => section
        .AddToggle("enabled", ModSettingsText.Literal("Enabled"),
            new ModSettingsValueBinding<MySettings, bool>(
                "MyMod", "settings", SaveScope.Global,
                s => s.Enabled, (s, v) => s.Enabled = v))));

// 4. Read settings anywhere in your mod
var settings = RitsuLibFramework.GetDataStore("MyMod").Get<MySettings>("settings");
if (settings.Enabled) { ... }
```

---

## Diagnostics: RitsuLib Warning Areas

RitsuLib logs warnings for common authoring mistakes. Treat these categories as checklists
before release:

| Warning area | Common causes |
|---|---|
| Content registration | Model registered too late, twice, or with a conflicting ID |
| Asset paths | A profile points at a missing `res://` resource |
| Localization | A key is missing from a game table or I18N source |
| Unlocks | Rule references an epoch or character that cannot resolve |
| Patching | Required target method is missing; patch class has no Harmony method |
| Audio | Bank, event path, GUID, bus, or audio file cannot resolve |

**Release blockers** — fix before publishing:
- Character asset path warnings (fewer safe fallbacks than cards/relics)
- Required patch failures (`IsCritical = true` patches that cannot apply)
- Model ID conflicts

---

## Debug Compatibility Mode

RitsuLib has a built-in debug compatibility mode (off by default). Enable it through the
RitsuLib settings page during development to expose fallback toggles for:
- Missing localization
- Invalid unlock epochs
- Missing Architect dialogue

Do **not** rely on debug fallbacks as the normal release path.

---

## Reading the Game Logs

Game logs are written to:
- Windows: `%APPDATA%\Godot\app_userdata\Slay the Spire 2\logs\`

Look for lines tagged with your mod ID for registration results and any warnings RitsuLib
emits. Use `Entry.Logger.Info(...)` / `Entry.Logger.Warning(...)` for your own diagnostic output.
