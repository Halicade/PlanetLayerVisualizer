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
            if (base.Visible && TryGetWindow(out var window)) {
                return window.Settings.Alpha > 0f;
            }

            return false;
        }
    }

    public override bool VisibleWhenLayerNotSelected => false;

    public override bool VisibleInBackground => false;

    private static bool TryGetWindow(out BiomeVisualWindow window) {
        return Find.WindowStack.TryGetWindow<BiomeVisualWindow>(out window);
    }

    private SettingsOptions _settings;

    public override IEnumerable Regenerate() {
        TryGetWindow(out var window);
        _settings = window.Settings;


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
            if (!_settings.countImpassable) {
                if (!Find.WorldPathGrid.PassableFast(tileForID)) {
                    continue;
                }
            }

            float valueLookingAt = defLookingAt.Worker.RetrieveData(tileForID, _settings.TileLookingAt);

            if ((!_settings.InvertCutoff
                 && _settings.Cutoff != 0f
                 && valueLookingAt < _settings.Cutoff
                 && _settings.CutoffEnabled)
                || (_settings.InvertCutoff
                    && !Mathf.Approximately(_settings.Cutoff, defLookingAt.expectedMinMaxValue.max)
                    && valueLookingAt > _settings.Cutoff && _settings.CutoffEnabled)) {
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
            //byte rainValue = (byte)Mathf.Lerp(0, 255, rainfallDecimal);
            /*
            float normalizedValue = Mathf.Clamp(((valueLookingAt - minValue) / (maxValue - minValue)),0f,0.4f);
            float lerpedValue = Mathf.Lerp(0.0f, 1.0f, normalizedValue);*/
            //Log.Message("elevation normalized is " + elevationValue);
            for (int j = unsafeTileIDToVertsOffsets[i]; j < num3; j++) {
                subMesh.verts.Add(unsafeVerts[j]);

                subMesh.colors.Add(defLookingAt.Worker.RetrieveColor(valueLookingAt, defLookingAt)
                    .WithAlpha(_settings.Alpha));

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