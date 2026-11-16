using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "NewShopItemData", menuName = "Game Data/Shop Item Data")]
public class ShopItemDataSO : ScriptableObject
{
    [Header("Item Identity")]
    public string itemName;
    [TextArea]
    public string description;
    public Sprite icon;
    public ShopItemType type;
    public ItemRarity rarity = ItemRarity.Dusty;

    [Header("Economy")]
    public int basePrice = 40;

    [ShowIf("type", ShopItemType.PermanentBuff)]
    [Header("Permanent Upgrade Config")]
    public GrimoireTarget grimoireTarget = GrimoireTarget.None;
    [ShowIf("type", ShopItemType.PermanentBuff)]
    public int scoreBonusValue = 20;

    [ShowIf("type", ShopItemType.PassiveBuff)]
    [Header("Passive Upgrade Config")]
    public WaxSealType sealType = WaxSealType.None;

    [ShowIf("type", ShopItemType.Consumable)]
    [Header("Consumable Config")]
    public ConsumableType consumableType = ConsumableType.None;
}
