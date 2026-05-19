using STS2_Starborn.Powers;

namespace STS2_Starborn.Hooks;

/// <summary>
/// 实现此接口的模型可修改印记调谐/超限的消耗值。
/// 对标原版 ModifyDamage / ModifyOrbValue 模式。
/// </summary>
public interface IConsumeModifier
{
    /// <summary>Phase 1 (Absolute): 替换消耗值，对标原版 <c>ModifyDamage</c> 的初始值设定。</summary>
    int ModifyTuningConsume(SealElementMarkPower mark, int consume) => consume;
    int ModifyOverloadConsume(SealElementMarkPower mark, int consume) => consume;

    /// <summary>Phase 2 (Additive): 增减消耗值，对标原版 <c>ModifyDamageAdditive</c> 的 delta 语义。</summary>
    int ModifyTuningConsumeAdditive(SealElementMarkPower mark, int consume) => 0;
    int ModifyOverloadConsumeAdditive(SealElementMarkPower mark, int consume) => 0;
}
