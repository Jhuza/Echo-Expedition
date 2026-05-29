using UnityEngine;

public class PlayerAttack : MonoBehaviour {

    [SerializeField] private float damage = 25f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private LayerMask enemyLayer;

    private float lastAttackTime;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.F) && Time.time >= lastAttackTime + attackCooldown) {
            Attack();
        }
    }

    private void Attack() {
        lastAttackTime = Time.time;

        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);
        Debug.Log("Colliders detectados: " + hits.Length);

        foreach (Collider hit in hits) {
            Debug.Log("Hit: " + hit.gameObject.name);
            Health health = hit.GetComponentInParent<Health>();
            Debug.Log("Health encontrado: " + health);
            if (health != null) {
                health.TakeDamage(damage);
            }
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}