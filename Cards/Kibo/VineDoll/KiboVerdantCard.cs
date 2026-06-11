using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2_Starborn.Cards.Pile;
using STS2_Starborn.Combat;

namespace STS2_Starborn.Cards.Kibo;

[RegisterCard(typeof(KiboCardPool))]
[KiboAbilityOf(KiboTypeId.VineDoll, true)]
public sealed class KiboVerdantCard() : KiboCard(CardType.Skill, TargetType.AllEnemies)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        KiboKeywords.UltimateKeyword,
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(30m, ValueProp.Move),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(CombatState!)
            .Execute(choiceContext);

        KiboPileManager.RemoveKiboFromCombat(Owner, KiboTypeId.VineDoll);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(15m);
    }
}
