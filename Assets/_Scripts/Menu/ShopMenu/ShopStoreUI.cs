using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopStoreUI : MonoBehaviour
{
    [Header("Heal")]
    [SerializeField] private Button healButton;
    [SerializeField] private TMP_Text healPriceText;

    [Header("Shop Shelves")]
    [SerializeField] private List<ShopItemSlotUI> waxSealSlots; // 3 slot
    [SerializeField] private List<ShopItemSlotUI> grimoireSlots; // 2 slot
    [SerializeField] private List<ShopItemSlotUI> consumableSlots; // 2 slot

    private void OnEnable()
    {
        GameEventHandler.OnTokenChanged += HandleTokenChanged;
        GameEventHandler.OnHPChanged += HandleHPChanged;
    }

    private void OnDisable()
    {
        GameEventHandler.OnTokenChanged -= HandleTokenChanged;
        GameEventHandler.OnHPChanged -= HandleHPChanged;
    }

    private void Start()
    {
        PopulateShelf(waxSealSlots, ShopManager.Instance.CurrentPassiveStock);
        PopulateShelf(grimoireSlots, ShopManager.Instance.CurrentGrimoireStock);
        PopulateShelf(consumableSlots, ShopManager.Instance.CurrentConsumableStock);

        RefreshHealButton();
    }

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
            GameEventHandler.OnSigilStatsUpdated?.Invoke();
            RefreshHealButton();
        }
    }

    public void OnHealClicked()
    {
        if (ShopManager.Instance != null && ShopManager.Instance.BuyHeal())
        {
            RefreshHealButton();
        }
    }

    private void RefreshHealButton()
    {
        if (GameManager.Instance?.CurrentRun == null) return;

        GameRunStateData run = GameManager.Instance.CurrentRun;
        int cost = ShopManager.Instance.GetHealPrice();

        if (healPriceText != null) healPriceText.text = $"${cost}";

        healButton.interactable = (run.currentHP < run.GetEffectiveMaxHP()) && (run.currentTokens >= cost);
    }

    private void HandleTokenChanged(int newTokens)
    {
        RefreshHealButton();
        RefreshShelfAffordability(waxSealSlots, newTokens);
        RefreshShelfAffordability(grimoireSlots, newTokens);
        RefreshShelfAffordability(consumableSlots, newTokens);
    }

    private void HandleHPChanged(int _) => RefreshHealButton();

    private void RefreshShelfAffordability(List<ShopItemSlotUI> slots, int tokens)
    {
        foreach (var slot in slots)
        {
            if (slot != null && slot.gameObject.activeSelf) slot.RefreshAffordable(tokens);
        }
    }
}