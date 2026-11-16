using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public abstract class BaseInventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Visual References")]
    [SerializeField] private Image itemIcon;

    [Header("Shop Sell Popup")]
    [SerializeField] private GameObject sellButtonObj;
    [SerializeField] private TMP_Text sellPriceText;

    protected ShopItemDataSO currentItem;
    protected int slotIndex;

    protected virtual void OnEnable() => GameEventHandler.OnCloseAllSellPopups += HideSellButton;
    protected virtual void OnDisable() => GameEventHandler.OnCloseAllSellPopups -= HideSellButton;

    protected virtual void Start()
    {
        if (sellButtonObj != null) sellButtonObj.SetActive(false);
    }

    public void SetItem(ShopItemDataSO item, int index)
    {
        currentItem = item;
        slotIndex = index;
        HideSellButton();

        if (itemIcon != null)
        {
            itemIcon.enabled = (item != null);
            if (item != null) itemIcon.sprite = item.icon;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem != null) GameEventHandler.OnItemTooltipShow?.Invoke(currentItem);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameEventHandler.OnItemTooltipHide?.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null || GameManager.Instance == null) return;

        if (GameManager.Instance.CurrentState == GameState.Match) OnMatchClick();
        else if (GameManager.Instance.CurrentState == GameState.Shop) ToggleSellPopup();
    }

    protected abstract void OnMatchClick();
    protected abstract void ExecuteSell(ShopItemDataSO item);

    private void ToggleSellPopup()
    {
        if (sellButtonObj == null) return;

        bool willShow = !sellButtonObj.activeSelf;
        GameEventHandler.OnCloseAllSellPopups?.Invoke();

        if (willShow)
        {
            int refund = Mathf.FloorToInt(currentItem.basePrice * 0.5f);
            if (sellPriceText != null) sellPriceText.text = $"Sell +{refund}";
            sellButtonObj.SetActive(true);
        }
    }

    public void OnSellButtonClicked()
    {
        if (currentItem == null) return;

        ExecuteSell(currentItem);
        GameEventHandler.OnItemTooltipHide?.Invoke();
        HideSellButton();
    }

    public void HideSellButton()
    {
        if (sellButtonObj != null) sellButtonObj.SetActive(false);
    }
}