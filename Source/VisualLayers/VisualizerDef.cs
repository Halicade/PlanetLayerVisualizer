using System;
using UnityEngine;
using Verse;

namespace PlanetLayerVisualizer;

public class VisualizerDef : Def
{
    public float minValue = 0f;
    public float maxValue = 100f;
    
    public FloatRange expectedMinMaxValue = new(0,100);
    
    public int steps = 10;

    //Higher is closer to top
    public int displayPriority = 50;
    public bool refreshOnTileMove = false;

    public Color colorStart = Color.white;
    public Color colorEnd = Color.black;

    public Type VisualLayerClass;

    private VisualLayer _worker;
    
    public float CachedMaxValue;
    public float CachedMinValue;
    public int CachedSteps;

    public bool SliderValueChanged() => !Mathf.Approximately(maxValue, CachedMaxValue) ||
                                   !Mathf.Approximately(minValue, CachedMinValue) || 
                                   steps != CachedSteps;

    public void InitializeValues() {
        CachedMaxValue = maxValue;
        CachedMinValue = minValue;
        CachedSteps = steps;
    }
    
    public void UpdateValues() {
        maxValue = CachedMaxValue;
        minValue = CachedMinValue;
        steps = CachedSteps;
    }


    public VisualLayer Worker => _worker ??= GenWorker<VisualLayer>.Get(VisualLayerClass);
}