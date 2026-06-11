using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Commands;
using STS2_Starborn.Hooks;

namespace STS2_Starborn.Relics;

[RegisterRelic(typeof(StarbornRelicPool))]
public class ChickDollRelic : StarbornRelic, IKiboSwitchListener
{
    private bool _hasTriggeredThisTurn;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task BeforeSideTurnEndEarly(
        PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player && participants.Contains(Owner.Creature))
            _hasTriggeredThisTurn = false;
    }

    public async Task AfterKiboSwitchOn(Player player, string typeId)
    {
        if (_hasTriggeredThisTurn) return;
        var combatState = player.Creature.CombatState;
        if (combatState == null) return;

        Flash();
        _hasTriggeredThisTurn = true;
        await KiboCmd.TryAutoPlayRandomNormalCard(player, combatState);
    }
}
