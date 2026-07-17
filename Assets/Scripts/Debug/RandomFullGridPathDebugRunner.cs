using System;
using CircuitRepair.Generation;
using UnityEngine;

namespace CircuitRepair.Debugging
{
    public sealed class RandomFullGridPathDebugRunner : MonoBehaviour
    {
        private const int TestSeed = 12345;

        private void Start()
        {
            FullGridPath firstPath = RandomFullGridPathGenerator.Generate(5, 5, TestSeed);
            FullGridPath secondPath = RandomFullGridPathGenerator.Generate(5, 5, TestSeed);

            Debug.Log("===== RANDOM FULL-GRID PATH TEST =====");

            Check("Path contains all 25 positions", firstPath.Count == 25, true);
            Check("Path starts at the top-left", firstPath.Start == new Vector2Int(0, 0), true);
            Check("Path ends at the bottom-right", firstPath.End == new Vector2Int(4, 4), true);
            Check("Same seed creates the same path", HaveMatchingPaths(firstPath, secondPath), true);
            Check("Even 6x6 bottom-right endpoint is rejected", ThrowsForSixBySix(), true);
            Check("Path leaves source toward East", firstPath[1] == new Vector2Int(1, 0), true);
            Check("Path enters ground from West", firstPath[firstPath.Count - 2] == new Vector2Int(3, 4), true);
        }

        private static bool HaveMatchingPaths(FullGridPath first, FullGridPath second)
        {
            if (first.Width != second.Width || first.Height != second.Height || first.Count != second.Count)
            {
                return false;
            }

            for (int i = 0; i < first.Count; i++)
            {
                if (first[i] != second[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ThrowsForSixBySix()
        {
            try
            {
                RandomFullGridPathGenerator.Generate(6, 6, TestSeed);
                return false;
            }
            catch (InvalidOperationException)
            {
                return true;
            }
        }

        private static void Check(string description, bool actual, bool expected)
        {
            string result = actual == expected ? "PASS" : "FAIL";
            Debug.Log($"{result} | {description} | Actual: {actual} | Expected: {expected}");
        }
    }
}