using UnityEngine;

public class Health : MonoBehaviour {

    [SerializeField] private float maxHP = 100f;
    [SerializeField] private bool mostrarUI = false;
    [SerializeField] private GameObject damageNumberPrefab;

    private float currentHP;

    private void Start() {
        currentHP = maxHP;
        PushToHUD();
    }

    public void TakeDamage(float amount) {
        currentHP -= amount;
        Debug.Log($"{gameObject.name} recibió {amount} daño. HP: {currentHP}/{maxHP}");

        if (damageNumberPrefab != null) {
            GameObject num = Instantiate(damageNumberPrefab, transform.position + transform.up, transform.rotation);
            num.GetComponent<DamageNumber>().Setup(amount);
        }

        PushToHUD();

        if (currentHP <= 0) {
            Die();
        }
    }

    private void PushToHUD() {
        if (mostrarUI && HUDManager.Instance != null) {
            HUDManager.Instance.UpdateHealth(currentHP, maxHP);
        }
    }

    private void Die() {
        Debug.Log($"{gameObject.name} murió");
        Destroy(gameObject);
    }

    public float GetHP() => currentHP;
    public float GetMaxHP() => maxHP;
}
