using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Cards.Pile;
using STS2_Starborn.Character;
using STS2_Starborn.Commands;

namespace STS2_Starborn.Cards.Rare;

/// <summary>
/// 极致的星结卡：选择一只奇波进行召唤并使用其战技与绝技。
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public sealed class UltimateStarboundCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Rare, TargetType.Self
)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            if (!IsUpgraded)
                yield return CardKeyword.Exhaust;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Get the combat storage pile (contains all available Kibos)
        var combatStorage = KiboPileManager.GetStorageCombatPile(Owner);
        if (combatStorage == null) return;

        // Filter to only RepCards (one per Kibo type)
        var repCards = combatStorage.Cards
            .Where(c => KiboPileManager.IsRepCardType(c.GetType()))
            .ToList();

        if (repCards.Count == 0) return;

        // Let player choose a Kibo (select its RepCard)
        var prefs = new CardSelectorPrefs(
            new LocString("cards", "STS2_STARBORN_CARD_ULTIMATE_STARBOUND_CARD.choicePrompt"), 1)
        {
            Cancelable = true
        };

        var selected = (await CardSelectCmd.FromCombatPile(choiceContext, combatStorage, Owner, prefs,
            card => KiboPileManager.IsRepCardType(card.GetType()))).ToList();

        var chosenRepCard = selected.FirstOrDefault();
        if (chosenRepCard == null) return;

        // Extract Kibo type from the chosen RepCard
        var kiboType = KiboPileManager.GetKiboType(chosenRepCard);
        if (kiboType == null) return;

        // Summon the chosen Kibo
        await KiboCmd.Summon(choiceContext, Owner, kiboType);

        // Get the active pile (now contains the chosen Kibo's abilities)
        var activePile = KiboPileManager.GetActivePile(Owner);
        if (activePile == null) return;

        var combatState = Owner.Creature.CombatState;
        if (combatState == null) return;

        // Find and play all normal abilities
        var normalAbilities = activePile.Cards
            .Where(c => c.HasModKeyword(KiboKeywords.NormalKeyword))
            .ToList();

        foreach (var normalAbility in normalAbilities)
            await KiboCmd.AutoPlay(choiceContext, normalAbility, combatState);

        // Find and play all ultimate abilities
        var ultimateAbilities = activePile.Cards
            .Where(c => c.HasModKeyword(KiboKeywords.UltimateKeyword))
            .ToList();

        foreach (var ultimateAbility in ultimateAbilities)
            await KiboCmd.AutoPlay(choiceContext, ultimateAbility, combatState);
    }

    protected override void OnUpgrade()
    {
        // Upgrade removes Exhaust keyword
    }
}
