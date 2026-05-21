using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Hooks;

namespace STS2_Starborn.Relics;

[RegisterRelic(typeof(StarbornRelicPool))]
public class TuningArmorRelic : StarbornRelic, ITuningOverloadListener
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public async Task AfterTuning(PlayerChoiceContext ctx, MarkSlot slot, int consume, CardModel? source)
    {
        await CreatureCmd.GainBlock(Owner.Creature, 1m, ValueProp.Unpowered, null);
    }
}
