using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
public class SpeedBotMovment : MonoBehaviour
{
    public enum TipoBot { Crawler, Slider, Aerial }

    [Header("Identidade do Chassi")]
    public TipoBot tipoBot;

    [Tooltip("Crawler = 0.9 | Aerial = 0.5 | Slider = 0.1")]
    [Range(0f, 1f)] public float aderenciaBase = 0.5f;

    [Tooltip("Crawler = 0.9 | Aerial = 0.5 | Slider = 0.1")]
    [Range(0f, 1f)] public float durabilidadeBase = 0.5f;

    [Header("Atributos Base do Motor")]
    public float velocidadeMaximaBase = 15f;
    public float aceleracaoBase = 30f;
    public float forcaPulo = 12f;

    [Header("Parkour")]
    public float forcaWallJumpY = 14f;
    public float forcaWallJumpX = 4f;
    public float distanciaSensor = 0.1f;
    public Vector2 tamanhoCaixaSensor = new Vector2(0.1f, 0.8f);

    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private bool isGrounded;
    private bool isTouchingWall;
    private float lastMoveDirection = 1f;

    // --- VARIÁVEIS DE ESTADO (RPG) ---
    private string terrenoAtual = "Normal";
    private float stunTimer = 0f;
    private float debuffFogoTimer = 0f;
    private float multiplicadorNitro = 1f;
    private float nitroTimer = 0f;

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
        // 1. Controle de Tempo no Ar (Passiva do Aerial)

        // 2. Controle de Stun (Perda de controle total, mas sem ser jogado para trás)
        if (stunTimer > 0)
        {
            stunTimer -= Time.fixedDeltaTime;
            // Fricção forte para parar o robô no lugar enquanto está atordoado
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.8f, rb.linearVelocity.y);
            return;
        }

        // --- INÍCIO DO CÁLCULO DE RPG ---
        float velMaxAtual = velocidadeMaximaBase;
        float acelAtual = aceleracaoBase;
        float gripAtual = aderenciaBase;
        float friccao = 0.9f;

        // 3. Sinergias de Terreno (A Pedra, Papel e Tesoura)
        if (terrenoAtual == "Lama")
        {
            if (tipoBot == TipoBot.Crawler)
            {
                velMaxAtual *= 1.3f; // Buff forte
                acelAtual *= 1.3f;
                gripAtual = Mathf.Clamp01(gripAtual + 0.3f);
            }
            else if (tipoBot == TipoBot.Slider)
            {
                velMaxAtual *= 0.3f; // Nerf pesado
                acelAtual *= 0.2f;
                gripAtual = Mathf.Clamp01(gripAtual - 0.4f);
            }
            else if (tipoBot == TipoBot.Aerial)
            {
                velMaxAtual *= 0.7f; // Nerf leve
                acelAtual *= 0.7f;
            }
        }
        else if (terrenoAtual == "Gelo")
        {
            friccao = 0.99f; // Gelo sempre escorrega
            if (tipoBot == TipoBot.Slider)
            {
                velMaxAtual *= 1.4f; // Buff forte
                acelAtual *= 1.4f;
                gripAtual = Mathf.Clamp01(gripAtual + 0.3f);
            }
            else if (tipoBot == TipoBot.Crawler)
            {
                velMaxAtual *= 0.3f; // Nerf pesado
                acelAtual *= 0.2f;
                gripAtual = Mathf.Clamp01(gripAtual - 0.4f);
            }
            else if (tipoBot == TipoBot.Aerial)
            {
                velMaxAtual *= 0.7f; // Nerf leve
                acelAtual *= 0.7f;
            }
        }

        // 4. Buff do Aerial (Voo prolongado)
        if (tipoBot == TipoBot.Aerial)
        {
            // Ganha 15% a mais de Velocidade Final e 10% de Aceleração em qualquer terreno
            velMaxAtual *= 1.15f;
            acelAtual *= 1.10f;

            // Leve compensação de aderência (ele "flutua" um pouco sobre o terreno ruim)
            if (terrenoAtual == "Lama" || terrenoAtual == "Gelo") gripAtual += 0.1f;
        }

        // 5. Debuff do Fogo (Reduz status após passar pelo fogo)
        if (debuffFogoTimer > 0)
        {
            debuffFogoTimer -= Time.fixedDeltaTime;
            velMaxAtual *= 0.5f; // Corta na metade
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

        // --- FIM DO CÁLCULO DE RPG ---

        // Leitura de Input e Aplicação Física
        float moveInput = 0f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput = 1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput = -1f;

        if (Mathf.Abs(rb.linearVelocity.x) < velMaxAtual && moveInput != 0)
        {
            rb.AddForce(new Vector2(moveInput * acelAtual, 0), ForceMode2D.Force);
        }

        if (moveInput == 0 && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * friccao, rb.linearVelocity.y);
        }

        // Clamp para não ultrapassar a velocidade calculada
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

    public void SofrerPuxao(float forcaPuxao, float direcaoX)
    {
        rb.linearVelocity = Vector2.zero; // Quebra o momentum atual
        rb.AddForce(new Vector2(direcaoX * forcaPuxao, 4f), ForceMode2D.Impulse); // Puxa brutalmente e levanta um pouco
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
