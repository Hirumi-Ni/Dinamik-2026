using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MatchScoreUI : MonoBehaviour
{
    [Header("Score & Turn Displays")]
    [SerializeField] private TMP_Text scoreTargetText;
    [SerializeField] private TMP_Text currentScoreText;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text multText;
    [SerializeField] private TMP_Text comboText;

    [Header("Stage Completion")]
    [SerializeField] private Button completeStageButton;

    private void OnEnable()
    {
        GameEventHandler.OnScoreUpdated += HandleScoreUpdated;
        GameEventHandler.OnTurnUpdated += HandleTurnUpdated;
        GameEventHandler.OnGameStateChanged += HandleGameStateChanged;

        GameEventHandler.OnMatchSuccess += HandleMatchSuccess;
        GameEventHandler.OnMatchFailed += ResetComboText;
        GameEventHandler.OnBombTriggered += ResetComboText;
    }

    private void OnDisable()
    {
        GameEventHandler.OnScoreUpdated -= HandleScoreUpdated;
        GameEventHandler.OnTurnUpdated -= HandleTurnUpdated;
        GameEventHandler.OnGameStateChanged -= HandleGameStateChanged;

        GameEventHandler.OnMatchSuccess -= HandleMatchSuccess;
        GameEventHandler.OnMatchFailed -= ResetComboText;
        GameEventHandler.OnBombTriggered -= ResetComboText;
    }

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentLevelData != null)
        {
            scoreTargetText.text = GameManager.Instance.CurrentLevelData.levelTargetScore.ToString();
            turnText.text = GameManager.Instance.CurrentLevelData.levelMaxTurn.ToString();
        }

        if (completeStageButton != null) completeStageButton.interactable = false;

        ResetComboText();
    }

    private void HandleGameStateChanged(GameState newState)
    {
        if (newState == GameState.Match && GameManager.Instance.CurrentLevelData != null)
        {
            scoreTargetText.text = GameManager.Instance.CurrentLevelData.levelTargetScore.ToString();
        }
    }

    private void HandleScoreUpdated(int baseScore, float turnMultiplier, int totalScore)
    {
        currentScoreText.text = totalScore.ToString();
        multText.text = $"{turnMultiplier:0.##}x";

        if (completeStageButton != null && ScoreManager.Instance != null) completeStageButton.interactable = ScoreManager.Instance.IsTargetReached();
    }

    private void HandleTurnUpdated(int remainingTurn)
    {
        turnText.text = remainingTurn.ToString();
    }

    private void HandleMatchSuccess(CardDataSO matchedCard, int currentCombo)
    {
        int comboMult = (int)Mathf.Pow(2, currentCombo - 1);
        comboText.text = $"{comboMult}x";
    }

    private void ResetComboText()
    {
        comboText.text = "-";
    }

    public void OnCompleteStageClicked()
    {
        if (GameManager.Instance != null) GameManager.Instance.CompleteMatch();
    }
}