using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using STS2_Starborn.Cards.Kibo;

namespace STS2_Starborn.Hooks;

public static class KiboHooks
{
    // ── Card auto-play ──

    public static bool AnyListenerPreventsKiboAutoPlay(ICombatState combatState, CardModel card)
    {
        foreach (var model in combatState.IterateHookListeners())
        {
            if (model is IKiboCardPlayListener listener
                && listener.ShouldPreventKiboAutoPlay(card))
                return true;
        }
        return false;
    }

    public static async Task BeforeKiboCardAutoPlay(ICombatState combatState, CardModel card)
    {
        foreach (var model in combatState.IterateHookListeners())
        {
            if (model is IKiboCardPlayListener listener)
                await listener.BeforeKiboCardAutoPlayed(card);
            model.InvokeExecutionFinished();
        }
    }

    public static async Task AfterKiboCardAutoPlay(ICombatState combatState, CardModel card)
    {
        foreach (var model in combatState.IterateHookListeners())
        {
            if (model is IKiboCardPlayListener listener)
                await listener.AfterKiboCardAutoPlayed(card);
            model.InvokeExecutionFinished();
        }
    }

    // ── Random auto-play ──

    public static async Task BeforeKiboRandomAutoPlay(ICombatState combatState, CardModel card, string keywordId)
    {
        foreach (var model in combatState.IterateHookListeners())
        {
            if (model is IKiboCardPlayListener listener)
                await listener.BeforeKiboRandomAutoPlay(card, keywordId);
            model.InvokeExecutionFinished();
        }
    }

    public static async Task AfterKiboRandomAutoPlay(ICombatState combatState, CardModel card, string keywordId)
    {
        foreach (var model in combatState.IterateHookListeners())
        {
            if (model is IKiboCardPlayListener listener)
                await listener.AfterKiboRandomAutoPlay(card, keywordId);
            model.InvokeExecutionFinished();
        }
    }

    // ── Kibo switch ──

    public static bool AnyListenerPreventsKiboSwitch(ICombatState combatState,
        Player player, KiboTypeId from, KiboTypeId to)
    {
        foreach (var model in combatState.IterateHookListeners())
        {
            if (model is IKiboSwitchListener listener
                && listener.ShouldPreventKiboSwitch(player, from, to))
                return true;
        }
        return false;
    }

    public static async Task BeforeKiboSwitch(ICombatState combatState,
        Player player, KiboTypeId from, KiboTypeId to)
    {
        foreach (var model in combatState.IterateHookListeners())
        {
            if (model is IKiboSwitchListener listener)
                await listener.BeforeKiboSwitch(player, from, to);
            model.InvokeExecutionFinished();
        }
    }

    public static async Task AfterKiboSwitch(ICombatState combatState,
        Player player, KiboTypeId from, KiboTypeId to)
    {
        foreach (var model in combatState.IterateHookListeners())
        {
            if (model is IKiboSwitchListener listener)
                await listener.AfterKiboSwitch(player, from, to);
            model.InvokeExecutionFinished();
        }
    }
}
