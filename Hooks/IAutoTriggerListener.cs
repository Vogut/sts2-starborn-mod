using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2_Starborn.Combat;

namespace STS2_Starborn.Hooks;

/// <summary>
/// 监听回合开始时的自动触发流程（检查并触发主副属性印记的调谐/超限）。
/// 钩子在检查所有槽位之前/之后调用，而非每个槽位单独调用。
/// </summary>
public interface IAutoTriggerListener
{
    /// <summary>
    /// 在回合开始检查所有槽位的自动触发之前调用。
    /// </summary>
    Task BeforeAutoTrigger(PlayerChoiceContext ctx)
        => Task.CompletedTask;

    /// <summary>
    /// 在回合开始检查并执行所有槽位的自动触发之后调用。
    /// </summary>
    /// <param name="anyTriggered">是否有任一槽位实际触发了调谐或超限</param>
    Task AfterAutoTrigger(PlayerChoiceContext ctx, bool anyTriggered)
        => Task.CompletedTask;
}
