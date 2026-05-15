using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;
using Starborn.Character;
using Starborn.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Starborn.Card.Common;

/// <summary>
/// 测试卡牌：将主属性印记切换为火属性并叠加印记层数。
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public class SwitchPrimaryMarkCard : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Starborn/cards/{GetType().Name}.png"
    );
    
    // 叠加印记层数，初始值为2，升级后变为3
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Magic", 2)];
    
    public SwitchPrimaryMarkCard() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var mark = Owner.Creature.FindPower<PrimaryMarkPower>();
        if (mark != null)
        {
            mark.CurrentAttribute = SealAttribute.Fire;
            await PowerCmd.Apply<PrimaryMarkPower>(choiceContext, Owner.Creature, DynamicVars["Magic"].IntValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Magic"].BaseValue += 1; // 2 → 3
    }
}
