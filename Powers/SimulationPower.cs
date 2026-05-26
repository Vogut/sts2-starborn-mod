using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_Starborn.Powers;

/// <summary>
/// 节能：所有牌耗能减少 Amount 点（最低 0）。参照 CuriousPower 实现。
/// </summary>
[RegisterPower]
public class SimulationPower : StarbornPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: Const.Paths.PowerIcon(GetType()),
        BigIconPath: Const.Paths.PowerBigIcon(GetType())
    );

    public override bool TryModifyEnergyCostInCombat(
        CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card.Owner.Creature != Owner)
            return false;
        if (originalCost <= 0m)
            return false;
        modifiedCost = originalCost - Amount;
        if (modifiedCost < 0m)
            modifiedCost = 0m;
        return true;
    }
}
