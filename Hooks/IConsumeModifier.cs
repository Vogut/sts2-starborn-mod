using STS2_Starborn.Combat;

namespace STS2_Starborn.Hooks;

/// <summary>
/// 实现此接口的模型可修改印记调谐/超限的消耗值。
/// 对标原版 ModifyDamage / ModifyOrbValue 模式。
/// </summary>
public interface IConsumeModifier
{
    int ModifyTuningConsume(MarkSlot slot, int consume) => consume;
    int ModifyOverloadConsume(MarkSlot slot, int consume) => consume;
    int ModifyTuningConsumeAdditive(MarkSlot slot, int consume) => 0;
    int ModifyOverloadConsumeAdditive(MarkSlot slot, int consume) => 0;
}
