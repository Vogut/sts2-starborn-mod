# Epoch / Timeline System — STS2 RitsuLib Mod

The timeline (epoch) system drives both **character unlock** and **progressive content unlock**
as the player achieves milestones. Every character mod needs a story + a set of epochs.

---

## Concept Overview

Each "epoch" is a milestone. The player earns epochs by completing certain in-game actions.
Epochs can gate cards, relics, potions, or even entire characters behind unlock requirements.

The two registration styles (attributes + content pack) work identically — choose based on
whether you prefer the registration to live next to each class or in one central place.

---

## Epoch Base Classes

| Base class | Use for |
|---|---|
| `ModEpochTemplate` | Any epoch — the universal base |
| `ModStoryTemplate` | The story node that groups your epochs into one narrative arc |

`CharacterUnlockEpochTemplate`, `PackDeclaredCardUnlockEpochTemplate`, etc. still exist
in older mods but `ModEpochTemplate` + content pack rules is now the preferred approach.

---

## Unlock Rules (Content Pack Style — preferred)

```csharp
RitsuLibFramework.CreateContentPack("MyMod")
    // Register story and epochs
    .Story<MyStory>()
    .Epoch<MyCharacterEpoch>()
    .StoryEpoch<MyStory, MyCharacterEpoch>()
    .ModEpochAutoTimelineSlotAfterColumn<MyCharacterEpoch>(EpochEra.Seeds0)

    .Epoch<MyFirstWinEpoch>()
    .StoryEpoch<MyStory, MyFirstWinEpoch>()
    .ModEpochAutoTimelineSlotAfterColumn<MyFirstWinEpoch>(EpochEra.Seeds0)

    // Unlock rules
    .UnlockEpochAfterRunAs<MyCharacter, MyCharacterEpoch>()  // complete any run
    .UnlockEpochAfterWinAs<MyCharacter, MyFirstWinEpoch>()   // win a run

    // Gate content behind an epoch
    .RequireEpoch<MyRareCard, MyFirstWinEpoch>()

    // If unlocking a character from another character's progression:
    // .UnlockCharacterAfterRunAs<PreviousCharacter, MyCharacterEpoch>()
    // .RevealAscensionAfterEpoch<MyCharacter, MyAscensionEpoch>()

    .Apply();
```

Full unlock rule table:

| Rule | Trigger |
|---|---|
| `.UnlockEpochAfterRunAs<TChar, TEpoch>()` | Any run completed with that character |
| `.UnlockEpochAfterWinAs<TChar, TEpoch>()` | A win with that character |
| `.UnlockEpochAfterAscensionWin<TChar, TEpoch>(level)` | Ascension win at or above `level` |
| `.UnlockEpochAfterAscensionOneWin<TChar, TEpoch>()` | Shortcut for ascension 1 win |
| `.UnlockEpochAfterRunCount<TEpoch>(count, requireVictory)` | Account-level run count |
| `.UnlockEpochAfterEliteVictories<TChar, TEpoch>(count)` | Counted elite victories |
| `.UnlockEpochAfterBossVictories<TChar, TEpoch>(count)` | Counted boss victories |
| `.RevealAscensionAfterEpoch<TChar, TEpoch>()` | Show ascension UI after epoch granted |
| `.RequireEpoch<TModel, TEpoch>()` | Gate any model behind an epoch |
| `.UnlockCharacterAfterRunAs<TPrev, TEpoch>()` | Unlock from another character's story |

---

## Epoch Base Classes & Attribute Style (legacy-compatible)

The attribute approach still works and is fine for smaller mods:

```csharp
[RegisterStory]
public sealed class MyStory : ModStoryTemplate
{
    protected override string StoryKey => "mymod";
}

[RegisterEpoch]
[RegisterStoryEpoch(typeof(MyStory), Order = 0)]
[AutoTimelineSlotAfterColumn(EpochEra.Seeds0)]
public sealed class MyCharacterEpoch : ModEpochTemplate
{
    public override string Id => "MY_CHARACTER_EPOCH";
    public override EpochAssetProfile AssetProfile => new()
    {
        PackedPortraitPath = Const.Paths.Images + "/epoch_character.png",
        BigPortraitPath = Const.Paths.Images + "/epoch_character_big.png",
    };
}
```

