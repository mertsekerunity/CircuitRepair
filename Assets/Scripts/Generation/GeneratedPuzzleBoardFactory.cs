using System;
using CircuitRepair.Core;

namespace CircuitRepair.Generation
{
    public static class GeneratedPuzzleBoardFactory
    {
        public static BoardData Create(GeneratedPuzzleSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            return Create(settings, settings.LayoutSeed, settings.ScrambleSeed);
        }

        public static BoardData Create(GeneratedPuzzleSettings settings, int layoutSeed, int scrambleSeed)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            GeneratedPuzzleLayout layout = RandomSinglePathPuzzleLayoutGenerator.Generate(
                settings.Width,
                settings.Height,
                layoutSeed,
                settings.MinimumBendCount,
                settings.MaximumBendCount);

            BoardData board = GeneratedBoardFactory.CreateSolvedBoard(layout);

            BoardScrambler.Scramble(board, scrambleSeed);

            return board;
        }
    }
}