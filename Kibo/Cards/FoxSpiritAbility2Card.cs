using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_Starborn.Kibo.Cards;

[RegisterCard(typeof(KiboCardPool))]
public sealed class FoxSpiritAbility2Card() : ModCardTemplate(
    0, CardType.Skill, CardRarity.Token, TargetType.Self, showInCardLibrary: false)
{
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: Const.Paths.KiboCardPortrait(GetType()));

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(4, ValueProp.Unpowered),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }
}
