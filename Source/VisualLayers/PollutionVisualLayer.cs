using RimWorld.Planet;

namespace PlanetLayerVisualizer;

public class PollutionVisualLayer : VisualLayer
{
    public override float RetrieveData(PlanetTile tile, Tile selectedTile) {
        return tile.Tile.pollution;
    }


}