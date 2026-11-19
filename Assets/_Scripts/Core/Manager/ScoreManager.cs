using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Current Level Scores")]
    public int baseScore = 0;
    public int currentCombo = 0;
    public int targetScore = 500;

    [Header("Turn Count")]
    public int maxTurn = 10;
    public int remainingTurn = 10;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ResetScoreForNewLevel(int levelTargetScore, int levelMaxTurn)
    {
        baseScore = 0;
        currentCombo = 0;

        targetScore = levelTargetScore;

        maxTurn = levelMaxTurn;
        remainingTurn = maxTurn;

        NotifyScoreUpdate();
    }

    public void AddMatchScore(CardDataSO cardData)
    {
        bool hasUpgradeMgr = UpgradeManager.Instance != null;
        GameRunStateData run = (GameManager.Instance != null) ? GameManager.Instance.CurrentRun : null;

        int comboGain = (run != null && run.HasSeal(WaxSealType.TwinEclipse) && cardData.cardType == CardType.Crescent) ? 2 : 1;
        currentCombo += comboGain;

        int effectiveCardScore = hasUpgradeMgr ? UpgradeManager.Instance.GetEffectiveBaseScore(cardData) : cardData.cardBaseScore;
        float matchMult = hasUpgradeMgr ? UpgradeManager.Instance.GetMatchMult(currentCombo) : Mathf.Pow(2, currentCombo - 1);

        int turnPlayed = maxTurn - remainingTurn;

        float matchAdditiveMult = 1.0f;
        if (run != null && run.HasSeal(WaxSealType.TrinityCadence) && cardData.cardType == CardType.Triquetra && turnPlayed > 0 && turnPlayed % 3 == 0)
        {
            matchAdditiveMult += 1.5f;
        }

        float totalMatchMult = matchMult * matchAdditiveMult;
        int addedScore = Mathf.RoundToInt(effectiveCardScore * totalMatchMult);
        baseScore += addedScore;

        GameEventHandler.OnMatchScoreCalculated?.Invoke(effectiveCardScore, totalMatchMult, addedScore);

        if (hasUpgradeMgr) UpgradeManager.Instance.OnMatchResolved(cardData, currentCombo);

        GameEventHandler.OnMatchSuccess?.Invoke(cardData, currentCombo);

        NotifyScoreUpdate();
    }

    public void ResetCombo()
    {
        currentCombo = 0;
    }

    public void RegisterTurn()
    {
        remainingTurn--;
        if (remainingTurn < 0) remainingTurn = 0;

        if (UpgradeManager.Instance != null) UpgradeManager.Instance.RegisterTurnAttempt();

        GameEventHandler.OnTurnUpdated?.Invoke(remainingTurn);
        NotifyScoreUpdate();
    }

    public void AddTurn(int amount)
    {
        remainingTurn += amount;
        GameEventHandler.OnTurnUpdated?.Invoke(remainingTurn);
        NotifyScoreUpdate();
    }

    public float GetTurnMultiplier()
    {
        int turnsUsed = maxTurn - remainingTurn;
        float usagePercentage = ((float)turnsUsed / maxTurn) * 100f;

        float baseTurnMult = 0.5f;
        if (usagePercentage <= 30f) baseTurnMult = 3.0f;
        else if (usagePercentage <= 60f) baseTurnMult = 2.0f;
        else if (usagePercentage <= 70f) baseTurnMult = 1.0f;

        if (UpgradeManager.Instance != null)
        {
            return UpgradeManager.Instance.GetEffectiveBaseMult(baseTurnMult);
        }

        return baseTurnMult;
    }

    public float GetNextTurnMultiplier()
    {
        int nextTurnsUsed = (maxTurn - remainingTurn) + 1;
        float nextUsagePercentage = ((float)nextTurnsUsed / maxTurn) * 100f;

        float nextBaseTurnMult = 0.5f;
        if (nextUsagePercentage <= 30f) nextBaseTurnMult = 3.0f;
        else if (nextUsagePercentage <= 60f) nextBaseTurnMult = 2.0f;
        else if (nextUsagePercentage <= 70f) nextBaseTurnMult = 1.0f;

        if (UpgradeManager.Instance != null)
        {
            return UpgradeManager.Instance.GetEffectiveBaseMult(nextBaseTurnMult);
        }

        return nextBaseTurnMult;
    }

    public int GetTotalScore()
    {
        return Mathf.RoundToInt(baseScore * GetTurnMultiplier());
    }

    public bool IsTargetReached()
    {
        return GetTotalScore() >= targetScore;
    }

    public int GetExcessScore()
    {
        return Mathf.Max(0, GetTotalScore() - targetScore);
    }

    private void NotifyScoreUpdate()
    {
        GameEventHandler.OnScoreUpdated?.Invoke(baseScore, GetTurnMultiplier(), GetTotalScore());
    }
}