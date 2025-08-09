using RimWorld;

namespace PlanetLayerVisualizer;

[DefOf]
public static class VisualizerDefOf
{
    public static VisualizerDef PLV_None;

    static VisualizerDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(VisualizerDefOf));
    }
}