using UnityEngine;

/// <summary>
/// Stub legacy. La UI del inventario la maneja HUDManager (que ya escucha la
/// tecla I). Este script no hace nada en runtime; existe solo para no romper
/// referencias de componentes en escenas/prefabs antiguos.
/// </summary>
public class InventoryUI : MonoBehaviour {
    // Sin Update: HUDManager.Update() ya captura la tecla y togglea el panel.
    // Si los dos togglearan en el mismo frame, se cancelarían y el panel
    // parecería no abrirse.
}
