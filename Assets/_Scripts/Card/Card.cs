using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Card : MonoBehaviour
{
    public CardDataSO CardData { get; private set; }

    [Header("Card Visuals")]
    [SerializeField] private Image cardBackgroundImage;
    [SerializeField] private Image cardIconImage;
    [SerializeField] private Sprite cardFrontViewBG;
    [SerializeField] private Sprite cardBackViewBG;
    [SerializeField] private CanvasGroup cardCanvasGroup; 

    [Header("Bomb Warning")]
    [SerializeField] private GameObject bombWarningOverlay;

    #region [Properties]
    public bool IsFlipped { get; private set; } = false;
    public bool IsMatched { get; private set; } = false;
    public bool IsMarkedBomb { get; private set; } = false;
    #endregion

    private Coroutine peekCoroutine;
    private Button cardButton;

    private void Awake()
    {
        cardButton = GetComponent<Button>();
    }

    public void Setup(CardDataSO data)
    {
        CardData = data;
        IsFlipped = false;
        IsMatched = false;
        IsMarkedBomb = false;

        if (peekCoroutine != null)
        {
            StopCoroutine(peekCoroutine);
            peekCoroutine = null;
        }

        if (cardCanvasGroup != null) cardCanvasGroup.alpha = 1f;
        if (cardButton != null) cardButton.interactable = true;
        if (bombWarningOverlay != null) bombWarningOverlay.SetActive(false);

        cardBackgroundImage.color = Color.white;
        cardIconImage.sprite = CardData.cardIconSprite;
        if (cardCanvasGroup != null) cardCanvasGroup.alpha = 1f;

        ShowBack();
    }

    public void ClickCard()
    {
        if (IsFlipped || IsMatched) return;
        MatchManager.Instance.OnCardClicked(this);
    }

    public void FlipOpen()
    {
        IsFlipped = true;

        if (peekCoroutine != null)
        {
            StopCoroutine(peekCoroutine);
            peekCoroutine = null;
        }

        if (cardCanvasGroup != null) cardCanvasGroup.alpha = 1f;
        ShowFront();
    }

    public void FlipClose()
    {
        IsFlipped = false;
        ShowBack();
    }

    public void SetBombTriggered()
    {
        IsMatched = true;
        IsFlipped = true;
        if (cardCanvasGroup != null) cardCanvasGroup.alpha = 1f;
        ShowFront();

        cardBackgroundImage.color = Color.red;
    }

    public void SetMatched()
    {
        IsMatched = true;
        IsFlipped = true;
        if (cardCanvasGroup != null) cardCanvasGroup.alpha = 0.5f;
    }

    private void ShowFront()
    {
        cardIconImage.gameObject.SetActive(true);
        cardBackgroundImage.sprite = cardFrontViewBG;
    }

    private void ShowBack()
    {
        cardIconImage.gameObject.SetActive(false);
        cardBackgroundImage.sprite = cardBackViewBG;
    }

    #region [Upgrade & Consumable Effects: Peek & Garlic]
    public void Peek(float duration)
    {
        if (IsFlipped || IsMatched) return;

        if (peekCoroutine != null) StopCoroutine(peekCoroutine);
        peekCoroutine = StartCoroutine(PeekRoutine(duration));
    }

    private IEnumerator PeekRoutine(float duration)
    {
        ShowFront();
        if (cardCanvasGroup != null) cardCanvasGroup.alpha = 0.45f;

        yield return new WaitForSeconds(duration);

        if (!IsFlipped && !IsMatched)
        {
            if (cardCanvasGroup != null) cardCanvasGroup.alpha = 1f;
            ShowBack();
        }

        peekCoroutine = null;
    }

    public void MarkAsBomb()
    {
        IsMarkedBomb = true;

        if (peekCoroutine != null)
        {
            StopCoroutine(peekCoroutine);
            peekCoroutine = null;
        }

        ShowBack();
        cardBackgroundImage.color = new Color(0.65f, 0.15f, 0.15f, 1f);

        if (cardCanvasGroup != null) cardCanvasGroup.alpha = 1f;
        if (bombWarningOverlay != null) bombWarningOverlay.SetActive(true);
    }
    #endregion
}