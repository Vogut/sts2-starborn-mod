using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Character;
using STS2_Starborn.Commands;

namespace STS2_Starborn.Cards.Common;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class RollingImpactCard() : StarbornCard(
    1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    public override string? KiboSummonType => KiboTypeId.ArmoredPangolin;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        KiboKeywords.KiboKeywordId.GetModCardKeyword(),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromCard(
                ModelDb.GetById<CardModel>(ModelDb.GetId<KiboArmoredPangolinRepCard>()));
            var def = KiboTypeRegistry.Get(KiboTypeId.ArmoredPangolin);
            foreach (var tip in def.CreatePlayableCardHoverTips())
                yield return tip;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var combatState = base.CombatState;
        if (combatState == null) return;

        var enemyCount = combatState.HittableEnemies.Count(e => e.IsAlive);
        if (enemyCount == 0) return;

        await KiboCmd.Summon(choiceContext, Owner, KiboSummonType!);

        await StarbornCmd.Bounce(
            choiceContext, Owner, this,
            DynamicVars.Damage.BaseValue,
            enemyCount*2);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
