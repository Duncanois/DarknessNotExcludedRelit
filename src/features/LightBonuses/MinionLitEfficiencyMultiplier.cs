using System.Collections.Generic;
using HarmonyLib;
using System.Reflection.Emit;
using TUNING;

namespace DarknessNotIncluded.LightBonuses
{
  public static class MinionLitEfficiencyMultiplier
  {
    private static int litWorkspaceLux;

    private static Config.Observer configObserver = new Config.Observer((config) =>
    {
      litWorkspaceLux = config.litWorkspaceLux;
    });

    [HarmonyPatch(typeof(Workable))]
    [HarmonyPatch("GetEfficiencyMultiplier")]
    static class Workable_GetEfficiencyMultiplier_Patch
    {
      static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
      {
        var indexLightIntensity = typeof(Grid.LightIntensityIndexer).GetMethod("get_Item");
        var newInstructions = new List<CodeInstruction>(instructions);
        for (var i = 1; i < newInstructions.Count - 1; i++)
        {
          //Debug.Log("T:" + i + ":" + newInstructions[i].opcode + "::"
          //    + (newInstructions[i].operand != null ? newInstructions[i].operand.ToString() : newInstructions[i].operand));
          // We're searching for:
          // 
          //  if (Grid.LightIntensity[num2] > DUPLICANTSTATS.STANDARD.Light.NO_LIGHT)
          //
          // To be somewhat flexible, check only for the NO_LIGHT part.
          //
          // IL:
          //
          //   ldfld System.Int32 NO_LIGHT
          //   ble.s Label3
          var currInstruction = newInstructions[i];
          var nextInstruction = newInstructions[i + 1];

          if (currInstruction.opcode == OpCodes.Ldfld && currInstruction.operand.ToString() == "System.Int32 NO_LIGHT"
           && nextInstruction.opcode == OpCodes.Ble_S)
          {
            // Rewrite to:
            //
            //  if (Grid.LightIntensity[num2] > Workable_GetEfficiencyMultiplier_Patch.GetLitWorkspaceLuxForPatch(DUPLICANTSTATS.STANDARD.Light))
            //
            // (The function taking the part before NO_LIGHT as the argument discards it, which makes adjusting the IL simpler.)
            //
            // IL:
            //
            //   call static Workable_GetEfficiencyMultiplier_Patch Workable_GetEfficiencyMultiplier_Patch.GetLitWorkspaceLuxForPatch()
            //   ble.s Label3
            newInstructions[i] = CodeInstruction.Call(typeof(Workable_GetEfficiencyMultiplier_Patch), "GetLitWorkspaceLuxForPatch");
            return newInstructions;
          }
        }

        Debug.LogError("DarknessNotExcluded: Failed to patch Workable.GetEfficiencyMultiplier()");
        return newInstructions;
      }

      static int GetLitWorkspaceLuxForPatch(DUPLICANTSTATS.LIGHT dummy)
      {
        return litWorkspaceLux - 1;
      }
    }
  }
}
