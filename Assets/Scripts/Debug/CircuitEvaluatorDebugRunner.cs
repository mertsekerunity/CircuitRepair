using CircuitRepair.Core;
using CircuitRepair.Gameplay;
using UnityEngine;

namespace CircuitRepair.Debugging
{
    public sealed class CircuitEvaluatorDebugRunner : MonoBehaviour
    {
        private void Start()
        {
            BoardData board = PrototypeBoardFactory.CreatePresentationTestBoard();

            Debug.Log("===== INITIAL CIRCUIT EVALUATION =====");

            CircuitEvaluationResult initialResult = CircuitEvaluator.Evaluate(board);

            Check("Source is powered", initialResult.IsPowered(new Vector2Int(0, 0)), true);
            Check("Top bend is powered", initialResult.IsPowered(new Vector2Int(1, 0)), true);
            Check("Middle straight is powered", initialResult.IsPowered(new Vector2Int(1, 1)), true);
            Check("Ground is powered", initialResult.IsPowered(new Vector2Int(2, 2)), true);
            Check("Bulb is powered", initialResult.IsPowered(new Vector2Int(0, 1)), false);
            Check("Ground is reached", initialResult.GroundReached, true);
            Check("All bulbs are powered", initialResult.AllBulbsPowered, false);
            Check("All ports are connected", initialResult.AllPortsConnected, false);
            Check("Puzzle is solved", initialResult.IsSolved, false);

            Debug.Log("===== CIRCUIT EVALUATION AFTER ROTATION =====");

            board.GetTile(new Vector2Int(1, 1)).RotateClockwise();
            CircuitEvaluationResult rotatedResult = CircuitEvaluator.Evaluate(board);

            Check("Source remains powered", rotatedResult.IsPowered(new Vector2Int(0, 0)), true);
            Check("Top bend remains powered", rotatedResult.IsPowered(new Vector2Int(1, 0)), true);
            Check("Middle straight is no longer powered", rotatedResult.IsPowered(new Vector2Int(1, 1)), false);
            Check("Ground is no longer powered", rotatedResult.IsPowered(new Vector2Int(2, 2)), false);
            Check("Ground is no longer reached", rotatedResult.GroundReached, false);
            Check("Puzzle remains unsolved", rotatedResult.IsSolved, false);
        }

        private static void Check(string description, bool actual, bool expected)
        {
            string result = actual == expected ? "PASS" : "FAIL";
            Debug.Log($"{result} | {description} | Actual: {actual} | Expected: {expected}");
        }
    }
}