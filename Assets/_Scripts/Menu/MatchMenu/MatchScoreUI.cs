using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class MatchScoreUI : MonoBehaviour
{
    [Header("Score & Turn Displays (Existing)")]
    [SerializeField] private TMP_Text scoreTargetText;
    [SerializeField] private TMP_Text currentScoreText;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text multText;
    [SerializeField] private TMP_Text comboText;

    [Header("Stage Completion (Existing)")]
    [SerializeField] private Button completeStageButton;

    [Header("Match Juice & Next Mult Indicators (New)")]
    [SerializeField] private TMP_Text matchCalcPopupText;   // TMP_Text baru di bawah garis putus-putus
    [SerializeField] private TMP_Text nextMultWarningText;  // TMP_Text baru untuk teks "Next 2x! Mult"

    [Header("Feedback Colors")]
    [SerializeField] private Color gainHighlightColor = new Color(0.35f, 1f, 0.5f, 1f);
    [SerializeField] private Color dropWarningColor = new Color(1f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color nextDropColor = new Color(1f, 0.8f, 0.2f, 1f);

    private int displayedTotalScore = 0;
    private float lastMultiplier = 3f;
    private int lastRemainingTurn = -1;
    private bool wasTargetReached = false;
    private Vector2 popupInitialAnchoredPos;

    private Color defaultScoreColor = Color.white;
    private Color defaultMultColor = Color.white;
    private Color defaultComboColor = Color.white;
    private Color defaultTurnColor = Color.white;

    private Tween totalScoreRollTween;
    private Tween nextMultPulseTween;
    private Tween finishBtnPulseTween;
    private Sequence calcSeq;

    private void Awake()
    {
        if (currentScoreText != null) defaultScoreColor = currentScoreText.color;
        if (multText != null) defaultMultColor = multText.color;
        if (comboText != null) defaultComboColor = comboText.color;
        if (turnText != null) defaultTurnColor = turnText.color;

        if (matchCalcPopupText != null)
        {
            popupInitialAnchoredPos = matchCalcPopupText.rectTransform.anchoredPosition;
            matchCalcPopupText.alpha = 0f;
        }
    }

    private void OnEnable()
    {
        GameEventHandler.OnScoreUpdated += HandleScoreUpdated;
        GameEventHandler.OnTurnUpdated += HandleTurnUpdated;
        GameEventHandler.OnGameStateChanged += HandleGameStateChanged;
        GameEventHandler.OnMatchScoreCalculated += PlayMatchScoreCalculation;

        GameEventHandler.OnMatchSuccess += HandleMatchSuccess;
        GameEventHandler.OnMatchFailed += HandleComboBroken;
        GameEventHandler.OnBombTriggered += HandleComboBroken;
    }

    private void OnDisable()
    {
        GameEventHandler.OnScoreUpdated -= HandleScoreUpdated;
        GameEventHandler.OnTurnUpdated -= HandleTurnUpdated;
        GameEventHandler.OnGameStateChanged -= HandleGameStateChanged;
        GameEventHandler.OnMatchScoreCalculated -= PlayMatchScoreCalculation;

        GameEventHandler.OnMatchSuccess -= HandleMatchSuccess;
        GameEventHandler.OnMatchFailed -= HandleComboBroken;
        GameEventHandler.OnBombTriggered -= HandleComboBroken;

        totalScoreRollTween?.Kill();
        nextMultPulseTween?.Kill();
        finishBtnPulseTween?.Kill();
        calcSeq?.Kill();
    }

    private void Start()
    {
        SyncInitialUI();
    }

    private void SyncInitialUI()
    {
        if (ScoreManager.Instance != null)
        {
            if (scoreTargetText != null)
                scoreTargetText.text = ScoreManager.Instance.targetScore.ToString();

            displayedTotalScore = ScoreManager.Instance.GetTotalScore();
            if (currentScoreText != null)
                currentScoreText.text = displayedTotalScore.ToString();

            lastRemainingTurn = ScoreManager.Instance.remainingTurn;
            if (turnText != null)
                turnText.text = lastRemainingTurn.ToString();

            lastMultiplier = ScoreManager.Instance.GetTurnMultiplier();
            if (multText != null)
                multText.text = $"{lastMultiplier:0.##}x";
        }
        else if (GameManager.Instance != null && GameManager.Instance.CurrentLevelData != null)
        {
            if (scoreTargetText != null)
                scoreTargetText.text = GameManager.Instance.CurrentLevelData.levelTargetScore.ToString();

            lastRemainingTurn = GameManager.Instance.CurrentLevelData.levelMaxTurn;
            if (turnText != null)
                turnText.text = lastRemainingTurn.ToString();
        }

        if (completeStageButton != null)
            completeStageButton.interactable = (ScoreManager.Instance != null && ScoreManager.Instance.IsTargetReached());

        wasTargetReached = false;
        ResetComboText(animateShake: false);
        UpdateNextTurnMultIndicator(lastMultiplier);
    }

    private void HandleGameStateChanged(GameState newState)
    {
        if (newState == GameState.Match)
        {
            SyncInitialUI();
        }
    }

    private void HandleScoreUpdated(int baseScore, float turnMultiplier, int totalScore)
    {
        // 1. Rolling Counter + Punch pada currentScoreText
        if (currentScoreText != null)
        {
            if (totalScore != displayedTotalScore)
            {
                totalScoreRollTween?.Kill();
                int startTotal = displayedTotalScore;
                totalScoreRollTween = DOTween.To(() => startTotal, x =>
                {
                    displayedTotalScore = x;
                    currentScoreText.text = displayedTotalScore.ToString();
                }, totalScore, 0.4f).SetEase(Ease.OutCubic);

                if (totalScore > startTotal)
                    PlayGainPulse(currentScoreText, defaultScoreColor, 0.22f);
                else
                    PlayDropShake(currentScoreText, defaultScoreColor);
            }
            else
            {
                currentScoreText.text = totalScore.ToString();
            }
        }

        // 2. Animasi Stage Multiplier (3x -> 2x -> 1x)
        if (multText != null)
        {
            multText.text = $"{turnMultiplier:0.##}x";

            if (!Mathf.Approximately(turnMultiplier, lastMultiplier))
            {
                if (turnMultiplier > lastMultiplier)
                    PlayGainPulse(multText, defaultMultColor, 0.26f);
                else
                    PlayDropShake(multText, defaultMultColor);
            }
        }

        lastMultiplier = turnMultiplier;
        UpdateNextTurnMultIndicator(turnMultiplier);

        // 3. Cek & Animasi Tombol Finish Stage
        if (completeStageButton != null && ScoreManager.Instance != null)
        {
            bool isReached = ScoreManager.Instance.IsTargetReached();
            completeStageButton.interactable = isReached;

            if (isReached && !wasTargetReached)
            {
                wasTargetReached = true;
                RectTransform btnRect = completeStageButton.GetComponent<RectTransform>();
                if (btnRect != null)
                {
                    btnRect.DOPunchScale(Vector3.one * 0.16f, 0.35f, 6, 1f);
                    finishBtnPulseTween?.Kill();
                    finishBtnPulseTween = btnRect.DOScale(1.04f, 0.65f)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo);
                }
            }
        }
    }

    private void HandleTurnUpdated(int remainingTurn)
    {
        if (turnText == null) return;
        turnText.text = remainingTurn.ToString();

        if (lastRemainingTurn != -1)
        {
            if (remainingTurn > lastRemainingTurn)
            {
                PlayGainPulse(turnText, defaultTurnColor, 0.22f);
            }
            else if (remainingTurn <= 2)
            {
                turnText.DOKill();
                turnText.transform.localScale = Vector3.one;
                turnText.transform.DOPunchScale(Vector3.one * 0.2f, 0.22f, 5, 1f);
                turnText.DOColor(dropWarningColor, 0.1f).OnComplete(() =>
                {
                    if (remainingTurn > 0) turnText.DOColor(defaultTurnColor, 0.35f);
                });
            }
        }

        lastRemainingTurn = remainingTurn;
        if (ScoreManager.Instance != null)
        {
            UpdateNextTurnMultIndicator(ScoreManager.Instance.GetTurnMultiplier());
        }
    }

    private void UpdateNextTurnMultIndicator(float currentStageMult)
    {
        if (ScoreManager.Instance == null || nextMultWarningText == null) return;

        float nextStageMult = ScoreManager.Instance.GetNextTurnMultiplier();
        bool willDropNextTurn = nextStageMult < currentStageMult;

        nextMultPulseTween?.Kill();
        nextMultWarningText.transform.localScale = Vector3.one;

        if (willDropNextTurn)
        {
            nextMultWarningText.text = $"Next {nextStageMult:0.##}x! Mult";
            nextMultWarningText.color = nextDropColor;

            // Teks berdenyut pelan sebagai peringatan bahwa turn berikutnya pengali akan turun
            nextMultPulseTween = nextMultWarningText.transform
                .DOScale(1.08f, 0.55f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            nextMultWarningText.text = $"Next {nextStageMult:0.##}x Mult";
            nextMultWarningText.color = new Color(1f, 1f, 1f, 0.65f);
        }
    }

    private void HandleMatchSuccess(CardDataSO matchedCard, int currentCombo)
    {
        if (comboText != null)
        {
            int comboMult = (int)Mathf.Pow(2, currentCombo - 1);
            comboText.text = $"{comboMult}x";

            float punchStrength = Mathf.Min(0.18f + (currentCombo * 0.045f), 0.42f);
            PlayGainPulse(comboText, defaultComboColor, punchStrength);
        }
    }

    public void PlayMatchScoreCalculation(int effectiveCardScore, float matchMult, int addedBaseScore)
    {
        if (matchCalcPopupText == null) return;

        calcSeq?.Kill();
        matchCalcPopupText.DOKill();
        matchCalcPopupText.transform.DOKill();

        RectTransform popupRect = matchCalcPopupText.rectTransform;
        popupRect.anchoredPosition = popupInitialAnchoredPos;
        matchCalcPopupText.alpha = 1f;
        matchCalcPopupText.color = Color.white;
        matchCalcPopupText.transform.localScale = Vector3.one * 0.65f;

        // Tahap 1: Tampilkan perkalian kartu di bawah garis putus-putus (misal: "+200 × 4x")
        matchCalcPopupText.text = $"+{effectiveCardScore} × {matchMult:0.##}x";

        calcSeq = DOTween.Sequence();
        calcSeq.Append(matchCalcPopupText.transform.DOScale(1f, 0.16f).SetEase(Ease.OutBack));
        calcSeq.AppendInterval(0.22f);

        // Tahap 2: Ubah menjadi hasil bersih (+800) dan punch
        calcSeq.AppendCallback(() =>
        {
            matchCalcPopupText.text = $"+{addedBaseScore}";
            matchCalcPopupText.color = gainHighlightColor;
            matchCalcPopupText.transform.DOPunchScale(Vector3.one * 0.25f, 0.2f, 5, 1f);
        });
        calcSeq.AppendInterval(0.24f);

        // Tahap 3: Terbang naik menyatu ke currentScoreText di atas garis lalu memudar
        calcSeq.Append(popupRect.DOAnchorPosY(popupInitialAnchoredPos.y + 42f, 0.25f).SetEase(Ease.InBack));
        calcSeq.Join(matchCalcPopupText.DOFade(0f, 0.25f));
    }

    private void HandleComboBroken()
    {
        bool hadCombo = comboText != null && comboText.text != "-";
        ResetComboText(animateShake: hadCombo);
    }

    private void ResetComboText(bool animateShake)
    {
        if (comboText == null) return;
        comboText.text = "-";
        if (animateShake) PlayDropShake(comboText, defaultComboColor);
    }

    #region [DOTween Helpers]
    private void PlayGainPulse(TMP_Text targetText, Color originalColor, float punchAmount)
    {
        targetText.DOKill();
        targetText.transform.DOKill();
        targetText.transform.localScale = Vector3.one;

        targetText.transform.DOPunchScale(Vector3.one * punchAmount, 0.26f, 6, 1f);
        targetText.color = gainHighlightColor;
        targetText.DOColor(originalColor, 0.38f).SetEase(Ease.OutQuad);
    }

    private void PlayDropShake(TMP_Text targetText, Color originalColor)
    {
        targetText.DOKill();
        targetText.transform.DOKill();
        targetText.transform.localScale = Vector3.one;

        targetText.transform.DOShakePosition(0.3f, new Vector3(8f, 0f, 0f), 20, 0f);
        targetText.color = dropWarningColor;
        targetText.DOColor(originalColor, 0.4f).SetEase(Ease.OutQuad);
    }
    #endregion

    public void OnCompleteStageClicked()
    {
        if (GameManager.Instance != null) GameManager.Instance.CompleteMatch();
    }
}