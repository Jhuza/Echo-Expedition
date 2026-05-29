using UnityEngine;

public class ChestInteractable : MonoBehaviour, Interactable {

    [SerializeField] private ItemData llaveRequerida;
    [SerializeField] private ItemData contenido; // ItemData que da el cofre al abrirse

    private bool abierto = false;

    public void Interact() {
        if (abierto) return;

        ItemData llave = BuscarEnInventario(llaveRequerida);

        if (llave == null) {
            Debug.Log("Necesitas la llave para abrir este cofre");
            return;
        }

        Inventory.Instance.GetItems().Remove(llave);
        abierto = true;

        // Agrega el contenido directamente al inventario
        if (contenido != null) {
            Inventory.Instance.AddItem(contenido); // ya refresca HUD
            Debug.Log($"¡Cofre abierto! Obtuviste: {contenido.itemName}");
        } else {
            HUDManager.Instance?.RefreshInventory(); // por el Remove de la llave
            Debug.Log("¡Cofre abierto!");
        }
    }

    public string GetPromptText() {
        if (abierto) return "Cofre vacío";

        ItemData llave = BuscarEnInventario(llaveRequerida);
        if (llave != null)
            return "[E] Abrir cofre";
        else
            return $"[E] Necesitas: {llaveRequerida.itemName}";
    }

    ItemData BuscarEnInventario(ItemData itemBuscado) {
        foreach (ItemData item in Inventory.Instance.GetItems()) {
            if (item == itemBuscado) return item;
        }
        return null;
    }
}