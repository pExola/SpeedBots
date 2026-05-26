using UnityEngine; // Importa as ferramentas principais da engine da Unity.

// É a "casca" física dos personagens no mundo. Transforma um boneco parado no mapa em um ator completo.
// Ao assinar o contrato IInteractable, ele fica pronto para escutar o clique/interação do jogador.
public class NPC : MonoBehaviour, IInteractable
{
    [Header("Configuração do Twine")] // Organiza visualmente as opções na tela do Inspector.

    // "Gaveta" pública onde você digita o nome do roteiro de texto (arquivo .twee) exato daquele personagem.
    public string arquivoDoDialogo;

    // Define qual é o primeiro "Nó" (ou página) da história que deve ser lido quando a conversa começar.
    public string noInicial = "Inicio";

    [Header("Transição de Cena")]
    // Variável opcional: permite engatilhar um teletransporte/mudança de fase assim que o diálogo atual chegar ao fim.
    public string cenaAoEncerrar = "";

    // Método obrigatório do IInteractable. É acionado no exato momento em que o jogador aperta o botão na frente do NPC.
    public void Interagir()
    {
        if (gameObject.CompareTag("Piastri"))
        {
            // Se for o Piastri, ele acessa a variável global dos robôs e dá o sinal verde!
            SelecaoSpeedBot.falouComOscar = true;
            Debug.Log("[NPC] Você falou com o Piastri! Seleção de SpeedBots liberada.");
        }
        // Quando ativado, ele faz uma "tabelinha" de comandos, conectando a arte do jogo com o roteiro:

        // 1. Primeiro comando: Grita para o tradutor (LeitorTwine) ler o arquivo de texto que está na gaveta.
        LeitorTwine.Instance.CarregarTwee(arquivoDoDialogo);

        // 2. Segundo comando: Manda o ator (DialogueManager) subir no palco e desenhar a caixa de texto na tela, 
        // começando pela página inicial e já informando se haverá uma mudança de cena no final.
        DialogueManager.Instance.IniciarDialogo(noInicial, cenaAoEncerrar);
    }
}