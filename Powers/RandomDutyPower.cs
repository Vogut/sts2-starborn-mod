using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.RunRngs;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Cards;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Cards.Pile;
using STS2_Starborn.Commands;
using STS2_Starborn.Hooks;

namespace STS2_Starborn.Powers;

/// <summary>
/// 随机上岗 Power：使用奇波牌召唤奇波时，随机召唤一只奇波并使用其奇波战技。
/// </summary>
[RegisterPower]
public class RandomDutyPower : StarbornPower, IKiboSwitchListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: Const.Paths.PowerIcon(GetType()),
        BigIconPath: Const.Paths.PowerBigIcon(GetType())
    );

    private CardModel? _currentPlayingCard;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await base.AfterCardPlayed(choiceContext, cardPlay);

        // 记录当前打出的卡牌用于拦截判断
        _currentPlayingCard = cardPlay.Card;
    }

    /// <summary>
    /// 拦截奇波切换，只有当前正在打出的是有 KiboSummonType 的玩家卡牌时，才替换为随机奇波召唤。
    /// </summary>
    public bool ShouldPreventKiboSwitch(Player player, string from, string to)
    {
        // 只有当前 Power 持有者是该 Player 的 Creature 时才拦截
        if (player.Creature != Owner) return false;

        // 检查当前是否有正在打出的卡牌
        if (_currentPlayingCard == null) return false;

        // 检查这张卡是否是 StarbornCard 且有 KiboSummonType
        if (_currentPlayingCard is not StarbornCard starbornCard) return false;
        if (string.IsNullOrEmpty(starbornCard.KiboSummonType)) return false;

        // 排除奇波牌本身（RepCard 和能力牌）
        if (_currentPlayingCard is KiboCard) return false;

        // 是玩家打出的召唤卡，进行拦截
        return true;
    }

    public async Task BeforeKiboSwitch(Player player, string from, string to)
    {
        if (player.Creature != Owner) return;

        Flash();

        var combatStorage = KiboPileManager.GetStorageCombatPile(player);
        if (combatStorage == null) return;

        var candidates = KiboPileManager.GetKiboTypesInPile(combatStorage).ToList();
        if (candidates.Count == 0) return;

        var ctx = new BlockingPlayerChoiceContext();
        var combatState = player.Creature.CombatState;
        if (combatState == null) return;

        var rng = ModRunRngRegistry.Get(player, Const.ModId, "random_duty_summon");

        // 根据层数循环触发多次
        var times = Amount;
        for (var i = 0; i < times; i++)
        {
            // 每次随机选择一只奇波（包括当前活跃的）
            var randomTypeId = candidates[rng.NextInt(0, candidates.Count)];

            // 获取当前活跃奇波
            var currentActive = KiboPileManager.GetActiveKiboType(player);

            // 换下当前奇波，换上随机奇波
            if (currentActive != null)
                await KiboCmd.SwitchOff(ctx, player, currentActive);
            await KiboCmd.SwitchOn(ctx, player, randomTypeId);

            // 自动打出随机奇波的一张普通能力牌
            await KiboCmd.TryAutoPlayRandomNormalCard(player, combatState);
        }

        // 清空记录
        _currentPlayingCard = null;
    }
}
