using System;
using System.Collections.Generic;
using CircuitRepair.Core;
using UnityEngine;

namespace CircuitRepair.Generation
{
    public static class RandomFullGridPathGenerator
    {
        private const int MaxExploredNodes = 250000;

        private static readonly Direction[] CardinalDirections =
        {
            Direction.North,
            Direction.East,
            Direction.South,
            Direction.West
        };

        public static FullGridPath Generate(int width, int height, int seed)
        {
            if (width < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Random full-grid path width must be at least three.");
            }

            if (height < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(height), height, "Random full-grid path height must be at least three.");
            }

            Vector2Int startPosition = Vector2Int.zero;
            Vector2Int endPosition = new Vector2Int(width - 1, height - 1);

            ValidateEndpointParity(width, height, startPosition, endPosition);

            Vector2Int firstStepPosition = startPosition + Direction.East.ToGridOffset();
            Vector2Int groundEntryPosition = endPosition + Direction.West.ToGridOffset();

            bool[,] visited = new bool[width, height];
            List<Vector2Int> path = new List<Vector2Int>(width * height);
            System.Random random = new System.Random(seed);

            visited[startPosition.x, startPosition.y] = true;
            visited[firstStepPosition.x, firstStepPosition.y] = true;

            path.Add(startPosition);
            path.Add(firstStepPosition);

            int exploredNodeCount = 0;
            bool pathFound = TryBuildPath(width, height, endPosition, groundEntryPosition, visited, path, random, ref exploredNodeCount);

            if (!pathFound)
            {
                throw new InvalidOperationException($"Failed to generate a random {width}x{height} full-grid path for seed {seed} after exploring {exploredNodeCount} search nodes.");
            }

            return new FullGridPath(width, height, path);
        }

        private static bool TryBuildPath(int width, int height, Vector2Int endPosition, Vector2Int groundEntryPosition, bool[,] visited, List<Vector2Int> path, System.Random random, ref int exploredNodeCount)
        {
            exploredNodeCount++;

            if (exploredNodeCount > MaxExploredNodes)
            {
                return false;
            }

            Vector2Int currentPosition = path[path.Count - 1];
            int totalPositionCount = width * height;

            if (path.Count == totalPositionCount)
            {
                return currentPosition == endPosition;
            }

            if (currentPosition == endPosition)
            {
                return false;
            }

            List<Vector2Int> candidates = GetCandidates(width, height, currentPosition, endPosition, groundEntryPosition, visited, path.Count);
            Shuffle(candidates, random);

            foreach (Vector2Int candidate in candidates)
            {
                visited[candidate.x, candidate.y] = true;
                path.Add(candidate);

                bool canContinue =
                    IsRemainingRegionConnected(width, height, candidate, visited, path.Count) &&
                    HaveValidRemainingDegrees(width, height, candidate, endPosition, visited);

                if (canContinue && TryBuildPath(width, height, endPosition, groundEntryPosition, visited, path, random, ref exploredNodeCount))
                {
                    return true;
                }

                path.RemoveAt(path.Count - 1);
                visited[candidate.x, candidate.y] = false;
            }

            return false;
        }

        private static List<Vector2Int> GetCandidates(int width, int height, Vector2Int currentPosition, Vector2Int endPosition, Vector2Int groundEntryPosition, bool[,] visited, int currentPathCount)
        {
            List<Vector2Int> candidates = new();
            int totalPositionCount = width * height;

            foreach (Direction direction in CardinalDirections)
            {
                Vector2Int candidate = currentPosition + direction.ToGridOffset();

                if (!IsInside(width, height, candidate) || visited[candidate.x, candidate.y])
                {
                    continue;
                }

                if (candidate == groundEntryPosition && currentPathCount != totalPositionCount - 2)
                {
                    continue;
                }

                if (candidate == endPosition)
                {
                    bool isFinalStep = currentPathCount == totalPositionCount - 1;
                    bool entersGroundFromWest = currentPosition == groundEntryPosition;

                    if (!isFinalStep || !entersGroundFromWest)
                    {
                        continue;
                    }
                }

                candidates.Add(candidate);
            }

            return candidates;
        }

        private static bool IsRemainingRegionConnected(int width, int height, Vector2Int currentPosition, bool[,] visited, int currentPathCount)
        {
            Queue<Vector2Int> positionsToVisit = new();
            bool[,] reached = new bool[width, height];

            positionsToVisit.Enqueue(currentPosition);
            reached[currentPosition.x, currentPosition.y] = true;

            int reachedCount = 1;

            while (positionsToVisit.Count > 0)
            {
                Vector2Int position = positionsToVisit.Dequeue();

                foreach (Direction direction in CardinalDirections)
                {
                    Vector2Int neighbour = position + direction.ToGridOffset();

                    if (!IsInside(width, height, neighbour) || reached[neighbour.x, neighbour.y])
                    {
                        continue;
                    }

                    if (visited[neighbour.x, neighbour.y])
                    {
                        continue;
                    }

                    reached[neighbour.x, neighbour.y] = true;
                    reachedCount++;
                    positionsToVisit.Enqueue(neighbour);
                }
            }

            int unvisitedCount = width * height - currentPathCount;
            int expectedReachableCount = unvisitedCount + 1;

            return reachedCount == expectedReachableCount;
        }

        private static bool HaveValidRemainingDegrees(int width, int height, Vector2Int currentPosition, Vector2Int endPosition, bool[,] visited)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (visited[x, y])
                    {
                        continue;
                    }

                    Vector2Int position = new Vector2Int(x, y);
                    int availableNeighbourCount = CountAvailableNeighbours(width, height, position, currentPosition, visited);
                    int requiredNeighbourCount = position == endPosition ? 1 : 2;

                    if (availableNeighbourCount < requiredNeighbourCount)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static int CountAvailableNeighbours(int width, int height, Vector2Int position, Vector2Int currentPosition, bool[,] visited)
        {
            int count = 0;

            foreach (Direction direction in CardinalDirections)
            {
                Vector2Int neighbour = position + direction.ToGridOffset();

                if (!IsInside(width, height, neighbour))
                {
                    continue;
                }

                if (neighbour == currentPosition || !visited[neighbour.x, neighbour.y])
                {
                    count++;
                }
            }

            return count;
        }

        private static void ValidateEndpointParity(int width, int height, Vector2Int startPosition, Vector2Int endPosition)
        {
            int totalPositionCount = width * height;

            bool endpointsHaveSameColor = (startPosition.x + startPosition.y) % 2 == (endPosition.x + endPosition.y) % 2;
            bool endpointsMustHaveSameColor = totalPositionCount % 2 == 1;

            if (endpointsHaveSameColor != endpointsMustHaveSameColor)
            {
                throw new InvalidOperationException($"A full-grid path from {startPosition} to {endPosition} is impossible on a {width}x{height} grid because of checkerboard parity.");
            }
        }

        private static bool IsInside(int width, int height, Vector2Int position)
        {
            return position.x >= 0 && position.x < width && position.y >= 0 && position.y < height;
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