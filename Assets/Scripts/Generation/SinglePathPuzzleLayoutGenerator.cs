using System;
using CircuitRepair.Core;
using UnityEngine;

namespace CircuitRepair.Generation
{
    public static class SinglePathPuzzleLayoutGenerator
    {
        private const float MinimumBulbPathFraction = 0.30f;
        private const float MaximumBulbPathFraction = 0.70f;

        public static GeneratedPuzzleLayout Generate(int width, int height)
        {
            FullGridPath path = SerpentinePathGenerator.Generate(width, height);
            return Generate(path);
        }

        public static GeneratedPuzzleLayout Generate(FullGridPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            CircuitTopology topology = CreateTopology(path);
            Vector2Int bulbPosition = ChooseBulbPosition(path, topology);

            return new GeneratedPuzzleLayout(topology, path.Start, path.End, new[] { bulbPosition });
        }

        private static CircuitTopology CreateTopology(FullGridPath path)
        {
            CircuitTopology topology = new CircuitTopology(path.Width, path.Height);

            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector2Int currentPosition = path[i];
                Vector2Int nextPosition = path[i + 1];
                Vector2Int offset = nextPosition - currentPosition;
                Direction direction = DirectionUtility.FromGridOffset(offset);

                topology.Connect(currentPosition, direction);
            }

            return topology;
        }

        private static Vector2Int ChooseBulbPosition(FullGridPath path, CircuitTopology topology)
        {
            int lastPathIndex = path.Count - 1;
            int minimumIndex = Mathf.CeilToInt(lastPathIndex * MinimumBulbPathFraction);
            int maximumIndex = Mathf.FloorToInt(lastPathIndex * MaximumBulbPathFraction);
            int targetIndex = lastPathIndex / 2;

            int bestIndex = -1;
            int bestDistanceFromCenter = int.MaxValue;

            for (int index = minimumIndex; index <= maximumIndex; index++)
            {
                Vector2Int position = path[index];
                Direction ports = topology.GetPorts(position);

                if (ports != (Direction.West | Direction.East))
                {
                    continue;
                }

                int distanceFromCenter = Mathf.Abs(index - targetIndex);

                if (distanceFromCenter >= bestDistanceFromCenter)
                {
                    continue;
                }

                bestIndex = index;
                bestDistanceFromCenter = distanceFromCenter;
            }

            if (bestIndex < 0)
            {
                throw new InvalidOperationException("No horizontal bulb position could be found within the allowed middle path range.");
            }

            return path[bestIndex];
        }
    }
}