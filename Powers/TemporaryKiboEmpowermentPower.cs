using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_Starborn.Powers;

[RegisterPower]
public class TemporaryKiboEmpowermentPower : ModTemporaryPowerTemplate
{
    public override AbstractModel OriginModel => ModelDb.Power<KiboEmpowermentPower>();
    public override PowerModel InternallyAppliedPower => ModelDb.Power<KiboEmpowermentPower>();

    public override PowerAssetProfile AssetProfile => new(
        IconPath: Const.Paths.PowerIcon(GetType()),
        BigIconPath: Const.Paths.PowerBigIcon(GetType())
    );
}
