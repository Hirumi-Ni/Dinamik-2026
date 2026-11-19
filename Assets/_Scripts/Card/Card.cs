using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CardView))]
public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    public CardDataSO CardData { get; private set; }

    #region [Properties]
    public bool IsFlipped { get; private set; } = false;
    public bool IsMatched { get; private set; } = false;
    public bool IsMarkedBomb { get; private set; } = false;
    #endregion

    private CardView cardView;
    private Button cardButton;
    private bool isDealing = false;

    private void Awake()
    {
        cardView = GetComponent<CardView>();
        cardButton = GetComponent<Button>();
    }

    public void Setup(CardDataSO data)
    {
        CardData = data;
        IsFlipped = false;
        IsMatched = false;
        IsMarkedBomb = false;
        isDealing = false;

        if (cardButton != null) cardButton.interactable = true;
        if (cardView != null) cardView.InitializeVisual(CardData.cardIconSprite);
    }
    public void PrepareForDeal()
    {
        isDealing = true;
        if (cardButton != null) cardButton.interactable = false;
        cardView?.PrepareHiddenForDeal();
    }

    public void PlayDealAnimation(Vector3 deckWorldOrigin, float delay)
    {
        isDealing = true;
        if (cardButton != null) cardButton.interactable = false;

        if (cardView != null)
        {
            cardView.PlayDeal(deckWorldOrigin, delay, () =>
            {
                isDealing = false;
                if (!IsMatched && cardButton != null) cardButton.interactable = true;
            });
        }
        else
        {
            isDealing = false;
            if (cardButton != null) cardButton.interactable = true;
        }
    }

    #region [Pointer & Click Events]
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsMatched || isDealing) return;
        cardView?.PlayHoverEnter();
        cardView?.UpdateHoverTilt(eventData);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (IsMatched || isDealing) return;
        cardView?.UpdateHoverTilt(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        cardView?.PlayHoverExit();
    }

    public void ClickCard()
    {
        if (IsFlipped || IsMatched || isDealing) return;
        MatchManager.Instance.OnCardClicked(this);
    }
    #endregion

    #region [State Transitions]
    public void FlipOpen()
    {
        IsFlipped = true;
        cardView?.PlayFlip(showFrontSide: true);
    }

    public void FlipClose()
    {
        IsFlipped = false;
        cardView?.PlayFlip(showFrontSide: false);
    }

    public void SetMatched()
    {
        IsMatched = true;
        IsFlipped = true;
        if (cardButton != null) cardButton.interactable = false;
        cardView?.SetMatchedVisual();
    }

    public void SetBombArmed()
    {
        IsMatched = true;
        IsFlipped = true;
        if (cardButton != null) cardButton.interactable = false;
        cardView?.SetBombArmedVisual();
    }

    public void SetBombExploded()
    {
        IsMatched = true;
        IsFlipped = true;
        if (cardButton != null) cardButton.interactable = false;
        cardView?.SetBombExplodedVisual();
    }

    public void SetBombDefused()
    {
        IsMatched = true;
        IsFlipped = true;
        if (cardButton != null) cardButton.interactable = false;
        cardView?.SetBombDefusedVisual();
    }
    #endregion

    #region [Consumable & Seal Effects]
    public void Peek(float duration)
    {
        if (IsFlipped || IsMatched) return;
        cardView?.PlayPeek(duration, () => !IsFlipped && !IsMatched);
    }

    public void MarkAsBomb()
    {
        IsMarkedBomb = true;
        cardView?.ShowMarkedBombVisual();
    }
    #endregion
}