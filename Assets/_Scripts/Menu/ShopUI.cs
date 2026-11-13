using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopUI : MonoBehaviour
{
    [Header("Currency")]
    [SerializeField] private TMP_Text tokenText;
    [SerializeField] private List<GameObject> lifeIcons;
    [SerializeField] private TMP_Text stageText;

    [Header("Heal Button")]
    [SerializeField] private Button healButton;
    [SerializeField] private TMP_Text healPriceText;

    [Header("Grimoire / Item Test Buttons (2 Grimoires)")]
    [SerializeField] private List<Button> grimoireButtons;
    [SerializeField] private List<TMP_Text> grimoireLabels;

    [Header("Next Stage Option Buttons (Max 2)")]
    [SerializeField] private List<Button> stageChoiceButtons;
    [SerializeField] private List<TMP_Text> stageChoiceLabels;

    [Header("Right Panel - Current Sigil Values")]
    [SerializeField] private CardDataSO triquetraData;
    [SerializeField] private CardDataSO crescentData;
    [SerializeField] private CardDataSO ankhData;
    [SerializeField] private CardDataSO lotusData;

    [SerializeField] private TMP_Text triquetraValueText;
    [SerializeField] private TMP_Text crescentValueText;
    [SerializeField] private TMP_Text ankhValueText;
    [SerializeField] private TMP_Text lotusValueText;

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
        GameRunStateData run = GameManager.Instance.CurrentRun;
        UpdateTokenUI(run.currentTokens);
        UpdateHPUI(run.currentHP);
        if (stageText != null) stageText.text = $"Stage {run.currentStage} / 6";

        RefreshSigilValues();
        RefreshHealButton();
        healButton.onClick.AddListener(OnHealClicked);

        var grimoires = ShopManager.Instance.CurrentGrimoireStock;
        for (int i = 0; i < grimoireButtons.Count; i++)
        {
            if (i < grimoires.Count)
            {
                int index = i;
                ShopItemDataSO item = grimoires[index];
                grimoireLabels[i].text = $"{item.itemName}\n+{item.scoreBonusValue} Base\n({item.basePrice} Token)";
                grimoireButtons[i].onClick.AddListener(() => OnBuyGrimoire(index));
            }
            else grimoireButtons[i].gameObject.SetActive(false);
        }

        var choices = ShopManager.Instance.CurrentLevelChoices;
        for (int i = 0; i < stageChoiceButtons.Count; i++)
        {
            if (i < choices.Count)
            {
                int choiceIdx = i;
                LevelDataSO lvl = choices[choiceIdx];
                stageChoiceLabels[i].text = $"{lvl.levelName}\nTarget: {lvl.levelTargetScore} | Turn: {lvl.levelMaxTurn} | Cluster: {lvl.levelMaxCluster}";
                stageChoiceButtons[i].onClick.AddListener(() => ShopManager.Instance.SelectNextLevel(choiceIdx));
            }
            else stageChoiceButtons[i].gameObject.SetActive(false);
        }
    }

    private void OnHealClicked()
    {
        if (ShopManager.Instance.BuyHeal())
        {
            RefreshHealButton();
        }
    }

    private void RefreshHealButton()
    {
        int cost = ShopManager.Instance.GetHealPrice();
        healPriceText.text = $"Heal +1 HP ({cost} Token)";
        healButton.interactable = GameManager.Instance.CurrentRun.currentHP < GameManager.Instance.CurrentRun.maxHP;
    }

    private void RefreshSigilValues()
    {
        GameRunStateData run = GameManager.Instance.CurrentRun;

        triquetraValueText.text = (triquetraData.cardBaseScore + run.GetSymbolBonus(CardType.Triquetra)).ToString();
        crescentValueText.text = (crescentData.cardBaseScore + run.GetSymbolBonus(CardType.Crescent)).ToString();
        ankhValueText.text = (ankhData.cardBaseScore + run.GetSymbolBonus(CardType.Ankh)).ToString();
        lotusValueText.text = (lotusData.cardBaseScore + run.GetSymbolBonus(CardType.Lotus)).ToString();
    }

    private void OnBuyGrimoire(int index)
    {
        ShopItemDataSO item = ShopManager.Instance.CurrentGrimoireStock[index];
        if (ShopManager.Instance.BuyShopItem(item))
        {
            grimoireButtons[index].interactable = false;
            grimoireLabels[index].text = "SOLD";
            RefreshSigilValues();
        }
    }

    private void UpdateTokenUI(int tokens) => tokenText.text = tokens.ToString();

    private void UpdateHPUI(int hp)
    {
        for (int i = 0; i < lifeIcons.Count; i++) lifeIcons[i].SetActive(i < hp);
    }
}
