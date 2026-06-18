using System.Collections.Generic;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2_Starborn.Commands;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Element;

public sealed class WaterElement : StarbornElement
{
    public override SealElementType Attribute => SealElementType.Water;

    public override LocString ElementDescription =>
        new LocString("powers", "STS2_STARBORN_ELEMENT_WATER.description");

    public override IEnumerable<PowerModel> AssociatedPowers =>
        [ModelDb.Power<SurgePower>(), ModelDb.Power<DrownPower>()];

    public override async Task OnThreshold(PlayerChoiceContext ctx, Player owner, int stacks, CardModel? source = null, IReadOnlyList<Creature>? targets = null)
    {
        await PowerCmd.Apply<SurgePower>(
            ctx, owner.Creature, stacks, owner.Creature, null);

        var combatState = owner.Creature.CombatState;
        if (combatState != null)
            await KiboCmd.TryAutoPlayRandomNormalCard(owner, combatState);
    }

    public override async Task OnEnhanced(PlayerChoiceContext ctx, Player owner, int stacks, CardModel? source = null, IReadOnlyList<Creature>? targets = null)
    {
        await PowerCmd.Apply<SurgePower>(
            ctx, owner.Creature, stacks, owner.Creature, null);
        await PowerCmd.Apply<DrownPower>(
            ctx, owner.Creature.CombatState!.HittableEnemies, 1, owner.Creature, null);

        var combatState = owner.Creature.CombatState;
        if (combatState != null)
            await KiboCmd.TryAutoPlayRandomNormalCard(owner, combatState);
    }
}
