using System.Collections.Generic;
using UnityEngine;

public class InventoryHotbarUI : MonoBehaviour
{
    [SerializeField] private List<PassiveSlotUI> passiveSlots;      
    [SerializeField] private List<ConsumableSlotUI> consumableSlots; 

    private void OnEnable() => GameEventHandler.OnInventoryUpdated += RefreshHotbar;
    private void OnDisable() => GameEventHandler.OnInventoryUpdated -= RefreshHotbar;

    private void Start() => RefreshHotbar();

    public void RefreshHotbar()
    {
        if (GameManager.Instance?.CurrentRun == null) return;
        GameRunStateData run = GameManager.Instance.CurrentRun;

        for (int i = 0; i < passiveSlots.Count; i++)
        {
            ShopItemDataSO item = (i < run.ownedPassives.Count) ? run.ownedPassives[i] : null;
            passiveSlots[i].SetItem(item, i);
        }

        for (int i = 0; i < consumableSlots.Count; i++)
        {
            ShopItemDataSO item = (i < run.ownedConsumables.Count) ? run.ownedConsumables[i] : null;
            consumableSlots[i].SetItem(item, i);
        }
    }
}