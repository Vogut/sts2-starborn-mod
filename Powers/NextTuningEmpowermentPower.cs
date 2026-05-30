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
/// 下一次谐调/超限时获得额外有效层数，生效后消失。
/// </summary>
[RegisterPower]
public class NextTuningEmpowermentPower : StarbornPower, IElementMarkModifier, ITuningOverloadListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: Const.Paths.PowerIcon(GetType()),
        BigIconPath: Const.Paths.PowerBigIcon(GetType())
    );

    public int ModifyEffectiveStacks(MarkSlot slot, int stacks) => stacks + Amount;

    public async Task AfterTuning(
        PlayerChoiceContext ctx, MarkSlot slot, int consume, CardModel? source)
    {
        await PowerCmd.Remove(this);
    }

    public async Task AfterOverload(
        PlayerChoiceContext ctx, MarkSlot slot, int consume, CardModel? source)
    {
        await PowerCmd.Remove(this);
    }
}
