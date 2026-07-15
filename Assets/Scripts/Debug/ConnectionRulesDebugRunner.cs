using CircuitRepair.Core;
using UnityEngine;

namespace CircuitRepair.Debugging
{
    public sealed class ConnectionRulesDebugRunner : MonoBehaviour
    {
        private void Start()
        {
            BoardData board =
                CreateConnectedBoard(out TileData middleStraight);

            Debug.Log("===== CONNECTED BOARD TEST =====");

            Check(
                "Source -> East seam",
                ConnectionRules.AreConnected(
                    board,
                    new Vector2Int(0, 0),
                    Direction.East),
                expected: true);

            Check(
                "Top Bend -> South seam",
                ConnectionRules.AreConnected(
                    board,
                    new Vector2Int(1, 0),
                    Direction.South),
                expected: true);

            Check(
                "Middle Straight -> South seam",
                ConnectionRules.AreConnected(
                    board,
                    new Vector2Int(1, 1),
                    Direction.South),
                expected: true);

            Check(
                "Bottom Bend -> East seam",
                ConnectionRules.AreConnected(
                    board,
                    new Vector2Int(1, 2),
                    Direction.East),
                expected: true);

            Check(
                "Middle Straight has no dangling ports",
                ConnectionRules.AreAllPortsConnected(
                    board,
                    new Vector2Int(1, 1)),
                expected: true);

            Debug.Log("===== ROTATING MIDDLE STRAIGHT =====");

            middleStraight.RotateClockwise();

            Check(
                "Top Bend -> South seam after rotation",
                ConnectionRules.AreConnected(
                    board,
                    new Vector2Int(1, 0),
                    Direction.South),
                expected: false);

            Check(
                "Middle Straight has no dangling ports after rotation",
                ConnectionRules.AreAllPortsConnected(
                    board,
                    new Vector2Int(1, 1)),
                expected: false);

            Check(
                "Source is still locally connected",
                ConnectionRules.AreAllPortsConnected(
                    board,
                    new Vector2Int(0, 0)),
                expected: true);
        }

        private static BoardData CreateConnectedBoard(
            out TileData middleStraight)
        {
            BoardData board = new BoardData(3, 3);

            TileData source = new TileData(
                new Vector2Int(0, 0),
                TileType.Source);

            TileData topBend = new TileData(
                new Vector2Int(1, 0),
                TileType.Bend,
                TileRotation.Degrees270);

            middleStraight = new TileData(
                new Vector2Int(1, 1),
                TileType.Straight,
                TileRotation.Degrees90);

            TileData bottomBend = new TileData(
                new Vector2Int(1, 2),
                TileType.Bend,
                TileRotation.Degrees90);

            TileData ground = new TileData(
                new Vector2Int(2, 2),
                TileType.Ground);

            board.SetTile(source);
            board.SetTile(topBend);
            board.SetTile(middleStraight);
            board.SetTile(bottomBend);
            board.SetTile(ground);

            return board;
        }

        private static void Check(
            string description,
            bool actual,
            bool expected)
        {
            string result =
                actual == expected
                    ? "PASS"
                    : "FAIL";

            Debug.Log(
                $"{result} | {description} | " +
                $"Actual: {actual} | Expected: {expected}");
        }
    }
}