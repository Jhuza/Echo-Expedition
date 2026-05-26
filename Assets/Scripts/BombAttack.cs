using UnityEngine;

public class BombAttack : MonoBehaviour {

    [SerializeField] private float rangoDeteccion = 8f;
    [SerializeField] private float cooldown = 0.5f;
    [SerializeField] private LayerMask capasEnemigo;
    [SerializeField] private GameObject efectoExplosionPrefab;

    private float lastUseTime;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Q) && Time.time >= lastUseTime + cooldown) {
            UsarBomba();
        }
    }

    private void UsarBomba() {
        if (Inventory.Instance.ContarBombas() <= 0) {
            Debug.Log("No tienes bombas");
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, rangoDeteccion, capasEnemigo);

        if (hits.Length == 0) {
            Debug.Log("No hay enemigos cerca");
            return;
        }

        Collider enemigoMasCercano = null;
        float menorDist = Mathf.Infinity;

        foreach (Collider hit in hits) {
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < menorDist) {
                menorDist = dist;
                enemigoMasCercano = hit;
            }
        }

        if (enemigoMasCercano == null) return;

        ItemData bomba = Inventory.Instance.UsarBomba();
        lastUseTime = Time.time;

        if (enemigoMasCercano.TryGetComponent(out Health health)) {
            health.TakeDamage(bomba.damage);
        }

        if (efectoExplosionPrefab != null) {
            Instantiate(efectoExplosionPrefab, enemigoMasCercano.transform.position, Quaternion.identity);
        }

        Debug.Log($"Bomba usada en {enemigoMasCercano.gameObject.name}. Bombas restantes: {Inventory.Instance.ContarBombas()}");
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
    }

    private void OnGUI() {
        GUIStyle style = new GUIStyle();
        style.fontSize = Mathf.RoundToInt(Screen.height * 0.025f);
        style.normal.textColor = Color.white;

        int bombas = Inventory.Instance != null ? Inventory.Instance.ContarBombas() : 0;

        GUI.Label(
            new Rect(Screen.width * 0.02f, Screen.height * 0.22f, Screen.width * 0.2f, Screen.height * 0.05f),
            $"[Q] Bomba ({bombas})",
            style
        );
    }
}