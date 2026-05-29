using UnityEngine;

public class BombAttack : MonoBehaviour {

    [SerializeField] private float rangoDeteccion = 8f;
    [SerializeField] private float cooldown = 0.5f;
    [SerializeField] private LayerMask capasEnemigo;
    [SerializeField] private GameObject efectoExplosionPrefab;

    private float lastUseTime;

    private void Start() {
        PushBombCountToHUD();
    }

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

        PushBombCountToHUD();
        HUDManager.Instance?.RefreshInventory();

        Debug.Log($"Bomba usada en {enemigoMasCercano.gameObject.name}. Bombas restantes: {Inventory.Instance.ContarBombas()}");
    }

    private void PushBombCountToHUD() {
        if (HUDManager.Instance != null && Inventory.Instance != null) {
            HUDManager.Instance.UpdateBombCount(Inventory.Instance.ContarBombas());
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
    }
}
