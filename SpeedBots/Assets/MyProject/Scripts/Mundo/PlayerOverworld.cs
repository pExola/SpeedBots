using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerOverworld : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    [HideInInspector] public Vector2 lastFacingDirection = Vector2.down;

    // --- NOVA VARIÁVEL DO ANIMATOR ---
    private Animator anim;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); // Conecta o código ao componente
    }

    void Update()
    {
        // Se estiver conversando, trava o movimento e a animação
        if (DialogueManager.Instance != null && DialogueManager.Instance.isTalking)
        {
            moveInput = Vector2.zero;
            if (anim != null) anim.SetFloat("Speed", 0f);
            return;
        }

        moveInput = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveInput.y = 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveInput.y = -1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput.x = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput.x = 1f;
        }

        if (moveInput != Vector2.zero)
        {
            lastFacingDirection = moveInput.normalized;

            // --- A MÁGICA: Envia a direção para a Blend Tree ---
            if (anim != null)
            {
                anim.SetFloat("Horizontal", lastFacingDirection.x);
                anim.SetFloat("Vertical", lastFacingDirection.y);
            }
        }

        // --- Envia a velocidade para saber se está parado ou andando ---
        if (anim != null)
        {
            anim.SetFloat("Speed", moveInput.sqrMagnitude);
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }
}
