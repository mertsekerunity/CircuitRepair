using System;
using UnityEngine;

namespace CircuitRepair.Generation
{
    [Serializable]
    public sealed class GeneratedPuzzleSettings
    {
        [SerializeField, Min(3)] private int width = 5;
        [SerializeField, Min(3)] private int height = 5;
        [SerializeField, Min(0)] private int minimumTJunctionCount = 6;
        [SerializeField, Min(0)] private int maximumTJunctionCount = 10;

        [Header("Seeds")]
        [SerializeField] private int layoutSeed = 12345;
        [SerializeField] private int scrambleSeed = 67890;

        public int Width => width;
        public int Height => height;
        public int MinimumTJunctionCount => minimumTJunctionCount;
        public int MaximumTJunctionCount => maximumTJunctionCount;
        public int LayoutSeed => layoutSeed;
        public int ScrambleSeed => scrambleSeed;
    }
}