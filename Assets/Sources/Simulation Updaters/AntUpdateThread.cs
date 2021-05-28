using System.Threading;
using UnityEngine;

namespace PerformAnts
{
    public class AntUpdateThread
    {
        // Frame data
        private int _antCount;
        private int _startIndex;
        private Ant_Data[] _ants;
        private Map _map;
        private SimulationParams _simulationParameters;
        private float _deltaTime;

        // Thread data
        private Thread _thread;
        private bool _jobTrigger;
        private bool _stopTrigger;

        private System.Random _randomGen;

        public AntUpdateThread()
        {
            _jobTrigger = false;
            _stopTrigger = false;

            _randomGen = new System.Random((int)System.DateTime.Now.Ticks);

            _thread = new Thread(Thread);
            _thread.Start();
        }

        public void Stop()
        {
            _jobTrigger = false;
            _stopTrigger = true;
        }

        public void PlayFrame(Ant_Data[] ants, int startIndex, int antCount, Map map, SimulationParams parameters, float deltaTime)
        {
            _ants = ants;
            _antCount = antCount;
            _startIndex = startIndex;
            _map = map;
            _simulationParameters = parameters;
            _deltaTime = deltaTime;

            _jobTrigger = true;
        }

        public void Join()
        {
            while (_jobTrigger)
            {

            }
        }

        private void Thread()
        {
            while(!_stopTrigger)
            {
                if (_jobTrigger)
                {
                    for(int i = 0; i < _antCount; i++)
                    {
                        AntBehavior(_ants, _map, _startIndex + i, _deltaTime);
                    }
                    _jobTrigger = false;
                }
            }
        }

        private bool AntIsInNest(Ant_Data ant)
        {
            return (Mathf.Pow(_simulationParameters.nestPos.x - ant.x, 2) + Mathf.Pow(_simulationParameters.nestPos.y - ant.y, 2)) < 100f;
        }

