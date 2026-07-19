using System;
using System.Collections.Generic;
using UnityEngine;

namespace CircuitRepair.Core
{
    public sealed class MultiCircuitEvaluationResult
    {
        private readonly List<MultiCircuitNetworkResult> networks;
        private readonly Dictionary<Vector2Int, MultiCircuitNetworkResult> networksByPosition;

        public IReadOnlyList<MultiCircuitNetworkResult> Networks => networks;

        public bool AllPortsConnected { get; }
        public bool AllNetworksValid { get; }
        public bool IsSolved => AllPortsConnected && AllNetworksValid;

        public int NetworkCount => networks.Count;

        public MultiCircuitEvaluationResult(IEnumerable<MultiCircuitNetworkResult> networks, bool allPortsConnected)
        {
            if (networks == null)
            {
                throw new ArgumentNullException(nameof(networks));
            }

            this.networks = new List<MultiCircuitNetworkResult>(networks);
            networksByPosition = new Dictionary<Vector2Int, MultiCircuitNetworkResult>();

            bool allNetworksValid = this.networks.Count > 0;

            foreach (MultiCircuitNetworkResult network in this.networks)
            {
                if (network == null)
                {
                    throw new ArgumentException("Network collection cannot contain null entries.", nameof(networks));
                }

                if (!network.IsValid)
                {
                    allNetworksValid = false;
                }

                foreach (Vector2Int position in network.Positions)
                {
                    if (networksByPosition.ContainsKey(position))
                    {
                        throw new InvalidOperationException($"Position {position} belongs to more than one network.");
                    }

                    networksByPosition.Add(position, network);
                }
            }

            AllPortsConnected = allPortsConnected;
            AllNetworksValid = allNetworksValid;
        }

        public bool TryGetNetwork(Vector2Int position, out MultiCircuitNetworkResult network)
        {
            return networksByPosition.TryGetValue(position, out network);
        }

        public bool IsSourceReachable(Vector2Int position)
        {
            return TryGetNetwork(position, out MultiCircuitNetworkResult network) && network.SourceCount > 0;
        }

        public MultiCircuitNetworkState GetNetworkState(Vector2Int position)
        {
            if (!TryGetNetwork(position, out MultiCircuitNetworkResult network))
            {
                throw new ArgumentOutOfRangeException(nameof(position), position, "Position does not belong to an evaluated network.");
            }

            return network.State;
        }
    }
}