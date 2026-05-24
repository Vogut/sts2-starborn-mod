using MegaCrit.Sts2.Core.Map;

namespace STS2_Starborn.Map;

public sealed class KiboActMap : ActMap
{
    public override MapPoint BossMapPoint { get; }
    public override MapPoint StartingMapPoint { get; }
    protected override MapPoint?[,] Grid { get; }

    public KiboActMap(ActMap original)
    {
        int oldRows = original.GetRowCount();
        int newRows = oldRows + 1;
        Grid = new MapPoint[7, newRows];

        StartingMapPoint = original.StartingMapPoint;

        var kiboNode = new MapPoint(3, 1)
        {
            PointType = MapPointType.Unknown,
        };
        Grid[3, 1] = kiboNode;

        for (int r = 1; r < oldRows; r++)
        for (int c = 0; c < 7; c++)
        {
            var pt = original.GetPoint(c, r);
            if (pt != null)
            {
                pt.coord.row = r + 1;
                Grid[c, r + 1] = pt;
            }
        }

        foreach (var child in StartingMapPoint.Children.ToList())
            StartingMapPoint.RemoveChildPoint(child);
        StartingMapPoint.AddChildPoint(kiboNode);

        for (int c = 0; c < 7; c++)
            if (Grid[c, 2] is { } child)
                kiboNode.AddChildPoint(child);

        BossMapPoint = original.BossMapPoint;
        BossMapPoint.coord.row = newRows;

        startMapPoints.Add(kiboNode);
    }
}
