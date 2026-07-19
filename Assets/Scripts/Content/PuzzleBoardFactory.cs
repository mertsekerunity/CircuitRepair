using CircuitRepair.Core;
using UnityEngine;

namespace CircuitRepair.Content
{
    public static class PuzzleBoardFactory
    {
        public static BoardData Create(PuzzleDefinition definition)
        {
            PuzzleDefinitionValidator.ThrowIfInvalid(definition);

            BoardData board = new BoardData(definition.Width, definition.Height);

            foreach (TileDefinition tileDefinition in definition.Tiles)
            {
                Vector2Int position = tileDefinition.Position;

                TileData tile = new TileData(position, tileDefinition.Type, tileDefinition.InitialRotation, tileDefinition.CurrentRating);

                board.SetTile(tile);
            }

            return board;
        }
    }
}