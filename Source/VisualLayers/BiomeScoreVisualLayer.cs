using RimWorld.Planet;

namespace PlanetLayerVisualizer;

public class BiomeScoreVisualLayer : VisualLayer
{
    public override float RetrieveData(PlanetTile tile, Tile selectedTile) {
        return selectedTile.PrimaryBiome.Worker.GetScore(
            tile.Tile.PrimaryBiome, tile.Tile, tile);
    }
}