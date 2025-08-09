using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Noise;

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

    private float _maxTemperature;
    private float _minTemperature;

    private float _layerMaxTemperature;
    private float _layerMinTemperature;
    private float _layerAvgTemperature;

    private float _layerMaxPollution;
    private float _layerMinPollution;
    private float _layerAvgPollution;

    private float _layerlandmarkCount;

    private float _forageability;
    private float _pollution;
    private float _nearPollution;
    private float _diseaseFreq;
    private int _totalBiomeCount = DefDatabase<BiomeDef>.DefCount;

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
    private bool _finishedCalculating = false;
    private int _startingCount = 0;

    private const int IntervalToIncrement = 500;

    private Dictionary<BiomeDef, ListOfTileSpecifics> _biomeSpecificDict;

    /// True sort by score, false sort by name
    private bool _biomeScoreSortLogic = true;

    private bool _biomeScoreButton = false;

    private readonly List<BiomeAndScore> _biomeScoreList = [];

    private class BiomeAndScore(BiomeDef biome, float biomeScore)
    {
        public BiomeDef Biome = biome;
        public float BiomeScore = biomeScore;
    }

    public SettingsOptions Settings = new();

    public override Vector2 InitialSize => new(1000f, 500f);
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

    protected virtual void OnNoiseChanged() {
        Find.World.renderer.SetDirty<WorldDrawLayer_ShowVisuals>(PlanetLayer.Selected);
    }

    private Vector2 _centerScrollPosition = Vector2.zero;
    private Vector2 _leftScrollPosition = Vector2.zero;
    private Vector2 _rightScrollPosition = Vector2.zero;

    private Rect _leftColumn;
    private Rect _leftOutRect;
    private Rect _biomeColViewRect;

    private Rect _biomeNameCol;
    private Rect _biomeScoreCol;

    private Rect _centerColumn;
    private Rect _centerOutRect;
    private Rect _centerViewRect;

    private Rect _rightColumn;
    private Rect _rightOutRect;
    private Rect _rightViewRect;

    private Rect _rightViewTopBar;

    private bool _initialLoad = true;

    private const int TotalMenuNumbers = 8;


    public override void DoWindowContents(Rect rect) {
        if (!Find.World.renderer.TryGetLayer<WorldDrawLayer_ShowVisuals>(PlanetLayer.Selected, out var _)) return;

        if (_initialLoad) {
            _leftColumn = rect.LeftPart(0.66f);
            _rightColumn = rect.RightPart(0.33f);
            _centerColumn = _leftColumn.RightHalf();
            _leftColumn = _leftColumn.LeftHalf();

            // +30 is for the top row button
            _leftOutRect = new Rect(_leftColumn.x, _leftColumn.y + 30f, _leftColumn.width, _leftColumn.height);
            _biomeColViewRect = new Rect(_leftColumn.x, _leftColumn.y,
                width: _leftColumn.width - 30f, // Width
                height: Text.LineHeight * (_totalBiomeCount + 1));

            _biomeNameCol = _biomeColViewRect.LeftPart(0.7f);
            _biomeScoreCol = _biomeColViewRect.RightPart(0.3f);

            _centerOutRect = new Rect(_centerColumn.x, _centerColumn.y, _centerColumn.width, _centerColumn.height);
            _centerViewRect = new Rect(_centerColumn.x, _centerColumn.y,
                width: _centerColumn.width - 30f, // Width
                height: 800);

            _rightViewTopBar = _rightColumn.TopPart(0.15f);
            _rightOutRect = _rightColumn.BottomPart(0.85f);
            _rightViewRect = new Rect(_rightColumn.x, _rightOutRect.y,
                width: _rightColumn.width - 30f, // Width
                height: 800);


            _initialLoad = false;
        }

        RecalculateTileInfo();
        RecalculateLayerInfo();

        CenterColumnDisplay();
        LeftColumnDisplay();
        RightColumnDisplay();


        if (Settings.ToggleUpdate && CacheChanged()) {
            OnNoiseChanged();
        }
    }

    private void RightColumnDisplay() {
        /*
         * TODO create Window_DebugTable using
         * DebugTables.MakeTablesDialog
         */

        //Listing_Standard listingStandardRight = new();
        //DebugTables.MakeTablesDialog();
        bool startWorldCalculation = Widgets.ButtonText(_rightViewTopBar.LeftHalf(),
            "PLV_StartLayerCalc".Translate());
        if (startWorldCalculation) {
            _currentlyCalculating = true;
        }

        if (_currentlyCalculating) {
            Widgets.Label(_rightViewTopBar.RightHalf(),
                "PLV_CalculationProgress".Translate(_startingCount, _totalTileCount)
            );
        }
        else {
            if (_finishedCalculating) {
                /*
                 TODO click to open CSV
                Widgets.Label(_rightViewTopBar.RightHalf(),
                    "Completed".Translate()                );
                    */
            }
        }

        Widgets.BeginScrollView(_rightOutRect, ref _rightScrollPosition, _rightViewRect);

        //listingStandardRight.End();
        Widgets.EndScrollView();
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
                text: biomeScore.Biome.label,
                x: _biomeNameCol.x,
                width: _biomeNameCol.width,
                tip: new TipSignal("PLV_SelectedBiome".Translate(biomeScore.Biome.label, biomeScore.Biome.defName,
                    biomeScore.Biome?.modContentPack?.PackageId)));
            Widgets.Label(
                curY: ref yScore,
                x: _biomeScoreCol.x,
                width: _biomeScoreCol.width,
                text: biomeScore.BiomeScore.ToString(CultureInfo.InvariantCulture));
        }

        Widgets.EndScrollView();
    }

    private void CenterColumnDisplay() {
        Listing_Standard listingStandardCenter = new();
        Widgets.BeginScrollView(_centerOutRect, ref _centerScrollPosition, _centerViewRect);
        listingStandardCenter.Begin(_centerViewRect);
        if (_selectedPlanetTile == PlanetTile.Invalid) {
            Widgets.Label(_centerViewRect, "PLV_NoSelection".Translate());
            //Log.Message("Planet layer is invalid");
            listingStandardCenter.End();
            Widgets.EndScrollView();
            return;
        }


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

        listingStandardCenter.End();
        Widgets.EndScrollView();
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
        if (!Equals(_currentLayer, _selectedPlanetTile.Layer)) {
            //Log.Message("Current layer has changed");
            _currentLayer = _selectedPlanetTile.Layer;

            _listOfLayersTiles = _currentLayer.Tiles;
            _cachedLayer = _currentLayer;

            _biomeSpecificDict = new Dictionary<BiomeDef, ListOfTileSpecifics>();
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
            _layerlandmarkCount = 0;
        }


        if (_currentlyCalculating) {
            int countToReach = _startingCount + IntervalToIncrement;
            if (countToReach > _totalTileCount)
                countToReach = _totalTileCount;
            for (int currentValueAt = _startingCount;
                 currentValueAt < countToReach;
                 currentValueAt++) {
                if (_biomeSpecificDict.TryGetValue(_listOfLayersTiles[currentValueAt].tile.Tile.PrimaryBiome,
                        out ListOfTileSpecifics biomeLookingAt)) {
                    biomeLookingAt.AddTileSpecifics(_listOfLayersTiles[currentValueAt].tile.Tile);
                }
                else {
                    _biomeSpecificDict.Add(_listOfLayersTiles[currentValueAt].tile.Tile.PrimaryBiome,
                        new ListOfTileSpecifics(_listOfLayersTiles[currentValueAt].tile.Tile));
                }
                /*
                _layerMaxRainFall = Math.Max(_layerMaxRainFall, _listOfLayersTiles[currentValueAt].rainfall);
                _layerMinRainFall = Math.Min(_layerMinRainFall, _listOfLayersTiles[currentValueAt].rainfall);
                _layerMaxElevation = Math.Max(_layerMaxElevation, _listOfLayersTiles[currentValueAt].elevation);
                _layerMinElevation = Math.Min(_layerMinElevation, _listOfLayersTiles[currentValueAt].elevation);
                _layerMaxTemperature =
                    Math.Max(_layerMaxTemperature, _listOfLayersTiles[currentValueAt].MaxTemperature);
                _layerMinTemperature =
                    Math.Min(_layerMinTemperature, _listOfLayersTiles[currentValueAt].MinTemperature);*/
            }

            _startingCount = countToReach;
            Log.Message("Current count total " + _startingCount + " remaining " + (_totalTileCount - _startingCount));
            int prevTileCount = 0;
            if (_startingCount == _totalTileCount) {
                Log.Message("Biome counting is done!");
                _currentlyCalculating = false;
                _finishedCalculating = true;

                foreach (KeyValuePair<BiomeDef, ListOfTileSpecifics> biomeStuff in _biomeSpecificDict) {
                    _layerMaxRainFall = Math.Max(_layerMaxRainFall, biomeStuff.Value.BiomeMaxRainFall);
                    _layerMinRainFall = Math.Min(_layerMinRainFall, biomeStuff.Value.BiomeMinRainFall);
                    _layerMaxElevation = Math.Max(_layerMaxElevation, biomeStuff.Value.BiomeMaxElevation);
                    _layerMinElevation = Math.Min(_layerMinElevation, biomeStuff.Value.BiomeMinElevation);
                    _layerMaxTemperature = Math.Max(_layerMaxTemperature, biomeStuff.Value.BiomeMaxTemperature);
                    _layerMinTemperature = Math.Min(_layerMinTemperature, biomeStuff.Value.BiomeMinTemperature);
                    _layerMaxPollution = Math.Max(_layerMaxPollution, biomeStuff.Value.MaxPollution);
                    _layerMinPollution = Math.Min(_layerMinPollution, biomeStuff.Value.MinPollution);
                    _layerlandmarkCount += biomeStuff.Value.landMarkCount;

                    if (prevTileCount == 0) {
                        _layerAvgRainFall = biomeStuff.Value.AvgRainfall;
                        _layerAvgElevation = biomeStuff.Value.AvgElevation;
                        _layerAvgTemperature = biomeStuff.Value.AvgTemperature;
                        _layerAvgPollution = biomeStuff.Value.AvgPollution;
                    }
                    else {
                        _layerAvgRainFall = (_layerAvgRainFall + biomeStuff.Value.AvgRainfall) /
                                            (prevTileCount + biomeStuff.Value.TileCount);
                        _layerAvgElevation = (_layerAvgElevation + biomeStuff.Value.AvgElevation) /
                                             (prevTileCount + biomeStuff.Value.TileCount);
                        _layerAvgTemperature = (_layerAvgTemperature + biomeStuff.Value.AvgTemperature) /
                                               (prevTileCount + biomeStuff.Value.TileCount);
                        _layerAvgPollution = (_layerAvgPollution + biomeStuff.Value.AvgPollution) /
                                             (prevTileCount + biomeStuff.Value.TileCount);
                    }

                    prevTileCount = biomeStuff.Value.TileCount;
                }

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