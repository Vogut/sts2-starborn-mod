using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2_Starborn.Combat;

namespace STS2_Starborn.Element;

public sealed class WoodElement : StarbornElement
{
    public override SealElementType Attribute => SealElementType.Wood;

    public override LocString ElementDescription =>
        new LocString("powers", "STS2_STARBORN_ELEMENT_WOOD.description");

    public override async Task OnThreshold(PlayerChoiceContext ctx, Player owner, int stacks) =>
        await CreatureCmd.GainBlock(owner.Creature, new BlockVar(stacks, default), null);

    public override async Task OnEnhanced(PlayerChoiceContext ctx, Player owner, int stacks)
    {
        await CreatureCmd.GainBlock(owner.Creature, new BlockVar(stacks * 2, default), null);
        if (ElementMarkManager.IsFirstOverload(SealElementType.Wood))
            await CreatureCmd.Heal(owner.Creature, stacks);
    }
}
