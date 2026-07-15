using System;
using CircuitRepair.Core;
using CircuitRepair.Presentation;
using UnityEngine;

namespace CircuitRepair.Gameplay
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private BoardPresenter boardPresenter;

        public BoardData Board { get; private set; }

        private void Start()
        {
            if (boardPresenter == null)
            {
                throw new InvalidOperationException($"{nameof(boardPresenter)} is not assigned in the Inspector.");
            }

            Board = PrototypeBoardFactory.CreatePresentationTestBoard();
            boardPresenter.Present(Board);
        }
    }
}