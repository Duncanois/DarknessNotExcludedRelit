using HarmonyLib;
using KMod;
using PeterHan.PLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using System;

namespace DarknessNotIncluded
{
  public class Mod : UserMod2
  {
    public static Version Version = typeof(Mod).Assembly.GetName().Version;

    public override void OnLoad(Harmony harmony)
    {
      base.OnLoad(harmony);

      try
      {
        PUtil.InitLibrary();
      }
      catch (Exception ex)
      {
        Log.Error("Failed to initialize PLib", ex);
      }

      Log.Info($"Bundled PLib version: {PVersion.VERSION}");
      Log.Info($" Active PLib version: {PLibUtils.ActiveVersion()} (via assembly: {PLibUtils.ActiveAssembly().FullName})");

      // Run a compatibility probe early so users can report missing targets.
      CompatibilityProbe.Run();

      // Register options UI / serializer
      Config.Initialize(this);

      // Ensure runtime config instance exists (read saved settings or use defaults)
      try
      {
        var settings = POptions.ReadSettings<Config>();
        if (settings != null)
          Config.instance = settings;
        else if (Config.instance == null)
          Config.instance = new Config();
      }
      catch (Exception ex)
      {
        Log.Warn("Failed to read settings; using defaults", ex);
        if (Config.instance == null) Config.instance = new Config();
      }

      // Initialize custom shapes only when user enabled them in config.
      if (Config.instance.enableCustomLightShapes)
      {
        CustomLightShapes.Initialize();
        Log.Info("Custom light shapes enabled.");
      }
      else
      {
        CustomLightShapes.Enabled = false;
        Log.Info("Custom light shapes disabled by configuration.");
      }
    }
  }
}
