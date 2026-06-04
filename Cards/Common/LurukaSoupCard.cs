using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.RunRngs;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Common;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class LurukaSoupCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    private static readonly MarkSlot[] _slots =
    [
        MarkSlot.Primary,
        MarkSlot.Secondary,
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Times", 4m),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var rng = ModRunRngRegistry.Get(Owner, Const.ModId, "luruka_soup");
        var times = DynamicVars["Times"].IntValue;

        for (int i = 0; i < times; i++)
        {
            var slot = _slots[rng.NextInt(0, _slots.Length)];
            var pool = slot == MarkSlot.Primary
                ? ElementPoolRegistry.PrimaryPool
                : ElementPoolRegistry.SecondaryPool;
            var element = pool[rng.NextInt(0, pool.Length)];
            await SealElementMarkCmd.GainElementMarks(choiceContext, slot, Owner, 1, element);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Times"].UpgradeValueBy(2m);
    }
}
