using System;
using System.Collections.Generic;
using UnityEngine;

namespace CircuitRepair.Generation
{
    public static class SerpentinePathGenerator
    {
        public static FullGridPath Generate(int width, int height)
        {
            if (width < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Serpentine path width must be at least three.");
            }

            if (height < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(height), height, "Serpentine path height must be at least three.");
            }

            if (height % 2 == 0)
            {
                throw new ArgumentException("Horizontal serpentine paths require an odd height to end at the bottom-right position.", nameof(height));
            }

            List<Vector2Int> positions = new(width * height);

            for (int y = 0; y < height; y++)
            {
                if (y % 2 == 0)
                {
                    for (int x = 0; x < width; x++)
                    {
                        positions.Add(new Vector2Int(x, y));
                    }
                }
                else
                {
                    for (int x = width - 1; x >= 0; x--)
                    {
                        positions.Add(new Vector2Int(x, y));
                    }
                }
            }

            FullGridPath path = new FullGridPath(width, height, positions);

            if (path.Start != Vector2Int.zero)
            {
                throw new InvalidOperationException($"Generated path must start at (0, 0), but starts at {path.Start}.");
            }

            Vector2Int expectedGroundPosition = new Vector2Int(width - 1, height - 1);

            if (path.End != expectedGroundPosition)
            {
                throw new InvalidOperationException($"Generated path must end at {expectedGroundPosition}, but ends at {path.End}.");
            }

            return path;
        }
    }
}