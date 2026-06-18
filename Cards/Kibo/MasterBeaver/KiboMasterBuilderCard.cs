using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Kibo;

[RegisterCard(typeof(KiboCardPool))]
[KiboAbilityOf(KiboTypeId.MasterBeaver)]
public sealed class KiboMasterBuilderCard() : KiboCard(1, CardType.Skill, TargetType.Self)
{
    public override bool GainsBlock => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        KiboKeywords.NormalKeyword,
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(7, ValueProp.Move),
        ElementMark(1, SealElementType.Water, "ElementMarkWater"),
        ElementMark(1, SealElementType.Wood, "ElementMarkWood"),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var varName = Owner.RunState.Rng.CombatTargets.NextBool()
            ? "ElementMarkWater"
            : "ElementMarkWood";
        var elementVar = (SealElementVar)DynamicVars[varName];

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await SealElementMarkCmd.GainElementMarks(
            choiceContext, MarkSlot.Primary, Owner, elementVar.IntValue, elementVar.ElementType);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(5);
    }
}
