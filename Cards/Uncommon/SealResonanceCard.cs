using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Uncommon;

/// <summary>
/// 调谐印刻：3费能力牌。调谐/超限的印记消耗-1，回合开始时属性不再重置为无属性。
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public class SealResonanceCard() : StarbornCard(
    3, CardType.Power, CardRarity.Uncommon, TargetType.None
)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply(choiceContext, ModelDb.Power<SealResonancePower>(),
            Owner.Creature, 1, Owner.Creature, this);
    }
}
