using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace PlanetLayerVisualizer;

public class LandmarkVisualLayer : VisualLayer
{
    public override float RetrieveData(PlanetTile tile, Tile selectedTile) {
        return tile.Tile.Landmark!=null ? 1 : 0;
    }

    public override Color RetrieveColor(float value, VisualizerDef visualDef, float AlphaValue) {
        return value==0 ? Color.clear : visualDef.colorEnd.WithAlpha(AlphaValue);
    }
}