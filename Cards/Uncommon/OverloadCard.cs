using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Commands;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Uncommon;

[RegisterCard(typeof(StarbornCardPool))]
public class OverloadCard() : StarbornCard(
    2, CardType.Skill, CardRarity.Uncommon, TargetType.None
)
{
    protected override bool IsPlayable => PrimaryMark != null;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        StarbornCardVars.Overload(10, SealElementType.Fire),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SealElementMarkCmd.SetElementType(choiceContext, PrimaryMark!, SealElementType.Fire);
        var need = SealElementMarkPower.ThresholdStacks - PrimaryMark!.DisplayAmount;
        if (need > 0)
            await SealElementMarkCmd.GainElementMarks(choiceContext, PrimaryMark, need, this);
        await StarbornCmd.Overload(choiceContext, PrimaryMark, 1, this);
        var remain = SealElementMarkPower.MaxSealStacks - PrimaryMark.DisplayAmount;
        if (remain > 0)
            await SealElementMarkCmd.GainElementMarks(choiceContext, PrimaryMark, remain, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Overload"].BaseValue += 5; // 10 → 15
    }
}
