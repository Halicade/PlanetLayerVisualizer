using LudeonTK;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PlanetLayerVisualizer;

public class ShowRainFall : Window_DevListing
{
    public float rainFall;
    public float alpha = 1;
    public float cutoff;
    public bool cutoffEnabled = true;
    public bool invertCutoff = false;
    private bool toggleUpdate = true;


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

            rainFall = num;
            PrintLabel($"Selected tile ({selectedTile}):");
            PrintLabel("Coordinates: " + tileCenter.ToString("0.#"));
            PrintLabel($"Rainfall value: {num}");

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