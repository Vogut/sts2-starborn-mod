using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Relics;

namespace STS2_Starborn.Events;

/// <summary>
/// 碧蓝航线事件：持有 Anchor、HornCleat、CaptainsWheel 时触发。
/// 三阶段：进入 → 选择 WisdomCube 或 MetaCube → 退出。
/// </summary>
[RegisterSharedEvent]
public sealed class AzurLaneEvent : ModEventTemplate
{
    public override bool IsAllowed(IRunState runState) =>
        runState.Players.All(p =>
            p.Relics.Any(r => r.Id == ModelDb.Relic<Anchor>().Id) &&
            p.Relics.Any(r => r.Id == ModelDb.Relic<HornCleat>().Id) &&
            p.Relics.Any(r => r.Id == ModelDb.Relic<CaptainsWheel>().Id));

    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: "res://images/events/battleworn_dummy.png"
    );

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, Enter, InitialOptionKey("ENTER")),
    ];

    private Task Enter()
    {
        SetEventState(PageDescription("CHOOSE"), [
            new EventOption(this, ChooseWisdom, ModOptionKey("CHOOSE", "WISDOM")),
            new EventOption(this, ChooseMeta, ModOptionKey("CHOOSE", "META")),
        ]);
        return Task.CompletedTask;
    }

    private async Task ChooseWisdom()
    {
        await RelicCmd.Obtain<WisdomCube>(Owner!);
        SetEventFinished(PageDescription("WISDOM_CHOSEN"));
    }

    private async Task ChooseMeta()
    {
        await RelicCmd.Obtain<MetaCube>(Owner!);
        SetEventFinished(PageDescription("META_CHOSEN"));
    }
}
