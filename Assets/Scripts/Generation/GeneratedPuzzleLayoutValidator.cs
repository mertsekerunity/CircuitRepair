using System;
using System.Collections.Generic;
using CircuitRepair.Core;
using UnityEngine;

namespace CircuitRepair.Generation
{
    public static class GeneratedPuzzleLayoutValidator
    {
        private static readonly Direction[] CardinalDirections =
        {
            Direction.North,
            Direction.East,
            Direction.South,
            Direction.West
        };

        public static IReadOnlyList<string> GetErrors(GeneratedPuzzleLayout layout)
        {
            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            List<string> errors = new();
            ValidateTiles(layout, errors);
            ValidateConnectivity(layout, errors);

            return errors;
        }

        public static void ThrowIfInvalid(GeneratedPuzzleLayout layout)
        {
            IReadOnlyList<string> errors = GetErrors(layout);

            if (errors.Count == 0)
            {
                return;
            }

            string message = $"Generated puzzle layout is invalid:\n- {string.Join("\n- ", errors)}";
            throw new InvalidOperationException(message);
        }

        private static void ValidateTiles(GeneratedPuzzleLayout layout, List<string> errors)
        {
            CircuitTopology topology = layout.Topology;

            for (int y = 0; y < topology.Height; y++)
            {
                for (int x = 0; x < topology.Width; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    Direction ports = topology.GetPorts(position);
                    int degree = ports.CountDirections();

                    if (position == layout.SourcePosition)
                    {
                        ValidateSpecialPorts(position, TileType.Source, ports, errors);
                        continue;
                    }

                    if (position == layout.GroundPosition)
                    {
                        ValidateSpecialPorts(position, TileType.Ground, ports, errors);
                        continue;
                    }

                    if (layout.IsBulb(position))
                    {
                        ValidateSpecialPorts(position, TileType.Bulb, ports, errors);
                        continue;
                    }

                    if (degree == 0)
                    {
                        errors.Add($"Tile at position {position} has no connections.");
                        continue;
                    }

                    if (degree == 1)
                    {
                        errors.Add($"Non-special tile at position {position} cannot have degree one.");
                        continue;
                    }

                    if (degree == 4)
                    {
                        errors.Add($"Tile at position {position} requires an unsupported cross tile.");
                        continue;
                    }

                    if (!WireTileResolver.TryResolve(ports, out _, out _))
                    {
                        errors.Add($"Ports '{ports}' at position {position} cannot be represented by a supported wire tile.");
                    }
                }
            }
        }

        private static void ValidateSpecialPorts(Vector2Int position, TileType type, Direction actualPorts, List<string> errors)
        {
            Direction expectedPorts = TileRules.GetBasePorts(type);

            if (actualPorts != expectedPorts)
            {
                errors.Add($"{type} at position {position} requires ports '{expectedPorts}', but topology contains '{actualPorts}'.");
            }
        }

        private static void ValidateConnectivity(GeneratedPuzzleLayout layout, List<string> errors)
        {
            CircuitTopology topology = layout.Topology;
            Queue<Vector2Int> positionsToVisit = new();
            HashSet<Vector2Int> visitedPositions = new();

            positionsToVisit.Enqueue(layout.SourcePosition);
            visitedPositions.Add(layout.SourcePosition);

            while (positionsToVisit.Count > 0)
            {
                Vector2Int currentPosition = positionsToVisit.Dequeue();

                foreach (Direction direction in CardinalDirections)
                {
                    if (!topology.HasConnection(currentPosition, direction))
                    {
                        continue;
                    }

                    Vector2Int neighbourPosition = currentPosition + direction.ToGridOffset();

                    if (visitedPositions.Add(neighbourPosition))
                    {
                        positionsToVisit.Enqueue(neighbourPosition);
                    }
                }
            }

            int expectedTileCount = topology.Width * topology.Height;

            if (visitedPositions.Count != expectedTileCount)
            {
                errors.Add($"Topology is not fully connected. Source reaches {visitedPositions.Count} of {expectedTileCount} cells.");
            }
        }
    }
}