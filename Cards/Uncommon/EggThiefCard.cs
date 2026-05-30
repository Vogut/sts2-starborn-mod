using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.RunRngs;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Character;

namespace STS2_Starborn.Cards.Uncommon;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class EggThiefCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var kiboKeyword = KiboKeywords.KiboKeywordId.GetModCardKeyword();
        var candidates = Owner.Character.CardPool
            .GetUnlockedCards(
                Owner.UnlockState,
                Owner.RunState.CardMultiplayerConstraint)
            .Where(c => c.CanonicalKeywords.Contains(kiboKeyword))
            .ToList();

        if (candidates.Count == 0) return;

        var rng = ModRunRngRegistry.Get(Owner, Const.ModId, "EggThief");
        var picked = candidates[rng.NextInt(0, candidates.Count)];

        var card = Owner.Creature.CombatState!.CreateCard(picked, Owner);
        card.SetToFreeThisTurn();
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
