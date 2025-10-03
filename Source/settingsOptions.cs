using System;
using RimWorld;
using RimWorld.Planet;

namespace PlanetLayerVisualizer;

public static class SettingsOptions
{
    public static float Alpha = 1;
    public static float Cutoff=0;
    public static bool InvertCutoff = false;
    public static int ViewLookingAt = 0;
    public static bool ToggleUpdate = true;
    public static bool CacheChanged = false;
    public static Tile TileLookingAt;
    public static bool CountImpassable = false;
}
