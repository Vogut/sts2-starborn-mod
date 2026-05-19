using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Characters;

namespace STS2_Starborn.Kibo;

[RegisterPower]
public sealed class KiboActivePower : ModPowerTemplate
{
    private KiboTypeId _activeType;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;
    public override bool ShouldReceiveCombatHooks => true;

    public void SetActiveKiboType(KiboTypeId typeId)
    {
        _activeType = typeId;
    }

    public override async Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
    {
        if (side != Owner.Side)
            return;

        var player = Owner.Player;
        if (player == null)
            return;

        var pile = KiboPileManager.GetPile(player);
        if (pile == null || pile.Cards.Count == 0)
        {
            await KiboPileManager.RefillPile(new BlockingPlayerChoiceContext(), player, _activeType);
            pile = KiboPileManager.GetPile(player);
        }

        if (pile == null || pile.Cards.Count == 0)
            return;

        var card = pile.Cards[0];
        await CardCmd.AutoPlay(new BlockingPlayerChoiceContext(), card, null);
    }

    public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(
        CardModel card, bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position)
    {
        if (card.HasModKeyword(KiboKeywords.PileMemberKeywordId))
            return (KiboPileManager.GetPileType(), position);

        return (pileType, position);
    }
}
