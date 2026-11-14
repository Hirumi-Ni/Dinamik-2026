using UnityEngine;

[CreateAssetMenu(fileName = "NewShopItemData", menuName = "Game Data/Shop Item Data")]
public class ShopItemDataSO : ScriptableObject
{
    [Header("Item Identity")]
    public string itemName;
    public string description;
    public Sprite icon;
    public ShopItemType type;
    public ItemRarity rarity = ItemRarity.Dusty;

    [Header("Economy")]
    public int basePrice = 40;

    [Header("Permanent Upgrade Config")]
    public GrimoireTarget grimoireTarget = GrimoireTarget.None;
    public int scoreBonusValue = 20;

    [Header("Passive Upgrade Config")]
    public WaxSealType sealType = WaxSealType.None;
    public float effectValue = 0f;
}
