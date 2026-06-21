using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Element;
/// <summary>
/// 火属性效果提供者。
/// 调谐：施加等同于印记层数的灼烧。
///   卡牌触发时跟随卡牌目标；自动触发时对随机单个敌人施加。
/// 超限：施加等同于2倍印记层数的灼烧。
///   卡牌触发时跟随卡牌目标；自动触发时对随机单个敌人施加。
/// </summary>
public sealed class FireElement : StarbornElement
{
    public override SealElementType Attribute => SealElementType.Fire;

    public override IEnumerable<PowerModel> AssociatedPowers => [ModelDb.Power<BurnPower>()];

    public override async Task OnThreshold(PlayerChoiceContext ctx, Player owner, int stacks, CardModel? source = null, IReadOnlyList<Creature>? targets = null) =>
        await ApplyBurn(ctx, owner, stacks, targets);

    public override async Task OnEnhanced(PlayerChoiceContext ctx, Player owner, int stacks, CardModel? source = null, IReadOnlyList<Creature>? targets = null) =>
        await ApplyBurn(ctx, owner, stacks * 2, targets);

    private static async Task ApplyBurn(PlayerChoiceContext ctx, Player owner, int stacks, IReadOnlyList<Creature>? targets)
    {
        if (targets != null && targets.Count > 0)
        {
            // 卡牌有目标（单体或群体）→ 对每个目标施加
            foreach (var target in targets)
                await PowerCmd.Apply<BurnPower>(ctx, target, stacks, owner.Creature, null);
            return;
        }

        // 无目标（回合开始自动触发、或 TargetType.None 卡牌）：随机选择一个可攻击敌人
        var enemies = owner.Creature.CombatState!.HittableEnemies.ToList();
        if (enemies.Count == 0) return;

        var randomTarget = owner.RunState.Rng.CombatTargets.NextItem(enemies);
        if (randomTarget != null)
            await PowerCmd.Apply<BurnPower>(ctx, randomTarget, stacks, owner.Creature, null);
    }
}
