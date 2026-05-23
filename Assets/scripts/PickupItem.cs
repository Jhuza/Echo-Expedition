using UnityEngine;

public class PickupItem : MonoBehaviour, Interactable {

    [SerializeField] private ItemData itemData; // <- eso reemplazara el nombre y la información por lo que sea que tenga el objeto que pongamos (ItemName y Icon)

    public void Interact() {
        Inventory.Instance.AddItem(itemData);
        Debug.Log($"Recogiste: {itemData.itemName}");
        Destroy(gameObject);
    }

    public string GetPromptText() {
        return $"[E] Recoger {itemData.itemName}";
    }
}