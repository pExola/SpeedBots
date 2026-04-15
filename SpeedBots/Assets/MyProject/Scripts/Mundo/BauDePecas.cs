using UnityEngine; // Importa as ferramentas principais da engine da Unity.

// Cria a classe que representa a caixa de loot no mapa.
// Ao assinar o contrato IInteractable, ela garante que a "mão" do jogador conseguirá ativá-la.
public class BauDePecas : MonoBehaviour, IInteractable
{
    [Header("O que tem dentro? (Pode colocar várias!)")] // Organiza visualmente o componente lá no Inspector.

    // O segredo do Array: Os colchetes [] transformam uma única variável em uma lista/coleção.
    // Isso permite que você arraste quantas peças diferentes você quiser para dentro deste baú no Inspector.
    public PecaSpeedBot[] pecasEscondidas;

    // O "cadeado lógico". Começa como falso porque o baú nasce fechado e cheio de itens.
    // Serve para impedir que o jogador pegue os mesmos itens infinitamente clicando sem parar.
    private bool jaAberto = false;

    // Função engatilhada no exato momento em que o jogador encosta no baú e aperta o botão de interação.
    public void Interagir()
    {
        // 1. A CHECAGEM DO CADEADO: O baú já foi saqueado antes?
        if (jaAberto)
        {
            // Se for verdadeiro, avisa no console e aborta a missão!
            Debug.Log("[BAÚ] Este baú já está vazio.");
            return; // O 'return' corta o código imediatamente aqui, impedindo que os itens sejam dados de novo.
        }

        int itensPegos = 0; // Cria um contador temporário para sabermos o total de itens coletados.

        // 2. A COLETA: Usa um laço de repetição (foreach) para passar por todas as peças do array.
        foreach (PecaSpeedBot peca in pecasEscondidas)
        {
            // Checagem de segurança para não tentar dar um item que você esqueceu de preencher no Inspector (nulo).
            if (peca != null)
            {
                // Joga a peça atual da lista diretamente para a mochila do jogador através do Singleton.
                InventarioManager.Instance.AdicionarPeca(peca);
                itensPegos++; // Soma +1 no contador de peças transferidas.
            }
        }

        // 3. A TRANCA: Assim que entregar tudo, ele muda o cadeado para 'true'.
        // Agora, se o jogador tentar clicar de novo, o baú vai barrar a ação lá no 'if (jaAberto)' inicial.
        jaAberto = true;

        // Exibe uma mensagem de comemoração no console dizendo exatamente o tamanho do saque.
        Debug.Log($"[BAÚ] Baú aberto! Você pegou {itensPegos} itens.");
    }
}