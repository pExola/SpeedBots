using UnityEngine;

public class SpeedBotIA : MonoBehaviour
{
    [Header("Configuração Temporária")]
    public float moveSpeed = 9f; // Deixei um pouco mais lento que o Player (10f) teste a vitória
    public float jumpForce = 12f;

    [Header("Sensores")]
    public float sensorDistance = 1.5f; // Distância que a IA "enxerga" para frente

    private Rigidbody2D rb;
    private bool isGrounded;
    public float sensorOffsetX = 0.6f;  // O quanto o raio vai para frente
    public float sensorOffsetY = -0.5f; // O quanto o raio vai para baixo (valores negativos descem)

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 1. Movimento constante para a direita
        rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);

        // 2. Criando o Offset (Faz o raio nascer um pouco à frente do centro do robô)
        Vector2 sensorOrigin = new Vector2(transform.position.x + sensorOffsetX, transform.position.y + sensorOffsetY);

        // Lança o raio a partir do novo ponto de origem
        RaycastHit2D hit = Physics2D.Raycast(sensorOrigin, Vector2.right, sensorDistance);

        // 3. Lógica de Pulo
        if (hit.collider != null && hit.collider.CompareTag("Pista") && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    // Checagem de chão (Igual ao do Player)
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.y > 0.5f)
            isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }

    //Isso desenha uma linha vermelha no Editor para você ver até onde o sensor alcança
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector2 sensorOrigin = new Vector2(transform.position.x + sensorOffsetX, transform.position.y + sensorOffsetY);
        Gizmos.DrawLine(sensorOrigin, sensorOrigin + (Vector2.right * sensorDistance));
    }
}
