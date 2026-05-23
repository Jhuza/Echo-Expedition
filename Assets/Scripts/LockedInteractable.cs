using UnityEngine;

public class LockedInteractable : MonoBehaviour, Interactable {

    [SerializeField] private int itemsRequeridos = 3;
    [SerializeField] private string mensajeDesbloqueado = "¡Objeto activado!";
    [SerializeField] private string mensajeBloqueado = "Necesitas {0} materiales más";

    public void Interact() {
        int itemsActuales = Inventory.Instance.GetItems().Count;

        if (itemsActuales >= itemsRequeridos) {
            Debug.Log(mensajeDesbloqueado);
            // Aquí luego puedes poner lo que quieras que suceda
        } else {
            int faltantes = itemsRequeridos - itemsActuales;
            Debug.Log(string.Format(mensajeBloqueado, faltantes));
        }
    }

    public string GetPromptText() {
        int itemsActuales = Inventory.Instance.GetItems().Count;
        int faltantes = itemsRequeridos - itemsActuales;

        if (itemsActuales >= itemsRequeridos) {
            return "[E] Activar";
        } else {
            return $"[E] Faltan {faltantes} materiales";
        }
    }
}