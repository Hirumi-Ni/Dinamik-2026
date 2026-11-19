using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class MatchScoreUI : MonoBehaviour
{
    [Header("Score & Turn Displays")]
    [SerializeField] private TMP_Text scoreTargetText;
    [SerializeField] private TMP_Text baseScoreText;
    [SerializeField] private TMP_Text currentScoreText;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text multText;
    [SerializeField] private TMP_Text comboText;

    [Header("Next Mult Warning & Match Popup")]
    [SerializeField] private TMP_Text nextMultWarningText;
    [SerializeField] private Image baseMultBadgeImage;
    [SerializeField] private TMP_Text matchCalcPopupText;

    [Header("Stage Completion")]
    [SerializeField] private Button completeStageButton;

    [Header("Feedback Colors")]
    [SerializeField] private Color gainHighlightColor = new Color(0.35f, 1f, 0.5f, 1f);
    [SerializeField] private Color dropWarningColor = new Color(1f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color nextDropColor = new Color(1f, 0.8f, 0.2f, 1f);

    private int displayedTotalScore = 0;
    private int displayedBaseScore = 0;
    private float lastMultiplier = 3f;
    private int lastRemainingTurn = -1;
    private bool wasTargetReached = false;
    private Vector2 popupInitialAnchoredPos;

    private Color defaultScoreColor = Color.white;
    private Color defaultBaseColor = Color.white;
    private Color defaultMultColor = Color.white;
    private Color defaultComboColor = Color.white;
    private Color defaultTurnColor = Color.white;

    private Tween totalScoreRollTween;
    private Tween baseScoreRollTween;
    private Tween multWarningPulseTween;
    private Tween finishBtnPulseTween;
    private Sequence calcSeq;

    private void Awake()
    {
        if (currentScoreText != null) defaultScoreColor = currentScoreText.color;
        if (baseScoreText != null) defaultBaseColor = baseScoreText.color;
        if (multText != null) defaultMultColor = multText.color;
        if (comboText != null) defaultComboColor = comboText.color;
        if (turnText != null) defaultTurnColor = turnText.color;

        if (matchCalcPopupText != null)
            popupInitialAnchoredPos = matchCalcPopupText.rectTransform.anchoredPosition;
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
        baseScoreRollTween?.Kill();
        multWarningPulseTween?.Kill();
        finishBtnPulseTween?.Kill();
        calcSeq?.Kill();
    }

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentLevelData != null)
        {
            scoreTargetText.text = GameManager.Instance.CurrentLevelData.levelTargetScore.ToString("N0");
            lastRemainingTurn = GameManager.Instance.CurrentLevelData.levelMaxTurn;
            turnText.text = lastRemainingTurn.ToString();
        }

        if (completeStageButton != null) completeStageButton.interactable = false;
        if (matchCalcPopupText != null) matchCalcPopupText.alpha = 0f;

        displayedTotalScore = 0;
        displayedBaseScore = 0;
        wasTargetReached = false;
        lastMultiplier = (ScoreManager.Instance != null) ? ScoreManager.Instance.GetTurnMultiplier() : 3f;

        ResetComboText(animateShake: false);
        UpdateNextTurnMultIndicator(lastMultiplier);
    }

    private void HandleGameStateChanged(GameState newState)
    {
        if (newState == GameState.Match && GameManager.Instance != null && GameManager.Instance.CurrentLevelData != null)
        {
            scoreTargetText.text = GameManager.Instance.CurrentLevelData.levelTargetScore.ToString("N0");
        }
    }

    private void HandleScoreUpdated(int targetBaseScore, float turnMultiplier, int targetTotalScore)
    {
        // 1. Rolling Counter untuk Base Score
        if (baseScoreText != null && targetBaseScore != displayedBaseScore)
        {
            baseScoreRollTween?.Kill();
            int startBase = displayedBaseScore;
            baseScoreRollTween = DOTween.To(() => startBase, x =>
            {
                displayedBaseScore = x;
                baseScoreText.text = displayedBaseScore.ToString("N0");
            }, targetBaseScore, 0.35f).SetEase(Ease.OutCubic);

            if (targetBaseScore > startBase) PlayGainPulse(baseScoreText, defaultBaseColor, 0.18f);
        }

        // 2. Rolling Counter untuk Total Score
        if (currentScoreText != null && targetTotalScore != displayedTotalScore)
        {
            totalScoreRollTween?.Kill();
            int startTotal = displayedTotalScore;
            totalScoreRollTween = DOTween.To(() => startTotal, x =>
            {
                displayedTotalScore = x;
                currentScoreText.text = displayedTotalScore.ToString("N0");
            }, targetTotalScore, 0.42f).SetEase(Ease.OutCubic);

            if (targetTotalScore > startTotal)
                PlayGainPulse(currentScoreText, defaultScoreColor, 0.22f);
            else
                PlayDropShake(currentScoreText, defaultScoreColor);
        }

        // 3. Animasi Stage Multiplier (x3 -> x2 -> x1)
        if (multText != null)
        {
            multText.text = $"x{turnMultiplier:0.##}";

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

        // 4. Cek & Animasi Tombol Finish Stage
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

        multWarningPulseTween?.Kill();

        if (willDropNextTurn)
        {
            nextMultWarningText.text = $"Next: x{nextStageMult:0.##} ▼";
            nextMultWarningText.color = nextDropColor;

            if (baseMultBadgeImage != null)
            {
                multWarningPulseTween = baseMultBadgeImage
                    .DOFade(0.55f, 0.5f)
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }
        else
        {
            nextMultWarningText.text = $"Next: x{nextStageMult:0.##}";
            nextMultWarningText.color = new Color(1f, 1f, 1f, 0.65f);

            if (baseMultBadgeImage != null)
            {
                Color c = baseMultBadgeImage.color;
                c.a = 1f;
                baseMultBadgeImage.color = c;
            }
        }
    }

    private void HandleMatchSuccess(CardDataSO matchedCard, int currentCombo)
    {
        if (comboText != null)
        {
            int comboMult = (int)Mathf.Pow(2, currentCombo - 1);
            comboText.text = $"{comboMult}x";

            // Semakin tinggi combo, semakin bertenaga pantulan angkanya
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

        // Tahap 1: Tampilkan perkalian kartu (misal: "+200 × 4x")
        matchCalcPopupText.text = $"+{effectiveCardScore} × {matchMult:0.##}x";

        calcSeq = DOTween.Sequence();
        calcSeq.Append(matchCalcPopupText.transform.DOScale(1f, 0.16f).SetEase(Ease.OutBack));
        calcSeq.AppendInterval(0.22f);

        // Tahap 2: Ubah menjadi hasil penjumlahan (+800) dengan kilat hijau & punch
        calcSeq.AppendCallback(() =>
        {
            matchCalcPopupText.text = $"+{addedBaseScore:N0}";
            matchCalcPopupText.color = gainHighlightColor;
            matchCalcPopupText.transform.DOPunchScale(Vector3.one * 0.25f, 0.2f, 5, 1f);
        });
        calcSeq.AppendInterval(0.24f);

        // Tahap 3: Melesat ke atas menyatu ke area Current Score lalu memudar
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