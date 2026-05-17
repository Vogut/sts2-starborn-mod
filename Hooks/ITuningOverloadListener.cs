using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Hooks;

/// <summary>
/// 实现此接口的模型（遗物、能力、卡牌等）可在调谐/超限生命周期中收到通知。
/// 所有方法均有默认空实现，实现者按需覆写。
/// </summary>
public interface ITuningOverloadListener
{
    /// <summary>调谐消耗印记前调用。</summary>
    Task BeforeTuning(PlayerChoiceContext ctx, SealElementMarkPower mark, int consume, Creature owner, CardModel? source)
        => Task.CompletedTask;

    /// <summary>调谐效果完成后调用。</summary>
    Task AfterTuning(PlayerChoiceContext ctx, SealElementMarkPower mark, int consume, Creature owner, CardModel? source)
        => Task.CompletedTask;

    /// <summary>超限消耗印记前调用。此时已验证层数 >= ThresholdStacks。</summary>
    Task BeforeOverload(PlayerChoiceContext ctx, SealElementMarkPower mark, int consume, Creature owner, CardModel? source)
        => Task.CompletedTask;

    /// <summary>超限效果完成后调用。</summary>
    Task AfterOverload(PlayerChoiceContext ctx, SealElementMarkPower mark, int consume, Creature owner, CardModel? source)
        => Task.CompletedTask;
}
