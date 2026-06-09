using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Character;
using STS2_Starborn.Commands;
using STS2_Starborn.Powers;
using MegaCrit.Sts2.Core.Models.Powers;

namespace STS2_Starborn.Cards.Common;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class BackTowardEnemyCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Common, TargetType.None)
{
    public override string? KiboSummonType => KiboTypeId.MuroRabbit;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Ethereal,
        KiboKeywords.KiboKeywordId.GetModCardKeyword(),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<ShacklingPotionPower>();
            yield return KiboTypeRegistry.Get(KiboTypeId.MuroRabbit)
                .CreateCompactCardGridHoverTips();
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("StrengthLoss", 4m),
        new DynamicVar("Repeat", 2m),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await KiboCmd.Summon(choiceContext, Owner, KiboSummonType!);

        if (CombatState == null) return;

        var enemies = CombatState.Enemies.Where(e => e.IsAlive).ToList();
        if (enemies.Count == 0) return;

        var rng = Owner.RunState.Rng.CombatTargets;
        var lossAmount = DynamicVars["StrengthLoss"].BaseValue;
        var times = (int)DynamicVars["Repeat"].BaseValue;

        for (var i = 0; i < times; i++)
        {
            var target = rng.NextItem(enemies);
            if (target == null) continue;

            await PowerCmd.Apply<ShacklingPotionPower>(
                choiceContext, target, lossAmount, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["StrengthLoss"].UpgradeValueBy(1m);
    }
}
