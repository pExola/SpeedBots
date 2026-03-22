using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
public class SpeedBotMovment : MonoBehaviour
{
    public enum TipoBot { Crawler, Slider, Aerial }

    [Header("Identidade do Chassi")]
    public TipoBot tipoBot;

    [Tooltip("Crawler = 0.2 | Aerial = 0.5 | Slider = 0.9")]
    [Range(0f, 1f)] public float arrancadaBase = 0.5f;

    [Tooltip("Crawler = 0.9 | Aerial = 0.5 | Slider = 0.2")]
    [Range(0f, 1f)] public float durabilidadeBase = 0.5f;

    [Header("Atributos do Motor (Controlados pela Progressão)")]
    // Estes valores agora começam invisíveis no Inspector para não confundir, 
    // pois o SpeedBotProgression que vai preenchê-los.
    [HideInInspector] public float velocidadeMaximaBase = 15f;
    [HideInInspector] public float aceleracaoBase = 30f;

    [Header("Parkour")]
    public float forcaPulo = 12f;
    public float forcaWallJumpY = 14f;
    public float forcaWallJumpX = 4f;
    public float distanciaSensor = 0.1f;
    public Vector2 tamanhoCaixaSensor = new Vector2(0.1f, 0.8f);

    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private bool isGrounded;
    private bool isTouchingWall;
    private float lastMoveDirection = 1f;

    // --- VARIÁVEIS DE ESTADO ---
    private string terrenoAtual = "Normal";
    private float stunTimer = 0f;
    private float debuffFogoTimer = 0f;
    private float debuffGanchoTimer = 0f;
    private float multiplicadorNitro = 1f;
    private float nitroTimer = 0f;

    // --- NOVO: Controle de Arrancada ---
    private float tempoAcelerando = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        float moveInput = 0f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput = 1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput = -1f;

        if (moveInput != 0) lastMoveDirection = moveInput;

        VerificarParede();

