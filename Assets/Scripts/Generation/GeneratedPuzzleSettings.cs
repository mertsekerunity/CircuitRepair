using System;
using UnityEngine;

namespace CircuitRepair.Generation
{
    [Serializable]
    public sealed class GeneratedPuzzleSettings
    {
        [SerializeField, Min(3)] private int width = 5;
        [SerializeField, Min(3)] private int height = 5;

        [SerializeField] private int scrambleSeed = 67890;

        public int Width => width;
        public int Height => height;
        public int ScrambleSeed => scrambleSeed;
    }
}