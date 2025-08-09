using System.Collections;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Unity.Collections;
using UnityEngine;
using Verse;

namespace PlanetLayerVisualizer;

public class WorldDrawLayer_ShowBiomeScore : WorldDrawLayer
{
    private readonly List<List<int>> triangleIndexToTileID = new List<List<int>>();

    private Material material;

    public override bool Visible {
        get
        {
            if (base.Visible && TryGetWindow(out var window)) {
                return window.alpha > 0f;
            }

            return false;
        }
    }

    public override bool VisibleWhenLayerNotSelected => false;

    public override bool VisibleInBackground => false;

    private bool TryGetWindow(out ShowBiomeScore window) {
        return Find.WindowStack.TryGetWindow<ShowBiomeScore>(out window);
    }

    public override IEnumerable Regenerate() {
        if (material == null) {
            material = new Material(WorldMaterials.VertexColorTransparent);
        }

        TryGetWindow(out var window);

        foreach (object item in base.Regenerate()) {
            yield return item;
        }

        NativeArray<int> unsafeTileIDToVerts_offsets = planetLayer.UnsafeTileIDToVerts_offsets;

        NativeArray<Vector3> unsafeVerts = planetLayer.UnsafeVerts;

        //Huge guess here. Going to say maximum rainfall is like 6000
        byte alphaValue = (byte)Mathf.Clamp(Mathf.RoundToInt(window.alpha * 255.999f), 0, 255);
        triangleIndexToTileID.Clear();
        Log.Message("Hopefully should be visualing somewthing");
        for (int i = 0; i < planetLayer.TilesCount; i++) {
            PlanetTile tileForID = planetLayer.PlanetTileForID(i);
            //Vector3 tileCenter = Find.WorldGrid.GetTileCenter(new PlanetTile(i, planetLayer));
            //num is originally the perlin. Now the rainfall
            float rainfallAmount = tileForID.Tile.rainfall;
            float rainfallDecimal = rainfallAmount / 10000;


            if ((!window.invertCutoff && window.cutoff != 0f && rainfallAmount < window.cutoff && window.cutoffEnabled)
                ||
                (window.invertCutoff && !Mathf.Approximately(window.cutoff, 6000f) &&
                 rainfallAmount > window.cutoff && window.cutoffEnabled)) {
                continue;
            }

            LayerSubMesh subMesh = GetSubMesh(material, out int subMeshIndex);
            while (subMeshIndex >= triangleIndexToTileID.Count) {
                triangleIndexToTileID.Add([]);
            }

            int count = subMesh.verts.Count;
            int num2 = 0;

            int num3 = ((i + 1 < unsafeTileIDToVerts_offsets.Length)
                ? unsafeTileIDToVerts_offsets[i + 1]
                : unsafeVerts.Length);
            //byte rainValue = (byte)Mathf.Lerp(0, 255, rainfallDecimal);
            float rainValue = Mathf.Lerp(0, 255, rainfallDecimal) / 360;

            for (int j = unsafeTileIDToVerts_offsets[i]; j < num3; j++) {
                subMesh.verts.Add(unsafeVerts[j]);

                subMesh.colors.Add((Color.HSVToRGB(rainValue, 1f, 0.5f)).WithAlpha(window.alpha));

                //original was for grayscale. Works better for Large variance
                //subMesh.colors.Add(new Color32(rainValue, rainValue, rainValue, alphaValue));

                if (j < num3 - 2) {
                    subMesh.tris.Add(count + num2 + 2);
                    subMesh.tris.Add(count + num2 + 1);
                    subMesh.tris.Add(count);
                    triangleIndexToTileID[subMeshIndex].Add(i);
                }

                num2++;
            }
        }

        FinalizeMesh(MeshParts.All);
    }
}