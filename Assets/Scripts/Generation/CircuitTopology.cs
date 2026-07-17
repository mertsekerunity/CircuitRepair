using System;
using CircuitRepair.Core;
using UnityEngine;

namespace CircuitRepair.Generation
{
    public sealed class CircuitTopology
    {
        private readonly Direction[,] ports;

        public int Width { get; }
        public int Height { get; }

        public CircuitTopology(int width, int height)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Topology width must be greater than zero.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), height, "Topology height must be greater than zero.");
            }

            Width = width;
            Height = height;
            ports = new Direction[width, height];
        }

        public bool IsInside(Vector2Int position)
        {
            return position.x >= 0 &&
                   position.x < Width &&
                   position.y >= 0 &&
                   position.y < Height;
        }

        public Direction GetPorts(Vector2Int position)
        {
            ValidatePosition(position);
            return ports[position.x, position.y];
        }

        public int GetDegree(Vector2Int position)
        {
            return GetPorts(position).CountDirections();
        }

        public bool HasConnection(Vector2Int position, Direction direction)
        {
            ValidatePosition(position);
            ValidateCardinalDirection(direction);

            return (ports[position.x, position.y] & direction) != 0;
        }

        public void Connect(Vector2Int position, Direction direction)
        {
            ValidatePosition(position);
            ValidateCardinalDirection(direction);

            Vector2Int neighbourPosition = position + direction.ToGridOffset();

            if (!IsInside(neighbourPosition))
            {
                throw new InvalidOperationException($"Cannot connect {position} toward {direction} because the neighbour is outside the topology.");
            }

            ports[position.x, position.y] |= direction;
            ports[neighbourPosition.x, neighbourPosition.y] |= direction.Opposite();
        }

        public void Disconnect(Vector2Int position, Direction direction)
        {
            ValidatePosition(position);
            ValidateCardinalDirection(direction);

            Vector2Int neighbourPosition = position + direction.ToGridOffset();

            if (!IsInside(neighbourPosition))
            {
                throw new InvalidOperationException($"Cannot disconnect {position} toward {direction} because the neighbour is outside the topology.");
            }

            ports[position.x, position.y] &= ~direction;
            ports[neighbourPosition.x, neighbourPosition.y] &= ~direction.Opposite();
        }

        private void ValidatePosition(Vector2Int position)
        {
            if (!IsInside(position))
            {
                throw new ArgumentOutOfRangeException(nameof(position), position, "Position is outside the topology boundaries.");
            }
        }

        private static void ValidateCardinalDirection(Direction direction)
        {
            if (!direction.IsCardinalDirection())
            {
                throw new ArgumentException("A topology connection requires exactly one cardinal direction.", nameof(direction));
            }
        }
    }
}