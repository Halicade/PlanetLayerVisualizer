using System;
using UnityEngine;
using Verse;

namespace PlanetLayerVisualizer;

public class VisualizerDef : Def
{
    public float minValue = 0f;

    public float maxValue = 100f;
    public float valueCutoffMin = 0;
    public float valueCutoffMax = 100f;
    public int steps = 10;

    //Higher is closer to top
    public int displayPriority = 50;
    public bool refreshOnTileMove = false;

    public Color colorStart = Color.white;
    public Color colorEnd = Color.black;

    public Type VisualLayerClass;

    private VisualLayer worker;

    public VisualLayer Worker => worker ??= GenWorker<VisualLayer>.Get(VisualLayerClass);
}