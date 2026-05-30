using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Common;

/// <summary>
/// 测试卡牌：将主属性印记切换为火属性并叠加印记层数。
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public class SwitchPrimaryMarkCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Common, TargetType.None
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        StarbornCardVars.ElementMark(2, SealElementType.Fire),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SealElementMarkCmd.GainElementMarks(
            choiceContext, MarkSlot.Primary, Owner,
            DynamicVars["ElementMark"].IntValue, SealElementType.Fire);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ElementMark"].BaseValue += 1;
    }
}
