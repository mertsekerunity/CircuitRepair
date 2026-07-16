using System;
using CircuitRepair.Core;
using UnityEngine;

namespace CircuitRepair.Presentation
{
    public sealed class TileView : MonoBehaviour
    {
        [Header("Visual References")]
        [SerializeField] private Transform rotatingVisual;
        [SerializeField] private SpriteRenderer componentRenderer;

        [Header("Power State")]
        [SerializeField] private Color poweredColor = Color.white;
        [SerializeField] private Color unpoweredColor = new Color(0.65f, 0.65f, 0.65f, 1f);

        private TileData data;

        public TileData Data => data;

        public void Initialize(TileData tileData, TileSpriteLibrary spriteLibrary)
        {
            if (tileData == null)
            {
                throw new ArgumentNullException(nameof(tileData));
            }

            if (spriteLibrary == null)
            {
                throw new ArgumentNullException(nameof(spriteLibrary));
            }

            ValidateReferences();

            data = tileData;

            componentRenderer.sprite = spriteLibrary.GetSprite(data.Type);

            Refresh();
        }

        public void Refresh()
        {
            if (data == null)
            {
                throw new InvalidOperationException("TileView can not be refreshed without Initialize call.");
            }

            float clockwiseAngle = -90f * (int)data.Rotation;

            rotatingVisual.localRotation = Quaternion.Euler(0f, 0f, clockwiseAngle);
        }

        private void ValidateReferences()
        {
            if (rotatingVisual == null)
            {
                throw new InvalidOperationException($"{nameof(rotatingVisual)} Not assigned in Inspector.");
            }

            if (componentRenderer == null)
            {
                throw new InvalidOperationException($"{nameof(componentRenderer)} Not assigned in Inspector.");
            }
        }

        public void SetPowered(bool isPowered)
        {
            componentRenderer.color = isPowered ? poweredColor : unpoweredColor;
        }
    }
}