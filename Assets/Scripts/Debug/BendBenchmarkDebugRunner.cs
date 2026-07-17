using System.Collections.Generic;
using CircuitRepair.Core;
using CircuitRepair.Generation;
using UnityEngine;

namespace CircuitRepair.Debugging
{
    public sealed class BendBenchmarkDebugRunner : MonoBehaviour
    {
        private static readonly int[] TargetBendCounts = { 8, 10, 12, 14, 16, 18 };

        [SerializeField] private int firstLayoutSeed;
        [SerializeField, Min(1)] private int seedsToScan = 5000;
        [SerializeField, Min(1)] private int seedsPerBendCount = 3;

        private void Start()
        {
            Dictionary<int, List<int>> seedsByBendCount = CreateResultDictionary();

            for (int offset = 0; offset < seedsToScan; offset++)
            {
                int layoutSeed = firstLayoutSeed + offset;
                GeneratedPuzzleLayout layout = RandomSinglePathPuzzleLayoutGenerator.Generate(5, 5, layoutSeed);
                int bendCount = CountBends(layout);

                if (!seedsByBendCount.TryGetValue(bendCount, out List<int> matchingSeeds))
                {
                    continue;
                }

                if (matchingSeeds.Count >= seedsPerBendCount)
                {
                    continue;
                }

                matchingSeeds.Add(layoutSeed);

                if (HaveEnoughSeeds(seedsByBendCount))
                {
                    break;
                }
            }

            LogResults(seedsByBendCount);
        }

        private static Dictionary<int, List<int>> CreateResultDictionary()
        {
            Dictionary<int, List<int>> results = new();

            foreach (int bendCount in TargetBendCounts)
            {
                results.Add(bendCount, new List<int>());
            }

            return results;
        }

        private bool HaveEnoughSeeds(Dictionary<int, List<int>> seedsByBendCount)
        {
            foreach (List<int> seeds in seedsByBendCount.Values)
            {
                if (seeds.Count < seedsPerBendCount)
                {
                    return false;
                }
            }

            return true;
        }

        private static int CountBends(GeneratedPuzzleLayout layout)
        {
            int bendCount = 0;

            for (int y = 0; y < layout.Topology.Height; y++)
            {
                for (int x = 0; x < layout.Topology.Width; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);

                    if (position == layout.SourcePosition || position == layout.GroundPosition || layout.IsBulb(position))
                    {
                        continue;
                    }

                    Direction ports = layout.Topology.GetPorts(position);

                    if (WireTileResolver.TryResolve(ports, out TileType type, out _) && type == TileType.Bend)
                    {
                        bendCount++;
                    }
                }
            }

            return bendCount;
        }

        private void LogResults(Dictionary<int, List<int>> seedsByBendCount)
        {
            Debug.Log("===== BEND BENCHMARK SEEDS =====", this);

            foreach (int bendCount in TargetBendCounts)
            {
                List<int> seeds = seedsByBendCount[bendCount];
                string seedText = seeds.Count > 0 ? string.Join(", ", seeds) : "None found";

                Debug.Log($"Bends: {bendCount} | Layout Seeds: {seedText}", this);
            }
        }
    }
}