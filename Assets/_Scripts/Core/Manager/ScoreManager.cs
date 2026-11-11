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
        currentCombo++;

        int comboMultiplier = (int)Mathf.Pow(2, currentCombo - 1); //1x = 1 combo, 2x = 2 combo, 4x = 3 combo
        int addedScore = cardData.cardBaseScore * comboMultiplier;

        baseScore += addedScore;
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
        GameEventHandler.OnTurnUpdated?.Invoke(remainingTurn);
        NotifyScoreUpdate();
    }

    public float GetTurnMultiplier()
    {
        int turnsUsed = maxTurn - remainingTurn;
        float usagePercentage = ((float)turnsUsed / maxTurn) * 100f;

        if (usagePercentage <= 30f) return 3.0f; // Turn yang dipakai <= 30% -> 3x
        if (usagePercentage <= 60f) return 2.0f; // Turn yang dipakai 31% - 60% -> 2x
        if (usagePercentage <= 70f) return 1.0f; // Turn yang dipakai 61% - 70% -> 1x
        return 0.5f; // Turn yang dipakai > 70% -> 0.5x
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