using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Token;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class ElementToolboxCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Token, TargetType.None
)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        StarbornCardVars.ElementMark(1, SealElementType.None, name: "Primary"),
        StarbornCardVars.ElementMark(1, SealElementType.None, name: "Secondary"),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SealElementMarkCmd.GainElementMarks(choiceContext, MarkSlot.Primary, Owner, DynamicVars["Primary"].IntValue);
        await SealElementMarkCmd.GainElementMarks(choiceContext, MarkSlot.Secondary, Owner, DynamicVars["Secondary"].IntValue);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
