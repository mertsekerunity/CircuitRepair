using System;

namespace CircuitRepair.Core
{
    public static class TileRules
    {
        public static bool IsRotatable(TileType type)
        {
            return type is
                TileType.Straight or
                TileType.Bend or
                TileType.TJunction;
        }

        public static Direction GetBasePorts(TileType type)
        {
            return type switch
            {
                TileType.Straight =>
                    Direction.West | Direction.East,

                TileType.Bend =>
                    Direction.West | Direction.North,

                TileType.TJunction =>
                    Direction.West |
                    Direction.East |
                    Direction.South,

                TileType.Source =>
                    Direction.East,

                TileType.Ground =>
                    Direction.West,

                TileType.Bulb =>
                    Direction.West | Direction.East,

                _ => throw new ArgumentOutOfRangeException(
                    nameof(type),
                    type,
                    "Tanımsız tile tipi.")
            };
        }

        public static Direction GetPorts(TileType type, TileRotation rotation)
        {
            Direction basePorts = GetBasePorts(type);
            return basePorts.RotateDirectionsClockwise((int)rotation);
        }
    }
}