using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PlanetLayerVisualizer;

public class MutatorCountVisualLayer : VisualLayer
{
    public override float RetrieveData(PlanetTile tileLookingAt, Tile selectedTile) {
        return tileLookingAt.Tile.Mutators.Count;
    }

    public override Color RetrieveColor(float value, VisualizerDef visualDef, float alphaValue) {
        if (value == 0)
            return Color.clear;
        if (value <= visualDef.minValue)
            return visualDef.colorStart.WithAlpha(alphaValue);
        float intervalValue = (visualDef.maxValue - visualDef.minValue) / visualDef.steps;
        float currentValue = visualDef.minValue;
        for (int i = 0; i < visualDef.steps; i++) {
            if (currentValue > value) {
                return Color.Lerp(visualDef.colorStart, visualDef.colorEnd,
                    (float)i / visualDef.steps).WithAlpha(alphaValue);
            }

            currentValue += intervalValue;
        }

        return visualDef.colorEnd.WithAlpha(alphaValue);
        
    }
}