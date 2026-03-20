using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private bool raceEnded = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (raceEnded) return;

        if (collision.CompareTag("Player"))
        {
            raceEnded = true;

            // 1. Aplica o XP
            SpeedBotProgression progresso = collision.GetComponent<SpeedBotProgression>();
            if (progresso != null) progresso.AdicionarXP(100f);

            // 2. Procura a Tela de Resultados e ativa a Vitória
            TelaResultados tela = Object.FindFirstObjectByType<TelaResultados>();
            if (tela != null)
            {
                tela.MostrarResultados(true);
            }
        }
        else if (collision.CompareTag("Inimigo"))
        {
            raceEnded = true;

            // Procura a Tela de Resultados e ativa a Derrota
            TelaResultados tela = Object.FindFirstObjectByType<TelaResultados>();
            if (tela != null)
            {
                tela.MostrarResultados(false);
            }
        }
    }
}
