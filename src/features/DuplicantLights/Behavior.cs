using DarknessNotIncluded.Exploration;
using System;
using DarknessNotIncluded;

namespace DarknessNotIncluded.DuplicantLights
{
  public static class Behavior
  {
    public abstract class UnitLights : KMonoBehaviour, ISim33ms
    {
      private static bool disableLightsInBedrooms;
      private static bool disableLightsInLitAreas;
      private static MinionLightingConfig minionLightingConfig;
      private static bool occludeVisibilityByWalls;

      private static Config.Observer configObserver = new Config.Observer((config) =>
      {
        disableLightsInBedrooms = config.disableDupeLightsInBedrooms;
        disableLightsInLitAreas = config.disableDupeLightsInLitAreas;
        minionLightingConfig = config.minionLightingConfig;
        occludeVisibilityByWalls = config.occludeVisibilityByWalls;
      });

      [MyCmpGet]
      private GridVisibility gridVisibility;

      public Light2D Light { get; set; }

      private MinionLightType currentLightType = MinionLightType.None;

      protected override void OnPrefabInit()
      {
        base.OnPrefabInit();

        Light = gameObject.AddComponent<Light2D>();

        // Ensure GridVisibility exists on all units (including preview/minion select)
        gridVisibility = gameObject.AddOrGet<GridVisibility>();

        Config.ObserveFor(this, (config) =>
        {
          UpdateLights(true);
        });
      }

      protected override void OnSpawn()
      {
        base.OnSpawn();
        UpdateLights();
      }

      public void Sim33ms(float dt)
      {
        UpdateLights();
      }

      private void UpdateLights(bool force = false)
      {
        if (gameObject == null) return;

        var lightType = GetActiveLightType(minionLightingConfig);
        var lightConfig = minionLightingConfig.Get(lightType);

        // Simple circular reveal only (respect config via RevealArea)
        if (gridVisibility != null)
        {
          gridVisibility.SetRadius(lightConfig.reveal);
          VisibilityUtils.RevealArea(Grid.PosToCell(gameObject), gridVisibility.radius, gridVisibility.innerRadius);
        }

        if (disableLightsInBedrooms && lightType != MinionLightType.None)
        {
          if (MinionRoomState.SleepersInSameRoom(gameObject))
          {
            lightType = MinionLightType.None;
          }
        }

        if (disableLightsInLitAreas && lightType != MinionLightType.None)
        {
          var cell = Grid.PosToCell(gameObject);
          var cellLux = Grid.IsValidCell(cell) ? Grid.LightIntensity[cell] : 0;

          var dupeLux = Light.enabled ? Light.Lux : 0;
          var baseCellLux = Math.Max(0, cellLux - dupeLux);
          var targetLux = lightConfig.lux;
          if (lightType == MinionLightType.Intrinsic) targetLux *= 2;

          if (baseCellLux >= targetLux)
          {
            lightType = MinionLightType.None;
          }
        }

        SetLightType(lightType, force);
      }

      private void SetLightType(MinionLightType lightType, bool force)
      {
        if (lightType == currentLightType && !force) return;
        currentLightType = lightType;
        minionLightingConfig.Get(lightType).ConfigureLight(Light);
      }

      protected abstract MinionLightType GetActiveLightType(MinionLightingConfig minionLightingConfig);
    }
  }
}
