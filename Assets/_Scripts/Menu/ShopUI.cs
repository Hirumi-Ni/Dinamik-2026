using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance { get; private set; }

    [Header("Left Panel - HUD & Tooltip")]
    [SerializeField] private TMP_Text tokenText;
    [SerializeField] private List<GameObject> lifeIcons;
    [SerializeField] private TMP_Text stageText;

    [Space(5)]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TMP_Text tooltipNameText;
    [SerializeField] private TMP_Text tooltipCategoryText;
    [SerializeField] private TMP_Text tooltipDescText;

    [Header("Center Panel - Heal Button")]
    [SerializeField] private Button healButton;
    [SerializeField] private TMP_Text healPriceText;

    [Header("Center Panel - Shop Item Slots (Prefab Instances)")]
    [SerializeField] private List<ShopItemSlotUI> waxSealSlots;     
    [SerializeField] private List<ShopItemSlotUI> grimoireSlots;  
    [SerializeField] private List<ShopItemSlotUI> consumableSlots;  

    [Header("Center Panel - Next Stage Options (Max 2)")]
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

    [Header("Right Panel - Owned Inventory Icons")]
    [SerializeField] private List<Image> ownedPassiveIcons;     
    [SerializeField] private List<Image> ownedConsumableIcons;  

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        GameEventHandler.OnTokenChanged += HandleTokenChanged;
        GameEventHandler.OnHPChanged += UpdateHPUI;
        GameEventHandler.OnInventoryUpdated += RefreshInventoryUI;
    }

    private void OnDisable()
    {
        GameEventHandler.OnTokenChanged -= HandleTokenChanged;
        GameEventHandler.OnHPChanged -= UpdateHPUI;
        GameEventHandler.OnInventoryUpdated -= RefreshInventoryUI;
    }

    private void Start()
    {
        HideTooltip();

        GameRunStateData run = GameManager.Instance.CurrentRun;
        UpdateTokenUI(run.currentTokens);
        UpdateHPUI(run.currentHP);
        if (stageText != null) stageText.text = $"Stage {run.currentStage} / 6";

        RefreshSigilValues();
        RefreshInventoryUI();
        RefreshHealButton();
        healButton.onClick.AddListener(OnHealClicked);

        PopulateShelf(waxSealSlots, ShopManager.Instance.CurrentPassiveStock);
        PopulateShelf(grimoireSlots, ShopManager.Instance.CurrentGrimoireStock);
        PopulateShelf(consumableSlots, ShopManager.Instance.CurrentConsumableStock);

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

    #region [Shop Shelves & Purchases]
    private void PopulateShelf(List<ShopItemSlotUI> slots, List<ShopItemDataSO> stock)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < stock.Count)
            {
                slots[i].gameObject.SetActive(true);
                slots[i].Setup(stock[i], OnItemSlotClicked);
            }
            else
            {
                slots[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnItemSlotClicked(ShopItemSlotUI slotUI, ShopItemDataSO item)
    {
        if (ShopManager.Instance.BuyShopItem(item))
        {
            slotUI.MarkSold();
            RefreshSigilValues();
            RefreshHealButton();
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
        GameRunStateData run = GameManager.Instance.CurrentRun;
        int cost = ShopManager.Instance.GetHealPrice();
        healPriceText.text = $"Heal +1 HP ({cost} Token)";
        healButton.interactable = (run.currentHP < run.GetEffectiveMaxHP()) && (run.currentTokens >= cost);
    }

    #endregion

    #region [Tooltip System]
    public void ShowTooltip(ShopItemDataSO item)
    {
        if (tooltipPanel == null || item == null) return;

        tooltipPanel.SetActive(true);
        tooltipNameText.text = item.itemName;
        tooltipCategoryText.text = $"{item.rarity} • {GetCategoryLabel(item.type)}";

        tooltipDescText.text = (UpgradeManager.Instance != null) ? UpgradeManager.Instance.GetFormattedDescription(item) : item.description;
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    private string GetCategoryLabel(ShopItemType category)
    {
        switch (category)
        {
            case ShopItemType.PassiveBuff: return "Wax Seal (Passive)";
            case ShopItemType.PermanentBuff: return "Grimoire (Permanent)";
            case ShopItemType.Consumable: return "Relic (Consumable)";
            default: return "gay";
        }
    }
    #endregion

    #region [HUD & Right Panel Updates]
    private void HandleTokenChanged(int newTokens)
    {
        UpdateTokenUI(newTokens);
        RefreshHealButton();

        RefreshShelfAffordability(waxSealSlots, newTokens);
        RefreshShelfAffordability(grimoireSlots, newTokens);
        RefreshShelfAffordability(consumableSlots, newTokens);
    }

    private void RefreshShelfAffordability(List<ShopItemSlotUI> slots, int tokens)
    {
        foreach (var slot in slots)
        {
            if (slot != null && slot.gameObject.activeSelf) slot.RefreshAffordable(tokens);
        }
    }

    private void RefreshSigilValues()
    {
        GameRunStateData run = GameManager.Instance.CurrentRun;

        triquetraValueText.text = (triquetraData.cardBaseScore + run.GetSymbolBonus(CardType.Triquetra)).ToString();
        crescentValueText.text = (crescentData.cardBaseScore + run.GetSymbolBonus(CardType.Crescent)).ToString();
        ankhValueText.text = (ankhData.cardBaseScore + run.GetSymbolBonus(CardType.Ankh)).ToString();
        lotusValueText.text = (lotusData.cardBaseScore + run.GetSymbolBonus(CardType.Lotus)).ToString();
    }

    private void RefreshInventoryUI()
    {
        GameRunStateData run = GameManager.Instance.CurrentRun;

        for (int i = 0; i < ownedPassiveIcons.Count; i++)
        {
            if (i < run.ownedPassives.Count)
            {
                ownedPassiveIcons[i].enabled = true;
                ownedPassiveIcons[i].sprite = run.ownedPassives[i].icon;
            }
            else
            {
                ownedPassiveIcons[i].enabled = false;
            }
        }

        for (int i = 0; i < ownedConsumableIcons.Count; i++)
        {
            if (i < run.ownedConsumables.Count)
            {
                ownedConsumableIcons[i].enabled = true;
                ownedConsumableIcons[i].sprite = run.ownedConsumables[i].icon;
            }
            else
            {
                ownedConsumableIcons[i].enabled = false;
            }
        }
    }

    private void UpdateTokenUI(int tokens) => tokenText.text = tokens.ToString();

    private void UpdateHPUI(int hp)
    {
        for (int i = 0; i < lifeIcons.Count; i++) lifeIcons[i].SetActive(i < hp);
    }
    #endregion
}