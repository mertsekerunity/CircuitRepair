using System;
using CircuitRepair.Core;
using UnityEngine;

namespace CircuitRepair.Content
{
    [Serializable]
    public sealed class TileDefinition
    {
        [SerializeField] private Vector2Int position;
        [SerializeField] private TileType type;
        [SerializeField] private TileRotation initialRotation;

        public Vector2Int Position => position;
        public TileType Type => type;
        public TileRotation InitialRotation => initialRotation;
    }
}