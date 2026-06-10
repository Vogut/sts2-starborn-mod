# Advanced Topics — STS2 RitsuLib Mod

---

## SavedAttachedState (Per-Object Run Persistence)

Use `SavedAttachedState<TOwner, TValue>` to attach a serializable value to a model object
(relic, card, player, etc.) that persists within a run's save/load cycle.

### Example: turn counter on a relic

```csharp
using STS2RitsuLib.Utils;

[RegisterRelic(typeof(MyRelicPool))]
public sealed class MyRelic : ModRelicTemplate
{
    // One static tracker shared by all instances; keyed per-instance via the indexer
    public static readonly SavedAttachedState<MyRelic, int> TurnsPassed =
        new("TurnsPassed", () => 0);   // default factory — note: () => not _ =>

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext ctx, Player player)
    {
        TurnsPassed[this]++;
        await CardPileCmd.Draw(ctx, 1, player);
    }
}
```

### Example: bool flag (single-use relic)

```csharp
public static readonly SavedAttachedState<MyRelic, bool> WasUsed =
    new("WasUsed", () => false);

public override bool ShouldDieLate(Creature creature) => !WasUsed[this];

public override async Task AfterPreventingDeath(PlayerChoiceContext ctx, Player player)
{
    WasUsed[this] = true;
    SetDisabled(true);
    await HpCmd.Heal(player, (int)(player.MaxHp * 0.35f), this);
}
```

Use `AttachedState<TOwner, TValue>` (non-saved) for transient combat state that resets between runs:

```csharp
private static readonly AttachedState<MyRelic, bool> _usedThisTurn =
    new("UsedThisTurn", () => false);
```

---

## DataStore (Global / Profile / Run Persistence)

For persistent mod settings and account-wide data, use `RitsuLibFramework.GetDataStore()`.
Register data inside `BeginModDataRegistration` so all slots initialize together:

```csharp
using (RitsuLibFramework.BeginModDataRegistration("MyMod"))
{
    var store = RitsuLibFramework.GetDataStore("MyMod");

    store.Register(
        key: "settings",
        fileName: "settings.json",
        scope: SaveScope.Global,          // Global = all game profiles share it
        defaultFactory: () => new MySettings(),
        autoCreateIfMissing: true);

    store.Register(
        key: "progress",
        fileName: "progress.json",
        scope: SaveScope.Profile,         // Profile = per-game-save-file
        defaultFactory: () => new MyProgress(),
        autoCreateIfMissing: true);
}
```

Read and write:

```csharp
var store = RitsuLibFramework.GetDataStore("MyMod");
var settings = store.Get<MySettings>("settings");

store.Modify<MySettings>("settings", data => { data.Volume = 60; });
store.Save("settings");
```

### SaveScope + RunSavedData

Use `SaveScope` for DataStore-persisted data across saves/profiles, and `RunSavedDataStore` for per-run sidecar data:

**SaveScope** values (`STS2RitsuLib.Utils.Persistence`):
| Scope | Use for |
|---|---|
| `SaveScope.Global` | Mod settings, account-wide preferences |
| `SaveScope.Profile` | Progression, unlock data tied to a save file |
| `SaveScope.InMemory` | Temporary data using the store API without writing to disk |

**RunSavedDataStore** for per-run data (replaces the old `SaveScope.RunSavedData`):
```csharp
var store = RitsuLibFramework.GetRunSavedDataStore(ModId);
store.RegisterPerPlayer<MyRunData>("my_run_data");

// Access in gameplay:
var data = store.Get<MyRunData>(player);
store.Modify<MyRunData>(player, data => data.Counter++);
```

Always use a class (not a primitive) so you can add fields in future versions without migrating the storage slot.

---

## Harmony Patches

Use RitsuLib's patcher API instead of vanilla Harmony directly — it supports graceful mod disabling on failure:

```csharp
// In Entry.Init():
var patcher = RitsuLibFramework.CreatePatcher("MyMod", "combat");
patcher.RegisterPatch<MyCombatPatch>();        // single IPatchMethod
patcher.RegisterPatch<MyOtherPatch>();
RitsuLibFramework.ApplyRequiredPatcher(patcher, DisableMod);

// For patches grouped into one class implementing IModPatches:
// patcher.RegisterPatches<MyGroupedPatches>();   // IModPatches (plural)

// Optional: separate patcher for a feature that can safely fail
var optPatcher = RitsuLibFramework.CreatePatcher("MyMod", "compat");
optPatcher.RegisterPatch<MyCompatPatch>();
optPatcher.PatchAll();  // not "required" — mod stays active if it fails
```

