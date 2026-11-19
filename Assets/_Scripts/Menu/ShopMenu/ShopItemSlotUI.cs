using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ShopItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Visual References")]
    [SerializeField] private Image slotIcon;
    [SerializeField] private TMP_Text slotPrice;
    [SerializeField] private Button buyButton;
    [SerializeField] private GameObject soldOverlay;

    public ShopItemDataSO CurrentItem { get; private set; }
    private System.Action<ShopItemSlotUI, ShopItemDataSO> onPurchaseCallback;

    public void Setup(ShopItemDataSO item, System.Action<ShopItemSlotUI, ShopItemDataSO> onBuy)
    {
        CurrentItem = item;
        onPurchaseCallback = onBuy;

        if (slotIcon != null) slotIcon.sprite = item.icon;
        if (slotPrice != null) slotPrice.text = $"${item.basePrice.ToString()}";
        if (soldOverlay != null) soldOverlay.SetActive(false);
        if (buyButton != null) buyButton.interactable = true;

        if (GameManager.Instance?.CurrentRun != null) RefreshAffordable(GameManager.Instance.CurrentRun.currentTokens);
    }

    public void OnBuyButtonClicked()
    {
        if (CurrentItem != null) onPurchaseCallback?.Invoke(this, CurrentItem);
    }

    public void RefreshAffordable(int currentToken)
    {
        if (CurrentItem == null || buyButton == null || !buyButton.interactable) return;

        if (slotPrice != null) buyButton.interactable = (currentToken >= CurrentItem.basePrice) ? true : false;
    }

    public void MarkSold()
    {
        if (buyButton != null) buyButton.interactable = false;
        if (slotPrice != null)
        {
            slotPrice.text = "SOLD";
            slotPrice.color = Color.gray;
        }
        if (soldOverlay != null) soldOverlay.SetActive(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CurrentItem != null) GameEventHandler.OnItemTooltipShow?.Invoke(CurrentItem);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameEventHandler.OnItemTooltipHide?.Invoke();
    }
}