using STS2_Starborn.Combat;

namespace STS2_Starborn.Hooks;

public interface IElementMarkModifier
{
    // ── Consume ──

    int ModifyTuningConsume(MarkSlot slot, int consume) => consume;
    int ModifyOverloadConsume(MarkSlot slot, int consume) => consume;
    int ModifyTuningConsumeAdditive(MarkSlot slot, int consume) => 0;
    int ModifyOverloadConsumeAdditive(MarkSlot slot, int consume) => 0;

    // ── Effective stacks ──

    int ModifyEffectiveStacks(MarkSlot slot, int stacks) => stacks;
}
