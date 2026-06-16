using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Combat;
using STS2_Starborn.Element;
using STS2_Starborn.Hooks;

namespace STS2_Starborn.Powers;

/// <summary>
/// 变换自在：每当你切换属性时，获得格挡。
/// </summary>
[RegisterPower]
public class ProteanPower : StarbornPower, ISealElementMarkListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: Const.Paths.PowerIcon(GetType()),
        BigIconPath: Const.Paths.PowerBigIcon(GetType())
    );

    public async Task AfterElementChanged(PlayerChoiceContext ctx, MarkSlot slot,
        SealElementType from, SealElementType to)
    {
        // 只有真正切换了属性才触发（不是切换到相同属性）
        if (from == to) return;

        Flash();

        // 获得格挡
        await CreatureCmd.GainBlock(Owner, new BlockVar(Amount, default), null);
    }
}