        private void AntBehavior(Ant_Data[] ants, Map map, int id, float deltaTime)
        {
            var ant = ants[id];

            // Steering (random)
            float randomSteering = (_randomGen.Next(0, 1000000) / 1000000f - 0.5f) * 2f * _simulationParameters.randomSteeringWeight;

            Quaternion rotationFront = Quaternion.Euler(Vector3.forward * ant.steering);
            Quaternion rotationFrontLeft = Quaternion.Euler(Vector3.forward * (ant.steering + 45));
            Quaternion rotationFrontRight = Quaternion.Euler(Vector3.forward * (ant.steering - 45));

            Vector2 frontVectorSteering = rotationFront * Vector2.right;
            Vector2 leftVectorSteering = rotationFrontLeft * Vector2.right;
            Vector2 rightVectorSteering = rotationFrontRight * Vector2.right;

            // Steering (pheromons)

            float leftPheromons = 0f, rightPheromons = 0f, frontPheromons = 0f;
            if (ant.state == AntState.CARRYING_FOOD || ant.state == AntState.RETREATING) // If carrying food or unable to emit more home pheromons, start following home pheromons.
            {
                leftPheromons = map.GetAreaHomePheromonStrength((int)(ant.x + leftVectorSteering.x * _simulationParameters.pheromonSenseRadius), (int)(ant.y + leftVectorSteering.y * _simulationParameters.pheromonSenseRadius), _simulationParameters.pheromonSenseRadius);
                frontPheromons = map.GetAreaHomePheromonStrength((int)(ant.x + frontVectorSteering.x * _simulationParameters.pheromonSenseRadius), (int)(ant.y + frontVectorSteering.y * _simulationParameters.pheromonSenseRadius), _simulationParameters.pheromonSenseRadius);
                rightPheromons = map.GetAreaHomePheromonStrength((int)(ant.x + rightVectorSteering.x * _simulationParameters.pheromonSenseRadius), (int)(ant.y + rightVectorSteering.y * _simulationParameters.pheromonSenseRadius), _simulationParameters.pheromonSenseRadius);
            }
            else
            {
                leftPheromons = map.GetAreaResourcePheromonStrength((int)(ant.x + leftVectorSteering.x * _simulationParameters.pheromonSenseRadius), (int)(ant.y + leftVectorSteering.y * _simulationParameters.pheromonSenseRadius), _simulationParameters.pheromonSenseRadius);
                frontPheromons = map.GetAreaResourcePheromonStrength((int)(ant.x + frontVectorSteering.x * _simulationParameters.pheromonSenseRadius), (int)(ant.y + frontVectorSteering.y * _simulationParameters.pheromonSenseRadius), _simulationParameters.pheromonSenseRadius);
                rightPheromons = map.GetAreaResourcePheromonStrength((int)(ant.x + rightVectorSteering.x * _simulationParameters.pheromonSenseRadius), (int)(ant.y + rightVectorSteering.y * _simulationParameters.pheromonSenseRadius), _simulationParameters.pheromonSenseRadius);
            }

            float total = leftPheromons + rightPheromons + frontPheromons;
            bool pheromonGuided = total > _simulationParameters.minimumPheromonForSteering;
            if (pheromonGuided)
            {
                ant.steering += randomSteering / _simulationParameters.pheromonSteeringWeight + 45f * leftPheromons / total - 45f * rightPheromons / total;
            }
            else
            {
                ant.steering += randomSteering;
            }

            ant.steering = ant.steering % 360;
            if (ant.steering < 0f)
            {
                ant.steering = 360 + ant.steering;
            }


            // Leave pheromons

            if (ant.state == AntState.CARRYING_FOOD)
            {
                float add = 1f / (ant.timeSinceStateChange + 1) * _simulationParameters.pheromonEmissionMultiplier;
                map.PheromonsData[ant.ix * map.Height + ant.iy].y += add;

            }
            else if (ant.state == AntState.EXPLORING)
            {
                float add = 1f / (ant.timeSinceStateChange + 1) * _simulationParameters.pheromonEmissionMultiplier;
                map.PheromonsData[ant.ix * map.Height + ant.iy].x += add;

            }

            // Movement

            float newX, newY;
            newX = ant.x + frontVectorSteering.x;
            newY = ant.y + frontVectorSteering.y;

            if (map.GetTileTypeAt((int)newX, (int)newY) == Tile_Type.WALL)
            {
                ant.steering += 180;
                ant.steering %= 360;
            }
            else
            {
                // Perform the movement
                ant.x = newX;
                ant.y = newY;
                ant.ix = (int)newX;
                ant.iy = (int)newY;

                // Check for food or deposit
                if (ant.state != AntState.CARRYING_FOOD && map.TileTypes[ant.ix, ant.iy] == Tile_Type.FOOD)
                {
                    ant.state = AntState.CARRYING_FOOD;
                    ant.timeSinceStateChange = 0f;
                    map.GetFoodFromTile(ant.ix, ant.iy);
                    Debug.Log("Ant picked up food !");

                    // Turn around after picking up food.
                    ant.steering += 180;
                    ant.steering %= 360;
                }
                else if (AntIsInNest(ant))
                {
                    if (ant.state == AntState.CARRYING_FOOD)
                    {
                        ant.state = AntState.EXPLORING;
                        Debug.Log("Ant brought food back !");
                    }
                    else if (ant.state == AntState.RETREATING)
                    {
                        ant.state = AntState.EXPLORING;
                    }
                    ant.timeSinceStateChange = 0f;
                }
                else
                {
                    ant.timeSinceStateChange += deltaTime;
                    if (!pheromonGuided && ant.timeSinceStateChange > 60f && ant.state == AntState.EXPLORING)
                    {
                        ant.state = AntState.RETREATING;
                    }
                }
            }
            ants[id] = ant;
        }

    }
}



