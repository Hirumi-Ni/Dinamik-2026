using System;

public static class GameEventHandler
{
    #region [Event Kartu/Match] 
    public static Action<Card> OnCardFlipped; 
    public static Action<CardDataSO, int> OnMatchSuccess; // (Data Kartu, ComboCount)
    public static Action OnMatchFailed;
    public static Action OnBombTriggered;
    public static Action OnBombNeutralized;
    #endregion

    #region [Event Score/Progress]
    public static Action<int> OnTurnUpdated; // (Current Turn)
    public static Action<int, float, int> OnScoreUpdated; // (BaseScore, TurnMultiplier, TotalScore)
    public static Action<int> OnHPChanged; // (Current HP)
    public static Action<int> OnTokenChanged; // (Current Token)
    public static Action<GameState> OnGameStateChanged;
    public static Action<int, int> OnClusterUpdated; // (Current Cluster, Max Cluster)
    public static Action<int, int, int, int, int> OnStageSummaryTriggered; // (stageNumber, totalScore, targetScore, remainingTurns, currentLevelCluster)
    #endregion

    public static Action OnInventoryUpdated;
    public static Action<ShopItemDataSO> OnItemTooltipShow;
    public static Action OnItemTooltipHide;
    public static Action OnCloseAllSellPopups;
    public static Action OnSigilStatsUpdated;
}
