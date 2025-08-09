using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PlanetLayerVisualizer;

public class RainfallVisualLayer : VisualLayer
{
    public override float RetrieveData(PlanetTile tile, Tile selectedTile) {
        return tile.Tile.rainfall;
    }
}