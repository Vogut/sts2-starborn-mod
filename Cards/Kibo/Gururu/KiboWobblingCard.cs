using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace STS2_Starborn.Cards.Kibo;

[RegisterCard(typeof(KiboCardPool))]
[KiboAbilityOf(KiboTypeId.Gururu)]
public sealed class KiboWobblingCard() : KiboCard(1, CardType.Attack, TargetType.AnyEnemy)
{
    public override bool GainsBlock => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        KiboKeywords.NormalKeyword,
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6, ValueProp.Move),
        new BlockVar(5, ValueProp.Move),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var damage = DynamicVars.Damage.IntValue;
        var block = DynamicVars.Block.IntValue;

        // 随机选择：0 = 伤害，1 = 格挡
        var choice = Owner.RunState.Rng.CombatCardSelection.NextInt(2);

        if (choice == 0)
        {
            // 造成伤害
            await DamageCmd.Attack(damage)
                .FromCard(this)
                .Targeting(cardPlay.Target!)
                .Execute(choiceContext);
        }
        else
        {
            // 获得格挡
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6m);
        DynamicVars.Block.UpgradeValueBy(5m);
    }
}
