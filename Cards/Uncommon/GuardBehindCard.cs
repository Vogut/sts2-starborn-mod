using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Cards;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Uncommon;

/// <summary>
/// 护至身后：1费罕见技能。获得5点格挡。调谐1副属性。
/// 升级：格挡 5 → 8。
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public sealed class GuardBehindCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Uncommon, TargetType.Self
)
{
    private const int TuningConsume = 1;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5, ValueProp.Move),
        Tuning(TuningConsume, SealElementType.Any, "Tuning", MarkSlot.Secondary),
        Overload(TuningConsume + 1, SealElementType.Any, "Overload", MarkSlot.Secondary),
        StarbornCardVars.IfCanOverload(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        await StarbornCmd.Tuning(choiceContext, MarkSlot.Secondary, Owner,
            DynamicVars["Tuning"].IntValue, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
