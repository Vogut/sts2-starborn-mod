using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2_Starborn.Combat;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Token;

public interface IElementChoosable
{
    SealElementType ElementType { get; }
    Task OnChosen(PlayerChoiceContext ctx, Player player, MarkSlot slot);
}
