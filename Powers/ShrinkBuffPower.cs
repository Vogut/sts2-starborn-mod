using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_Starborn.Powers;

/// <summary>
/// 变小 Buff：可堆叠，每层让目标体型缩小 10%，下限 20%。
/// 参考原版 ShrinkPower 的 ScaleTo 实现。
/// </summary>
[RegisterPower]
public class ShrinkBuffPower : StarbornPower
{
    private const float ScalePerStack = 0.1f;
    private const float MinScale = 0.2f;
    private const float Duration = 0.75f;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: Const.Paths.PowerIcon(GetType()),
        BigIconPath: Const.Paths.PowerBigIcon(GetType())
    );

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        ApplyScale();
        return Task.CompletedTask;
    }

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power != this) return;
        ApplyScale();
        await Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        NCombatRoom.Instance?.GetCreatureNode(oldOwner)?.ScaleTo(1f, Duration);
        return Task.CompletedTask;
    }

    private void ApplyScale()
    {
        var scale = Math.Max(MinScale, 1f - ScalePerStack * (float)Amount);
        NCombatRoom.Instance?.GetCreatureNode(Owner)?.ScaleTo(scale, Duration);
    }
}
