using UnityEngine;

public class Health : MonoBehaviour {

    [SerializeField] private float maxHP = 100f;
    [SerializeField] private bool mostrarUI = false;
    [SerializeField] private GameObject damageNumberPrefab;

    private float currentHP;

    private void Start() {
        currentHP = maxHP;
    }

    public void TakeDamage(float amount) {
        currentHP -= amount;
        Debug.Log($"{gameObject.name} recibió {amount} daño. HP: {currentHP}/{maxHP}");

        if (damageNumberPrefab != null) {
            GameObject num = Instantiate(damageNumberPrefab, transform.position + transform.up, transform.rotation);
            num.GetComponent<DamageNumber>().Setup(amount);
        }

        if (currentHP <= 0) {
            Die();
        }
    }

    private void Die() {
        Debug.Log($"{gameObject.name} murió");
        Destroy(gameObject);
    }

    public float GetHP() => currentHP;
    public float GetMaxHP() => maxHP;

    private void OnGUI() {
        if (!mostrarUI) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = Mathf.RoundToInt(Screen.height * 0.04f); // ← más grande
        style.normal.textColor = Color.white;

        GUI.Label(
            new Rect(Screen.width * 0.02f, Screen.height * 0.08f, Screen.width * 0.25f, Screen.height * 0.06f),
            $"❤ HP: {currentHP}/{maxHP}",
            style
        );
    }
}