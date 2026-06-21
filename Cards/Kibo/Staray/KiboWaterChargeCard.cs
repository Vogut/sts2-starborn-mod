using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2_Starborn.Cards;
using STS2_Starborn.Commands;
using STS2_Starborn.Combat;
using STS2_Starborn.Element;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Kibo;

[RegisterCard(typeof(KiboCardPool))]
[KiboAbilityOf(KiboTypeId.Staray)]
public sealed class KiboWaterChargeCard() : KiboCard(1, CardType.Skill, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        KiboKeywords.NormalKeyword,
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        StarbornCardVars.ElementMark(1, SealElementType.Water),
        new IntVar("Drown", 1),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<DrownPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SealElementMarkCmd.GainElementMarks(
            choiceContext, MarkSlot.Primary, Owner, DynamicVars["ElementMark"].IntValue);

        var combatState = Owner.Creature.CombatState;
        if (combatState == null) return;

        foreach (var enemy in combatState.HittableEnemies.Where(enemy => enemy.IsAlive))
        {
            await PowerCmd.Apply<DrownPower>(
                choiceContext, enemy, DynamicVars["Drown"].IntValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ElementMark"].UpgradeValueBy(2);
    }
}
