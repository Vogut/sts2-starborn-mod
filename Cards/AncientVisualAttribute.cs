using System;

namespace STS2_Starborn.Cards;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class AncientVisualAttribute : Attribute
{
    public float TextBgAlpha { get; set; } = 1f;

    public bool HideTypePlaque { get; set; }
}
