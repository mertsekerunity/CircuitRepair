using System;
using CircuitRepair.Gameplay;
using CircuitRepair.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CircuitRepair.Input
{
    public sealed class BoardInputController : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField, Min(0.05f)] private float maxTapDuration = 0.5f;

        private BoardController boardController;
        private TileView pressedTile;
        private float pressStartedAt;

        public void Initialize(BoardController controller)
        {
            boardController = controller ?? throw new ArgumentNullException(nameof(controller));

            if (worldCamera == null)
            {
                throw new InvalidOperationException($"{nameof(worldCamera)} is not assigned in the Inspector.");
            }
        }

        private void Update()
        {
            if (boardController == null || Pointer.current == null)
            {
                return;
            }

            if (Pointer.current.press.wasPressedThisFrame)
            {
                BeginPress();
            }

            if (Pointer.current.press.wasReleasedThisFrame)
            {
                EndPress();
            }
        }

        private void BeginPress()
        {
            Vector2 screenPosition = Pointer.current.position.ReadValue();

            pressedTile = GetTileAtScreenPosition(screenPosition);
            pressStartedAt = Time.unscaledTime;
        }

        private void EndPress()
        {
            if (pressedTile == null)
            {
                return;
            }

            Vector2 screenPosition = Pointer.current.position.ReadValue();
            TileView releasedTile = GetTileAtScreenPosition(screenPosition);

            float pressDuration = Time.unscaledTime - pressStartedAt;
            bool isShortTap = pressDuration <= maxTapDuration;
            bool releasedOnSameTile = releasedTile == pressedTile;

            if (isShortTap && releasedOnSameTile && pressedTile.Data != null)
            {
                boardController.TryRotateTile(pressedTile.Data.Position);
            }

            pressedTile = null;
        }

        private TileView GetTileAtScreenPosition(Vector2 screenPosition)
        {
            Vector3 worldPosition = ScreenToWorldPosition(screenPosition);
            Collider2D hitCollider = Physics2D.OverlapPoint(worldPosition);

            if (hitCollider == null)
            {
                return null;
            }

            TileView tileView = hitCollider.GetComponentInParent<TileView>();

            if (tileView == null || tileView.Data == null)
            {
                return null;
            }

            return tileView;
        }

        private Vector3 ScreenToWorldPosition(Vector2 screenPosition)
        {
            float distanceFromCamera = -worldCamera.transform.position.z;
            Vector3 screenPoint = new Vector3(screenPosition.x, screenPosition.y, distanceFromCamera);

            return worldCamera.ScreenToWorldPoint(screenPoint);
        }
    }
}