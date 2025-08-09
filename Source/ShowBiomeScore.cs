using System.Collections.Generic;
using System.ComponentModel;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PlanetLayerVisualizer;

public class ShowBiomeScore : Window_DevListing
{
    public float rainFall;
    public float alpha = 1;
    public float cutoff;
    public bool cutoffEnabled = true;
    private bool toggleUpdate = true;
    public bool invertCutoff = false;
    private PlanetTile cachedTile;
    private readonly List<BiomeAndScore> biomeScoreList=[];

    private class BiomeAndScore(string biomeLabel, float biomeScore)
    {
        public string BiomeLabel = biomeLabel;
        public float BiomeScore = biomeScore;
    }


    public override void PreOpen() {
        base.PreOpen();
        OnNoiseChanged();
    }

    public override void PostClose() {
        base.PostClose();
        OnNoiseChanged();
    }

    protected virtual void OnNoiseChanged() {
        Find.World.renderer.SetDirty<WorldDrawLayer_ShowVisuals>(PlanetLayer.Selected);
    }

    protected override void DoWindowListing() {
        PrintLabel("Rainfall visualizer", GameFont.Small);
        
        
        if (Find.World.renderer.TryGetLayer<WorldDrawLayer_ShowVisuals>(PlanetLayer.Selected, out var _)) {
            if (Find.WorldSelector.SelectedTile == PlanetTile.Invalid) {
                PrintLabel("No tile selected");
                return;
            }

            
            Toggle("Updating: " + (toggleUpdate ? "Enabled" : "Disabled"), ref toggleUpdate);

            PlanetTile selectedTile = Find.WorldSelector.SelectedTile;
            Vector3 tileCenter = Find.WorldGrid.GetTileCenter(selectedTile);
            float num = selectedTile.Tile.rainfall;

            if (selectedTile != cachedTile) {
                biomeScoreList.Clear();
                List<BiomeDef> allBiomes = DefDatabase<BiomeDef>.AllDefsListForReading;
                for (int index = 0; index < allBiomes.Count; index++) {
                    BiomeDef biome = allBiomes[index];
                    if (biome.implemented && biome.generatesNaturally &&
                        biome.Worker.CanPlaceOnLayer(biome, selectedTile.Layer)) {
                        float score = biome.Worker.GetScore(biome, selectedTile.Tile, selectedTile);
                        //Log.Message("Score at tile for Biome " + biome.label + " is " + score);
                        biomeScoreList.Add(new BiomeAndScore(biome.label,score));
                        
                    }
                }

                cachedTile = selectedTile;
                biomeScoreList.SortByDescending(x=> x.BiomeScore);
            }


            rainFall = num;
            PrintLabel($"Selected tile ({selectedTile}):");
            PrintLabel("Coordinates: " + tileCenter.ToString("0.#"));
            PrintLabel($"Rainfall value: {num}");
            foreach (var bscorelist in biomeScoreList) {
                PrintLabel($"Score: {bscorelist.BiomeScore:000}\tBiome: {bscorelist.BiomeLabel}");
            }

            SliderFieldFloat(
                label: $"Cutoff: {cutoff:0000}",
                min: 0,
                max: 6000,
                value: ref cutoff,
                enabled: ref cutoffEnabled
            );
            Toggle(label: "invert cutoff", value: ref invertCutoff);
            //SliderFieldInt(label: $"Cutoff: {cutoff:0000}", min: 0, max: 6000, value: ref cutoff);

            SliderFieldFloat($"Alpha: {alpha * 100f:0}%", ref alpha);

            if (base.Dirty && toggleUpdate) {
                OnNoiseChanged();
                OnChanged();
            }
        }
    }
}