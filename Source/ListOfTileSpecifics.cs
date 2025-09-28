using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;

namespace PlanetLayerVisualizer;

internal class ListOfTileSpecifics(Tile biomeInfo)
{
    public BiomeDef SpecificBiome = biomeInfo.PrimaryBiome;
    public string SpecificBiomeDefName = biomeInfo.PrimaryBiome.defName;
    public string SpecificBiomeName = biomeInfo.PrimaryBiome.label;
    public string BiomeModSource = biomeInfo.PrimaryBiome?.modContentPack?.Name;
    public int TileCount = 1;
    public float BiomeMaxRainFall = biomeInfo.rainfall;
    public float BiomeMinRainFall = biomeInfo.rainfall;
    public float BiomeAvgRainfall = biomeInfo.rainfall;
    public float BiomeMaxTemperature = biomeInfo.MaxTemperature;
    public float BiomeMinTemperature = biomeInfo.MinTemperature;
    public float BiomeAvgTemperature = biomeInfo.temperature;
    public float BiomeMaxElevation = biomeInfo.elevation;
    public float BiomeMinElevation = biomeInfo.elevation;
    public float BiomeAvgElevation = biomeInfo.elevation;
    public float BiomeMaxPollution = biomeInfo.pollution;
    public float BiomeMinPollution = biomeInfo.pollution;
    public float BiomeAvgPollution = biomeInfo.pollution;
    public int LandmarkCount = biomeInfo.Landmark != null ? 1 : 0;


    public void AddTileSpecifics(Tile biomeInfo) {
        TileCount++;
        BiomeMaxRainFall = Math.Max(BiomeMaxRainFall, biomeInfo.rainfall);
        BiomeMinRainFall = Math.Min(BiomeMinRainFall, biomeInfo.rainfall);
        BiomeAvgRainfall = (BiomeAvgRainfall + biomeInfo.rainfall) / 2;
        BiomeMaxElevation = Math.Max(BiomeMaxElevation, biomeInfo.elevation);
        BiomeMinElevation = Math.Min(BiomeMinElevation, biomeInfo.elevation);
        BiomeAvgElevation = (BiomeAvgElevation + biomeInfo.elevation) / 2;
        BiomeMaxTemperature = Math.Max(BiomeMaxTemperature, biomeInfo.MaxTemperature);
        BiomeMinTemperature = Math.Min(BiomeMinTemperature, biomeInfo.MinTemperature);
        BiomeAvgTemperature = (BiomeAvgTemperature + biomeInfo.temperature) / 2;
        BiomeMaxPollution = Math.Max(BiomeMaxPollution, biomeInfo.pollution);
        BiomeMinPollution = Math.Min(BiomeMinPollution, biomeInfo.pollution);
        BiomeAvgPollution = (BiomeAvgPollution + biomeInfo.pollution) / 2;
        LandmarkCount = biomeInfo.Landmark != null ? LandmarkCount + 1 : LandmarkCount;
    }

    /// <summary>
    /// I would like to apologize for the mess that I have created
    /// </summary>
    public void AddTotalManually(
        string specificBiomeDefName,
        string specificBiomeName,
        string biomeModSource,
        int tileCount,
        float biomeMaxRainFall,
        float biomeMinRainFall,
        float avgRainfall,
        float biomeMaxTemperature,
        float biomeMinTemperature,
        float avgTemperature,
        float biomeMaxElevation,
        float biomeMinElevation,
        float avgElevation,
        float maxPollution,
        float minPollution,
        float avgPollution,
        int landMarkCount) {
        this.SpecificBiomeDefName = specificBiomeDefName;
        this.SpecificBiomeName = specificBiomeName;
        this.BiomeModSource = biomeModSource;
        this.TileCount = tileCount;
        this.BiomeMaxRainFall = biomeMaxRainFall;
        this.BiomeMinRainFall = biomeMinRainFall;
        this.BiomeAvgRainfall = avgRainfall;
        this.BiomeMaxTemperature = biomeMaxTemperature;
        this.BiomeMinTemperature = biomeMinTemperature;
        this.BiomeAvgTemperature = avgTemperature;
        this.BiomeMaxElevation = biomeMaxElevation;
        this.BiomeMinElevation = biomeMinElevation;
        this.BiomeAvgElevation = avgElevation;
        this.BiomeMaxPollution = maxPollution;
        this.BiomeMinPollution = minPollution;
        this.BiomeAvgPollution = avgPollution;
        this.LandmarkCount = landMarkCount;
    }
}