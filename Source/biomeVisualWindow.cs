using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PlanetLayerVisualizer;

public class BiomeVisualWindow : Window
{
    private BiomeDef _selectedBiome;
    private float _selectedRainFall;
    private float _selectedElevation;
    private float _selectedPollution;
    private float _layerMaxRainFall;
    private float _layerMinRainFall;
    private float _layerAvgRainFall;
    private float _layerMaxElevation;
    private float _layerMinElevation;
    private float _layerAvgElevation;

    private float _layerMaxTemperature;
    private float _layerMinTemperature;
    private float _layerAvgTemperature;

    private float _layerMaxPollution;
    private float _layerMinPollution;
    private float _layerAvgPollution;

    private int _layerLandmarkCount;

    private readonly int _totalBiomeCount = DefDatabase<BiomeDef>.DefCount;

    private readonly List<VisualizerDef> _possibleVisuals =
        DefDatabase<VisualizerDef>.AllDefsListForReading.OrderByDescending(x => x.displayPriority).ToList();

    public VisualizerDef DefLookingAt;

    private PlanetTile _selectedPlanetTile;
    private Tile _selectedTile;
    private PlanetTile _cachedPlanetTile;
    private PlanetLayer _currentLayer = null;
    private PlanetLayer _cachedLayer = null;
    private int _totalTileCount;

    private List<Tile> _listOfLayersTiles;
    private bool _currentlyCalculating = false;
    private bool _startNewCalculation = false;
    private bool _finishedCalculating = false;
    private int _startingCount = 0;

    private const int IntervalToIncrement = 500;

    /// int is the hash of the biomeDef
    private Dictionary<int, ListOfTileSpecifics> _biomeSpecificDict;

    /// True sort by score, false sort by name
    private bool _biomeScoreSortLogic = true;

    private bool _biomeScoreButton = false;

    private readonly List<BiomeAndScore> _biomeScoreList = [];

    private class BiomeAndScore(BiomeDef biome, float biomeScore)
    {
        public readonly BiomeDef Biome = biome;
        public readonly String BiomeLabelCached = GetSubstring(biome.defName,15);
        public readonly float BiomeScore = biomeScore;
        public readonly string BiomeScoreCached = biomeScore.ToString("0.##");

        public readonly TipSignal tipSig = new TipSignal("PLV_SelectedBiome".Translate(biome.label, biome.defName,
            biome?.modContentPack?.PackageId));
    }

    public SettingsOptions Settings = new();

    public override Vector2 InitialSize => new(700f, 550f);
    /*
    protected override void SetInitialSizeAndPosition() {
        base.SetInitialSizeAndPosition();

        windowRect.x = ((float)UI.screenWidth - windowRect.width) / 2f;
        windowRect.y = ((float)UI.screenHeight - windowRect.height) / 2f;

    }*/

    public override void PreOpen() {
        base.PreOpen();
        OnNoiseChanged();
        _initialLoad = true;
        DefLookingAt = _possibleVisuals[0];
    }

    public override void PostClose() {
        base.PostClose();
        OnNoiseChanged();
    }

    private static void OnNoiseChanged() {
        Find.World.renderer.SetDirty<WorldDrawLayer_ShowVisuals>(PlanetLayer.Selected);
    }

    private Vector2 _leftScrollPosition = Vector2.zero;

    private Rect _leftColumn;
    private Rect _leftOutRect;
    private Rect _biomeColViewRect;

    private Rect _biomeNameCol;

    private Rect _biomeScoreCol;

/*
    private Rect _centerColumn;
    private Rect _centerOutRect;
    private Rect _centerViewRect;


    private Rect _rightOutRect;
    */
    private Rect _rightColumn;


    private bool _initialLoad = true;


