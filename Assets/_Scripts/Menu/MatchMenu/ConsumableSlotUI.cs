public class ConsumableSlotUI : BaseInventorySlotUI
{
    protected override void OnMatchClick()
    {
        GameManager.Instance.UseConsumable(slotIndex);
        GameEventHandler.OnItemTooltipHide?.Invoke();
    }

    protected override void ExecuteSell(ShopItemDataSO item)
    {
        GameManager.Instance.SellOwnedItem(item, true);
    }
}