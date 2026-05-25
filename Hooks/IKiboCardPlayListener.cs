using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace STS2_Starborn.Hooks;

public interface IKiboCardPlayListener
{
    Task BeforeKiboRandomAutoPlay(CardModel card, string keywordId) => Task.CompletedTask;
    Task AfterKiboRandomAutoPlay(CardModel card, string keywordId) => Task.CompletedTask;

    Task BeforeKiboCardAutoPlayed(CardModel card) => Task.CompletedTask;
    Task AfterKiboCardAutoPlayed(CardModel card) => Task.CompletedTask;
}
