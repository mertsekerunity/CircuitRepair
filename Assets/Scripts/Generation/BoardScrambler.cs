using System;
using System.Collections.Generic;
using CircuitRepair.Core;
using UnityEngine;

namespace CircuitRepair.Generation
{
    public static class BoardScrambler
    {
        public static void Scramble(BoardData board, int seed, int minimumScrambledTileCount = 16, int maximumScrambledTileCount = 20)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            List<TileData> rotatableTiles = GetRotatableTiles(board);

            ValidateScrambleRange(rotatableTiles.Count, minimumScrambledTileCount, maximumScrambledTileCount);

            System.Random random = new System.Random(seed);
            Shuffle(rotatableTiles, random);

            int scrambledTileCount = random.Next(minimumScrambledTileCount, maximumScrambledTileCount + 1);

            for (int i = 0; i < scrambledTileCount; i++)
            {
                ScrambleTile(rotatableTiles[i], random);
            }
        }

        private static List<TileData> GetRotatableTiles(BoardData board)
        {
            List<TileData> tiles = new();

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);

                    if (board.TryGetTile(position, out TileData tile) && tile.IsRotatable)
                    {
                        tiles.Add(tile);
                    }
                }
            }

            if (tiles.Count == 0)
            {
                throw new InvalidOperationException("Board does not contain any rotatable tiles to scramble.");
            }

            return tiles;
        }

        private static void ScrambleTile(TileData tile, System.Random random)
        {
            int quarterTurns = tile.Type switch
            {
                TileType.Straight => 1,
                TileType.Bend or TileType.TJunction => random.Next(1, 4),
                _ => throw new ArgumentOutOfRangeException(nameof(tile.Type), tile.Type, "Tile type is not supported by the board scrambler.")
            };

            for (int i = 0; i < quarterTurns; i++)
            {
                tile.RotateClockwise();
            }
        }

        private static void ValidateScrambleRange(int rotatableTileCount, int minimumCount, int maximumCount)
        {
            if (minimumCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumCount), minimumCount, "Minimum scrambled tile count must be at least one.");
            }

            if (maximumCount < minimumCount)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumCount), maximumCount, "Maximum scrambled tile count cannot be lower than the minimum.");
            }

            if (maximumCount > rotatableTileCount)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumCount), maximumCount, $"Maximum scrambled tile count cannot exceed the {rotatableTileCount} rotatable tiles on the board.");
            }
        }

        private static void Shuffle<T>(IList<T> values, System.Random random)
        {
            for (int i = values.Count - 1; i > 0; i--)
            {
                int swapIndex = random.Next(i + 1);
                (values[i], values[swapIndex]) = (values[swapIndex], values[i]);
            }
        }
    }
}