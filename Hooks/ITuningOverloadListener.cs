using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2_Starborn.Combat;

namespace STS2_Starborn.Hooks;

public interface ITuningOverloadListener
{
    Task BeforeTuning(PlayerChoiceContext ctx, MarkSlot slot, int consume, CardModel? source)
        => Task.CompletedTask;

    Task AfterTuning(PlayerChoiceContext ctx, MarkSlot slot, int consume, CardModel? source)
        => Task.CompletedTask;

    Task BeforeOverload(PlayerChoiceContext ctx, MarkSlot slot, int consume, CardModel? source)
        => Task.CompletedTask;

    Task AfterOverload(PlayerChoiceContext ctx, MarkSlot slot, int consume, CardModel? source)
        => Task.CompletedTask;
}
