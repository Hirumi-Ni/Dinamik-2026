using System.Collections;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance { get; private set; }

    private Card firstSelectedCard;
    private Card secondSelectedCard;
    private Card activeDeathCard;

    private bool isCheckingMatch = false;
    private bool isInputLocked = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ResetMatchState();
    }

    public void ResetMatchState()
    {
        firstSelectedCard = null;
        secondSelectedCard = null;
        activeDeathCard = null;
        isCheckingMatch = false;
        isInputLocked = false;
    }

    public void SetInputLocked(bool locked)
    {
        isInputLocked = locked;
    }

    public void OnCardClicked(Card card)
    {
        if (isInputLocked || isCheckingMatch || card == null || card.IsFlipped || card.IsMatched) return;

        card.FlipOpen();

        if (firstSelectedCard == null && card.CardData.cardType == CardType.Bomb)
        {
            ActivateNewDeathCard(card);
            ScoreManager.Instance.ResetCombo();
            GameEventHandler.OnMatchFailed?.Invoke();
            return;
        }

        if (firstSelectedCard == null)
        {
            firstSelectedCard = card;
            return;
        }

        // KASUS 3: Klik Kedua
        secondSelectedCard = card;
        StartCoroutine(CheckMatchRoutine());
    }

    private IEnumerator CheckMatchRoutine()
    {
        isCheckingMatch = true;
        yield return new WaitForSeconds(0.5f);

        if (secondSelectedCard.CardData.cardType == CardType.Bomb)
        {
            ActivateNewDeathCard(secondSelectedCard);
            firstSelectedCard.FlipClose();

            ScoreManager.Instance.ResetCombo();
            GameEventHandler.OnMatchFailed?.Invoke();

            ScoreManager.Instance.RegisterTurn();
            if (GameManager.Instance.CurrentState == GameState.Match)
            {
                GameManager.Instance.EvaluateTurnLossCondition();
            }
        }
        else if (firstSelectedCard.CardData.cardType == secondSelectedCard.CardData.cardType)
        {
            firstSelectedCard.SetMatched();
            secondSelectedCard.SetMatched();

            if (activeDeathCard != null)
            {
                activeDeathCard.SetBombDefused();
                activeDeathCard = null;
            }

            ScoreManager.Instance.RegisterTurn();
            ScoreManager.Instance.AddMatchScore(firstSelectedCard.CardData);

            if (BoardManager.Instance.AllSafeCardMatched())
            {
                activeDeathCard = null;
                GameManager.Instance.HandleAllSafeCardsCleared();
            }
            else
            {
                GameManager.Instance.EvaluateTurnLossCondition();
            }
        }
        else
        {
            firstSelectedCard.FlipClose();
            secondSelectedCard.FlipClose();

            if (activeDeathCard != null)
            {
                DetonateActiveDeathCard();
            }

            ScoreManager.Instance.ResetCombo();
            GameEventHandler.OnMatchFailed?.Invoke();

            ScoreManager.Instance.RegisterTurn();
            if (GameManager.Instance.CurrentState == GameState.Match)
            {
                GameManager.Instance.EvaluateTurnLossCondition();
            }
        }

        firstSelectedCard = null;
        secondSelectedCard = null;
        isCheckingMatch = false;
    }

    private void ActivateNewDeathCard(Card newDeathCard)
    {
        if (activeDeathCard != null)
        {
            DetonateActiveDeathCard();
        }

        if (GameManager.Instance.CurrentState != GameState.Match) return;

        activeDeathCard = newDeathCard;
        activeDeathCard.SetBombArmed();
    }

    private void DetonateActiveDeathCard()
    {
        if (activeDeathCard == null) return;

        activeDeathCard.SetBombExploded();
        activeDeathCard = null;

        GameEventHandler.OnBombTriggered?.Invoke();
    }

    public bool TryDefuseActiveDeathCard()
    {
        if (activeDeathCard == null) return false;

        activeDeathCard.SetBombDefused();
        activeDeathCard = null;
        return true;
    }
}