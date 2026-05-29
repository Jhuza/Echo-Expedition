using UnityEngine;
public class JugadorAnimacion : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El Transform del parent (cápsula con Rigidbody). Se asigna automáticamente si es null.")]
    public Transform playerParent;

    [Header("Rotación del modelo")]
    public float velocidadRotacion = 10f;

    [Header("Detección de suelo")]
    public float raycastDistancia = 1.1f;

    [Header("NPC Interaction")]
    [Tooltip("Radio para detectar NPCs cercanos y activar Happy Idle.")]
    public float radioDeteccionNPC = 3f;
    [Tooltip("Layer o tag de NPCs. El overlap busca colliders con tag NPC.")]
    public float radioSalidaNPC = 4f;

    // ── Componentes ──────────────────────────────────────────────────────────
    private Animator anim;
    private Rigidbody rb;

    // ── Estado interno ────────────────────────────────────────────────────────
    private bool estaEnSuelo;
    private bool saltoPendiente;
    private bool enModoHablar;          // true mientras está "hablando" con NPC

    // ── Hashes ───────────────────────────────────────────────────────────────
    private static readonly int HashIsRunning  = Animator.StringToHash("isRunning");
    private static readonly int HashIsGrounded = Animator.StringToHash("isGrounded");
    private static readonly int HashJump       = Animator.StringToHash("Jump");
    private static readonly int HashAttack     = Animator.StringToHash("Attack");
    private static readonly int HashReaction   = Animator.StringToHash("Reaction");
    private static readonly int HashHappy      = Animator.StringToHash("Happy");
    private static readonly int HashExcavate   = Animator.StringToHash("Excavate");
    private static readonly int HashPickup     = Animator.StringToHash("Pickup");
    private static readonly int HashPush       = Animator.StringToHash("Push");
    private static readonly int HashBackflip   = Animator.StringToHash("Backflip");

    // ─────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        anim = GetComponent<Animator>();

        if (playerParent == null)
            playerParent = transform.parent;

        if (playerParent != null)
            rb = playerParent.GetComponent<Rigidbody>();

        if (anim == null)
            Debug.LogError("[JugadorAnimacion] No se encontró Animator en " + gameObject.name);
        if (rb == null)
            Debug.LogError("[JugadorAnimacion] No se encontró Rigidbody en el parent.");
    }

    void Update()
    {
        VerificarSuelo();
        ActualizarMovimiento();
        GirarModeloHaciaMovimiento();
        DetectarInputSalto();
        DetectarInputAcciones();
        DetectarNPC();
    }

    void VerificarSuelo()
    {
        RaycastHit hit;
        bool golpeó = Physics.Raycast(
            playerParent.position,
            -playerParent.up,
            out hit,
            raycastDistancia
        );

        estaEnSuelo = golpeó;

        anim.SetBool(HashIsGrounded, estaEnSuelo);
    }

    // ── Movimiento ────────────────────────────────────────────────────────────

    void ActualizarMovimiento()
    {
        float hRaw = Input.GetAxisRaw("Horizontal");
        float vRaw = Input.GetAxisRaw("Vertical");

        bool hayInput = Mathf.Abs(hRaw) > 0.01f || Mathf.Abs(vRaw) > 0.01f;

        // Corriendo: basta con que haya input y no estemos hablando.
        // No exigimos estaEnSuelo porque en planetas esféricos el raycast puede
        // fallar un frame (orientación con Slerp) y eso cortaría la animación.
        // Si el personaje está en el aire, el animator ya prioriza Jump/Run Jump
        // independientemente del valor de isRunning.
        bool corriendo = hayInput && !enModoHablar;
        anim.SetBool(HashIsRunning, corriendo);
    }

    // ── Orientación visual ────────────────────────────────────────────────────

    void GirarModeloHaciaMovimiento()
    {
        float hRaw = Input.GetAxisRaw("Horizontal");
        float vRaw = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(hRaw) < 0.01f && Mathf.Abs(vRaw) < 0.01f) return;

        Vector3 dirMovimiento = (playerParent.right * hRaw + playerParent.forward * vRaw).normalized;
        if (dirMovimiento == Vector3.zero) return;

        Quaternion rotObjetivo = Quaternion.LookRotation(dirMovimiento, playerParent.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rotObjetivo,
            Time.deltaTime * velocidadRotacion
        );
    }

    // ── Salto ─────────────────────────────────────────────────────────────────

    void DetectarInputSalto()
    {
        if (Input.GetButtonDown("Jump") && estaEnSuelo && !saltoPendiente)
        {
            saltoPendiente = true;
            anim.SetTrigger(HashJump);
        }

        if (!estaEnSuelo && saltoPendiente)
            saltoPendiente = false;
    }

    // ── Acciones ──────────────────────────────────────────────────────────────

    void DetectarInputAcciones()
    {
        if (Input.GetKeyDown(KeyCode.F))  anim.SetTrigger(HashAttack);   // Ataque
        if (Input.GetKeyDown(KeyCode.X))  anim.SetTrigger(HashExcavate); // Excavar
        if (Input.GetKeyDown(KeyCode.C))  anim.SetTrigger(HashPickup);   // Recoger
        if (Input.GetKeyDown(KeyCode.V))  anim.SetTrigger(HashPush);     // Empujar
        if (Input.GetKeyDown(KeyCode.Z))  anim.SetTrigger(HashBackflip); // Backflip

        // Hablar con NPC — E activa/desactiva modo hablar si hay un NPC cerca
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (HayNPCCerca())
                ActivarHablar();
            // Salir del modo hablar se hace moviéndose (ver DetectarNPC)
        }
    }

    // ── NPC / Happy Idle ──────────────────────────────────────────────────────

    void DetectarNPC()
    {
        if (!enModoHablar) return;

        // Si el jugador empieza a moverse, sale del modo hablar
        float hRaw = Input.GetAxisRaw("Horizontal");
        float vRaw = Input.GetAxisRaw("Vertical");
        bool hayInput = Mathf.Abs(hRaw) > 0.01f || Mathf.Abs(vRaw) > 0.01f;

        if (hayInput)
            DesactivarHablar();
    }

    bool HayNPCCerca()
    {
        // Busca cualquier collider con tag "NPC" en el radio indicado
        Collider[] cercanos = Physics.OverlapSphere(playerParent.position, radioDeteccionNPC);
        foreach (Collider col in cercanos)
        {
            if (col.CompareTag("NPC"))
                return true;
        }
        return false;
    }

    void ActivarHablar()
    {
        enModoHablar = true;
        anim.SetTrigger(HashHappy);
        anim.SetBool(HashIsRunning, false);
    }

    void DesactivarHablar()
    {
        enModoHablar = false;
        // Vuelve a idle; el siguiente frame ActualizarMovimiento decide si corre
    }

    // ── API pública ───────────────────────────────────────────────────────────

    /// <summary>
    /// Llama esto desde el script del enemigo cuando golpea al jugador.
    /// Ejemplo desde el enemigo:
    ///   FindObjectOfType&lt;JugadorAnimacion&gt;().RecibirImpacto();
    /// O mejor:
    ///   colision.GetComponentInParent&lt;JugadorAnimacion&gt;().RecibirImpacto();
    /// </summary>
    public void RecibirImpacto()
    {
        anim.SetTrigger(HashReaction);
    }

    /// <summary>
    /// Detección automática de impacto por colisión física (sin IsTrigger).
    /// El enemigo necesita tag "Enemy". Funciona con colisiones normales (no trigger).
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            anim.SetTrigger(HashReaction);
        }
    }

    // Dibuja el radio de detección de NPC en el editor para debug
    private void OnDrawGizmosSelected()
    {
        if (playerParent == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(playerParent.position, radioDeteccionNPC);
    }
}