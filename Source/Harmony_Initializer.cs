using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PlanetLayerVisualizer;

internal class Harmony_Initializer : Mod
{
    private static readonly Type PatchType = typeof(Harmony_Initializer);

    public Harmony_Initializer(ModContentPack content) : base(content) {
        LongEventHandler.QueueLongEvent(action: HarmonyPatches,
            textKey: null,
            doAsynchronously: true,
            exceptionHandler: null
        );
    }

    private static void HarmonyPatches() {
        Harmony harmony = new("Hali.PlanetVisualizer");

        harmony.Patch(AccessTools.Method(typeof(PlaySettings), "DoWorldViewControls"),
            postfix: new HarmonyMethod(PatchType, nameof(WorldViewControlsPostFix)));
        
    }

    private static void WorldViewControlsPostFix(WidgetRow row) {
        if (row.ButtonIcon(Resources.VisualizerButton)) {
            Find.WindowStack.Add(new BiomeVisualWindow());
        }
    }
}