    public override void DoWindowContents(Rect rect) {
        if (!Find.World.renderer.TryGetLayer<WorldDrawLayer_ShowVisuals>(PlanetLayer.Selected, out var _)) return;

        if (_initialLoad) {
            _leftColumn = rect.LeftPart(0.33f);
            _rightColumn = rect.RightPart(0.66f);

            // +30 is for the top row button
            _leftOutRect = new Rect(_leftColumn.x, _leftColumn.y + 30f, _leftColumn.width, _leftColumn.height - 30f);
            _biomeColViewRect = new Rect(_leftColumn.x, _leftColumn.y,
                width: _leftColumn.width - 30f, // Width
                height: Text.LineHeight * (_totalBiomeCount + 1));

            _biomeNameCol = _biomeColViewRect.LeftPart(0.7f);
            _biomeScoreCol = _biomeColViewRect.RightPart(0.3f);


            _rightColumn = new Rect(_rightColumn.x + 10,
                _rightColumn.y,
                width: _rightColumn.width - 10f,
                height: 550);

            _initialLoad = false;
        }

        RecalculateTileInfo();
        RecalculateLayerInfo();

        LeftColumnDisplay();
        RightColumnDisplay();


        if (Settings.ToggleUpdate && CacheChanged()) {
            OnNoiseChanged();
        }
    }

