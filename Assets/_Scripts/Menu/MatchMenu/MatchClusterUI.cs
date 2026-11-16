using UnityEngine;
using TMPro;

public class MatchClusterUI : MonoBehaviour
{
    [SerializeField] private TMP_Text clusterText;

    private void OnEnable() => GameEventHandler.OnClusterUpdated += HandleClusterUpdated;
    private void OnDisable() => GameEventHandler.OnClusterUpdated -= HandleClusterUpdated;

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentLevelData != null)
        {
            HandleClusterUpdated(GameManager.Instance.CurrentLevelCluster, GameManager.Instance.CurrentLevelData.levelMaxCluster);
        }
    }

    private void HandleClusterUpdated(int currentCluster, int maxCluster)
    {
        if (clusterText != null) clusterText.text = $"{currentCluster}/{maxCluster}";
    }

    // PUBLIC: Assign ke PauseButton.OnClick() di Inspector
    public void OnPauseClicked()
    {
        // Panggil PauseMenuUI atau Time.timeScale = 0f di sini
    }
}