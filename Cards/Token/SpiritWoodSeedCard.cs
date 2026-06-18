using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Commands;
using STS2_Starborn.Combat;
using STS2_Starborn.Element;
using STS2_Starborn.Hooks;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace STS2_Starborn.Cards.Token;

[RegisterCard(typeof(TokenCardPool))]
public sealed class SpiritWoodSeedCard() : StarbornCard(
    0, CardType.Skill, CardRarity.Token, TargetType.Self
)
{
    private const int BaseConsume = 1;

    protected override bool IsPlayable
    {
        get
        {
            var stacks = SecondaryStacks;
            var consume = stacks > ElementMarkState.ThresholdStacks
                ? DynamicVars["Overload"].IntValue
                : DynamicVars["Tuning"].IntValue;

            if (CombatState != null)
            {
                consume = stacks > ElementMarkState.ThresholdStacks
                    ? SealElementMarkHooks.ModifyOverloadConsume(CombatState, MarkSlot.Secondary, consume)
                    : SealElementMarkHooks.ModifyTuningConsume(CombatState, MarkSlot.Secondary, consume);
            }

            return consume >= 0 && stacks >= consume;
        }
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust, CardKeyword.Ethereal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        Tuning(BaseConsume, SealElementType.Wood, "Tuning", MarkSlot.Secondary),
        Overload(BaseConsume + 1, SealElementType.Wood, "Overload", MarkSlot.Secondary),
        StarbornCardVars.IfCanOverload(MarkSlot.Secondary)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (SecondaryStacks > ElementMarkState.ThresholdStacks)
        {
            var overloadElementType = ((SealElementVar)DynamicVars["Overload"]).ElementType;
            await StarbornCmd.Overload(choiceContext, MarkSlot.Secondary, Owner,
                DynamicVars["Overload"].IntValue, overloadElementType, this);
        }
        else
        {
            var tuningElementType = ((SealElementVar)DynamicVars["Tuning"]).ElementType;
            await StarbornCmd.Tuning(choiceContext, MarkSlot.Secondary, Owner,
                DynamicVars["Tuning"].IntValue, tuningElementType, this);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
    }
}
