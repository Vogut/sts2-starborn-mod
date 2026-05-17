using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Commands;
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
    protected override bool IsPlayable =>
        StarbornCmd.CanTuning(PrimaryMark, DynamicVars["Tuning"].IntValue);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        StarbornCardVars.ElementMark(2, SealElementType.Fire),
        StarbornCardVars.Tuning(1, SealElementType.Fire),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await StarbornCmd.Tuning(choiceContext, PrimaryMark!, DynamicVars["Tuning"].IntValue, SealElementType.Fire, this);
        await SealElementMarkCmd.GainElementMarks(choiceContext, PrimaryMark!, DynamicVars["ElementMark"].IntValue, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ElementMark"].BaseValue += 1; // 2 → 3
    }
}
