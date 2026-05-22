using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.Creatures;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Runs;

namespace STS2_Starborn.Relics;

/// <summary>
/// 星临者的起始遗物，战斗开始时为玩家赋予主属性印记和副属性印记（各1层）。
/// 使用 Akabeko 模式：在第1回合玩家方回合开始时应用能力（AfterSideTurnStart + RoundNumber == 1）。
/// </summary>
[RegisterRelic(typeof(StarbornRelicPool))]
[RegisterCharacterStarterRelic(typeof(Starborn))]
public class StarbornMarkRelic : StarbornRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override Task AfterSideTurnStart(CombatSide side,IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == base.Owner.Creature.Side && combatState.RoundNumber == 1)
        {
            ElementMarkRunData.Modify(base.Owner, data =>
            {
                data.PrimaryStacks = 1;
                data.SecondaryStacks = 1;
            });
            ElementMarkManager.NotifyMarksChanged();
        }
        return Task.CompletedTask;
    }
}
