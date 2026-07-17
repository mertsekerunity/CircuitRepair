using System;
using CircuitRepair.Core;
using UnityEngine;

namespace CircuitRepair.Generation
{
    public static class BoardScrambler
    {
        public static void Scramble(BoardData board, int seed)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            System.Random random = new System.Random(seed);
            int rotatableTileCount = 0;

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);

                    if (!board.TryGetTile(position, out TileData tile) || !tile.IsRotatable)
                    {
                        continue;
                    }

                    int quarterTurns = GetScrambleQuarterTurns(tile.Type, random);
                    RotateTile(tile, quarterTurns);
                    rotatableTileCount++;
                }
            }

            if (rotatableTileCount == 0)
            {
                throw new InvalidOperationException("Board does not contain any rotatable tiles to scramble.");
            }
        }

        private static int GetScrambleQuarterTurns(TileType type, System.Random random)
        {
            return type switch
            {
                TileType.Straight => 1,
                TileType.Bend or TileType.TJunction => random.Next(1, 4),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Tile type is not supported by the board scrambler.")
            };
        }

        private static void RotateTile(TileData tile, int quarterTurns)
        {
            for (int i = 0; i < quarterTurns; i++)
            {
                tile.RotateClockwise();
            }
        }
    }
}