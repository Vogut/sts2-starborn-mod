using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Cards.Event;

namespace STS2_Starborn.Relics;

/// <summary>
/// 元魔方：拾起时将 演算和失控 加入牌组。参照 ArcaneScroll 实现。
/// </summary>
[RegisterRelic(typeof(SharedRelicPool))]
public class MetaCube : StarbornRelic
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override async Task AfterObtained()
    {
        var card1 = Owner.RunState.CreateCard<SimulationCard>(Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card1, PileType.Deck));
        var card2 = Owner.RunState.CreateCard<OutOfControlCard>(Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card2, PileType.Deck));
    }
}
