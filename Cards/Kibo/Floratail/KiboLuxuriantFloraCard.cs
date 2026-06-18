using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2_Starborn.Element;
using STS2_Starborn.Commands;
using STS2_Starborn.Combat;

namespace STS2_Starborn.Cards.Kibo;

[RegisterCard(typeof(KiboCardPool))]
[KiboAbilityOf(KiboTypeId.Floratail)]
public sealed class KiboLuxuriantFloraCard() : KiboCard(1, CardType.Skill, TargetType.Self)
{
    public override bool GainsBlock => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        KiboKeywords.NormalKeywordId.GetModCardKeyword(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(4, ValueProp.Move),
        ElementMark(1, SealElementType.Wood),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await SealElementMarkCmd.GainElementMarks(
            choiceContext, MarkSlot.Secondary, Owner, DynamicVars["ElementMark"].IntValue);
    }
}
