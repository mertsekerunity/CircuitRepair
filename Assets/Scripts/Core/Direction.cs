using System;

namespace CircuitRepair.Core
{
    [Flags]
    public enum Direction : byte
    {
        None = 0,
        North = 1 << 0,
        East = 1 << 1,
        South = 1 << 2,
        West = 1 << 3,

        All = North | East | South | West
    }
}