using System;
using UnityEngine;

namespace CircuitRepair.Presentation
{
    [RequireComponent(typeof(Camera))]
    public sealed class BoardCameraFitter : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float paddingMultiplier = 1.15f;
        [SerializeField] private Vector2 centerOffset;

        private Camera worldCamera;

        private void Awake()
        {
            worldCamera = GetComponent<Camera>();
        }

        public void Fit(BoardPresenter boardPresenter)
        {
            if (boardPresenter == null)
            {
                throw new ArgumentNullException(nameof(boardPresenter));
            }

            if (!worldCamera.orthographic)
            {
                throw new InvalidOperationException("BoardCameraFitter requires an orthographic camera.");
            }

            if (worldCamera.aspect <= 0f)
            {
                throw new InvalidOperationException("Camera aspect ratio must be greater than zero.");
            }

            Vector2 boardSize = boardPresenter.GetBoardWorldSize();

            float sizeRequiredByHeight = boardSize.y * 0.5f;
            float sizeRequiredByWidth = boardSize.x / (2f * worldCamera.aspect);

            worldCamera.orthographicSize = Mathf.Max(sizeRequiredByHeight, sizeRequiredByWidth) * paddingMultiplier;

            Vector3 boardCenter = boardPresenter.GetBoardWorldCenter();
            float cameraX = boardCenter.x + centerOffset.x;
            float cameraY = boardCenter.y + centerOffset.y;

            worldCamera.transform.position = new Vector3(cameraX, cameraY, worldCamera.transform.position.z);
        }
    }
}