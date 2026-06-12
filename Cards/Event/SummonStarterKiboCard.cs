using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2_Starborn.Character;
using STS2_Starborn.Commands;
using STS2_Starborn.Runs;
using STS2_Starborn.Cards.Kibo;

namespace STS2_Starborn.Cards.Event;

/// <summary>
/// 召唤初始奇波的卡牌。在战斗开始时由 StarBoundCardRelic 添加到手牌。
/// 动态读取 KiboRunData.ActiveKiboTypeId 来召唤对应的奇波，因此自动支持奇波进化。
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public sealed class SummonStarterKiboCard() : StarbornCard(
    0, CardType.Skill, CardRarity.Token, TargetType.Self, shouldShowInCardLibrary: false)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        KiboKeywords.KiboKeywordId.GetModCardKeyword(),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            if (Owner == null) yield break;

            var data = KiboRunData.Get(Owner);
            if (data?.ActiveKiboTypeId == null) yield break;
            if (!Kibo.KiboTypeId.TryParse(data.ActiveKiboTypeId, out var typeId)) yield break;

            var def = Kibo.KiboTypeRegistry.Get(typeId);
            var repCardModel = ModelDb.GetById<CardModel>(ModelDb.GetId(def.RepCardType));
            yield return HoverTipFactory.FromCard(repCardModel);
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var data = KiboRunData.Get(Owner);
        if (data?.ActiveKiboTypeId == null) return;
        if (!Kibo.KiboTypeId.TryParse(data.ActiveKiboTypeId, out var typeId)) return;

        await KiboCmd.Summon(choiceContext, Owner, typeId);
    }
}
