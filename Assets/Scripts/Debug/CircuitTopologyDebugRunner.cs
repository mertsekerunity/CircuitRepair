using CircuitRepair.Core;
using CircuitRepair.Generation;
using UnityEngine;

namespace CircuitRepair.Debugging
{
    public sealed class CircuitTopologyDebugRunner : MonoBehaviour
    {
        private void Start()
        {
            CircuitTopology topology = new CircuitTopology(3, 3);

            topology.Connect(new Vector2Int(0, 0), Direction.East);
            topology.Connect(new Vector2Int(1, 0), Direction.South);
            topology.Connect(new Vector2Int(1, 1), Direction.West);

            Debug.Log("===== CIRCUIT TOPOLOGY TEST =====");

            Check("First cell has East", topology.HasConnection(new Vector2Int(0, 0), Direction.East), true);
            Check("Second cell has reciprocal West", topology.HasConnection(new Vector2Int(1, 0), Direction.West), true);
            Check("Second cell has South", topology.HasConnection(new Vector2Int(1, 0), Direction.South), true);
            Check("Center cell has reciprocal North", topology.HasConnection(new Vector2Int(1, 1), Direction.North), true);
            Check("Center cell has West", topology.HasConnection(new Vector2Int(1, 1), Direction.West), true);
            Check("Second cell degree is two", topology.GetDegree(new Vector2Int(1, 0)) == 2, true);
            Check("Center cell degree is two", topology.GetDegree(new Vector2Int(1, 1)) == 2, true);
            Check("Unused cell degree is zero", topology.GetDegree(new Vector2Int(2, 2)) == 0, true);
        }

        private static void Check(string description, bool actual, bool expected)
        {
            string result = actual == expected ? "PASS" : "FAIL";
            Debug.Log($"{result} | {description} | Actual: {actual} | Expected: {expected}");
        }
    }
}