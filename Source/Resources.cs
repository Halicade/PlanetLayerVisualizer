using RimWorld;
using UnityEngine;
using Verse;

namespace PlanetLayerVisualizer;

[StaticConstructorOnStartup]
public class Resources
{
    public static readonly Texture2D VisualizerButton = ContentFinder<Texture2D>.Get("UI/Buttons/PLV_Visualizer");
}