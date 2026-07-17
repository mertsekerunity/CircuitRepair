using System;
using System.Collections.Generic;
using CircuitRepair.Core;
using CircuitRepair.Generation;
using UnityEngine;

namespace CircuitRepair.Debugging
{
    public sealed class GeneratedPuzzleLayoutValidatorDebugRunner : MonoBehaviour
    {
        private void Start()
        {
            GeneratedPuzzleLayout validLayout = CreateValidLayout();
            GeneratedPuzzleLayout invalidLayout = CreateInvalidLayout();

            IReadOnlyList<string> validErrors = GeneratedPuzzleLayoutValidator.GetErrors(validLayout);
            IReadOnlyList<string> invalidErrors = GeneratedPuzzleLayoutValidator.GetErrors(invalidLayout);

            Debug.Log("===== GENERATED LAYOUT VALIDATOR TEST =====");

            Check("Valid layout has no errors", validErrors.Count == 0, true);
            Check("Invalid layout reports errors", invalidErrors.Count > 0, true);
            Check("Invalid layout throws during validation", ThrowsValidationException(invalidLayout), true);
            Check("Valid layout creates a solved board", CircuitEvaluator.Evaluate(GeneratedBoardFactory.CreateSolvedBoard(validLayout)).IsSolved, true);
        }

        private static bool ThrowsValidationException(GeneratedPuzzleLayout layout)
        {
            try
            {
                GeneratedPuzzleLayoutValidator.ThrowIfInvalid(layout);
                return false;
            }
            catch (InvalidOperationException)
            {
                return true;
            }
        }

        private static GeneratedPuzzleLayout CreateValidLayout()
        {
            CircuitTopology topology = CreateSnakeTopology();

            return new GeneratedPuzzleLayout(
                topology,
                new Vector2Int(0, 0),
                new Vector2Int(2, 2),
                new[] { new Vector2Int(1, 2) });
        }

        private static GeneratedPuzzleLayout CreateInvalidLayout()
        {
            CircuitTopology topology = CreateSnakeTopology();

            return new GeneratedPuzzleLayout(
                topology,
                new Vector2Int(0, 0),
                new Vector2Int(1, 2),
                new[] { new Vector2Int(1, 0) });
        }

        private static CircuitTopology CreateSnakeTopology()
        {
            CircuitTopology topology = new CircuitTopology(3, 3);

            topology.Connect(new Vector2Int(0, 0), Direction.East);
            topology.Connect(new Vector2Int(1, 0), Direction.East);
            topology.Connect(new Vector2Int(2, 0), Direction.South);
            topology.Connect(new Vector2Int(2, 1), Direction.West);
            topology.Connect(new Vector2Int(1, 1), Direction.West);
            topology.Connect(new Vector2Int(0, 1), Direction.South);
            topology.Connect(new Vector2Int(0, 2), Direction.East);
            topology.Connect(new Vector2Int(1, 2), Direction.East);

            return topology;
        }

        private static void Check(string description, bool actual, bool expected)
        {
            string result = actual == expected ? "PASS" : "FAIL";
            Debug.Log($"{result} | {description} | Actual: {actual} | Expected: {expected}");
        }
    }
}