using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2_Starborn.Powers;
using STS2RitsuLib.Patching.Models;

namespace STS2_Starborn.Patches;

public class BurnPowerBloodBarRefreshForegroundPatch : IPatchMethod
{
    public static string PatchId => "sts2_starborn_burn_blood_bar_RefreshForeground";
    public static string Description => "Show burn damage prediction on creature health bars";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return [new(typeof(NHealthBar), "RefreshForeground", [])];
    }

    public static void Postfix(NHealthBar __instance)
    {
        var creature = Traverse.Create(__instance).Field("_creature").GetValue<Creature>();
        if (creature?.CurrentHp <= 0) return;
        var burnPower = creature!.GetPower<BurnPower>();
        if (burnPower == null) return;
        int burnDmg = burnPower.CalculateTotalDamageNextTurn();
        if (burnDmg <= 0) return;

        var traverse = Traverse.Create(__instance);
        var burnFg = traverse.Field("_poisonForeground").GetValue<Control>();
        var hpFg = traverse.Field("_hpForeground").GetValue<Control>();
        var maxFgWidth = traverse.Property("MaxFgWidth").GetValue<float>();
        float offsetRight = GetFgWidth(__instance, creature.CurrentHp, maxFgWidth) - maxFgWidth;

        if (burnDmg >= creature.CurrentHp)
        {
            burnFg.Visible = true;
            burnFg.OffsetLeft = 0f;
            burnFg.OffsetRight = offsetRight;
            burnFg.SelfModulate = new Color("#FF4500");
            hpFg.Visible = false;
        }
        else
        {
            burnFg.Visible = true;
            float fgWidth = GetFgWidth(__instance, creature.CurrentHp - burnDmg, maxFgWidth);
            hpFg.OffsetRight = fgWidth - maxFgWidth;
            hpFg.Visible = true;
            int patchMarginLeft = ((NinePatchRect)burnFg).PatchMarginLeft;
            burnFg.OffsetLeft = Math.Max(0f, fgWidth - patchMarginLeft);
            burnFg.OffsetRight = offsetRight;
            burnFg.SelfModulate = new Color("#FF4500");
        }
    }

    private static float GetFgWidth(NHealthBar instance, int amount, float maxFgWidth)
    {
        var creature = Traverse.Create(instance).Field("_creature").GetValue<Creature>();
        if (creature!.MaxHp <= 0) return 0f;
        float val = (float)amount / creature.MaxHp * maxFgWidth;
        return Math.Max(val, creature.CurrentHp > 0 ? 12f : 0f);
    }
}

public class BurnPowerBloodBarRefreshTextPatch : IPatchMethod
{
    public static string PatchId => "sts2_starborn_burn_blood_bar_RefreshText";
    public static string Description => "Show burn damage prediction on creature health bars";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return [new(typeof(NHealthBar), "RefreshText", [])];
    }

    public static void Postfix(NHealthBar __instance)
    {
        var traverse = Traverse.Create(__instance);
        var creature = traverse.Field("_creature").GetValue<Creature>();
        if (creature == null || creature.CurrentHp <= 0 || !creature.HpDisplay.ShowsNumbers()) return;
        var burnPower = creature.GetPower<BurnPower>();
        if (burnPower == null) return;
        int burnDmg = burnPower.CalculateTotalDamageNextTurn();
        if (burnDmg <= 0 || burnDmg < creature.CurrentHp) return;

        var hpLabel = traverse.Field("_hpLabel").GetValue<MegaCrit.Sts2.addons.mega_text.MegaLabel>();
        hpLabel.AddThemeColorOverride("font_color", new Color("#FF6347"));
        hpLabel.AddThemeColorOverride("font_outline_color", new Color("#8B0000"));
    }
}
