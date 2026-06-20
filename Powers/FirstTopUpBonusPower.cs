using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_Starborn.Powers;

/// <summary>
/// 首充双倍能力：战斗结束时额外获得一次完整的战斗奖励（卡牌、遗物、金币）。
/// </summary>
[RegisterPower]
public class FirstTopUpBonusPower : StarbornPower
{
    private const int CardOptionCount = 3;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: Const.Paths.PowerIcon(GetType()),
        BigIconPath: Const.Paths.PowerBigIcon(GetType())
    );

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        if (Amount <= 0) return;
        if (Owner.Player == null) return;

        Flash();

        // Always add card reward
        room.AddExtraReward(Owner.Player,
            new CardReward(CardCreationOptions.ForRoom(Owner.Player, room.RoomType), CardOptionCount, Owner.Player));

        var goldReward = CreateGoldReward(room, Owner.Player);
        if (goldReward != null)
        {
            room.AddExtraReward(Owner.Player, goldReward);
        }

        // Add relic reward only for Elite rooms (like vanilla combat rewards).
        if (room.RoomType == RoomType.Elite)
        {
            room.AddExtraReward(Owner.Player,
                new RelicReward(Owner.Player));
        }

        await Task.CompletedTask;
    }

    private static GoldReward? CreateGoldReward(CombatRoom room, Player player)
    {
        return room.RoomType switch
        {
            RoomType.Monster when room.GoldProportion > 0f => new GoldReward(
                (int)Math.Round(room.Encounter.MinGoldReward * room.GoldProportion),
                (int)Math.Round(room.Encounter.MaxGoldReward * room.GoldProportion),
                player),
            RoomType.Elite or RoomType.Boss => new GoldReward(
                room.Encounter.MinGoldReward,
                room.Encounter.MaxGoldReward,
                player),
            _ => null,
        };
    }
}
