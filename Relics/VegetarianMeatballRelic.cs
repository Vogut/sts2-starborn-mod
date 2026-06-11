using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;

namespace STS2_Starborn.Relics;

[RegisterRelic(typeof(StarbornRelicPool))]
public class VegetarianMeatballRelic : StarbornRelic
{
    public override RelicRarity Rarity => RelicRarity.Shop;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new MaxHpVar(10m)];

    public override async Task AfterObtained()
    {
        Creature creature = Owner.Creature;
        await CreatureCmd.GainMaxHp(creature, DynamicVars.MaxHp.BaseValue);
        await CreatureCmd.Heal(creature, creature.MaxHp - creature.CurrentHp);
    }
}
