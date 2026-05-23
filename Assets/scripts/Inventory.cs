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

    public List<ItemData> GetItems() {
        return items;
    }
}