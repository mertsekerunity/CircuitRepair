using System;
using CircuitRepair.Core;
using UnityEngine;

namespace CircuitRepair.Gameplay
{
    public sealed class BoardController
    {
        public BoardData Board { get; }

        public event Action<Vector2Int> TileRotated;

        public BoardController(BoardData board)
        {
            Board = board ?? throw new ArgumentNullException(nameof(board));
        }

        public bool TryRotateTile(Vector2Int position)
        {
            if (!Board.TryGetTile(position, out TileData tile))
            {
                return false;
            }

            if (!tile.IsRotatable)
            {
                return false;
            }

            tile.RotateClockwise();
            TileRotated?.Invoke(position);

            return true;
        }
    }
}