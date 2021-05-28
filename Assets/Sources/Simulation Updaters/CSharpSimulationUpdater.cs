using System;
using UnityEngine;

namespace PerformAnts
{

    [Serializable]
    public struct SimulationParams
    {
        public float randomSteeringWeight;
        public float pheromonSteeringWeight;
        public float homePheromonDecayRate;
        public float resourcePheromonDecayRate;
        public int pheromonSenseRadius;
        public float minimumPheromonForSteering;
        public float pheromonEmissionMultiplier;
        public Vector2 nestPos;
    }


    /// <summary>
    /// Standard C# updater.
    /// </summary>
    public class CSharpSimulationUpdater : MonoBehaviour, ISimulationUpdater
    {
        [SerializeField]
        private SimulationParams _simulationParameters;
        [SerializeField]
        private ComputeShader _pheromonDecayComputeShader;
        [SerializeField]
        private int _antUpdateThreadCount;
        [SerializeField]
        private bool _pheromonDecay = true;

        private ComputeBuffer _pheromonBuffer;

        private AntUpdateThread[] _antUpdateThreads;
        

        public void InitializeAnts(Ant_Data[] ants)
        {
            for(int antID = 0; antID < ants.Length; antID++)
            {
                ants[antID] = new Ant_Data()
                {
                    steering = UnityEngine.Random.value * 360f,
                    state = AntState.EXPLORING,
                    x = _simulationParameters.nestPos.x,
                    ix = (int)_simulationParameters.nestPos.x,
                    y = _simulationParameters.nestPos.y,
                    iy = (int)_simulationParameters.nestPos.y
                };
            }

            _antUpdateThreads = new AntUpdateThread[_antUpdateThreadCount];
            for(int i = 0; i < _antUpdateThreadCount; i++)
            {
                _antUpdateThreads[i] = new AntUpdateThread();
            }
        }

        public void InitializeMap(Map map)
        {
            _pheromonBuffer = new ComputeBuffer(map.Width * map.Height, sizeof(float) * 2, ComputeBufferType.Default);
            _pheromonBuffer.SetData(map.PheromonsData);
            _pheromonDecayComputeShader.SetBuffer(_pheromonDecayComputeShader.FindKernel("CSMain"), "Result", _pheromonBuffer);
           
            _pheromonDecayComputeShader.SetInt("Width", map.Width);
            _pheromonDecayComputeShader.SetInt("WidthPerThread", map.Width / 32);
            _pheromonDecayComputeShader.SetInt("Height", map.Height);
            _pheromonDecayComputeShader.SetInt("HeightPerThread", map.Width / 32);

            _pheromonDecayComputeShader.SetFloat("HomePheromonDecayRate", _simulationParameters.homePheromonDecayRate);
            _pheromonDecayComputeShader.SetFloat("ResourcePheromonDecayRate", _simulationParameters.resourcePheromonDecayRate);
        }

        public void PlayOneFrame(Ant_Data[] ants, Map map)
        {
            int antsPerThread = ants.Length / _antUpdateThreadCount;
            for(int i = 0; i < _antUpdateThreadCount; i++)
            {
                _antUpdateThreads[i].PlayFrame(ants, antsPerThread * i, antsPerThread, map, _simulationParameters, Time.deltaTime);
            }
            for (int i = 0; i < _antUpdateThreadCount; i++)
            {
                _antUpdateThreads[i].Join();
            }

            UpdatePheromons(map);
        }

        private void UpdatePheromons(Map map)
        {
            // Decay
            if (_pheromonDecay)
            {
                _pheromonBuffer.SetData(map.PheromonsData);
                _pheromonDecayComputeShader.Dispatch(_pheromonDecayComputeShader.FindKernel("CSMain"), 8, 8, 1);
                _pheromonBuffer.GetData(map.PheromonsData);
            }


            // Always emit very high levels of "home" pheromons near nest
            for (int x = (int)_simulationParameters.nestPos.x - 10; x < (int)_simulationParameters.nestPos.x + 10; x++)
            {
                for (int y = (int)_simulationParameters.nestPos.y - 10; y < (int)_simulationParameters.nestPos.y + 10; y++)
                {
                    if (x >= 0 && x < map.Width && y >= 0 && y < map.Height)
                    {
                        map.PheromonsData[x * map.Height + y].x = 10f;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (_pheromonBuffer != null)
            {
                _pheromonBuffer.Dispose();
                for (int i = 0; i < _antUpdateThreadCount; i++)
                {
                    _antUpdateThreads[i].Stop();
                }
            }

        }
    }
}



