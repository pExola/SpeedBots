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
                             $"Velocidade: {playerStats.GetStatusVelocidade()}/100\n" +
                             $"Aceleração: {playerStats.GetStatusAceleracao()}/100";
        }
    }
}
