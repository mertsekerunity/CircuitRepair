using CircuitRepair.Content;
using CircuitRepair.Core;
using CircuitRepair.Generation;
using CircuitRepair.Input;
using CircuitRepair.Presentation;
using System;
using UnityEngine;

namespace CircuitRepair.Gameplay
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] private PuzzleSourceType puzzleSourceType = PuzzleSourceType.Authored;
        [SerializeField] private PuzzleDefinition puzzleDefinition;
        [SerializeField] private GeneratedPuzzleSettings generatedPuzzleSettings = new();

        [Header("Scene References")]
        [SerializeField] private BoardPresenter boardPresenter;
        [SerializeField] private BoardConnectionPresenter boardConnectionPresenter;
        [SerializeField] private BoardCameraFitter boardCameraFitter;
        [SerializeField] private BoardInputController boardInputController;

        private BoardController boardController;
        private CircuitController circuitController;

        public BoardData Board { get; private set; }

        private void Start()
        {
            ValidateReferences();
            StartPuzzle();
        }

        public void RestartPuzzle()
        {
            StartPuzzle();
        }

        [ContextMenu("Restart Puzzle")]
        private void RestartPuzzleFromContextMenu()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Puzzle can only be restarted while the game is running.", this);
                return;
            }

            RestartPuzzle();
        }

        private void StartPuzzle()
        {
            UnbindControllers();

            Board = CreateBoard();

            boardPresenter.Present(Board);
            boardCameraFitter.Fit(boardPresenter);
            boardConnectionPresenter.Present(Board);

            boardController = new BoardController(Board);
            circuitController = new CircuitController(Board);

            BindControllers();

            boardInputController.Initialize(boardController);
            circuitController.Reevaluate();
        }

        private BoardData CreateBoard()
        {
            return puzzleSourceType switch
            {
                PuzzleSourceType.Authored => PuzzleBoardFactory.Create(puzzleDefinition),
                PuzzleSourceType.Generated => GeneratedPuzzleBoardFactory.Create(generatedPuzzleSettings),
                _ => throw new ArgumentOutOfRangeException(nameof(puzzleSourceType), puzzleSourceType, "Unsupported puzzle source type.")
            };
        }

        private void BindControllers()
        {
            boardController.TileRotated += boardPresenter.RefreshTile;
            boardController.TileRotated += boardConnectionPresenter.RefreshAround;
            boardController.TileRotated += circuitController.HandleTileRotated;

            circuitController.EvaluationChanged += boardPresenter.RefreshPowerState;
            circuitController.PuzzleSolved += HandlePuzzleSolved;
        }

        private void UnbindControllers()
        {
            if (boardController != null)
            {
                boardController.TileRotated -= boardPresenter.RefreshTile;
                boardController.TileRotated -= boardConnectionPresenter.RefreshAround;

                if (circuitController != null)
                {
                    boardController.TileRotated -= circuitController.HandleTileRotated;
                }
            }

            if (circuitController != null)
            {
                circuitController.EvaluationChanged -= boardPresenter.RefreshPowerState;
                circuitController.PuzzleSolved -= HandlePuzzleSolved;
            }

            boardController = null;
            circuitController = null;
        }

        private void HandlePuzzleSolved()
        {
            boardController.SetInteractionEnabled(false);
            Debug.Log("Puzzle solved.");
        }

        private void OnDestroy()
        {
            UnbindControllers();
        }

        private void ValidateReferences()
        {
            switch (puzzleSourceType)
            {
                case PuzzleSourceType.Authored:
                    if (puzzleDefinition == null)
                    {
                        throw new InvalidOperationException($"{nameof(puzzleDefinition)} is not assigned for the authored puzzle source.");
                    }

                    break;

                case PuzzleSourceType.Generated:
                    if (generatedPuzzleSettings == null)
                    {
                        throw new InvalidOperationException($"{nameof(generatedPuzzleSettings)} is not assigned for the generated puzzle source.");
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(puzzleSourceType), puzzleSourceType, "Unsupported puzzle source type.");
            }

            if (boardPresenter == null)
            {
                throw new InvalidOperationException($"{nameof(boardPresenter)} is not assigned in the Inspector.");
            }

            if (boardConnectionPresenter == null)
            {
                throw new InvalidOperationException($"{nameof(boardConnectionPresenter)} is not assigned in the Inspector.");
            }

            if (boardCameraFitter == null)
            {
                throw new InvalidOperationException($"{nameof(boardCameraFitter)} is not assigned in the Inspector.");
            }

            if (boardInputController == null)
            {
                throw new InvalidOperationException($"{nameof(boardInputController)} is not assigned in the Inspector.");
            }
        }
    }
}