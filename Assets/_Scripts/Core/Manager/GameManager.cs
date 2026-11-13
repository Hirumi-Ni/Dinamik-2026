using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    #region [gk tau apa ini refactor belakangan dah]
    [SerializeField] private int maxHP = 3;
    private int currentHP;
    public GameState CurrentState { get; private set; }
    #endregion

    #region [Level Data]
    public LevelDataSO CurrentLevelData { get; private set; }
    public int CurrentLevelStage { get; private set; } = 1;
    public int CurrentLevelCluster { get; private set; } = 1;
    [SerializeField] private LevelDataSO startingLevelData;
    #endregion

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        GameEventHandler.OnBombTriggered += TakeDamage;
    }

    private void OnDisable()
    {
        GameEventHandler.OnBombTriggered -= TakeDamage;
    }

    private void Start()
    {
        StartNewRun();
    }

    public void StartNewRun()
    {
        currentHP = maxHP;
        CurrentLevelStage = 1;
        GameEventHandler.OnHPChanged?.Invoke(currentHP);

        StartLevel(startingLevelData);
    }

    public void StartLevel(LevelDataSO selectedLevel)
    {
        SetState(GameState.Match);
        CurrentLevelData = selectedLevel;
        CurrentLevelCluster = 1;
        
        ScoreManager.Instance.ResetScoreForNewLevel(CurrentLevelData.levelTargetScore, CurrentLevelData.levelMaxTurn);
        BoardManager.Instance.GenerateBoard();
        GameEventHandler.OnClusterUpdated?.Invoke(CurrentLevelCluster, CurrentLevelData.levelMaxCluster);
    }

    public void TakeDamage()
    {
        currentHP--;
        Debug.Log($"Current HP: {currentHP}");
        GameEventHandler.OnHPChanged?.Invoke(currentHP);

        if (currentHP <= 0)
        {
            SetState(GameState.GameOver);
            Debug.Log("YOU LOSE!!!! Nyawanya habis");
        }
    }

    public void EvaluateTurnLossCondition()
    {
        if (ScoreManager.Instance.remainingTurn <= 0 && !ScoreManager.Instance.IsTargetReached())
        {
            SetState(GameState.GameOver);
            Debug.Log("YOU LOSE!!! Turnnya habis dan belum menyentuh skor");
        }
    }

    public void HandleAllSafeCardsCleared()
    {
        if (CurrentState != GameState.Match) return;

        int maxCluster = (CurrentLevelData != null) ? CurrentLevelData.levelMaxCluster : 1;

        if (CurrentLevelCluster < maxCluster)
        {
            Debug.Log("Transisi to Next Cluster");
            StartCoroutine(TransitionToNextCluster(maxCluster));
        }
        else
        {
            if (ScoreManager.Instance.IsTargetReached())
            {
                Debug.Log("Semua kartu aman terbuka & Target tercapai! Otomatis lanjut ke Shop.");
                //CompleteMatch();
            }
            else
            {
                Debug.Log("Semua kartu aman habis terbuka, tapi skor tidak mencukupi target! Game Over.");
                //SetState(GameState.GameOver);
            }
        }
    }

    private IEnumerator TransitionToNextCluster(int maxCluster)
    {
        MatchManager.Instance.SetInputLocked(true);
        yield return new WaitForSeconds(0.85f);

        if (CurrentState != GameState.Match) yield break;

        CurrentLevelCluster++;
        GameEventHandler.OnClusterUpdated?.Invoke(CurrentLevelCluster, maxCluster);
        BoardManager.Instance.GenerateBoard();
    }

    public void CompleteMatch()
    {
        if (CurrentState != GameState.Match) return;
        if (!ScoreManager.Instance.IsTargetReached()) return;

        CurrentLevelStage++;
        SetState(GameState.Shop);
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        GameEventHandler.OnGameStateChanged?.Invoke(newState);
    }
}