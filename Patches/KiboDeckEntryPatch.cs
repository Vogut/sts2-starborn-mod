using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Patching.Models;
using STS2_Starborn.Cards;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Cards.Pile;
using STS2_Starborn.Runs;

namespace STS2_Starborn.Patches;

public sealed class KiboDeckEntryPatch : IPatchMethod
{
    public static string PatchId => "sts2_starborn_kibo_deck_entry";
    public static string Description => "Create Kibo master cards when a summon card enters the player deck";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return
        [
            new(typeof(Hook), nameof(Hook.AfterCardChangedPiles),
                [typeof(IRunState), typeof(ICombatState), typeof(CardModel), typeof(PileType), typeof(AbstractModel)]),
        ];
    }

    public static void Postfix(CardModel card)
    {
        if (card.Pile?.Type != PileType.Deck) return;
        if (card is not StarbornCard { KiboSummonType: { } typeId }) return;
        if (card.Owner == null) return;

        KiboRunData.Modify(card.Owner, data =>
        {
            data.OwnedKiboTypeIds.Add(typeId.ToString());
            data.ActiveKiboTypeId = typeId.ToString();
        });
        TaskHelper.RunSafely(KiboPileManager.CreateMasterCards(card.Owner, typeId));
    }
}
