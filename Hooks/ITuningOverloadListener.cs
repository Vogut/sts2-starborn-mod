using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Hooks;

public interface ITuningOverloadListener
{
     /// <summary>调谐生命周期。</summary>
    Task BeforeTuning(PlayerChoiceContext ctx, SealElementMarkPower mark, int consume, CardModel? source)
        => Task.CompletedTask;

    Task AfterTuning(PlayerChoiceContext ctx, SealElementMarkPower mark, int consume, CardModel? source)
        => Task.CompletedTask;
    /// <summary>超限生命周期。</summary>
    Task BeforeOverload(PlayerChoiceContext ctx, SealElementMarkPower mark, int consume, CardModel? source)
        => Task.CompletedTask;

    Task AfterOverload(PlayerChoiceContext ctx, SealElementMarkPower mark, int consume, CardModel? source)
        => Task.CompletedTask;
}
