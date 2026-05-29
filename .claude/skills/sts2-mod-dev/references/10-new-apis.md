# RitsuLib v0.3.3–0.3.5 New APIs

This reference covers APIs added in RitsuLib v0.3.3 through v0.3.5. When writing mod code, prefer these
newer APIs over older patterns. The source code in `STS2-RitsuLib/` is always the most authoritative reference.

---

## v0.3.5: String-Based Keyword/Tag Methods Deprecated (BREAKING)

RitsuLib v0.3.5 deprecates six extension methods that accept raw `string` IDs. Every call to the old
overloads incurs a per-call lookup and produces a compiler warning. **Always pre-mint values and use the
strongly-typed overloads.**

### Deprecated → Migration

| Deprecated (string) | Migration (strongly-typed) |
|---|---|
| `card.AddModKeyword(string)` | `card.AddModKeyword(CardKeyword)` |
| `card.RemoveModKeyword(string)` | `card.RemoveModKeyword(CardKeyword)` |
| `card.HasModKeyword(string)` | `card.HasModKeyword(CardKeyword)` |
| `card.AddModCardTag(string)` | `card.AddModCardTag(CardTag)` |
| `card.RemoveModCardTag(string)` | `card.RemoveModCardTag(CardTag)` |
| `card.HasModCardTag(string)` | `card.HasModCardTag(CardTag)` |

### Migration Pattern

Pre-mint the `CardKeyword` / `CardTag` value once as a static field, then use it everywhere:

```csharp
using STS2RitsuLib.Keywords;

// Old — compiler warning, per-call hash lookup:
public static readonly string MyKeywordId =
    ModContentRegistry.GetQualifiedKeywordId(ModId, "my_keyword");
// ...
card.AddModKeyword(MyKeywordId);
card.HasModKeyword(MyKeywordId);

// New — no warning, single lookup at static init time:
public static readonly CardKeyword MyKeyword = MyKeywordId.GetModCardKeyword();
// ...
card.AddModKeyword(MyKeyword);
card.HasModKeyword(MyKeyword);
```

`GetModCardKeyword()` calls `ModKeywordRegistry.GetCardKeyword(id)`, a pure hash function with no
registration dependency — safe to call during static initialization. The same pattern applies to
`CardTag` via `GetModCardTag()`.

### Deterministic Minting

Keyword, CardTag, and CardPile resolution all switched to deterministic minting in v0.3.5.
`GetCardKeyword(id)`, `GetCardTag(id)`, and `GetPileType(id)` always return a valid value for
any input string — pre-registration is no longer required for lookup. Registration still matters
for hover-tip metadata (keywords) and UI configuration (piles).

---

## v0.3.4: ModRunRngRegistry — Deterministic Per-Mod RNG

Provides per-mod, per-run, per-stream RNG seeded from the run's master seed. Save-safe and
multiplayer-synced. **Replace all `Random.Shared` and `new Random()` calls with this.**

```csharp
using STS2RitsuLib.RunRngs;

// Player-scoped RNG stream (preferred)
var rng = ModRunRngRegistry.Get(player, ModId, "my_stream_id");
var index = rng.NextInt(0, candidates.Count);
var item = rng.NextItem(candidates);

// Run-scoped RNG stream
var rng = ModRunRngRegistry.Get(runState, ModId, "my_stream_id");
var value = rng.NextDouble(0, 1);
```

Stream IDs ("my_stream_id") are arbitrary strings that partition RNG state — use distinct IDs for
distinct gameplay decisions (e.g. `"kibo_summon_random"`, `"kibo_auto_play"`).

---

## v0.3.4: Right-Click Interaction System

Register right-click handlers on cards, relics, powers, or potions. Works in both single-player
and multiplayer (via sidecar sync).

### Per-Model Interface (implement on your model class)

```csharp
using STS2RitsuLib.Interactions.RightClick;

// Card gets right-click inspect behavior
public class MyCard : ModCardTemplate, IModRightClickableCard
{
    public async Task OnRightClick(ModRightClickExecutionContext ctx)
    {
        // Show info popup, open custom screen, etc.
    }
}
```

Available interfaces: `IModRightClickableCard`, `IModRightClickableRelic`,
`IModRightClickablePower`, `IModRightClickablePotion`.

### Standalone Handler (register without modifying the model)

```csharp
using STS2RitsuLib.Interactions.RightClick;

ModRightClickRegistry.Register<CardModel>(
    ModId, "my_right_click",
    canHandle: ctx => ctx.Model is CardModel card && card.HasModKeyword(MyKeyword),
    execute: async ctx => { /* handle right-click */ },
    priority: 0);
```

---

## v0.3.4: Relic Visibility System

Hide relics from the player's relic bar without removing them from the inventory:

```csharp
using STS2RitsuLib.Relics.Visibility;

// Register a visibility rule
ModRelicVisibilityRegistry.Register(ModId, relic =>
{
    if (relic is MyRelic myRelic && myRelic.ShouldBeHidden)
        return false;
    return true;
});

// Or use the convenience API:
RitsuLibFramework.RegisterRelicVisibilityRule(ModId, relic => !IsHidden(relic));
```

Call `RitsuLibFramework.RefreshRelicVisibility()` after changing visibility state.

---

## v0.3.4: Model Identity System

Tracks mutable model instances across multiplayer sync boundaries. Useful for identifying
"the same card" after a network round-trip:

