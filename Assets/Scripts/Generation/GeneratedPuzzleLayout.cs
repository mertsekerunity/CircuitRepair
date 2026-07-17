using System;
using System.Collections.Generic;
using UnityEngine;

namespace CircuitRepair.Generation
{
    public sealed class GeneratedPuzzleLayout
    {
        private readonly HashSet<Vector2Int> bulbPositions;

        public CircuitTopology Topology { get; }
        public Vector2Int SourcePosition { get; }
        public Vector2Int GroundPosition { get; }
        public IReadOnlyCollection<Vector2Int> BulbPositions => bulbPositions;

        public GeneratedPuzzleLayout(CircuitTopology topology, Vector2Int sourcePosition, Vector2Int groundPosition, IEnumerable<Vector2Int> bulbPositions)
        {
            Topology = topology ?? throw new ArgumentNullException(nameof(topology));
            SourcePosition = sourcePosition;
            GroundPosition = groundPosition;
            this.bulbPositions = bulbPositions != null ? new HashSet<Vector2Int>(bulbPositions) : throw new ArgumentNullException(nameof(bulbPositions));

            Validate();
        }

        public bool IsBulb(Vector2Int position)
        {
            return bulbPositions.Contains(position);
        }

        private void Validate()
        {
            ValidatePosition(SourcePosition, "Source");
            ValidatePosition(GroundPosition, "Ground");

            if (SourcePosition == GroundPosition)
            {
                throw new InvalidOperationException("Source and ground cannot occupy the same position.");
            }

            if (bulbPositions.Count == 0)
            {
                throw new InvalidOperationException("Generated puzzle layout must contain at least one bulb.");
            }

            foreach (Vector2Int bulbPosition in bulbPositions)
            {
                ValidatePosition(bulbPosition, "Bulb");

                if (bulbPosition == SourcePosition || bulbPosition == GroundPosition)
                {
                    throw new InvalidOperationException($"Bulb at position {bulbPosition} overlaps source or ground.");
                }
            }
        }

        private void ValidatePosition(Vector2Int position, string roleName)
        {
            if (!Topology.IsInside(position))
            {
                throw new InvalidOperationException($"{roleName} position {position} is outside the topology boundaries.");
            }
        }
    }
}