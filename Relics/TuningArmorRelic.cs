using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Hooks;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Relics;

[RegisterRelic(typeof(StarbornRelicPool))]
public class TuningArmorRelic : StarbornRelic, ITuningOverloadListener
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public async Task AfterTuning(PlayerChoiceContext ctx, SealElementMarkPower mark, int consume, Creature owner, CardModel? source)
    {
        await CreatureCmd.GainBlock(Owner.Creature, 1m, ValueProp.Unpowered, null);
    }
}
