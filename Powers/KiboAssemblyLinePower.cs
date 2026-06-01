using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.RunRngs;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Cards.Token;
using STS2_Starborn.Hooks;

namespace STS2_Starborn.Powers;

[RegisterPower]
public class KiboAssemblyLinePower : StarbornPower, IKiboCardPlayListener
{
    private static readonly Type[] TokenCardTypes =
    [
        typeof(FragileAxeCard),
        typeof(ElementToolboxCard),
        typeof(LowStarboundCard),
        typeof(SturdyPlankCard),
        typeof(FragilePickaxeCard),
    ];

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: Const.Paths.PowerIcon(GetType()),
        BigIconPath: Const.Paths.PowerBigIcon(GetType())
    );

    public bool ShouldPreventKiboAutoPlay(CardModel card) => true;

    public override async Task AfterSideTurnStart(
        CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        if (!participants.Contains(Owner)) return;

        var player = combatState.Players.FirstOrDefault(p => p.Creature == Owner);
        if (player == null) return;

        Flash();

        var rng = ModRunRngRegistry.Get(player, Const.ModId, "KiboAssemblyLine");
        var count = (int)Amount;
        for (var i = 0; i < count; i++)
        {
            var pickedType = TokenCardTypes[rng.NextInt(0, TokenCardTypes.Length)];
            var canonical = ModelDb.GetById<CardModel>(ModelDb.GetId(pickedType));
            var card = combatState.CreateCard(canonical, player);
            CardCmd.Upgrade(card);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);
        }
    }
}
