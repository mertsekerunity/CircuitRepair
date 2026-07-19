using System;
using System.Collections.Generic;
using UnityEngine;

namespace CircuitRepair.Core
{
    public static class MultiCircuitEvaluator
    {
        public static MultiCircuitEvaluationResult Evaluate(BoardData board)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            List<MultiCircuitNetworkResult> networks = FindNetworks(board);
            bool allPortsConnected = AreAllPortsConnected(board);

            return new MultiCircuitEvaluationResult(networks, allPortsConnected);
        }

        private static List<MultiCircuitNetworkResult> FindNetworks(BoardData board)
        {
            List<MultiCircuitNetworkResult> networks = new();
            HashSet<Vector2Int> visitedPositions = new();

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);

                    if (!board.TryGetTile(position, out _) || visitedPositions.Contains(position))
                    {
                        continue;
                    }

                    HashSet<Vector2Int> networkPositions = TraverseNetwork(board, position, visitedPositions);
                    MultiCircuitNetworkResult network = EvaluateNetwork(board, networkPositions);

                    networks.Add(network);
                }
            }

            return networks;
        }

        private static HashSet<Vector2Int> TraverseNetwork(BoardData board, Vector2Int startPosition, HashSet<Vector2Int> visitedPositions)
        {
            Queue<Vector2Int> positionsToVisit = new();
            HashSet<Vector2Int> networkPositions = new();

            positionsToVisit.Enqueue(startPosition);
            visitedPositions.Add(startPosition);
            networkPositions.Add(startPosition);

            while (positionsToVisit.Count > 0)
            {
                Vector2Int currentPosition = positionsToVisit.Dequeue();

                foreach (Direction direction in DirectionUtility.CardinalDirections)
                {
                    if (!ConnectionRules.AreConnected(board, currentPosition, direction))
                    {
                        continue;
                    }

                    Vector2Int neighbourPosition = currentPosition + direction.ToGridOffset();

                    if (!visitedPositions.Add(neighbourPosition))
                    {
                        continue;
                    }

                    networkPositions.Add(neighbourPosition);
                    positionsToVisit.Enqueue(neighbourPosition);
                }
            }

            return networkPositions;
        }

        private static MultiCircuitNetworkResult EvaluateNetwork(BoardData board, IEnumerable<Vector2Int> positions)
        {
            List<Vector2Int> networkPositions = new(positions);

            int sourceCount = 0;
            int bulbCount = 0;
            int groundCount = 0;

            int totalSourceRating = 0;
            int totalBulbDemand = 0;

            foreach (Vector2Int position in networkPositions)
            {
                TileData tile = board.GetTile(position);

                switch (tile.Type)
                {
                    case TileType.Source:
                        sourceCount++;
                        totalSourceRating += tile.CurrentRating;
                        break;

                    case TileType.Bulb:
                        bulbCount++;
                        totalBulbDemand += tile.CurrentRating;
                        break;

                    case TileType.Ground:
                        groundCount++;
                        break;
                }
            }

            MultiCircuitNetworkState state = DetermineNetworkState(sourceCount, bulbCount, groundCount, totalSourceRating, totalBulbDemand);

            return new MultiCircuitNetworkResult(networkPositions, sourceCount, bulbCount, groundCount, totalSourceRating, totalBulbDemand, state);
        }

        private static MultiCircuitNetworkState DetermineNetworkState(int sourceCount, int bulbCount, int groundCount, int totalSourceRating, int totalBulbDemand)
        {
            if (sourceCount == 0)
            {
                return MultiCircuitNetworkState.NoSource;
            }

            if (sourceCount > 1)
            {
                return MultiCircuitNetworkState.MixedSources;
            }

            if (bulbCount == 0)
            {
                return MultiCircuitNetworkState.MissingBulb;
            }

            if (bulbCount > 1)
            {
                return MultiCircuitNetworkState.MultipleBulbs;
            }

            if (groundCount == 0)
            {
                return MultiCircuitNetworkState.MissingGround;
            }

            if (groundCount > 1)
            {
                return MultiCircuitNetworkState.MultipleGrounds;
            }

            if (totalSourceRating < totalBulbDemand)
            {
                return MultiCircuitNetworkState.Underpowered;
            }

            if (totalSourceRating > totalBulbDemand)
            {
                return MultiCircuitNetworkState.Overloaded;
            }

            return MultiCircuitNetworkState.Valid;
        }

        private static bool AreAllPortsConnected(BoardData board)
        {
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);

                    if (!board.TryGetTile(position, out _))
                    {
                        continue;
                    }

                    if (!ConnectionRules.AreAllPortsConnected(board, position))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}