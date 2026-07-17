using System.Collections.Generic;
using CircuitRepair.Core;
using CircuitRepair.Generation;
using UnityEngine;

namespace CircuitRepair.Debugging
{
    public sealed class RandomPuzzleLayoutGeneratorDebugRunner : MonoBehaviour
    {
        private const int TestSeed = 12345;

        private void Start()
        {
            GeneratedPuzzleLayout firstLayout = RandomPuzzleLayoutGenerator.Generate(5, 5, TestSeed, 6, 10);
            GeneratedPuzzleLayout secondLayout = RandomPuzzleLayoutGenerator.Generate(5, 5, TestSeed, 6, 10);

            IReadOnlyList<string> errors = GeneratedPuzzleLayoutValidator.GetErrors(firstLayout);
            BoardData solvedBoard = GeneratedBoardFactory.CreateSolvedBoard(firstLayout);
            CircuitEvaluationResult solvedResult = CircuitEvaluator.Evaluate(solvedBoard);

            int tJunctionCount = CountTilesWithDegree(firstLayout.Topology, 3);

            Debug.Log("===== RANDOM PUZZLE LAYOUT GENERATOR TEST =====");

            Check("Generated layout has no validation errors", errors.Count == 0, true);
            Check("Same seed creates the same topology", HaveMatchingTopologies(firstLayout.Topology, secondLayout.Topology), true);
            Check("Same seed creates the same bulb position", HaveMatchingBulbPositions(firstLayout, secondLayout), true);
            Check("Source has only East", firstLayout.Topology.GetPorts(firstLayout.SourcePosition) == Direction.East, true);
            Check("Ground has only West", firstLayout.Topology.GetPorts(firstLayout.GroundPosition) == Direction.West, true);
            Check("Bulb has horizontal ports", HasHorizontalBulb(firstLayout), true);
            Check("No cell has degree four", CountTilesWithDegree(firstLayout.Topology, 4) == 0, true);
            Check("T-junction count is even", tJunctionCount % 2 == 0, true);
            Check("Generated board starts solved", solvedResult.IsSolved, true);
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

        private static bool HaveMatchingBulbPositions(GeneratedPuzzleLayout first, GeneratedPuzzleLayout second)
        {
            foreach (Vector2Int bulbPosition in first.BulbPositions)
            {
                return second.IsBulb(bulbPosition);
            }

            return false;
        }

        private static bool HasHorizontalBulb(GeneratedPuzzleLayout layout)
        {
            foreach (Vector2Int bulbPosition in layout.BulbPositions)
            {
                return layout.Topology.GetPorts(bulbPosition) == (Direction.West | Direction.East);
            }

            return false;
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