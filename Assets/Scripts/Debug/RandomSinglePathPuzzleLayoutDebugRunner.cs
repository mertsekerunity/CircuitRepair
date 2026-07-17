using System.Collections.Generic;
using CircuitRepair.Core;
using CircuitRepair.Generation;
using UnityEngine;

namespace CircuitRepair.Debugging
{
    public sealed class RandomSinglePathPuzzleLayoutDebugRunner : MonoBehaviour
    {
        private const int TestLayoutSeed = 12345;

        private void Start()
        {
            GeneratedPuzzleLayout firstLayout = RandomSinglePathPuzzleLayoutGenerator.Generate(5, 5, TestLayoutSeed);
            GeneratedPuzzleLayout secondLayout = RandomSinglePathPuzzleLayoutGenerator.Generate(5, 5, TestLayoutSeed);

            IReadOnlyList<string> errors = GeneratedPuzzleLayoutValidator.GetErrors(firstLayout);
            BoardData solvedBoard = GeneratedBoardFactory.CreateSolvedBoard(firstLayout);
            CircuitEvaluationResult result = CircuitEvaluator.Evaluate(solvedBoard);
            Vector2Int bulbPosition = GetOnlyBulbPosition(firstLayout);

            Debug.Log("===== RANDOM SINGLE-PATH LAYOUT TEST =====");

            Check("Generated layout has no validation errors", errors.Count == 0, true);
            Check("Same layout seed creates the same topology", HaveMatchingTopologies(firstLayout.Topology, secondLayout.Topology), true);
            Check("Same layout seed creates the same bulb position", secondLayout.IsBulb(bulbPosition), true);
            Check("Source has only East", firstLayout.Topology.GetPorts(firstLayout.SourcePosition) == Direction.East, true);
            Check("Ground has only West", firstLayout.Topology.GetPorts(firstLayout.GroundPosition) == Direction.West, true);
            Check("Bulb has horizontal ports", firstLayout.Topology.GetPorts(bulbPosition) == (Direction.West | Direction.East), true);
            Check("Only source and ground have degree one", CountTilesWithDegree(firstLayout.Topology, 1) == 2, true);
            Check("All other tiles have degree two", CountTilesWithDegree(firstLayout.Topology, 2) == 23, true);
            Check("Layout contains no T-junctions", CountTilesWithDegree(firstLayout.Topology, 3) == 0, true);
            Check("Generated board starts solved", result.IsSolved, true);
        }

        private static Vector2Int GetOnlyBulbPosition(GeneratedPuzzleLayout layout)
        {
            foreach (Vector2Int bulbPosition in layout.BulbPositions)
            {
                return bulbPosition;
            }

            throw new System.InvalidOperationException("Layout does not contain a bulb.");
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

        private static void Check(string description, bool actual, bool expected)
        {
            string result = actual == expected ? "PASS" : "FAIL";
            Debug.Log($"{result} | {description} | Actual: {actual} | Expected: {expected}");
        }
    }
}