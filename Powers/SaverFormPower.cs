using Godot;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Combat;
using STS2_Starborn.Element;
using STS2_Starborn.Hooks;

namespace STS2_Starborn.Powers;

/// <summary>
/// 救世模式：调谐/超限的印记消耗-1，且回合开始时印记属性不再重置为无属性。
/// 作为 Buff 存在，可被移除。
/// </summary>
[RegisterPower]
public class SaverFormPower : StarbornPower, IConsumeModifier, ISealElementMarkListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;
    public override PowerAssetProfile AssetProfile => new(
        IconPath: Const.Paths.PowerIcon(GetType()),
        BigIconPath: Const.Paths.PowerBigIcon(GetType())
    );

    public override string? CustomBigIconPath =>
        ResourceLoader.Exists(AssetProfile.BigIconPath) ? AssetProfile.BigIconPath : CustomIconPath;

    public int ModifyTuningConsumeAdditive(MarkSlot slot, int consume) => -1;
    public int ModifyOverloadConsumeAdditive(MarkSlot slot, int consume) => -1;

    public bool ShouldPreventElementChange(MarkSlot slot, SealElementType from, SealElementType to)
        => to == SealElementType.None;
}