    private void RightColumnDisplay() {
        //Listing_Standard listingStandardRight = new();
        //DebugTables.MakeTablesDialog();

        float yValue = _rightColumn.y;

        if (_selectedPlanetTile == PlanetTile.Invalid) {
            Widgets.Label(_rightColumn, "PLV_NoSelection".Translate());
            //Log.Message("Planet layer is invalid");

            //Widgets.EndScrollView();
            return;
        }

        bool startWorldCalculation = Widgets.ButtonText(
            new Rect(_rightColumn.x, _rightColumn.y, _rightColumn.width / 2, 30),
            "PLV_StartLayerCalc".Translate());


        if (startWorldCalculation) {
            _biomeSpecificDict.Clear();
            _currentlyCalculating = true;
            _startNewCalculation = true;
            _startingCount = 0;
        }


        if (_currentlyCalculating) {
            Log.Warning("Y value was " + yValue);
            Widgets.Label(
                new Rect(_rightColumn.x + _rightColumn.width / 2, _rightColumn.y, _rightColumn.width / 2, 30),
                ref yValue,
                "PLV_CalculationProgress".Translate(_startingCount, _totalTileCount)
            );
            Log.Message("Y value is " + yValue);
        }
        else {
            yValue += 38;
        }


        //Scrolling not needed for this column
        //Widgets.BeginScrollView(_centerOutRect, ref _centerScrollPosition, _centerViewRect);


        Listing_Standard listingStandardCenter = new();

        listingStandardCenter.Begin(
            new Rect(_rightColumn.x,
                _rightColumn.y + yValue,
                _rightColumn.width,
                _rightColumn.height - yValue));
        listingStandardCenter.CheckboxLabeled("PLV_EnableUpdating".Translate(), ref Settings.ToggleUpdate);

        Vector3 tileCenter = Find.WorldGrid.GetTileCenter(_selectedPlanetTile);
        listingStandardCenter.Label("PLV_SelectedTile".Translate(_selectedPlanetTile.tileId));
        listingStandardCenter.Label("PLV_SelectedCoordinates".Translate(tileCenter.ToString("0.#")));

        Rect widgetSize = listingStandardCenter.GetRect(Text.LineHeight);
        if (Widgets.ButtonText(rect: widgetSize,
                label: _possibleVisuals[Settings.ViewLookingAt].LabelCap)) {
            List<FloatMenuOption> list2 = [];
            for (int menuNumber = 0; menuNumber < _possibleVisuals.Count; menuNumber++) {
                int menuHolder = menuNumber;

                list2.Add(new FloatMenuOption(_possibleVisuals[menuNumber].LabelCap,
                    delegate {
                        if (Settings.ViewLookingAt != menuHolder) {
                            Settings.ViewLookingAt = menuHolder;
                            Settings.CacheChanged = true;
                            DefLookingAt = _possibleVisuals[menuHolder];
                            DefLookingAt.initializeValues();
                        }
                    }));
            }

            Find.WindowStack.Add(new FloatMenu(list2));
        }

        listingStandardCenter.Label("PLV_SelectedBiome".Translate(_selectedBiome.label, _selectedBiome.defName,
            _selectedBiome?.modContentPack?.PackageId));
        listingStandardCenter.Label("PLV_SelectedRainfall".Translate(_selectedRainFall));
        listingStandardCenter.Label("PLV_SelectedElevation".Translate(_selectedElevation));
        listingStandardCenter.Label("PLV_SelectedPollution".Translate(_selectedPollution));

        bool cachedImpassable = Settings.countImpassable;
        listingStandardCenter.CheckboxLabeled("PLV_CountImpassable".Translate(), ref cachedImpassable);
        if (!cachedImpassable.Equals(Settings.countImpassable)) {
            Settings.countImpassable = cachedImpassable;
            Settings.CacheChanged = true;
        }

        float cachedCutoffValue = listingStandardCenter.SliderLabeled(
            label: "PLV_CutoffText".Translate($"{Settings.Cutoff:0000}"), val: Settings.Cutoff,
            min: DefLookingAt.minValue, max: DefLookingAt.maxValue);

        if (!Mathf.Approximately(cachedCutoffValue, Settings.Cutoff)) {
            Settings.Cutoff = cachedCutoffValue;
            Settings.CacheChanged = true;
        }

        bool cachedCutoff = Settings.InvertCutoff;
        listingStandardCenter.CheckboxLabeled("PLV_InvertCutoff".Translate(), ref cachedCutoff);
        if (!cachedCutoff.Equals(Settings.InvertCutoff)) {
            Settings.InvertCutoff = cachedCutoff;
            Settings.CacheChanged = true;
        }

        float cachedAlpha = listingStandardCenter.SliderLabeled(
            label: "PLV_AlphaDisplay".Translate($"{Settings.Alpha * 100f:0}"),
            val: Settings.Alpha,
            min: 0,
            max: 1);
        if (!Mathf.Approximately(cachedAlpha, Settings.Alpha)) {
            Settings.Alpha = cachedAlpha;
            Settings.CacheChanged = true;
        }

        yValue += listingStandardCenter.CurHeight + 15;
        listingStandardCenter.End();


        DefLookingAt.CachedMinValue = Widgets.HorizontalSlider(
            new Rect(_rightColumn.x, yValue, _rightColumn.width, 30f),
            value: DefLookingAt.CachedMinValue,
            min: DefLookingAt.expectedMinMaxValue.min, max: DefLookingAt.expectedMinMaxValue.max,
            leftAlignedLabel: "PLV_MinLabel".Translate(),
            rightAlignedLabel: DefLookingAt.CachedMinValue.ToString(),
            roundTo: 1);
        yValue += 30;
        DefLookingAt.CachedMaxValue = Widgets.HorizontalSlider(
            new Rect(_rightColumn.x, yValue, _rightColumn.width, 30f),
            value: DefLookingAt.CachedMaxValue,
            min: DefLookingAt.expectedMinMaxValue.min, max: DefLookingAt.expectedMinMaxValue.max,
            leftAlignedLabel: "PLV_MaxLabel".Translate(),
            rightAlignedLabel: DefLookingAt.CachedMaxValue.ToString(),
            roundTo: 1
        );
        yValue += 30;
        DefLookingAt.CachedSteps = (int)Widgets.HorizontalSlider(
            new Rect(_rightColumn.x, yValue, _rightColumn.width, 30f),
            value: DefLookingAt.CachedSteps, min: 2, max: 100,
            leftAlignedLabel: "PLV_StepsLabel".Translate(),
            rightAlignedLabel: DefLookingAt.CachedSteps.ToString(),
            roundTo: 1
        );

        if (DefLookingAt.ValuesChanged()) {
            DefLookingAt.UpdateValues();
            Settings.CacheChanged = true;
        }
    }


