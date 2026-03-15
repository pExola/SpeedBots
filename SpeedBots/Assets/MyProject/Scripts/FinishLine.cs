using UnityEngine;

public class FinishLine : MonoBehaviour
{
    // Essa variável evita que dê "Vitória" e "Derrota" ao mesmo tempo se chegarem juntos
    private bool raceEnded = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Se a corrida já acabou, ignora qualquer outra colisão
        if (raceEnded) return;

        if (collision.CompareTag("Player"))
        {
            Debug.Log("VITÓRIA! O Player cruzou a linha primeiro.");
            raceEnded = true;
            Time.timeScale = 0; // Pausa o jogo
        }
        else if (collision.CompareTag("Inimigo"))
        {
            Debug.Log("DERROTA! O Rival cruzou a linha primeiro.");
            raceEnded = true;
            Time.timeScale = 0; // Pausa o jogo
        }
    }
}
