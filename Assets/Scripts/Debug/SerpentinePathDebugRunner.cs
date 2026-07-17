using System;
using CircuitRepair.Generation;
using UnityEngine;

namespace CircuitRepair.Debugging
{
    public sealed class SerpentinePathDebugRunner : MonoBehaviour
    {
        private void Start()
        {
            FullGridPath path = SerpentinePathGenerator.Generate(5, 5);

            Debug.Log("===== SERPENTINE PATH TEST =====");

            Check("Path contains all 25 positions", path.Count == 25, true);
            Check("Path starts at the top-left", path.Start == new Vector2Int(0, 0), true);
            Check("Path ends at the bottom-right", path.End == new Vector2Int(4, 4), true);

            Check("First row moves right", path[4] == new Vector2Int(4, 0), true);
            Check("Second row moves left", path[5] == new Vector2Int(4, 1), true);
            Check("Second row ends on the left", path[9] == new Vector2Int(0, 1), true);
            Check("Even-height generation is rejected", ThrowsForEvenHeight(), true);
        }

        private static bool ThrowsForEvenHeight()
        {
            try
            {
                SerpentinePathGenerator.Generate(6, 6);
                return false;
            }
            catch (ArgumentException)
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