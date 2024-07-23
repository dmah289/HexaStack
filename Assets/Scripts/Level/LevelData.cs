using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Level Data")]
public class LevelData : ScriptableObject
{
    public int gridSize;
    public int maxHexAmount;
    public bool[] hexGridActiveFlat;
    public Color[] colors;

    public static class Database
    {
        public static LevelData GetCurrentLevelByIndex(int index)
            => Resources.Load<LevelData>($"LevelData/Level {index}");
    }
}
