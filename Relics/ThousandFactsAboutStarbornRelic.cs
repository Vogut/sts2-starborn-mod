using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Cards;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Relics;

[RegisterRelic(typeof(StarbornRelicPool))]
public class ThousandFactsAboutStarbornRelic : StarbornRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        StarbornCardVars.ElementMark(3, SealElementType.Light)
    ];

    public override async Task BeforeCombatStart()
    {
        Flash();
        var elementType = ((SealElementVar)DynamicVars["ElementMark"]).ElementType;
        await SealElementMarkCmd.GainElementMarks(
            new ThrowingPlayerChoiceContext(),
            MarkSlot.Primary,
            Owner,
            DynamicVars["ElementMark"].IntValue,
            elementType);
    }
}
