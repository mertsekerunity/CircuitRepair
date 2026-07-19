using System;
using CircuitRepair.Content;
using CircuitRepair.Core;
using CircuitRepair.Generation;
using CircuitRepair.Input;
using CircuitRepair.Presentation;
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
        private MultiCircuitController multiCircuitController;

        private PuzzleRuleSet currentRuleSet;

        private int currentPuzzleIndex;
        private int currentLayoutSeed;
        private int currentScrambleSeed;

        public BoardData Board { get; private set; }
        public int CurrentPuzzleIndex => currentPuzzleIndex;
        public int CurrentLayoutSeed => currentLayoutSeed;
        public int CurrentScrambleSeed => currentScrambleSeed;
        public PuzzleRuleSet CurrentRuleSet => currentRuleSet;

        private void Awake()
        {
        #if UNITY_ANDROID || UNITY_IOS
            Application.targetFrameRate = 60;
        #endif
        }

        private void Start()
        {
            ValidateReferences();
            InitializePuzzleSequence();
            StartPuzzle();
        }

        public void RestartPuzzle()
        {
            StartPuzzle();
        }

        public void NextPuzzle()
        {
            if (puzzleSourceType != PuzzleSourceType.Generated)
            {
                Debug.LogWarning("Next Puzzle is only supported for generated puzzles.", this);
                return;
            }

            currentPuzzleIndex++;

            UpdateCurrentSeeds();
            StartPuzzle();
        }

        public void PreviousPuzzle()
        {
            if (puzzleSourceType != PuzzleSourceType.Generated)
            {
                Debug.LogWarning("Previous Puzzle is only supported for generated puzzles.", this);
                return;
            }

            if (currentPuzzleIndex <= 0)
            {
                Debug.LogWarning("The first generated puzzle is already active.", this);
                return;
            }

            currentPuzzleIndex--;

            UpdateCurrentSeeds();
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

        [ContextMenu("Next Puzzle")]
        private void NextPuzzleFromContextMenu()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("The next puzzle can only be started while the game is running.", this);
                return;
            }

            NextPuzzle();
        }

        [ContextMenu("Previous Puzzle")]
        private void PreviousPuzzleFromContextMenu()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("The previous puzzle can only be started while the game is running.", this);
                return;
            }

            PreviousPuzzle();
        }

        private void InitializePuzzleSequence()
        {
            currentPuzzleIndex = 0;

            if (puzzleSourceType == PuzzleSourceType.Generated)
            {
                UpdateCurrentSeeds();
            }
        }

        private void UpdateCurrentSeeds()
        {
            currentLayoutSeed = unchecked(generatedPuzzleSettings.LayoutSeed + currentPuzzleIndex);
            currentScrambleSeed = unchecked(generatedPuzzleSettings.ScrambleSeed + currentPuzzleIndex);
        }

        private void StartPuzzle()
        {
            UnbindControllers();

            currentRuleSet = ResolveCurrentRuleSet();
            Board = CreateBoard();

            boardPresenter.Present(Board);
            boardCameraFitter.Fit(boardPresenter);
            boardConnectionPresenter.Present(Board);

            boardController = new BoardController(Board);

            CreateCircuitController();
            BindControllers();

            boardInputController.Initialize(boardController);

            ReevaluateCurrentCircuit();
            LogPuzzleStarted();
        }

        private BoardData CreateBoard()
        {
            return puzzleSourceType switch
            {
                PuzzleSourceType.Authored => PuzzleBoardFactory.Create(puzzleDefinition),

                PuzzleSourceType.Generated => GeneratedPuzzleBoardFactory.Create(
                    generatedPuzzleSettings,
                    currentLayoutSeed,
                    currentScrambleSeed),

                _ => throw new ArgumentOutOfRangeException(nameof(puzzleSourceType), puzzleSourceType, "Unsupported puzzle source type.")
            };
        }

        private PuzzleRuleSet ResolveCurrentRuleSet()
        {
            return puzzleSourceType switch
            {
                PuzzleSourceType.Authored => puzzleDefinition.RuleSet,
                PuzzleSourceType.Generated => PuzzleRuleSet.SingleCircuit,
                _ => throw new ArgumentOutOfRangeException(nameof(puzzleSourceType), puzzleSourceType, "Unsupported puzzle source type.")
            };
        }

        private void CreateCircuitController()
        {
            switch (currentRuleSet)
            {
                case PuzzleRuleSet.SingleCircuit:
                    circuitController = new CircuitController(Board);
                    break;

                case PuzzleRuleSet.MultiCircuit:
                    multiCircuitController = new MultiCircuitController(Board);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(currentRuleSet), currentRuleSet, "Unsupported puzzle rule set.");
            }
        }

        private void BindControllers()
        {
            boardController.TileRotated += boardPresenter.RefreshTile;
            boardController.TileRotated += boardConnectionPresenter.RefreshAround;

            if (circuitController != null)
            {
                boardController.TileRotated += circuitController.HandleTileRotated;

                circuitController.EvaluationChanged += boardPresenter.RefreshPowerState;
                circuitController.PuzzleSolved += HandlePuzzleSolved;
            }

            if (multiCircuitController != null)
            {
                boardController.TileRotated += multiCircuitController.HandleTileRotated;

                multiCircuitController.EvaluationChanged += boardPresenter.RefreshMultiCircuitPowerState;
                multiCircuitController.PuzzleSolved += HandlePuzzleSolved;
            }
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

                if (multiCircuitController != null)
                {
                    boardController.TileRotated -= multiCircuitController.HandleTileRotated;
                }
            }

            if (circuitController != null)
            {
                circuitController.EvaluationChanged -= boardPresenter.RefreshPowerState;
                circuitController.PuzzleSolved -= HandlePuzzleSolved;
            }

            if (multiCircuitController != null)
            {
                multiCircuitController.EvaluationChanged -= boardPresenter.RefreshMultiCircuitPowerState;
                multiCircuitController.PuzzleSolved -= HandlePuzzleSolved;
            }

            boardController = null;
            circuitController = null;
            multiCircuitController = null;
        }

        private void ReevaluateCurrentCircuit()
        {
            switch (currentRuleSet)
            {
                case PuzzleRuleSet.SingleCircuit:
                    circuitController.Reevaluate();
                    break;

                case PuzzleRuleSet.MultiCircuit:
                    multiCircuitController.Reevaluate();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(currentRuleSet), currentRuleSet, "Unsupported puzzle rule set.");
            }
        }

        private void HandlePuzzleSolved()
        {
            boardController.SetInteractionEnabled(false);

            if (puzzleSourceType == PuzzleSourceType.Generated)
            {
                Debug.Log(
                    $"Puzzle solved. Index: {currentPuzzleIndex}, " +
                    $"Layout Seed: {currentLayoutSeed}, " +
                    $"Scramble Seed: {currentScrambleSeed}.",
                    this);

                return;
            }

            Debug.Log($"Authored {currentRuleSet} puzzle solved.", this);
        }

        private void LogPuzzleStarted()
        {
            if (puzzleSourceType == PuzzleSourceType.Generated)
            {
                Debug.Log(
                    $"Puzzle started. Index: {currentPuzzleIndex}, " +
                    $"Layout Seed: {currentLayoutSeed}, " +
                    $"Scramble Seed: {currentScrambleSeed}.",
                    this);

                return;
            }

            Debug.Log($"Authored {currentRuleSet} puzzle started.", this);
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