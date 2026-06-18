using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Utils;
using STS2_Starborn.Character;

namespace STS2_Starborn.Relics;

/// <summary>
/// 签到本：每经过7场战斗，额外掉落一次卡牌、遗物奖励以及50金币。
/// </summary>
[RegisterRelic(typeof(StarbornRelicPool))]
public class DailyLoginBookRelic : StarbornRelic
{
    private const int CombatInterval = 7;
    private const int GoldAmount = 50;
    private const int CardOptionCount = 3;

    private static readonly SavedAttachedState<DailyLoginBookRelic, int> s_combatCount =
        new("CombatCount", () => 0);

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("CombatInterval", CombatInterval),
        new IntVar("GoldAmount", GoldAmount),
    ];

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        var count = s_combatCount[this] + 1;

        if (count >= CombatInterval)
        {
            s_combatCount[this] = 0;
            Flash();

            room.AddExtraReward(Owner,
                new CardReward(CardCreationOptions.ForRoom(Owner, room.RoomType), CardOptionCount, Owner));
            room.AddExtraReward(Owner,
                new RelicReward(Owner));
            room.AddExtraReward(Owner,
                new GoldReward(GoldAmount, Owner));
        }
        else
        {
            s_combatCount[this] = count;
        }

        await Task.CompletedTask;
    }
}
