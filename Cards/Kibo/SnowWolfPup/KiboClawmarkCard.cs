using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Kibo;

[RegisterCard(typeof(KiboCardPool))]
[KiboAbilityOf(KiboTypeId.SnowWolfPup, true)]
public sealed class KiboClawmarkCard() : KiboCard(1, CardType.Skill, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        KiboKeywords.UltimateKeyword,
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        StarbornCardVars.ElementMark(1, SealElementType.Ice, "ElementMarkSecondary"),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (IsUpgraded)
        {
            await SealElementMarkCmd.GainElementMarks(
                choiceContext, MarkSlot.Secondary, Owner,
                DynamicVars["ElementMarkSecondary"].IntValue);
        }
        else
        {
            await SealElementMarkCmd.SetElementType(
                choiceContext, MarkSlot.Secondary, Owner, SealElementType.Ice);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
