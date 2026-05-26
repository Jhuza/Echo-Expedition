using UnityEngine;

public class Enemigo : MonoBehaviour
{
    public Transform jugador;
    public float velocidad = 3f;
    public float fuerzaGravedad = 20f;
    public float distanciaDeteccion = 15f;
    public float distanciaAtaque = 1.5f;

    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackCooldown = 1f;
    private float lastAttackTime;

    private Rigidbody rb;
    private GameObject[] planetas;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
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

        // Ataca si está lo suficientemente cerca
        if (distancia <= distanciaAtaque)
        {
            AtacarJugador();
        }
    }

    void AtacarJugador()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            if (jugador.TryGetComponent(out Health health))
            {
                health.TakeDamage(damage);
            }
        }
    }

    void PerseguirJugador()
    {
        Vector3 dirHaciaJugador = (jugador.position - transform.position).normalized;
        Vector3 dirMovimiento = Vector3.ProjectOnPlane(dirHaciaJugador, transform.up).normalized;
        rb.MovePosition(rb.position + dirMovimiento * velocidad * Time.fixedDeltaTime);

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