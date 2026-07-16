using System;
using System.Collections.Generic;
using UnityEngine;

namespace CircuitRepair.Core
{
    public static class CircuitEvaluator
    {
        private static readonly Direction[] CardinalDirections =
        {
            Direction.North,
            Direction.East,
            Direction.South,
            Direction.West
        };

        public static CircuitEvaluationResult Evaluate(BoardData board)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            Vector2Int sourcePosition = FindSingleSource(board);
            HashSet<Vector2Int> poweredPositions = TraversePoweredNetwork(board, sourcePosition);

            bool groundReached = false;
            bool allBulbsPowered = true;
            bool allPortsConnected = true;
            int bulbCount = 0;

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);

                    if (!board.TryGetTile(position, out TileData tile))
                    {
                        continue;
                    }

                    if (tile.Type == TileType.Ground && poweredPositions.Contains(position))
                    {
                        groundReached = true;
                    }

                    if (tile.Type == TileType.Bulb)
                    {
                        bulbCount++;

                        if (!poweredPositions.Contains(position))
                        {
                            allBulbsPowered = false;
                        }
                    }

                    if (!ConnectionRules.AreAllPortsConnected(board, position))
                    {
                        allPortsConnected = false;
                    }
                }
            }

            if (bulbCount == 0)
            {
                allBulbsPowered = false;
            }

            return new CircuitEvaluationResult(poweredPositions, groundReached, allBulbsPowered, allPortsConnected);
        }

        private static HashSet<Vector2Int> TraversePoweredNetwork(BoardData board, Vector2Int sourcePosition)
        {
            Queue<Vector2Int> positionsToVisit = new Queue<Vector2Int>();
            HashSet<Vector2Int> poweredPositions = new HashSet<Vector2Int>();

            positionsToVisit.Enqueue(sourcePosition);
            poweredPositions.Add(sourcePosition);

            while (positionsToVisit.Count > 0)
            {
                Vector2Int currentPosition = positionsToVisit.Dequeue();

                foreach (Direction direction in CardinalDirections)
                {
                    if (!ConnectionRules.AreConnected(board, currentPosition, direction))
                    {
                        continue;
                    }

                    Vector2Int neighbourPosition = currentPosition + direction.ToGridOffset();

                    if (poweredPositions.Add(neighbourPosition))
                    {
                        positionsToVisit.Enqueue(neighbourPosition);
                    }
                }
            }

            return poweredPositions;
        }

        private static Vector2Int FindSingleSource(BoardData board)
        {
            bool sourceFound = false;
            Vector2Int sourcePosition = default;

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);

                    if (!board.TryGetTile(position, out TileData tile) || tile.Type != TileType.Source)
                    {
                        continue;
                    }

                    if (sourceFound)
                    {
                        throw new InvalidOperationException("Board must contain exactly one source tile.");
                    }

                    sourceFound = true;
                    sourcePosition = position;
                }
            }

            if (!sourceFound)
            {
                throw new InvalidOperationException("Board must contain exactly one source tile.");
            }

            return sourcePosition;
        }
    }
}