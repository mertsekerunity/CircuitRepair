using System;
using CircuitRepair.Content;
using CircuitRepair.Core;
using UnityEngine;

namespace CircuitRepair.Debugging
{
    public sealed class MultiCircuitEvaluatorDebugRunner : MonoBehaviour
    {
        [SerializeField] private PuzzleDefinition puzzleDefinition;

        [ContextMenu("Evaluate Multi-Circuit Puzzle")]
        private void EvaluatePuzzle()
        {
            if (puzzleDefinition == null)
            {
                throw new InvalidOperationException($"{nameof(puzzleDefinition)} is not assigned in the Inspector.");
            }

            if (puzzleDefinition.RuleSet != PuzzleRuleSet.MultiCircuit)
            {
                Debug.LogWarning("Assigned puzzle does not use the MultiCircuit rule set.", this);
                return;
            }

            BoardData board = PuzzleBoardFactory.Create(puzzleDefinition);
            MultiCircuitEvaluationResult result = MultiCircuitEvaluator.Evaluate(board);

            Debug.Log(
                $"Multi-circuit evaluation | Networks: {result.NetworkCount} | " +
                $"All Ports Connected: {result.AllPortsConnected} | " +
                $"All Networks Valid: {result.AllNetworksValid} | " +
                $"Solved: {result.IsSolved}",
                this);

            for (int i = 0; i < result.Networks.Count; i++)
            {
                MultiCircuitNetworkResult network = result.Networks[i];

                Debug.Log(
                    $"Network {i} | Tiles: {network.TileCount} | " +
                    $"Sources: {network.SourceCount} ({network.TotalSourceRating}A) | " +
                    $"Bulbs: {network.BulbCount} ({network.TotalBulbDemand}A) | " +
                    $"Grounds: {network.GroundCount} | " +
                    $"State: {network.State}",
                    this);
            }
        }
    }
}