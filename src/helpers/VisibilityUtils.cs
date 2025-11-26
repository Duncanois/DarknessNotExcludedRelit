using System;
using System.Collections.Generic;

namespace DarknessNotIncluded
{
  public static class VisibilityUtils
  {
    // Bresenham line generator (cells inclusive)
    public static IEnumerable<int> CellsOnLine(int x0, int y0, int x1, int y1)
    {
      int dx = Math.Abs(x1 - x0);
      int dy = -Math.Abs(y1 - y0);
      int sx = x0 < x1 ? 1 : -1;
      int sy = y0 < y1 ? 1 : -1;
      int err = dx + dy;
      int x = x0;
      int y = y0;

      while (true)
      {
        yield return Grid.XYToCell(x, y);
        if (x == x1 && y == y1) yield break;
        int e2 = 2 * err;
        if (e2 >= dy)
        {
          err += dy;
          x += sx;
        }
        if (e2 <= dx)
        {
          err += dx;
          y += sy;
        }
      }
    }

    // Lightweight blocking test: solids and common building object on foundation layer
    public static bool IsBlockingCell(int cell)
    {
      if (!Grid.IsValidCell(cell)) return false;
      if (Grid.Solid[cell]) return true;

      // Extra guard for building-type blockers
      var obj = Grid.Objects[cell, (int)ObjectLayer.Building];
      if (obj != null) return true;

      return false;
    }

    // Reveal cells from origin with LOS; blocked cells are only revealed if already lit or visible.
    public static void RevealWithLineOfSight(int origin, int radius)
    {
      if (!Grid.IsValidCell(origin)) return;

      var originXY = Grid.CellToXY(origin);
      int ox = originXY.x;
      int oy = originXY.y;

      int r = Math.Max(1, radius);
      int minx = Math.Max(0, ox - r);
      int maxx = Math.Min(Grid.WidthInCells - 1, ox + r);
      int miny = Math.Max(0, oy - r);
      int maxy = Math.Min(Grid.HeightInCells - 1, oy + r);

      for (int y = miny; y <= maxy; y++)
      {
        for (int x = minx; x <= maxx; x++)
        {
          int cell = Grid.XYToCell(x, y);
          if (!Grid.IsValidCell(cell)) continue;
          if (cell == origin) continue;
          if (Math.Abs(x - ox) + Math.Abs(y - oy) <= 1) continue; // immediate neighbours handled elsewhere
          if (x - ox == 0 && y - oy == 0) continue;
          if ((x - ox) * (x - ox) + (y - oy) * (y - oy) > radius * radius) continue;

          bool blocked = false;
          foreach (int stepCell in CellsOnLine(ox, oy, x, y))
          {
            if (stepCell == origin) continue;
            if (stepCell == cell) break;
            if (IsBlockingCell(stepCell))
            {
              blocked = true;
              break;
            }
          }

          if (blocked)
          {
            if (Grid.LightIntensity[cell] > 0 || Grid.Visible[cell] > 0 || Grid.ExposedToSunlight[cell] > 0)
            {
              Grid.Reveal(cell);
            }
          }
          else
          {
            Grid.Reveal(cell);
          }
        }
      }
    }
  }
}