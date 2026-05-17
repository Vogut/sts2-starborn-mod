using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2_Starborn.Hooks;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Commands;

/// <summary>
/// 印记系统的统一命令入口，仿 ForgeCmd/OstyCmd 模式集中管理印记属性切换与层数叠加。
/// </summary>
public static class SealMarkCmd
{
    /// <summary>
    /// 将印记的 <see cref="SealElementMarkPower.CurrentElementType"/> 切换为 <paramref name="element"/>，
    /// 并通过 <see cref="SealMarkHooks.BeforeElementChanged"/> 和 <see cref="SealMarkHooks.AfterElementChanged"/> 通知所有 <see cref="ISealElementMarkListener"/> 监听者。
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
 
        await SealMarkHooks.AfterElementChanged(element_mark.CombatState, ctx, element_mark, oldElement, dst_element);
    }

    /// <summary>
    /// 为印记叠加 <paramref name="stacks"/> 层，委托给 <see cref="PowerCmd.Apply{TPower}"/> 处理层数同步。
    /// </summary>
    /// <typeparam name="TPower">具体的印记能力类型，需继承 <see cref="SealElementMarkPower"/></typeparam>
    public static async Task GainElementMarks<TPower>(
        PlayerChoiceContext ctx,
        SealElementMarkPower mark,
        int stacks,
        Creature owner,
        CardModel? cardSource)
        where TPower : SealElementMarkPower
    {
        if (stacks <= 0)
            return;

        await PowerCmd.Apply<TPower>(ctx, owner, stacks, owner, cardSource);
    }

    /// <summary>
    /// 移除印记的指定层数，委托给 <see cref="PowerCmd.Apply{TPower}"/> 处理层数同步。
    /// </summary>
    /// <typeparam name="TPower">具体的印记能力类型，需继承 <see cref="SealElementMarkPower"/></typeparam>
    public static async Task RemoveElementMarks<TPower>(
        PlayerChoiceContext ctx,
        SealElementMarkPower mark,
        int stacks,
        Creature owner,
        CardModel? cardSource)
        where TPower : SealElementMarkPower
    {
        if (stacks <= 0)
            return;

        await PowerCmd.Apply<TPower>(ctx, owner, -stacks, owner, cardSource);
    }    
}
