using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2_Starborn.Combat;
using STS2_Starborn.Hooks;
using STS2_Starborn.Element;

namespace STS2_Starborn.Commands;

/// <summary>
/// 印记专属命令入口，负责属性切换与层数增减操作。
/// 通用操作（调谐、超限）见 <see cref="StarbornCmd"/>。
/// </summary>
public static class SealElementMarkCmd
{
    public static async Task SetElementType(
        PlayerChoiceContext ctx,
        MarkSlot slot,
        Player player,
        SealElementType dstElement)
    {
        var combatState = player.Creature.CombatState;
        if (combatState == null) return;

        var oldElement = ElementMarkState.GetElementType(player, slot);
        if (oldElement == dstElement) return;

        if (SealElementMarkHooks.AnyListenerPreventsElementChange(combatState, slot, oldElement, dstElement))
            return;

        ElementMarkState.SetElementType(player, slot, dstElement);
        if (dstElement != SealElementType.None)
            ElementMarkManager.RecordSwitch(player, dstElement);
        await SealElementMarkHooks.AfterElementChanged(combatState, ctx, slot, oldElement, dstElement);
    }

    /// <summary>
    /// 为印记叠加 <paramref name="stacks"/> 层，委托给 <see cref="PowerCmd.Apply(PowerModel, Creature, decimal, Creature?, CardModel?)"/>
    /// 处理层数同步。
    /// </summary>
    public static async Task GainElementMarks(
        PlayerChoiceContext ctx,
        MarkSlot slot,
        Player player,
        int stacks)
    {
        if (stacks <= 0) return;
        var current = ElementMarkState.GetStacks(player, slot);
        ElementMarkState.SetStacks(player, slot, current + stacks);
    }

    /// <summary>
    /// 移除印记的指定层数，委托给 <see cref="PowerCmd.Apply"/> 处理层数同步。
    /// </summary>
    public static async Task RemoveElementMarks(
        PlayerChoiceContext ctx,
        MarkSlot slot,
        Player player,
        int stacks)
    {
        if (stacks <= 0) return;
        var current = ElementMarkState.GetStacks(player, slot);
        ElementMarkState.SetStacks(player, slot, current - stacks);
    }
}
