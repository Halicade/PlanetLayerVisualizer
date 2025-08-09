using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;

namespace PlanetLayerVisualizer;

internal class ListOfTileSpecifics(Tile biomeInfo)
{
    public BiomeDef SpecificBiome = biomeInfo.PrimaryBiome;
    public int TileCount = 1;
    public float BiomeMaxRainFall = biomeInfo.rainfall;
    public float BiomeMinRainFall = biomeInfo.rainfall;
    public float AvgRainfall = biomeInfo.rainfall;
    public float BiomeMaxTemperature = biomeInfo.MaxTemperature;
    public float BiomeMinTemperature = biomeInfo.MinTemperature;
    public float AvgTemperature = biomeInfo.temperature;
    public float BiomeMaxElevation = biomeInfo.elevation;
    public float BiomeMinElevation = biomeInfo.elevation;
    public float AvgElevation = biomeInfo.elevation;
    public float MaxPollution = biomeInfo.pollution;
    public float MinPollution = biomeInfo.pollution;
    public float AvgPollution = biomeInfo.pollution;
    public int landMarkCount = biomeInfo.Landmark != null ? 1 : 0;


    public void AddTileSpecifics(Tile biomeInfo) {
        TileCount++;
        BiomeMaxRainFall = Math.Max(BiomeMaxRainFall, biomeInfo.rainfall);
        BiomeMinRainFall = Math.Min(BiomeMinRainFall, biomeInfo.rainfall);
        AvgRainfall = (AvgRainfall + biomeInfo.rainfall) / 2;
        BiomeMaxElevation = Math.Max(BiomeMaxElevation, biomeInfo.elevation);
        BiomeMinElevation = Math.Min(BiomeMinElevation, biomeInfo.elevation);
        AvgElevation = (AvgElevation + biomeInfo.elevation) / 2;
        BiomeMaxTemperature = Math.Max(BiomeMaxTemperature, biomeInfo.MaxTemperature);
        BiomeMinTemperature = Math.Min(BiomeMinTemperature, biomeInfo.MinTemperature);
        AvgTemperature = (AvgTemperature + biomeInfo.temperature) / 2;
        MaxPollution = Math.Max(BiomeMaxTemperature, biomeInfo.pollution);
        MinPollution = Math.Min(BiomeMinTemperature, biomeInfo.pollution);
        AvgPollution = (AvgTemperature + biomeInfo.pollution) / 2;
        landMarkCount = biomeInfo.Landmark != null ? landMarkCount + 1 : landMarkCount;
    }
}