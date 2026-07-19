using System;
using CircuitRepair.Core;
using UnityEngine;

namespace CircuitRepair.Gameplay
{
    public sealed class MultiCircuitController
    {
        public BoardData Board { get; }
        public MultiCircuitEvaluationResult CurrentResult { get; private set; }

        public event Action<MultiCircuitEvaluationResult> EvaluationChanged;
        public event Action PuzzleSolved;

        public MultiCircuitController(BoardData board)
        {
            Board = board ?? throw new ArgumentNullException(nameof(board));
        }

        public void Reevaluate()
        {
            bool wasSolved = CurrentResult != null && CurrentResult.IsSolved;

            CurrentResult = MultiCircuitEvaluator.Evaluate(Board);
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