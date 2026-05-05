using UnityEngine;

public class Enemigo : MonoBehaviour
{
    public Transform jugador;
    public float velocidad = 3f;
    public float fuerzaGravedad = 20f;
    public float distanciaDeteccion = 15f;
    public float distanciaAtaque = 1.5f;

    private Rigidbody rb;
    private GameObject[] planetas;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // Pa que busque los planetas de misma forma que el jugador
        planetas = GameObject.FindGameObjectsWithTag("Planeta");
    }

    void FixedUpdate()
    {
        AplicarGravedad();
        OrientarAlPlaneta();

        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia <= distanciaDeteccion && distancia > distanciaAtaque)
        {
            PerseguirJugador();
        }
    }

    void PerseguirJugador()
    {
        // Dirección hacia el jugador desde el plano del enemigo
        Vector3 dirHaciaJugador = (jugador.position - transform.position).normalized;

        // Proyección para que no se despegue
        Vector3 dirMovimiento = Vector3.ProjectOnPlane(dirHaciaJugador, transform.up).normalized;

        rb.MovePosition(rb.position + dirMovimiento * velocidad * Time.fixedDeltaTime);

        // Rotación para que observe al jugador
        if (dirMovimiento != Vector3.zero)
        {
            Quaternion rotObjetivo = Quaternion.LookRotation(dirMovimiento, transform.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotObjetivo, Time.deltaTime * 8f);
        }
    }

    void OrientarAlPlaneta()
    {
        GameObject planetaCercano = ObtenerPlanetaCercano();
        if (planetaCercano == null) return;

        Vector3 dirHaciaArriba = (transform.position - planetaCercano.transform.position).normalized;
        Quaternion rotacionObjetivo = Quaternion.FromToRotation(transform.up, dirHaciaArriba) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, Time.deltaTime * 10f);
    }

    void AplicarGravedad()
    {
        GameObject planetaCercano = ObtenerPlanetaCercano();
        if (planetaCercano == null) return;

        Vector3 dirGravedad = (planetaCercano.transform.position - transform.position).normalized;
        rb.AddForce(dirGravedad * fuerzaGravedad, ForceMode.Acceleration);
    }

    GameObject ObtenerPlanetaCercano()
    {
        GameObject cercano = null;
        float menorDistancia = Mathf.Infinity;

        foreach (GameObject planeta in planetas)
        {
            float dist = Vector3.Distance(transform.position, planeta.transform.position);
            if (dist < menorDistancia)
            {
                menorDistancia = dist;
                cercano = planeta;
            }
        }
        return cercano;
    }
}