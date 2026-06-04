using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;
using STS2_Starborn.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace STS2_Starborn.Cards.Common;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class SweetPunishmentCard() : StarbornCard(
    0, CardType.Attack, CardRarity.Common, TargetType.AllEnemies
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        StarbornCardVars.Tuning(1, SealElementType.Light),
        new DamageVar(5,ValueProp.Move),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null) return;

        var tuningElementType = ((SealElementVar)DynamicVars["Tuning"]).ElementType;
        await StarbornCmd.Tuning(choiceContext, MarkSlot.Primary, Owner,
            DynamicVars["Tuning"].IntValue, tuningElementType, this);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(CombatState)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}
