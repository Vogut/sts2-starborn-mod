using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Uncommon;

/// <summary>
/// 腾龙舞：2费稀有攻击。超限1水。造成10点伤害。释放当前奇波的奇波特技。
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public sealed class DragonDanceCard() : StarbornCard(
    3, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10m, ValueProp.Move),
        StarbornCardVars.Overload(1, SealElementType.Water),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        var overloadElementType = ((SealElementVar)DynamicVars["Overload"]).ElementType;
        await StarbornCmd.Overload(choiceContext, MarkSlot.Primary, Owner,
            DynamicVars["Overload"].IntValue, overloadElementType, this);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        await KiboCmd.TryAutoPlayRandomUltimateCard(Owner, Owner.Creature.CombatState!);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
    }
}
