using UnityEngine;

namespace PerformAnts
{
    public class Map
    {
        private Tile_Type[,] _tileTypes;
        private Vector2[] _pheromons;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public Tile_Type[,] TileTypes { get { return _tileTypes; } }
        public Vector2[] PheromonsData { get { return _pheromons; } }

        public Map(Tile_Type[,] tileTypes, Tile_Pheromon_Data[,] pheromons)
        {
            _tileTypes = tileTypes;
            Width = tileTypes.GetLength(0);
            Height = tileTypes.GetLength(1);

            _pheromons = new Vector2[Width * Height];

            for(int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    _pheromons[x * Height + y] = new Vector2(pheromons[x, y].homePheromonStrength, pheromons[x, y].resourcePheromonStrength);
                }
            }
        }

        public Tile_Type GetTileTypeAt(int x, int y)
        {
            return x < 0 || x >= _tileTypes.GetLength(0) || y < 0 || y >= _tileTypes.GetLength(1) ? Tile_Type.WALL : _tileTypes[x, y];
        }

        public void GetFoodFromTile(int x, int y)
        {
            if (x < 0 || x >= _tileTypes.GetLength(0) || y < 0 || y >= _tileTypes.GetLength(1))
            {
                throw new System.Exception("Error : Attempted to get food from a tile outside the map !");
            }

            if (_tileTypes[x, y] == Tile_Type.FOOD) _tileTypes[x, y] = Tile_Type.EMPTY;
        }

        public Tile_Pheromon_Data GetPheromonsAt(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                return new Tile_Pheromon_Data()
                {
                    homePheromonStrength = _pheromons[x * Height + y].x,
                    resourcePheromonStrength = _pheromons[x * Height + y].y
                };
            }
            else
            {
                return new Tile_Pheromon_Data()
                {
                    homePheromonStrength = 0f,
                    resourcePheromonStrength = 0f
                };
            }
        }

        public float GetAreaHomePheromonStrength(int x, int y, int radius)
        {
            return GetAreaPheromonStrength(x, y, radius, false);
        }

        public float GetAreaResourcePheromonStrength(int x, int y, int radius)
        {
            return GetAreaPheromonStrength(x, y, radius, true);
        }

        private float GetAreaPheromonStrength(int x, int y, int radius, bool resourcePheromon)
        {
            float max = 0f;
            for(int xCoord = x - radius; xCoord < x + radius; xCoord++)
            {
                for (int yCoord = y - radius; yCoord < y + radius; yCoord++)
                {
                    if (xCoord >= 0 && xCoord < Width && yCoord >= 0 && yCoord < Height)
                    {
                        var pheromons = GetPheromonsAt(xCoord, yCoord);
                        if (resourcePheromon)
                        {
                            float val;
                            if (TileTypes[xCoord, yCoord] == Tile_Type.FOOD)
                            {
                                val = 100f;
                            }
                            else
                            {
                                val = pheromons.resourcePheromonStrength;
                            }

                            if (val > max) max = val;
                        }
                        else
                        {
                            if (pheromons.homePheromonStrength > max)
                            {
                                max = pheromons.homePheromonStrength;
                            }
                        }
                    }
                }
            }

            return max;
        }
    }
}



