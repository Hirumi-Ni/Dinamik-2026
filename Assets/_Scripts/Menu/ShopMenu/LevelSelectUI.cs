using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelectUI : MonoBehaviour
{
    [Header("Next Stage Options")]
    [SerializeField] private List<Button> stageChoiceButtons;
    [SerializeField] private List<TMP_Text> stageChoiceLabels;

    private void Start()
    {
        RefreshLevelChoices();
    }

    public void RefreshLevelChoices()
    {
        if (ShopManager.Instance == null) return;

        var choices = ShopManager.Instance.CurrentLevelChoices;
        for (int i = 0; i < stageChoiceButtons.Count; i++)
        {
            if (i < choices.Count)
            {
                LevelDataSO lvl = choices[i];
                stageChoiceButtons[i].gameObject.SetActive(true);
                stageChoiceLabels[i].text = $"{lvl.levelName}\nTarget: {lvl.levelTargetScore} | Turn: {lvl.levelMaxTurn} | Cluster: {lvl.levelMaxCluster}";
            }
            else
            {
                stageChoiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnSelectLevel(int choiceIndex)
    {
        if (ShopManager.Instance == null) return;

        GameEventHandler.OnCloseAllSellPopups?.Invoke();
        ShopManager.Instance.SelectNextLevel(choiceIndex);
    }
}