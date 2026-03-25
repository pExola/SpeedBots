using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerOverworld : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    // Guarda a direção para saber com quem o player está tentando falar
    [HideInInspector] public Vector2 lastFacingDirection = Vector2.down;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Se estiver conversando, trava o movimento
        if (DialogueManager.Instance != null && DialogueManager.Instance.isTalking)
        {
            moveInput = Vector2.zero;
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
            // O get axis raw ajuda a manter o direcional limpo para pixel art
            lastFacingDirection = moveInput.normalized;
        }
    }

    void FixedUpdate()
    {
        // MovePosition evita bugar nas quinas dos Tilemap Colliders
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }
}
