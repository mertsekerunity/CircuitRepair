using System;
using System.Collections.Generic;
using CircuitRepair.Core;
using CircuitRepair.Generation;
using UnityEngine;

namespace CircuitRepair.Debugging
{
    public sealed class RandomSinglePathPuzzleStressDebugRunner : MonoBehaviour
    {
        [SerializeField] private int firstLayoutSeed;
        [SerializeField, Min(1)] private int seedCount = 1000;

        private void Start()
        {
            RunStressTest(5, 5);
        }

        private void RunStressTest(int width, int height)
        {
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            int minimumBendCount = int.MaxValue;
            int maximumBendCount = 0;
            int totalBendCount = 0;

            int minimumBulbDistance = int.MaxValue;
            int maximumBulbDistance = 0;
            int totalBulbDistance = 0;

            for (int offset = 0; offset < seedCount; offset++)
            {
                int layoutSeed = firstLayoutSeed + offset;
                int scrambleSeed = unchecked(layoutSeed * 397 ^ 7919);

                try
                {
                    GeneratedPuzzleLayout firstLayout = RandomSinglePathPuzzleLayoutGenerator.Generate(width, height, layoutSeed);
                    GeneratedPuzzleLayout secondLayout = RandomSinglePathPuzzleLayoutGenerator.Generate(width, height, layoutSeed);

                    IReadOnlyList<string> errors = GeneratedPuzzleLayoutValidator.GetErrors(firstLayout);

                    if (errors.Count > 0)
                    {
                        ReportFailure(width, height, layoutSeed, $"Validation errors: {string.Join(" | ", errors)}");
                        return;
                    }

                    if (!HaveMatchingLayouts(firstLayout, secondLayout))
                    {
                        ReportFailure(width, height, layoutSeed, "The same layout seed produced different layouts.");
                        return;
                    }

                    BoardData solvedBoard = GeneratedBoardFactory.CreateSolvedBoard(firstLayout);
                    CircuitEvaluationResult solvedResult = CircuitEvaluator.Evaluate(solvedBoard);

                    if (!solvedResult.IsSolved)
                    {
                        ReportFailure(width, height, layoutSeed, "The generated solved board was not evaluated as solved.");
                        return;
                    }

                    BoardData scrambledBoard = GeneratedBoardFactory.CreateSolvedBoard(firstLayout);
                    BoardScrambler.Scramble(scrambledBoard, scrambleSeed);
                    CircuitEvaluationResult scrambledResult = CircuitEvaluator.Evaluate(scrambledBoard);

                    if (scrambledResult.IsSolved)
                    {
                        ReportFailure(width, height, layoutSeed, "The scrambled board was still evaluated as solved.");
                        return;
                    }

                    int bendCount = CountTilesOfType(solvedBoard, TileType.Bend);
                    int bulbDistance = GetDistanceFromSourceToBulb(firstLayout);

                    minimumBendCount = Mathf.Min(minimumBendCount, bendCount);
                    maximumBendCount = Mathf.Max(maximumBendCount, bendCount);
                    totalBendCount += bendCount;

                    minimumBulbDistance = Mathf.Min(minimumBulbDistance, bulbDistance);
                    maximumBulbDistance = Mathf.Max(maximumBulbDistance, bulbDistance);
                    totalBulbDistance += bulbDistance;
                }
                catch (Exception exception)
                {
                    ReportFailure(width, height, layoutSeed, $"{exception.GetType().Name}: {exception.Message}");
                    Debug.LogException(exception, this);
                    return;
                }
            }

            stopwatch.Stop();

            float averageBendCount = (float)totalBendCount / seedCount;
            float averageBulbDistance = (float)totalBulbDistance / seedCount;

            Debug.Log(
                $"PASS | {width}x{height} | Seeds: {seedCount} | " +
                $"Bends Min/Avg/Max: {minimumBendCount}/{averageBendCount:F2}/{maximumBendCount} | " +
                $"Bulb Distance Min/Avg/Max: {minimumBulbDistance}/{averageBulbDistance:F2}/{maximumBulbDistance} | " +
                $"Time: {stopwatch.ElapsedMilliseconds} ms",
                this);
        }

        private static bool HaveMatchingLayouts(GeneratedPuzzleLayout first, GeneratedPuzzleLayout second)
        {
            if (first.SourcePosition != second.SourcePosition || first.GroundPosition != second.GroundPosition)
            {
                return false;
            }

            if (first.BulbPositions.Count != second.BulbPositions.Count)
            {
                return false;
            }

            foreach (Vector2Int bulbPosition in first.BulbPositions)
            {
                if (!second.IsBulb(bulbPosition))
                {
                    return false;
                }
            }

            return HaveMatchingTopologies(first.Topology, second.Topology);
        }

        private static bool HaveMatchingTopologies(CircuitTopology first, CircuitTopology second)
        {
            if (first.Width != second.Width || first.Height != second.Height)
            {
                return false;
            }

            for (int y = 0; y < first.Height; y++)
            {
                for (int x = 0; x < first.Width; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);

                    if (first.GetPorts(position) != second.GetPorts(position))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static int CountTilesOfType(BoardData board, TileType type)
        {
            int count = 0;

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    if (board.GetTile(new Vector2Int(x, y)).Type == type)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private static int GetDistanceFromSourceToBulb(GeneratedPuzzleLayout layout)
        {
            Vector2Int bulbPosition = GetOnlyBulbPosition(layout);
            Queue<(Vector2Int position, int distance)> positionsToVisit = new();
            HashSet<Vector2Int> visitedPositions = new();

            positionsToVisit.Enqueue((layout.SourcePosition, 0));
            visitedPositions.Add(layout.SourcePosition);

            while (positionsToVisit.Count > 0)
            {
                (Vector2Int position, int distance) = positionsToVisit.Dequeue();

                if (position == bulbPosition)
                {
                    return distance;
                }

                foreach (Direction direction in DirectionUtility.CardinalDirections)
                {
                    if (!layout.Topology.HasConnection(position, direction))
                    {
                        continue;
                    }

                    Vector2Int neighbourPosition = position + direction.ToGridOffset();

                    if (visitedPositions.Add(neighbourPosition))
                    {
                        positionsToVisit.Enqueue((neighbourPosition, distance + 1));
                    }
                }
            }

            throw new InvalidOperationException("Bulb could not be reached from the source.");
        }

        private static Vector2Int GetOnlyBulbPosition(GeneratedPuzzleLayout layout)
        {
            foreach (Vector2Int bulbPosition in layout.BulbPositions)
            {
                return bulbPosition;
            }

            throw new InvalidOperationException("Layout does not contain a bulb.");
        }

        private void ReportFailure(int width, int height, int layoutSeed, string reason)
        {
            Debug.LogError($"FAIL | {width}x{height} | Layout Seed: {layoutSeed} | {reason}", this);
        }
    }
}