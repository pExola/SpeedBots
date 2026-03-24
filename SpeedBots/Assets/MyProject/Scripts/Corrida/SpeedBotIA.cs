using UnityEngine;

public class SpeedBotIA : MonoBehaviour
{
    public enum TipoBot { Crawler, Slider, Aerial }

    [Header("Identidade do Chassi")]
    public TipoBot tipoBot;

    [Tooltip("Crawler = 0.2 | Aerial = 0.5 | Slider = 0.9")]
    [Range(0f, 1f)] public float arrancadaBase = 0.5f;

    [Tooltip("Crawler = 0.9 | Aerial = 0.5 | Slider = 0.2")]
    [Range(0f, 1f)] public float durabilidadeBase = 0.5f;

    [Header("Atributos Base do Motor (Controlados pela Progressão)")]
    [HideInInspector] public float velocidadeMaximaBase = 15f;
    [HideInInspector] public float aceleracaoBase = 30f;

    [Header("Parkour")]
    public float forcaPulo = 12f;
    public float forcaWallJumpY = 14f;
    public float forcaWallJumpX = 4f;

    [Header("Sensores Frontais e Buraco")]
    public float distanciaSensorFrente = 0.5f;
    public Vector2 tamanhoCaixaSensor = new Vector2(0.1f, 0.8f);
    public float distanciaOlhoBuraco = 1.0f;
    public float avancoOlhoBuraco = 0.8f;

    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private bool isGrounded;
    private bool isTouchingWall;
    private float moveDirection = 1f;

