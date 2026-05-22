using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Creatures;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Models;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Cards.Pile;
using STS2_Starborn.Commands;
using STS2_Starborn.Runs;

namespace STS2_Starborn.Combat;

[RegisterSingleton]
public sealed class KiboCombatManager : HookedSingletonModel
{
    public static event Action? PileChanged;

    public KiboCombatManager() : base(HookedSingletonModel.HookType.Combat) { }

    public static void NotifyPileChanged() => PileChanged?.Invoke();

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player)
            return;

        var combatState = CombatManager.Instance.DebugOnlyGetState();
        if (combatState == null)
            return;

        var player = combatState.Players.FirstOrDefault();
        if (player == null)
            return;

        var data = KiboRunData.Get(player);
        if (data?.ActiveKiboTypeId == null)
            return;

        if (!Enum.TryParse<KiboTypeId>(data.ActiveKiboTypeId, out var typeId))
            return;

        var pile = KiboPileManager.GetPile(player);
        if (pile == null || pile.Cards.Count == 0)
        {
            await KiboPileManager.RefillPile(new BlockingPlayerChoiceContext(), player, typeId);
            pile = KiboPileManager.GetPile(player);
        }

        if (pile == null || pile.Cards.Count == 0)
            return;

        var normalCards = pile.Cards
            .Where(c => c.HasModKeyword(KiboKeywords.NormalKeywordId))
            .ToList();

        if (normalCards.Count == 0)
            return;

        var card = normalCards[Random.Shared.Next(normalCards.Count)];
        await KiboCmd.AutoPlay(new BlockingPlayerChoiceContext(), card, combatState);
    }

    public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(
        CardModel card, bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position)
    {
        if (card.HasModKeyword(KiboKeywords.PileMemberKeywordId))
            return (KiboPileManager.GetPileType(), position);

        return (pileType, position);
    }
}
