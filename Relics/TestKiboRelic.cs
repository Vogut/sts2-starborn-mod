using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Character;
using STS2_Starborn.Hooks;
using STS2_Starborn.Commands;

namespace STS2_Starborn.Relics;

[RegisterRelic(typeof(StarbornRelicPool))]
public class TestKiboRelic : StarbornRelic, IKiboCardPlayListener
{
    private bool _isReplaying;

    public override RelicRarity Rarity => RelicRarity.Common;

    async Task IKiboCardPlayListener.AfterKiboCardAutoPlayed(CardModel card)
    {
        if (_isReplaying)
            return;

        _isReplaying = true;
        await KiboCmd.AutoPlay(new BlockingPlayerChoiceContext(), card, base.Owner.Creature.CombatState!);
        _isReplaying = false;
    }
}
