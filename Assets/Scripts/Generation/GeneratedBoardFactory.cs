using System;
using CircuitRepair.Core;
using UnityEngine;

namespace CircuitRepair.Generation
{
    public static class GeneratedBoardFactory
    {
        public static BoardData CreateSolvedBoard(GeneratedPuzzleLayout layout)
        {
            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            CircuitTopology topology = layout.Topology;
            BoardData board = new BoardData(topology.Width, topology.Height);

            for (int y = 0; y < topology.Height; y++)
            {
                for (int x = 0; x < topology.Width; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    TileData tile = CreateTile(layout, position);
                    board.SetTile(tile);
                }
            }

            return board;
        }

        private static TileData CreateTile(GeneratedPuzzleLayout layout, Vector2Int position)
        {
            Direction ports = layout.Topology.GetPorts(position);

            if (position == layout.SourcePosition)
            {
                return CreateSpecialTile(position, TileType.Source, ports);
            }

            if (position == layout.GroundPosition)
            {
                return CreateSpecialTile(position, TileType.Ground, ports);
            }

            if (layout.IsBulb(position))
            {
                return CreateSpecialTile(position, TileType.Bulb, ports);
            }

            if (!WireTileResolver.TryResolve(ports, out TileType type, out TileRotation rotation))
            {
                throw new InvalidOperationException($"Ports '{ports}' at position {position} cannot be represented by a supported wire tile.");
            }

            return new TileData(position, type, rotation);
        }

        private static TileData CreateSpecialTile(Vector2Int position, TileType type, Direction actualPorts)
        {
            Direction expectedPorts = TileRules.GetBasePorts(type);

            if (actualPorts != expectedPorts)
            {
                throw new InvalidOperationException($"{type} at position {position} requires ports '{expectedPorts}', but topology contains '{actualPorts}'.");
            }

            return new TileData(position, type);
        }
    }
}