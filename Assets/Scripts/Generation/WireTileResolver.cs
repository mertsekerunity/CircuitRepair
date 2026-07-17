using System;
using CircuitRepair.Core;

namespace CircuitRepair.Generation
{
    public static class WireTileResolver
    {
        private static readonly TileType[] SupportedTypes =
        {
            TileType.Straight,
            TileType.Bend,
            TileType.TJunction
        };

        private static readonly TileRotation[] Rotations =
        {
            TileRotation.Degrees0,
            TileRotation.Degrees90,
            TileRotation.Degrees180,
            TileRotation.Degrees270
        };

        public static bool TryResolve(Direction ports, out TileType type, out TileRotation rotation)
        {
            int degree = ports.CountDirections();

            if (degree is not 2 and not 3)
            {
                type = default;
                rotation = default;
                return false;
            }

            foreach (TileType candidateType in SupportedTypes)
            {
                foreach (TileRotation candidateRotation in Rotations)
                {
                    Direction candidatePorts = TileRules.GetPorts(candidateType, candidateRotation);

                    if (candidatePorts != ports)
                    {
                        continue;
                    }

                    type = candidateType;
                    rotation = candidateRotation;
                    return true;
                }
            }

            type = default;
            rotation = default;
            return false;
        }

        public static void Resolve(Direction ports, out TileType type, out TileRotation rotation)
        {
            if (TryResolve(ports, out type, out rotation))
            {
                return;
            }

            throw new InvalidOperationException($"Port mask '{ports}' cannot be represented by a supported wire tile.");
        }
    }
}