using System.Collections.Generic;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Element;
/// <summary>
/// 火属性效果提供者。
/// 调谐：对全体敌人施加等同于印记层数的灼烧。
/// 超限：对全体敌人施加等同于2倍印记层数的灼烧。
/// </summary>
public sealed class FireElement : StarbornElement
{
    public override SealElementType Attribute => SealElementType.Fire;

    public override LocString ElementDescription =>
        new LocString("powers", "STS2_STARBORN_ELEMENT_FIRE.description");

    public override IEnumerable<PowerModel> AssociatedPowers => [ModelDb.Power<BurnPower>()];

    public override async Task OnThreshold(PlayerChoiceContext ctx, Player owner, int stacks) =>
        await PowerCmd.Apply<BurnPower>(ctx,
            owner.Creature.CombatState!.HittableEnemies, stacks, owner.Creature, null);

    public override async Task OnEnhanced(PlayerChoiceContext ctx, Player owner, int stacks) =>
        await PowerCmd.Apply<BurnPower>(ctx,
            owner.Creature.CombatState!.HittableEnemies, stacks * 2, owner.Creature, null);
}
