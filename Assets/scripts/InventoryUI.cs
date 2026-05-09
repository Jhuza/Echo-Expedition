using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour {

    private Inventory inventory;
    private bool isOpen = false;

    private void Start() {
        inventory = Inventory.Instance;
        Debug.Log("Inventory encontrado: " + inventory);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.I)) {
            isOpen = !isOpen;
        }
    }

    private void OnGUI() {
        if (!isOpen) return;

        // Fondo del inventario
        GUI.Box(new Rect(Screen.width / 2 - 150, 50, 300, 400), "Materiales recogidos");

        List<ItemData> items = inventory.GetItems();

        if (items.Count == 0) {
            GUI.Label(new Rect(Screen.width / 2 - 100, 100, 200, 30), "No tienes materiales aún");
            return;
        }

        for (int i = 0; i < items.Count; i++) {
            GUI.Label(
                new Rect(Screen.width / 2 - 120, 100 + (i * 30), 250, 25),
                $"• {items[i].itemName}"
            );
        }
    }
}