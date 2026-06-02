using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.RunRngs;

namespace STS2_Starborn.Cards.Kibo;

[RegisterCard(typeof(KiboCardPool))]
[KiboAbilityOf(KiboTypeId.MelodiousVine, true)]
public sealed class KiboAromatherapyCard() : KiboCard(CardType.Skill, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        KiboKeywords.UltimateKeyword,
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var statuses = Owner.PlayerCombatState!.AllCards
            .Where(c => c.Type == CardType.Status && c.Pile?.Type != PileType.Exhaust)
            .ToList();

        if (statuses.Count == 0) return;

        if (IsUpgraded)
        {
            foreach (var status in statuses)
                await CardCmd.Exhaust(choiceContext, status);
        }
        else
        {
            var rng = ModRunRngRegistry.Get(Owner, Const.ModId, "Aromatherapy");
            var picked = statuses[rng.NextInt(0, statuses.Count)];
            await CardCmd.Exhaust(choiceContext, picked);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
