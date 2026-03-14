using UnityEngine;
using UnityEngine.InputSystem;
public class SpeedBotMovment : MonoBehaviour
{
    [Header("Configuração Temporária")]
    public float moveSpeed = 10f;
    public float jumpForce = 12f;

    private Rigidbody2D rb;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 1. Lógica de movimento horizontal
        float moveInput = 0f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput = 1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput = -1f;

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // 2. Lógica de Pulo (Barra de Espaço)
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    // Checagem simples de chão usando colisão
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.y > 0.5f)
            isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }
}
