using System;
using System.Collections.Generic;
using UnityEngine;

namespace CircuitRepair.Generation
{
    public sealed class FullGridPath
    {
        private readonly List<Vector2Int> positions;

        public int Width { get; }
        public int Height { get; }
        public int Count => positions.Count;
        public Vector2Int Start => positions[0];
        public Vector2Int End => positions[positions.Count - 1];
        public IReadOnlyList<Vector2Int> Positions => positions;

        public Vector2Int this[int index] => positions[index];

        public FullGridPath(int width, int height, IEnumerable<Vector2Int> positions)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Path width must be greater than zero.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), height, "Path height must be greater than zero.");
            }

            Width = width;
            Height = height;
            this.positions = positions != null ? new List<Vector2Int>(positions) : throw new ArgumentNullException(nameof(positions));

            Validate();
        }

        private void Validate()
        {
            int expectedPositionCount = Width * Height;

            if (positions.Count != expectedPositionCount)
            {
                throw new InvalidOperationException($"A full grid path must contain exactly {expectedPositionCount} positions, but found {positions.Count}.");
            }

            HashSet<Vector2Int> visitedPositions = new();

            for (int i = 0; i < positions.Count; i++)
            {
                Vector2Int position = positions[i];

                if (!IsInside(position))
                {
                    throw new InvalidOperationException($"Path position {position} is outside the {Width}x{Height} grid.");
                }

                if (!visitedPositions.Add(position))
                {
                    throw new InvalidOperationException($"Path visits position {position} more than once.");
                }

                if (i > 0 && GetManhattanDistance(positions[i - 1], position) != 1)
                {
                    throw new InvalidOperationException($"Path positions {positions[i - 1]} and {position} are not cardinal neighbours.");
                }
            }
        }

        private bool IsInside(Vector2Int position)
        {
            return position.x >= 0 && position.x < Width && position.y >= 0 && position.y < Height;
        }

        private static int GetManhattanDistance(Vector2Int first, Vector2Int second)
        {
            return Mathf.Abs(first.x - second.x) + Mathf.Abs(first.y - second.y);
        }
    }
}