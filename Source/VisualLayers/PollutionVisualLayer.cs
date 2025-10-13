using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PlanetLayerVisualizer;

public class PollutionVisualLayer : VisualLayer
{
    public override float RetrieveData(PlanetTile tile, Tile selectedTile) {
        return tile.Tile.pollution;
    }

    public override Color RetrieveColor(float value, VisualizerDef visualDef, float AlphaValue) {
        if (value == 0)
            return Color.clear;
        if (value <= visualDef.minValue)
            return visualDef.colorStart.WithAlpha(AlphaValue);
        float intervalValue = (visualDef.maxValue - visualDef.minValue) / visualDef.steps;
        float currentValue = visualDef.minValue;
        for (int i = 0; i < visualDef.steps; i++) {
            if (currentValue > value) {
                return Color.Lerp(visualDef.colorStart, visualDef.colorEnd,
                    (float)i / visualDef.steps).WithAlpha(AlphaValue);
            }

            currentValue += intervalValue;
        }

        return visualDef.colorEnd.WithAlpha(AlphaValue);
        
    }

}