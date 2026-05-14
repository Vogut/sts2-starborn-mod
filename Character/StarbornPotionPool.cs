using STS2RitsuLib.Scaffolding.Content;

namespace Starborn.Character;

public class StarbornPotionPool : TypeListPotionPoolModel
{
    // 描述中使用的能量图标。大小为24x24。
    public override string? TextEnergyIconPath => "res://Starborn/starborn/energy/energy_starborn.png";
    // tooltip和卡牌左上角的能量图标。大小为74x74。
    public override string? BigEnergyIconPath => "res://Starborn/starborn/energy/energy_starborn_big.png";

    public override string EnergyColorName => "starborn";
}