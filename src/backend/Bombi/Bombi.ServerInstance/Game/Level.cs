using System.Drawing;
using System.Numerics;
using Bombi.ServerInstance.Networking;

namespace Bombi.ServerInstance.Game;

public sealed class Level
{
    private readonly TileType[] _tileType = new[]
    {
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
        1, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 1,
        1, 0, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 0, 1,
        1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1,
        1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1,
        1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1,
        1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1,
        1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1,
        1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1,
        1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1,
        1, 0, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 0, 1,
        1, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 1,
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
    }.Select(i => (TileType)i).ToArray();

    private readonly List<Bomb> _bombs = new();

    public Level()
    {
        for (var i = 0; i < _tileType.Length; i++)
        {
            if (_tileType[i] != TileType.Box) continue;
            _tileType[i] = Random.Shared.NextDouble() <= Constants.BoxSpawnPropability 
                ? TileType.Box 
                : TileType.Empty;
        }
    }

    public Vector2 GetStartPositionForPlayer(int index) =>
        index switch
        {
            0 => CoordinateSystem.TileToWorldCentered(new Point(1, 1)),
            1 => CoordinateSystem.TileToWorldCentered(new Point(13, 1)),
            2 => CoordinateSystem.TileToWorldCentered(new Point(1, 9)),
            3 => CoordinateSystem.TileToWorldCentered(new Point(13, 9)),
            _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
        };

    public void SerializeTo(ISerializerTarget target)
    {
        foreach (var tileType in _tileType)
        {
            target.Write((byte)tileType);
        }
        
        target.Write(_bombs.Count);
        foreach (var bomb in _bombs)
        {
            bomb.SerializeTo(target);
        }
    }
}