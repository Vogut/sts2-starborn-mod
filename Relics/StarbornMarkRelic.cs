using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Character;
using STS2_Starborn.Powers;


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

    public override async Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
    {
        if (side == base.Owner.Creature.Side && combatState.RoundNumber == 1)
        {
            if (!base.Owner.Creature.HasPower<PrimaryMarkPower>())
                await PowerCmd.Apply<PrimaryMarkPower>(new ThrowingPlayerChoiceContext(), base.Owner.Creature, 1, base.Owner.Creature, null, silent: true);

            if (!base.Owner.Creature.HasPower<SecondaryMarkPower>())
                await PowerCmd.Apply<SecondaryMarkPower>(new ThrowingPlayerChoiceContext(), base.Owner.Creature, 1, base.Owner.Creature, null, silent: true);
        }
    }
}
