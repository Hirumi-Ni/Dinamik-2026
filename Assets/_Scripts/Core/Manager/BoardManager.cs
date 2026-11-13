using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    [Header("Parent & Prefab")]
    [SerializeField] private GameObject gridParent;
    [SerializeField] private GameObject cardPrefab;

    [Header("Card Defaults")]
    [SerializeField] private List<CardDataSO> cardDefaultData;
    [SerializeField] private CardDataSO bombData;
    private List<Card> activeCard = new List<Card>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentLevelData != null)
        {
            LevelDataSO data = GameManager.Instance.CurrentLevelData;
            ScoreManager.Instance.ResetScoreForNewLevel(data.levelTargetScore, data.levelMaxTurn);
            GenerateBoard();
            GameEventHandler.OnClusterUpdated?.Invoke(GameManager.Instance.CurrentLevelCluster, data.levelMaxCluster);
        }
        else
        {
            GenerateBoard();
        }
    }

    public void GenerateBoard()
    {
        ClearBoard();
        List<CardDataSO> deck = BuildDeck();
        ShuffleBoard(deck);
        PopulateBoard(deck);

        MatchManager.Instance.ResetMatchState();
    }

    private void ClearBoard()
    {
        activeCard.Clear();
        foreach (Transform item in gridParent.transform)
        {
            Destroy(item.gameObject);
        }
    }

    private List<CardDataSO> BuildDeck()
    {
        List<CardDataSO> deck = new List<CardDataSO>();
        LevelDataSO levelData = GameManager.Instance.CurrentLevelData;

        int bombPairs = (levelData != null) ? levelData.levelBombPair : 1;
        int totalBombCards = bombPairs * 2;
        int totalSigilCards = 12 - totalBombCards; 
        int sigilPairs = totalSigilCards / 2;

        List<CardDataSO> symbolList = (levelData != null && levelData.levelSymbols.Count > 0) ? levelData.levelSymbols : cardDefaultData;

        for (int i = 0; i < totalBombCards; i++)
        {
            deck.Add(bombData);
        }

        int startIndex = Random.Range(0, symbolList.Count);

        for (int i = 0; i < sigilPairs; i++)
        {
            int currentIndex = (startIndex + i) % symbolList.Count;
            CardDataSO selectedSigil = symbolList[currentIndex];
            deck.Add(selectedSigil);
            deck.Add(selectedSigil);
        }

        return deck;
    }

    private void ShuffleBoard(List<CardDataSO> deck)
    {
        for (int i = 0; i < deck.Count; i++)
        {
            CardDataSO temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    private void PopulateBoard(List<CardDataSO> deck)
    {
        foreach (CardDataSO data in deck)
        {
            GameObject cardObj = Instantiate(cardPrefab, gridParent.transform);
            Card cardComp = cardObj.GetComponent<Card>();
            cardComp.Setup(data);
            activeCard.Add(cardComp);
        }
    }

    public bool AllSafeCardMatched()
    {
        foreach (Card card in activeCard)
        {
            if (!card.CardData.isBomb && !card.IsMatched) return false;
        }
        return true;
    }
}