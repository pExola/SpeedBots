using UnityEngine; // Importa as ferramentas principais da engine da Unity.
using TMPro; // Importa a biblioteca TextMeshPro para desenhar textos nítidos na interface do jogo.
using UnityEngine.SceneManagement; // Importa o gerenciador de cenas para podermos mudar da corrida para o mapa principal.

public class TelaResultados : MonoBehaviour // É o gerente de fluxo (Game Flow) do pós-corrida.
{
    [Header("UI e Status")] // Organiza as variáveis na tela do Inspector da Unity.
    public GameObject painelResultados; // A gaveta que guarda a tela (Canvas UI) de resultados para podermos ativá-la.
    public TextMeshProUGUI textoResultados; // O local onde o texto de "Vitória" ou "Derrota" será escrito.
    public SpeedBotProgression playerStats; // Acessa o cérebro de RPG do jogador para ler a evolução de nível/status.

    [Header("Transição")]
    [Tooltip("Digite o nome exato da sua cena do mapa principal")] // Dica que aparece ao passar o mouse na Unity.
    public string nomeCenaOverworld = "Overworld"; // O nome do arquivo da fase principal que será carregada depois.

    public void MostrarResultados(bool vitoria) // Função chamada pelo Juiz da linha de chegada, recebe true (Vitória) ou false (Derrota).
    {
        painelResultados.SetActive(true); // Ativa o Canvas UI na tela (que antes estava invisível).

        if (vitoria) // Se a sentença for Vitória...
        {
            // Exibe o feedback estético verde na tela e as recompensas.
            // Usa o cifrão ($"{}") para injetar valores diretos do código no texto. Puxa o status atualizado do SpeedBotProgression.
            textoResultados.text = "<color=green>VITÓRIA!</color>\n\n" +
                                   $"+100 XP\n" +
                                   $"Nível Atual: {playerStats.nivel}\n" +
                                   $"Velocidade: {playerStats.GetStatusVelocidade()}/100\n" +
                                   $"Aceleração: {playerStats.GetStatusAceleracao()}/100";
        }
        else // Se a sentença for Derrota...
        {
            // Exibe o feedback estético vermelho padrão de falha.
            textoResultados.text = "<color=red>DERROTA!</color>\nTente Novamente.";
        }

        // "Congela o tempo" do motor da Unity. 
        // Isso impede que a física calcule colisões ou que a IA continue correndo enquanto você lê a tela.
        Time.timeScale = 0;
    }

    // --- FUNÇÃO PARA O BOTÃO CONTINUAR ---
    public void VoltarParaOverworld() // Ação disparada quando o jogador clica no botão "Continuar" da interface.
    {
        // Devolve o tempo ao normal (1) ANTES de carregar a cena. 
        // É essencial, senão o jogador chegaria no Overworld e estaria tudo congelado.
        Time.timeScale = 1;

        // Usa o SceneManager da Unity para deletar a pista de corrida da memória e carregar o mapa principal.
        SceneManager.LoadScene(nomeCenaOverworld);
    }
}