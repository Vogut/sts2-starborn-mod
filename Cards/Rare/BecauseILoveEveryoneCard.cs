using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using MegaCrit.Sts2.Core.Localization;

namespace STS2_Starborn.Cards.Rare;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class BecauseILoveEveryoneCard() : StarbornCard(
    0, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies
)
{
    protected override bool HasEnergyCostX => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        new HoverTip(
            new LocString("static_hover_tips","STS2_STARBORN_BECAUSE_I_LOVE_EVERYONE_CARD.title"),           // 标题
            new LocString("static_hover_tips","STS2_STARBORN_BECAUSE_I_LOVE_EVERYONE_CARD.description")  // 描述
        ),
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<PoisonPower>(),
        HoverTipFactory.FromPower<DoomPower>(),
        HoverTipFactory.FromPower<DemisePower>(),
        HoverTipFactory.FromPower<StranglePower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int powerAmount = ResolveEnergyXValue();
        if (IsUpgraded)
        {
            powerAmount++;
        }
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        var enemies = CombatState!.HittableEnemies;
        await PowerCmd.Apply<VulnerablePower>(choiceContext, enemies, powerAmount, Owner.Creature, this);
        await PowerCmd.Apply<WeakPower>(choiceContext, enemies, powerAmount, Owner.Creature, this);
        await PowerCmd.Apply<PoisonPower>(choiceContext, enemies, powerAmount, Owner.Creature, this);
        await PowerCmd.Apply<DoomPower>(choiceContext, enemies, powerAmount, Owner.Creature, this);
        await PowerCmd.Apply<DemisePower>(choiceContext, enemies, powerAmount, Owner.Creature, this);
        await PowerCmd.Apply<StranglePower>(choiceContext, enemies, powerAmount, Owner.Creature, this);
    }
}
