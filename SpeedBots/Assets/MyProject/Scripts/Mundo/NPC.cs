using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    [Header("Configuração do Twine")]
    public string arquivoDoDialogo;
    public string noInicial = "Inicio";

    [Header("Configuração Especial (Piastri)")]
    // Novo campo para definir qual nó do Twine diz para o jogador ir escolher o robô
    public string noAposLiberarRobo = "LembreteEscolha";

    [Header("Transição de Cena")]
    public string cenaAoEncerrar = "";

    public void Interagir()
    {
        // Criamos uma variável local para decidir qual nó vai rodar, 
        // começando com o padrão definido no Inspector
        string noParaDisparar = noInicial;

        // 1. CHECAGEM EXCLUSIVA DO PIASTRI
        if (gameObject.CompareTag("Piastri"))
        {
            // Se a variável partilhada já for TRUE, significa que o jogador já conversou com ele antes
            if (SelecaoSpeedBot.falouComOscar)
            {
                // Altera o nó de destino para a fala de cobrança/direcionamento
                noParaDisparar = noAposLiberarRobo;
                Debug.Log("[NPC] Piastri relembrando o jogador de escolher um SpeedBot.");
            }
            else
            {
                // Se for a PRIMEIRA vez que conversam, ativa a permissão dos robôs 
                // para que as próximas tentativas entrem no bloco de cima
                SelecaoSpeedBot.falouComOscar = true;
                Debug.Log("[NPC] Você falou com o Piastri pela primeira vez! Seleção de SpeedBots liberada.");
            }
        }

        // 2. Dispara o diálogo com o nó definido pela checagem acima
        LeitorTwine.Instance.CarregarTwee(arquivoDoDialogo);
        DialogueManager.Instance.IniciarDialogo(noParaDisparar, cenaAoEncerrar);
    }
}