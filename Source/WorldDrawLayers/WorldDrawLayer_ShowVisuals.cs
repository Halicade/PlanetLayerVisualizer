using System.Collections;
using System.Collections.Generic;
using RimWorld.Planet;
using Unity.Collections;
using UnityEngine;
using Verse;

namespace PlanetLayerVisualizer;

public class WorldDrawLayer_ShowVisuals : WorldDrawLayer
{
    private readonly List<List<int>> _triangleIndexToTileID = [];

    private Material _material;

    public override bool Visible {
        get
        {
            if (base.Visible && Find.WindowStack.IsOpen<BiomeVisualWindow>()) {
                return SettingsOptions.Alpha > 0f;
            }

            return false;
        }
    }

    public override bool VisibleWhenLayerNotSelected => false;

    public override bool VisibleInBackground => false;

    public override IEnumerable Regenerate() {
        Find.WindowStack.TryGetWindow<BiomeVisualWindow>(out var window);

        foreach (object item in base.Regenerate()) {
            yield return item;
        }

        if (window.DefLookingAt == VisualizerDefOf.PLV_None)
            yield break;

        GenerateNewVisual(window.DefLookingAt);
    }


    private void GenerateNewVisual(VisualizerDef defLookingAt) {
        if (_material == null) {
            _material = new Material(WorldMaterials.VertexColorTransparent);
        }

        NativeArray<int> unsafeTileIDToVertsOffsets = planetLayer.UnsafeTileIDToVerts_offsets;

        NativeArray<Vector3> unsafeVerts = planetLayer.UnsafeVerts;

        _triangleIndexToTileID.Clear();
        for (int i = 0; i < planetLayer.TilesCount; i++) {
            PlanetTile tileForID = planetLayer.PlanetTileForID(i);
            //Vector3 tileCenter = Find.WorldGrid.GetTileCenter(new PlanetTile(i, planetLayer));
            //num is originally the perlin. Now the rainfall
            if (!SettingsOptions.CountImpassable) {
                if (!Find.WorldPathGrid.PassableFast(tileForID)) {
                    continue;
                }
            }

            float valueLookingAt = defLookingAt.Worker.RetrieveData(tileForID, SettingsOptions.TileLookingAt);

            if ((!SettingsOptions.InvertCutoff
                 && SettingsOptions.Cutoff != 0f
                 && valueLookingAt < SettingsOptions.Cutoff)
                || (SettingsOptions.InvertCutoff
                    && !Mathf.Approximately(SettingsOptions.Cutoff, defLookingAt.expectedMinMaxValue.max)
                    && valueLookingAt > SettingsOptions.Cutoff)) {
                continue;
            }

            LayerSubMesh subMesh = GetSubMesh(_material, out int subMeshIndex);
            while (subMeshIndex >= _triangleIndexToTileID.Count) {
                _triangleIndexToTileID.Add([]);
            }

            int count = subMesh.verts.Count;
            int num2 = 0;

            int num3 = ((i + 1 < unsafeTileIDToVertsOffsets.Length)
                ? unsafeTileIDToVertsOffsets[i + 1]
                : unsafeVerts.Length);
            for (int j = unsafeTileIDToVertsOffsets[i]; j < num3; j++) {
                subMesh.verts.Add(unsafeVerts[j]);

                subMesh.colors.Add(defLookingAt.Worker.RetrieveColor(valueLookingAt, defLookingAt, SettingsOptions.Alpha)
                );

                //original was for grayscale. Works better for Large variance
                //subMesh.colors.Add(new Color32(rainValue, rainValue, rainValue, alphaValue));

                if (j < num3 - 2) {
                    subMesh.tris.Add(count + num2 + 2);
                    subMesh.tris.Add(count + num2 + 1);
                    subMesh.tris.Add(count);
                    _triangleIndexToTileID[subMeshIndex].Add(i);
                }

                num2++;
            }
        }

        FinalizeMesh(MeshParts.All);
    }
}