using System;
using CircuitRepair.Core;
using UnityEngine;

namespace CircuitRepair.Presentation
{
    [CreateAssetMenu(fileName = "TileSpriteLibrary", menuName = "Circuit Repair/Tile Sprite Library")]
    public sealed class TileSpriteLibrary : ScriptableObject
    {
        [Header("Tile Sprites")]
        [SerializeField] private Sprite straight;
        [SerializeField] private Sprite bend;
        [SerializeField] private Sprite tJunction;
        [SerializeField] private Sprite source;
        [SerializeField] private Sprite ground;
        [SerializeField] private Sprite bulb;

        public Sprite GetSprite(TileType type)
        {
            Sprite sprite = type switch
            {
                TileType.Straight => straight,
                TileType.Bend => bend,
                TileType.TJunction => tJunction,
                TileType.Source => source,
                TileType.Ground => ground,
                TileType.Bulb => bulb,

                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Undefined tile type.")
            };

            if (sprite == null)
            {
                throw new InvalidOperationException($"no sprite for {type} in TileSpriteLibrary.");
            }

            return sprite;
        }
    }
}