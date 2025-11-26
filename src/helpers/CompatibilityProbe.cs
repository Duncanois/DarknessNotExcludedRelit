using System;
using System.Reflection;
using HarmonyLib;

namespace DarknessNotIncluded
{
  /// <summary>
  /// Logs presence/absence of critical game types and methods to help diagnose
  /// version drift. Safe to leave in release; minimal overhead.
  /// </summary>
  internal static class CompatibilityProbe
  {
    public static void Run()
    {
      try
      {
        ProbeType("Game");
        ProbeMethod(typeof(Game), "OnPrefabInit");

        ProbeType("RationalAi");
        ProbeMethod(typeof(RationalAi), "InitializeStates");

        ProbeType("ModifierSet");
        ProbeMethod(typeof(ModifierSet), "Initialize");

        ProbeType("SleepChore+States");
        // Nested types must be resolved differently; try both legacy and new names.
        var sleepStates = typeof(SleepChore).GetNestedType("States", BindingFlags.Public | BindingFlags.NonPublic);
        if (sleepStates != null) ProbeMethod(sleepStates, "InitializeStates");
        else Log.Warn("SleepChore.States type missing");

        var sleepStatesInstance = typeof(SleepChore).GetNestedType("StatesInstance", BindingFlags.Public | BindingFlags.NonPublic);
        if (sleepStatesInstance != null) ProbeMethod(sleepStatesInstance, "CheckLightLevel");
        else Log.Warn("SleepChore.StatesInstance type missing");

        ProbeType("World");
        ProbeMethod(typeof(World), "OnSpawn");

        // PLib Options dialog path (reflection based code depends on this)
        var optionsDialog = AccessTools.TypeByName("PeterHan.PLib.Options.OptionsDialog");
        if (optionsDialog == null)
          Log.Warn("OptionsDialog type not found; presets injection may fail");
        else
          Log.Info("OptionsDialog type OK: {0}", optionsDialog.FullName);
      }
      catch (Exception ex)
      {
        Log.Error("Compatibility probe failed", ex);
      }
    }

    private static void ProbeType(string typeName)
    {
      var type = AccessTools.TypeByName(typeName);
      if (type == null) Log.Warn("Type missing: {0}", typeName); else Log.Info("Type present: {0}", typeName);
    }

    private static void ProbeMethod(Type type, string methodName)
    {
      if (type == null) return;
      var method = AccessTools.Method(type, methodName);
      if (method == null)
        Log.Warn("Method missing: {0}.{1}", type.Name, methodName);
      else
        Log.Info("Method present: {0}.{1}()", type.Name, methodName);
    }
  }
}
