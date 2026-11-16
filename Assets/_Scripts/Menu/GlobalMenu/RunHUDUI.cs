using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RunHUDUI : MonoBehaviour
{
    [SerializeField] private TMP_Text tokenText;
    [SerializeField] private List<GameObject> lifeIcons;

    private void OnEnable()
    {
        GameEventHandler.OnTokenChanged += UpdateTokenUI;
        GameEventHandler.OnHPChanged += UpdateHPUI;
    }

    private void OnDisable()
    {
        GameEventHandler.OnTokenChanged -= UpdateTokenUI;
        GameEventHandler.OnHPChanged -= UpdateHPUI;
    }

    private void Start()
    {
        if (GameManager.Instance?.CurrentRun == null) return;
        GameRunStateData run = GameManager.Instance.CurrentRun;

        UpdateTokenUI(run.currentTokens);
        UpdateHPUI(run.currentHP);
    }

    private void UpdateTokenUI(int tokens)
    {
        if (tokenText != null) tokenText.text = tokens.ToString();
    }

    private void UpdateHPUI(int hp)
    {
        for (int i = 0; i < lifeIcons.Count; i++)
        {
            if (lifeIcons[i] != null) lifeIcons[i].SetActive(i < hp);
        }
    }
}