using UnityEngine;

public class SpeedBotIA : MonoBehaviour
{
    public enum TipoBot { Crawler, Slider, Aerial }

    [Header("Identidade do Chassi")]
    public TipoBot tipoBot;

    [Range(0f, 1f)] public float aderenciaBase = 0.5f;
    [Range(0f, 1f)] public float durabilidadeBase = 0.5f;

    [Header("Atributos Base do Motor")]
    public float velocidadeMaximaBase = 14f;
    public float aceleracaoBase = 25f;
    public float forcaPulo = 12f;

    [Header("Parkour (Vector Style)")]
    public float forcaWallJumpY = 14f;
    public float forcaWallJumpX = 4f;

    [Header("Sensores Frontais (Obstáculos)")]
    public float distanciaSensorFrente = 0.5f;
    public Vector2 tamanhoCaixaSensor = new Vector2(0.1f, 0.8f);

    [Header("Sensores de Buraco (Precipício)")]
    public float distanciaOlhoBuraco = 1.0f;
    public float avancoOlhoBuraco = 0.8f;

    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private bool isGrounded;
    private bool isTouchingWall;
    private float moveDirection = 1f;

    // --- VARIÁVEIS DE ESTADO (RPG) ---
    private string terrenoAtual = "Normal";
    private float stunTimer = 0f;
    private float debuffFogoTimer = 0f;
    private float multiplicadorNitro = 1f;
    private float nitroTimer = 0f;
    private float debuffGanchoTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
    }

    void Update()
    {
        // Se estiver atordoado pelo fogo, a IA não "pensa" (não pula nem faz wall-jump)
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
        // 1. Controle de Direção da IA
        if (!isGrounded && isTouchingWall && rb.linearVelocity.y > 0)
        {
            // Mantém a direção invertida no corredor de WallJump
        }
        else if (isGrounded)
        {
            moveDirection = 1f; // Força a ida para a direita sempre que pisa no chão firme
        }

        // 2. Controle de Stun
        if (stunTimer > 0)
        {
            stunTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.8f, rb.linearVelocity.y);
            return;
        }

        // --- CÁLCULO DE RPG (Sinergias) ---
        float velMaxAtual = velocidadeMaximaBase;
        float acelAtual = aceleracaoBase;
        float gripAtual = aderenciaBase;

        if (terrenoAtual == "Lama")
        {
            if (tipoBot == TipoBot.Crawler)
            {
                velMaxAtual *= 1.3f; acelAtual *= 1.3f;
                gripAtual = Mathf.Clamp01(gripAtual + 0.3f);
            }
            else if (tipoBot == TipoBot.Slider)
            {
                velMaxAtual *= 0.3f; acelAtual *= 0.2f;
                gripAtual = Mathf.Clamp01(gripAtual - 0.4f);
            }
            else if (tipoBot == TipoBot.Aerial)
            {
                velMaxAtual *= 0.7f; acelAtual *= 0.7f;
            }
        }
        else if (terrenoAtual == "Gelo")
        {
            if (tipoBot == TipoBot.Slider)
            {
                velMaxAtual *= 1.4f; acelAtual *= 1.4f;
                gripAtual = Mathf.Clamp01(gripAtual + 0.3f);
            }
            else if (tipoBot == TipoBot.Crawler)
            {
                velMaxAtual *= 0.3f; acelAtual *= 0.2f;
                gripAtual = Mathf.Clamp01(gripAtual - 0.4f);
            }
            else if (tipoBot == TipoBot.Aerial)
            {
                velMaxAtual *= 0.7f; acelAtual *= 0.7f;
            }
        }

        // Passiva do Aerial
        if (tipoBot == TipoBot.Aerial)
        {
            velMaxAtual *= 1.15f;
            acelAtual *= 1.10f;
            if (terrenoAtual == "Lama" || terrenoAtual == "Gelo") gripAtual += 0.1f;
        }

        // Debuff do Fogo
        if (debuffFogoTimer > 0)
        {
            debuffFogoTimer -= Time.fixedDeltaTime;
            velMaxAtual *= 0.5f;
            acelAtual *= 0.5f;
        }

        // 6. Efeito do Nitrogênio
        if (nitroTimer > 0)
        {
            nitroTimer -= Time.fixedDeltaTime;
            velMaxAtual *= multiplicadorNitro;
            acelAtual *= multiplicadorNitro;
        }
        else
        {
            multiplicadorNitro = 1f;
        }

        // Debuff do Gancho (Reduz drasticamente a velocidade e aceleração)
        if (debuffGanchoTimer > 0)
        {
            debuffGanchoTimer -= Time.fixedDeltaTime;
            velMaxAtual *= 0.4f; // Fica só com 40% da velocidade
            acelAtual *= 0.4f;
        }

        // --- APLICAÇÃO FÍSICA ---
        if (Mathf.Abs(rb.linearVelocity.x) < velMaxAtual)
        {
            rb.AddForce(new Vector2(moveDirection * acelAtual, 0), ForceMode2D.Force);
        }

        // Clamp de Velocidade
        if (Mathf.Abs(rb.linearVelocity.x) > velMaxAtual)
        {
            float velXSuave = Mathf.Lerp(rb.linearVelocity.x, velMaxAtual * Mathf.Sign(rb.linearVelocity.x), 0.1f);
            rb.linearVelocity = new Vector2(velXSuave, rb.linearVelocity.y);
        }
    }

    // --- TRIGGERS DOS TERRENOS ---
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

    // --- MÉTODOS DE AÇÃO ---
    private void PuloNormal()
    {
        float impulsoFinal = (tipoBot == TipoBot.Aerial) ? forcaPulo * 1.3f : forcaPulo;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * impulsoFinal, ForceMode2D.Impulse);
    }

    private bool DevePularDoTerreno()
    {
        // Fogo queima todo mundo, então a IA sempre vai tentar pular para sair dele
        if (terrenoAtual == "Fogo") return true;

        // Se for Slider e pisou na Lama, pula para tentar fugir! (Mas o debuff já aplicou na aterrissagem)
        if (tipoBot == TipoBot.Slider && terrenoAtual == "Lama") return true;

        // Se for Crawler e pisou no Gelo, pula!
        if (tipoBot == TipoBot.Crawler && terrenoAtual == "Gelo") return true;

        // Se for Aerial, ele odeia tocar o chão em áreas ruins, então tenta pular sempre que tocar em Lama ou Gelo
        if (tipoBot == TipoBot.Aerial && (terrenoAtual == "Lama" || terrenoAtual == "Gelo")) return true;

        // Se for Pista normal (ou um terreno que a IA gosta), não pula.
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

    // --- SENSORES E COLISÕES (MANTIDOS) ---
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

        // Se não bater em NADA, é literalmente um buraco mortal. Pode pular!
        if (hit.collider == null) return true;

        // Se bateu em algo, confere se é o cenário do jogo. Se não for nenhuma dessas 4 tags 
        // (ex: bateu num inimigo ou enfeite), pula por segurança.
        string tag = hit.collider.tag;
        if (tag != "Pista" && tag != "Lama" && tag != "Gelo" && tag != "Fogo")
        {
            return true;
        }

        return false;
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
        return moveDirection;
    }

    private void OnCollisionStay2D(Collision2D collision) { if (collision.contacts[0].normal.y > 0.5f) isGrounded = true; }
    private void OnCollisionExit2D(Collision2D collision) { isGrounded = false; }

    private void OnDrawGizmos()
    {
        if (col == null) col = GetComponent<CapsuleCollider2D>();
        if (col == null) return;

        Vector2 centro = col.bounds.center;
        Vector2 destinoCaixa = centro + (new Vector2(moveDirection, 0) * (col.bounds.extents.x + distanciaSensorFrente));
        Gizmos.color = isTouchingWall ? Color.green : Color.blue;
        Gizmos.DrawWireCube(destinoCaixa, tamanhoCaixaSensor);

        float centroX = col.bounds.center.x;
        float centroY = col.bounds.center.y;
        Vector2 origemBuraco = new Vector2(centroX + (avancoOlhoBuraco * moveDirection), centroY);
        float distanciaRaio = col.bounds.extents.y + 0.8f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origemBuraco, origemBuraco + (Vector2.down * distanciaRaio));
    }
}
