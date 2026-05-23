using UnityEngine;

public class DamageNumber : MonoBehaviour {

    private float damage;
    private float lifetime = 1f;
    private float floatSpeed = 1.5f;
    private float timer;

    public void Setup(float damageAmount) {
        damage = damageAmount;
    }

    private void Update() {
        // Flota hacia arriba
        transform.position += transform.up * floatSpeed * Time.deltaTime;

        timer += Time.deltaTime;
        if (timer >= lifetime) {
            Destroy(gameObject);
        }
    }

    private void OnGUI() {
        // Convierte posición 3D a posición en pantalla
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

        if (screenPos.z < 0) return; // Si está detrás de la cámara no lo muestra

        GUIStyle style = new GUIStyle();
        style.fontSize = Mathf.RoundToInt(Screen.height * 0.035f);
        style.normal.textColor = Color.yellow;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;

        // GUI usa Y invertida respecto a la pantalla
        float guiY = Screen.height - screenPos.y;

        GUI.Label(
            new Rect(screenPos.x - 50, guiY - 25, 100, 50),
            $"-{damage}",
            style
        );
    }
}