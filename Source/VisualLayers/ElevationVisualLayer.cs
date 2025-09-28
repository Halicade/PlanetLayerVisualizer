using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace PlanetLayerVisualizer;

public class ElevationVisualLayer : VisualLayer
{
    public override float RetrieveData(PlanetTile tile, Tile selectedTile) {
        return tile.Tile.elevation;
    }
}