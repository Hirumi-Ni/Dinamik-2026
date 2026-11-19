using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class SigilToggleSlot
{
    public CardType sigilType;
    public GameObject sigilObject;
}

[System.Serializable]
public class LevelChoiceCardUI
{
    [Header("Card Root")]
    public GameObject cardRoot;

    [Header("Sigil GameObjects (4 Types)")]
    public List<SigilToggleSlot> sigilSlots; // Cukup aktifkan/nonaktifkan sesuai tipe sigil di level

    [Header("Stats Texts")]
    public TMP_Text sigilCountText;  // Format: "12x"
    public TMP_Text bombCountText;   // Format: "2x"
    public TMP_Text targetScoreText; // Format: "1000"
    public TMP_Text turnCountText;   // Format: "12 Turns"

    [Header("Action")]
    public Button continueButton;
}

public class LevelSelectUI : MonoBehaviour
{
    [Header("Next Stage Option Cards")]
    [SerializeField] private List<LevelChoiceCardUI> stageChoiceCards;

    private void Start()
    {
        RefreshLevelChoices();
    }

    public void RefreshLevelChoices()
    {
        if (ShopManager.Instance == null) return;

        var choices = ShopManager.Instance.CurrentLevelChoices;

        for (int i = 0; i < stageChoiceCards.Count; i++)
        {
            LevelChoiceCardUI cardUI = stageChoiceCards[i];

            if (i < choices.Count)
            {
                LevelDataSO lvl = choices[i];
                if (cardUI.cardRoot != null) cardUI.cardRoot.SetActive(true);

                int totalCards = (lvl.levelTotalCards % 2 != 0) ? lvl.levelTotalCards + 1 : lvl.levelTotalCards;
                int totalBombCards = Mathf.Min(lvl.levelBombPair * 2, totalCards - 2);
                int totalSigilCards = totalCards - totalBombCards;

                if (cardUI.sigilCountText != null)
                    cardUI.sigilCountText.text = $"{totalSigilCards}x";

                if (cardUI.bombCountText != null)
                    cardUI.bombCountText.text = $"{totalBombCards}x";

                if (cardUI.targetScoreText != null)
                    cardUI.targetScoreText.text = lvl.levelTargetScore.ToString();

                if (cardUI.turnCountText != null)
                    cardUI.turnCountText.text = $"{lvl.levelMaxTurn} Turns";

                for (int j = 0; j < cardUI.sigilSlots.Count; j++)
                {
                    SigilToggleSlot slot = cardUI.sigilSlots[j];
                    if (slot.sigilObject == null) continue;

                    bool hasThisSigil = LevelContainsSigil(lvl, slot.sigilType);
                    slot.sigilObject.SetActive(hasThisSigil);
                }

                if (cardUI.continueButton != null)
                {
                    int choiceIndex = i;
                    cardUI.continueButton.onClick.RemoveAllListeners();
                    cardUI.continueButton.onClick.AddListener(() => OnSelectLevel(choiceIndex));
                }
            }
            else
            {
                if (cardUI.cardRoot != null) cardUI.cardRoot.SetActive(false);
            }
        }
    }

    private bool LevelContainsSigil(LevelDataSO lvl, CardType targetType)
    {
        if (lvl == null || lvl.levelSymbols == null) return false;

        for (int i = 0; i < lvl.levelSymbols.Count; i++)
        {
            if (lvl.levelSymbols[i] != null && lvl.levelSymbols[i].cardType == targetType)
                return true;
        }
        return false;
    }

    public void OnSelectLevel(int choiceIndex)
    {
        if (ShopManager.Instance == null) return;

        GameEventHandler.OnCloseAllSellPopups?.Invoke();
        ShopManager.Instance.SelectNextLevel(choiceIndex);
    }
}