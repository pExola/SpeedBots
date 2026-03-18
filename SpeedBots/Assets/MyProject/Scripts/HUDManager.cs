using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public TextMeshProUGUI statsText;
    public SpeedBotProgression playerStats;

    void Update()
    {
        if (playerStats != null)
        {
            statsText.text = $"Nível: {playerStats.nivel}\n" +
                             $"Velocidade: {playerStats.GetVelocidadeAtual()}\n" +
                             $"Aceleração: {playerStats.GetAceleracaoAtual()}";
        }
    }
}
