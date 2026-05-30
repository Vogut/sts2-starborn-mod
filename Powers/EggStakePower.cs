using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.RunRngs;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Cards.Kibo;

namespace STS2_Starborn.Powers;

[RegisterPower]
public class EggStakePower : StarbornPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: Const.Paths.PowerIcon(GetType()),
        BigIconPath: Const.Paths.PowerBigIcon(GetType())
    );

    public override async Task AfterSideTurnStart(
        CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        if (!participants.Contains(Owner)) return;

        var player = combatState.Players.FirstOrDefault(p => p.Creature == Owner);
        if (player == null) return;

        var kiboKeyword = KiboKeywords.KiboKeywordId.GetModCardKeyword();
        var candidates = player.Character.CardPool
            .GetUnlockedCards(
                player.UnlockState,
                player.RunState.CardMultiplayerConstraint)
            .Where(c => c.CanonicalKeywords.Contains(kiboKeyword))
            .ToList();

        if (candidates.Count == 0) return;

        Flash();

        var rng = ModRunRngRegistry.Get(player, Const.ModId, "EggStake");
        var picked = candidates[rng.NextInt(0, candidates.Count)];

        var card = player.Creature.CombatState!.CreateCard(picked, player);
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);
    }
}
