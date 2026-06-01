using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace STS2_Starborn.UI;

/// <summary>
/// Godot Control that renders card previews in a 2-column grid at reduced scale.
/// Used by CompactCardGridHoverTipPatch to display multi-card hover tips compactly.
/// </summary>
public partial class NCompactCardGrid : Control
{
    private const float CardScale = 1f;
    private const float Gap = 4f;
    private const float FallbackCardWidth = 164f;
    private const float FallbackCardHeight = 230f;

    public void Setup(IReadOnlyList<CardModel> cards)
    {
        if (cards.Count == 0) return;

        float maxW = 0f;
        float maxH = 0f;

        for (var i = 0; i < cards.Count; i++)
        {
            var tip = PreloadManager.Cache
                .GetScene("res://scenes/ui/card_hover_tip.tscn")
                .Instantiate<Control>();
            AddChild(tip);

            var nCard = tip.GetNode<NCard>("%Card");
            nCard.Model = cards[i];
            nCard.UpdateVisuals(PileType.Deck, CardPreviewMode.Normal);
            tip.ResetSize();

            var baseW = tip.Size.X > 1f ? tip.Size.X : FallbackCardWidth;
            var baseH = tip.Size.Y > 1f ? tip.Size.Y : FallbackCardHeight;

            tip.Scale = new Vector2(CardScale, CardScale);

            var col = i % 2;
            var row = i / 2;
            var visW = baseW * CardScale;
            var visH = baseH * CardScale;

            tip.Position = new Vector2(col * (visW + Gap), row * (visH + Gap));

            maxW = Mathf.Max(maxW, tip.Position.X + visW);
            maxH = Mathf.Max(maxH, tip.Position.Y + visH);
        }

        Size = new Vector2(maxW, maxH);
    }
}
