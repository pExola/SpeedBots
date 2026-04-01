using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class EnquadramentoCamera : MonoBehaviour
{
    private BoxCollider2D colisor;

    private void Awake()
    {
        colisor = GetComponent<BoxCollider2D>();
    }

    // Trocamos Enter por Stay para verificar a posição continuamente
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // O Segredo: Só chama a câmera se o CENTRO (pivot) do robô cruzou a fronteira
            if (colisor.bounds.Contains(collision.transform.position))
            {
                GerenciadorDeCamera.Instance.MudarEnquadramento(colisor.bounds.center);
            }
        }
    }
}

