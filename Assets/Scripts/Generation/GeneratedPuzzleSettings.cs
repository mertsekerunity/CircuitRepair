using System;
using UnityEngine;

namespace CircuitRepair.Generation
{
    [Serializable]
    public sealed class GeneratedPuzzleSettings
    {
        [SerializeField, Min(3)] private int width = 5;
        [SerializeField, Min(3)] private int height = 5;

        [Header("Seeds")]
        [SerializeField] private int layoutSeed = 12345;
        [SerializeField] private int scrambleSeed = 67890;

        [Header("Path")]
        [SerializeField, Min(0)] private int minimumBendCount = 14;
        [SerializeField, Min(0)] private int maximumBendCount = 18;

        public int Width => width;
        public int Height => height;
        public int LayoutSeed => layoutSeed;
        public int ScrambleSeed => scrambleSeed;
        public int MinimumBendCount => minimumBendCount;
        public int MaximumBendCount => maximumBendCount;
    }
}