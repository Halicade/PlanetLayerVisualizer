using LudeonTK;
using Verse;

namespace PlanetLayerVisualizer;

public class ShowMeRainFall 
{
    
    [DebugAction("General", "Rainfall visualiser", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.WorldRenderedNow)]
    private static void TemperatureHeatMap()
    {
        if (Find.WindowStack.TryGetWindow<ShowRainFall>(out var window))
        {
            window.Close();
        }
        Find.WindowStack.Add(new ShowRainFall());
    }
    /*
    [DebugAction("General", "biome score display", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.WorldRenderedNow)]
    private static void BiomeScoreData()
    {
        if (Find.WindowStack.TryGetWindow<ShowBiomeScore>(out var window))
        {
            window.Close();
        }
        Find.WindowStack.Add(new ShowBiomeScore());
    }*/

}

public class Dialog_ShowRainfallWorld(){}