using MegaCrit.Sts2.Core.Entities.Players;
using STS2_Starborn.Cards.Kibo;

namespace STS2_Starborn.Hooks;

public interface IKiboSwitchListener
{
    bool ShouldPreventKiboSwitch(Player player, KiboTypeId from, KiboTypeId to) => false;
    Task BeforeKiboSwitch(Player player, KiboTypeId from, KiboTypeId to) => Task.CompletedTask;
    Task AfterKiboSwitch(Player player, KiboTypeId from, KiboTypeId to) => Task.CompletedTask;
}
