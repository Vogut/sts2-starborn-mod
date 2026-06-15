using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
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
    private const int GoldAmount = 50;

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

        // Always add gold reward
        room.AddExtraReward(Owner.Player,
            new GoldReward(GoldAmount, Owner.Player));

        // Add relic reward only for Elite and Boss rooms (like vanilla game)
        if (room.RoomType == RoomType.Elite)
        {
            room.AddExtraReward(Owner.Player,
                new RelicReward(Owner.Player));
        }

        await Task.CompletedTask;
    }
}
