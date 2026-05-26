using UnityEngine;

public class Jugador : MonoBehaviour
{
    public GameObject[] planetas;
    public float velocidad = 5f;
    public float fuerzaSalto = 7f;
    public float fuerzaGravedad = 20f;

    private Rigidbody rb;
    private bool estaEnSuelo = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Se desactiva la gravedad de unity
        rb.constraints = RigidbodyConstraints.FreezeRotation; // Para bloquear la rotación, además de en unity, por si acaso
    }

    void Update()
    {
        OrientarAlPlaneta();
        VerificarSuelo();
        Saltar();
    }

    void FixedUpdate()
    {
        AplicarGravedad();
        Mover();
    }

    void OrientarAlPlaneta()
    {
        // Busca el planeta más cercano
        GameObject planetaCercano = null;
        float menorDistancia = Mathf.Infinity;

        foreach (GameObject planeta in planetas)
        {
            float distancia = Vector3.Distance(transform.position, planeta.transform.position);
            if (distancia < menorDistancia)
            {
                menorDistancia = distancia;
                planetaCercano = planeta;
            }
        }

        if (planetaCercano == null) return;

        // Dirección hacia arriba del jugador
        Vector3 dirHaciaArriba = (transform.position - planetaCercano.transform.position).normalized;

        // Para que los pies apunten al planeta
        Quaternion rotacionObjetivo = Quaternion.FromToRotation(transform.up, dirHaciaArriba) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, Time.deltaTime * 10f);
    }

    void AplicarGravedad()
    {
        // Gravedad manual hacia el planeta más cercano
        GameObject planetaCercano = ObtenerPlanetaCercano();
        if (planetaCercano == null) return;

        Vector3 dirGravedad = (planetaCercano.transform.position - transform.position).normalized;
        rb.AddForce(dirGravedad * fuerzaGravedad, ForceMode.Acceleration);
    }

    void Mover()
    {
        float h = Input.GetAxis("Horizontal"); // A / D
        float v = Input.GetAxis("Vertical");   // W / S

        // Movimiento relativo a la orientación del jugador
        Vector3 movimiento = transform.right * h + transform.forward * v;
        rb.MovePosition(rb.position + movimiento * velocidad * Time.fixedDeltaTime);
    }

    void Saltar()
    {
        if (Input.GetButtonDown("Jump") && estaEnSuelo)
        {
            // Salto, alejándose del planeta, usando el vector del jugador
            rb.AddForce(transform.up * fuerzaSalto, ForceMode.Impulse);
        }
    }

    void VerificarSuelo()
    {
        // Raycast hacia abajo para saber si estamos en el suelo
        estaEnSuelo = Physics.Raycast(transform.position, -transform.up, 1.1f);
    }

    GameObject ObtenerPlanetaCercano()
    {
        GameObject cercano = null;
        float menorDistancia = Mathf.Infinity;

        foreach (GameObject planeta in planetas)
        {
            float distancia = Vector3.Distance(transform.position, planeta.transform.position);
            if (distancia < menorDistancia)
            {
                menorDistancia = distancia;
                cercano = planeta;
            }
        }

        return cercano;
    }
}