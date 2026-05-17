using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2_Starborn.Character;
using STS2_Starborn.Commands;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Common;

/// <summary>
/// 超限卡：将主属性印记切换为火属性并直接叠满至5层，显示超限效果数值及 tooltip。
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public class OverloadCard() : StarbornCard(
    2, CardType.Skill, CardRarity.Uncommon, TargetType.None
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        StarbornCardVars.Overload(10),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.Creature.FindPower<PrimaryMarkPower>() is { } mark)
        {
            await SealElementMarkCmd.SetElementType(choiceContext, mark, SealElementType.Fire);
            var need = SealElementMarkPower.ThresholdStacks - mark.DisplayAmount;
            if (need > 0)
                await SealElementMarkCmd.GainElementMarks(choiceContext, mark, need, Owner.Creature, this);
            await StarbornCmd.Overload(choiceContext, mark, 1, Owner.Creature, this);
            var remain = SealElementMarkPower.MaxSealStacks - mark.DisplayAmount;
            if (remain > 0)
                await SealElementMarkCmd.GainElementMarks(choiceContext, mark, remain, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Overload"].BaseValue += 5; // 10 → 15
    }
}
