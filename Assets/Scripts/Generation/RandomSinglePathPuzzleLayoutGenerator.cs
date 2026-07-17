using System;

namespace CircuitRepair.Generation
{
    public static class RandomSinglePathPuzzleLayoutGenerator
    {
        private const int MaxGenerationAttempts = 64;

        public static GeneratedPuzzleLayout Generate(int width, int height, int layoutSeed)
        {
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

                if (SinglePathPuzzleLayoutGenerator.TryGenerate(path, out GeneratedPuzzleLayout layout))
                {
                    return layout;
                }
            }

            throw new InvalidOperationException($"Failed to generate a valid random single-path {width}x{height} layout for seed {layoutSeed} after {MaxGenerationAttempts} attempts.");
        }
    }
}