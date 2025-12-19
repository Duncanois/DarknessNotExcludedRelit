using HarmonyLib;
using System;
using System.Collections.Generic;
using ProcGen;
using UnityEngine;

namespace DarknessNotIncluded.Exploration
{
  public static class RevealAllOfSpace
  {
    static HashSet<int> REVEALED_CELLS = new HashSet<int>();

    [HarmonyPatch(typeof(World)), HarmonyPatch("OnSpawn")]
    static class Patched_World_OnSpawn
    {
      static void Postfix()
      {
        Grid.OnReveal += (targetCell) =>
        {
          try
          {
            // Basic guards
            if (!Grid.IsValidCell(targetCell)) return;
            if (REVEALED_CELLS.Contains(targetCell)) return;
            if (Game.Instance == null || Game.Instance.world == null) return;
            if (ClusterManager.Instance == null) return;

            // Ensure world index is valid and world exists
            int worldIdx;
            try
            {
              worldIdx = Grid.WorldIdx[targetCell];
            }
            catch
            {
              return;
            }
            if (worldIdx < 0) return;

            var world = ClusterManager.Instance.GetWorld(worldIdx);
            if (world == null) return;

            // Only proceed for space + sunlight cells
            if (!IsSpaceBiomeAndLitBySunlight(targetCell)) return;

            var maxSize = world.WorldSize.x * world.WorldSize.y;
            var cells = GameUtil.FloodCollectCells(targetCell, IsSpaceBiomeAndLitBySunlight, maxSize);

            foreach (var cell in cells)
            {
              if (!Grid.IsValidCell(cell)) continue;
              // Add returns false if already present; avoids duplicate reveals
              if (!REVEALED_CELLS.Add(cell)) continue;
              try
              {
                Grid.Reveal(cell);
              }
              catch (Exception ex)
              {
                Debug.LogWarning($"[DarknessNotIncluded] Grid.Reveal failed for cell {cell}: {ex}");
              }
            }
          }
          catch (Exception ex)
          {
            Debug.LogWarning($"[DarknessNotIncluded] RevealAllOfSpace OnReveal handler threw: {ex}");
          }
        };
      }

      static bool IsSpaceBiomeAndLitBySunlight(int cell)
      {
        // Fail-safe checks
        if (!Grid.IsValidCell(cell)) return false;
        if (Game.Instance == null || Game.Instance.world == null) return false;

        var zoneRender = Game.Instance.world.zoneRenderData;
        if (zoneRender == null) return false;

        SubWorld.ZoneType zoneType;
        try
        {
          zoneType = zoneRender.GetSubWorldZoneType(cell);
        }
        catch
        {
          return false;
        }

        var isSpaceBiome = zoneType == SubWorld.ZoneType.Space;

        bool isSpaceCell = false;
        try
        {
          // guard access to Grid.Objects
          isSpaceCell = Grid.Objects[cell, 2] == null;
        }
        catch
        {
          return false;
        }

        bool isLitBySunlight = false;
        try
        {
          isLitBySunlight = Grid.ExposedToSunlight[cell] > 0;
        }
        catch
        {
          return false;
        }

        return isSpaceBiome && isSpaceCell && isLitBySunlight;
      }
    }
  }
}
