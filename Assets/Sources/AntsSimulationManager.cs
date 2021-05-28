using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PerformAnts
{

    /// <summary>
    /// Simulation flow driver & resources manager
    /// </summary>
    public class AntsSimulationManager : MonoBehaviour
    {
        [SerializeField]
        private int _startingAntCount;
        [SerializeField]
        private Texture2D _startingGrid;
        [SerializeField]
        private MonoBehaviour _simulationUpdaterComponent;
        [SerializeField]
        private MonoBehaviour _simulationRendererComponent;

        private Ant_Data[] _ants;
        private Map _map;

        private ISimulationUpdater _simulationUpdater;
        private ISimulationRenderer _simulationRenderer;

        private void Awake()
        {
            _ants = new Ant_Data[_startingAntCount];
            Tile_Type[,] tileTypes = new Tile_Type[_startingGrid.width, _startingGrid.height];
            Tile_Pheromon_Data[,] pheromons = new Tile_Pheromon_Data[_startingGrid.width, _startingGrid.height];

            if (_simulationRendererComponent is ISimulationRenderer)
            {
                _simulationRenderer = (ISimulationRenderer)_simulationRendererComponent;
            }
            else
            {
                Debug.LogError("Error : Component " + _simulationRendererComponent.name + " is not a ISimulationRenderer !");
                enabled = false;
            }

            if (_simulationUpdaterComponent is ISimulationUpdater)
            {
                _simulationUpdater = (ISimulationUpdater)_simulationUpdaterComponent;
            }
            else
            {
                Debug.LogError("Error : Component " + _simulationUpdaterComponent.name + " is not a ISimulationUpdater !");
                enabled = false;
            }

            // Read map
            Color32[] pixels = _startingGrid.GetPixels32();
            for (int y = 0; y < _startingGrid.height; y++)
            {
                for(int x = 0; x < _startingGrid.width; x++)
                {
                    Color32 pixel = pixels[y * _startingGrid.width + x];
                    if (pixel == Color.green)
                    {
                        tileTypes[x, y] = Tile_Type.FOOD;
                    }
                }
            }

            _map = new Map(tileTypes, pheromons);

            _simulationUpdater.InitializeAnts(_ants);
            _simulationUpdater.InitializeMap(_map);
            _simulationRenderer.Initialize(_map);
        }

        private void Update()
        {
            _simulationRenderer.UpdateRenderer(_ants, _map);
            _simulationUpdater.PlayOneFrame(_ants, _map);
        }

        public void SetSimulationTimescale(float timeScale)
        {
            Time.timeScale = 5f * timeScale;
        }
    }
}



