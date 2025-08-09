using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace PlanetLayerVisualizer
{
    
    internal class Harmony_Initializer :Mod
    {
        private static readonly Texture2D VisualizerButton = ContentFinder<Texture2D>.Get("UI/Buttons/BLV_Visualizer");

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

        [HarmonyPostfix]
        private static void WorldViewControlsPostFix(WidgetRow row) {
            
            if (row.ButtonIcon(VisualizerButton)) {
                Find.WindowStack.Add(new BiomeVisualWindow());
            } 
        }
    }
}