using System;
using CircuitRepair.Core;
using CircuitRepair.Input;
using CircuitRepair.Presentation;
using UnityEngine;

namespace CircuitRepair.Gameplay
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private BoardPresenter boardPresenter;
        [SerializeField] private BoardConnectionPresenter boardConnectionPresenter;
        [SerializeField] private BoardInputController boardInputController;

        private BoardController boardController;

        public BoardData Board { get; private set; }

        private void Start()
        {
            ValidateReferences();

            Board = PrototypeBoardFactory.CreatePresentationTestBoard();

            boardPresenter.Present(Board);
            boardConnectionPresenter.Present(Board);

            boardController = new BoardController(Board);
            boardController.TileRotated += boardPresenter.RefreshTile;
            boardController.TileRotated += boardConnectionPresenter.RefreshAround;

            boardInputController.Initialize(boardController);
        }

        private void OnDestroy()
        {
            if (boardController == null)
            {
                return;
            }

            boardController.TileRotated -= boardPresenter.RefreshTile;
            boardController.TileRotated -= boardConnectionPresenter.RefreshAround;
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
        }
    }
}