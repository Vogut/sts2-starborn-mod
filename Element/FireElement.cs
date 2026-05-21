using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.ValueProps;

namespace STS2_Starborn.Element;
/// <summary>
/// 火属性效果提供者。
/// 基础效果：对全体敌人造成 3 点伤害；强化效果：造成 10 点伤害。
/// </summary>
public sealed class FireElement : Element
{
    public override SealElementType Attribute => SealElementType.Fire;

    public override LocString ElementDescription =>
        new LocString("powers", "STS2_STARBORN_ELEMENT_FIRE.description");

    public override async Task OnThreshold(PlayerChoiceContext ctx, Player owner) =>
        await CreatureCmd.Damage(ctx, owner.Creature.CombatState!.HittableEnemies, 3m, ValueProp.Unpowered, owner.Creature, null);

    public override async Task OnEnhanced(PlayerChoiceContext ctx, Player owner) =>
        await CreatureCmd.Damage(ctx, owner.Creature.CombatState!.HittableEnemies, 10m, ValueProp.Unpowered, owner.Creature, null);
}
