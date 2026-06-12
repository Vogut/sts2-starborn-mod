using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Cards;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Hooks;

namespace STS2_Starborn.Relics;

/// <summary>
/// 哈库茵权戒：每次调谐/超限后，造成1点伤害，弹跳5次。
/// </summary>
[RegisterRelic(typeof(StarbornRelicPool))]
public class HakuinRingRelic : StarbornRelic, ITuningOverloadListener
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(1, ValueProp.Unpowered),
        new IntVar("Bounce", 5),
    ];

    protected override IEnumerable<string> RegisteredKeywordIds =>
        [StarbornKeywords.BounceKeywordId];

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public async Task AfterTuning(PlayerChoiceContext ctx, MarkSlot slot, int consume, CardModel? source)
    {
        Flash();
        await StarbornCmd.Bounce(
            Owner,
            Owner.Creature,
            DynamicVars.Damage.BaseValue,
            (int)DynamicVars["Bounce"].BaseValue);
    }

    public async Task AfterOverload(PlayerChoiceContext ctx, MarkSlot slot, int consume, CardModel? source)
    {
        Flash();
        await StarbornCmd.Bounce(
            Owner,
            Owner.Creature,
            DynamicVars.Damage.BaseValue,
            (int)DynamicVars["Bounce"].BaseValue);
    }
}
