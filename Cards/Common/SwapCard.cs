using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Character;
using STS2_Starborn.Cards;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Common;

[RegisterCard(typeof(StarbornCardPool))]
public class SwapCard() : StarbornCard(
    1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4, ValueProp.Move),
        new DamageVar("Dam2", 6, ValueProp.Move),
        Tuning(1, SealElementType.Any),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        await DamageCmd.Attack(DynamicVars["Dam2"].BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        var tuningVar = (SealElementVar)DynamicVars["Tuning"];
        await StarbornCmd.Tuning(choiceContext, MarkSlot.Primary, Owner, tuningVar.IntValue, tuningVar.ElementType, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
        DynamicVars["Dam2"].UpgradeValueBy(2);
    }
}
