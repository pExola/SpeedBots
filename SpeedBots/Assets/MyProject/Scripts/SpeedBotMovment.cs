using UnityEngine;
using UnityEngine.InputSystem;
public class SpeedBotMovment : MonoBehaviour
{
    [Header("Atributos do Motor")]
    public float velocidadeMaxima = 15f;
    public float aceleracao = 30f;
    public float forcaPulo = 12f;

    [Header("Parkour (Vector Style)")]
    public float forcaWallJumpY = 14f;
    public float forcaWallJumpX = 4f;
    public float distanciaSensor = 0.1f;
    public Vector2 tamanhoCaixaSensor = new Vector2(0.1f, 0.8f); // Caixa fina e alta para focar só na parede

    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private bool isGrounded;
    private bool isTouchingWall;
    private float lastMoveDirection = 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        // 1. Atualiza a direção do olhar
        float moveInput = 0f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput = 1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput = -1f;

        if (moveInput != 0) lastMoveDirection = moveInput;

        // 2. Checa a parede com o novo método infalível
        VerificarParede();

        // 3. Pulos
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (isGrounded)
            {
                PuloNormal();
            }
            else if (isTouchingWall)
            {
                WallJump();
            }
        }
    }

    void FixedUpdate()
    {
        if (Keyboard.current == null) return;

        float moveInput = 0f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput = 1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput = -1f;

        // Aceleração
        if (Mathf.Abs(rb.linearVelocity.x) < velocidadeMaxima && moveInput != 0)
        {
            rb.AddForce(new Vector2(moveInput * aceleracao, 0), ForceMode2D.Force);
        }

        // Fricção
        if (moveInput == 0 && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.9f, rb.linearVelocity.y);
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
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * forcaPulo, ForceMode2D.Impulse);
    }

    private void WallJump()
    {
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(lastMoveDirection * forcaWallJumpX, forcaWallJumpY), ForceMode2D.Impulse);
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