    // --- VARIÁVEIS DE ESTADO ---
    private string terrenoAtual = "Normal";
    private float stunTimer = 0f;
    private float debuffFogoTimer = 0f;
    private float multiplicadorNitro = 1f;
    private float nitroTimer = 0f;
    private float debuffGanchoTimer = 0f;
    private float tempoAcelerando = 0f; 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
    }

    void Update()
    {
        if (stunTimer > 0) return;

        VerificarAmbiente();

        if (isGrounded)
        {
            if (isTouchingWall || DetectarBuraco() || DevePularDoTerreno())
            {
                PuloNormal();
            }
        }
        else if (isTouchingWall)
        {
            WallJump();
        }
    }

    void FixedUpdate()
    {
        if (!isGrounded && isTouchingWall && rb.linearVelocity.y > 0) { }
        else if (isGrounded) { moveDirection = 1f; }

        if (stunTimer > 0)
        {
            stunTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.8f, rb.linearVelocity.y);
            tempoAcelerando = 0f; // Zera a arrancada se tomar stun
            return;
        }

        float velMaxAtual = velocidadeMaximaBase;
        float acelAtual = aceleracaoBase;
        float friccao = 0.9f;

        // 1. ARRANCADA DA IA (O Efeito Estilingue)
        if (isGrounded)
        {
            tempoAcelerando += Time.fixedDeltaTime;
            if (tempoAcelerando <= 2.0f)
            {
                float bonusAcel = Mathf.Lerp(1.2f, 3.0f, arrancadaBase);
                float bonusVel = Mathf.Lerp(1.0f, 1.3f, arrancadaBase);
                acelAtual *= bonusAcel;
                velMaxAtual *= bonusVel;
            }
        }

        // 2. SINERGIAS DE TERRENO E DURABILIDADE EXTREMA
        if (terrenoAtual == "Lama")
        {
            if (tipoBot == TipoBot.Crawler)
            {
                velMaxAtual *= 1.3f; acelAtual *= 1.3f;
            }
            else if (tipoBot == TipoBot.Slider || tipoBot == TipoBot.Aerial)
            {
                float retencaoStatus = Mathf.Lerp(0.2f, 0.85f, durabilidadeBase);
                velMaxAtual *= retencaoStatus;
                acelAtual *= (retencaoStatus - 0.1f);
            }
        }
        else if (terrenoAtual == "Gelo")
        {
            friccao = 0.99f;
            if (tipoBot == TipoBot.Slider)
            {
                velMaxAtual *= 1.4f; acelAtual *= 1.4f;
            }
            else if (tipoBot == TipoBot.Crawler || tipoBot == TipoBot.Aerial)
            {
                float retencaoStatus = Mathf.Lerp(0.2f, 0.85f, durabilidadeBase);
                velMaxAtual *= retencaoStatus;
                acelAtual *= (retencaoStatus - 0.1f);
            }
        }

        // DURABILIDADE NAS RAMPAS
        if (rb.linearVelocity.y > 0.5f && isGrounded)
        {
            float penalidadeRampa = Mathf.Lerp(0.3f, 1.0f, durabilidadeBase);
            acelAtual *= penalidadeRampa;
        }

        if (tipoBot == TipoBot.Aerial)
        {
            velMaxAtual *= 1.15f; acelAtual *= 1.10f;
        }

        // 3. DEBUFFS DE COMBATE
        if (debuffFogoTimer > 0)
        {
            debuffFogoTimer -= Time.fixedDeltaTime;
            velMaxAtual *= 0.5f; acelAtual *= 0.5f;
        }

        if (debuffGanchoTimer > 0)
        {
            debuffGanchoTimer -= Time.fixedDeltaTime;
            velMaxAtual *= 0.4f; acelAtual *= 0.4f;
        }

        if (nitroTimer > 0)
        {
            nitroTimer -= Time.fixedDeltaTime;
            velMaxAtual *= multiplicadorNitro;
            acelAtual *= multiplicadorNitro;
        }
        else { multiplicadorNitro = 1f; }

        // --- APLICAÇÃO FÍSICA ---
        if (Mathf.Abs(rb.linearVelocity.x) < velMaxAtual)
        {
            rb.AddForce(new Vector2(moveDirection * acelAtual, 0), ForceMode2D.Force);
        }

        if (Mathf.Abs(rb.linearVelocity.x) > velMaxAtual)
        {
            float velXSuave = Mathf.Lerp(rb.linearVelocity.x, velMaxAtual * Mathf.Sign(rb.linearVelocity.x), 0.1f);
            rb.linearVelocity = new Vector2(velXSuave, rb.linearVelocity.y);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Lama")) terrenoAtual = "Lama";
        if (collision.CompareTag("Gelo")) terrenoAtual = "Gelo";
        if (collision.CompareTag("Fogo"))
        {
            stunTimer = Mathf.Lerp(1.2f, 0.1f, durabilidadeBase);
            debuffFogoTimer = 3.0f;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Lama") || collision.CompareTag("Gelo")) terrenoAtual = "Normal";
    }

    private void PuloNormal()
    {
        float impulsoFinal = (tipoBot == TipoBot.Aerial) ? forcaPulo * 1.3f : forcaPulo;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * impulsoFinal, ForceMode2D.Impulse);
    }

    private bool DevePularDoTerreno()
    {
        if (terrenoAtual == "Fogo") return true;
        if (tipoBot == TipoBot.Slider && terrenoAtual == "Lama") return true;
        if (tipoBot == TipoBot.Crawler && terrenoAtual == "Gelo") return true;
        if (tipoBot == TipoBot.Aerial && (terrenoAtual == "Lama" || terrenoAtual == "Gelo")) return true;
        return false;
    }

    private void WallJump()
    {
        moveDirection = moveDirection * -1;
        float puloYFinal = (tipoBot == TipoBot.Aerial) ? forcaWallJumpY * 1.2f : forcaWallJumpY;
        float puloXFinal = (tipoBot == TipoBot.Aerial) ? forcaWallJumpX * 1.2f : forcaWallJumpX;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(moveDirection * puloXFinal, puloYFinal), ForceMode2D.Impulse);
    }

    private void VerificarAmbiente()
    {
        Vector2 centro = col.bounds.center;
        Vector2 destinoCaixa = centro + (new Vector2(moveDirection, 0) * (col.bounds.extents.x + distanciaSensorFrente));
        Collider2D[] hits = Physics2D.OverlapBoxAll(destinoCaixa, tamanhoCaixaSensor, 0f);
        isTouchingWall = false;
        foreach (Collider2D hit in hits) { if (hit.CompareTag("Parede")) { isTouchingWall = true; break; } }
    }

    private bool DetectarBuraco()
    {
        float centroX = col.bounds.center.x;
        float centroY = col.bounds.center.y;
        Vector2 origem = new Vector2(centroX + (avancoOlhoBuraco * moveDirection), centroY);
        float distanciaRaio = col.bounds.extents.y + 0.8f;
        RaycastHit2D hit = Physics2D.Raycast(origem, Vector2.down, distanciaRaio);

        if (hit.collider == null) return true;

        string tag = hit.collider.tag;
        if (tag != "Pista" && tag != "Lama" && tag != "Gelo" && tag != "Fogo") return true;
        return false;
    }

    public void AtivarNitro(float forca, float duracao) { multiplicadorNitro = forca; nitroTimer = duracao; }
    public void TomarStunDeItem(float tempoBase)
    {
        stunTimer = Mathf.Lerp(tempoBase, tempoBase * 0.2f, durabilidadeBase);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.3f, rb.linearVelocity.y);
    }
    public void SofrerPuxao(float forcaPuxao, float direcaoX, float tempoDebuff)
    {
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(direcaoX * forcaPuxao, 4f), ForceMode2D.Impulse);
        debuffGanchoTimer = tempoDebuff;
    }
    public float GetDirecaoOlhar() { return moveDirection; }

    private void OnCollisionStay2D(Collision2D collision) { if (collision.contacts[0].normal.y > 0.5f) isGrounded = true; }
    private void OnCollisionExit2D(Collision2D collision) { isGrounded = false; }
}
