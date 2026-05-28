using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace STS2_Starborn.Element;

public abstract class StarbornElement
{
    private static readonly Dictionary<SealElementType, StarbornElement> _elements = new()
    {
        [SealElementType.None]  = new NonElement(),
        [SealElementType.Fire]  = new FireElement(),
        [SealElementType.Water] = new WaterElement(),
        [SealElementType.Wood]  = new WoodElement(),
        [SealElementType.Ice]   = new IceElement(),
        [SealElementType.Wind]  = new WindElement(),
        [SealElementType.Light] = new LightElement(),
    };

    public abstract SealElementType Attribute { get; }
    public abstract LocString ElementDescription { get; }
    public virtual int TuningConsume => 1;
    public virtual int OverloadConsume => 2;
    public virtual IEnumerable<PowerModel> AssociatedPowers => [];
    public abstract Task OnThreshold(PlayerChoiceContext ctx, Player owner, int stacks);
    public abstract Task OnEnhanced(PlayerChoiceContext ctx, Player owner, int stacks);

    public static StarbornElement For(SealElementType attribute) =>
        _elements.TryGetValue(attribute, out var ep) ? ep : _elements[SealElementType.None];
}
