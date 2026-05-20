using Godot;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_Starborn.Cards.Kibo;

[RegisterSharedCardPool]
public sealed class KiboCardPool : TypeListCardPoolModel
{
    public override string Title => "kibo";
    public override string EnergyColorName => "kibo";
    public override string? TextEnergyIconPath => Const.Paths.EnergySmall;
    public override string? BigEnergyIconPath => Const.Paths.EnergyBig;
    public override Color DeckEntryCardColor => new(1f, 0.6f, 0.2f);
    public override Color EnergyOutlineColor => new(1f, 0.6f, 0.2f);
    public override bool IsColorless => false;
}
