using UnityEngine;

namespace PerformAnts
{
    public class GameObjectBasedSimulationRenderer : MonoBehaviour, ISimulationRenderer
    {
        [SerializeField]
        private GameObject _antGameObject;
        [SerializeField]
        private float _antPositionToScenePosition = 100f;
        [SerializeField]
        private SpriteRenderer _targetSpriteRenderer;

        private GameObject[] _spawnedAntGameObjects;
        private Material _targetMaterial;

        private ComputeBuffer _tileTypeBuffer;
        private ComputeBuffer _tilePheromonsBuffer;

        private void SpawnGameObjects(int amount)
        {
            _spawnedAntGameObjects = new GameObject[amount];

            for(int i = 0; i < amount; i++)
            {
                _spawnedAntGameObjects[i] = Instantiate(_antGameObject, Vector3.zero, Quaternion.identity);
            }
        }

        public void UpdateRenderer(Ant_Data[] ants, Map map)
        {
            if (_spawnedAntGameObjects == null)
            {
                SpawnGameObjects(ants.Length);
            }

            for(int antID = 0; antID < ants.Length; antID++)
            {
                _spawnedAntGameObjects[antID].transform.position = new Vector2(ants[antID].ix, ants[antID].iy) / _antPositionToScenePosition;
                if (ants[antID].state == AntState.CARRYING_FOOD)
                {
                    _spawnedAntGameObjects[antID].GetComponent<Renderer>().material.color = Color.green;
                }
                else if (ants[antID].state == AntState.EXPLORING)
                {
                    _spawnedAntGameObjects[antID].GetComponent<Renderer>().material.color = Color.white;
                }
                else
                {
                    _spawnedAntGameObjects[antID].GetComponent<Renderer>().material.color = Color.red;
                }
            }

            // Update Shader
            UpdateMapTileData(map);
            UpdateMapPheromonData(map);

            for(int goID = 0; goID < _spawnedAntGameObjects.Length; goID++)
            {
                Quaternion rotationFront = Quaternion.Euler(Vector3.forward * ants[goID].steering);
                Debug.DrawRay(_spawnedAntGameObjects[goID].transform.position, rotationFront * Vector3.right);
            }
        }

        public void Initialize(Map map)
        {
            int width, height;
            width = map.Width;
            height = map.Height;
            _targetSpriteRenderer.sprite = Sprite.Create(new Texture2D(width, height), new Rect(Vector2.zero, new Vector2(width, height)), Vector2.zero, _antPositionToScenePosition);
            _targetMaterial = _targetSpriteRenderer.material;

            _targetMaterial.SetInt("width", width);
            _targetMaterial.SetInt("height", height);

            _tileTypeBuffer = new ComputeBuffer(width * height, sizeof(int), ComputeBufferType.Default);
            _tilePheromonsBuffer = new ComputeBuffer(width * height, sizeof(float) * 2, ComputeBufferType.Default);

            UpdateMapTileData(map);
            UpdateMapPheromonData(map);
        }

        private void UpdateMapTileData(Map map)
        {
            int width = map.Width;
            int height = map.Height;
            int[] data = new int[width * height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int n;
                    switch (map.TileTypes[x, y])
                    {
                        case (Tile_Type.EMPTY):
                            n = 0;
                            break;
                        case (Tile_Type.FOOD):
                            n = 1;
                            break;
                        case (Tile_Type.WALL):
                            n = 2;
                            break;
                        default:
                            n = 0;
                            break;
                    }

                    data[x * height + y] = n;
                }
            }

            _tileTypeBuffer.SetData(data);
            _targetMaterial.SetBuffer("grid", _tileTypeBuffer);
        }

        private void UpdateMapPheromonData(Map map)
        {
            _tilePheromonsBuffer.SetData(map.PheromonsData);
            _targetMaterial.SetBuffer("pheromons", _tilePheromonsBuffer);
        }

        private void OnDestroy()
        {
            _tileTypeBuffer.Dispose();
            _tilePheromonsBuffer.Dispose();
        }
    }
}



