using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour {

    public static Inventory Instance;
    private List<ItemData> items = new List<ItemData>();

    private void Awake() {
        Instance = this;
    }

    public void AddItem(ItemData item) {
        items.Add(item);
        Debug.Log($"Inventario: {items.Count} items");
    }

    public List<ItemData> GetItems() => items;

    public int ContarBombas() {
        int count = 0;
        foreach (var item in items)
            if (item.tipo == ItemType.Bomba) count++;
        return count;
    }

    public ItemData UsarBomba() {
        for (int i = 0; i < items.Count; i++) {
            if (items[i].tipo == ItemType.Bomba) {
                ItemData bomba = items[i];
                items.RemoveAt(i);
                return bomba;
            }
        }
        return null;
    }
}