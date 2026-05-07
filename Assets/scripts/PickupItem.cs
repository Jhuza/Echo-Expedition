using UnityEngine;

public class PickupItem : MonoBehaviour, Interactable {

    [SerializeField] private string itemName = "Item";
    [SerializeField] private Sprite itemIcon;

    public void Interact() {
        //Añadir aquí el sistema de inventario, si es que lo hago
        Debug.Log($"Recogiste: nombre item");
        Destroy(gameObject);
    }

    public string GetPromptText() {
        return $"[E] Recoger {itemName}";
    }
}