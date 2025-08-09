using RimWorld.Planet;
using UnityEngine;

namespace PlanetLayerVisualizer;

public class MinTempVisualLayer : VisualLayer
{
    public override float RetrieveData(PlanetTile tile, Tile selectedTile) {
        return tile.Tile.MinTemperature;
    }

    public override Color RetrieveColor(float value, VisualizerDef visualDef) {
        if (value <= visualDef.minValue)
            return visualDef.colorStart;
        float intervalValue = (visualDef.maxValue - visualDef.minValue) / visualDef.steps;
        float currentValue = visualDef.minValue;
        for (int i = 0; i < visualDef.steps; i++) {
            if (currentValue > value) {
                Color.RGBToHSV(visualDef.colorStart, out float startHue, out float startSat, out float startVal);
                Color.RGBToHSV(visualDef.colorEnd, out float endHue, out float endSat, out float endVal);

                return Color.HSVToRGB(Mathf.Lerp(startHue, endHue, (float)i / visualDef.steps),
                    Mathf.Lerp(startSat, endSat, (float)i / visualDef.steps),
                    Mathf.Lerp(startVal, endVal, (float)i / visualDef.steps)
                );
            }

            currentValue += intervalValue;
        }

        return visualDef.colorEnd;
    }
}