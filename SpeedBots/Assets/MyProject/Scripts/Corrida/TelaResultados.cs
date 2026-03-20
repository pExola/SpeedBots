using UnityEngine;
using TMPro;
public class TelaResultados : MonoBehaviour
{
    public GameObject painelResultados;
    public TextMeshProUGUI textoResultados;
    public SpeedBotProgression playerStats;

    public void MostrarResultados(bool vitoria)
    {
        painelResultados.SetActive(true);

        if (vitoria)
        {
            textoResultados.text = "<color=green>VITÓRIA!</color>\n\n" +
                                   $"+100 XP\n" +
                                   $"Nível Atual: {playerStats.nivel}\n" +
                                   $"Velocidade: {playerStats.GetVelocidadeAtual()}\n" +
                                   $"Aceleração: {playerStats.GetAceleracaoAtual()}";
        }
        else
        {
            textoResultados.text = "<color=red>DERROTA!</color>\nTente Novamente.";
        }

        Time.timeScale = 0; // Pausa o jogo depois de mostrar a tela
    }
}
