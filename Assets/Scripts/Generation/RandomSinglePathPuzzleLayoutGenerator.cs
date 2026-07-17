using CircuitRepair.Core;
using System;
using UnityEngine;

namespace CircuitRepair.Generation
{
    public static class RandomSinglePathPuzzleLayoutGenerator
    {
        private const int MaxGenerationAttempts = 64;

        public static GeneratedPuzzleLayout Generate(int width, int height, int layoutSeed)
        {
            return Generate(width, height, layoutSeed, 0, int.MaxValue);
        }

        public static GeneratedPuzzleLayout Generate(int width, int height, int layoutSeed, int minimumBendCount, int maximumBendCount)
        {
            ValidateBendRange(minimumBendCount, maximumBendCount);

            System.Random seedSequence = new System.Random(layoutSeed);

            for (int attempt = 0; attempt < MaxGenerationAttempts; attempt++)
            {
                int pathSeed = seedSequence.Next();

                FullGridPath path;

                try
                {
                    path = RandomFullGridPathGenerator.Generate(width, height, pathSeed);
                }
                catch (InvalidOperationException)
                {
                    continue;
                }

                if (!SinglePathPuzzleLayoutGenerator.TryGenerate(path, out GeneratedPuzzleLayout layout))
                {
                    continue;
                }

                int bendCount = CountBends(layout);

                if (bendCount < minimumBendCount || bendCount > maximumBendCount)
                {
                    continue;
                }

                return layout;
            }

            throw new InvalidOperationException(
                $"Failed to generate a valid random single-path {width}x{height} layout " +
                $"with {minimumBendCount}-{maximumBendCount} bends for seed {layoutSeed} " +
                $"after {MaxGenerationAttempts} attempts.");
        }

        private static void ValidateBendRange(int minimumBendCount, int maximumBendCount)
        {
            if (minimumBendCount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumBendCount),
                    minimumBendCount,
                    "Minimum bend count cannot be negative.");
            }

            if (maximumBendCount < minimumBendCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumBendCount),
                    maximumBendCount,
                    "Maximum bend count cannot be lower than the minimum.");
            }
        }

        private static int CountBends(GeneratedPuzzleLayout layout)
        {
            int bendCount = 0;
            CircuitTopology topology = layout.Topology;

            for (int y = 0; y < topology.Height; y++)
            {
                for (int x = 0; x < topology.Width; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);

                    if (position == layout.SourcePosition ||
                        position == layout.GroundPosition ||
                        layout.IsBulb(position))
                    {
                        continue;
                    }

                    Direction ports = topology.GetPorts(position);

                    if (WireTileResolver.TryResolve(ports, out TileType type, out _) &&
                        type == TileType.Bend)
                    {
                        bendCount++;
                    }
                }
            }

            return bendCount;
        }
    }
}