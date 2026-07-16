using System;
using System.Collections.Generic;
using CircuitRepair.Core;
using UnityEngine;

namespace CircuitRepair.Content
{
    public static class PuzzleDefinitionValidator
    {
        public static IReadOnlyList<string> GetErrors(PuzzleDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            List<string> errors = new();
            HashSet<Vector2Int> occupiedPositions = new();

            int sourceCount = 0;
            int groundCount = 0;
            int bulbCount = 0;

            foreach (TileDefinition tileDefinition in definition.Tiles)
            {
                if (tileDefinition == null)
                {
                    errors.Add("Puzzle contains a null tile entry.");
                    continue;
                }

                Vector2Int position = tileDefinition.Position;

                if (!IsInside(definition, position))
                {
                    errors.Add($"Tile at position {position} is outside the board boundaries.");
                    continue;
                }

                if (!occupiedPositions.Add(position))
                {
                    errors.Add($"Puzzle contains more than one tile at position {position}.");
                    continue;
                }

                switch (tileDefinition.Type)
                {
                    case TileType.Source:
                        sourceCount++;
                        break;

                    case TileType.Ground:
                        groundCount++;
                        break;

                    case TileType.Bulb:
                        bulbCount++;
                        break;
                }

                if (!TileRules.IsRotatable(tileDefinition.Type) && tileDefinition.InitialRotation != TileRotation.Degrees0)
                {
                    errors.Add($"{tileDefinition.Type} at position {position} is not rotatable and must use Degrees0.");
                }
            }

            AddMissingTileErrors(definition, occupiedPositions, errors);

            if (sourceCount != 1)
            {
                errors.Add($"Puzzle must contain exactly one source tile, but found {sourceCount}.");
            }

            if (groundCount != 1)
            {
                errors.Add($"Puzzle must contain exactly one ground tile, but found {groundCount}.");
            }

            if (bulbCount < 1)
            {
                errors.Add("Puzzle must contain at least one bulb tile.");
            }

            return errors;
        }

        public static void ThrowIfInvalid(PuzzleDefinition definition)
        {
            IReadOnlyList<string> errors = GetErrors(definition);

            if (errors.Count == 0)
            {
                return;
            }

            string message = $"Puzzle definition '{definition.name}' is invalid:\n- {string.Join("\n- ", errors)}";
            throw new InvalidOperationException(message);
        }

        private static bool IsInside(PuzzleDefinition definition, Vector2Int position)
        {
            return position.x >= 0 &&
                   position.x < definition.Width &&
                   position.y >= 0 &&
                   position.y < definition.Height;
        }

        private static void AddMissingTileErrors(PuzzleDefinition definition, HashSet<Vector2Int> occupiedPositions, List<string> errors)
        {
            for (int y = 0; y < definition.Height; y++)
            {
                for (int x = 0; x < definition.Width; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);

                    if (!occupiedPositions.Contains(position))
                    {
                        errors.Add($"Puzzle does not contain a tile at position {position}.");
                    }
                }
            }
        }
    }
}