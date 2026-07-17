using System.Collections.Generic;
using CircuitRepair.Core;
using CircuitRepair.Generation;
using UnityEngine;

namespace CircuitRepair.Debugging
{
    public sealed class SinglePathPuzzleLayoutDebugRunner : MonoBehaviour
    {
        private void Start()
        {
            FullGridPath path = SerpentinePathGenerator.Generate(5, 5);
            GeneratedPuzzleLayout layout = SinglePathPuzzleLayoutGenerator.Generate(path);

            IReadOnlyList<string> errors = GeneratedPuzzleLayoutValidator.GetErrors(layout);
            BoardData solvedBoard = GeneratedBoardFactory.CreateSolvedBoard(layout);
            CircuitEvaluationResult solvedResult = CircuitEvaluator.Evaluate(solvedBoard);

            Vector2Int bulbPosition = GetOnlyBulbPosition(layout);
            int bulbPathIndex = FindPathIndex(path, bulbPosition);

            Debug.Log("===== SINGLE PATH PUZZLE LAYOUT TEST =====");

            Check("Layout has no validation errors", errors.Count == 0, true);
            Check("Source has only East", layout.Topology.GetPorts(layout.SourcePosition) == Direction.East, true);
            Check("Ground has only West", layout.Topology.GetPorts(layout.GroundPosition) == Direction.West, true);
            Check("Bulb has horizontal ports", layout.Topology.GetPorts(bulbPosition) == (Direction.West | Direction.East), true);
            Check("Bulb is in the middle path range", bulbPathIndex >= 8 && bulbPathIndex <= 16, true);
            Check("Only source and ground have degree one", CountTilesWithDegree(layout.Topology, 1) == 2, true);
            Check("All other tiles have degree two", CountTilesWithDegree(layout.Topology, 2) == 23, true);
            Check("Layout contains no T-junctions", CountTilesWithDegree(layout.Topology, 3) == 0, true);
            Check("Generated board is solved", solvedResult.IsSolved, true);
        }

        private static Vector2Int GetOnlyBulbPosition(GeneratedPuzzleLayout layout)
        {
            foreach (Vector2Int bulbPosition in layout.BulbPositions)
            {
                return bulbPosition;
            }

            throw new System.InvalidOperationException("Layout does not contain a bulb.");
        }

        private static int FindPathIndex(FullGridPath path, Vector2Int position)
        {
            for (int i = 0; i < path.Count; i++)
            {
                if (path[i] == position)
                {
                    return i;
                }
            }

            return -1;
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