using System;

/// <summary>
/// Represents one note/tile in the chart.
/// </summary>
[Serializable]
public class TileData
{
    public float HitTime; // seconds from song start when it should be hit

    public bool IsLong;

    public float Duration; // > 0 for long notes

    public Lane Lane;
}