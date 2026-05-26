using UnityEngine;

public class PickupItem : MonoBehaviour, Interactable {

    [SerializeField] private ItemData itemData;
    private bool recogido = false;

    public void Interact() {
        if (recogido) return;
        recogido = true;

        Inventory.Instance.AddItem(itemData);
        Debug.Log($"Recogiste: {itemData.itemName}");
        Destroy(gameObject);
    }

    public string GetPromptText() {
        return $"[E] Recoger {itemData.itemName}";
    }
}