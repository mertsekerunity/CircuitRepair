using System;
using CircuitRepair.Core;
using UnityEngine;

namespace CircuitRepair.Presentation
{
    public sealed class BoardConnectionPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SolderJointView solderJointPrefab;
        [SerializeField] private Transform connectionVisualsRoot;
        [SerializeField] private BoardPresenter boardPresenter;

        private SolderJointView[,] eastJoints;
        private SolderJointView[,] southJoints;

        public BoardData Board { get; private set; }

        public void Present(BoardData board)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            ValidateReferences();
            Clear();

            Board = board;
            eastJoints = new SolderJointView[board.Width - 1, board.Height];
            southJoints = new SolderJointView[board.Width, board.Height - 1];

            CreateEastJoints();
            CreateSouthJoints();
            RefreshAll();
        }

        public void RefreshAround(Vector2Int position)
        {
            if (Board == null)
            {
                throw new InvalidOperationException("Connections cannot be refreshed before Present is called.");
            }

            RefreshEastJoint(position);
            RefreshEastJoint(position + Direction.West.ToGridOffset());

            RefreshSouthJoint(position);
            RefreshSouthJoint(position + Direction.North.ToGridOffset());
        }

        public void RefreshAll()
        {
            if (Board == null)
            {
                throw new InvalidOperationException("Connections cannot be refreshed before Present is called.");
            }

            for (int y = 0; y < Board.Height; y++)
            {
                for (int x = 0; x < Board.Width - 1; x++)
                {
                    RefreshEastJoint(new Vector2Int(x, y));
                }
            }

            for (int y = 0; y < Board.Height - 1; y++)
            {
                for (int x = 0; x < Board.Width; x++)
                {
                    RefreshSouthJoint(new Vector2Int(x, y));
                }
            }
        }

        public void Clear()
        {
            DestroyJoints(eastJoints);
            DestroyJoints(southJoints);

            eastJoints = null;
            southJoints = null;
            Board = null;
        }

        private void CreateEastJoints()
        {
            for (int y = 0; y < Board.Height; y++)
            {
                for (int x = 0; x < Board.Width - 1; x++)
                {
                    Vector2Int ownerPosition = new Vector2Int(x, y);
                    Vector2Int neighbourPosition = ownerPosition + Direction.East.ToGridOffset();

                    eastJoints[x, y] = CreateJoint(ownerPosition, neighbourPosition, $"Solder_East_{x}_{y}");
                }
            }
        }

        private void CreateSouthJoints()
        {
            for (int y = 0; y < Board.Height - 1; y++)
            {
                for (int x = 0; x < Board.Width; x++)
                {
                    Vector2Int ownerPosition = new Vector2Int(x, y);
                    Vector2Int neighbourPosition = ownerPosition + Direction.South.ToGridOffset();

                    southJoints[x, y] = CreateJoint(ownerPosition, neighbourPosition, $"Solder_South_{x}_{y}");
                }
            }
        }

        private SolderJointView CreateJoint(Vector2Int firstPosition, Vector2Int secondPosition, string jointName)
        {
            if (!boardPresenter.TryGetTileView(firstPosition, out TileView firstTileView))
            {
                return null;
            }

            if (!boardPresenter.TryGetTileView(secondPosition, out TileView secondTileView))
            {
                return null;
            }

            SolderJointView jointView = Instantiate(solderJointPrefab, connectionVisualsRoot);
            jointView.name = jointName;
            jointView.transform.position = (firstTileView.transform.position + secondTileView.transform.position) * 0.5f;
            jointView.transform.rotation = Quaternion.identity;

            return jointView;
        }

        private void RefreshEastJoint(Vector2Int ownerPosition)
        {
            if (ownerPosition.x < 0 || ownerPosition.x >= Board.Width - 1)
            {
                return;
            }

            if (ownerPosition.y < 0 || ownerPosition.y >= Board.Height)
            {
                return;
            }

            SolderJointView jointView = eastJoints[ownerPosition.x, ownerPosition.y];

            if (jointView == null)
            {
                return;
            }

            bool isConnected = ConnectionRules.AreConnected(Board, ownerPosition, Direction.East);
            jointView.SetConnected(isConnected);
        }

        private void RefreshSouthJoint(Vector2Int ownerPosition)
        {
            if (ownerPosition.x < 0 || ownerPosition.x >= Board.Width)
            {
                return;
            }

            if (ownerPosition.y < 0 || ownerPosition.y >= Board.Height - 1)
            {
                return;
            }

            SolderJointView jointView = southJoints[ownerPosition.x, ownerPosition.y];

            if (jointView == null)
            {
                return;
            }

            bool isConnected = ConnectionRules.AreConnected(Board, ownerPosition, Direction.South);
            jointView.SetConnected(isConnected);
        }

        private void DestroyJoints(SolderJointView[,] joints)
        {
            if (joints == null)
            {
                return;
            }

            foreach (SolderJointView jointView in joints)
            {
                if (jointView != null)
                {
                    Destroy(jointView.gameObject);
                }
            }
        }

        private void ValidateReferences()
        {
            if (solderJointPrefab == null)
            {
                throw new InvalidOperationException($"{nameof(solderJointPrefab)} is not assigned in the Inspector.");
            }

            if (connectionVisualsRoot == null)
            {
                throw new InvalidOperationException($"{nameof(connectionVisualsRoot)} is not assigned in the Inspector.");
            }

            if (boardPresenter == null)
            {
                throw new InvalidOperationException($"{nameof(boardPresenter)} is not assigned in the Inspector.");
            }
        }
    }
}