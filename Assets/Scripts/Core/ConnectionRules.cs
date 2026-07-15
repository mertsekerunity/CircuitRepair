using System;
using UnityEngine;

namespace CircuitRepair.Core
{
    public static class ConnectionRules
    {
        private static readonly Direction[] CardinalDirections =
        {
            Direction.North,
            Direction.East,
            Direction.South,
            Direction.West
        };

        public static bool AreConnected(BoardData board, Vector2Int tilePosition, Direction direction)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (!direction.IsCardinalDirection())
            {
                throw new ArgumentException(
                    "Connection control must be done with a single direction.",
                    nameof(direction));
            }

            if (!board.TryGetTile(tilePosition, out TileData tile))
            {
                return false;
            }

            Vector2Int neighbourPosition = tilePosition + direction.ToGridOffset();

            if (!board.TryGetTile(neighbourPosition, out TileData neighbour))
            {
                return false;
            }

            bool tileFacesNeighbour = tile.HasPort(direction);

            bool neighbourFacesTile = neighbour.HasPort(direction.Opposite());

            return tileFacesNeighbour && neighbourFacesTile;
        }

        public static bool AreAllPortsConnected(BoardData board, Vector2Int tilePosition)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (!board.TryGetTile(tilePosition, out TileData tile))
            {
                return false;
            }

            foreach (Direction direction in CardinalDirections)
            {
                if (!tile.HasPort(direction))
                {
                    continue;
                }

                if (!AreConnected(board, tilePosition, direction))
                {
                    return false;
                }
            }

            return true;
        }
    }
}