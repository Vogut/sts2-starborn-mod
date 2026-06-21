using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Combat;
using STS2_Starborn.Hooks;

namespace STS2_Starborn.Powers;

/// <summary>
/// 律动：每次进行调谐或超限后，对随机一名敌人造成伤害。
/// </summary>
public class RhythmPower : StarbornPower, ITuningOverloadListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: Const.Paths.PowerIcon(GetType()),
        BigIconPath: Const.Paths.PowerBigIcon(GetType())
    );

    public async Task AfterTuning(PlayerChoiceContext ctx, MarkSlot slot, int consume, CardModel? source)
    {
        await DealDamageToRandomEnemy(ctx);
    }

    public async Task AfterOverload(PlayerChoiceContext ctx, MarkSlot slot, int consume, CardModel? source)
    {
        await DealDamageToRandomEnemy(ctx);
    }

    private async Task DealDamageToRandomEnemy(PlayerChoiceContext ctx)
    {
        var combatState = Owner.CombatState!;
        var enemies = combatState.HittableEnemies.ToList();
        if (enemies.Count == 0) return;

        var player = combatState.Players.FirstOrDefault(p => p.Creature == Owner);
        if (player == null) return;

        var target = player.RunState.Rng.CombatTargets.NextItem(enemies);
        if (target == null) return;
        Flash();
        await CreatureCmd.Damage(ctx, target, Amount, ValueProp.Unpowered, Owner, null);
    }
}
