using UnityEngine;

public class PlayerInteract : MonoBehaviour {

    [SerializeField] private float interactRange = 2f;
    [SerializeField] private LayerMask interactLayer;

    private Interactable currentInteractable;

    private void Update() {
        LookForInteractable();

        if (currentInteractable != null) {
            HUDManager.Instance?.ShowPrompt(currentInteractable.GetPromptText());
        } else {
            HUDManager.Instance?.HidePrompt();
        }

        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null) {
            currentInteractable.Interact();
        }
    }

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

    public string GetCurrentPrompt() {
        return currentInteractable != null ? currentInteractable.GetPromptText() : "";
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
