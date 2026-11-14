using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Global Config")]
    [SerializeField] private int defaultMaxHP = 3;
    [SerializeField] private LevelDataSO startingLevelData;

    public GameRunStateData CurrentRun { get; private set; }
    public LevelDataSO CurrentLevelData { get; private set; }
    public int CurrentLevelCluster { get; private set; } = 1;
    public GameState CurrentState { get; private set; }

    [SerializeField] private int maxPassiveSlot = 4;
    [SerializeField] private int maxConsumableSlot = 2;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (CurrentRun == null)
            {
                CurrentRun = new GameRunStateData(defaultMaxHP);
                CurrentLevelData = startingLevelData;
            }
        }
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        GameEventHandler.OnBombTriggered += TakeDamage;
    }

    private void OnDisable()
    {
        GameEventHandler.OnBombTriggered -= TakeDamage;
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == GameScenes.StageScene.ToString())
        {
            SetState(GameState.Match);
        }
    }

    public void StartNewRun()
    {
        CurrentRun = new GameRunStateData(defaultMaxHP);
        GameEventHandler.OnHPChanged?.Invoke(CurrentRun.currentHP);
        GameEventHandler.OnTokenChanged?.Invoke(CurrentRun.currentTokens);
        GameEventHandler.OnInventoryUpdated?.Invoke();

        StartLevel(startingLevelData);
    }

    public void StartLevel(LevelDataSO selectedLevel)
    {
        CurrentLevelData = selectedLevel;
        CurrentLevelCluster = 1;
        SetState(GameState.Match);

        ChangeScene(GameScenes.StageScene);
    }

    public void CompleteMatch()
    {
        if (CurrentState != GameState.Match) return;
        if (!ScoreManager.Instance.IsTargetReached()) return;

        int reward = CalculateStageReward();
        AddTokens(reward);

        CurrentRun.currentStage++;
        SetState(GameState.Shop);

        ChangeScene(GameScenes.ShopScene);
    }

    private int CalculateStageReward()
    {
        int baseReward = 25;

        // Sisa turn kali 5 token
        int remainingTurns = Mathf.Max(0, ScoreManager.Instance.remainingTurn);
        int turnBonus = remainingTurns * 5;

        // Sisa target dibagi 5
        int excessScore = ScoreManager.Instance.GetExcessScore();
        int scoreBonus = excessScore / 5;

        int rawTokens = baseReward + turnBonus + scoreBonus;
        return (UpgradeManager.Instance != null) ? UpgradeManager.Instance.CalculateFinalStageTokens(rawTokens, CurrentLevelCluster) : rawTokens;
    }

    #region [Currency & Shop Transaction Helpers]
    public void AddTokens(int amount)
    {
        CurrentRun.currentTokens += amount;
        GameEventHandler.OnTokenChanged?.Invoke(CurrentRun.currentTokens);
    }

    public bool TrySpendTokens(int amount)
    {
        if (CurrentRun.currentTokens < amount) return false;

        CurrentRun.currentTokens -= amount;
        GameEventHandler.OnTokenChanged?.Invoke(CurrentRun.currentTokens);
        return true;
    }

    public int GetCurrentHealCost(int baseCost = 40, int increment = 30)
    {
        return baseCost + (CurrentRun.healPurchaseCount * increment);
    }

    public bool TryPurchaseHeal(int baseCost = 40, int increment = 30)
    {
        if (CurrentRun.currentHP >= CurrentRun.maxHP) return false;

        int cost = GetCurrentHealCost(baseCost, increment);
        if (!TrySpendTokens(cost)) return false;

        CurrentRun.healPurchaseCount++;

        HealPlayer(1);
        return true;
    }

    public bool TryPurchaseItem(ShopItemDataSO item)
    {
        if (item == null) return false;

        if (item.type == ShopItemType.PassiveBuff && CurrentRun.ownedPassives.Count >= maxPassiveSlot) return false;

        if (item.type == ShopItemType.Consumable && CurrentRun.ownedConsumables.Count >= maxConsumableSlot) return false;

        if (!TrySpendTokens(item.basePrice)) return false;

        switch (item.type)
        {
            case ShopItemType.PermanentBuff:
                CurrentRun.AddSymbolBonus(item.grimoireTarget, item.scoreBonusValue);
                break;

            case ShopItemType.PassiveBuff:
                CurrentRun.ownedPassives.Add(item);
                if (item.sealType == WaxSealType.Bloodstone) HealPlayer(2);
                GameEventHandler.OnInventoryUpdated?.Invoke();
                break;

            case ShopItemType.Consumable:
                CurrentRun.ownedConsumables.Add(item);
                GameEventHandler.OnInventoryUpdated?.Invoke();
                break;
        }

        return true;
    }
    #endregion

    #region [Win & Lose Conditions]
    public void TakeDamage()
    {
        CurrentRun.currentHP--;
        GameEventHandler.OnHPChanged?.Invoke(CurrentRun.currentHP);

        if (CurrentRun.currentHP <= 0)
        {
            if (CurrentRun.HasSeal(WaxSealType.UndyingCovenant) && !CurrentRun.hasTriggeredUndyingThisStage)
            {
                CurrentRun.hasTriggeredUndyingThisStage = true;
                CurrentRun.currentHP = 1;

                GameEventHandler.OnHPChanged?.Invoke(CurrentRun.currentHP);

                if (ScoreManager.Instance != null) ScoreManager.Instance.AddTurn(2);
                return;
            }
            SetState(GameState.GameOver);
        }
    }

    public void HealPlayer(int amount)
    {
        CurrentRun.currentHP++;
        GameEventHandler.OnHPChanged?.Invoke(CurrentRun.currentHP);
    }

    public void EvaluateTurnLossCondition()
    {
        if (ScoreManager.Instance.remainingTurn <= 0 && !ScoreManager.Instance.IsTargetReached())
        {
            SetState(GameState.GameOver);
        }
    }

    public void HandleAllSafeCardsCleared()
    {
        if (CurrentState != GameState.Match) return;
        if (UpgradeManager.Instance != null) UpgradeManager.Instance.OnClusterCleared();

        int maxCluster = (CurrentLevelData != null) ? CurrentLevelData.levelMaxCluster : 1;

        if (CurrentLevelCluster < maxCluster)
        {
            StartCoroutine(TransitionToNextCluster(maxCluster));
        }
        else
        {
            if (ScoreManager.Instance.IsTargetReached()) CompleteMatch();
            else SetState(GameState.GameOver);
        }

    }

    private IEnumerator TransitionToNextCluster(int maxCluster)
    {
        MatchManager.Instance.SetInputLocked(true);
        yield return new WaitForSeconds(0.85f);

        if (CurrentState != GameState.Match) yield break;

        CurrentLevelCluster++;
        GameEventHandler.OnClusterUpdated?.Invoke(CurrentLevelCluster, maxCluster);
        BoardManager.Instance.GenerateBoard();
    }
    #endregion

    #region [States & Scenes Management]
    public void SetState(GameState newState)
    {
        CurrentState = newState;
        GameEventHandler.OnGameStateChanged?.Invoke(newState);
    }

    public void ChangeScene(GameScenes gameScenes)
    {
        SceneManager.LoadScene(gameScenes.ToString());
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().ToString()); 
    }
    #endregion
}