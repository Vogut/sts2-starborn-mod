using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Cards.Pile;
using STS2_Starborn.Commands;
using STS2_Starborn.Hooks;

namespace STS2_Starborn.Powers;

[RegisterPower]
public class WolfPackPower : StarbornPower, IKiboCardPlayListener, IKiboSwitchListener
{
    private bool _isPlayingAllWolfCards;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: Const.Paths.PowerIcon(GetType()),
        BigIconPath: Const.Paths.PowerBigIcon(GetType())
    );

    // ── IKiboCardPlayListener ──

    public bool ShouldPreventKiboAutoPlay(CardModel card)
    {
        if (_isPlayingAllWolfCards) return false;
        return true;
    }

    // ── IKiboSwitchListener ──

    public async Task AfterKiboSwitchOff(Player player, KiboTypeId typeId)
    {
        if (typeId == KiboTypeId.SwiftWolf && player.Creature == Owner)
            await PowerCmd.Remove(this);
    }

    // ── End of turn: play all SwiftWolf normal cards ──

    public override async Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player) return;
        if (!participants.Contains(Owner)) return;

        var combatState = base.CombatState;
        if (combatState == null) return;

        var player = combatState.Players.FirstOrDefault(p => p.Creature == Owner);
        if (player == null) return;

        var activeType = KiboPileManager.GetActiveKiboType(player);
        if (activeType != KiboTypeId.SwiftWolf) return;

        var activePile = KiboPileManager.GetActivePile(player);
        if (activePile == null) return;

        var normalCards = activePile.Cards
            .Where(c => c.HasModKeyword(KiboKeywords.NormalKeyword))
            .ToList();
        if (normalCards.Count == 0) return;

        _isPlayingAllWolfCards = true;
        try
        {
            foreach (var card in normalCards)
                await KiboCmd.AutoPlay(new BlockingPlayerChoiceContext(), card, combatState);
        }
        finally
        {
            _isPlayingAllWolfCards = false;
        }
    }
}
