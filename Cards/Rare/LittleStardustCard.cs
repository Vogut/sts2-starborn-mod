using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Rare;

/// <summary>
/// 小星尘：消耗。调谐0主属性。每打出一次，这张牌在本局游戏中就额外调谐一次。
/// 升级：额外调谐副属性
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public sealed class LittleStardustCard() : StarbornCard(
    2, CardType.Skill, CardRarity.Rare, TargetType.Self
)
{
    private int _currentTuneCount = 1;

    [SavedProperty]
    public int CurrentTuneCount
    {
        get => _currentTuneCount;
        set
        {
            AssertMutable();
            _currentTuneCount = value;
            DynamicVars["TuneCount"].BaseValue = _currentTuneCount;
        }
    }

    protected override bool IsPlayable =>
        StarbornCmd.CanTuning(Owner, MarkSlot.Primary);

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        Tuning(0, SealElementType.Any, "Tuning", MarkSlot.Primary),
        Tuning(0, SealElementType.Any, "TuningSecondary", MarkSlot.Secondary),
        new IntVar("TuneCount", CurrentTuneCount),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var tuneCount = CurrentTuneCount;
        var primaryElementType = ((SealElementVar)DynamicVars["Tuning"]).ElementType;

        // Tune primary element multiple times based on current count
        for (int i = 0; i < tuneCount; i++)
        {
            await StarbornCmd.Tuning(choiceContext, MarkSlot.Primary, Owner,
                DynamicVars["Tuning"].IntValue, primaryElementType, this);
        }

        if (IsUpgraded)
        {
            var secondaryElementType = ((SealElementVar)DynamicVars["TuningSecondary"]).ElementType;

            // Tune secondary element multiple times based on current count
            for (int i = 0; i < tuneCount; i++)
            {
                await StarbornCmd.Tuning(choiceContext, MarkSlot.Secondary, Owner,
                    DynamicVars["TuningSecondary"].IntValue, secondaryElementType, this);
            }
        }

        // Increase tune count for next play (like GeneticAlgorithm)
        BuffFromPlay(1);
        (DeckVersion as LittleStardustCard)?.BuffFromPlay(1);
    }

    protected override void OnUpgrade()
    {
        // Adds secondary tuning in OnPlay
    }

    protected override void AfterDowngraded()
    {
        UpdateTuneCount();
    }

    private void BuffFromPlay(int extraCount)
    {
        CurrentTuneCount += extraCount;
    }

    private void UpdateTuneCount()
    {
        DynamicVars["TuneCount"].BaseValue = CurrentTuneCount;
    }
}
