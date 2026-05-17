using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Character;
using STS2_Starborn.Commands;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Common;

/// <summary>
/// 测试卡牌：将主属性印记切换为火属性并叠加印记层数。
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public class SwitchPrimaryMarkCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Common, TargetType.None
)
{
   
    // 叠加印记层数，初始值为2，升级后变为3
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Magic", 2)];   

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.Creature.FindPower<PrimaryMarkPower>() is { } mark)
        {
            await SealMarkCmd.SetElementType(choiceContext, mark, SealElementType.Fire);
            await SealMarkCmd.GainElementMarks<PrimaryMarkPower>(choiceContext, mark, DynamicVars["Magic"].IntValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Magic"].BaseValue += 1; // 2 → 3
    }
}
