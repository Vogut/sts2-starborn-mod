using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Commands;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Uncommon;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class DuelCommandCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Uncommon, TargetType.Self
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Empower", 5),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var stacks = DynamicVars["Empower"].IntValue;
        await PowerCmd.Apply<TemporaryKiboEmpowermentPower>(
            choiceContext, Owner.Creature, stacks, Owner.Creature, this);
        await KiboCmd.TryAutoPlayRandomUltimateCard(Owner, Owner.Creature.CombatState!);
    }

    protected override void OnUpgrade()
        => DynamicVars["Empower"].BaseValue = 10;
}
