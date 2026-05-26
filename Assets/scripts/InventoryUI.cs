using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour {

    private Inventory inventory;
    private bool isOpen = false;

    private void Start() {
        inventory = Inventory.Instance;
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

        GUI.Label(
            new Rect(Screen.width * 0.02f, Screen.height * 0.02f, Screen.width * 0.2f, Screen.height * 0.05f),
            isOpen ? "[I] Cerrar inventario" : "[I] Ver inventario",
            labelStyle
        );

        if (!isOpen) return;

        float boxW = Screen.width * 0.25f;
        float boxH = Screen.height * 0.5f;
        float boxX = Screen.width / 2 - boxW / 2;
        float boxY = Screen.height * 0.1f;

        GUI.Box(new Rect(boxX, boxY, boxW, boxH), "Inventario");

        List<ItemData> items = inventory.GetItems();

        if (items.Count == 0) {
            GUI.Label(
                new Rect(boxX + 10, boxY + 30, boxW - 20, Screen.height * 0.04f),
                "No tienes nada aún",
                labelStyle
            );
            return;
        }

        // Agrupa los items por nombre para mostrar cantidad
        Dictionary<string, (ItemData data, int cantidad)> agrupados = new Dictionary<string, (ItemData, int)>();

        foreach (ItemData item in items) {
            if (agrupados.ContainsKey(item.itemName)) {
                agrupados[item.itemName] = (item, agrupados[item.itemName].cantidad + 1);
            } else {
                agrupados[item.itemName] = (item, 1);
            }
        }

        // Estilos por tipo
        GUIStyle materialStyle = new GUIStyle();
        materialStyle.fontSize = Mathf.RoundToInt(Screen.height * 0.025f);
        materialStyle.normal.textColor = Color.white;

        GUIStyle bombaStyle = new GUIStyle();
        bombaStyle.fontSize = Mathf.RoundToInt(Screen.height * 0.025f);
        bombaStyle.normal.textColor = new Color(1f, 0.5f, 0f); // Naranja para bombas

        int i = 0;
        foreach (var kvp in agrupados) {
            ItemData data = kvp.Value.data;
            int cantidad = kvp.Value.cantidad;

            string etiqueta = data.tipo == ItemType.Bomba ? "💣" : "•";
            string texto = cantidad > 1
                ? $"{etiqueta} {data.itemName} x{cantidad}"
                : $"{etiqueta} {data.itemName}";

            GUIStyle estiloActual = data.tipo == ItemType.Bomba ? bombaStyle : materialStyle;

            GUI.Label(
                new Rect(boxX + 10, boxY + 30 + (i * Screen.height * 0.04f), boxW - 20, Screen.height * 0.04f),
                texto,
                estiloActual
            );
            i++;
        }
    }
} 