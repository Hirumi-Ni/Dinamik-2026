using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance { get; private set; }

    #region [Checker Helpers]
    private List<Card> selectedCards = new List<Card>();
    private bool isBombActive = false;
    private bool isProcessMatch = false;
    #endregion

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ResetMatchState()
    {
        selectedCards.Clear();
        isBombActive = false;
        isProcessMatch = false;
    }

    public void OnCardClicked(Card clickedCard)
    {
        if (GameManager.Instance.CurrentState != GameState.Match) return;
        if (isProcessMatch || clickedCard.IsFlipped || clickedCard.IsMatched) return;

        clickedCard.FlipOpen();
        selectedCards.Add(clickedCard);
        GameEventHandler.OnCardFlipped?.Invoke(clickedCard);

        if (selectedCards.Count == 1) 
        {
            if (clickedCard.CardData.isBomb) isBombActive = true;
            return;
        }

        if (selectedCards.Count == 2)
        {
            if (selectedCards[0].CardData.isBomb && selectedCards[1].CardData.isBomb)
            {
                StartCoroutine(ProcessTwoBombsMatch());
            }
            else if (!selectedCards[0].CardData.isBomb && selectedCards[1].CardData.isBomb)
            {
                StartCoroutine(ProcessNormalThenBomb());
            }
            else if (!isBombActive)
            {
                StartCoroutine(ProcessNormalMatch());
            }
            return;
        }

        if (selectedCards.Count == 3 && isBombActive)
        {
            StartCoroutine(ProcessBombRescue());
        }
    }

    private IEnumerator ProcessNormalMatch()
    {
        isProcessMatch = true;
        yield return new WaitForSeconds(0.5f);

        Card cardA = selectedCards[0];
        Card cardB = selectedCards[1];

        if (cardA.CardData.cardType == cardB.CardData.cardType)
        {
            ScoreManager.Instance.AddMatchScore(cardA.CardData);
            cardA.SetMatched();
            cardB.SetMatched();
            GameEventHandler.OnMatchSuccess?.Invoke(cardA.CardData, ScoreManager.Instance.currentCombo);
        }
        else
        {
            ScoreManager.Instance.ResetCombo();
            cardA.FlipClose();
            cardB.FlipClose();
            GameEventHandler.OnMatchFailed?.Invoke();
        }

        EndAttempt();
    }

    private IEnumerator ProcessNormalThenBomb()
    {
        isProcessMatch = true;
        yield return new WaitForSeconds(0.5f);

        Card normalCard = selectedCards[0];
        Card bombCard = selectedCards[1];

        normalCard.FlipClose();

        selectedCards.Clear();
        selectedCards.Add(bombCard);

        isBombActive = true;
        isProcessMatch = false;

        ScoreManager.Instance.RegisterTurn();
        GameManager.Instance.EvaluateTurnLossCondition();
    }

    private IEnumerator ProcessTwoBombsMatch()
    {
        isProcessMatch = true;
        yield return new WaitForSeconds(0.5f);

        ScoreManager.Instance.ResetCombo();
        selectedCards[0].SetBombTriggered();
        selectedCards[1].SetBombTriggered();

        GameEventHandler.OnBombTriggered?.Invoke();
        isBombActive = false;

        EndAttempt();
    }

    private IEnumerator ProcessBombRescue()
    {
        isProcessMatch = true;
        yield return new WaitForSeconds(0.5f);

        Card bombCard = selectedCards[0];
        Card cardB = selectedCards[1];
        Card cardC = selectedCards[2];

        if (cardB.CardData.cardType == cardC.CardData.cardType)
        {
            ScoreManager.Instance.AddMatchScore(cardB.CardData);
            cardB.SetMatched();
            cardC.SetMatched();

            bombCard.SetMatched(); 
            GameEventHandler.OnBombNeutralized?.Invoke();
        }
        else
        {
            ScoreManager.Instance.ResetCombo();
            bombCard.SetBombTriggered(); 

            cardB.FlipClose();
            cardC.FlipClose();

            GameEventHandler.OnBombTriggered?.Invoke();
        }

        isBombActive = false;
        EndAttempt();
    }

    private void EndAttempt()
    {
        ScoreManager.Instance.RegisterTurn(); 
        selectedCards.Clear();
        isProcessMatch = false;

        if (BoardManager.Instance.AllSafeCardMatched())
        {
            GameManager.Instance.HandleAllSafeCardsCleared();
            return;
        }

        GameManager.Instance.EvaluateTurnLossCondition();
    }
}