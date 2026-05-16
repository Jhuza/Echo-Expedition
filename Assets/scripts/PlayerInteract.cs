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

    private void OnGUI() {
        if (currentInteractable != null) {
            GUIStyle style = new GUIStyle();
            style.fontSize = Mathf.RoundToInt(Screen.height * 0.03f);
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;

            GUI.Label(
                new Rect(Screen.width / 2 - Screen.width * 0.15f, Screen.height * 0.6f, Screen.width * 0.3f, Screen.height * 0.05f),
                currentInteractable.GetPromptText(),
                style
            );
        }
    }
}