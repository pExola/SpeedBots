using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("VITÓRIA! O SpeedBot cruzou a linha.");
            // Aqui futuramente chamaremos a tela de resultados
            Time.timeScale = 0; // Pausa o jogo para marcar o fim
        }
    }
}
