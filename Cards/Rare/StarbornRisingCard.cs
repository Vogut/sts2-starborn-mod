using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;

namespace STS2_Starborn.Cards.Rare;

/// <summary>
/// 星临者立于大地之上：1费技能牌。你每有一层印记获得2点护甲（升级后3点）。
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public class StarbornRisingCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Rare, TargetType.Self
)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(2, ValueProp.Move),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var blockPerStack = DynamicVars.Block.BaseValue;
        var totalStacks = PrimaryStacks + SecondaryStacks;
        var totalBlock = blockPerStack * totalStacks;

        await CreatureCmd.GainBlock(Owner.Creature, totalBlock, ValueProp.Move, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Block"].UpgradeValueBy(1m);
    }
}
