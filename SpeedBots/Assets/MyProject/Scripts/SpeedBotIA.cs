using UnityEngine;

public class SpeedBotIA : MonoBehaviour
{
    [Header("Atributos do Motor")]
    public float velocidadeMaxima = 14f;
    public float aceleracao = 25f;
    public float forcaPulo = 12f;

    [Header("Parkour (Vector Style)")]
    public float forcaWallJumpY = 14f;
    public float forcaWallJumpX = 4f;

    [Header("Sensores Frontais (Obstáculos)")]
    public float distanciaSensorFrente = 0.5f;
    public Vector2 tamanhoCaixaSensor = new Vector2(0.1f, 0.8f);

    [Header("Sensores de Buraco (Precipício)")]
    public float distanciaOlhoBuraco = 1.0f; // Quão longe ela "olha" para baixo
    public float avancoOlhoBuraco = 0.8f; // O quanto esse olho fica à frente do robô

    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private bool isGrounded;
    private bool isTouchingWall;
    private float moveDirection = 1f; // A IA começa querendo ir para a direita

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
    }

    void Update()
    {
        VerificarAmbiente();

        // Lógica de Pulo
        if (isGrounded)
        {
            // Pula se houver um obstáculo na frente OU se o chão à frente acabar (buraco)
            if (isTouchingWall || DetectarBuraco())
            {
                PuloNormal();
            }
        }
        else if (isTouchingWall)
        {
            // Se estiver no ar e encostar numa parede, tenta o Wall-Jump
            WallJump();
        }
    }

    void FixedUpdate()
    {
        // Se a IA estiver grudada numa parede no ar (Wall-Jump ativo), ela inverte a direção temporariamente
        // para desgrudar da parede e ir para a outra (como no corredor da sua imagem)
        if (!isGrounded && isTouchingWall && rb.linearVelocity.y > 0)
        {
            // Mantém a direção atual (que foi invertida no WallJump)
        }
        else if (isGrounded)
        {
            // Volta a forçar a ida para a direita sempre que pisar no chão firme
            moveDirection = 1f;
        }

        // Aplica aceleração na direção que a IA quer ir
        if (Mathf.Abs(rb.linearVelocity.x) < velocidadeMaxima)
        {
            rb.AddForce(new Vector2(moveDirection * aceleracao, 0), ForceMode2D.Force);
        }
    }

    private void VerificarAmbiente()
    {
        Vector2 centro = col.bounds.center;
        Vector2 destinoCaixa = centro + (new Vector2(moveDirection, 0) * (col.bounds.extents.x + distanciaSensorFrente));

        Collider2D[] hits = Physics2D.OverlapBoxAll(destinoCaixa, tamanhoCaixaSensor, 0f);
        isTouchingWall = false;

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Parede"))
            {
                isTouchingWall = true;
                break;
            }
        }
    }

    private bool DetectarBuraco()
    {
        // 1. A origem do raio agora é no CENTRO do robô (barriga), não mais no pé.
        float centroX = col.bounds.center.x;
        float centroY = col.bounds.center.y;

        Vector2 origem = new Vector2(centroX + (avancoOlhoBuraco * moveDirection), centroY);

        // 2. A distância do raio é a metade da altura do robô (do centro até o pé) + uma margem de segurança
        // O valor 0.8f é a tolerância. Se a descida for MUITO íngreme, vocês podem aumentar para 1.0f ou mais.
        float distanciaRaio = col.bounds.extents.y + 0.8f;

        RaycastHit2D hit = Physics2D.Raycast(origem, Vector2.down, distanciaRaio);

        if (hit.collider == null || !hit.collider.CompareTag("Pista"))
        {
            return true;
        }
        return false;
    }

    private void PuloNormal()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * forcaPulo, ForceMode2D.Impulse);
    }

    private void WallJump()
    {
        rb.linearVelocity = Vector2.zero;

        // A IA inverte a própria vontade de andar (para quicar de uma parede para a outra no corredor)
        moveDirection = moveDirection * -1;

        rb.AddForce(new Vector2(moveDirection * forcaWallJumpX, forcaWallJumpY), ForceMode2D.Impulse);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.y > 0.5f) isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }

    private void OnDrawGizmos()
    {
        if (col == null) col = GetComponent<CapsuleCollider2D>();
        if (col == null) return;

        // Desenha a caixa sensora de parede (Azul/Verde)
        Vector2 centro = col.bounds.center;
        Vector2 destinoCaixa = centro + (new Vector2(moveDirection, 0) * (col.bounds.extents.x + distanciaSensorFrente));
        Gizmos.color = isTouchingWall ? Color.green : Color.blue;
        Gizmos.DrawWireCube(destinoCaixa, tamanhoCaixaSensor);

        // Desenha o sensor de buraco CORRIGIDO (Amarelo)
        float centroX = col.bounds.center.x;
        float centroY = col.bounds.center.y;
        Vector2 origemBuraco = new Vector2(centroX + (avancoOlhoBuraco * moveDirection), centroY);

        float distanciaRaio = col.bounds.extents.y + 0.8f; // Mesma matemática do método acima

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origemBuraco, origemBuraco + (Vector2.down * distanciaRaio));
    }
}
