using System;
using RimWorld;
using RimWorld.Planet;

namespace PlanetLayerVisualizer;

public class SettingsOptions
{
    public float Alpha = 1;
    public float Cutoff;
    public bool CutoffEnabled = true;
    public bool InvertCutoff = false;
    public int ViewLookingAt = 0;
    public bool ToggleUpdate = true;
    public bool CacheChanged = false;
    public Tile TileLookingAt;
    public bool countImpassable = false;



    public SettingsOptions() {
        Cutoff = 0;
    }
}
