# Project Setup — STS2 RitsuLib Mod

## Prerequisites

- **Godot 4.5.1 Mono** (the `.NET` variant from godotengine.org/download/archive/4.5.1-stable/)
- **.NET SDK 9+** from dotnet.microsoft.com
- **IDE**: Rider (strongly recommended for beginners) or VS Code with C# Dev Kit

---

## Step 1: Create the Godot Project

Open Godot → New Project → Renderer: **Mobile** (matches game rendering).
Note your project name — use it consistently as your mod ID.

After opening the project, click **"Create C# Solution"** in the top-left editor menu.

---

## Step 2: mod_manifest.json (the mod descriptor)

Create `{ModId}.json` in your project root (e.g., `MyMod.json`):

```json
{
  "id": "MyMod",
  "name": "My Character Mod",
  "author": "YourName",
  "description": "A new playable character.",
  "version": "0.1.0",
  "min_game_version": "0.105.0",
  "has_pck": true,
  "has_dll": true,
  "dependencies": [
    { "id": "STS2-RitsuLib", "min_version": "0.2.27" }
  ],
  "affects_gameplay": true
}
```

Version strings **must** be three-part (`X.Y.Z`) since game version 0.105.0.

---

## Step 3: Configure .csproj

Replace the default `.csproj` content:

```xml
<Project Sdk="Godot.NET.Sdk/4.5.1">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>true</ImplicitUsings>
    <LangVersion>12.0</LangVersion>
    <Nullable>enable</Nullable>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>

    <!-- Change to your actual STS2 install path -->
    <Sts2Dir>D:\Steam\steamapps\common\Slay the Spire 2</Sts2Dir>
    <Sts2DataDir>$(Sts2Dir)\data_sts2_windows_x86_64</Sts2DataDir>
  </PropertyGroup>

  <ItemGroup>
    <Reference Include="sts2">
      <HintPath>$(Sts2DataDir)\sts2.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="0Harmony">
      <HintPath>$(Sts2DataDir)\0Harmony.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <!-- NuGet: pick the right variant for your game version -->
    <PackageReference Include="STS2.RitsuLib" Version="*" />
    <!-- For stable release version: -->
    <!-- <PackageReference Include="STS2.RitsuLib.Compat.0.103.2" Version="*" /> -->
  </ItemGroup>

  <!-- Auto-copy DLL and JSON to game mods folder after build -->
  <Target Name="CopyMod" AfterTargets="PostBuildEvent">
    <MakeDir Directories="$(Sts2Dir)\mods\" />
    <Copy SourceFiles="$(TargetPath)" DestinationFolder="$(Sts2Dir)\mods\$(MSBuildProjectName)\" />
    <Copy SourceFiles="$(MSBuildProjectName).json"
          DestinationFolder="$(Sts2Dir)\mods\$(MSBuildProjectName)\" />
  </Target>
</Project>
```

---

## Step 4: Entry.cs — the mod entry point

Create `Scripts/Entry.cs`:

```csharp
using System.Reflection;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Patching.Core;

namespace MyMod.Scripts;

[ModInitializer(nameof(Init))]
public static class Entry
{
    public const string ModId = "MyMod";  // must match your JSON "id"
    public static Logger Logger { get; private set; } = null!;

    public static void Init()
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Create a logger first — useful for early diagnostics
        Logger = RitsuLibFramework.CreateLogger(ModId);

        // Required: scans the assembly for all [Register*] attributes
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);

        // Only needed if your mod has C# scripts attached to .tscn scenes
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);

        // Register Harmony patches through RitsuLib's patcher (recommended)
        var patcher = RitsuLibFramework.CreatePatcher(ModId, "main");
        patcher.RegisterPatches<MyModPatches>();
        RitsuLibFramework.ApplyRequiredPatcher(patcher, DisableMod);
    }

    private static void DisableMod()
    {
        // Mark your mod disabled when a required patch cannot apply.
    }
}
```

**Key notes:**
- `RegisterModAssembly` handles all content registration (cards, relics, powers, epochs, etc.). Missing this call causes all `[Register*]` attributes to silently fail.
- `EnsureGodotScriptsRegistered` is only needed when the mod contains C# scripts attached to `.tscn` scenes. Pure model or patch mods can omit it.
- `RitsuLibFramework.CreatePatcher` + `ApplyRequiredPatcher` is the recommended way to apply Harmony patches — it gracefully disables the mod if a critical patch fails. Use separate patchers for optional features that should not disable the whole mod on failure.

---

## Step 5: Const.cs (recommended)

Create `Scripts/Const.cs` to centralize asset paths and avoid magic strings:

```csharp
namespace MyMod.Scripts;

public static class Const
{
    public const string ModId = Entry.ModId;
    public const string Version = "0.1.0";

    public static class Paths
    {
        private const string Root = "res://MyMod";
        public const string Images = Root + "/images";
        public const string Cards = Images + "/cards";
        public const string Relics = Images + "/relics";
        public const string Powers = Images + "/powers";
        public const string Scenes = Root + "/scenes";
        public const string EnergyIcon = Images + "/energy_icon.png";
        public const string EnergyIconBig = Images + "/energy_icon_big.png";
    }
}
```

---

## Step 6: Build and Export

**Build DLL:**
```bash
dotnet build
```
The post-build target copies the DLL to the game's `mods/` folder automatically.

**Export PCK** (Godot assets):
1. Project → Export → Add → Windows Desktop
2. Click "Export PCK/ZIP"
3. Filename: `MyMod.pck` (must be `.pck`, not `.zip`)
4. Save to the same folder as the DLL: `{STS2Dir}/mods/MyMod/`

> For macOS compatibility, open `export_presets.cfg` and change
> `binary_format/architecture="x86_64"` to `binary_format/architecture="msil"`.

---

## Step 7: Verify Installation

Your mod folder should look like:
```
{STS2Dir}/mods/MyMod/
├── MyMod.json
├── MyMod.dll
└── MyMod.pck
```

Launch STS2 and check the mod list screen. If the mod appears but content is missing, ensure the two
`Init()` calls are present and check the game logs for errors.
