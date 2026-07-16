using System;
using CircuitRepair.Core;
using CircuitRepair.Input;
using CircuitRepair.Presentation;
using CircuitRepair.Content;
using UnityEngine;

namespace CircuitRepair.Gameplay
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] private PuzzleDefinition puzzleDefinition;

        [SerializeField] private BoardPresenter boardPresenter;
        [SerializeField] private BoardConnectionPresenter boardConnectionPresenter;
        [SerializeField] private BoardInputController boardInputController;
        [SerializeField] private BoardCameraFitter boardCameraFitter;

        private BoardController boardController;
        private CircuitController circuitController;

        public BoardData Board { get; private set; }

        private void Start()
        {
            ValidateReferences();

            Board = PuzzleBoardFactory.Create(puzzleDefinition);

            boardPresenter.Present(Board);
            boardCameraFitter.Fit(boardPresenter);
            boardConnectionPresenter.Present(Board);

            boardController = new BoardController(Board);
            circuitController = new CircuitController(Board);

            boardController.TileRotated += boardPresenter.RefreshTile;
            boardController.TileRotated += boardConnectionPresenter.RefreshAround;
            boardController.TileRotated += circuitController.HandleTileRotated;

            circuitController.EvaluationChanged += boardPresenter.RefreshPowerState;
            circuitController.PuzzleSolved += HandlePuzzleSolved;

            circuitController.Reevaluate();
            boardInputController.Initialize(boardController);
        }

        private void OnDestroy()
        {
            if (boardController != null)
            {
                boardController.TileRotated -= boardPresenter.RefreshTile;
                boardController.TileRotated -= boardConnectionPresenter.RefreshAround;
                boardController.TileRotated -= circuitController.HandleTileRotated;
            }

            if (circuitController != null)
            {
                circuitController.EvaluationChanged -= boardPresenter.RefreshPowerState;
                circuitController.PuzzleSolved -= HandlePuzzleSolved;
            }
        }

        private void ValidateReferences()
        {
            if (boardPresenter == null)
            {
                throw new InvalidOperationException($"{nameof(boardPresenter)} is not assigned in the Inspector.");
            }

            if (boardConnectionPresenter == null)
            {
                throw new InvalidOperationException($"{nameof(boardConnectionPresenter)} is not assigned in the Inspector.");
            }

            if (boardInputController == null)
            {
                throw new InvalidOperationException($"{nameof(boardInputController)} is not assigned in the Inspector.");
            }

            if (boardCameraFitter == null)
            {
                throw new InvalidOperationException($"{nameof(boardCameraFitter)} is not assigned in the Inspector.");
            }

            if (puzzleDefinition == null)
            {
                throw new InvalidOperationException($"{nameof(puzzleDefinition)} is not assigned in the Inspector.");
            }
        }

        private void HandlePuzzleSolved()
        {
            boardController.SetInteractionEnabled(false);
            Debug.Log("Puzzle solved.");
        }
    }
}