using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Progression Config")]
    [SerializeField] private RunProgressionDataSO runProgression;

    [Header("Item Pools")]
    [SerializeField] private List<ShopItemDataSO> passiveSealPool;
    [SerializeField] private List<ShopItemDataSO> grimoirePool;
    [SerializeField] private List<ShopItemDataSO> consumablePool;

    [Header("Heal Config")]
    [SerializeField] private int healBaseCost = 40;
    [SerializeField] private int healCostIncrement = 30;

    public List<ShopItemDataSO> CurrentPassiveStock { get; private set; } = new List<ShopItemDataSO>();
    public List<ShopItemDataSO> CurrentGrimoireStock { get; private set; } = new List<ShopItemDataSO>();
    public List<ShopItemDataSO> CurrentConsumableStock { get; private set; } = new List<ShopItemDataSO>();
    public List<LevelDataSO> CurrentLevelChoices { get; private set; } = new List<LevelDataSO>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        GenerateShopStock();
        GenerateNextStageChoices();
    }

    public void GenerateShopStock()
    {
        CurrentPassiveStock = PickRandomItems(passiveSealPool, 3);
        CurrentGrimoireStock = PickRandomItems(grimoirePool, 2);
        CurrentConsumableStock = PickRandomItems(consumablePool, 2);
    }

    public void GenerateNextStageChoices()
    {
        CurrentLevelChoices.Clear();
        if (runProgression == null || GameManager.Instance == null) return;

        int nextStage = GameManager.Instance.CurrentRun.currentStage;
        StageOptions stageOption = runProgression.GetStageOption(nextStage);

        if (stageOption == null || stageOption.possibleLevels.Count == 0) return;

        if (stageOption.possibleLevels.Count == 1)
        {
            CurrentLevelChoices.Add(stageOption.possibleLevels[0]);
        }
        else
        {
            List<LevelDataSO> tempPool = new List<LevelDataSO>(stageOption.possibleLevels);
            for (int i = 0; i < 2 && tempPool.Count > 0; i++)
            {
                int randIndex = Random.Range(0, tempPool.Count);
                CurrentLevelChoices.Add(tempPool[randIndex]);
                tempPool.RemoveAt(randIndex);
            }
        }
    }

    private List<ShopItemDataSO> PickRandomItems(List<ShopItemDataSO> pool, int count)
    {
        List<ShopItemDataSO> result = new List<ShopItemDataSO>();
        if (pool == null || pool.Count == 0) return result;

        List<ShopItemDataSO> tempPool = new List<ShopItemDataSO>(pool);

        for (int i = 0; i < count; i++)
        {
            if (tempPool.Count == 0) tempPool.AddRange(pool);

            int randIndex = Random.Range(0, tempPool.Count);
            result.Add(tempPool[randIndex]);
            tempPool.RemoveAt(randIndex);
        }

        return result;
    }

    #region [UI Button]
    public bool BuyHeal()
    {
        return GameManager.Instance.TryPurchaseHeal(healBaseCost, healCostIncrement);
    }

    public int GetHealPrice()
    {
        return GameManager.Instance.GetCurrentHealCost(healBaseCost, healCostIncrement);
    }

    public bool BuyShopItem(ShopItemDataSO item)
    {
        return GameManager.Instance.TryPurchaseItem(item);
    }

    public void SelectNextLevel(int choiceIndex)
    {
        if (choiceIndex < 0 || choiceIndex >= CurrentLevelChoices.Count) return;

        LevelDataSO chosenLevel = CurrentLevelChoices[choiceIndex];
        GameManager.Instance.StartLevel(chosenLevel);
    }
    #endregion
}