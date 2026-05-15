using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Starborn.Powers;

[RegisterPower]
public class SecondaryMarkPower : SealMarkPower
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Starborn/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://Starborn/images/powers/{GetType().Name}.png"
    );
}
