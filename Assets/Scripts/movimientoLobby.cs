using UnityEngine;

public class movimientoLobby : MonoBehaviour
{
    public float velocidad = 5f;
    public float fuerzaSalto = 7f;

    private Rigidbody rb;
    private bool estaEnSuelo = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Que el Rigidbody no rote solo
    }

    void Update()
    {
        VerificarSuelo();

        if (Input.GetButtonDown("Jump") && estaEnSuelo)
        {
            rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        Mover();
    }

    void Mover()
    {
        float h = Input.GetAxis("Horizontal"); // A / D
        float v = Input.GetAxis("Vertical");   // W / S

        // Movimiento relativo a donde mira la cámara
        Transform camara = Camera.main.transform;
        Vector3 adelante = Vector3.ProjectOnPlane(camara.forward, Vector3.up).normalized;
        Vector3 derecha = Vector3.ProjectOnPlane(camara.right, Vector3.up).normalized;

        Vector3 movimiento = (adelante * v + derecha * h).normalized;
        rb.MovePosition(rb.position + movimiento * velocidad * Time.fixedDeltaTime);

        // Rotar el personaje hacia donde se mueve
        if (movimiento != Vector3.zero)
        {
            Quaternion rotObjetivo = Quaternion.LookRotation(movimiento);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotObjetivo, Time.deltaTime * 10f);
        }
    }

    void VerificarSuelo()
    {
        estaEnSuelo = Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
}