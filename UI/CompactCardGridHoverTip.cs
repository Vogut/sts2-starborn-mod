using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace STS2_Starborn.UI;

/// <summary>
/// Custom IHoverTip that bundles multiple cards for compact grid display.
/// Intercepted by CompactCardGridHoverTipPatch before NHoverTipSet.Init() casts.
/// </summary>
public class CompactCardGridHoverTip : IHoverTip
{
    public IReadOnlyList<CardModel> Cards { get; }

    public string Id { get; }

    public bool IsSmart => false;
    public bool IsDebuff => false;
    public bool IsInstanced => false;
    public AbstractModel? CanonicalModel => null;

    public CompactCardGridHoverTip(IEnumerable<CardModel> cards)
    {
        Cards = cards.ToList();
        Id = "sts2_starborn:compact_grid:" +
             string.Join(",", Cards.Select(c => c.Id.ToString()).Order());
    }
}