Implement `IPatchMethod` for each patch target:

```csharp
using STS2RitsuLib.Patching.Core;

public sealed class MyCombatPatches : IPatchMethod
{
    public static string PatchId => "my_mod_combat_patch";
    public static string Description => "Adjust combat start behavior";   // default: "Patch"
    public static bool IsCritical => true;                                 // default: true

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(CombatRoom), "OnEnter"),
        // Disambiguate overloads: new(typeof(Cls), "Method", new[] { typeof(int) })
    ];

    public static void Postfix(CombatRoom __instance)
    {
        // Standard Harmony postfix — access __instance, __result, arguments by name
    }
}
```

For runtime-discovered methods, use `patcher.RegisterDynamicPatch(...)` instead.

> Use `IsCritical = false` for compatibility patches that can safely be skipped.
> Use ILSpy / dnSpy to inspect `{STS2Dir}/data_sts2_windows_x86_64/sts2.dll`.

---

## Lifecycle Events

Subscribe to framework and game events instead of patching when the event system covers the use case:

```csharp
// One-shot subscription (auto-disposes after first call):
RitsuLibFramework.SubscribeLifecycle<CombatStartingEvent>((evt, sub) =>
{
    PrepareForCombat(evt.RunState);
    sub.Dispose();
});

// Persistent subscription:
var sub = RitsuLibFramework.SubscribeLifecycle<GameReadyEvent>(evt =>
{
    Entry.Logger.Info($"Game ready");
});
// dispose sub when no longer needed
```

Common lifecycle events:

| Category | Key events |
|---|---|
| Framework | `FrameworkInitializingEvent`, `FrameworkInitializedEvent` |
| Model setup | `ContentRegistrationClosedEvent`, `ModelRegistryInitializedEvent` |
| Game node | `GameTreeEnteredEvent`, `GameReadyEvent` |
| Profiles / saves | `ProfileSwitchedEvent`, `RunSavingEvent`, `ProgressSavedEvent` |
| Run flow | `RunStartedEvent`, `RunLoadedEvent`, `RunEndedEvent`, `RoomEnteredEvent` |
| Combat | `CombatStartingEvent`, `CombatEndedEvent`, `CombatVictoryEvent`, `CardPlayedEvent` |
| Cards | `CardDrawnEvent`, `CardDiscardedEvent`, `CardExhaustedEvent`, `CardsFlushedEvent` |
| Rewards | `GoldGainedEvent`, `RelicObtainedEvent`, `RewardTakenEvent` |
| Unlocks | `EpochObtainedEvent`, `EpochRevealedEvent` |

---

## ModRunRngRegistry — Deterministic Per-Mod RNG (v0.3.4)

Replace `Random.Shared` and `new Random()` with per-mod, per-run RNG streams that are save-safe and multiplayer-synced:

```csharp
using STS2RitsuLib.RunRngs;

// Player-scoped RNG
var rng = ModRunRngRegistry.Get(player, ModId, "my_stream");
var index = rng.NextInt(0, candidates.Count);
var item = rng.NextItem(candidates);    // convenience: pick random element

// Run-scoped RNG
var rng = ModRunRngRegistry.Get(runState, ModId, "my_stream");
```

Stream IDs partition RNG state — use distinct IDs for distinct gameplay decisions.

---

## ModCardVars — Dynamic Variables

The preferred API for card description variables is `ModCardVars` via the `DynamicVarSet` property:

```csharp
public override DynamicVarSet DynamicVars => new()
{
    ModCardVars.Int("damage", Damage),
    ModCardVars.Int("block", Block),
    // Computed: value changes based on card state
    ModCardVars.Computed("bonus", 0, card => card?.Upgraded == true ? 5 : 0),
    // With shared hover tooltip
    ModCardVars.Int("heat", Heat)
        .WithSharedTooltip("MY_MOD_HEAT", "res://MyMod/images/ui/heat.png"),
};
```

