using RimWorld.Planet;
using Verse;

namespace PlanetLayerVisualizer;

public class MovementDifficultyVisualLayer :VisualLayer
{
    public override float RetrieveData(PlanetTile tile, Tile selectedTile) {
        return WorldPathGrid.CalculatedMovementDifficultyAt(tile, true) *
               Find.WorldGrid.GetRoadMovementDifficultyMultiplier(tile,
                   PlanetTile.Invalid);
    }


}