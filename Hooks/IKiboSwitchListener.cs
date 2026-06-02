using MegaCrit.Sts2.Core.Entities.Players;
using STS2_Starborn.Cards.Kibo;

namespace STS2_Starborn.Hooks;

public interface IKiboSwitchListener
{
    bool ShouldPreventKiboSwitch(Player player, string from, string to) => false;
    Task BeforeKiboSwitch(Player player, string from, string to) => Task.CompletedTask;
    Task AfterKiboSwitch(Player player, string from, string to) => Task.CompletedTask;
    /// <summary>是否阻止换下指定奇波。</summary>
    bool ShouldPreventKiboSwitchOff(Player player, string typeId) => false;

    /// <summary>换下前触发。</summary>
    Task BeforeKiboSwitchOff(Player player, string typeId) => Task.CompletedTask;

    /// <summary>换下后触发。</summary>
    Task AfterKiboSwitchOff(Player player, string typeId) => Task.CompletedTask;

    /// <summary>是否阻止换上指定奇波。</summary>
    bool ShouldPreventKiboSwitchOn(Player player, string typeId) => false;

    /// <summary>换上前触发。</summary>
    Task BeforeKiboSwitchOn(Player player, string typeId) => Task.CompletedTask;

    /// <summary>换上后触发。</summary>
    Task AfterKiboSwitchOn(Player player, string typeId) => Task.CompletedTask;

}
