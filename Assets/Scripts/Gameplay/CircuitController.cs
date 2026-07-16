using System;
using CircuitRepair.Core;
using UnityEngine;

namespace CircuitRepair.Gameplay
{
    public sealed class CircuitController
    {
        public BoardData Board { get; }
        public CircuitEvaluationResult CurrentResult { get; private set; }

        public event Action<CircuitEvaluationResult> EvaluationChanged;
        public event Action PuzzleSolved;

        public CircuitController(BoardData board)
        {
            Board = board ?? throw new ArgumentNullException(nameof(board));
        }

        public void Reevaluate()
        {
            bool wasSolved = CurrentResult != null && CurrentResult.IsSolved;

            CurrentResult = CircuitEvaluator.Evaluate(Board);
            EvaluationChanged?.Invoke(CurrentResult);

            if (!wasSolved && CurrentResult.IsSolved)
            {
                PuzzleSolved?.Invoke();
            }
        }

        public void HandleTileRotated(Vector2Int position)
        {
            Reevaluate();
        }
    }
}