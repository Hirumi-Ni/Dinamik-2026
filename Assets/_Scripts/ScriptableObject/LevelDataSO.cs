using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Game Data/Level Data")]
public class LevelDataSO : ScriptableObject
{
    [Header("Level Identity")]
    public string levelName = "Level 1";
    public string levelDesc = "Desc";

    [Header("Level Value")]
    public int levelTargetScore = 500;      
    public int levelMaxTurn = 10;
    public int levelMaxCluster = 1;

    [Header("Level Cards")]
    public List<CardDataSO> levelSymbols;
    [Min(0)] public int levelBombPair = 1; // 1 pair = 2 tile/bom
    [Min(4)] public int levelTotalCards = 12;

    private void OnValidate()
    {
        if (levelTotalCards < 4) levelTotalCards = 4;
        if (levelTotalCards % 2 != 0) levelTotalCards += 1;

        int maxAllowedBombPairs = (levelTotalCards - 2) / 2;
        levelBombPair = Mathf.Clamp(levelBombPair, 0, maxAllowedBombPairs);
    }
}