        if (stunTimer <= 0 && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (isGrounded) PuloNormal();
            else if (isTouchingWall) WallJump();
        }
    }

    void FixedUpdate()
    {
        if (stunTimer > 0)
        {
            stunTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.8f, rb.linearVelocity.y);
            tempoAcelerando = 0f; // Zera a arrancada se tomar stun
            return;
        }

        float moveInput = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput = 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput = -1f;
        }

        // --- INÍCIO DO CÁLCULO DE RPG ---
        float velMaxAtual = velocidadeMaximaBase;
        float acelAtual = aceleracaoBase;
        float friccao = 0.9f;

        if (moveInput != 0 && isGrounded)
        {
            tempoAcelerando += Time.fixedDeltaTime;
            if (tempoAcelerando <= 2.0f)
            {
                // MÁGICA DO GAME FEEL:
                // Arrancada 0.0 (Slider Nível 1) = Aceleração x1.2 e Velocidade x1.0
                // Arrancada 1.0 (Slider Nível 20) = Aceleração x3.0 e Velocidade x1.3 (Ele ultrapassa o limite físico!)
                float bonusAcel = Mathf.Lerp(1.2f, 3.0f, arrancadaBase);
                float bonusVel = Mathf.Lerp(1.0f, 1.3f, arrancadaBase);

                acelAtual *= bonusAcel;
                velMaxAtual *= bonusVel;
            }
        }
        else if (moveInput == 0)
        {
            tempoAcelerando = 0f; // Soltou o botão, reseta a arrancada
        }

        // 2. Sinergias de Terreno e a NOVA Durabilidade Extrema
        if (terrenoAtual == "Lama")
        {
            if (tipoBot == TipoBot.Crawler)
            {
                velMaxAtual *= 1.3f; acelAtual *= 1.3f;
            }
            else if (tipoBot == TipoBot.Slider || tipoBot == TipoBot.Aerial)
            {
                // Durabilidade 0.0 = Cai para 20% da velocidade (quase atola)
                // Durabilidade 1.0 = Segura 85% da velocidade (passa rasgando)
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

        // ... (Debuffs de Fogo, Gancho e Nitro continuam iguais aqui no meio) ...

        // --- APLICAÇÃO FÍSICA E RAMPAS ---

        // DURABILIDADE NAS RAMPAS (Subidas pesadas):
        if (rb.linearVelocity.y > 0.5f && isGrounded)
        {
            // Durabilidade 0.0 = Perde 70% da força do motor na subida
            // Durabilidade 1.0 = Ignora a subida (100% de força)
            float penalidadeRampa = Mathf.Lerp(0.3f, 1.0f, durabilidadeBase);
            acelAtual *= penalidadeRampa;
        }

        // Passiva do Aerial
        if (tipoBot == TipoBot.Aerial)
        {
            velMaxAtual *= 1.15f;
            acelAtual *= 1.10f;
        }

        // 3. Debuffs de Combate e Nitro
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
        // A NOVA DURABILIDADE EM RAMPAS (Subidas):
        // Se a velocidade Y for positiva (subindo), aplicamos uma pequena resistência à aceleração,
        // mas robôs com alta durabilidade (como o Crawler) ignoram isso.
        if (rb.linearVelocity.y > 0.5f && isGrounded)
        {
            float penalidadeRampa = Mathf.Lerp(0.7f, 1.0f, durabilidadeBase);
            acelAtual *= penalidadeRampa;
        }

        if (Mathf.Abs(rb.linearVelocity.x) < velMaxAtual && moveInput != 0)
        {
            rb.AddForce(new Vector2(moveInput * acelAtual, 0), ForceMode2D.Force);
        }

        if (moveInput == 0 && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * friccao, rb.linearVelocity.y);
        }

        if (Mathf.Abs(rb.linearVelocity.x) > velMaxAtual)
        {
            float velXSuave = Mathf.Lerp(rb.linearVelocity.x, velMaxAtual * Mathf.Sign(rb.linearVelocity.x), 0.1f);
            rb.linearVelocity = new Vector2(velXSuave, rb.linearVelocity.y);
        }
    }

    private void VerificarParede()
    {
        Vector2 centro = col.bounds.center;
        Vector2 direcao = lastMoveDirection > 0 ? Vector2.right : Vector2.left;

        // Coloca a caixa de verificação EXATAMENTE onde o Gizmo desenha na tela
        Vector2 destino = centro + (direcao * (col.bounds.extents.x + distanciaSensor));

        // Pega todos os colisores que estão tocando na caixa verde/vermelha
        Collider2D[] hits = Physics2D.OverlapBoxAll(destino, tamanhoCaixaSensor, 0f);

        isTouchingWall = false; // Começa assumindo que não tem parede

        foreach (Collider2D hit in hits)
        {
            // Se qualquer coisa dentro da caixa tiver a tag Ground, a parede é detectada!
            if (hit.CompareTag("Parede"))
            {
                isTouchingWall = true;
                break;
            }
        }
    }

    private void PuloNormal()
    {
        float impulsoFinal = (tipoBot == TipoBot.Aerial) ? forcaPulo * 1.3f : forcaPulo;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * impulsoFinal, ForceMode2D.Impulse);
    }

    private void WallJump()
    {
        float puloYFinal = (tipoBot == TipoBot.Aerial) ? forcaWallJumpY * 1.2f : forcaWallJumpY;
        float puloXFinal = (tipoBot == TipoBot.Aerial) ? forcaWallJumpX * 1.2f : forcaWallJumpX;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(lastMoveDirection * puloXFinal, puloYFinal), ForceMode2D.Impulse); 
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Garante que só considera chão se a superfície for plana/rampa
        if (collision.contacts[0].normal.y > 0.5f) isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }

    // Detecta quando o robô ENTRA em uma zona especial
    // --- TRIGGERS DOS TERRENOS ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Lama")) terrenoAtual = "Lama";
        if (collision.CompareTag("Gelo")) terrenoAtual = "Gelo";

        if (collision.CompareTag("Fogo"))
        {
            // O Fogo agora apenas trava os controles (Stun) e deixa um debuff
            // Durabilidade 1.0 (Crawler) toma só 0.1s de stun. Durabilidade 0.0 toma 1.2s.
            stunTimer = Mathf.Lerp(1.2f, 0.1f, durabilidadeBase);
            debuffFogoTimer = 3.0f; // O robô fica manco por 3 segundos
            Debug.Log($"FOGO! Stun de {stunTimer}s aplicado.");
        }
    }

    // --- MÉTODOS PÚBLICOS DE COMBATE ---
    public void AtivarNitro(float forca, float duracao)
    {
        multiplicadorNitro = forca;
        nitroTimer = duracao;
    }

    public void TomarStunDeItem(float tempoBase)
    {
        // A durabilidadeBase do seu RPG reduz o tempo da armadilha!
        stunTimer = Mathf.Lerp(tempoBase, tempoBase * 0.2f, durabilidadeBase);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.3f, rb.linearVelocity.y); // Freia 70% na hora
    }

    public void SofrerPuxao(float forcaPuxao, float direcaoX, float tempoDebuff)
    {
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(direcaoX * forcaPuxao, 4f), ForceMode2D.Impulse);

        // Aplica o tempo de lentidão no motor!
        debuffGanchoTimer = tempoDebuff;
    }

    // Retorna a direção para o Gancho saber para onde atirar
    public float GetDirecaoOlhar()
    {
        // No Player, troque 'moveDirection' por 'lastMoveDirection'
        return lastMoveDirection;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Lama") || collision.CompareTag("Gelo"))
        {
            terrenoAtual = "Normal";
        }
    }

    private void OnDrawGizmos()
    {
        if (col == null) col = GetComponent<CapsuleCollider2D>();
        if (col == null) return;

        Vector2 centro = col.bounds.center;
        Vector2 direcao = lastMoveDirection > 0 ? Vector2.right : Vector2.left;
        Vector2 destino = centro + (direcao * (col.bounds.extents.x + distanciaSensor));

        // A caixa vai ficar verde quando detectar o Ground
        Gizmos.color = isTouchingWall ? Color.green : Color.red;
        Gizmos.DrawWireCube(destino, tamanhoCaixaSensor);
    }
}
