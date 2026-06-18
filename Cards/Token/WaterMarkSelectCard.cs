using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Element;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace STS2_Starborn.Cards.Token;

[RegisterCard(typeof(TokenCardPool))]
public sealed class WaterMarkSelectCard() : MarkSelectCard(SealElementType.Water);
