using System;
using UnityEngine;

namespace CircuitRepair.Core
{
    public sealed class BoardData
    {
        private readonly TileData[,] tiles;

        public int Width { get; }
        public int Height { get; }

        public BoardData(int width, int height)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(width),
                    width,
                    "Board width must be greater than zero.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(height),
                    height,
                    "Board height must be greater than zero.");
            }

            Width = width;
            Height = height;

            tiles = new TileData[width, height];
        }

        public bool IsInside(Vector2Int position)
        {
            return position.x >= 0 &&
                   position.x < Width &&
                   position.y >= 0 &&
                   position.y < Height;
        }

        public void SetTile(TileData tile)
        {
            if (tile == null)
            {
                throw new ArgumentNullException(nameof(tile));
            }

            if (!IsInside(tile.Position))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(tile),
                    tile.Position,
                    "Tile position is out of the board.");
            }

            tiles[tile.Position.x, tile.Position.y] = tile;
        }

        public TileData GetTile(Vector2Int position)
        {
            if (!IsInside(position))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(position),
                    position,
                    "Desired position is out of the board.");
            }

            return tiles[position.x, position.y];
        }

        public bool TryGetTile(Vector2Int position, out TileData tile)
        {
            if (!IsInside(position))
            {
                tile = null;
                return false;
            }

            tile = tiles[position.x, position.y];
            return tile != null;
        }
    }
}