using System;
using System.Collections.Generic;
using CircuitRepair.Core;
using CircuitRepair.Generation;
using UnityEngine;

namespace CircuitRepair.Debugging
{
    public sealed class RandomPuzzleLayoutStressDebugRunner : MonoBehaviour
    {
        [SerializeField] private int firstSeed;
        [SerializeField, Min(1)] private int seedCount = 250;

        private void Start()
        {
            RunStressTest(5, 5);
            RunStressTest(6, 6);
        }

        private void RunStressTest(int width, int height)
        {
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            int minimumTJunctionCount = int.MaxValue;
            int maximumTJunctionCount = 0;
            int totalTJunctionCount = 0;

            for (int offset = 0; offset < seedCount; offset++)
            {
                int seed = firstSeed + offset;

                try
                {
                    GeneratedPuzzleLayout firstLayout = RandomPuzzleLayoutGenerator.Generate(width, height, seed);
                    GeneratedPuzzleLayout secondLayout = RandomPuzzleLayoutGenerator.Generate(width, height, seed);

                    IReadOnlyList<string> errors = GeneratedPuzzleLayoutValidator.GetErrors(firstLayout);

                    if (errors.Count > 0)
                    {
                        ReportFailure(width, height, seed, $"Validation errors: {string.Join(" | ", errors)}");
                        return;
                    }

                    if (!HaveMatchingLayouts(firstLayout, secondLayout))
                    {
                        ReportFailure(width, height, seed, "The same seed produced different layouts.");
                        return;
                    }

                    BoardData solvedBoard = GeneratedBoardFactory.CreateSolvedBoard(firstLayout);
                    CircuitEvaluationResult result = CircuitEvaluator.Evaluate(solvedBoard);

                    if (!result.IsSolved)
                    {
                        ReportFailure(width, height, seed, "The generated solved board was not evaluated as solved.");
                        return;
                    }

                    BoardData scrambledBoard = GeneratedBoardFactory.CreateSolvedBoard(firstLayout);
                    BoardScrambler.Scramble(scrambledBoard, seed);
                    CircuitEvaluationResult scrambledResult = CircuitEvaluator.Evaluate(scrambledBoard);

                    if (scrambledResult.IsSolved)
                    {
                        ReportFailure(width, height, seed, "The scrambled board was still evaluated as solved.");
                        return;
                    }

                    int tJunctionCount = CountTilesWithDegree(firstLayout.Topology, 3);

                    if (tJunctionCount % 2 != 0)
                    {
                        ReportFailure(width, height, seed, $"The T-junction count was odd: {tJunctionCount}.");
                        return;
                    }

                    minimumTJunctionCount = Mathf.Min(minimumTJunctionCount, tJunctionCount);
                    maximumTJunctionCount = Mathf.Max(maximumTJunctionCount, tJunctionCount);
                    totalTJunctionCount += tJunctionCount;
                }
                catch (Exception exception)
                {
                    ReportFailure(width, height, seed, $"{exception.GetType().Name}: {exception.Message}");
                    Debug.LogException(exception, this);
                    return;
                }
            }

            stopwatch.Stop();

            float averageTJunctionCount = (float)totalTJunctionCount / seedCount;

            Debug.Log(
                $"PASS | {width}x{height} | Seeds: {seedCount} | " +
                $"T-junctions Min/Avg/Max: {minimumTJunctionCount}/{averageTJunctionCount:F2}/{maximumTJunctionCount} | " +
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

        private static int CountTilesWithDegree(CircuitTopology topology, int degree)
        {
            int count = 0;

            for (int y = 0; y < topology.Height; y++)
            {
                for (int x = 0; x < topology.Width; x++)
                {
                    if (topology.GetDegree(new Vector2Int(x, y)) == degree)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private void ReportFailure(int width, int height, int seed, string reason)
        {
            Debug.LogError($"FAIL | {width}x{height} | Seed: {seed} | {reason}", this);
        }
    }
}