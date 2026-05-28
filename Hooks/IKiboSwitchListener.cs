using MegaCrit.Sts2.Core.Entities.Players;
using STS2_Starborn.Cards.Kibo;

namespace STS2_Starborn.Hooks;

public interface IKiboSwitchListener
{
    bool ShouldPreventKiboSwitch(Player player, KiboTypeId from, KiboTypeId to) => false;
    Task BeforeKiboSwitch(Player player, KiboTypeId from, KiboTypeId to) => Task.CompletedTask;
    Task AfterKiboSwitch(Player player, KiboTypeId from, KiboTypeId to) => Task.CompletedTask;
    /// <summary>是否阻止换下指定奇波。</summary>
    bool ShouldPreventKiboSwitchOff(Player player, KiboTypeId typeId) => false;

    /// <summary>换下前触发。</summary>
    Task BeforeKiboSwitchOff(Player player, KiboTypeId typeId) => Task.CompletedTask;

    /// <summary>换下后触发。</summary>
    Task AfterKiboSwitchOff(Player player, KiboTypeId typeId) => Task.CompletedTask;

    /// <summary>是否阻止换上指定奇波。</summary>
    bool ShouldPreventKiboSwitchOn(Player player, KiboTypeId typeId) => false;

    /// <summary>换上前触发。</summary>
    Task BeforeKiboSwitchOn(Player player, KiboTypeId typeId) => Task.CompletedTask;

    /// <summary>换上后触发。</summary>
    Task AfterKiboSwitchOn(Player player, KiboTypeId typeId) => Task.CompletedTask;

}
