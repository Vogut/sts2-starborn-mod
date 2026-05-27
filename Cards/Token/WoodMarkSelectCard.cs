using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Token;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class WoodMarkSelectCard() : MarkSelectCard(SealElementType.Wood);
