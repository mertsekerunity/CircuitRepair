using System;
using System.Collections.Generic;
using UnityEngine;

namespace CircuitRepair.Core
{
    public sealed class MultiCircuitNetworkResult
    {
        private readonly HashSet<Vector2Int> positions;

        public IReadOnlyCollection<Vector2Int> Positions => positions;

        public int SourceCount { get; }
        public int BulbCount { get; }
        public int GroundCount { get; }

        public int TotalSourceRating { get; }
        public int TotalBulbDemand { get; }

        public MultiCircuitNetworkState State { get; }

        public bool IsValid => State == MultiCircuitNetworkState.Valid;
        public int TileCount => positions.Count;

        public MultiCircuitNetworkResult(IEnumerable<Vector2Int> positions, int sourceCount, int bulbCount, int groundCount, int totalSourceRating, int totalBulbDemand, MultiCircuitNetworkState state)
        {
            this.positions = positions != null
                ? new HashSet<Vector2Int>(positions)
                : throw new ArgumentNullException(nameof(positions));

            SourceCount = sourceCount;
            BulbCount = bulbCount;
            GroundCount = groundCount;
            TotalSourceRating = totalSourceRating;
            TotalBulbDemand = totalBulbDemand;
            State = state;
        }

        public bool Contains(Vector2Int position)
        {
            return positions.Contains(position);
        }
    }
}