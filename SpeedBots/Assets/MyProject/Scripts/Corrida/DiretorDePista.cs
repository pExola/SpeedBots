using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DiretorDePista : MonoBehaviour
{
    [Header("Para qual lado a pista vai agora?")]
    [Tooltip("1 = Direita | -1 = Esquerda")]
    [Range(-1f, 1f)] public float novaDirecao = -1f;

    // Opcional: Desenha uma seta na tela do desenvolvedor para você ver o fluxo da pista
    private void OnDrawGizmos()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f); // Verde transparente
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);

            // Desenha uma linha indicando a direção
            Gizmos.color = Color.green;
            Gizmos.DrawRay(col.bounds.center, new Vector2(novaDirecao, 0) * 3f);
        }
    }
}