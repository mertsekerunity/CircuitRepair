using System;
using System.Collections.Generic;
using CircuitRepair.Core;
using UnityEngine;

namespace CircuitRepair.Generation
{
    public static class RandomPuzzleLayoutGenerator
    {
        private const int MaxGenerationAttempts = 64;

        private static readonly Direction[] CardinalDirections =
        {
            Direction.North,
            Direction.East,
            Direction.South,
            Direction.West
        };

        public static GeneratedPuzzleLayout Generate(int width, int height, int seed)
        {
            if (width < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Generated puzzle width must be at least three.");
            }

            if (height < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(height), height, "Generated puzzle height must be at least three.");
            }

            System.Random random = new System.Random(seed);
            Vector2Int sourcePosition = Vector2Int.zero;
            Vector2Int groundPosition = new Vector2Int(width - 1, height - 1);

            for (int attempt = 0; attempt < MaxGenerationAttempts; attempt++)
            {
                Vector2Int bulbPosition = ChooseBulbPosition(width, height, sourcePosition, groundPosition, random);
                CircuitTopology topology = CreateFullGridTopology(width, height);

                RestrictPorts(topology, sourcePosition, Direction.East);
                RestrictPorts(topology, groundPosition, Direction.West);
                RestrictPorts(topology, bulbPosition, Direction.West | Direction.East);

                if (!TryRemoveUnsupportedConnections(topology, sourcePosition, groundPosition, random))
                {
                    continue;
                }

                GeneratedPuzzleLayout layout = new GeneratedPuzzleLayout(topology, sourcePosition, groundPosition, new[] { bulbPosition });

                if (GeneratedPuzzleLayoutValidator.GetErrors(layout).Count == 0)
                {
                    return layout;
                }
            }

            throw new InvalidOperationException($"Failed to generate a valid {width}x{height} puzzle layout after {MaxGenerationAttempts} attempts.");
        }

        private static CircuitTopology CreateFullGridTopology(int width, int height)
        {
            CircuitTopology topology = new CircuitTopology(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);

                    if (x < width - 1)
                    {
                        topology.Connect(position, Direction.East);
                    }

                    if (y < height - 1)
                    {
                        topology.Connect(position, Direction.South);
                    }
                }
            }

            return topology;
        }

        private static Vector2Int ChooseBulbPosition(int width, int height, Vector2Int sourcePosition, Vector2Int groundPosition, System.Random random)
        {
            List<Vector2Int> candidates = new();

            for (int y = 0; y < height; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);

                    if (GetManhattanDistance(position, sourcePosition) <= 1)
                    {
                        continue;
                    }

                    if (GetManhattanDistance(position, groundPosition) <= 1)
                    {
                        continue;
                    }

                    candidates.Add(position);
                }
            }

            if (candidates.Count == 0)
            {
                throw new InvalidOperationException("No valid horizontal bulb position could be found.");
            }

            return candidates[random.Next(candidates.Count)];
        }

        private static void RestrictPorts(CircuitTopology topology, Vector2Int position, Direction allowedPorts)
        {
            foreach (Direction direction in CardinalDirections)
            {
                if ((allowedPorts & direction) != 0)
                {
                    continue;
                }

                if (topology.HasConnection(position, direction))
                {
                    topology.Disconnect(position, direction);
                }
            }
        }

        private static bool TryRemoveUnsupportedConnections(CircuitTopology topology, Vector2Int sourcePosition, Vector2Int groundPosition, System.Random random)
        {
            while (true)
            {
                List<Vector2Int> degreeFourPositions = GetDegreeFourPositions(topology);

                if (degreeFourPositions.Count == 0)
                {
                    return true;
                }

                Shuffle(degreeFourPositions, random);
                bool removedConnection = false;

                foreach (Vector2Int position in degreeFourPositions)
                {
                    List<Direction> connectedDirections = GetConnectedDirections(topology, position);
                    Shuffle(connectedDirections, random);

                    foreach (Direction direction in connectedDirections)
                    {
                        if (!CanRemoveConnection(topology, position, direction, sourcePosition, groundPosition))
                        {
                            continue;
                        }

                        topology.Disconnect(position, direction);

                        if (IsFullyConnected(topology, sourcePosition))
                        {
                            removedConnection = true;
                            break;
                        }

                        topology.Connect(position, direction);
                    }

                    if (removedConnection)
                    {
                        break;
                    }
                }

                if (!removedConnection)
                {
                    return false;
                }
            }
        }

        private static List<Vector2Int> GetDegreeFourPositions(CircuitTopology topology)
        {
            List<Vector2Int> positions = new();

            for (int y = 0; y < topology.Height; y++)
            {
                for (int x = 0; x < topology.Width; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);

                    if (topology.GetDegree(position) == 4)
                    {
                        positions.Add(position);
                    }
                }
            }

            return positions;
        }

        private static List<Direction> GetConnectedDirections(CircuitTopology topology, Vector2Int position)
        {
            List<Direction> directions = new();

            foreach (Direction direction in CardinalDirections)
            {
                if (topology.HasConnection(position, direction))
                {
                    directions.Add(direction);
                }
            }

            return directions;
        }

        private static bool CanRemoveConnection(CircuitTopology topology, Vector2Int position, Direction direction, Vector2Int sourcePosition, Vector2Int groundPosition)
        {
            Vector2Int neighbourPosition = position + direction.ToGridOffset();

            int positionMinimumDegree = GetMinimumDegree(position, sourcePosition, groundPosition);
            int neighbourMinimumDegree = GetMinimumDegree(neighbourPosition, sourcePosition, groundPosition);

            if (topology.GetDegree(position) - 1 < positionMinimumDegree)
            {
                return false;
            }

            if (topology.GetDegree(neighbourPosition) - 1 < neighbourMinimumDegree)
            {
                return false;
            }

            return true;
        }

        private static int GetMinimumDegree(Vector2Int position, Vector2Int sourcePosition, Vector2Int groundPosition)
        {
            return position == sourcePosition || position == groundPosition ? 1 : 2;
        }

        private static bool IsFullyConnected(CircuitTopology topology, Vector2Int sourcePosition)
        {
            Queue<Vector2Int> positionsToVisit = new();
            HashSet<Vector2Int> visitedPositions = new();

            positionsToVisit.Enqueue(sourcePosition);
            visitedPositions.Add(sourcePosition);

            while (positionsToVisit.Count > 0)
            {
                Vector2Int currentPosition = positionsToVisit.Dequeue();

                foreach (Direction direction in CardinalDirections)
                {
                    if (!topology.HasConnection(currentPosition, direction))
                    {
                        continue;
                    }

                    Vector2Int neighbourPosition = currentPosition + direction.ToGridOffset();

                    if (visitedPositions.Add(neighbourPosition))
                    {
                        positionsToVisit.Enqueue(neighbourPosition);
                    }
                }
            }

            return visitedPositions.Count == topology.Width * topology.Height;
        }

        private static int GetManhattanDistance(Vector2Int first, Vector2Int second)
        {
            return Mathf.Abs(first.x - second.x) + Mathf.Abs(first.y - second.y);
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