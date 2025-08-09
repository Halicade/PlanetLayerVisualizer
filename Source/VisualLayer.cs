using RimWorld.Planet;
using UnityEngine;

namespace PlanetLayerVisualizer;

public abstract class VisualLayer
{
    public abstract float RetrieveData(PlanetTile tileLookingAt, Tile selectedTile);

    public virtual Color RetrieveColor(float value, VisualizerDef visualDef) {

        if (value <= visualDef.minValue)
            return visualDef.colorStart;
        float intervalValue = (visualDef.maxValue - visualDef.minValue) / visualDef.steps;
        float currentValue = visualDef.minValue;
        for (int i = 0; i < visualDef.steps; i++) {
            if (currentValue > value) {
                return Color.Lerp(visualDef.colorStart, visualDef.colorEnd,
                    (float)i / visualDef.steps);
            }

            currentValue += intervalValue;
        }

        return visualDef.colorEnd;
    }
}