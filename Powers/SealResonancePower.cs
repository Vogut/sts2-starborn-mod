using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Hooks;

namespace STS2_Starborn.Powers;

/// <summary>
/// 调谐印刻：调谐/超限的印记消耗-1，且回合开始时印记属性不再重置为无属性。
/// 作为 Buff 存在，可被移除。
/// </summary>
[RegisterPower]
public class SealResonancePower : StarbornPower, IConsumeModifier, ISealElementMarkListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public int ModifyTuningConsume(SealElementMarkPower mark, int consume) => consume - 1;
    public int ModifyOverloadConsume(SealElementMarkPower mark, int consume) => consume - 1;

    public bool ShouldPreventElementChange(SealElementMarkPower mark, SealElementType from, SealElementType to)
        => to == SealElementType.None; // 仅阻止重置为无属性，不阻止主动切换
}
