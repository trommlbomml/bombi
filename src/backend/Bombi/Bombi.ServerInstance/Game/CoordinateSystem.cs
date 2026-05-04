using System.Drawing;
using System.Numerics;

namespace Bombi.ServerInstance.Game;

public static class CoordinateSystem
{
    public static Vector2 TileToWorldCentered(Point tile)
        => new(
            tile.X * Constants.TileSize + Constants.TileSize / 2,
            tile.Y * Constants.TileSize + Constants.TileSize / 2
        );
}