using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.RunRngs;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;

namespace STS2_Starborn.Cards.Uncommon;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class LurukaSoupCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    private static readonly MarkSlot[] _slots =
    [
        MarkSlot.Primary,
        MarkSlot.Secondary,
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1),
        new IntVar("Stacks", 1),
        new DynamicVar("Times", 4m),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var cardToExhaust = (await CardSelectCmd.FromHand(
            choiceContext, Owner,
            new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, DynamicVars.Cards.IntValue),
            null, this)).FirstOrDefault();
        if (cardToExhaust != null)
            await CardCmd.Exhaust(choiceContext, cardToExhaust);

        var rng = ModRunRngRegistry.Get(Owner, Const.ModId, "luruka_soup");
        var times = DynamicVars["Times"].IntValue;
        var stacks = DynamicVars["Stacks"].IntValue;

        for (int i = 0; i < times; i++)
        {
            var availableSlots = _slots.Where(CanGainElementMark).ToArray();
            if (availableSlots.Length == 0) break;

            var slot = availableSlots[rng.NextInt(0, availableSlots.Length)];
            await SealElementMarkCmd.GainElementMarks(choiceContext, slot, Owner, stacks);
        }
    }

    private bool CanGainElementMark(MarkSlot slot)
    {
        return slot switch
        {
            MarkSlot.Primary => PrimaryStacks < ElementMarkManager.MaxSealStacks,
            MarkSlot.Secondary => SecondaryStacks < ElementMarkManager.MaxSealStacks,
            _ => false,
        };
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Times"].UpgradeValueBy(2m);
    }
}
