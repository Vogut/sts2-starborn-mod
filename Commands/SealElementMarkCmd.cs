using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2_Starborn.Hooks;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Commands;

/// <summary>
/// 印记专属命令入口，负责属性切换与层数增减操作。
/// 通用操作（调谐、超限）见 <see cref="StarbornCmd"/>。
/// </summary>
public static class SealElementMarkCmd
{
    /// <summary>
    /// 将印记的 <see cref="SealElementMarkPower.CurrentElementType"/> 切换为 <paramref name="dst_element"/>，
    /// 并通过 <see cref="SealElementMarkHooks.AfterElementChanged"/> 通知所有 <see cref="ISealElementMarkListener"/> 监听者。
    /// 新旧属性相同时直接返回，不触发 Hook。
    /// </summary>
    public static async Task SetElementType(
        PlayerChoiceContext ctx,
        SealElementMarkPower element_mark,
        SealElementType dst_element)
    {
        var oldElement = element_mark.CurrentElementType;
        if (oldElement == dst_element)
            return;

        element_mark.CurrentElementType = dst_element;
        await SealElementMarkHooks.AfterElementChanged(element_mark.CombatState, ctx, element_mark, oldElement, dst_element);
    }

    /// <summary>
    /// 为印记叠加 <paramref name="stacks"/> 层，委托给 <see cref="PowerCmd.Apply(PowerModel, Creature, decimal, Creature?, CardModel?)"/>
    /// 处理层数同步。
    /// </summary>
    public static async Task GainElementMarks(
        PlayerChoiceContext ctx,
        SealElementMarkPower mark,
        int stacks,
        CardModel? cardSource = null)
    {
        if (stacks <= 0) return;
        var owner = mark.Owner;
        await PowerCmd.Apply(ctx, mark, owner, stacks, owner, cardSource);
    }

    /// <summary>
    /// 移除印记的指定层数，委托给 <see cref="PowerCmd.Apply"/> 处理层数同步。
    /// </summary>
    public static async Task RemoveElementMarks(
        PlayerChoiceContext ctx,
        SealElementMarkPower mark,
        int stacks,
        CardModel? cardSource = null)
    {
        if (stacks <= 0) return;
        var owner = mark.Owner;
        await PowerCmd.Apply(ctx, mark, owner, -stacks, owner, cardSource);
    }
}
