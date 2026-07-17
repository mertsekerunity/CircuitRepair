using System;
using CircuitRepair.Core;
using UnityEngine;

namespace CircuitRepair.Generation
{
    public static class GeneratedBoardFactory
    {
        public static BoardData CreateSolvedBoard(GeneratedPuzzleLayout layout)
        {
            GeneratedPuzzleLayoutValidator.ThrowIfInvalid(layout);

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
                return CreateSpecialTile(position, TileType.Source);
            }

            if (position == layout.GroundPosition)
            {
                return CreateSpecialTile(position, TileType.Ground);
            }

            if (layout.IsBulb(position))
            {
                return CreateSpecialTile(position, TileType.Bulb);
            }

            WireTileResolver.Resolve(ports, out TileType type, out TileRotation rotation);

            return new TileData(position, type, rotation);
        }

        private static TileData CreateSpecialTile(Vector2Int position, TileType type)
        {
            return new TileData(position, type);
        }
    }
}