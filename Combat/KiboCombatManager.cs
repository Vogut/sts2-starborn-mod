using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Models;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Cards.Pile;

namespace STS2_Starborn.Combat;

[RegisterSingleton]
public sealed class KiboCombatManager : HookedSingletonModel
{
    public KiboCombatManager() : base(HookedSingletonModel.HookType.Combat) { }

    public override async Task BeforeCombatStart()
    {
        var combatState = CombatManager.Instance.DebugOnlyGetState();
        if (combatState == null) return;

        foreach (var player in combatState.Players)
            await KiboPileManager.InitializeForCombat(player);
    }

    public override async Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player)
            return;

        var combatState = CombatManager.Instance.DebugOnlyGetState();
        if (combatState == null)
            return;

        var player = combatState.Players.FirstOrDefault();
        if (player == null)
            return;

        await KiboPileManager.TryAutoPlayRandomNormalCard(player, combatState);
    }

    public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(
        CardModel card, bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position)
    {
        if (card.HasModKeyword(KiboKeywords.PileMemberKeywordId))
            return (KiboPileManager.GetActivePileType(), position);

        return (pileType, position);
    }
}
