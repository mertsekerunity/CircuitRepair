using System;
using System.Collections.Generic;
using UnityEngine;

namespace CircuitRepair.Core
{
    public sealed class CircuitEvaluationResult
    {
        private readonly HashSet<Vector2Int> poweredPositions;

        public bool GroundReached { get; }
        public bool AllBulbsPowered { get; }
        public bool AllPortsConnected { get; }
        public bool AllTilesPowered { get; }

        public bool IsSolved => GroundReached && AllBulbsPowered && AllPortsConnected && AllTilesPowered;
        public int PoweredTileCount => poweredPositions.Count;

        public CircuitEvaluationResult(IEnumerable<Vector2Int> poweredPositions, bool groundReached, bool allBulbsPowered, bool allPortsConnected, bool allTilesPowered)
        {
            this.poweredPositions = poweredPositions != null
                ? new HashSet<Vector2Int>(poweredPositions)
                : throw new ArgumentNullException(nameof(poweredPositions));

            GroundReached = groundReached;
            AllBulbsPowered = allBulbsPowered;
            AllPortsConnected = allPortsConnected;
            AllTilesPowered = allTilesPowered;
        }

        public bool IsPowered(Vector2Int position)
        {
            return poweredPositions.Contains(position);
        }
    }
}