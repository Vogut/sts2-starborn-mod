using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Kibo;

[RegisterCard(typeof(KiboCardPool))]
[KiboAbilityOf(KiboTypeId.Cabbird, true)]
public sealed class KiboManCard() : KiboCard(1, CardType.Skill, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        KiboKeywords.UltimateKeyword,
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<YouAreSufferingPower>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2),
        new IntVar("YouAreSufferingStacks", 1),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Check if player has YouAreSufferingPower
        var power = Owner.Creature.Powers.FirstOrDefault(p => p is YouAreSufferingPower);

        if (power != null)
        {
            // Has the power: draw cards and remove it
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
            await PowerCmd.Remove(power);
        }
        else
        {
            // Doesn't have the power: apply it
            await PowerCmd.Apply<YouAreSufferingPower>(
                choiceContext, Owner.Creature,
                DynamicVars["YouAreSufferingStacks"].IntValue,
                Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(3);
        DynamicVars["YouAreSufferingStacks"].UpgradeValueBy(1);
    }
}