```csharp
// Ensure a model is tracked
RitsuLibFramework.EnsureModelIdentity(cardModel);

// Get an identity token (for sending to another peer)
if (RitsuLibFramework.TryGetModelIdentity(cardModel, out var token))
    SendToPeer(token);

// Resolve a token back to a model (on the receiving peer)
if (RitsuLibFramework.TryResolveModelIdentity(token, out var resolvedModel))
    UseResolvedModel(resolvedModel);
```

Most mods won't need this directly — it's used internally by right-click and sidecar sync.
Relevant only if you're building custom multiplayer-synced interactions.

---

## v0.3.5: Card Transform Tracking

Listen for all `CardCmd.Transform` calls (card upgrades, metamorphosis, etc.):

```csharp
using STS2RitsuLib.Cards.Transforms;

// Register a global listener
RitsuLibFramework.GetCardTransformRegistry(ModId).Register(
    listenerId: "my_transforms",
    listener: ctx =>
    {
        Logger.Info($"{ctx.Original} → {ctx.Replacement} in pile {ctx.OriginalPile}");
        return Task.CompletedTask;
    });

// Register for specific card types
RitsuLibFramework.GetCardTransformRegistry(ModId)
    .Register<MyCard, MyUpgradedCard>("my_upgrade_tracker");
```

---

## v0.3.4: Godot Node Factory System

Typed, safe Godot node creation. RitsuLib automatically registers factories during
`EnsureGodotScriptsRegistered()`:

```csharp
// Create a node from a .tscn file
var visuals = RitsuGodotNodeFactories.CreateFromScene<NCreatureVisuals>(
    "res://MyMod/scenes/my_character.tscn");

// Create from a .tres resource
var material = RitsuGodotNodeFactories.CreateFromResource<ShaderMaterial>(
    "res://MyMod/materials/my_material.tres");
```

No manual `.Instantiate<T>()` + casting needed.

---

## v0.3.3: Enhanced Toast System

Rich, tracked toast notifications with progress support:

```csharp
using STS2RitsuLib.Ui.Toast;

// Simple toast
RitsuToastService.Show(RitsuToastRequest.Info("Something happened"));

// Tracked toast with progress (returns a handle)
var handle = RitsuToastService.ShowTracked(RitsuToastRequest.Info("Processing..."));
handle.Update(progress: 0.5f);
handle.UpdateBody("Almost done...");
handle.Dismiss(); // or Close() to keep visible until clicked
```

Toast request factory methods: `RitsuToastRequest.Info(text)`, `.Warning(text)`, `.Error(text)`.
Fluent builders: `.WithTitle(title)`, `.WithDuration(seconds)`, `.WithOnClick(action)`, `.AsPersistent()`.

---

## v0.3.3: Logger.ErrorNoTrace()

Log error messages without the automatic stack trace:

```csharp
Logger.ErrorNoTrace("Something went wrong but we don't need a full trace.");
```

---

## v0.3.3: ModSettings ReadOnlyOnHostSurfaces

Mark individual settings entries as read-only on host surfaces in multiplayer:

```csharp
page.AddToggle("enabled", ...)
    .WithEntryReadOnlyOnHostSurfaces();
```

---

## v0.3.4: HarmonyIL Utility Classes

IL-level transpiler helpers for Harmony patches. Use when you need instruction-level control:

```csharp
using STS2RitsuLib.Utils.HarmonyIl;

// Pattern matching
var pattern = HarmonyIlPattern.Create(
    HarmonyIl.IsLdarg(0),
    HarmonyIl.IsCall(typeof(SomeType).GetMethod("SomeMethod")),
    HarmonyIl.IsStloc(out var localRef));

// Find in IL
var matches = pattern.FindAll(instructions);
foreach (var match in matches)
{
    // match[1] is the Call instruction
}
```

See `STS2-RitsuLib/Utils/HarmonyIl/` for the full API.

---

## v0.3.4: Sidecar Networking Sync

A synchronization layer for multiplayer mod data. Used internally by right-click and model identity.
Custom sync actions/messages:

```csharp
// Register a sync action (ordered against game actions)
RitsuLibSidecarSyncActions.Register(new RitsuLibSidecarSyncActionDescriptor<MyPayload>(
    opcode: 100,
    serialize: payload => WritePayload(payload),
    deserialize: reader => ReadPayload(reader),
    handle: (state, payload) => HandlePayload(payload)));

// Register a sync message (peer-to-peer, not action-ordered)
RitsuLibSidecarSyncMessages.Register(new RitsuLibSidecarSyncMessageDescriptor<MyMessage>(
    opcode: 200,
    serialize: msg => WriteMessage(msg),
    deserialize: reader => ReadMessage(reader),
    handle: (state, msg) => HandleMessage(msg)));
```

Most mods won't need this directly unless they add custom multiplayer-synced mechanics.

---

## Summary: When to Use Each API

| API | Use when |
|---|---|
| Pre-minted `CardKeyword`/`CardTag` | Anywhere you call `HasModKeyword`/`AddModKeyword`/`HasModCardTag` |
| `ModRunRngRegistry` | Any random choice that should be deterministic per run |
| `ModRightClickRegistry` / `IModRightClickableCard` | Adding inspect/info on right-click |
| `ModRelicVisibilityRegistry` | Hiding relics from UI without removing them |
| `ModCardTransformRegistry` | Tracking card upgrades/transforms for achievements or UI |
| `RitsuGodotNodeFactories` | Creating Godot nodes from .tscn/.tres files |
| `RitsuToastService` | On-screen notifications (especially tracked progress) |
| `Logger.ErrorNoTrace()` | Error logging without stack noise |
| `ModModelIdentity` | Multiplayer sync for mutable model references |
| `HarmonyIl` / `HarmonyIlPattern` | IL-level transpiler patches |
