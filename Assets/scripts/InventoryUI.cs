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
        GUIStyle labelStyle = new GUIStyle();
        labelStyle.fontSize = Mathf.RoundToInt(Screen.height * 0.025f);
        labelStyle.normal.textColor = Color.white;

        // Indicador siempre visible
        GUI.Label(
            new Rect(Screen.width * 0.02f, Screen.height * 0.02f, Screen.width * 0.2f, Screen.height * 0.05f),
            isOpen ? "[I] Cerrar inventario" : "[I] Ver materiales",
            labelStyle
        );

        if (!isOpen) return;

        float boxW = Screen.width * 0.25f;
        float boxH = Screen.height * 0.5f;
        float boxX = Screen.width / 2 - boxW / 2;
        float boxY = Screen.height * 0.1f;

        GUI.Box(new Rect(boxX, boxY, boxW, boxH), "Materiales recogidos");

        List<ItemData> items = inventory.GetItems();

        if (items.Count == 0) {
            GUI.Label(
                new Rect(boxX + 10, boxY + 30, boxW - 20, Screen.height * 0.04f),
                "No tienes materiales aún",
                labelStyle
            );
            return;
        }

        for (int i = 0; i < items.Count; i++) {
            GUI.Label(
                new Rect(boxX + 10, boxY + 30 + (i * Screen.height * 0.04f), boxW - 20, Screen.height * 0.04f),
                $"• {items[i].itemName}",
                labelStyle
            );
        }
    }
}