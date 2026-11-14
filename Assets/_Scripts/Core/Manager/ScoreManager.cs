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

        float matchAdditiveMult = 1.0f;
        if (run != null && run.HasSeal(WaxSealType.TrinityCadence) && cardData.cardType == CardType.Triquetra && remainingTurn % 3 == 0)
        {
            matchAdditiveMult += 1.5f;
        }

        int addedScore = Mathf.RoundToInt(effectiveCardScore * matchMult * matchAdditiveMult);
        baseScore += addedScore;

        if (hasUpgradeMgr) UpgradeManager.Instance.OnMatchResolved(cardData, currentCombo);

        NotifyScoreUpdate();
    }

    public void ResetCombo()
    {
        currentCombo = 0;
    }

    public void RegisterTurn()
    {
        remainingTurn--;
        Debug.Log($"Current Turn {remainingTurn}");
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
        if (usagePercentage <= 30f) baseTurnMult = 3.0f; // <= 30% -> 3x
        else if (usagePercentage <= 60f) baseTurnMult = 2.0f; // 31% - 60% -> 2x
        else if (usagePercentage <= 70f) baseTurnMult = 1.0f; // 61% - 70% -> 1x

        if (GameManager.Instance != null && GameManager.Instance.CurrentRun != null)
        {
            GameRunStateData run = GameManager.Instance.CurrentRun;

            if (run.HasSeal(WaxSealType.GlassCannon))
            {
                int missingHP = Mathf.Max(0, run.GetEffectiveMaxHP() - run.currentHP);
                baseTurnMult += missingHP * 0.75f;
            }

            if (run.HasSeal(WaxSealType.Ouroboros))
            {
                baseTurnMult += run.ouroborosStack * 0.15f;
            }
        }

        return baseTurnMult;
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