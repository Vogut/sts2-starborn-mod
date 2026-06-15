using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;

namespace STS2_Starborn.Cards.Common;

/// <summary>
/// 雷电强袭：1费攻击牌。失去2主属性印记。造成5点伤害弹跳4次。
/// 升级：5 -> 7
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public sealed class ThunderAssaultCard() : StarbornCard(
    1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy
)
{
    protected override bool IsPlayable =>
        PrimaryStacks >= 2;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5, ValueProp.Move),
        new RepeatVar(4),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Lose 2 primary element marks
        await SealElementMarkCmd.RemoveElementMarks(choiceContext, MarkSlot.Primary, Owner, 2);

        // Deal damage, bouncing among enemies
        await StarbornCmd.Bounce(
            choiceContext, Owner, this,
            DynamicVars.Damage.BaseValue,
            DynamicVars.Repeat.IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}
