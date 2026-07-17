using CircuitRepair.Core;
using CircuitRepair.Generation;
using UnityEngine;

namespace CircuitRepair.Debugging
{
    public sealed class GeneratedBoardFactoryDebugRunner : MonoBehaviour
    {
        private void Start()
        {
            CircuitTopology topology = CreateSnakeTopology();

            GeneratedPuzzleLayout layout = new GeneratedPuzzleLayout(
                topology,
                new Vector2Int(0, 0),
                new Vector2Int(2, 2),
                new[] { new Vector2Int(1, 2) });

            BoardData board = GeneratedBoardFactory.CreateSolvedBoard(layout);
            CircuitEvaluationResult result = CircuitEvaluator.Evaluate(board);

            Debug.Log("===== GENERATED BOARD FACTORY TEST =====");

            CheckTile(board, new Vector2Int(0, 0), TileType.Source, TileRotation.Degrees0);
            CheckTile(board, new Vector2Int(1, 0), TileType.Straight, TileRotation.Degrees0);
            CheckTile(board, new Vector2Int(2, 0), TileType.Bend, TileRotation.Degrees270);
            CheckTile(board, new Vector2Int(0, 1), TileType.Bend, TileRotation.Degrees180);
            CheckTile(board, new Vector2Int(1, 2), TileType.Bulb, TileRotation.Degrees0);
            CheckTile(board, new Vector2Int(2, 2), TileType.Ground, TileRotation.Degrees0);

            Check("Ground is reached", result.GroundReached, true);
            Check("All bulbs are powered", result.AllBulbsPowered, true);
            Check("All ports are connected", result.AllPortsConnected, true);
            Check("All tiles are powered", result.AllTilesPowered, true);
            Check("Generated board is solved", result.IsSolved, true);
            Check("Powered tile count is nine", result.PoweredTileCount == 9, true);
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

        private static void CheckTile(BoardData board, Vector2Int position, TileType expectedType, TileRotation expectedRotation)
        {
            TileData tile = board.GetTile(position);
            bool passed = tile.Type == expectedType && tile.Rotation == expectedRotation;

            Debug.Log($"{(passed ? "PASS" : "FAIL")} | Position: {position} | Actual: {tile.Type}, {tile.Rotation} | Expected: {expectedType}, {expectedRotation}");
        }

        private static void Check(string description, bool actual, bool expected)
        {
            string result = actual == expected ? "PASS" : "FAIL";
            Debug.Log($"{result} | {description} | Actual: {actual} | Expected: {expected}");
        }
    }
}