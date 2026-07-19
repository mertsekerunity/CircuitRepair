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

            Dictionary<int, int> sourceRatingCounts = new();
            Dictionary<int, int> bulbRatingCounts = new();

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
                        ValidateRatedTile(definition, tileDefinition, sourceRatingCounts, errors);
                        break;

                    case TileType.Bulb:
                        bulbCount++;
                        ValidateRatedTile(definition, tileDefinition, bulbRatingCounts, errors);
                        break;

                    case TileType.Ground:
                        groundCount++;
                        ValidateUnratedTile(tileDefinition, errors);
                        break;

                    default:
                        ValidateUnratedTile(tileDefinition, errors);
                        break;
                }
            }

            AddMissingTileErrors(definition, occupiedPositions, errors);
            AddRuleSetErrors(definition, sourceCount, bulbCount, groundCount, sourceRatingCounts, bulbRatingCounts, errors);

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

        private static void AddRuleSetErrors(PuzzleDefinition definition, int sourceCount, int bulbCount, int groundCount, IReadOnlyDictionary<int, int> sourceRatingCounts, IReadOnlyDictionary<int, int> bulbRatingCounts, List<string> errors)
        {
            switch (definition.RuleSet)
            {
                case PuzzleRuleSet.SingleCircuit:
                    AddSingleCircuitErrors(sourceCount, bulbCount, groundCount, errors);
                    break;

                case PuzzleRuleSet.MultiCircuit:
                    AddMultiCircuitErrors(sourceCount, bulbCount, groundCount, sourceRatingCounts, bulbRatingCounts, errors);
                    break;

                default:
                    errors.Add($"Puzzle uses unsupported rule set {definition.RuleSet}.");
                    break;
            }
        }

        private static void AddSingleCircuitErrors(int sourceCount, int bulbCount, int groundCount, List<string> errors)
        {
            if (sourceCount != 1)
            {
                errors.Add($"Single-circuit puzzle must contain exactly one source tile, but found {sourceCount}.");
            }

            if (groundCount != 1)
            {
                errors.Add($"Single-circuit puzzle must contain exactly one ground tile, but found {groundCount}.");
            }

            if (bulbCount < 1)
            {
                errors.Add("Single-circuit puzzle must contain at least one bulb tile.");
            }
        }

        private static void AddMultiCircuitErrors(int sourceCount, int bulbCount, int groundCount, IReadOnlyDictionary<int, int> sourceRatingCounts, IReadOnlyDictionary<int, int> bulbRatingCounts, List<string> errors)
        {
            if (sourceCount < 2)
            {
                errors.Add($"Multi-circuit puzzle must contain at least two source tiles, but found {sourceCount}.");
            }

            if (sourceCount != bulbCount)
            {
                errors.Add(
                    $"Multi-circuit puzzle must contain the same number of sources and bulbs, " +
                    $"but found {sourceCount} sources and {bulbCount} bulbs.");
            }

            if (bulbCount != groundCount)
            {
                errors.Add(
                    $"Multi-circuit puzzle must contain one ground for each bulb, " +
                    $"but found {bulbCount} bulbs and {groundCount} grounds.");
            }

            if (!HaveMatchingRatingCounts(sourceRatingCounts, bulbRatingCounts))
            {
                errors.Add("Multi-circuit puzzle source ratings must have matching bulb ratings.");
            }
        }

        private static void ValidateRatedTile(PuzzleDefinition definition, TileDefinition tileDefinition, Dictionary<int, int> ratingCounts, List<string> errors)
        {
            if (definition.RuleSet == PuzzleRuleSet.MultiCircuit && tileDefinition.CurrentRating <= 0)
            {
                errors.Add(
                    $"{tileDefinition.Type} at position {tileDefinition.Position} " +
                    "must have a positive current rating in a multi-circuit puzzle.");

                return;
            }

            if (tileDefinition.CurrentRating > 0)
            {
                IncrementCount(ratingCounts, tileDefinition.CurrentRating);
            }
        }

        private static void ValidateUnratedTile(TileDefinition tileDefinition, List<string> errors)
        {
            if (tileDefinition.CurrentRating == 0)
            {
                return;
            }

            errors.Add(
                $"{tileDefinition.Type} at position {tileDefinition.Position} " +
                $"cannot have current rating {tileDefinition.CurrentRating}.");
        }

        private static bool HaveMatchingRatingCounts(IReadOnlyDictionary<int, int> first, IReadOnlyDictionary<int, int> second)
        {
            if (first.Count != second.Count)
            {
                return false;
            }

            foreach (KeyValuePair<int, int> pair in first)
            {
                if (!second.TryGetValue(pair.Key, out int secondCount) || secondCount != pair.Value)
                {
                    return false;
                }
            }

            return true;
        }

        private static void IncrementCount(Dictionary<int, int> counts, int value)
        {
            counts.TryGetValue(value, out int currentCount);
            counts[value] = currentCount + 1;
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