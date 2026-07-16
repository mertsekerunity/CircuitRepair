using System;
using System.Collections.Generic;
using CircuitRepair.Core;
using UnityEngine;

namespace CircuitRepair.Presentation
{
    public sealed class BoardPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TileView tilePrefab;
        [SerializeField] private TileSpriteLibrary spriteLibrary;
        [SerializeField] private Transform tilesRoot;

        [Header("Layout")]
        [SerializeField, Min(0.1f)] private float tileSpacing = 1f;

        private readonly Dictionary<Vector2Int, TileView> tileViews = new();

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
            Vector2 origin = CalculateOrigin(board.Width, board.Height);

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);

                    if (!board.TryGetTile(position, out TileData tile))
                    {
                        continue;
                    }

                    TileView tileView = Instantiate(tilePrefab, tilesRoot);
                    tileView.name = $"Tile_{x}_{y}_{tile.Type}";
                    tileView.transform.localPosition = CalculateLocalPosition(position, origin);
                    tileView.transform.localRotation = Quaternion.identity;
                    tileView.transform.localScale = Vector3.one;
                    tileView.Initialize(tile, spriteLibrary);

                    tileViews.Add(position, tileView);
                }
            }
        }

        public bool TryGetTileView(Vector2Int position, out TileView tileView)
        {
            return tileViews.TryGetValue(position, out tileView);
        }

        public void RefreshTile(Vector2Int position)
        {
            if (!tileViews.TryGetValue(position, out TileView tileView))
            {
                throw new KeyNotFoundException($"No TileView exists at position {position}.");
            }

            tileView.Refresh();
        }

        public void RefreshAll()
        {
            foreach (TileView tileView in tileViews.Values)
            {
                tileView.Refresh();
            }
        }

        public void Clear()
        {
            foreach (TileView tileView in tileViews.Values)
            {
                if (tileView != null)
                {
                    Destroy(tileView.gameObject);
                }
            }

            tileViews.Clear();
            Board = null;
        }

        private Vector2 CalculateOrigin(int width, int height)
        {
            float startX = -((width - 1) * tileSpacing) * 0.5f;
            float startY = ((height - 1) * tileSpacing) * 0.5f;

            return new Vector2(startX, startY);
        }

        private Vector3 CalculateLocalPosition(Vector2Int gridPosition, Vector2 origin)
        {
            float x = origin.x + gridPosition.x * tileSpacing;
            float y = origin.y - gridPosition.y * tileSpacing;

            return new Vector3(x, y, 0f);
        }

        private void ValidateReferences()
        {
            if (tilePrefab == null)
            {
                throw new InvalidOperationException($"{nameof(tilePrefab)} is not assigned in the Inspector.");
            }

            if (spriteLibrary == null)
            {
                throw new InvalidOperationException($"{nameof(spriteLibrary)} is not assigned in the Inspector.");
            }

            if (tilesRoot == null)
            {
                throw new InvalidOperationException($"{nameof(tilesRoot)} is not assigned in the Inspector.");
            }
        }

        public void RefreshPowerState(CircuitEvaluationResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            foreach (KeyValuePair<Vector2Int, TileView> pair in tileViews)
            {
                pair.Value.SetPowered(result.IsPowered(pair.Key));
            }
        }
    }
}