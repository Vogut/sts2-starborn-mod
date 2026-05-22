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

Scope guide:

| Scope | Use for |
|---|---|
| `SaveScope.Global` | Mod settings, account-wide preferences |
| `SaveScope.Profile` | Progression, unlock data tied to a save file |
| `SaveScope.InMemory` | Temporary data using the store API without writing to disk |
| `SaveScope.RunSavedData` | Per-run sidecar data (needs `contextProvider` overload) |

Always use a class (not a primitive) so you can add fields in future versions without migrating the storage slot.

---

## Harmony Patches

Use RitsuLib's patcher API instead of vanilla Harmony directly — it supports graceful mod disabling on failure:

```csharp
// In Entry.Init():
var patcher = RitsuLibFramework.CreatePatcher("MyMod", "combat");
patcher.RegisterPatches<MyCombatPatches>();
RitsuLibFramework.ApplyRequiredPatcher(patcher, DisableMod);

// Optional: separate patcher for a feature that can safely fail
var optPatcher = RitsuLibFramework.CreatePatcher("MyMod", "compat");
optPatcher.RegisterPatches<MyCompatPatches>();
optPatcher.PatchAll();  // not "required" — mod stays active if it fails
```

Implement `IPatchMethod` for each patch target:

```csharp
using STS2RitsuLib.Patching.Core;

public sealed class MyCombatPatches : IPatchMethod
{
    public static string PatchId => "my_mod_combat_patch";
    public static string Description => "Adjust combat start behavior";
    public static bool IsCritical => true;

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

Register via content pack:

```csharp
RitsuLibFramework.CreateContentPack("MyMod")
    .KeywordOwned("HEAT")          // card_keywords table
    // or: .CardKeywordOwnedByLocNamespace("heat")
    .Apply();
```

Add to `card_keywords/eng.json`:
```json
{
    "MY_MOD_HEAT.title": "Heat",
    "MY_MOD_HEAT.description": "Some cards change based on current Heat."
}
```

---

## FMOD Audio

```csharp
// In Entry.Init() — register .bank files bundled in your PCK
RitsuLibFramework.RegisterAudioBank("res://MyMod/audio/MyMod.bank");
RitsuLibFramework.RegisterAudioBank("res://MyMod/audio/MyMod.strings.bank");
```

Reference the FMOD event path in `CharacterAssetProfile.Audio`:
```csharp
Audio = new() { AttackSfx = "event:/MyMod/character_attack" }
```

Or play directly: `AudioCmd.Play("event:/sfx/mymod/my_sfx");`

---

## Custom Events (In-Game Story Events)

```csharp
[RegisterSharedEvent]
// or: [RegisterActEvent(typeof(MyAct))]
public sealed class MyEvent : ModEventTemplate
{
    protected override async Task OnEnterEvent(PlayerChoiceContext ctx, Player player)
    {
        var choice = await EventCmd.PresentChoice(ctx, ["Option A", "Option B", "Leave"]);
        switch (choice)
        {
            case 0:  await HandleOptionA(ctx, player); break;
            case 1:  await HandleOptionB(ctx, player); break;
        }
    }

    // Helper from ModEventTemplate:
    // protected string ModOptionKey(string pageName, string optionName)
    // protected LocString PageDescription(string pageName)
}
```

Event localization keys: `{ENTRY}.pages.INITIAL.description`, `{ENTRY}.pages.INITIAL.options.OPEN`, etc.
Keep page and option names stable after release.

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

