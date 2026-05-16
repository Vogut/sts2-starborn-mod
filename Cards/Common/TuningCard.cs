using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2_Starborn.Character;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Common;

/// <summary>
/// 调谐卡：将主属性印记切换为火属性并叠加层数，显示调谐效果数值及 tooltip。
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public class TuningCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Common, TargetType.None
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Magic", 2),
        StarbornCardVars.Tuning(3),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var mark = Owner.Creature.FindPower<PrimaryMarkPower>();
        if (mark != null)
        {
            await mark.SetAttribute(choiceContext, SealAttribute.Fire);
            await PowerCmd.Apply<PrimaryMarkPower>(choiceContext, Owner.Creature, DynamicVars["Magic"].IntValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Magic"].BaseValue += 1; // 2 → 3
    }
}
