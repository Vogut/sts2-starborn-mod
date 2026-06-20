using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Patching.Models;

namespace STS2_Starborn.Patches;

public sealed class IndomitableHeartAttackInterruptPatch : IPatchMethod
{
    private sealed class InterruptMarker;

    private static readonly ConditionalWeakTable<AttackCommand, InterruptMarker> InterruptedAttacks = new();
    private static readonly ConditionalWeakTable<Creature, ActiveAttackState> ActiveAttacks = new();

    public static string PatchId => "sts2_starborn_indomitable_heart_attack_interrupt";
    public static string Description => "Stops remaining AttackCommand hits after Indomitable Heart stuns the attacker";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return [new(typeof(AttackCommand), "GetPossibleTargets")];
    }

    internal static void Track(AttackCommand attack)
    {
        if (attack.Attacker == null)
            return;

        ActiveAttacks.GetValue(attack.Attacker, _ => new ActiveAttackState()).Attack = attack;
    }

    internal static void Clear(AttackCommand attack)
    {
        InterruptedAttacks.Remove(attack);

        if (attack.Attacker == null)
            return;

        if (ActiveAttacks.TryGetValue(attack.Attacker, out var state) && state.Attack == attack)
            state.Attack = null;
    }

    public static void InterruptCurrentAttack(Creature dealer)
    {
        if (ActiveAttacks.TryGetValue(dealer, out var state) && state.Attack != null)
            Interrupt(state.Attack);
    }

    private static void Interrupt(AttackCommand attack)
    {
        InterruptedAttacks.GetValue(attack, _ => new InterruptMarker());
    }

    public static bool Prefix(AttackCommand __instance, ref IReadOnlyList<Creature> __result)
    {
        if (!InterruptedAttacks.TryGetValue(__instance, out _))
            return true;

        __result = [];
        return false;
    }

    private sealed class ActiveAttackState
    {
        public AttackCommand? Attack { get; set; }
    }
}

public sealed class IndomitableHeartAttackTrackingPatch : IPatchMethod
{
    public static string PatchId => "sts2_starborn_indomitable_heart_attack_tracking";
    public static string Description => "Tracks the active AttackCommand for Indomitable Heart interruption";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return [new(typeof(AttackCommand), nameof(AttackCommand.Execute), [typeof(PlayerChoiceContext)])];
    }

    public static void Prefix(AttackCommand __instance)
    {
        IndomitableHeartAttackInterruptPatch.Track(__instance);
    }

    public static void Postfix(AttackCommand __instance, Task<AttackCommand>? __result)
    {
        _ = ClearWhenComplete(__instance, __result);
    }

    private static async Task ClearWhenComplete(AttackCommand attack, Task<AttackCommand>? result)
    {
        try
        {
            if (result != null)
                await result.ConfigureAwait(false);
        }
        catch
        {
            // The original task is still returned to the game; this observer only cleans tracking state.
        }
        finally
        {
            IndomitableHeartAttackInterruptPatch.Clear(attack);
        }
    }
}
