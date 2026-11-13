using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchHUDController : MonoBehaviour
{
    [Header("Left Panel")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private TextMeshProUGUI tokenText;
    [SerializeField] private List<GameObject> lifeIcons; 
    [SerializeField] private TextMeshProUGUI clusterText;

    [Header("Right Panel")]
    [SerializeField] private TextMeshProUGUI scoreTargetText;
    [SerializeField] private Button completeStageButton;
    [SerializeField] private TextMeshProUGUI currentScoreText;
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private TextMeshProUGUI multText;
    [SerializeField] private TextMeshProUGUI comboText; 

    private void OnEnable()
    {
        GameEventHandler.OnScoreUpdated += HandleScoreUpdated;
        GameEventHandler.OnTurnUpdated += HandleTurnUpdated;
        GameEventHandler.OnHPChanged += HandleHPChanged;
        GameEventHandler.OnClusterUpdated += HandleClusterUpdated;
        GameEventHandler.OnGameStateChanged += HandleGameStateChanged;

        GameEventHandler.OnMatchSuccess += HandleMatchSuccess;
        GameEventHandler.OnMatchFailed += ResetComboText;
        GameEventHandler.OnBombTriggered += ResetComboText;

        completeStageButton.onClick.AddListener(OnCompleteStageClicked);
    }

    private void OnDisable()
    {
        GameEventHandler.OnScoreUpdated -= HandleScoreUpdated;
        GameEventHandler.OnTurnUpdated -= HandleTurnUpdated;
        GameEventHandler.OnHPChanged -= HandleHPChanged;
        GameEventHandler.OnClusterUpdated -= HandleClusterUpdated;
        GameEventHandler.OnGameStateChanged -= HandleGameStateChanged;

        GameEventHandler.OnMatchSuccess -= HandleMatchSuccess;
        GameEventHandler.OnMatchFailed -= ResetComboText;
        GameEventHandler.OnBombTriggered -= ResetComboText;

        completeStageButton.onClick.RemoveListener(OnCompleteStageClicked);
    }

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentLevelData != null)
        {
            scoreTargetText.text = GameManager.Instance.CurrentLevelData.levelTargetScore.ToString();
            turnText.text = GameManager.Instance.CurrentLevelData.levelMaxTurn.ToString();
        }

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
        multText.text = $"{turnMultiplier}x";
        completeStageButton.interactable = ScoreManager.Instance.IsTargetReached();
    }

    private void HandleTurnUpdated(int remainingTurn)
    {
        turnText.text = remainingTurn.ToString();
    }

    private void HandleHPChanged(int currentHP)
    {
        for (int i = 0; i < lifeIcons.Count; i++)
        {
            lifeIcons[i].SetActive(i < currentHP);
        }
    }

    private void HandleClusterUpdated(int currentCluster, int maxCluster)
    {
        clusterText.text = $"{currentCluster}/{maxCluster}";
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

    private void OnCompleteStageClicked()
    {
        GameManager.Instance.CompleteMatch();
    }
}