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

            GeneratedPuzzleLayout layout = RandomPuzzleLayoutGenerator.Generate(settings.Width, settings.Height, settings.LayoutSeed, settings.MinimumTJunctionCount, settings.MaximumTJunctionCount);
            BoardData board = GeneratedBoardFactory.CreateSolvedBoard(layout);

            BoardScrambler.Scramble(board, settings.ScrambleSeed);

            return board;
        }
    }
}