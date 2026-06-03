using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_Starborn.Powers;

/// <summary>
/// 无垢海：死亡时回复所有血量，结束敌人回合，抽满手牌，并将来源卡牌从卡组中移除。
/// 由 PureSeaCard 施加，为一次性效果。
/// </summary>
[RegisterPower]
public class PureSeaPower : StarbornPower
{
    private class Data
    {
        public CardModel? SourceCard;
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: Const.Paths.PowerIcon(GetType()),
        BigIconPath: Const.Paths.PowerBigIcon(GetType())
    );

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        GetInternalData<Data>().SourceCard = cardSource;
        return Task.CompletedTask;
    }

    public override bool ShouldDie(Creature creature)
    {
        if (creature != Owner)
            return true;
        return false;
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        if (creature != Owner)
            return;

        // 1. 回复满血
        var healAmount = Owner.MaxHp - Owner.CurrentHp;
        if (healAmount > 0m)
            await CreatureCmd.Heal(Owner, healAmount);

        // 2. 结束敌人回合 + 抽满手牌
        var player = Owner.Player;
        if (player != null)
        {
            // 强制跳过当前阶段
            PlayerCmd.EndTurn(player, canBackOut: false);

            // 抽满手牌
            var hand = player.PlayerCombatState.Hand;
            var cardsToDraw = CardPile.MaxCardsInHand - hand.Cards.Count;
            if (cardsToDraw > 0)
                await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), cardsToDraw, player);
        }

        // 3. 从卡组中永久移除此牌
        var sourceCard = GetInternalData<Data>().SourceCard;
        if (sourceCard != null)
            await CardPileCmd.RemoveFromDeck(sourceCard);

        // 4. 移除自身能力（一次性效果）
        await PowerCmd.Remove(this);
    }
}
