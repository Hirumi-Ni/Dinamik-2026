using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    [Header("Parent & Prefab")]
    [SerializeField] private GameObject gridParent;
    [SerializeField] private GameObject cardPrefab;

    [Header("Grid Sizing")]
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private Vector2 defaultCardSize = new Vector2(240f, 340f);
    [SerializeField] private Vector2 defaultSpacing = new Vector2(10f, 10f);

    [Header("Card Defaults")]
    [SerializeField] private List<CardDataSO> cardDefaultData;
    [SerializeField] private CardDataSO bombData;
    private List<Card> activeCard = new List<Card>();

    [Header("Deal Animation")]
    [SerializeField] private RectTransform deckSpawnPoint; 
    [SerializeField] private float dealDelay = 0.05f;
    private Coroutine dealBoardCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (gridLayout == null && gridParent != null) gridLayout = gridParent.GetComponent<GridLayoutGroup>();
        RectTransform gridRect = gridParent.GetComponent<RectTransform>();
    }

    private void Start()
    {
        if (UpgradeManager.Instance != null) UpgradeManager.Instance.ResetStageTracker();

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
        if (dealBoardCoroutine != null)
        {
            StopCoroutine(dealBoardCoroutine);
            dealBoardCoroutine = null;
        }

        ClearBoard();

        if (MatchManager.Instance != null)
        {
            MatchManager.Instance.ResetMatchState();
        }

        List<CardDataSO> deck = BuildDeck();
        ShuffleBoard(deck);
        PopulateBoard(deck);

        if (deckSpawnPoint != null && activeCard.Count > 0)
        {
            dealBoardCoroutine = StartCoroutine(DealCardsRoutine());
        }
        else if (GameManager.Instance != null && GameManager.Instance.CurrentRun != null &&
                 GameManager.Instance.CurrentRun.HasSeal(WaxSealType.EyeOfHorus))
        {
            StartCoroutine(EyeOfHorusPeek(0.3f));
        }
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
        LevelDataSO levelData = GameManager.Instance != null ? GameManager.Instance.CurrentLevelData : null;

        int totalCards = (levelData != null) ? levelData.levelTotalCards : 12;
        if (totalCards % 2 != 0) totalCards += 1;

        AdjustGridLayout(totalCards);

        int bombPairs = (levelData != null) ? levelData.levelBombPair : 1;
        int totalBombCards = Mathf.Min(bombPairs * 2, totalCards - 2);
        int totalSigilCards = totalCards - totalBombCards;
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

    private void AdjustGridLayout(int totalCards)
    {
        if (gridLayout == null) return;

        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;

        if (totalCards <= 8)
        {
            gridLayout.constraintCount = 4;
            gridLayout.cellSize = defaultCardSize;
            gridLayout.spacing = defaultSpacing;
        }
        else if (totalCards <= 12)
        {
            gridLayout.constraintCount = 4;
            gridLayout.cellSize = defaultCardSize;
            gridLayout.spacing = defaultSpacing;
        }
        else if (totalCards <= 16)
        {
            gridLayout.constraintCount = 4;
            gridLayout.cellSize = defaultCardSize * 0.80f;
            gridLayout.spacing = defaultSpacing * 0.80f;
        }
        else if (totalCards <= 20)
        {
            gridLayout.constraintCount = 5;
            gridLayout.cellSize = defaultCardSize * 0.75f;
            gridLayout.spacing = defaultSpacing * 0.75f;
        }
        else
        {
            gridLayout.constraintCount = 6;
            gridLayout.cellSize = defaultCardSize * 0.65f;
            gridLayout.spacing = defaultSpacing * 0.65f;
        }
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

            if (deckSpawnPoint != null)
            {
                cardComp.PrepareForDeal();
            }

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

    private IEnumerator DealCardsRoutine()
    {
        // Kunci seluruh klik kartu di MatchManager selama proses deal!
        if (MatchManager.Instance != null)
        {
            MatchManager.Instance.SetInputLocked(true);
        }

        // Tunggu sampai seluruh Canvas & GridLayoutGroup selesai menghitung posisi world tiap kartu
        Canvas.ForceUpdateCanvases();
        yield return new WaitForEndOfFrame();

        float singleCardTravelTime = 0.38f;
        Vector3 spawnWorldPos = deckSpawnPoint.position;

        for (int i = 0; i < activeCard.Count; i++)
        {
            if (activeCard[i] != null)
            {
                activeCard[i].PlayDealAnimation(spawnWorldPos, i * dealDelay);
            }
        }

        // Tunggu hingga kartu terakhir mendarat di slotnya
        float totalDealDuration = ((activeCard.Count - 1) * dealDelay) + singleCardTravelTime;
        yield return new WaitForSeconds(totalDealDuration);

        // Buka kunci input setelah semua kartu mendarat
        if (MatchManager.Instance != null)
        {
            MatchManager.Instance.SetInputLocked(false);
        }

        dealBoardCoroutine = null;

        // Jalankan Eye of Horus setelah semua kartu selesai di-deal agar tidak ketimpa flip/deal
        if (GameManager.Instance != null && GameManager.Instance.CurrentRun != null &&
            GameManager.Instance.CurrentRun.HasSeal(WaxSealType.EyeOfHorus))
        {
            StartCoroutine(EyeOfHorusPeek(0.15f));
        }
    }

    private IEnumerator EyeOfHorusPeek(float initialDelay)
    {
        yield return new WaitForSeconds(initialDelay);
        PeekRandomCards(2, 1.0f);
    }

    public void PeekRandomCards(int count, float duration)
    {
        if (activeCard == null || activeCard.Count == 0) return;

        List<Card> closedCards = new List<Card>();
        foreach (var card in activeCard)
        {
            if (card != null && !card.IsFlipped && !card.IsMatched && !card.IsMarkedBomb)
            {
                closedCards.Add(card);
            }
        }

        int actualCount = Mathf.Min(count, activeCard.Count);
        for (int i = 0; i < actualCount; i++)
        {
            int randIdx = Random.Range(0, activeCard.Count);
            activeCard[randIdx].Peek(duration);
            activeCard.RemoveAt(randIdx);
        }
    }

    public void MarkOrDisarmBomb()
    {
        if (activeCard == null || activeCard.Count == 0) return;

        List<Card> hiddenBombs = new List<Card>();
        foreach (var card in activeCard)
        {
            if (card != null && card.CardData.cardType == CardType.Bomb && !card.IsFlipped && !card.IsMarkedBomb)
            {
                hiddenBombs.Add(card);
            }
        }

        if (hiddenBombs.Count > 0)
        {
            int randIdx = Random.Range(0, hiddenBombs.Count);
            hiddenBombs[randIdx].MarkAsBomb();
        }
    }
}