Reference variables in descriptions: `"Deal {damage} damage. Gain {block} Block."`.

Safe reading from external code:
```csharp
var amount = card.DynamicVars.GetIntOrDefault("damage");
var hasHeat = card.DynamicVars.HasPositiveValue("heat");
```

---

## Custom Keywords

### Registration

Register via content pack or attribute:

```csharp
// Content pack style
RitsuLibFramework.CreateContentPack("MyMod")
    .KeywordOwned("HEAT", ModKeywordCardDescriptionPlacement.Below, includeInCardHoverTip: true)
    .Apply();

// Attribute style (on any class in the mod assembly)
[RegisterOwnedCardKeyword("heat", IncludeInCardHoverTip = true)]
public class MyKeywords { }
```

Localization in `card_keywords/eng.json`:
```json
{
    "MY_MOD_KEYWORD_HEAT.title": "Heat",
    "MY_MOD_KEYWORD_HEAT.description": "Some cards change based on current Heat."
}
```

### Migration: String → CardKeyword (v0.3.5)

RitsuLib v0.3.5 deprecated string-based keyword extension methods. **Always pre-mint a `CardKeyword` value and use the strongly-typed overloads:**

```csharp
using STS2RitsuLib.Keywords;

// Old — compiler warning, per-call hash:
card.HasModKeyword("MY_MOD_KEYWORD_HEAT");
card.AddModKeyword("MY_MOD_KEYWORD_HEAT");

// New — pre-mint once, use everywhere:
public static readonly CardKeyword HeatKeyword = "MY_MOD_KEYWORD_HEAT".GetModCardKeyword();
card.HasModKeyword(HeatKeyword);
card.AddModKeyword(HeatKeyword);
```

`GetModCardKeyword()` is a pure hash function — no registration dependency, safe at static init time.
The same pattern applies to CardTags via `GetModCardTag()`.

### Logger.ErrorNoTrace() (v0.3.3)

Log errors without the automatic stack trace:

```csharp
Logger.ErrorNoTrace("Something went wrong but we don't need a full trace.");
```

---

## FMOD Audio

Reference the FMOD event path in `CharacterAssetProfile.Audio`:
```csharp
Audio = new() { AttackSfx = "event:/MyMod/character_attack" }
```

Or play directly: `AudioCmd.Play("event:/sfx/mymod/my_sfx");`

Audio banks are registered via the Godot audio system. See the vanilla game's audio setup for the current registration pattern.

---

## Custom Events (In-Game Story Events)

```csharp
[RegisterSharedEvent]
// or: [RegisterActEvent(typeof(MyAct))]
public sealed class MyEvent : ModEventTemplate
{
    protected override async Task OnEnterEvent(PlayerChoiceContext ctx, Player player)
    {
        // Use event page system — present pages with options via EventCmd API
        // ModEventTemplate provides helpers for building option keys and page descriptions
    }

    // Helpers from ModEventTemplate:
    // protected string ModOptionKey(string pageName, string optionName)
    // protected LocString PageDescription(string pageName)
}
```

Event options are defined via localization pages. Each page has a description and a list of options.
Event page localization keys:
- `{ENTRY}.pages.INITIAL.description` — page description text
- `{ENTRY}.pages.INITIAL.options.OPEN` — option button label
Keep page and option names stable after release — they are part of the player's event-choice history.

---

## Cross-Mod Compatibility

Define public interfaces in your mod's public API and broadcast via the game's hook listener pattern:

```csharp
// Public interface (in your mod's public assembly)
public interface IMySystemListener
{
    void OnMyEvent(MyEventArgs args);
}

// Broadcast in combat:
foreach (var listener in combatState.IterateHookListeners<IMySystemListener>())
    listener.OnMyEvent(args);
```

---

## Debugging Tips

- **Console commands**: Enable via launch args. Common: `power`, `relic`, `card`, `gold`, `hp`
- **Hot-reload**: Content changes reload without full restart in dev mode
- **Logging**: `Entry.Logger.Info("message")` — appears in game log files
- **ILSpy / dnSpy**: Decompile `{STS2Dir}/data_sts2_windows_x86_64/sts2.dll` to understand vanilla behavior

