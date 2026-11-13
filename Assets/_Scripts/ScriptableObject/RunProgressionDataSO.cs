using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class StageOptions
{
    public string stageName;
    public List<LevelDataSO> possibleLevels;
}

[CreateAssetMenu(fileName = "NewRunConfigData", menuName = "Game Data/Run Progression")]
public class RunProgressionDataSO : ScriptableObject
{
    public List<StageOptions> stages;
    public StageOptions GetStageOption(int stageNumber)
    {
        int index = Mathf.Clamp(stageNumber - 1, 0, stages.Count - 1);
        return stages[index];
    }
}
