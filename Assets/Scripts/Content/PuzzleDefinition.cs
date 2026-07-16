using System.Collections.Generic;
using UnityEngine;

namespace CircuitRepair.Content
{
    [CreateAssetMenu(fileName = "PuzzleDefinition", menuName = "Circuit Repair/Puzzle Definition")]
    public sealed class PuzzleDefinition : ScriptableObject
    {
        [Header("Board Size")]
        [SerializeField, Min(1)] private int width = 3;
        [SerializeField, Min(1)] private int height = 3;

        [Header("Tiles")]
        [SerializeField] private List<TileDefinition> tiles = new();

        public int Width => width;
        public int Height => height;
        public IReadOnlyList<TileDefinition> Tiles => tiles;

        [ContextMenu("Validate Puzzle")]
        private void ValidatePuzzle()
        {
            IReadOnlyList<string> errors = PuzzleDefinitionValidator.GetErrors(this);

            if (errors.Count == 0)
            {
                Debug.Log($"Puzzle definition '{name}' is valid.", this);
                return;
            }

            foreach (string error in errors)
            {
                Debug.LogError($"{name}: {error}", this);
            }
        }
    }
}