using RimWorld.Planet;
using UnityEngine;

namespace PlanetLayerVisualizer;

public abstract class VisualLayer
{
    /// <param name="tileLookingAt">The tile that is currently being looped through</param>
    /// <param name="selectedTile">The tile that was selected by the mouse</param>
    public abstract float RetrieveData(PlanetTile tileLookingAt, Tile selectedTile);

    /// <param name="value">Normally retrieved through RetrieveData</param>
    /// <param name="visualDef">Def looking at for values being used</param>
    /// <returns>Color in RGB format</returns>
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