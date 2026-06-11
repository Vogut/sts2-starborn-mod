using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;

namespace STS2_Starborn.Relics;

[RegisterRelic(typeof(StarbornRelicPool))]
public class FlyingSaddleRelic : StarbornRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task BeforeCombatStart()
    {
        Flash();
        await PowerCmd.Apply<SoarPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1m, Owner.Creature, null);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player && participants.Contains(Owner.Creature))
        {
            Flash();
            await PowerCmd.Remove<SoarPower>(Owner.Creature);
        }
    }
}
