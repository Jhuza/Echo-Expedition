using UnityEngine;

public class CamaraJugador : MonoBehaviour {

    [SerializeField] private Transform objetivo;
    [SerializeField] private float distancia = 6f;
    [SerializeField] private float altura = 2.5f;
    [SerializeField] private float suavidad = 5f;

    private void LateUpdate() {
        if (objetivo == null) return;

        // Se posiciona siempre detrás del jugador según su orientación actual
        Vector3 posicionObjetivo = objetivo.position
            - objetivo.forward * distancia
            + objetivo.up * altura;

        transform.position = Vector3.Lerp(transform.position, posicionObjetivo, Time.deltaTime * suavidad);

        // Siempre mira al jugador
        transform.LookAt(objetivo.position + objetivo.up * altura / 2);
    }
}