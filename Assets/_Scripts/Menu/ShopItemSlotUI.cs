using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ShopItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Slot Type")]
    [SerializeField] private bool isWaxSeal = false;

    [Header("Visual References")]
    [SerializeField] private Image slotIcon;
    [SerializeField] private TMP_Text slotPrice;
    [SerializeField] private Button buyButton;
    [SerializeField] private GameObject soldOverlay;

    private ShopItemDataSO currentItem;
    private System.Action<ShopItemSlotUI, ShopItemDataSO> onPurchaseCallback;

    public void Setup(ShopItemDataSO item, System.Action<ShopItemSlotUI, ShopItemDataSO> onBuy)
    {
        currentItem = item;
        onPurchaseCallback = onBuy;

        slotIcon.sprite = item.icon;
        slotPrice.text = item.basePrice.ToString();
        if (soldOverlay != null) soldOverlay.SetActive(false);

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => onPurchaseCallback?.Invoke(this, currentItem));

        RefreshAffordable(GameManager.Instance.CurrentRun.currentTokens);
    }

    public void RefreshAffordable(int currentToken)
    {
        if (currentItem == null || !buyButton.interactable) return;

        slotPrice.color = (currentToken >= currentItem.basePrice) ? Color.white : Color.red;
    }

    public void MarkSold()
    {
        buyButton.interactable = false;
        slotPrice.text = "SOLD";
        if (soldOverlay != null) soldOverlay.SetActive(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem != null) ShopUI.Instance.ShowTooltip(currentItem);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ShopUI.Instance.HideTooltip();
    }
}
