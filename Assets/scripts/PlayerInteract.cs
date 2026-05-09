using UnityEngine;

public class PlayerInteract : MonoBehaviour {

    [SerializeField] private float interactRange = 2f;
    [SerializeField] private LayerMask interactLayer;

    private Interactable currentInteractable;

    private void Update() {
        LookForInteractable();

        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null) {
            currentInteractable.Interact();
        }
    }

    // Para buscar el más cercano
    private void LookForInteractable() {
        Collider[] colliders = Physics.OverlapSphere(transform.position, interactRange);

        currentInteractable = null;

        float closestDistance = interactRange;
        foreach (Collider col in colliders) {
            if (col.TryGetComponent(out Interactable interactable)) {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < closestDistance) {
                    closestDistance = dist;
                    currentInteractable = interactable;
                }
            }
        }
    }

    // Para que muestre el prompt
    public string GetCurrentPrompt() {
        return currentInteractable != null ? currentInteractable.GetPromptText() : "";
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }


    // Para que el texto salga al acercarse al interactuable (como se diga)
    private void OnGUI() {
         if (currentInteractable != null) {
            GUIStyle style = new GUIStyle();
            style.fontSize = 24;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;

            GUI.Label(
                new Rect(Screen.width / 2 - 150, Screen.height / 2 + 50, 300, 40),
                currentInteractable.GetPromptText(),
                style
            );
        }
    }
}