using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Cards;
using STS2_Starborn.Character;
using STS2_Starborn.Commands;

namespace STS2_Starborn.Relics;

[RegisterRelic(typeof(StarbornRelicPool))]
public class HakuinRingRelic : StarbornRelic
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3, ValueProp.Unpowered),
        new IntVar("Counts", 5),
    ];

    protected override IEnumerable<string> RegisteredKeywordIds =>
        [StarbornKeywords.BounceKeywordId];

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override async Task BeforeCombatStart()
    {
        Flash();
        await StarbornCmd.Bounce(
            Owner,
            Owner.Creature,
            DynamicVars.Damage.BaseValue,
            (int)DynamicVars["Counts"].BaseValue);
    }
}
