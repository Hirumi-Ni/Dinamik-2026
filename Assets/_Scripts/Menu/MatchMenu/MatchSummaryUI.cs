using UnityEngine;
using TMPro;

public class MatchSummaryUI : MonoBehaviour
{
    [Header("Panel Root")]
    [SerializeField] private GameObject summaryPanelRoot;

    [Header("Header & Combined Score")]
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private TMP_Text scoreProgressText;

    [Header("Token Breakdown Container")]
    [SerializeField] private GameObject tokenBreakdownContainer;
    [SerializeField] private TMP_Text baseRewardText;
    [SerializeField] private TMP_Text turnBonusText;
    [SerializeField] private TMP_Text scoreBonusText;

    [Header("Conditional Seal Rows")]
    [SerializeField] private GameObject swiftRow;
    [SerializeField] private TMP_Text swiftBonusText;
    [SerializeField] private GameObject fortuneRow;
    [SerializeField] private TMP_Text fortuneBonusText;

    [Header("Total")]
    [SerializeField] private TMP_Text totalTokensText;

    private bool isStageCleared;

    private void Awake()
    {
        if (summaryPanelRoot != null) summaryPanelRoot.SetActive(false);
    }

    private void OnEnable()
    {
        GameEventHandler.OnStageSummaryTriggered += ShowSummary;
    }

    private void OnDisable()
    {
        GameEventHandler.OnStageSummaryTriggered -= ShowSummary;
    }

    public void ShowSummary(int stageNumber, int totalScore, int targetScore, int remainingTurns, int currentLevelCluster)
    {
        isStageCleared = (totalScore >= targetScore);

        if (headerText != null)
        {
            headerText.text = isStageCleared ? $"STAGE {stageNumber} CLEAR" : $"STAGE {stageNumber} FAILED";
            headerText.color = isStageCleared ? new Color(1f, 0.84f, 0f) : new Color(0.9f, 0.25f, 0.25f);
        }

        if (scoreProgressText != null)
        {
            scoreProgressText.text = $"{totalScore:N0} / {targetScore:N0}";
        }

        if (isStageCleared)
        {
            if (tokenBreakdownContainer != null) tokenBreakdownContainer.SetActive(true);

            int baseReward = 25;
            int turnBonus = Mathf.Max(0, remainingTurns) * 5;
            int excessScore = Mathf.Max(0, totalScore - targetScore);
            int scoreBonus = excessScore / 5;
            int rawTokens = baseReward + turnBonus + scoreBonus;

            if (baseRewardText != null) baseRewardText.text = $"+{baseReward}";
            if (turnBonusText != null) turnBonusText.text = $"+{turnBonus} ({Mathf.Max(0, remainingTurns)}x5)";
            if (scoreBonusText != null) scoreBonusText.text = $"+{scoreBonus}";

            int finalTokens = (UpgradeManager.Instance != null) ? UpgradeManager.Instance.CalculateFinalStageTokens(rawTokens, currentLevelCluster) : rawTokens;

            bool hasSwift = (UpgradeManager.Instance != null && GameManager.Instance.CurrentRun.HasSeal(WaxSealType.SwiftExecution) && currentLevelCluster == 1);
            bool hasFortune = (UpgradeManager.Instance != null && GameManager.Instance.CurrentRun.HasSeal(WaxSealType.Fortune));

            int swiftBonus = hasSwift ? 30 : 0;
            int fortuneBonus = hasFortune ? (finalTokens - rawTokens - swiftBonus) : 0;

            if (swiftRow != null) swiftRow.SetActive(hasSwift);
            if (swiftBonusText != null && hasSwift) swiftBonusText.text = $"+{swiftBonus}";

            if (fortuneRow != null) fortuneRow.SetActive(hasFortune);
            if (fortuneBonusText != null && hasFortune) fortuneBonusText.text = $"+{fortuneBonus}";

            if (totalTokensText != null) totalTokensText.text = $"+{finalTokens}";

            if (GameManager.Instance != null) GameManager.Instance.AddTokens(finalTokens);
        }
        else
        {
            if (tokenBreakdownContainer != null) tokenBreakdownContainer.SetActive(false);
            if (totalTokensText != null) totalTokensText.text = "0";
        }

        if (summaryPanelRoot != null) summaryPanelRoot.SetActive(true);
    }

    public void OnSummaryContinueClicked()
    {
        if (GameManager.Instance == null) return;

        if (isStageCleared)
        {
            if (GameManager.Instance.CurrentRun.currentStage >= 3)
            {
                GameManager.Instance.ChangeScene(GameScenes.GameOverScene);
                return;
            }

            GameManager.Instance.CurrentRun.currentStage++;
            GameManager.Instance.SetState(GameState.Shop);
            GameManager.Instance.ChangeScene(GameScenes.ShopScene);
        }
        else
        {
            GameManager.Instance.SetState(GameState.GameOver);
            GameManager.Instance.ChangeScene(GameScenes.GameOverScene);
        }
    }
}