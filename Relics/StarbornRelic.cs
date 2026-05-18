using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;


namespace STS2_Starborn.Relics;

/// <summary>
/// 遗物基类
/// </summary>
public abstract class StarbornRelic : ModRelicTemplate
{
    public override RelicAssetProfile AssetProfile => new(
        IconPath: Const.Paths.RelicIcon(GetType()),
        IconOutlinePath: Const.Paths.RelicIcon(GetType()),
        BigIconPath: Const.Paths.RelicIcon(GetType())
    );

}
