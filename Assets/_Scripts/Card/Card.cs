using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public CardDataSO CardData { get; private set; }
    [SerializeField] private Image cardBackgroundImage;
    [SerializeField] private Image cardIconImage;
    [SerializeField] private Sprite cardFrontViewBG;
    [SerializeField] private Sprite cardBackViewBG;
    [SerializeField] private CanvasGroup tesMatch;

    #region [Properties]
    public bool IsFlipped { get; private set; } = false;
    public bool IsMatched { get; private set; } = false;
    #endregion

    public void Setup(CardDataSO data)
    {
        CardData = data;
        IsFlipped = false;
        IsMatched = false;

        cardIconImage.sprite = CardData.cardIconSprite;
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
        ShowFront();

        cardBackgroundImage.color = Color.red;
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

    public void SetMatched()
    {
        IsMatched = true;
        IsFlipped = true;
        tesMatch.alpha = .5f;
    }
}
