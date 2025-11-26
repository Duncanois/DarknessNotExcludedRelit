using HarmonyLib;
using System;
using DarknessNotIncluded;

namespace DarknessNotIncluded.Exploration
{
  public static class GridVisibilityExtensions
  {
    public static void SetRadius(this GridVisibility gridVisibility, int radius)
    {
      if (gridVisibility == null) return;
      radius = Math.Max(1, radius);

      if (radius == gridVisibility.radius) return;
      gridVisibility.radius = radius;
      gridVisibility.innerRadius = (float)gridVisibility.radius * 0.7f;
    }

    [HarmonyPatch(typeof(GridVisibility)), HarmonyPatch("OnCellChange")]
    static class Patched_GridVisibility_OnCellChange
    {
      static bool Prefix(GridVisibility __instance)
      {
        if (__instance == null) return false;
        if (__instance.gameObject == null) return false;
        if (__instance.gameObject.HasTag(GameTags.Dead)) return false;

        var originCell = Grid.PosToCell(__instance);
        if (!Grid.IsValidCell(originCell)) return false;

        // LOS-aware reveal when occlusion is enabled, otherwise fallback to vanilla.
        VisibilityUtils.RevealArea(originCell, __instance.radius, __instance.innerRadius);

        return false; // skip original
      }
    }
  }
}
