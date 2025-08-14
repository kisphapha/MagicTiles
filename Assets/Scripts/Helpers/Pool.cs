using System;
using UnityEngine;

[Serializable]
public class Pool
{
    public string tag;         // e.g. "Tile", "Decoration"
    public GameObject prefab;  // The prefab to pool
    public int size;           // Initial pool size
}