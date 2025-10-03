using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PlanetLayerVisualizer;

public class MovementDifficultyVisualLayer :VisualLayer
{
    public override float RetrieveData(PlanetTile tile, Tile selectedTile) {
        if (tile.Tile.PrimaryBiome == BiomeDefOf.Orbit) return 999;
        return WorldPathGrid.CalculatedMovementDifficultyAt(tile, true) *
               Find.WorldGrid.GetRoadMovementDifficultyMultiplier(tile,
                   PlanetTile.Invalid);
    }

}