using System.Collections.Generic;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Element;

public sealed class IceElement : StarbornElement
{
    public override SealElementType Attribute => SealElementType.Ice;
    public override int OverloadConsume => 3;

    public override IEnumerable<PowerModel> AssociatedPowers => [ModelDb.Power<FreezePower>()];

    public override async Task OnThreshold(PlayerChoiceContext ctx, Player owner, int stacks, CardModel? source = null, IReadOnlyList<Creature>? targets = null) =>
        await PowerCmd.Apply<FreezePower>(
            ctx, owner.Creature.CombatState!.HittableEnemies, stacks, owner.Creature, null);

    public override async Task OnEnhanced(PlayerChoiceContext ctx, Player owner, int stacks, CardModel? source = null, IReadOnlyList<Creature>? targets = null) =>
        await PowerCmd.Apply<FreezePower>(
            ctx, owner.Creature.CombatState!.HittableEnemies, stacks * 2, owner.Creature, null);
}
