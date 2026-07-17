using CircuitRepair.Core;
using CircuitRepair.Generation;
using UnityEngine;

namespace CircuitRepair.Debugging
{
    public sealed class WireTileResolverDebugRunner : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("===== WIRE TILE RESOLVER TEST =====");

            CheckResolution(Direction.West | Direction.East, TileType.Straight, TileRotation.Degrees0);
            CheckResolution(Direction.North | Direction.South, TileType.Straight, TileRotation.Degrees90);

            CheckResolution(Direction.West | Direction.North, TileType.Bend, TileRotation.Degrees0);
            CheckResolution(Direction.North | Direction.East, TileType.Bend, TileRotation.Degrees90);
            CheckResolution(Direction.East | Direction.South, TileType.Bend, TileRotation.Degrees180);
            CheckResolution(Direction.South | Direction.West, TileType.Bend, TileRotation.Degrees270);

            CheckResolution(Direction.West | Direction.East | Direction.South, TileType.TJunction, TileRotation.Degrees0);
            CheckResolution(Direction.North | Direction.South | Direction.West, TileType.TJunction, TileRotation.Degrees90);
            CheckResolution(Direction.North | Direction.East | Direction.West, TileType.TJunction, TileRotation.Degrees180);
            CheckResolution(Direction.North | Direction.East | Direction.South, TileType.TJunction, TileRotation.Degrees270);

            CheckUnsupported(Direction.East);
            CheckUnsupported(Direction.All);
        }

        private static void CheckResolution(Direction ports, TileType expectedType, TileRotation expectedRotation)
        {
            bool resolved = WireTileResolver.TryResolve(ports, out TileType actualType, out TileRotation actualRotation);
            bool passed = resolved && actualType == expectedType && actualRotation == expectedRotation;

            Debug.Log($"{(passed ? "PASS" : "FAIL")} | Ports: {ports} | Actual: {actualType}, {actualRotation} | Expected: {expectedType}, {expectedRotation}");
        }

        private static void CheckUnsupported(Direction ports)
        {
            bool resolved = WireTileResolver.TryResolve(ports, out _, out _);
            Debug.Log($"{(!resolved ? "PASS" : "FAIL")} | Unsupported ports: {ports} | Resolved: {resolved} | Expected: False");
        }
    }
}