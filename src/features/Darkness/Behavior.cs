using System;
using DarknessNotIncluded;

namespace DarknessNotIncluded.Darkness
{
  public enum InspectionLevel
  {
    None,
    BasicDetails,
    FullDetails,
  }

  public static class Behavior
  {
    public static bool enabled = true;

    private static bool selectToolBlockedByDarkness;
    private static float gracePeriodCycles;
    private static bool occludeVisibilityByWalls;

    private static Config.Observer configObserver = new Config.Observer((config) =>
    {
      selectToolBlockedByDarkness = config.selectToolBlockedByDarkness;
      gracePeriodCycles = config.gracePeriodCycles;
      occludeVisibilityByWalls = config.occludeVisibilityByWalls;
    });

    static public int ActualOrImpliedLightLevel(int cell)
    {
      if (!Grid.IsValidCell(cell)) return 0;

      var cellLux = Grid.LightIntensity[cell];
      if (cellLux > 0) return cellLux;

      // Adjacent cells (75%)
      var nearbyLux = 0;
      ConsiderLux(ref nearbyLux, cell, Grid.CellAbove(cell));
      ConsiderLux(ref nearbyLux, cell, Grid.CellRight(cell));
      ConsiderLux(ref nearbyLux, cell, Grid.CellBelow(cell));
      ConsiderLux(ref nearbyLux, cell, Grid.CellLeft(cell));
      if (nearbyLux > 0) return (nearbyLux * 3) / 4;

      // Mid cells (50%)
      var midLux = 0;
      ConsiderLux(ref midLux, cell, Grid.CellUpRight(cell));
      ConsiderLux(ref midLux, cell, Grid.CellDownRight(cell));
      ConsiderLux(ref midLux, cell, Grid.CellDownLeft(cell));
      ConsiderLux(ref midLux, cell, Grid.CellUpLeft(cell));
      ConsiderLux(ref midLux, cell, Grid.CellAbove(Grid.CellAbove(cell)));
      ConsiderLux(ref midLux, cell, Grid.CellRight(Grid.CellRight(cell)));
      ConsiderLux(ref midLux, cell, Grid.CellBelow(Grid.CellBelow(cell)));
      ConsiderLux(ref midLux, cell, Grid.CellLeft(Grid.CellLeft(cell)));
      if (midLux > 0) return midLux / 2;

      // Far cells (25%)
      var farLux = 0;
      ConsiderLux(ref farLux, cell, Grid.CellUpRight(Grid.CellAbove(cell)));
      ConsiderLux(ref farLux, cell, Grid.CellUpRight(Grid.CellRight(cell)));
      ConsiderLux(ref farLux, cell, Grid.CellDownRight(Grid.CellRight(cell)));
      ConsiderLux(ref farLux, cell, Grid.CellDownRight(Grid.CellBelow(cell)));
      ConsiderLux(ref farLux, cell, Grid.CellDownLeft(Grid.CellBelow(cell)));
      ConsiderLux(ref farLux, cell, Grid.CellDownLeft(Grid.CellLeft(cell)));
      ConsiderLux(ref farLux, cell, Grid.CellUpLeft(Grid.CellLeft(cell)));
      ConsiderLux(ref farLux, cell, Grid.CellUpLeft(Grid.CellAbove(cell)));
      if (farLux > 0) return farLux / 4;

      return 0;
    }

    // Consider lux from `candidateCell` for origin `originCell` only if the candidate
    // cell is lit and not blocked from `originCell` by a blocking cell.
    static private void ConsiderLux(ref int maxLux, int originCell, int candidateCell)
    {
      if (!Grid.IsValidCell(candidateCell)) return;
      var candidateLux = Grid.LightIntensity[candidateCell];
      if (candidateLux <= 0) return;

      // If origin equals candidate, accept immediately.
      if (originCell == candidateCell)
      {
        maxLux = Math.Max(maxLux, candidateLux);
        return;
      }

      var originXY = Grid.CellToXY(originCell);
      var candidateXY = Grid.CellToXY(candidateCell);
      int ox = originXY.x, oy = originXY.y, cx = candidateXY.x, cy = candidateXY.y;

      bool blocked = false;
      foreach (var stepCell in VisibilityUtils.CellsOnLine(ox, oy, cx, cy))
      {
        if (stepCell == originCell) continue;
        if (stepCell == candidateCell) break;
        if (VisibilityUtils.IsBlockingCell(stepCell))
        {
          blocked = true;
          break;
        }
      }

      if (!blocked)
      {
        maxLux = Math.Max(maxLux, candidateLux);
      }
    }

    static public InspectionLevel InspectionLevelForCell(int cell)
    {
      if (!Grid.IsValidCell(cell)) return InspectionLevel.None;

      if (!Darkness.Behavior.enabled) return InspectionLevel.FullDetails;
      if (DebugHandler.RevealFogOfWar) return InspectionLevel.FullDetails;
      if (Grid.Visible[cell] <= 0) return InspectionLevel.None;

      if (!selectToolBlockedByDarkness) return InspectionLevel.FullDetails;
      if (GameClock.Instance.GetTimeInCycles() < gracePeriodCycles) return InspectionLevel.FullDetails;

      var lux = ActualOrImpliedLightLevel(cell);
      // Treat unlit tiles as unknown when selection is blocked by darkness
      return lux > 0 ? InspectionLevel.FullDetails : InspectionLevel.None;
    }
  }
}
