using System;
using UnityEngine;

namespace CircuitRepair.Core
{
    public static class DirectionUtility
    {
        public static Direction RotateDirectionsClockwise(this Direction directions, int quarterTurns = 1)
        {
            quarterTurns = ((quarterTurns % 4) + 4) % 4;

            for (int i = 0; i < quarterTurns; i++)
            {
                Direction rotated = Direction.None;

                if ((directions & Direction.North) != 0)
                    rotated |= Direction.East;

                if ((directions & Direction.East) != 0)
                    rotated |= Direction.South;

                if ((directions & Direction.South) != 0)
                    rotated |= Direction.West;

                if ((directions & Direction.West) != 0)
                    rotated |= Direction.North;

                directions = rotated;
            }

            return directions;
        }

        public static Direction Opposite(this Direction directions)
        {
            Direction opposite = Direction.None;

            if ((directions & Direction.North) != 0)
                opposite |= Direction.South;

            if ((directions & Direction.East) != 0)
                opposite |= Direction.West;

            if ((directions & Direction.South) != 0)
                opposite |= Direction.North;

            if ((directions & Direction.West) != 0)
                opposite |= Direction.East;

            return opposite;
        }

        public static Vector2Int ToGridOffset(this Direction direction)
        {
            return direction switch
            {
                Direction.North => new Vector2Int(0, -1),
                Direction.East => new Vector2Int(1, 0),
                Direction.South => new Vector2Int(0, 1),
                Direction.West => new Vector2Int(-1, 0),

                _ => throw new ArgumentOutOfRangeException(
                    nameof(direction),
                    direction,
                    "ToGridOffset must be called with one direction.")
            };
        }

        public static bool IsCardinalDirection(this Direction direction)
        {
            return direction is
                Direction.North or
                Direction.East or
                Direction.South or
                Direction.West;
        }

        public static int CountDirections(this Direction directions)
        {
            int count = 0;

            if ((directions & Direction.North) != 0)
            {
                count++;
            }

            if ((directions & Direction.East) != 0)
            {
                count++;
            }

            if ((directions & Direction.South) != 0)
            {
                count++;
            }

            if ((directions & Direction.West) != 0)
            {
                count++;
            }

            return count;
        }

        public static Direction FromGridOffset(Vector2Int offset)
        {
            if (offset == new Vector2Int(0, -1))
            {
                return Direction.North;
            }

            if (offset == new Vector2Int(1, 0))
            {
                return Direction.East;
            }

            if (offset == new Vector2Int(0, 1))
            {
                return Direction.South;
            }

            if (offset == new Vector2Int(-1, 0))
            {
                return Direction.West;
            }

            throw new ArgumentOutOfRangeException(nameof(offset), offset, "Grid offset must represent exactly one cardinal step.");
        }
    }
}