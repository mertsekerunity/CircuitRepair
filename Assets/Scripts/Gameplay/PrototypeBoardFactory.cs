using CircuitRepair.Core;
using UnityEngine;

namespace CircuitRepair.Gameplay
{
    public static class PrototypeBoardFactory
    {
        public static BoardData CreatePresentationTestBoard()
        {
            BoardData board = new BoardData(3, 3);

            board.SetTile(new TileData(new Vector2Int(0, 0), TileType.Source));
            board.SetTile(new TileData(new Vector2Int(1, 0), TileType.Bend, TileRotation.Degrees270));
            board.SetTile(new TileData(new Vector2Int(2, 0), TileType.TJunction));

            board.SetTile(new TileData(new Vector2Int(0, 1), TileType.Bulb));
            board.SetTile(new TileData(new Vector2Int(1, 1), TileType.Straight, TileRotation.Degrees90));
            board.SetTile(new TileData(new Vector2Int(2, 1), TileType.Straight, TileRotation.Degrees90));

            board.SetTile(new TileData(new Vector2Int(0, 2), TileType.Bend));
            board.SetTile(new TileData(new Vector2Int(1, 2), TileType.Bend, TileRotation.Degrees90));
            board.SetTile(new TileData(new Vector2Int(2, 2), TileType.Ground));

            return board;
        }
    }
}