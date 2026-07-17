using CircuitRepair.Core;
using CircuitRepair.Generation;
using UnityEngine;

namespace CircuitRepair.Debugging
{
    public sealed class BoardScramblerDebugRunner : MonoBehaviour
    {
        private const int TestSeed = 12345;

        private void Start()
        {
            GeneratedPuzzleLayout layout = CreateSnakeLayout();

            BoardData originalBoard = GeneratedBoardFactory.CreateSolvedBoard(layout);
            BoardData firstScrambledBoard = GeneratedBoardFactory.CreateSolvedBoard(layout);
            BoardData secondScrambledBoard = GeneratedBoardFactory.CreateSolvedBoard(layout);

            CircuitEvaluationResult solvedResult = CircuitEvaluator.Evaluate(originalBoard);

            BoardScrambler.Scramble(firstScrambledBoard, TestSeed, 4, 5);
            BoardScrambler.Scramble(secondScrambledBoard, TestSeed, 4, 5);

            CircuitEvaluationResult scrambledResult = CircuitEvaluator.Evaluate(firstScrambledBoard);
            int changedRotatableTiles = CountChangedRotatableTiles(originalBoard, firstScrambledBoard);

            Debug.Log("===== BOARD SCRAMBLER TEST =====");

            Check("Board starts solved", solvedResult.IsSolved, true);
            Check("Scrambled board is not solved", scrambledResult.IsSolved, false);
            Check("Same seed creates the same rotations", HaveMatchingRotations(firstScrambledBoard, secondScrambledBoard), true);
            Check("Source remains at Degrees0", firstScrambledBoard.GetTile(new Vector2Int(0, 0)).Rotation == TileRotation.Degrees0, true);
            Check("Bulb remains at Degrees0", firstScrambledBoard.GetTile(new Vector2Int(1, 2)).Rotation == TileRotation.Degrees0, true);
            Check("Ground remains at Degrees0", firstScrambledBoard.GetTile(new Vector2Int(2, 2)).Rotation == TileRotation.Degrees0, true);
            Check("Four or five rotatable tiles changed", changedRotatableTiles is >= 4 and <= 5, true);
        }

        private static int CountChangedRotatableTiles(BoardData originalBoard, BoardData scrambledBoard)
        {
            int changedCount = 0;

            for (int y = 0; y < originalBoard.Height; y++)
            {
                for (int x = 0; x < originalBoard.Width; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    TileData originalTile = originalBoard.GetTile(position);
                    TileData scrambledTile = scrambledBoard.GetTile(position);

                    if (originalTile.IsRotatable && originalTile.Rotation != scrambledTile.Rotation)
                    {
                        changedCount++;
                    }
                }
            }

            return changedCount;
        }

        private static bool HaveMatchingRotations(BoardData firstBoard, BoardData secondBoard)
        {
            for (int y = 0; y < firstBoard.Height; y++)
            {
                for (int x = 0; x < firstBoard.Width; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);

                    if (firstBoard.GetTile(position).Rotation != secondBoard.GetTile(position).Rotation)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static GeneratedPuzzleLayout CreateSnakeLayout()
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

            return new GeneratedPuzzleLayout(topology, new Vector2Int(0, 0), new Vector2Int(2, 2), new[] { new Vector2Int(1, 2) });
        }

        private static void Check(string description, bool actual, bool expected)
        {
            string result = actual == expected ? "PASS" : "FAIL";
            Debug.Log($"{result} | {description} | Actual: {actual} | Expected: {expected}");
        }
    }
}