    private void LeftColumnDisplay() {
        _biomeScoreButton = Widgets.ButtonText(new Rect(_leftColumn.x, _leftColumn.y, _leftColumn.width - 30f, 25f),
            _biomeScoreSortLogic ? "PLV_ScoreSort".Translate() : "PLV_NameSort".Translate());
        Widgets.BeginScrollView(_leftOutRect, ref _leftScrollPosition, _biomeColViewRect);

        if (_biomeScoreButton) {
            _biomeScoreSortLogic = !_biomeScoreSortLogic;
            _cachedPlanetTile = PlanetTile.Invalid;
        }

        float yName = 0f;
        float yScore = 0f;
        Widgets.Label(
            x: _biomeNameCol.x,
            text: "PLV_BiomeWord".Translate(),
            curY: ref yName,
            width: _biomeNameCol.width);
        Widgets.Label(
            x: _biomeScoreCol.x,
            text: "PLV_ScoreWord".Translate(),
            curY: ref yScore,
            width: _biomeScoreCol.width);

        foreach (var biomeScore in _biomeScoreList) {
            Widgets.Label(
                curY: ref yName,
                text: biomeScore.BiomeLabelCached,
                x: _biomeNameCol.x,
                width: _biomeNameCol.width,
                tip: biomeScore.tipSig);

            Widgets.Label(
                curY: ref yScore,
                x: _biomeScoreCol.x,
                width: _biomeScoreCol.width,
                text: biomeScore.BiomeScoreCached);
        }

        Widgets.EndScrollView();
    }

    private static string GetSubstring(string stringFormatted, int length) {
        return stringFormatted.Length < length ? stringFormatted : stringFormatted.Substring(0, length);
    }


    private bool CacheChanged() {
        if (Settings.CacheChanged) {
            Settings.CacheChanged = false;
            return true;
        }

        return false;
    }

    private void RecalculateTileInfo() {
        _selectedPlanetTile = Find.WorldSelector.SelectedTile;
        if (_selectedPlanetTile == _cachedPlanetTile) return;
        _cachedPlanetTile = _selectedPlanetTile;
        if (!_selectedPlanetTile.Valid) return;

        _selectedTile = _selectedPlanetTile.Tile;
        _selectedBiome = _selectedPlanetTile.Tile.PrimaryBiome;
        _selectedRainFall = _selectedPlanetTile.Tile.rainfall;
        _selectedElevation = _selectedTile.elevation;
        _selectedPollution = _selectedTile.pollution;

        Settings.TileLookingAt = _selectedPlanetTile.Tile;

        if (DefLookingAt.refreshOnTileMove) {
            Settings.CacheChanged = true;
        }

        _biomeScoreList.Clear();
        List<BiomeDef> allBiomes = DefDatabase<BiomeDef>.AllDefsListForReading;
        foreach (BiomeDef biome in allBiomes) {
            if (biome.implemented && biome.generatesNaturally &&
                biome.Worker.CanPlaceOnLayer(biome, _selectedTile.Layer)) {
                float score = biome.Worker.GetScore(biome, _selectedTile, _selectedPlanetTile);
                //Log.Message("Score at tile for Biome " + biome.label + " is " + score);
                _biomeScoreList.Add(new BiomeAndScore(biome, score));
            }
        }

        if (_biomeScoreSortLogic) {
            _biomeScoreList.SortByDescending(x => x.BiomeScore);
        }
        else {
            _biomeScoreList.SortBy(x => x.Biome.label);
        }
    }

