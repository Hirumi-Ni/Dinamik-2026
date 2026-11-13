using System;
using System.Collections.Generic;

[Serializable]
public class GameRunStateData
{
    #region [Starting Data]
    public int currentHP;
    public int maxHP;
    public int currentTokens;
    public int currentStage = 1;
    public int healPurchaseCount = 0;
    #endregion

    #region [Permanent Card Upgrade Value]
    public int triquetraBonus = 0;
    public int crescentBonus = 0;
    public int ankhBonus = 0;
    public int lotusBonus = 0;
    #endregion

    #region [Item Inventory]
    public List<ShopItemDataSO> ownedPassives = new List<ShopItemDataSO>();
    public List<ShopItemDataSO> ownedConsumables = new List<ShopItemDataSO>();
    #endregion

    #region [Methods & Constructor]
    public GameRunStateData(int startingMaxHP = 3)
    {
        maxHP = startingMaxHP;
        currentHP = startingMaxHP;
        currentTokens = 0;
        currentStage = 1;
        healPurchaseCount = 0;
    }

    public void AddSymbolBonus(GrimoireTarget target, int amount)
    {
        switch (target)
        {
            case GrimoireTarget.Triquetra: 
                triquetraBonus += amount; 
                break;
            case GrimoireTarget.Crescent: 
                crescentBonus += amount; 
                break;
            case GrimoireTarget.Ankh: 
                ankhBonus += amount; 
                break;
            case GrimoireTarget.Lotus: 
                lotusBonus += amount; 
                break;
            case GrimoireTarget.Covenant:
                triquetraBonus += amount;
                crescentBonus += amount;
                ankhBonus += amount;
                lotusBonus += amount;
                break;
            default: break;
        }
    }

    public int GetSymbolBonus(CardType type)
    {
        switch (type)
        {
            case CardType.Triquetra: return triquetraBonus;
            case CardType.Crescent: return crescentBonus;
            case CardType.Ankh: return ankhBonus;
            case CardType.Lotus: return lotusBonus;
            default: return 0;
        }
    }
    #endregion
}