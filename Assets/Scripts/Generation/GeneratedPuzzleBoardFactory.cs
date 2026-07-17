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

            GeneratedPuzzleLayout layout = SinglePathPuzzleLayoutGenerator.Generate(settings.Width, settings.Height);
            BoardData board = GeneratedBoardFactory.CreateSolvedBoard(layout);

            BoardScrambler.Scramble(board, settings.ScrambleSeed);

            return board;
        }
    }
}