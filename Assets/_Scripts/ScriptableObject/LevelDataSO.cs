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

    [Header("Level Cards")]
    public int levelBombPair = 1; // 1 pair = 2 tile/bom
    public List<CardDataSO> levelSymbols;
}