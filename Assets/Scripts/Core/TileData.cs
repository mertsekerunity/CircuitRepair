using UnityEngine;

namespace CircuitRepair.Core
{
    public sealed class TileData
    {
        public Vector2Int Position { get; }
        public TileType Type { get; }

        public TileRotation Rotation { get; private set; }

        public bool IsRotatable => TileRules.IsRotatable(Type);

        public Direction Ports =>
            TileRules.GetPorts(Type, Rotation);

        public TileData(Vector2Int position, TileType type, TileRotation rotation = TileRotation.Degrees0)
        {
            Position = position;
            Type = type;

            Rotation = IsRotatable
                ? rotation
                : TileRotation.Degrees0;
        }

        public bool HasPort(Direction direction)
        {
            return (Ports & direction) != 0;
        }

        public void RotateClockwise()
        {
            if (!IsRotatable)
                return;

            Rotation = (TileRotation)(((int)Rotation + 1) % 4);
        }
    }
}