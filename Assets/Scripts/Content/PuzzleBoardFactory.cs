using System;
using CircuitRepair.Core;
using UnityEngine;

namespace CircuitRepair.Content
{
    public static class PuzzleBoardFactory
    {
        public static BoardData Create(PuzzleDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            BoardData board = new BoardData(definition.Width, definition.Height);

            foreach (TileDefinition tileDefinition in definition.Tiles)
            {
                if (tileDefinition == null)
                {
                    throw new InvalidOperationException("Puzzle definition contains a null tile entry.");
                }

                Vector2Int position = tileDefinition.Position;

                if (!board.IsInside(position))
                {
                    throw new InvalidOperationException($"Tile at position {position} is outside the board boundaries.");
                }

                if (board.TryGetTile(position, out _))
                {
                    throw new InvalidOperationException($"Puzzle definition contains more than one tile at position {position}.");
                }

                TileData tile = new TileData(position, tileDefinition.Type, tileDefinition.InitialRotation);
                board.SetTile(tile);
            }

            EnsureBoardIsComplete(board);
            return board;
        }

        private static void EnsureBoardIsComplete(BoardData board)
        {
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);

                    if (!board.TryGetTile(position, out _))
                    {
                        throw new InvalidOperationException($"Puzzle definition does not contain a tile at position {position}.");
                    }
                }
            }
        }
    }
}