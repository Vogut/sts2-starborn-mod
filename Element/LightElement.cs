using System.Collections.Generic;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Element;

public sealed class LightElement : StarbornElement
{
    public override SealElementType Attribute => SealElementType.Light;

    public override LocString ElementDescription =>
        new LocString("powers", "STS2_STARBORN_ELEMENT_LIGHT.description");

    public override IEnumerable<PowerModel> AssociatedPowers => [ModelDb.Power<ExposePower>()];

    public override async Task OnThreshold(PlayerChoiceContext ctx, Player owner, int stacks) =>
        await PowerCmd.Apply<ExposePower>(
            ctx, owner.Creature.CombatState!.HittableEnemies, stacks * 2, owner.Creature, null);

    public override async Task OnEnhanced(PlayerChoiceContext ctx, Player owner, int stacks)
    {
        await PowerCmd.Apply<ExposePower>(
            ctx, owner.Creature.CombatState!.HittableEnemies, stacks * 2, owner.Creature, null);
        await CreatureCmd.Damage(ctx, owner.Creature.CombatState!.HittableEnemies, 5,
            DamageProps.cardUnpowered, owner.Creature, null);
    }
}
