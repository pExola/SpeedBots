using TMPro; // Importa a biblioteca TextMeshPro para podermos manipular textos visuais mais bonitos e nítidos na interface da Unity.
using UnityEngine; // Importa a biblioteca principal da Unity para que o script funcione no motor.

public class HUDManager : MonoBehaviour // Cria a classe que funciona como o painel, lendo os dados invisíveis e jogando na tela.
{
    public TextMeshProUGUI statsText; // Fica conectado ao objeto de texto visual da Unity onde as informações finais serão escritas.
    public SpeedBotProgression playerStats; // Fica conectado ao "cérebro" de status do jogador para sabermos os números exatos dele.

    void Update() // Roda a cada frame do jogo para garantir que o painel esteja sempre 100% atualizado.
    {
        if (playerStats != null) // Trava de segurança: só tenta escrever algo na tela se realmente achou o "cérebro" do jogador.
        {
            // Usa o cifrão ($) para montar uma frase dinâmica, injetando as variáveis dentro das chaves {} diretamente no texto.
            statsText.text = $"Nível: {playerStats.nivel}\n" + // Puxa o nível atual do jogador, escreve na tela e pula uma linha (\n).
                             $"Velocidade: {playerStats.GetStatusVelocidade()}/100\n" + // Puxa a conversão de Velocidade atual e pula uma linha.
                             $"Aceleração: {playerStats.GetStatusAceleracao()}/100"; // Puxa a conversão de Aceleração atual e finaliza o texto.
        }
    }
}