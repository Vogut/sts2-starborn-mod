using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Combat;
using STS2_Starborn.Hooks;

namespace STS2_Starborn.Powers;

/// <summary>
/// 免费调谐：下一次调谐/超限消耗降为 0。
/// </summary>
[RegisterPower]
public class FreeTuningPower : StarbornPower, IElementMarkModifier, ITuningOverloadListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: Const.Paths.PowerIcon(GetType()),
        BigIconPath: Const.Paths.PowerBigIcon(GetType())
    );

    public int ModifyTuningConsumeAdditive(MarkSlot slot, int consume) => -consume;
    public int ModifyOverloadConsumeAdditive(MarkSlot slot, int consume) => -consume;

    public async Task AfterTuning(PlayerChoiceContext ctx, MarkSlot slot, int consume, CardModel? source)
    {
        await PowerCmd.Decrement(this);
    }

    public async Task AfterOverload(PlayerChoiceContext ctx, MarkSlot slot, int consume, CardModel? source)
    {
        await PowerCmd.Decrement(this);
    }
}
