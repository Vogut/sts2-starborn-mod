using System.Collections.Generic;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Rare;

/// <summary>
/// 调谐印刻：3费能力牌。调谐/超限的印记消耗-1，回合开始时属性不再重置为无属性。
/// 升级前虚无，升级后失去虚无。能力不可叠加。
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public class SaverFormCard() : StarbornCard(
    3, CardType.Power, CardRarity.Rare, TargetType.Self
)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Ethereal];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<SaverFormPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<SaverFormPower>(choiceContext,
            Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
    }
}
