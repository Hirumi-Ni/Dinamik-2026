public class PassiveSlotUI : BaseInventorySlotUI
{
    protected override void OnMatchClick() { }

    protected override void ExecuteSell(ShopItemDataSO item)
    {
        GameManager.Instance.SellOwnedItem(item, false);
    }
}