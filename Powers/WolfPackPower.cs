using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
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

    public async Task AfterKiboCardAutoPlayed(CardModel card)
    {
        if (_isPlayingAllWolfCards)
            return;

        if (!card.HasModKeyword(KiboKeywords.NormalKeyword))
            return;
        if (!card.HasModKeyword(KiboKeywords.TypeKeywordValue(KiboTypeId.SwiftWolf)))
            return;

        var combatState = base.CombatState;
        if (combatState == null)
            return;

        var player = combatState.Players.FirstOrDefault(p => p.Creature == Owner);
        if (player == null)
            return;

        var activePile = KiboPileManager.GetActivePile(player);
        if (activePile == null)
            return;

        var others = activePile.Cards
            .Where(c => c != card)
            .Where(c => c.HasModKeyword(KiboKeywords.NormalKeyword)
                     && c.HasModKeyword(KiboKeywords.TypeKeywordValue(KiboTypeId.SwiftWolf)))
            .ToList();
        if (others.Count == 0)
            return;

        _isPlayingAllWolfCards = true;
        try
        {
            foreach (var c in others)
                await KiboCmd.AutoPlay(new BlockingPlayerChoiceContext(), c, combatState);
        }
        finally
        {
            _isPlayingAllWolfCards = false;
        }
    }

    // ── IKiboSwitchListener ──

    public async Task AfterKiboSwitchOff(Player player, string typeId)
    {
        if (typeId == KiboTypeId.SwiftWolf && player.Creature == Owner)
            await PowerCmd.Remove(this);
    }
}