    private void RecalculateLayerInfo() {
        if (!_selectedPlanetTile.Valid) return;
        if (!Equals(_currentLayer, _selectedPlanetTile.Layer) || _startNewCalculation) {
            _startNewCalculation = false;
            //Log.Message("Current layer has changed");
            _currentLayer = _selectedPlanetTile.Layer;

            _listOfLayersTiles = _currentLayer.Tiles;
            _cachedLayer = _currentLayer;

            _biomeSpecificDict = new Dictionary<int, ListOfTileSpecifics>();
            _totalTileCount = _currentLayer.TilesCount;

            _layerMaxRainFall = 0;
            _layerMinRainFall = float.PositiveInfinity;
            _layerMaxElevation = float.NegativeInfinity;
            _layerMinElevation = float.PositiveInfinity;
            _layerMaxTemperature = float.NegativeInfinity;
            _layerMinTemperature = float.PositiveInfinity;


            _layerAvgElevation = 0;
            _layerAvgTemperature = 0;
            _layerAvgPollution = 0;
            _layerAvgRainFall = 0;
            _layerLandmarkCount = 0;
        }


        if (_currentlyCalculating) {
            int countToReach = _startingCount + IntervalToIncrement;
            if (countToReach > _totalTileCount)
                countToReach = _totalTileCount;
            for (int currentValueAt = _startingCount; currentValueAt < countToReach; currentValueAt++) {
                if (_biomeSpecificDict.TryGetValue(
                        _listOfLayersTiles[currentValueAt].tile.Tile.PrimaryBiome.GetHashCode(),
                        out ListOfTileSpecifics biomeLookingAt)) {
                    biomeLookingAt.AddTileSpecifics(_listOfLayersTiles[currentValueAt].tile.Tile);
                }
                else {
                    Log.Message(
                        "We are adding a new biome " + _listOfLayersTiles[currentValueAt].tile.Tile.PrimaryBiome);
                    _biomeSpecificDict.Add(_listOfLayersTiles[currentValueAt].tile.Tile.PrimaryBiome.GetHashCode(),
                        new ListOfTileSpecifics(_listOfLayersTiles[currentValueAt].tile.Tile));
                }
            }

            _startingCount = countToReach;
            Log.Message("Current count total " + _startingCount + " remaining " + (_totalTileCount - _startingCount));
            int prevTileCount = 0;
            if (_startingCount == _totalTileCount) {
                Log.Message("Biome counting is done!");
                _currentlyCalculating = false;
                _finishedCalculating = true;

                foreach (var biomeData in _biomeSpecificDict.Values) {
                    _layerMaxRainFall = Math.Max(_layerMaxRainFall, biomeData.BiomeMaxRainFall);
                    _layerMinRainFall = Math.Min(_layerMinRainFall, biomeData.BiomeMinRainFall);
                    _layerMaxElevation = Math.Max(_layerMaxElevation, biomeData.BiomeMaxElevation);
                    _layerMinElevation = Math.Min(_layerMinElevation, biomeData.BiomeMinElevation);
                    _layerMaxTemperature = Math.Max(_layerMaxTemperature, biomeData.BiomeMaxTemperature);
                    _layerMinTemperature = Math.Min(_layerMinTemperature, biomeData.BiomeMinTemperature);
                    _layerMaxPollution = Math.Max(_layerMaxPollution, biomeData.BiomeMaxPollution);
                    _layerMinPollution = Math.Min(_layerMinPollution, biomeData.BiomeMinPollution);
                    _layerLandmarkCount += biomeData.LandmarkCount;

                    if (prevTileCount == 0) {
                        // default values for first tile
                        _layerAvgRainFall = biomeData.BiomeAvgRainfall;
                        _layerAvgElevation = biomeData.BiomeAvgElevation;
                        _layerAvgTemperature = biomeData.BiomeAvgTemperature;
                        _layerAvgPollution = biomeData.BiomeAvgPollution;
                    }
                    else {
                        _layerAvgRainFall = (_layerAvgRainFall * prevTileCount +
                                             biomeData.BiomeAvgRainfall * biomeData.TileCount) /
                                            (prevTileCount + biomeData.TileCount);
                        _layerAvgElevation = (_layerAvgElevation * prevTileCount +
                                              biomeData.BiomeAvgElevation * biomeData.TileCount) /
                                             (prevTileCount + biomeData.TileCount);
                        _layerAvgTemperature = (_layerAvgTemperature * prevTileCount +
                                                biomeData.BiomeAvgTemperature * biomeData.TileCount) /
                                               (prevTileCount + biomeData.TileCount);
                        _layerAvgPollution = (_layerAvgPollution * prevTileCount +
                                              biomeData.BiomeAvgPollution * biomeData.TileCount) /
                                             (prevTileCount + biomeData.TileCount);
                    }

                    prevTileCount = biomeData.TileCount;
                }

                //Total calculation of biomes gets stored as hash 0 just because
                _biomeSpecificDict.Add(0, new ListOfTileSpecifics(_selectedTile));

                _biomeSpecificDict.TryGetValue(0, out var tileSpecificStuff);

                tileSpecificStuff?.AddTotalManually(
                    "Total",
                    "Total",
                    "PlanetLayerVisualizer",
                    _totalTileCount,
                    _layerMaxRainFall,
                    _layerMinRainFall,
                    _layerAvgRainFall,
                    _layerMaxTemperature,
                    _layerMinTemperature,
                    _layerAvgTemperature,
                    _layerMaxElevation,
                    _layerMinElevation,
                    _layerAvgElevation,
                    _layerMaxPollution,
                    _layerMinPollution,
                    _layerAvgPollution,
                    _layerLandmarkCount
                );

                DebugTables.MakeTablesDialog(
                    _biomeSpecificDict.Values.OrderBy(x => x.TileCount),
                    new TableDataGetter<ListOfTileSpecifics>("biome defName", (x => x.SpecificBiomeDefName)),
                    new TableDataGetter<ListOfTileSpecifics>("biome label", (x => x.SpecificBiomeName)),
                    new TableDataGetter<ListOfTileSpecifics>("mod source", (x => x.BiomeModSource)),
                    new TableDataGetter<ListOfTileSpecifics>("Tile Count", (x => x.TileCount)),
                    new TableDataGetter<ListOfTileSpecifics>("Max Rainfall", (x => x.BiomeMaxRainFall)),
                    new TableDataGetter<ListOfTileSpecifics>("Min Rainfall", (x => x.BiomeMinRainFall)),
                    new TableDataGetter<ListOfTileSpecifics>("Avg Rainfall", (x => x.BiomeAvgRainfall)),
                    new TableDataGetter<ListOfTileSpecifics>("Max Temp", (x => x.BiomeMaxTemperature)),
                    new TableDataGetter<ListOfTileSpecifics>("Min Temp", (x => x.BiomeMinTemperature)),
                    new TableDataGetter<ListOfTileSpecifics>("Avg Temp", (x => x.BiomeAvgTemperature)),
                    new TableDataGetter<ListOfTileSpecifics>("Max Elevation", (x => x.BiomeMaxElevation)),
                    new TableDataGetter<ListOfTileSpecifics>("Min Elevation", (x => x.BiomeMinElevation)),
                    new TableDataGetter<ListOfTileSpecifics>("Avg Elevation", (x => x.BiomeAvgElevation)),
                    new TableDataGetter<ListOfTileSpecifics>("Max Pollution", (x => x.BiomeMaxPollution)),
                    new TableDataGetter<ListOfTileSpecifics>("Min Pollution", (x => x.BiomeMinPollution)),
                    new TableDataGetter<ListOfTileSpecifics>("Avg Pollution", (x => x.BiomeAvgPollution)),
                    new TableDataGetter<ListOfTileSpecifics>("Landmark Count", (x => x.LandmarkCount))
                );


                Log.Message("Planet layer calculations completed!");
            }
        }
    }


    public BiomeVisualWindow() {
        this.draggable = true;
        this.focusWhenOpened = false;
        this.drawShadow = false;
        this.closeOnAccept = false;
        this.closeOnCancel = false;
        this.preventCameraMotion = false;
        this.drawInScreenshotMode = false;
        this.onlyDrawInDevMode = true;
        this.doCloseX = true;
    }
}