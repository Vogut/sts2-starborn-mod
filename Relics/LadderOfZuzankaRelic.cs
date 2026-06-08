using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Element;
using STS2_Starborn.Hooks;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Relics;

/// <summary>
/// 祖赞卡之梯：每场战斗首次切换过3种属性后，获得2层调谐强化。
/// </summary>
[RegisterRelic(typeof(StarbornRelicPool))]
public class LadderOfZuzankaRelic : StarbornRelic, ISealElementMarkListener
{
    private bool _hasTriggeredThisCombat;

    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new IntVar("Stacks", 2)];

    public override async Task BeforeCombatStart()
    {
        _hasTriggeredThisCombat = false;
    }

    async Task ISealElementMarkListener.AfterElementChanged(
        PlayerChoiceContext ctx, MarkSlot slot, SealElementType from, SealElementType to)
    {
        if (_hasTriggeredThisCombat) return;
        if (to == SealElementType.None) return;

        var switchedCount = ElementMarkManager.GetSwitchedTypeCount(Owner);
        if (switchedCount >= 3)
        {
            _hasTriggeredThisCombat = true;
            Flash();
            await PowerCmd.Apply<TuningEmpowermentPower>(
                ctx, Owner.Creature, DynamicVars["Stacks"].IntValue, Owner.Creature, null);
        }
    }
}
