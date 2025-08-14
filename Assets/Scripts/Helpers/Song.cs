
using System;
using System.Collections.Generic;

/// <summary>
/// Represents one song's chart data.
/// Stored as JSON, loaded at runtime.
/// </summary>
[Serializable]
public class Song
{
    public string Name;

    public string Author;

    public string AudioFilePath;

    public float DropSpeed;

    public List<TileData> Tiles;

    public List<int> ScoreStars;
}
