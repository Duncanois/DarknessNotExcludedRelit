using System;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;

class Program
{
  static int Main(string[] args)
  {
    var game = @"E:\SteamLibrary\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed";
    AppDomain.CurrentDomain.AssemblyResolve += (sender, e) => {
      var path = Path.Combine(game, new AssemblyName(e.Name).Name + ".dll");
      return File.Exists(path) ? Assembly.LoadFrom(path) : null;
    };
    var assembly = Assembly.LoadFrom(Path.GetFullPath(args[0]));
    int count = 0, failed = 0;
    Type[] types;
    try { types = assembly.GetTypes(); }
    catch (ReflectionTypeLoadException e) {
      foreach (var error in e.LoaderExceptions.Select(x => x.Message).Distinct()) Console.WriteLine("LOADER: " + error);
      types = e.Types.Where(t => t != null).ToArray();
    }
    foreach (var type in types.Where(t => t.Namespace != null && t.Namespace.StartsWith("DarknessNotIncluded")))
    {
      var attrs = type.GetCustomAttributes(typeof(HarmonyPatch), false).Cast<HarmonyPatch>().ToArray();
      if (attrs.Length == 0) continue;
      count++;
      try {
        MethodBase target;
        var custom = type.GetMethod("TargetMethod", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (custom != null) target = (MethodBase)custom.Invoke(null, null);
        else {
          var info = HarmonyMethod.Merge(attrs.Select(a => a.info).ToList());
          target = AccessTools.Method(info.declaringType, info.methodName, info.argumentTypes);
        }
        if (target == null) throw new Exception("Target missing");
        var prefix = type.GetMethod("Prefix", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        if (prefix != null) {
          foreach (var p in prefix.GetParameters()) {
            if (p.Name.StartsWith("__")) continue;
            if (!target.GetParameters().Any(t => t.Name == p.Name)) throw new Exception("Prefix parameter missing: " + p.Name);
          }
        }
        Console.WriteLine("OK " + type.FullName + " -> " + target.DeclaringType.FullName + "." + target.Name);
      } catch (Exception e) { failed++; Console.WriteLine("FAIL " + type.FullName + ": " + e); }
    }
    Console.WriteLine("Checked " + count + " patch classes; failures: " + failed);
    return failed == 0 ? 0 : 1;
  }
}