Unlock trigger attributes go on the epoch or in the content pack (both work):

```csharp
[UnlockEpochAfterWinAs(typeof(MyCharacter))]
public sealed class MyFirstWinEpoch : ModEpochTemplate { ... }
```

---

## Epoch Era Timeline Positions

`EpochEra` constants control where an epoch node appears on the timeline:

| Era | Approximate meaning |
|---|---|
| `Seeds0` | Before Act 1 |
| `Blight1` | Act 1 area |
| `Peace0` | Between Act 1 and Act 2 |
| `Blight2` | Act 2 area |
| `Flourish0` | Between Act 2 and Act 3 |
| `Seeds2` | Act 3 area |
| `Invitation5` | End-game / ascension area |

---

## Full Example — 8 Epochs

```csharp
// MyStory.cs
[RegisterStory]
public sealed class MyStory : ModStoryTemplate
{
    protected override string StoryKey => "mymod";
}

// MyCharacterEpoch.cs — character unlock, placed before Act 1
[RegisterEpoch]
[RegisterStoryEpoch(typeof(MyStory), Order = 0)]
[AutoTimelineSlotAfterColumn(EpochEra.Seeds0)]
[RequireAllCardsInPool(typeof(MyCardPool))]     // all cards gated until character unlocked
public sealed class MyCharacterEpoch : ModEpochTemplate
{
    public override string Id => "MY_CHARACTER_EPOCH";
}

// MyFirstWinEpoch.cs — unlocks 3 new cards on first win
[RegisterEpoch]
[RegisterStoryEpoch(typeof(MyStory), Order = 1)]
[AutoTimelineSlotAfterColumn(EpochEra.Seeds0)]
[RegisterEpochCards(typeof(MyNewCard1), typeof(MyNewCard2), typeof(MyNewCard3))]
public sealed class MyFirstWinEpoch : ModEpochTemplate
{
    public override string Id => "MY_FIRST_WIN_EPOCH";
}

// ... additional epochs for Act clears, elite/boss milestones, ascension
```

Wire unlock rules via content pack:

```csharp
RitsuLibFramework.CreateContentPack("MyMod")
    .Story<MyStory>()
    .Epoch<MyCharacterEpoch>().StoryEpoch<MyStory, MyCharacterEpoch>()
    .Epoch<MyFirstWinEpoch>().StoryEpoch<MyStory, MyFirstWinEpoch>()
    .UnlockEpochAfterRunAs<MyCharacter, MyCharacterEpoch>()
    .UnlockEpochAfterWinAs<MyCharacter, MyFirstWinEpoch>()
    .UnlockEpochAfterEliteVictories<MyCharacter, MyEliteEpoch>(15)
    .UnlockEpochAfterBossVictories<MyCharacter, MyBossEpoch>(15)
    .UnlockEpochAfterAscensionOneWin<MyCharacter, MyAscensionEpoch>()
    .Apply();
```

---

## Dev Shortcut

While building the mod, bypass the epoch requirement:

```csharp
public override bool RequiresEpochAndTimeline => false;
```

Set it back to `true` before release so players unlock the character normally.

---

## epochs.json Localization

```json
{
    "MY_CHARACTER_EPOCH.title": "My Character",
    "MY_CHARACTER_EPOCH.description": "A new adventurer enters the Spire.",
    "MY_CHARACTER_EPOCH.unlockInfo": "Complete any run",
    "MY_CHARACTER_EPOCH.unlockText": "Unlock My Character",

    "MY_FIRST_WIN_EPOCH.title": "First Victory",
    "MY_FIRST_WIN_EPOCH.description": "Three new cards unlocked.",
    "MY_FIRST_WIN_EPOCH.unlockInfo": "Win a run as My Character",
    "MY_FIRST_WIN_EPOCH.unlockText": "First Steps"
}
```
