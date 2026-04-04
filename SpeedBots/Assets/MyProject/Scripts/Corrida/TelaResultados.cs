using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; 

public class TelaResultados : MonoBehaviour
{
    [Header("UI e Status")]
    public GameObject painelResultados;
    public TextMeshProUGUI textoResultados;
    public SpeedBotProgression playerStats;

    [Header("Transição")]
    [Tooltip("Digite o nome exato da sua cena do mapa principal")]
    public string nomeCenaOverworld = "Overworld";

    public void MostrarResultados(bool vitoria)
    {
        painelResultados.SetActive(true);

        if (vitoria)
        {
            textoResultados.text = "<color=green>VITÓRIA!</color>\n\n" +
                                   $"+100 XP\n" +
                                   $"Nível Atual: {playerStats.nivel}\n" +
                                   $"Velocidade: {playerStats.GetStatusVelocidade()}/100\n" +
                                   $"Aceleração: {playerStats.GetStatusAceleracao()}/100";
        }
        else
        {
            textoResultados.text = "<color=red>DERROTA!</color>\nTente Novamente.";
        }

        Time.timeScale = 0; // Pausa o jogo
    }

    // --- NOVA FUNÇÃO PARA O BOTÃO CONTINUAR ---
    public void VoltarParaOverworld()
    {
        // Despausa a física da Unity ANTES de carregar a cena, senão o Overworld nasce travado
        Time.timeScale = 1;
        SceneManager.LoadScene(nomeCenaOverworld);
    }
}